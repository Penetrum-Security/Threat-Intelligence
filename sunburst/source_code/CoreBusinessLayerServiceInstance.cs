// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.CoreBusinessLayerServiceInstance
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Orion.Core.BusinessLayer.Discovery;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.BusinessLayer;
using SolarWinds.Orion.Core.Common.Proxy.BusinessLayer;
using SolarWinds.ServiceDirectory.Client.Contract;
using System;
using System.ServiceModel;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class CoreBusinessLayerServiceInstance : BusinessLayerServiceInstanceBase<CoreBusinessLayerService>
  {
    private RescheduleDiscoveryJobsTask _discoveryJobRescheduler;
    private readonly IEngineInitiator _engineInitiator;

    public CoreBusinessLayerServiceInstance(
      int engineId,
      IEngineInitiator engineInitiator,
      CoreBusinessLayerService serviceInstance,
      ServiceHostBase serviceHost,
      IServiceDirectoryClient serviceDirectoryClient)
    {
      this.\u002Ector(engineId, engineInitiator.get_ServerName(), serviceInstance, serviceHost, serviceDirectoryClient);
      this._engineInitiator = engineInitiator;
      this.Service = serviceInstance;
      this.ServiceLogicalInstanceId = CoreBusinessLayerConfiguration.GetLogicalInstanceId(this.get_EngineId());
    }

    public CoreBusinessLayerService Service { get; }

    protected virtual string ServiceId
    {
      get
      {
        return "Core.BusinessLayer";
      }
    }

    protected virtual string ServiceLogicalInstanceId { get; }

    public void RouteJobToEngine(JobDescription jobDescription)
    {
      if (!string.IsNullOrEmpty(jobDescription.get_LegacyEngine()))
        return;
      jobDescription.set_LegacyEngine(this.get_EngineName());
    }

    public void StopRescheduleEngineDiscoveryJobsTask()
    {
      using (this._discoveryJobRescheduler)
        this._discoveryJobRescheduler = (RescheduleDiscoveryJobsTask) null;
    }

    public void InitRescheduleEngineDiscoveryJobsTask(bool isMaster)
    {
      this._discoveryJobRescheduler = new RescheduleDiscoveryJobsTask(new Func<int, bool>(this.Service.UpdateDiscoveryJobs), this.get_EngineId(), !isMaster, isMaster ? TimeSpan.FromSeconds(10.0) : TimeSpan.FromMinutes(10.0));
      this._discoveryJobRescheduler.StartPeriodicRescheduleTask();
    }

    public void RunRescheduleEngineDiscoveryJobsTask()
    {
      this._discoveryJobRescheduler?.QueueRescheduleAttempt();
    }

    public void InitializeEngine()
    {
      this._engineInitiator.InitializeEngine();
    }

    public void UpdateEngine(bool updateJobEngineThrottleInfo)
    {
      this._engineInitiator.UpdateInfo(updateJobEngineThrottleInfo);
    }
  }
}
