// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.RemoteCollectorEngineComponent
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.AgentManagement.Contract;
using SolarWinds.Orion.Core.BusinessLayer.Engines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class RemoteCollectorEngineComponent : IEngineComponent
  {
    private static readonly AgentStatus[] EngineUpStatuses;
    private readonly IRemoteCollectorAgentStatusProvider _agentStatusProvider;

    public RemoteCollectorEngineComponent(
      int engineId,
      IRemoteCollectorAgentStatusProvider agentStatusProvider)
    {
      IRemoteCollectorAgentStatusProvider agentStatusProvider1 = agentStatusProvider;
      if (agentStatusProvider1 == null)
        throw new ArgumentNullException(nameof (agentStatusProvider));
      this._agentStatusProvider = agentStatusProvider1;
      this.EngineId = engineId;
    }

    public int EngineId { get; }

    public EngineComponentStatus GetStatus()
    {
      return RemoteCollectorEngineComponent.ToEngineStatus(this._agentStatusProvider.GetStatus(this.EngineId));
    }

    private static EngineComponentStatus ToEngineStatus(AgentStatus agentStatus)
    {
      return !((IEnumerable<AgentStatus>) RemoteCollectorEngineComponent.EngineUpStatuses).Contains<AgentStatus>(agentStatus) ? EngineComponentStatus.Down : EngineComponentStatus.Up;
    }

    static RemoteCollectorEngineComponent()
    {
      // ISSUE: unable to decompile the method.
    }
  }
}
