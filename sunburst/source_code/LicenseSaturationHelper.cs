// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.LicenseSaturationHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common.Licensing;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal static class LicenseSaturationHelper
  {
    private static readonly Log Log = new Log();
    private static readonly int SaturationLimit = Settings.LicenseSaturationPercentage;

    internal static void CheckLicenseSaturation()
    {
      try
      {
        LicenseSaturationHelper.Log.Debug((object) "Checking license saturation");
        List<ModuleLicenseSaturationInfo> modulesSaturationInfo = LicenseSaturationLogic.GetModulesSaturationInfo(new int?(LicenseSaturationHelper.SaturationLimit));
        if (modulesSaturationInfo.Count == 0)
        {
          LicenseSaturationHelper.Log.DebugFormat("All modules below {0}% of their license", (object) LicenseSaturationHelper.SaturationLimit);
          LicenseSaturationNotificationItemDAL.Hide();
          LicensePreSaturationNotificationItemDAL.Hide();
        }
        else
        {
          List<ModuleLicenseSaturationInfo> list1 = ((IEnumerable<ModuleLicenseSaturationInfo>) modulesSaturationInfo).Where<ModuleLicenseSaturationInfo>((Func<ModuleLicenseSaturationInfo, bool>) (q => ((IEnumerable<ElementLicenseSaturationInfo>) q.get_ElementList()).Any<ElementLicenseSaturationInfo>((Func<ElementLicenseSaturationInfo, bool>) (l => l.get_Saturation() > 99.0)))).ToList<ModuleLicenseSaturationInfo>();
          List<ModuleLicenseSaturationInfo> list2 = ((IEnumerable<ModuleLicenseSaturationInfo>) modulesSaturationInfo).Where<ModuleLicenseSaturationInfo>((Func<ModuleLicenseSaturationInfo, bool>) (q => ((IEnumerable<ElementLicenseSaturationInfo>) q.get_ElementList()).Any<ElementLicenseSaturationInfo>((Func<ElementLicenseSaturationInfo, bool>) (l => l.get_Saturation() > (double) LicenseSaturationHelper.SaturationLimit && l.get_Saturation() < 100.0)))).ToList<ModuleLicenseSaturationInfo>();
          List<ElementLicenseSaturationInfo> overUsedElements = new List<ElementLicenseSaturationInfo>();
          list1.ForEach((Action<ModuleLicenseSaturationInfo>) (l => overUsedElements.AddRange((IEnumerable<ElementLicenseSaturationInfo>) l.get_ElementList().ToArray())));
          if (LicenseSaturationHelper.Log.get_IsInfoEnabled())
            LicenseSaturationHelper.Log.InfoFormat("These elements are at 100% of their license: {0}", (object) string.Join(";", ((IEnumerable<ElementLicenseSaturationInfo>) overUsedElements).Select<ElementLicenseSaturationInfo, string>((Func<ElementLicenseSaturationInfo, string>) (q => q.get_ElementType()))));
          LicenseSaturationNotificationItemDAL.Show(((IEnumerable<ElementLicenseSaturationInfo>) overUsedElements).Select<ElementLicenseSaturationInfo, string>((Func<ElementLicenseSaturationInfo, string>) (q => q.get_ElementType())));
          List<ElementLicenseSaturationInfo> warningElements = new List<ElementLicenseSaturationInfo>();
          Action<ModuleLicenseSaturationInfo> action = (Action<ModuleLicenseSaturationInfo>) (l => warningElements.AddRange((IEnumerable<ElementLicenseSaturationInfo>) l.get_ElementList().ToArray()));
          list2.ForEach(action);
          if (LicenseSaturationHelper.Log.get_IsInfoEnabled())
            LicenseSaturationHelper.Log.InfoFormat("These elements are above {0}% of their license: {1}", (object) LicenseSaturationHelper.SaturationLimit, (object) string.Join(";", ((IEnumerable<ElementLicenseSaturationInfo>) warningElements).Select<ElementLicenseSaturationInfo, string>((Func<ElementLicenseSaturationInfo, string>) (q => q.get_ElementType()))));
          LicensePreSaturationNotificationItemDAL.Show();
        }
      }
      catch (Exception ex)
      {
        LicenseSaturationHelper.Log.Error((object) "Exception running CheckLicenseSaturation:", ex);
      }
    }

    internal static void SaveElementsUsageInfo()
    {
      try
      {
        LicenseSaturationHelper.Log.Debug((object) "Collecting elements usage information to store in history");
        List<ModuleLicenseSaturationInfo> modulesSaturationInfo = LicenseSaturationLogic.GetModulesSaturationInfo(new int?());
        if (modulesSaturationInfo.Count != 0)
        {
          List<ElementLicenseSaturationInfo> elements = new List<ElementLicenseSaturationInfo>();
          modulesSaturationInfo.ForEach((Action<ModuleLicenseSaturationInfo>) (m => elements.AddRange((IEnumerable<ElementLicenseSaturationInfo>) m.get_ElementList().ToArray())));
          ElementsUsageDAL.Save((IEnumerable<ElementLicenseSaturationInfo>) elements);
        }
        else
          LicenseSaturationHelper.Log.DebugFormat("There is no elements usage information to store in history", Array.Empty<object>());
      }
      catch (Exception ex)
      {
        LicenseSaturationHelper.Log.Error((object) "Exception running SaveElementsUsageInfo:", ex);
      }
    }
  }
}
