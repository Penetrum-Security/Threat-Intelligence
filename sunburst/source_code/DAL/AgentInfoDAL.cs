// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.AgentInfoDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.AgentManagement.Contract.Models;
using SolarWinds.InformationService.InformationServiceClient;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Agent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class AgentInfoDAL : IAgentInfoDAL
  {
    public IEnumerable<AgentInfo> GetAgentsInfo()
    {
      return this.GetAgentsInfo((string) null, (IDictionary<string, object>) null);
    }

    public AgentInfo GetAgentInfoByNode(int nodeId)
    {
      return this.GetAgentsInfo("a.NodeID = @NodeID", (IDictionary<string, object>) new Dictionary<string, object>()
      {
        {
          "NodeID",
          (object) nodeId
        }
      }).FirstOrDefault<AgentInfo>();
    }

    public AgentInfo GetAgentInfo(int agentId)
    {
      return this.GetAgentsInfo("a.AgentId = @AgentId", (IDictionary<string, object>) new Dictionary<string, object>()
      {
        {
          "AgentId",
          (object) agentId
        }
      }).FirstOrDefault<AgentInfo>();
    }

    public AgentInfo GetAgentInfoByAgentAddress(string address)
    {
      Guid result;
      if (!Guid.TryParse(address, out result))
        return (AgentInfo) null;
      return this.GetAgentsInfo("a.AgentGuid = @AgentGuid", (IDictionary<string, object>) new Dictionary<string, object>()
      {
        {
          "AgentGuid",
          (object) result
        }
      }).FirstOrDefault<AgentInfo>();
    }

    public AgentInfo GetAgentInfoByIpOrHostname(string ipAddress, string hostname)
    {
      if (string.IsNullOrWhiteSpace(ipAddress) && string.IsNullOrWhiteSpace(hostname))
        throw new ArgumentException("ipAddress or hostname must be specified");
      List<string> stringList = new List<string>();
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      if (!string.IsNullOrWhiteSpace(ipAddress))
      {
        stringList.Add("a.IP = @ipAddress");
        dictionary.Add(nameof (ipAddress), (object) ipAddress);
      }
      if (!string.IsNullOrWhiteSpace(hostname))
      {
        stringList.Add("a.Hostname = @hostname");
        dictionary.Add(nameof (hostname), (object) hostname);
      }
      return this.GetAgentsInfo(string.Format("({0}) AND (n.ObjectSubType IS NULL OR n.ObjectSubType <> 'Agent')", (object) string.Join(" OR ", (IEnumerable<string>) stringList)), (IDictionary<string, object>) dictionary).FirstOrDefault<AgentInfo>();
    }

    public IEnumerable<AgentInfo> GetAgentsByNodesFilter(
      int engineId,
      string nodesWhereClause)
    {
      List<AgentInfo> agentInfoList = new List<AgentInfo>();
      using (InformationServiceConnection systemConnectionV3 = InformationServiceConnectionProvider.CreateSystemConnectionV3())
      {
        using (InformationServiceCommand command = systemConnectionV3.CreateCommand())
        {
          ((DbCommand) command).CommandText = string.Format("SELECT agents.AgentId, \r\n                            agents.AgentGuid, \r\n                            agents.NodeId, \r\n                            nodes.ObjectSubType,\r\n                            agents.Uri,\r\n                            agents.Name, \r\n                            agents.Hostname, \r\n                            agents.IP, \r\n                            agents.PollingEngineId,\r\n                            agents.AgentStatus, \r\n                            agents.AgentStatusMessage, \r\n                            agents.ConnectionStatus, \r\n                            agents.ConnectionStatusMessage,\r\n                            agents.OSType,\r\n                            agents.OSDistro,\r\n                            agents.OSVersion,\r\n                            p.PluginId, \r\n                            p.Status, \r\n                            p.StatusMessage\r\n                    FROM Orion.AgentManagement.Agent agents\r\n                    INNER JOIN Orion.Nodes Nodes ON Nodes.NodeID = agents.NodeID\r\n                    LEFT JOIN Orion.AgentManagement.AgentPlugin p ON agents.AgentId = p.AgentId \r\n                    WHERE agents.PollingEngineId=@engineId AND agents.ConnectionStatus = @connectionStatus {0}\r\n                    ORDER BY agents.AgentId", !string.IsNullOrWhiteSpace(nodesWhereClause) ? (object) (" AND " + nodesWhereClause) : (object) string.Empty);
          command.get_Parameters().AddWithValue("connectionStatus", (object) 1);
          command.get_Parameters().AddWithValue(nameof (engineId), (object) engineId);
          using (IDataReader reader = (IDataReader) command.ExecuteReader())
            agentInfoList = this.GetAgentsFromReader(reader);
        }
      }
      return ((IEnumerable<AgentInfo>) agentInfoList).Where<AgentInfo>((Func<AgentInfo, bool>) (x => x.get_NodeID().HasValue && x.get_NodeSubType() == "Agent"));
    }

    private IEnumerable<AgentInfo> GetAgentsInfo(
      string whereClause,
      IDictionary<string, object> parameters)
    {
      List<AgentInfo> agentInfoList = new List<AgentInfo>();
      using (InformationServiceConnection systemConnectionV3 = InformationServiceConnectionProvider.CreateSystemConnectionV3())
      {
        using (InformationServiceCommand command = systemConnectionV3.CreateCommand())
        {
          ((DbCommand) command).CommandText = string.Format("SELECT a.AgentId, \r\n                            a.AgentGuid, \r\n                            a.NodeId, \r\n                            n.ObjectSubType,\r\n                            a.Uri,\r\n                            a.Name, \r\n                            a.Hostname, \r\n                            a.IP, \r\n                            a.PollingEngineId,\r\n                            a.AgentStatus, \r\n                            a.AgentStatusMessage, \r\n                            a.ConnectionStatus, \r\n                            a.ConnectionStatusMessage,\r\n                            p.PluginId, \r\n                            p.Status, \r\n                            p.StatusMessage,\r\n                            a.OSType,\r\n                            a.OSDistro,\r\n                            a.OSVersion\r\n                    FROM Orion.AgentManagement.Agent a\r\n                    LEFT JOIN Orion.Nodes n ON n.NodeID = a.NodeID\r\n                    LEFT JOIN Orion.AgentManagement.AgentPlugin p ON a.AgentId = p.AgentId \r\n                    {0}\r\n                    ORDER BY a.AgentId", !string.IsNullOrEmpty(whereClause) ? (object) ("WHERE " + whereClause) : (object) string.Empty);
          if (parameters != null)
          {
            foreach (KeyValuePair<string, object> parameter in (IEnumerable<KeyValuePair<string, object>>) parameters)
              command.get_Parameters().AddWithValue(parameter.Key, parameter.Value);
          }
          using (IDataReader reader = (IDataReader) command.ExecuteReader())
            return (IEnumerable<AgentInfo>) this.GetAgentsFromReader(reader);
        }
      }
    }

    private List<AgentInfo> GetAgentsFromReader(IDataReader reader)
    {
      List<AgentInfo> agentInfoList = new List<AgentInfo>();
      AgentInfo agentInfo1 = (AgentInfo) null;
      while (reader.Read())
      {
        OsType result1;
        if (!Enum.TryParse<OsType>(DatabaseFunctions.GetString(reader, "OSType"), true, out result1))
          result1 = (OsType) 0;
        AgentInfo agentInfo2 = new AgentInfo();
        agentInfo2.set_AgentId(DatabaseFunctions.GetInt32(reader, "AgentId"));
        agentInfo2.set_AgentGuid(DatabaseFunctions.GetGuid(reader, "AgentGuid"));
        agentInfo2.set_NodeID(DatabaseFunctions.GetNullableInt32(reader, "NodeId"));
        agentInfo2.set_NodeSubType(DatabaseFunctions.GetString(reader, "ObjectSubType", (string) null));
        agentInfo2.set_Uri(DatabaseFunctions.GetString(reader, "Uri"));
        agentInfo2.set_PollingEngineId(DatabaseFunctions.GetInt32(reader, "PollingEngineId"));
        agentInfo2.set_AgentStatus(DatabaseFunctions.GetInt32(reader, "AgentStatus"));
        agentInfo2.set_AgentStatusMessage(DatabaseFunctions.GetString(reader, "AgentStatusMessage"));
        agentInfo2.set_ConnectionStatus(DatabaseFunctions.GetInt32(reader, "ConnectionStatus"));
        agentInfo2.set_ConnectionStatusMessage(DatabaseFunctions.GetString(reader, "ConnectionStatusMessage"));
        agentInfo2.set_OsType(result1);
        agentInfo2.set_OsDistro(DatabaseFunctions.GetString(reader, "OSDistro"));
        AgentInfo agentInfo3 = agentInfo2;
        agentInfo3.set_Name(DatabaseFunctions.GetString(reader, "Name"));
        agentInfo3.set_HostName(DatabaseFunctions.GetString(reader, "HostName"));
        agentInfo3.set_IPAddress(DatabaseFunctions.GetString(reader, "IP"));
        Version result2;
        if (Version.TryParse(DatabaseFunctions.GetString(reader, "OSVersion"), out result2))
          agentInfo3.set_OsVersion(result2);
        if (agentInfo1 == null || agentInfo1.get_AgentId() != agentInfo3.get_AgentId())
        {
          agentInfoList.Add(agentInfo3);
          agentInfo1 = agentInfo3;
        }
        AgentPluginInfo agentPluginInfo1 = new AgentPluginInfo();
        agentPluginInfo1.set_PluginId(DatabaseFunctions.GetString(reader, "PluginId", (string) null));
        AgentPluginInfo agentPluginInfo2 = agentPluginInfo1;
        if (agentPluginInfo2.get_PluginId() != null)
        {
          agentPluginInfo2.set_Status(DatabaseFunctions.GetInt32(reader, "Status"));
          agentPluginInfo2.set_StatusMessage(DatabaseFunctions.GetString(reader, "StatusMessage"));
          agentInfo1.AddPlugin(agentPluginInfo2);
        }
      }
      return agentInfoList;
    }

    public bool IsUniqueAgentName(string agentName)
    {
      using (InformationServiceConnection systemConnectionV3 = InformationServiceConnectionProvider.CreateSystemConnectionV3())
      {
        using (InformationServiceCommand command = systemConnectionV3.CreateCommand())
        {
          ((DbCommand) command).CommandText = "SELECT COUNT(AgentId) AS Cnt FROM Orion.AgentManagement.Agent WHERE Name = @Name";
          command.get_Parameters().AddWithValue("Name", (object) agentName);
          using (InformationServiceDataReader serviceDataReader = command.ExecuteReader())
            return !((DbDataReader) serviceDataReader).Read() || (int) ((DbDataReader) serviceDataReader)[0] == 0;
        }
      }
    }
  }
}
