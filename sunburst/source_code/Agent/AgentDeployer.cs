// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.AgentDeployer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common.Agent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class AgentDeployer
  {
    private static readonly Log log = new Log();
    private readonly IAgentInfoDAL agentInfoDal;

    public AgentDeployer()
      : this((IAgentInfoDAL) new AgentInfoDAL())
    {
    }

    internal AgentDeployer(IAgentInfoDAL agentInfoDal)
    {
      this.agentInfoDal = agentInfoDal;
    }

    public int StartDeployingAgent(AgentDeploymentSettings settings)
    {
      AgentDeployer.log.Debug((object) "StartDeployingAgent entered");
      AgentDeploymentWatcher instance = AgentDeploymentWatcher.GetInstance(this.agentInfoDal);
      instance.Start();
      int agentId = this.DeployAgent(settings);
      instance.AddOrUpdateDeploymentInfo(AgentDeploymentInfo.Calculate(new AgentManager(this.agentInfoDal).GetAgentInfo(agentId), (IEnumerable<string>) settings.get_RequiredPlugins(), (string) null));
      this.DeployMissingPlugins(agentId, (IEnumerable<string>) settings.get_RequiredPlugins());
      return agentId;
    }

    public void StartDeployingPlugins(
      int agentId,
      IEnumerable<string> requiredPlugins,
      Action<AgentDeploymentStatus> onFinishedCallback = null)
    {
      AgentDeployer.log.Debug((object) "StartDeployingPlugins entered");
      AgentDeploymentInfo deploymentInfo = AgentDeploymentInfo.Calculate(new AgentManager(this.agentInfoDal).GetAgentInfo(agentId), requiredPlugins, (string) null);
      deploymentInfo.set_StatusInfo(new AgentDeploymentStatusInfo(agentId, (AgentDeploymentStatus) 0));
      AgentDeploymentWatcher instance = AgentDeploymentWatcher.GetInstance(this.agentInfoDal);
      instance.AddOrUpdateDeploymentInfo(deploymentInfo);
      if (onFinishedCallback != null)
        instance.SetOnFinishedCallback(agentId, onFinishedCallback);
      this.DeployMissingPlugins(agentId, requiredPlugins);
    }

    private void DeployMissingPlugins(int agentId, IEnumerable<string> requiredPlugins)
    {
      AgentDeployer.log.DebugFormat("DeployMissingPlugins started, AgentId:{0}, RequiredPlugins:{1}", (object) agentId, (object) string.Join(",", requiredPlugins));
      AgentManager agentManager = new AgentManager(this.agentInfoDal);
      AgentInfo agentInfo = agentManager.GetAgentInfo(agentId);
      if (agentInfo == null)
        throw new ArgumentException(string.Format("Agent with Id:{0} not found", (object) agentId));
      foreach (string pluginId in agentInfo.get_Plugins().Where<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (p => p.get_Status() == 5 || p.get_Status() == 12)).Select<AgentPluginInfo, string>((Func<AgentPluginInfo, string>) (p => p.get_PluginId())).ToArray<string>())
      {
        AgentDeployer.log.DebugFormat("DeployMissingPlugins - Redeploying plugin {0}", (object) pluginId);
        agentManager.StartRedeployingPlugin(agentId, pluginId);
      }
      foreach (string pluginId in requiredPlugins.Where<string>((Func<string, bool>) (requiredPluginId => agentInfo.get_Plugins().All<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (installedPlugin => installedPlugin.get_PluginId() != requiredPluginId)))))
      {
        AgentDeployer.log.DebugFormat("DeployMissingPlugins - Deploying plugin {0}", (object) pluginId);
        agentManager.StartDeployingPlugin(agentId, pluginId);
      }
      agentInfo = agentManager.GetAgentInfo(agentId);
      if (agentInfo == null || agentInfo.get_AgentStatus() != 8 && !agentInfo.get_Plugins().Any<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (p => p.get_Status() == 3)) && !agentInfo.get_Plugins().Any<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (p => p.get_Status() == 13)))
        return;
      AgentDeployer.log.Debug((object) "DeployMissingPlugins - Approve update");
      agentManager.ApproveUpdate(agentId);
    }

    private int DeployAgent(AgentDeploymentSettings settings)
    {
      AgentDeployer.log.Debug((object) "DeployAgent started");
      AgentManager agentManager = new AgentManager(this.agentInfoDal);
      int agentId = agentManager.StartDeployingAgent(settings);
      agentManager.UpdateAgentNodeId(agentId, settings.get_NodeId());
      AgentDeployer.log.DebugFormat("DeployAgent finished, AgentId:{0}", (object) agentId);
      return agentId;
    }
  }
}
