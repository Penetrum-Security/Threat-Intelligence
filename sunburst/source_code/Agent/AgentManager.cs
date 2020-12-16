// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.AgentManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Agent;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Collections.Generic;
using System.Net;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class AgentManager
  {
    private static readonly Log log = new Log();
    private readonly IAgentInfoDAL _agentInfoDal;
    private const string AgentEntityName = "Orion.AgentManagement.Agent";

    public AgentManager(IAgentInfoDAL agentInfoDal)
    {
      this._agentInfoDal = agentInfoDal;
    }

    public AgentInfo GetAgentInfo(int agentId)
    {
      return this._agentInfoDal.GetAgentInfo(agentId);
    }

    public AgentInfo GetAgentInfoByNodeId(int nodeId)
    {
      return this._agentInfoDal.GetAgentInfoByNode(nodeId);
    }

    public AgentInfo DetectAgent(string ipAddress, string hostname)
    {
      if (string.IsNullOrWhiteSpace(ipAddress) && string.IsNullOrWhiteSpace(hostname))
        throw new ArgumentException("ipAddress or hostname must be specified");
      if (hostname.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
        hostname = Dns.GetHostName();
      return this._agentInfoDal.GetAgentInfoByIpOrHostname(ipAddress, hostname);
    }

    public int StartDeployingAgent(AgentDeploymentSettings settings)
    {
      if (settings == null)
        throw new ArgumentNullException(nameof (settings));
      if (string.IsNullOrWhiteSpace(settings.get_IpAddress()) && string.IsNullOrWhiteSpace(settings.get_Hostname()))
        throw new ArgumentException("ipAddress or hostname must be specified");
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = connectionProxyFactory.Create())
        {
          string str = !string.IsNullOrEmpty(settings.get_Hostname()) ? settings.get_Hostname() : settings.get_IpAddress();
          string agentName = str;
          int num = 1;
          while (!this._agentInfoDal.IsUniqueAgentName(agentName))
            agentName = string.Format("{0}-{1}", (object) str, (object) ++num);
          int agentId = (int) iinformationServiceProxy2.Invoke<int>("Orion.AgentManagement.Agent", "Deploy", new object[12]
          {
            (object) settings.get_EngineId(),
            (object) agentName,
            (object) str,
            (object) settings.get_IpAddress(),
            (object) settings.get_Credentials().get_Username(),
            (object) settings.get_Credentials().get_Password(),
            (object) (settings.get_Credentials().get_AdditionalUsername() ?? ""),
            (object) (settings.get_Credentials().get_AdditionalPassword() ?? ""),
            (object) settings.get_Credentials().get_PasswordIsPrivateKey(),
            (object) (settings.get_Credentials().get_PrivateKeyPassword() ?? ""),
            (object) 0,
            (object) (settings.get_InstallPackageId() ?? "")
          });
          this.UpdateAgentNodeId(agentId, 0);
          return agentId;
        }
      }
    }

    public void StartDeployingPlugin(int agentId, string pluginId)
    {
      if (string.IsNullOrEmpty(pluginId))
        throw new ArgumentNullException(nameof (pluginId), "Plugin Id must be specified.");
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = connectionProxyFactory.Create())
          iinformationServiceProxy2.Invoke<object>("Orion.AgentManagement.Agent", "DeployPlugin", new object[2]
          {
            (object) agentId,
            (object) pluginId
          });
      }
    }

    public void StartRedeployingPlugin(int agentId, string pluginId)
    {
      if (string.IsNullOrEmpty(pluginId))
        throw new ArgumentNullException(nameof (pluginId), "Plugin Id must be specified.");
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = connectionProxyFactory.Create())
          iinformationServiceProxy2.Invoke<object>("Orion.AgentManagement.Agent", "RedeployPlugin", new object[2]
          {
            (object) agentId,
            (object) pluginId
          });
      }
    }

    public void ApproveUpdate(int agentId)
    {
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = connectionProxyFactory.Create())
          iinformationServiceProxy2.Invoke<object>("Orion.AgentManagement.Agent", nameof (ApproveUpdate), new object[1]
          {
            (object) agentId
          });
      }
    }

    private void UpdateAgentNodeId(int agentId, int nodeId, IInformationServiceProxy2 proxy)
    {
      AgentInfo agentInfo = this._agentInfoDal.GetAgentInfo(agentId);
      if (agentInfo != null)
        ((IInformationServiceProxy) proxy).Update(agentInfo.get_Uri(), (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "NodeId",
            (object) nodeId
          }
        });
      else
        AgentManager.log.WarnFormat("Agent Id={0} not found.", (object) agentId);
    }

    public void UpdateAgentNodeId(int agentId, int nodeId)
    {
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 proxy = connectionProxyFactory.Create())
          this.UpdateAgentNodeId(agentId, nodeId, proxy);
      }
    }

    public void ResetAgentNodeId(int nodeId)
    {
      using (SwisConnectionProxyFactory connectionProxyFactory = new SwisConnectionProxyFactory())
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = connectionProxyFactory.Create())
        {
          AgentInfo agentInfoByNode = this._agentInfoDal.GetAgentInfoByNode(nodeId);
          if (agentInfoByNode != null)
            ((IInformationServiceProxy) iinformationServiceProxy2).Update(agentInfoByNode.get_Uri(), (IDictionary<string, object>) new Dictionary<string, object>()
            {
              {
                "NodeId",
                (object) nodeId
              }
            });
          else
            AgentManager.log.WarnFormat("Agent for NodeId={0} not found", (object) nodeId);
        }
      }
    }
  }
}
