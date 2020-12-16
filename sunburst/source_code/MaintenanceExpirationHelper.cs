// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintenanceExpirationHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal static class MaintenanceExpirationHelper
  {
    private static readonly Log log = new Log();

    internal static void CheckMaintenanceExpiration()
    {
      try
      {
        MaintenanceExpirationHelper.log.Debug((object) "Check Maintenance expiration");
        int expirationWarningDays = BusinessLayerSettings.Instance.MaintenanceExpirationWarningDays;
        Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> moduleExpirations = new Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>();
        ILicensingDAL licensing = (ILicensingDAL) new LicensingDAL();
        using (IEnumerator<LicenseInfoModel> enumerator = ((IEnumerable<LicenseInfoModel>) licensing.GetLicenses()).Where<LicenseInfoModel>((Func<LicenseInfoModel, bool>) (lic => !lic.get_IsHidden() && !lic.get_IsEvaluation() && !licensing.get_DefaultLicenseFilter().Contains<string>(lic.get_ProductName(), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase))).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            LicenseInfoModel current = enumerator.Current;
            if (MaintenanceExpirationHelper.log.get_IsDebugEnabled())
              MaintenanceExpirationHelper.log.Debug((object) string.Format("Module:{0} MaintenanceTo:{1} DaysLeft:{2}", (object) current.get_LicenseName(), (object) current.get_MaintenanceExpiration().Date, (object) current.get_DaysRemainingCount()));
            if (current.get_DaysRemainingCount() <= expirationWarningDays)
            {
              MaintenanceExpirationNotificationItemDAL.ExpirationInfo expirationInfo = new MaintenanceExpirationNotificationItemDAL.ExpirationInfo()
              {
                DaysToExpire = current.get_DaysRemainingCount(),
                ActivationKey = current.get_LicenseKey()
              };
              moduleExpirations[current.get_LicenseName()] = expirationInfo;
            }
          }
        }
        if (moduleExpirations.Count > 0)
        {
          MaintenanceExpirationHelper.log.Debug((object) string.Format("{0} products found to be notified", (object) moduleExpirations.Count));
          MaintenanceExpirationNotificationItemDAL.Show(moduleExpirations);
        }
        else
          MaintenanceExpirationNotificationItemDAL.Hide();
      }
      catch (Exception ex)
      {
        MaintenanceExpirationHelper.log.Warn((object) "Exception while checking maintenance expiration status: ", ex);
      }
    }
  }
}
