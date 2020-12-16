// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.IAgentInfoDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Agent;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public interface IAgentInfoDAL
  {
    IEnumerable<AgentInfo> GetAgentsInfo();

    IEnumerable<AgentInfo> GetAgentsByNodesFilter(
      int engineId,
      string nodesFilter);

    AgentInfo GetAgentInfoByNode(int nodeId);

    AgentInfo GetAgentInfo(int agentId);

    AgentInfo GetAgentInfoByIpOrHostname(string ipAddress, string hostname);

    AgentInfo GetAgentInfoByAgentAddress(string address);

    bool IsUniqueAgentName(string agentName);
  }
}
