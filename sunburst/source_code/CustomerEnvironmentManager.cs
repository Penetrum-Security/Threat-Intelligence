// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.CustomerEnvironmentManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class CustomerEnvironmentManager
  {
    public static CustomerEnvironmentInfoPack GetEnvironmentInfoPack()
    {
      CustomerEnvironmentInfoPack environmentInfoPack = new CustomerEnvironmentInfoPack();
      environmentInfoPack.OSVersion = Environment.OSVersion.VersionString;
      MaintenanceRenewalsCheckStatusDAL checkStatus = MaintenanceRenewalsCheckStatusDAL.GetCheckStatus();
      DateTime minValue;
      if (checkStatus != null)
      {
        DateTime? lastUpdateCheck = checkStatus.LastUpdateCheck;
        if (lastUpdateCheck.HasValue)
        {
          lastUpdateCheck = checkStatus.LastUpdateCheck;
          minValue = lastUpdateCheck.Value;
          goto label_4;
        }
      }
      minValue = DateTime.MinValue;
label_4:
      environmentInfoPack.LastUpdateCheck = minValue;
      environmentInfoPack.OrionDBVersion = DatabaseInfoDAL.GetOrionDBVersion();
      environmentInfoPack.SQLVersion = DatabaseInfoDAL.GetSQLEngineVersion();
      environmentInfoPack.Modules = MaintUpdateNotifySvcWrapper.GetModules(ModulesCollector.GetInstalledModules());
      environmentInfoPack.CustomerUniqueId = ModulesCollector.GetCustomerUniqueId();
      return environmentInfoPack;
    }
  }
}
