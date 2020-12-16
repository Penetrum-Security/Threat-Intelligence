// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.RemoteCollectorStatusProvider
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.AgentManagement.Contract;
using SolarWinds.Orion.Core.Common.Internals;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class RemoteCollectorStatusProvider : IRemoteCollectorAgentStatusProvider
  {
    private readonly CacheWithExpiration<IDictionary<int, AgentStatus>> _statusCache;

    public RemoteCollectorStatusProvider(
      ISwisConnectionProxyCreator swisProxyCreator,
      int masterEngineId,
      int cacheExpiration)
      : this(cacheExpiration, (Func<IDictionary<int, AgentStatus>>) (() => RemoteCollectorStatusProvider.GetCurrentStatuses(swisProxyCreator, masterEngineId)), (Func<DateTime>) (() => DateTime.UtcNow))
    {
      if (swisProxyCreator == null)
        throw new ArgumentNullException(nameof (swisProxyCreator));
    }

    internal RemoteCollectorStatusProvider(
      int cacheExpiration,
      Func<IDictionary<int, AgentStatus>> refreshFunc,
      Func<DateTime> currentTimeFunc)
    {
      if (currentTimeFunc == null)
        throw new ArgumentNullException(nameof (currentTimeFunc));
      this._statusCache = new CacheWithExpiration<IDictionary<int, AgentStatus>>(cacheExpiration, refreshFunc, currentTimeFunc);
    }

    public AgentStatus GetStatus(int engineId)
    {
      AgentStatus agentStatus;
      return !this._statusCache.Get().TryGetValue(engineId, out agentStatus) ? (AgentStatus) 0 : agentStatus;
    }

    public void InvalidateCache()
    {
      this._statusCache.Invalidate();
    }

    private static IDictionary<int, AgentStatus> GetCurrentStatuses(
      ISwisConnectionProxyCreator swisProxyCreator,
      int masterEngineId)
    {
      return (IDictionary<int, AgentStatus>) RemoteCollectorStatusProvider.GetStatuses(swisProxyCreator, masterEngineId).ToDictionary<KeyValuePair<int, AgentStatus>, int, AgentStatus>((Func<KeyValuePair<int, AgentStatus>, int>) (i => i.Key), (Func<KeyValuePair<int, AgentStatus>, AgentStatus>) (i => i.Value));
    }

    internal static IEnumerable<KeyValuePair<int, AgentStatus>> GetStatuses(
      ISwisConnectionProxyCreator swisProxyCreator,
      int masterEngineId)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<KeyValuePair<int, AgentStatus>>) new RemoteCollectorStatusProvider.\u003CGetStatuses\u003Ed__6(-2)
      {
        \u003C\u003E3__swisProxyCreator = swisProxyCreator,
        \u003C\u003E3__masterEngineId = masterEngineId
      };
    }
  }
}
