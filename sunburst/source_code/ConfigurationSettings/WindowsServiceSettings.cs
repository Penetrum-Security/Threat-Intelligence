// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.ConfigurationSettings.WindowsServiceSettings
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Settings;

namespace SolarWinds.Orion.Core.BusinessLayer.ConfigurationSettings
{
  internal class WindowsServiceSettings : SettingsBase
  {
    public static readonly WindowsServiceSettings Instance = new WindowsServiceSettings();
    [Setting(AllowServerOverride = true, Default = 20000, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int ServiceTimeout;

    private WindowsServiceSettings()
    {
      base.\u002Ector();
    }
  }
}
