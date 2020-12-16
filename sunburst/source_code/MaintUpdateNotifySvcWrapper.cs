// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvcWrapper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc;
using SolarWinds.Orion.Core.Common.Models;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class MaintUpdateNotifySvcWrapper
  {
    public static MaintenanceRenewalItemDAL GetNotificationItem(
      VersionInfo versionInfo)
    {
      MaintenanceRenewalItemDAL renewal = new MaintenanceRenewalItemDAL();
      MaintUpdateNotifySvcWrapper.UpdateNotificationItem(renewal, versionInfo);
      return renewal;
    }

    public static void UpdateNotificationItem(
      MaintenanceRenewalItemDAL renewal,
      VersionInfo versionInfo)
    {
      if (string.IsNullOrEmpty(versionInfo.Hotfix))
        renewal.Title = versionInfo.Message.MaintenanceMessage;
      else
        renewal.Title = string.Format("{0} {1}", (object) versionInfo.Message.MaintenanceMessage, (object) versionInfo.Hotfix);
      renewal.Description = versionInfo.ReleaseNotes;
      if (renewal.DateReleased < versionInfo.DateReleased)
        renewal.Ignored = false;
      renewal.Url = versionInfo.Link;
      renewal.SetNotAcknowledged();
      renewal.ProductTag = versionInfo.ProductTag;
      renewal.DateReleased = versionInfo.DateReleased;
      renewal.NewVersion = versionInfo.Version;
    }

    public static ModuleInfo[] GetModules(List<ModuleInfo> listModules)
    {
      ModuleInfo[] moduleInfoArray = new ModuleInfo[listModules.Count];
      for (int index = 0; index < listModules.Count; ++index)
        moduleInfoArray[index] = new ModuleInfo()
        {
          ProductDisplayName = listModules[index].get_ProductDisplayName(),
          HotfixVersion = listModules[index].get_HotfixVersion(),
          Version = listModules[index].get_Version(),
          ProductTag = listModules[index].get_ProductTag(),
          LicenseInfo = listModules[index].get_LicenseInfo()
        };
      return moduleInfoArray;
    }
  }
}
