// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Settings
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal static class Settings
  {
    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    internal static TimeSpan DiscoverESXNodesTimer
    {
      get
      {
        return TimeSpan.FromMinutes(5.0);
      }
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    internal static TimeSpan UpdateESXNotificationsTimer
    {
      get
      {
        return TimeSpan.FromMinutes(2.0);
      }
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    internal static TimeSpan VMwareESXJobTimeout
    {
      get
      {
        return TimeSpan.FromMinutes(10.0);
      }
    }

    internal static TimeSpan CheckMaintenanceRenewalsTimer
    {
      get
      {
        return TimeSpan.FromDays(7.0);
      }
    }

    internal static TimeSpan CheckOrionProductTeamBlogTimer
    {
      get
      {
        return TimeSpan.FromDays(7.0);
      }
    }

    internal static bool IsProductsBlogDisabled
    {
      get
      {
        return SettingsDAL.Get("ProductsBlog-Disable").Equals("1");
      }
    }

    internal static bool IsMaintenanceRenewalsDisabled
    {
      get
      {
        return SettingsDAL.Get("MaintenanceRenewals-Disable").Equals("1");
      }
    }

    internal static bool IsLicenseSaturationDisabled
    {
      get
      {
        return SettingsDAL.Get("LicenseSaturation-Disable").Equals("1");
      }
    }

    internal static bool IsAutomaticGeolocationEnabled
    {
      get
      {
        return SettingsDAL.Get("AutomaticGeolocation-Enable").Equals("1");
      }
    }

    internal static TimeSpan AutomaticGeolocationCheckInterval
    {
      get
      {
        string s;
        TimeSpan result;
        return WebSettingsDAL.TryGet(nameof (AutomaticGeolocationCheckInterval), ref s) && TimeSpan.TryParse(s, out result) ? result : TimeSpan.FromHours(1.0);
      }
    }

    internal static int LicenseSaturationPercentage
    {
      get
      {
        int result;
        return int.TryParse(SettingsDAL.Get("LicenseSaturation-WarningPercentage"), out result) ? result : 80;
      }
    }

    internal static int PollerLimitWarningScaleFactor
    {
      get
      {
        int result;
        return int.TryParse(SettingsDAL.Get(nameof (PollerLimitWarningScaleFactor)), out result) ? result : 85;
      }
    }

    internal static int PollerLimitreachedScaleFactor
    {
      get
      {
        int result;
        return int.TryParse(SettingsDAL.Get("PollerLimitReachedScaleFactor"), out result) ? result : 100;
      }
    }
  }
}
