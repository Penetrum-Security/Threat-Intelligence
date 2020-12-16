// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.NodeStatus.EnhancedNodeStatusCalculationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.PubSub;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.NodeStatus
{
  public class EnhancedNodeStatusCalculationSubscriber : ISubscriber, IDisposable
  {
    public static string SubscriptionUniqueName = "EnhancedNodeStatusCalculation";
    private static string SubscriptionQuery = "SUBSCRIBE CHANGES TO Orion.Settings WHEN SettingsID = 'EnhancedNodeStatusCalculation'";
    private static readonly Log log = new Log();
    private readonly ISubscriptionManager subscriptionManager;
    private readonly ISqlHelper sqlHelper;
    private ISubscription subscription;

    public EnhancedNodeStatusCalculationSubscriber(
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
      EnhancedNodeStatusCalculationSubscriber calculationSubscriber = this;
      if (calculationSubscriber.subscription == null)
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
      string subscriptionUniqueName = EnhancedNodeStatusCalculationSubscriber.SubscriptionUniqueName;
      if (str != subscriptionUniqueName)
        return;
      if (notification.get_SourceInstanceProperties() == null)
        EnhancedNodeStatusCalculationSubscriber.log.Error((object) "Argument SourceInstanceProperties is null.");
      else if (!notification.get_SourceInstanceProperties().ContainsKey("CurrentValue"))
      {
        EnhancedNodeStatusCalculationSubscriber.log.Error((object) "CurrentValue not supplied in SourceInstanceProperties.");
      }
      else
      {
        try
        {
          bool flag = Convert.ToInt32(notification.get_SourceInstanceProperties()["CurrentValue"]) == 1;
          EnhancedNodeStatusCalculationSubscriber.log.DebugFormat("Node status calculation changed to '{0} calculation', re-calculating node status ..", flag ? (object) "Enhanced" : (object) "Classic");
          // ISSUE: reference to a compiler-generated method
          await Task.Run(new Action(calculationSubscriber.\u003COnNotificationAsync\u003Eb__7_0));
        }
        catch (Exception ex)
        {
          EnhancedNodeStatusCalculationSubscriber.log.Error((object) "Indication handling failed", ex);
        }
      }
    }

    public EnhancedNodeStatusCalculationSubscriber Start()
    {
      EnhancedNodeStatusCalculationSubscriber.log.Debug((object) "Subscribing EnhancedNodeStatusCalculation changed indications..");
      try
      {
        if (this.subscription != null)
        {
          EnhancedNodeStatusCalculationSubscriber.log.Debug((object) "Already subscribed, unsubscribing first..");
          this.Unsubscribe(this.subscription.get_Id());
        }
        SubscriptionId subscriptionId1;
        ((SubscriptionId) ref subscriptionId1).\u002Ector("Core", EnhancedNodeStatusCalculationSubscriber.SubscriptionUniqueName, (Scope) 0);
        ISubscriptionManager subscriptionManager = this.subscriptionManager;
        SubscriptionId subscriptionId2 = subscriptionId1;
        SubscriberConfiguration subscriberConfiguration = new SubscriberConfiguration();
        subscriberConfiguration.set_SubscriptionQuery(EnhancedNodeStatusCalculationSubscriber.SubscriptionQuery);
        subscriberConfiguration.set_ReliableDelivery(true);
        subscriberConfiguration.set_AcknowledgeMode((AcknowledgeMode) 0);
        subscriberConfiguration.set_MessageTimeToLive(TimeSpan.Zero);
        this.subscription = subscriptionManager.Subscribe(subscriptionId2, (ISubscriber) this, subscriberConfiguration);
        return this;
      }
      catch (Exception ex)
      {
        EnhancedNodeStatusCalculationSubscriber.log.Error((object) "Failed to subscribe.", ex);
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
        EnhancedNodeStatusCalculationSubscriber.log.Debug((object) "Unsubscribing EnhancedNodeStatusCalculation changed indications..");
        this.Unsubscribe(this.subscription.get_Id());
        this.subscription = (ISubscription) null;
      }
      catch (Exception ex)
      {
        EnhancedNodeStatusCalculationSubscriber.log.Error((object) "Error unsubscribing subscription.", ex);
      }
    }

    private void Unsubscribe(SubscriptionId subscriptionId)
    {
      this.subscriptionManager.Unsubscribe(subscriptionId);
    }

    private void RecalculateNodeStatus()
    {
      using (SqlCommand textCommand = this.sqlHelper.GetTextCommand("EXEC dbo.[swsp_ReflowAllNodeChildStatus]"))
        this.sqlHelper.ExecuteNonQuery(textCommand);
    }
  }
}
