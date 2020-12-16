// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.SettingsToRegistry
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class SettingsToRegistry
  {
    private static readonly Log log = new Log();
    internal Action<string, object> WriteToRegistry = (Action<string, object>) ((a, b) => OrionConfiguration.SetSetting(a, b));
    internal bool ThrowExceptions;

    public void Synchronize()
    {
      this.Synchronize((SynchronizeItem) new SettingItem("SWNetPerfMon-Settings-SNMP-SocketRecyclingInterval"), "SNMP_SocketRecyclingInterval");
      this.Synchronize((SynchronizeItem) new SettingItem("SWNetPerfMon-Settings-SNMP-SocketKeepAliveInterval"), "SNMP_SocketKeepAliveInterval");
    }

    public void Synchronize(SynchronizeItem item, string registryValueName)
    {
      try
      {
        SettingsToRegistry.log.VerboseFormat("Synchronize ... {0}", new object[1]
        {
          (object) item
        });
        object databaseValue = item.GetDatabaseValue();
        SettingsToRegistry.log.VerboseFormat("Synchronize ... {0} - value {1}", new object[2]
        {
          (object) item,
          databaseValue
        });
        if (databaseValue == null)
          return;
        this.WriteToRegistry(registryValueName, databaseValue);
      }
      catch (Exception ex)
      {
        SettingsToRegistry.log.Error((object) string.Format("Failed to synchronize {0}", (object) item), ex);
        if (!this.ThrowExceptions)
          return;
        throw;
      }
    }
  }
}
