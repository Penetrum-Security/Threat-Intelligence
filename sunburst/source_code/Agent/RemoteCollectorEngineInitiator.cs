// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.RemoteCollectorEngineInitiator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.BusinessLayer.Engines;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.JobEngine;
using System;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class RemoteCollectorEngineInitiator : IEngineInitiator
  {
    private static readonly Dictionary<string, object> DefaultValues = new Dictionary<string, object>()
    {
      {
        "EngineVersion",
        (object) RegistrySettings.GetVersionDisplayString()
      }
    };
    private readonly bool _interfaceAvailable;
    private readonly IEngineDAL _engineDal;
    private readonly IThrottlingStatusProvider _throttlingStatusProvider;
    private readonly IEngineComponent _engineComponent;
    private const float DefaultPollingCompletion = 0.0f;

    public RemoteCollectorEngineInitiator(
      int engineId,
      string engineName,
      bool interfaceAvailable,
      IEngineDAL engineDal,
      IThrottlingStatusProvider throttlingStatusProvider,
      IEngineComponent engineComponent)
    {
      if (engineName == null)
        throw new ArgumentNullException(nameof (engineName));
      IEngineDAL iengineDal = engineDal;
      if (iengineDal == null)
        throw new ArgumentNullException(nameof (engineDal));
      this._engineDal = iengineDal;
      IThrottlingStatusProvider ithrottlingStatusProvider = throttlingStatusProvider;
      if (ithrottlingStatusProvider == null)
        throw new ArgumentNullException(nameof (throttlingStatusProvider));
      this._throttlingStatusProvider = ithrottlingStatusProvider;
      IEngineComponent engineComponent1 = engineComponent;
      if (engineComponent1 == null)
        throw new ArgumentNullException(nameof (engineComponent));
      this._engineComponent = engineComponent1;
      this.EngineId = engineId;
      this.ServerName = engineName.ToUpperInvariant();
      this._interfaceAvailable = interfaceAvailable;
    }

    public int EngineId { get; }

    public string ServerName { get; }

    public EngineComponentStatus ComponentStatus
    {
      get
      {
        return this._engineComponent.GetStatus();
      }
    }

    public bool AllowKeepAlive
    {
      get
      {
        return this.ComponentStatus == EngineComponentStatus.Up;
      }
    }

    public bool AllowPollingCompletion
    {
      get
      {
        return this.ComponentStatus == EngineComponentStatus.Up;
      }
    }

    public void InitializeEngine()
    {
      this._engineDal.UpdateEngineInfo(this.EngineId, RemoteCollectorEngineInitiator.DefaultValues, false, this._interfaceAvailable, this.AllowKeepAlive);
    }

    public void UpdateInfo(bool updateJobEngineThrottleInfo)
    {
      this._engineDal.UpdateEngineInfo(this.EngineId, new Dictionary<string, object>()
      {
        {
          "PollingCompletion",
          (object) (this.AllowPollingCompletion & updateJobEngineThrottleInfo ? this._throttlingStatusProvider.GetPollingCompletion() : 0.0f)
        }
      }, true, this._interfaceAvailable, this.AllowKeepAlive);
    }
  }
}
