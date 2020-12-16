// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Engines.MasterEngineInitiator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Licensing.Framework;
using SolarWinds.Licensing.Framework.Interfaces;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.JobEngine;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SolarWinds.Orion.Core.BusinessLayer.Engines
{
  public class MasterEngineInitiator : EngineHelper, IEngineInitiator
  {
    private static readonly Log log = new Log();
    internal Func<ILicenseManagerGen3> GetLicenseManager;
    internal Func<bool> GetIsAnyPoller;
    private readonly IThrottlingStatusProvider _throttlingStatusProvider;

    public MasterEngineInitiator()
      : this((IThrottlingStatusProvider) new ThrottlingStatusProvider())
    {
    }

    public MasterEngineInitiator(IThrottlingStatusProvider throttlingStatusProvider)
    {
      base.\u002Ector();
      IThrottlingStatusProvider ithrottlingStatusProvider = throttlingStatusProvider;
      if (ithrottlingStatusProvider == null)
        throw new ArgumentNullException(nameof (throttlingStatusProvider));
      this._throttlingStatusProvider = ithrottlingStatusProvider;
    }

    public float GetPollingCompletion()
    {
      return this._throttlingStatusProvider.GetPollingCompletion();
    }

    public void UpdateInfo()
    {
      this.UpdateInfo(true);
    }

    public void UpdateInfo(bool updateJobEngineThrottleInfo)
    {
      if (this.get_EngineID() == 0)
        throw new InvalidOperationException("Class wasn't initialized");
      int engineId = this.get_EngineID();
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      dictionary.Add("IP", (object) this.GetIPAddress());
      dictionary.Add("PollingCompletion", (object) this.GetPollingCompletion());
      int num = this.get_InterfacesSupported() ? 1 : 0;
      EngineDAL.UpdateEngineInfo(engineId, dictionary, true, num != 0);
      if (!updateJobEngineThrottleInfo)
        return;
      this.UpdateEngineThrottleInfo();
    }

    public void UpdateEngineThrottleInfo()
    {
      List<string> stringList = new List<string>();
      stringList.Add("Total Weight");
      stringList.Add("Scale Factor");
      try
      {
        List<EngineProperty> enginePropertyList = new List<EngineProperty>();
        int totalJobWeight = this._throttlingStatusProvider.GetTotalJobWeight();
        enginePropertyList.Add(new EngineProperty("Total Job Weight", "Total Weight", totalJobWeight.ToString()));
        foreach (KeyValuePair<string, int> scaleFactor in this._throttlingStatusProvider.GetScaleFactors())
          enginePropertyList.Add(new EngineProperty(scaleFactor.Key, "Scale Factor", scaleFactor.Value.ToString()));
        try
        {
          enginePropertyList.Add(new EngineProperty("Scale Licenses", "Scale Licenses", this.GetStackablePollersCount().ToString()));
          stringList.Add("Scale Licenses");
        }
        catch (Exception ex)
        {
          MasterEngineInitiator.log.Error((object) "Can't load stackable poller licenses", ex);
        }
        EngineDAL.UpdateEngineProperties(this.get_EngineID(), (IEnumerable<EngineProperty>) enginePropertyList, stringList.ToArray());
      }
      catch (Exception ex)
      {
        if (this.get_ThrowExceptions())
          throw;
        else
          MasterEngineInitiator.log.Error((object) ex);
      }
    }

    internal ulong GetStackablePollersCount()
    {
      ulong val1 = 0;
      LicenseType[] allowedLicensesTypes = new LicenseType[3]
      {
        (LicenseType) 4,
        (LicenseType) 7,
        null
      };
      List<\u003C\u003Ef__AnonymousType1<string, ulong>> list = ((IEnumerable<IProductLicense>) ((ILicenseManager) this.GetLicenseManager()).GetLicenses()).Where<IProductLicense>((Func<IProductLicense, bool>) (license => license.get_ExpirationDaysLeft() > 0 && ((IEnumerable<LicenseType>) allowedLicensesTypes).Contains<LicenseType>(license.get_LicenseType()) && license.GetFeature("Core.FeatureManager.Features.PollingEngine") != null)).Select(license => new
      {
        ProductName = license.get_ProductName(),
        PollerFeatureValue = license.GetFeature("Core.FeatureManager.Features.PollingEngine").get_Available()
      }).Where(licInfo => licInfo.PollerFeatureValue > 0UL).ToList();
      if (MasterEngineInitiator.log.get_IsDebugEnabled())
      {
        MasterEngineInitiator.log.Debug((object) "All available commercial and not expired licenses with PollingEngine feature to be processed:");
        list.ForEach(l => MasterEngineInitiator.log.Debug((object) string.Format("License product name: {0}, PollingEngine feature value:{1}", (object) l.ProductName, (object) l.PollerFeatureValue)));
      }
      try
      {
        foreach (var data in list.Where(l => l.ProductName.Equals("Core", StringComparison.OrdinalIgnoreCase)))
          checked { val1 += data.PollerFeatureValue; }
        if (!this.GetIsAnyPoller())
          checked { val1 += list.Where(license => !license.ProductName.Equals("Core", StringComparison.OrdinalIgnoreCase)).Select(license => license.PollerFeatureValue).DefaultIfEmpty<ulong>(1UL).Max<ulong>(); }
        return Math.Min(val1, 4UL);
      }
      catch (OverflowException ex)
      {
        return 4;
      }
    }

    [SpecialName]
    string IEngineInitiator.get_ServerName()
    {
      return this.get_ServerName();
    }

    void IEngineInitiator.InitializeEngine()
    {
      this.InitializeEngine();
    }
  }
}
