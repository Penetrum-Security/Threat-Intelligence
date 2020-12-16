// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DowntimeMonitoring.DowntimeMonitoringEnableSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.DowntimeMonitoring
{
  public class DowntimeMonitoringEnableSubscriber : ISubscriber
  {
    private static string SettingsKey = "SWNetPerfMon-Settings-EnableDowntimeMonitoring";
    private static readonly Log Log = new Log();
    private readonly ISubscriptionManager subscriptionManager;
    private DowntimeMonitoringNotificationSubscriber downtimeMonitoringSubscriber;
    private ISubscription subscription;

    public DowntimeMonitoringEnableSubscriber(
      DowntimeMonitoringNotificationSubscriber downtimeMonitoringSubscriber)
      : this(SubscriptionManager.get_Instance(), downtimeMonitoringSubscriber)
    {
    }

    public DowntimeMonitoringEnableSubscriber(
      ISubscriptionManager subscriptionManager,
      DowntimeMonitoringNotificationSubscriber downtimeMonitoringSubscriber)
    {
      if (subscriptionManager == null)
        throw new ArgumentNullException(nameof (subscriptionManager));
      if (downtimeMonitoringSubscriber == null)
        throw new ArgumentNullException(nameof (downtimeMonitoringSubscriber));
      this.subscriptionManager = subscriptionManager;
      this.downtimeMonitoringSubscriber = downtimeMonitoringSubscriber;
    }

    public DowntimeMonitoringNotificationSubscriber DowntimeMonitoringSubscriber
    {
      get
      {
        return this.downtimeMonitoringSubscriber;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof (DowntimeMonitoringSubscriber));
        this.downtimeMonitoringSubscriber = value;
      }
    }

    public Task OnNotificationAsync(Notification notification)
    {
      if (notification.get_SourceInstanceProperties() == null)
      {
        DowntimeMonitoringEnableSubscriber.Log.Error((object) "Argument sourceInstanceProperties is null");
        return Task.CompletedTask;
      }
      if (!notification.get_SourceInstanceProperties().ContainsKey("CurrentValue"))
      {
        DowntimeMonitoringEnableSubscriber.Log.Error((object) "CurrentValue not supplied in sourceInstanceProperties");
        return Task.CompletedTask;
      }
      try
      {
        DowntimeMonitoringEnableSubscriber.Log.DebugFormat("Downtime monitoring changed to {0}, unsubscribing..", notification.get_SourceInstanceProperties()["CurrentValue"]);
        int num = Convert.ToBoolean(notification.get_SourceInstanceProperties()["CurrentValue"]) ? 1 : 0;
        this.downtimeMonitoringSubscriber.Stop();
        if (num != 0)
        {
          DowntimeMonitoringEnableSubscriber.Log.Debug((object) "Re-subscribing..");
          this.downtimeMonitoringSubscriber.Start();
        }
        else
          this.SealIntervals();
      }
      catch (Exception ex)
      {
        DowntimeMonitoringEnableSubscriber.Log.Error((object) "Indication handling failed", ex);
        return Task.CompletedTask;
      }
      return Task.CompletedTask;
    }

    private void SealIntervals()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE [dbo].[NetObjectDowntime] SET [DateTimeUntil] = @now WHERE [DateTimeUntil] IS NULL"))
      {
        textCommand.Parameters.AddWithValue("@now", (object) DateTime.Now.ToUniversalTime());
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public void Start()
    {
      DowntimeMonitoringEnableSubscriber.Log.Debug((object) "Subscribing DowntimeMonitoringEnableSubscriber changed indications..");
      if (this.subscription != null)
      {
        DowntimeMonitoringEnableSubscriber.Log.Debug((object) "Already subscribed, unsubscribing first..");
        this.Stop(false);
      }
      try
      {
        string str = "SUBSCRIBE CHANGES TO Orion.Settings WHEN SettingsID = '" + DowntimeMonitoringEnableSubscriber.SettingsKey + "'";
        SubscriptionId subscriptionId;
        ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (DowntimeMonitoringEnableSubscriber).FullName, (Scope) 0);
        SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
        subscriberConfiguration1.set_SubscriptionQuery(str);
        SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
        this.subscription = this.subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2);
        DowntimeMonitoringEnableSubscriber.Log.TraceFormat("Subscribed with URI '{0}'", new object[1]
        {
          (object) this.subscription
        });
      }
      catch (Exception ex)
      {
        DowntimeMonitoringEnableSubscriber.Log.Error((object) "Failed to subscribe", ex);
        throw;
      }
    }

    public void Stop(bool sealInterval = true)
    {
      DowntimeMonitoringEnableSubscriber.Log.Debug((object) "Unsubscribing DowntimeMonitoringEnableSubscriber changed indications..");
      if (sealInterval)
      {
        try
        {
          this.SealIntervals();
        }
        catch (Exception ex)
        {
          DowntimeMonitoringEnableSubscriber.Log.Error((object) "Failed to seal intervals", ex);
          throw;
        }
      }
      if (this.subscription == null)
      {
        DowntimeMonitoringEnableSubscriber.Log.Debug((object) "SubscriptionUri not set, no action performed");
      }
      else
      {
        try
        {
          this.subscriptionManager.Unsubscribe(this.subscription.get_Id());
          this.subscription = (ISubscription) null;
        }
        catch (Exception ex)
        {
          DowntimeMonitoringEnableSubscriber.Log.Error((object) "Failed to unsubscribe", ex);
          throw;
        }
      }
    }
  }
}
