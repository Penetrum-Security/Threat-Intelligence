// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionCoreNotificationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class OrionCoreNotificationSubscriber : ISubscriber
  {
    private static readonly Log log = new Log(typeof (OrionCoreNotificationSubscriber));
    public const string OrionCoreIndications = "OrionCoreIndications";
    public const string NodeIndications = "NodeIndications";
    private readonly ISqlHelper _sqlHelper;
    private readonly ISubscriptionManager _subscriptionManager;
    private ISubscription _subscription;

    public OrionCoreNotificationSubscriber(ISqlHelper sqlHelper)
      : this(sqlHelper, SubscriptionManager.get_Instance())
    {
    }

    public OrionCoreNotificationSubscriber(
      ISqlHelper sqlHelper,
      ISubscriptionManager subscriptionManager)
    {
      ISqlHelper isqlHelper = sqlHelper;
      if (isqlHelper == null)
        throw new ArgumentNullException(nameof (sqlHelper));
      this._sqlHelper = isqlHelper;
      ISubscriptionManager isubscriptionManager = subscriptionManager;
      if (isubscriptionManager == null)
        throw new ArgumentNullException(nameof (subscriptionManager));
      this._subscriptionManager = isubscriptionManager;
    }

    public Task OnNotificationAsync(Notification notification)
    {
      if (OrionCoreNotificationSubscriber.log.get_IsDebugEnabled())
        OrionCoreNotificationSubscriber.log.DebugFormat("Indication of type \"{0}\" arrived.", (object) notification.get_IndicationType());
      try
      {
        if (notification.get_IndicationType() == IndicationHelper.GetIndicationType((IndicationType) 1))
        {
          object obj;
          if (notification.get_SourceInstanceProperties().TryGetValue("InstanceType", out obj))
          {
            if (string.Equals(obj as string, "Orion.Nodes", StringComparison.OrdinalIgnoreCase))
            {
              if (notification.get_SourceInstanceProperties().ContainsKey("NodeID"))
                this.InsertIntoDeletedTable(Convert.ToInt32(notification.get_SourceInstanceProperties()["NodeID"]));
              else
                OrionCoreNotificationSubscriber.log.WarnFormat("Indication is type of {0} but does not contain NodeID", (object) notification.get_IndicationType());
            }
          }
        }
      }
      catch (Exception ex)
      {
        OrionCoreNotificationSubscriber.log.Error((object) string.Format("Exception occured when processing incoming indication of type \"{0}\"", (object) notification.get_IndicationType()), ex);
      }
      return Task.CompletedTask;
    }

    public void Start()
    {
      Scheduler.get_Instance().Add(new ScheduledTask("OrionCoreIndications", new TimerCallback(this.Subscribe), (object) null, TimeSpan.FromSeconds(1.0), TimeSpan.FromMinutes(1.0)));
    }

    public void Stop()
    {
      Scheduler.get_Instance().Remove("OrionCoreIndications");
    }

    private void Subscribe(object state)
    {
      OrionCoreNotificationSubscriber.log.Debug((object) "Subscribing indications..");
      try
      {
        this.DeleteOldSubscriptions();
      }
      catch (Exception ex)
      {
        OrionCoreNotificationSubscriber.log.Warn((object) "Exception deleting old subscriptions:", ex);
      }
      try
      {
        SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
        subscriberConfiguration1.set_SubscriptionQuery("SUBSCRIBE System.InstanceDeleted");
        SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
        SubscriptionId subscriptionId;
        ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (OrionCoreNotificationSubscriber).FullName, (Scope) 0);
        this._subscription = this._subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2);
        if (OrionCoreNotificationSubscriber.log.get_IsDebugEnabled())
          OrionCoreNotificationSubscriber.log.DebugFormat("PubSub Subscription succeeded. ID:'{0}'", (object) this._subscription.get_Id());
        Scheduler.get_Instance().Remove("OrionCoreIndications");
      }
      catch (Exception ex)
      {
        OrionCoreNotificationSubscriber.log.Error((object) "Subscription did not succeed, retrying .. (Is SWIS v3 running ?)", ex);
      }
    }

    private void InsertIntoDeletedTable(int nodeId)
    {
      using (SqlCommand textCommand = this._sqlHelper.GetTextCommand("IF NOT EXISTS (SELECT NodeId FROM [dbo].[DeletedNodes] WHERE NodeId=@NodeId)  BEGIN   INSERT INTO [dbo].[DeletedNodes](NodeId)    VALUES(@NodeId)  END "))
      {
        textCommand.Parameters.AddWithValue("@NodeId", (object) nodeId);
        this._sqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    private void DeleteOldSubscriptions()
    {
      if (this._subscription == null)
        return;
      this._subscriptionManager.Unsubscribe(this._subscription.get_Id());
    }
  }
}
