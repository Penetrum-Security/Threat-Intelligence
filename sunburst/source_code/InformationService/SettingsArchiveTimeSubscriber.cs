// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.InformationService.SettingsArchiveTimeSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.PubSub;
using System;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.InformationService
{
  internal class SettingsArchiveTimeSubscriber : ISubscriber
  {
    private readonly Log log = new Log();
    private readonly ScheduledTaskInExactTime task;

    public SettingsArchiveTimeSubscriber(ScheduledTaskInExactTime task)
    {
      ScheduledTaskInExactTime scheduledTaskInExactTime = task;
      if (scheduledTaskInExactTime == null)
        throw new ArgumentNullException(nameof (task));
      this.task = scheduledTaskInExactTime;
    }

    public Task OnNotificationAsync(Notification notification)
    {
      try
      {
        this.task.set_ExactRunTime(DateTime.FromOADate(double.Parse(notification.get_SourceInstanceProperties()["CurrentValue"].ToString())));
      }
      catch (Exception ex)
      {
        this.log.Error((object) "Error when getting Archive time from SWIS.", ex);
      }
      return Task.CompletedTask;
    }
  }
}
