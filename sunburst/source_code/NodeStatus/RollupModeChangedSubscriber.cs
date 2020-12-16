// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.NodeStatus.RollupModeChangedSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.PubSub;
using SolarWinds.Shared;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.NodeStatus
{
  public class RollupModeChangedSubscriber : ISubscriber, IDisposable
  {
    public static string SubscriptionUniqueName = "RollupModeChanged";
    private static string SubscriptionQuery = "SUBSCRIBE CHANGES TO Orion.Nodes WHEN [Core.StatusRollupMode] CHANGES";
    private static readonly Log log = new Log();
    private readonly ISubscriptionManager subscriptionManager;
    private readonly ISqlHelper sqlHelper;
    private ISubscription subscription;

    public RollupModeChangedSubscriber(
      ISubscriptionManager subscriptionManager,
      ISqlHelper sqlHelper)
    {
      ISubscriptionManager isubscriptionManager = subscriptionManager;
      if (isubscriptionManager == null)
        throw new ArgumentNullException(nameof (subscriptionManager));
      this.subscriptionManager = isubscriptionManager;
      ISqlHelper isqlHelper = sqlHelper;
      if (isqlHelper == null)
        throw new ArgumentNullException(nameof (sqlHelper));
      this.sqlHelper = isqlHelper;
    }

    public async Task OnNotificationAsync(Notification notification)
    {
      RollupModeChangedSubscriber changedSubscriber1 = this;
      if (changedSubscriber1.subscription == null)
        return;
      Notification notification1 = notification;
      string str;
      if (notification1 == null)
      {
        str = (string) null;
      }
      else
      {
        SubscriptionId subscriptionId = notification1.get_SubscriptionId();
        str = ((SubscriptionId) ref subscriptionId).get_UniqueName();
      }
      string subscriptionUniqueName = RollupModeChangedSubscriber.SubscriptionUniqueName;
      if (str != subscriptionUniqueName)
        return;
      if (notification.get_SourceInstanceProperties() == null)
        RollupModeChangedSubscriber.log.Error((object) "Argument SourceInstanceProperties is null.");
      else if (!notification.get_SourceInstanceProperties().ContainsKey("Core.StatusRollupMode"))
      {
        RollupModeChangedSubscriber.log.Error((object) "Core.StatusRollupMode not supplied in SourceInstanceProperties.");
      }
      else
      {
        try
        {
          RollupModeChangedSubscriber changedSubscriber = changedSubscriber1;
          string instanceProperty = (string) notification.get_SourceInstanceProperties()["Core.StatusRollupMode"];
          EvaluationMethod evaluationMethod = instanceProperty != null ? (EvaluationMethod) Convert.ToInt32(instanceProperty) : (EvaluationMethod) 0;
          int nodeId = Convert.ToInt32(notification.get_SourceInstanceProperties()["NodeId"]);
          RollupModeChangedSubscriber.log.DebugFormat("Node with id '{0}' rollup mode changed to '{1}', re-calculating node status ..", (object) nodeId, (object) evaluationMethod);
          await Task.Run((Action) (() => changedSubscriber.RecalculateNodeStatus(nodeId)));
        }
        catch (Exception ex)
        {
          RollupModeChangedSubscriber.log.Error((object) "Indication handling failed", ex);
        }
      }
    }

    public RollupModeChangedSubscriber Start()
    {
      RollupModeChangedSubscriber.log.Debug((object) "Subscribing RollupMode changed indications..");
      try
      {
        if (this.subscription != null)
        {
          RollupModeChangedSubscriber.log.Debug((object) "Already subscribed, unsubscribing first..");
          this.Unsubscribe(this.subscription.get_Id());
        }
        SubscriptionId subscriptionId1;
        ((SubscriptionId) ref subscriptionId1).\u002Ector("Core", RollupModeChangedSubscriber.SubscriptionUniqueName, (Scope) 0);
        ISubscriptionManager subscriptionManager = this.subscriptionManager;
        SubscriptionId subscriptionId2 = subscriptionId1;
        SubscriberConfiguration subscriberConfiguration = new SubscriberConfiguration();
        subscriberConfiguration.set_SubscriptionQuery(RollupModeChangedSubscriber.SubscriptionQuery);
        subscriberConfiguration.set_ReliableDelivery(true);
        subscriberConfiguration.set_AcknowledgeMode((AcknowledgeMode) 0);
        subscriberConfiguration.set_MessageTimeToLive(TimeSpan.Zero);
        this.subscription = subscriptionManager.Subscribe(subscriptionId2, (ISubscriber) this, subscriberConfiguration);
        return this;
      }
      catch (Exception ex)
      {
        RollupModeChangedSubscriber.log.Error((object) "Failed to subscribe.", ex);
        throw;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (this.subscription == null)
        return;
      try
      {
        RollupModeChangedSubscriber.log.Debug((object) "Unsubscribing RollupMode changed indications..");
        this.Unsubscribe(this.subscription.get_Id());
        this.subscription = (ISubscription) null;
      }
      catch (Exception ex)
      {
        RollupModeChangedSubscriber.log.Error((object) "Error unsubscribing subscription.", ex);
      }
    }

    private void Unsubscribe(SubscriptionId subscriptionId)
    {
      this.subscriptionManager.Unsubscribe(subscriptionId);
    }

    private void RecalculateNodeStatus(int nodeId)
    {
      using (SqlCommand textCommand = this.sqlHelper.GetTextCommand("EXEC dbo.[swsp_ReflowNodeChildStatus] @nodeId"))
      {
        textCommand.Parameters.Add(new SqlParameter("@nodeId", (object) nodeId));
        this.sqlHelper.ExecuteNonQuery(textCommand);
      }
    }
  }
}
