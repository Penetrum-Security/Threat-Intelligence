// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.ScheduledTaskFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.BusinessLayer.InformationService;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.PubSub;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class ScheduledTaskFactory
  {
    private static readonly Log log = new Log();

    internal static ScheduledTask CreateDatabaseMaintenanceTask(
      ISubscriptionManager subscriptionManager)
    {
      string empty = string.Empty;
      DateTime dateTime;
      try
      {
        empty = SettingsDAL.Get("SWNetPerfMon-Settings-Archive Time");
        dateTime = DateTime.FromOADate(double.Parse(empty));
      }
      catch (Exception ex)
      {
        dateTime = DateTime.MinValue.Date.AddHours(2.0).AddMinutes(15.0);
        ScheduledTaskFactory.log.ErrorFormat("DB maintenance time setting is not set or is not correct. Setting value is {0}. \nException: {1}", (object) empty, (object) ex);
      }
      ScheduledTaskInExactTime task = new ScheduledTaskInExactTime("DatabaseMaintenance", new TimerCallback(ScheduledTaskFactory.RunDatabaseMaintenace), (object) null, dateTime);
      if (subscriptionManager != null)
      {
        SubscriptionId subscriptionId;
        ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (ScheduledTaskFactory).FullName, (Scope) 0);
        SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
        subscriberConfiguration1.set_SubscriptionQuery("SUBSCRIBE CHANGES TO Orion.Settings WHEN SettingsID = 'SWNetPerfMon-Settings-Archive Time'");
        SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
        SettingsArchiveTimeSubscriber archiveTimeSubscriber = new SettingsArchiveTimeSubscriber(task);
        subscriptionManager.Subscribe(subscriptionId, (ISubscriber) archiveTimeSubscriber, subscriberConfiguration2);
      }
      else
        ScheduledTaskFactory.log.Error((object) "SubscribtionProvider is not initialized.");
      return (ScheduledTask) task;
    }

    private static void RunDatabaseMaintenace(object state)
    {
      ScheduledTaskFactory.log.Info((object) "Database maintenance task is starting.");
      try
      {
        Process.Start(Path.Combine(OrionConfiguration.get_InstallPath(), "Database-Maint.exe"), "-Archive");
        ScheduledTaskFactory.log.Info((object) "Database maintenace task started.");
        SettingsDAL.Set("SWNetPerfMon-Settings-Last Archive", DateTime.UtcNow.ToOADate());
      }
      catch (Exception ex)
      {
        ScheduledTaskFactory.log.Error((object) "Error while executing Database-Maint.exe.", ex);
      }
    }
  }
}
