// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.CoreBusinessLayerService
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Collector.Contract;
using SolarWinds.Common.Net;
using SolarWinds.Common.Snmp;
using SolarWinds.Common.Utility;
using SolarWinds.InformationService.Contract2;
using SolarWinds.InformationService.Linq;
using SolarWinds.InformationService.Linq.Plugins;
using SolarWinds.InformationService.Linq.Plugins.Core.Orion;
using SolarWinds.JobEngine;
using SolarWinds.JobEngine.Security;
using SolarWinds.Licensing.Framework;
using SolarWinds.Licensing.Framework.Interfaces;
using SolarWinds.Logging;
using SolarWinds.Net.SNMP;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Actions;
using SolarWinds.Orion.Core.Actions.DAL;
using SolarWinds.Orion.Core.Actions.Runners;
using SolarWinds.Orion.Core.Alerting;
using SolarWinds.Orion.Core.Alerting.DAL;
using SolarWinds.Orion.Core.Alerting.Migration;
using SolarWinds.Orion.Core.Alerting.Migration.Plugins;
using SolarWinds.Orion.Core.Alerting.Models;
using SolarWinds.Orion.Core.Alerting.Plugins.Conditions.Sql;
using SolarWinds.Orion.Core.BusinessLayer.Agent;
using SolarWinds.Orion.Core.BusinessLayer.BL;
using SolarWinds.Orion.Core.BusinessLayer.CentralizedSettings;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.BusinessLayer.Discovery;
using SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache;
using SolarWinds.Orion.Core.BusinessLayer.Instrumentation;
using SolarWinds.Orion.Core.BusinessLayer.OneTimeJobs;
using SolarWinds.Orion.Core.BusinessLayer.Thresholds;
using SolarWinds.Orion.Core.BusinessLayer.TimeSeries;
using SolarWinds.Orion.Core.BusinessLayer.TraceRoute;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Agent;
using SolarWinds.Orion.Core.Common.Alerting;
using SolarWinds.Orion.Core.Common.BusinessLayer;
using SolarWinds.Orion.Core.Common.Catalogs;
using SolarWinds.Orion.Core.Common.CentralizedSettings;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Enums;
using SolarWinds.Orion.Core.Common.ExpressionEvaluator;
using SolarWinds.Orion.Core.Common.Extensions;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Instrumentation;
using SolarWinds.Orion.Core.Common.Instrumentation.Keys;
using SolarWinds.Orion.Core.Common.JobEngine;
using SolarWinds.Orion.Core.Common.Licensing;
using SolarWinds.Orion.Core.Common.MacroParsing;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Models.Alerts;
using SolarWinds.Orion.Core.Common.Models.Mib;
using SolarWinds.Orion.Core.Common.Models.Technology;
using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.ModuleManager;
using SolarWinds.Orion.Core.Common.Notification;
using SolarWinds.Orion.Core.Common.PackageManager;
using SolarWinds.Orion.Core.Common.Proxy.Audit;
using SolarWinds.Orion.Core.Common.Proxy.BusinessLayer;
using SolarWinds.Orion.Core.Common.Settings;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.Core.Discovery;
using SolarWinds.Orion.Core.Discovery.BL;
using SolarWinds.Orion.Core.Discovery.DAL;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using SolarWinds.Orion.Core.Jobs2;
using SolarWinds.Orion.Core.Models;
using SolarWinds.Orion.Core.Models.Actions;
using SolarWinds.Orion.Core.Models.Actions.Contexts;
using SolarWinds.Orion.Core.Models.Actions.Contracts;
using SolarWinds.Orion.Core.Models.Alerting;
using SolarWinds.Orion.Core.Models.Credentials;
using SolarWinds.Orion.Core.Models.DiscoveredObjects;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Enums;
using SolarWinds.Orion.Core.Models.Events;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Core.Models.MacroParsing;
using SolarWinds.Orion.Core.Models.OldDiscoveryModels;
using SolarWinds.Orion.Core.Models.Technology;
using SolarWinds.Orion.Core.Models.WebIntegration;
using SolarWinds.Orion.Core.Pollers.Node.ResponseTime;
using SolarWinds.Orion.Core.Pollers.Node.WMI;
using SolarWinds.Orion.Core.SharedCredentials;
using SolarWinds.Orion.Core.SharedCredentials.Credentials;
using SolarWinds.Orion.Core.Strings;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Contract.Models;
using SolarWinds.Orion.Discovery.Framework;
using SolarWinds.Orion.Discovery.Framework.Pluggability;
using SolarWinds.Orion.MacroProcessor;
using SolarWinds.Orion.Pollers.Framework.SNMP;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.ServiceDirectory.Wcf;
using SolarWinds.Orion.Swis.PubSub;
using SolarWinds.Orion.Web.Integration;
using SolarWinds.Orion.Web.Integration.Common.Models;
using SolarWinds.Orion.Web.Integration.Maintenance;
using SolarWinds.Orion.Web.Integration.SupportCases;
using SolarWinds.Serialization.Json;
using SolarWinds.ServiceDirectory.Client.Contract;
using SolarWinds.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  [ServiceBehavior(AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = false, InstanceContextMode = InstanceContextMode.Single, Name = "CoreServiceEngine")]
  [ErrorBehavior(typeof (CoreErrorHandler))]
  [SolarWinds.Orion.ServiceDirectory.Wcf.ServiceDirectory("Core.BusinessLayer")]
  public class CoreBusinessLayerService : ICoreBusinessLayer, IDisposable, IOneTimeAgentDiscoveryJobFactory, IServiceDirectoryIntegration
  {
    private readonly Guid ItemTypeCertificateUpdateRequired = new Guid("{4E9EB71A-3A11-468E-A672-1E3E440E4F89}");
    private Lazy<IActionRunner> actionRunner = new Lazy<IActionRunner>(new Func<IActionRunner>(CoreBusinessLayerService.CreateActionRunner));
    private readonly ActionMethodInvoker _actionMethodInvoker = new ActionMethodInvoker();
    private AuditingDAL auditingDal = new AuditingDAL();
    private DiscoveryLogic discoveryLogic = new DiscoveryLogic();
    private readonly object _syncRoot = new object();
    private readonly Dictionary<string, string> _elementCountQueries = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private readonly MibDAL mibDAL = new MibDAL();
    private NodeAssignmentHelper nodeHelper = new NodeAssignmentHelper();
    private readonly ManualResetEvent shutdownEvent = new ManualResetEvent(false);
    private readonly AuditingPluginManager _auditPluginManager = new AuditingPluginManager();
    private IJobFactory _jobFactory;
    private IPersistentDiscoveryCache _persistentDiscoveryCache;
    private static readonly Dictionary<string, Action<PollingEngineStatus, object>> statusParsers;
    private static readonly List<NodeSubType> WmiCompatibleNodeSubTypes;
    private const string SourceInstanceUriProperty = "SourceInstanceUri";
    private const string UriProperty = "Uri";
    private ExpirableCache<string, IEnumerable<MaintenanceStatus>> _maintenanceInfoCache;
    private ExpirableCache<string, IEnumerable<SupportCase>> _supportCasesCache;
    private ExpirableCache<string, LicenseAndManagementInfo> _LAMInfoCache;
    private static readonly IReadOnlyList<string> dataColumns;
    public static readonly Version CoreBusinessLayerServiceVersion;
    private static readonly Log log;
    private readonly CoreBusinessLayerPlugin parent;
    private readonly bool _areInterfacesSupported;
    private readonly IAgentInfoDAL _agentInfoDal;
    private readonly INodeBLDAL _nodeBlDal;
    private readonly ISettingsDAL _settingsDal;
    private readonly IOneTimeJobManager _oneTimeJobManager;
    private readonly IEngineDAL _engineDal;
    private readonly IEngineIdentityProvider _engineIdentityProvider;
    private readonly string _serviceLogicalInstanceId;
    private readonly IServiceProvider _serviceContainer;

    public void RemoveCertificateMaintenanceNotification()
    {
      this.DeleteNotificationItemById(this.ItemTypeCertificateUpdateRequired);
    }

    private static IActionRunner CreateActionRunner()
    {
      return (IActionRunner) new ActionRunner((IActionPluginsProvider) new MEFActionPluginsProvider(false), CoreBusinessLayerService.CreateProxy());
    }

    public ActionResult ExecuteAction(
      ActionDefinition actionDefinition,
      ActionContextBase context)
    {
      return this.actionRunner.Value.Execute(actionDefinition, context);
    }

    public string InvokeActionMethod(string actionTypeID, string methodName, string args)
    {
      return this._actionMethodInvoker.InvokeActionMethod(actionTypeID, methodName, args);
    }

    private static IInformationServiceProxyCreator CreateProxy()
    {
      return (IInformationServiceProxyCreator) new SwisConnectionProxyCreator((Func<SwisConnectionProxy>) (() => new SwisConnectionProxyFactory(true).CreateConnection()));
    }

    public string SimulateAction(ActionDefinition actionDefinition, ActionContextBase context)
    {
      return this.actionRunner.Value.Simulate(actionDefinition, context);
    }

    public int DeployAgent(AgentDeploymentSettings settings)
    {
      try
      {
        CoreBusinessLayerService.log.InfoFormat("DeployAgent on {0}-{1} called", (object) settings.get_IpAddress(), (object) settings.get_Hostname());
        return new AgentDeployer().StartDeployingAgent(settings);
      }
      catch (Exception ex)
      {
        throw MessageUtilities.NewFaultException<CoreFaultContract>(ex);
      }
    }

    public void DeployAgentPlugins(int agentId, IEnumerable<string> requiredPlugins)
    {
      this.DeployAgentPlugins(agentId, requiredPlugins, (Action<AgentDeploymentStatus>) null);
    }

    protected void DeployAgentPlugins(
      int agentId,
      IEnumerable<string> requiredPlugins,
      Action<AgentDeploymentStatus> onFinishedCallback)
    {
      CoreBusinessLayerService.log.InfoFormat("DeployAgentPlugins called, agentId:{0}, requiredPlugins:{1}", (object) agentId, (object) string.Join(",", requiredPlugins));
      new AgentDeployer().StartDeployingPlugins(agentId, requiredPlugins, onFinishedCallback);
    }

    public string[] GetRequiredAgentDiscoveryPlugins()
    {
      return DiscoveryHelper.GetAgentDiscoveryPluginIds();
    }

    public void DeployAgentDiscoveryPlugins(int agentId)
    {
      this.DeployAgentDiscoveryPluginsAsync(agentId);
    }

    public Task<AgentDeploymentStatus> DeployAgentDiscoveryPluginsAsync(
      int agentId)
    {
      TaskCompletionSource<AgentDeploymentStatus> taskSource = new TaskCompletionSource<AgentDeploymentStatus>();
      string[] discoveryPlugins = this.GetRequiredAgentDiscoveryPlugins();
      this.DeployAgentPlugins(agentId, (IEnumerable<string>) discoveryPlugins, (Action<AgentDeploymentStatus>) (status => taskSource.TrySetResult(status)));
      return taskSource.Task;
    }

    public AgentInfo GetAgentInfo(int agentId)
    {
      return new AgentManager(this._agentInfoDal).GetAgentInfo(agentId);
    }

    public AgentInfo GetAgentInfoByNodeId(int nodeId)
    {
      return new AgentManager(this._agentInfoDal).GetAgentInfoByNodeId(nodeId);
    }

    public AgentInfo DetectAgent(string ipAddress, string hostname)
    {
      return new AgentManager(this._agentInfoDal).DetectAgent(ipAddress, hostname);
    }

    public AgentDeploymentInfo GetAgentDeploymentInfo(int agentId)
    {
      return AgentDeploymentWatcher.GetInstance(this._agentInfoDal).GetAgentDeploymentInfo(agentId);
    }

    public void UpdateAgentNodeId(int agentId, int nodeId)
    {
      new AgentManager(this._agentInfoDal).UpdateAgentNodeId(agentId, nodeId);
    }

    public void ResetAgentNodeId(int nodeId)
    {
      new AgentManager(this._agentInfoDal).ResetAgentNodeId(nodeId);
    }

    private void UpdateNotification()
    {
      CoreBusinessLayerService.log.Debug((object) "Agent deployed, update notification item");
    }

    public List<KeyValuePair<string, string>> GetAlertList()
    {
      return AlertDAL.GetAlertList();
    }

    [Obsolete("Old alerting will be removed. Use GetAlertList() method instead.")]
    public List<KeyValuePair<string, string>> GetAlertNames(bool includeBasic)
    {
      return AlertDAL.GetAlertList(includeBasic);
    }

    public List<NetObjectType> GetAlertNetObjectTypes()
    {
      return (List<NetObjectType>) ModuleAlertsMap.get_NetObjectTypes().Items;
    }

    [Obsolete("Method does not return V2 alerts.")]
    public DataTable GetSortableAlertTable(
      string netObject,
      string deviceType,
      string alertID,
      string orderByClause,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs,
      bool includeBasic)
    {
      return AlertDAL.GetSortableAlertTable(netObject, deviceType, alertID, orderByClause, maxRecords, showAcknowledged, limitationIDs, includeBasic);
    }

    public List<ActiveAlertDetailed> GetAlertTableByDate(
      DateTime date,
      int? lastAlertHistoryId,
      List<int> limitationIDs,
      bool showAcknowledged)
    {
      return new ActiveAlertDAL().GetAlertTableByDate(date.ToLocalTime(), lastAlertHistoryId, limitationIDs);
    }

    public int GetLastAlertHistoryId()
    {
      return new AlertHistoryDAL().GetLastHystoryId();
    }

    [Obsolete("Method does not return V2 alerts.")]
    public DataTable GetPageableAlerts(
      List<int> limitationIDs,
      string period,
      int fromRow,
      int toRow,
      string type,
      string alertId,
      bool showAcknAlerts)
    {
      return AlertDAL.GetPageableAlerts(limitationIDs, period, fromRow, toRow, type, alertId, showAcknAlerts);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public DataTable GetAlertTable(
      string netObject,
      string deviceType,
      string alertID,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs)
    {
      return AlertDAL.GetAlertTable(netObject, deviceType, alertID, maxRecords, showAcknowledged, limitationIDs);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public DataTable GetAlerts(
      string netObject,
      string deviceType,
      string alertID,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs,
      bool includeBasic)
    {
      return AlertDAL.GetAlertTable(netObject, deviceType, alertID, maxRecords, showAcknowledged, limitationIDs, includeBasic);
    }

    [Obsolete("Old alerting will be removed")]
    public void AcknowledgeAlertsAction(List<string> alertKeys, string accountID)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID);
      this.FireUpdateNotification((IEnumerable<string>) alertKeys, (AlertUpdatedNotificationType) 1, accountID);
    }

    [Obsolete("Old alerting will be removed")]
    public void AcknowledgeAlertsFromAlertManager(List<string> alertKeys, string accountID)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, (AlertAcknowledgeType) 2, (string) null);
      this.FireUpdateNotification((IEnumerable<string>) alertKeys, (AlertUpdatedNotificationType) 1, accountID);
    }

    [Obsolete("Old alerting will be removed")]
    public void UnacknowledgeAlertsFromAlertManager(List<string> alertKeys, string accountID)
    {
      AlertDAL.UnacknowledgeAlertsAction(alertKeys, accountID, (AlertAcknowledgeType) 2);
      this.FireUpdateNotification((IEnumerable<string>) alertKeys, (AlertUpdatedNotificationType) 1, accountID);
    }

    [Obsolete("Old alerting will be removed")]
    public void AcknowledgeAlerts(List<string> alertKeys, string accountID, bool viaEmail)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, viaEmail);
      this.FireUpdateNotification((IEnumerable<string>) alertKeys, (AlertUpdatedNotificationType) 1, accountID);
    }

    [Obsolete("Old alerting will be removed")]
    public void AcknowledgeAlertsWithNotes(List<string> alertKeys, string accountID, string notes)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, false, notes);
      this.FireUpdateNotification((IEnumerable<string>) alertKeys, (AlertUpdatedNotificationType) 3, accountID);
    }

    public int GetAlertObjectId(string alertkey)
    {
      return AlertDAL.GetAlertObjectId(alertkey);
    }

    public int AcknowledgeAlertsWithMethod(
      List<string> alertKeys,
      string accountId,
      string notes,
      string method)
    {
      List<int> source = new List<int>();
      foreach (string alertKey in alertKeys)
      {
        string alertDefId;
        string activeObject;
        string objectType;
        if (AlertsHelper.TryParseAlertKey(alertKey.Replace("swis://", "swis//"), ref alertDefId, ref activeObject, ref objectType))
        {
          activeObject = activeObject.Replace("swis//", "swis://");
          if (!activeObject.StartsWith("swis://"))
          {
            CoreBusinessLayerService.log.WarnFormat("Unable to acknowledge alert {0} for net object {1}. Old alerts aren't supported.", (object) alertKey, (object) activeObject);
          }
          else
          {
            int alertObjectId = this.GetAlertObjectId(alertDefId, activeObject, objectType);
            if (alertObjectId > 0)
              source.Add(alertObjectId);
          }
        }
      }
      int num = 0;
      if (source.Any<int>())
        num += this.AcknowledgeAlertsWithMethodV2((IEnumerable<int>) source, accountId, notes, DateTime.UtcNow, method);
      return num;
    }

    public int AcknowledgeAlertsV2(
      IEnumerable<int> alertObjectIds,
      string accountId,
      string notes,
      DateTime acknowledgeDateTime)
    {
      return this.AcknowledgeAlertsWithMethodV2(alertObjectIds, accountId, notes, acknowledgeDateTime, (string) null);
    }

    public int AcknowledgeAlertsWithMethodV2(
      IEnumerable<int> alertObjectIds,
      string accountId,
      string notes,
      DateTime acknowledgeDateTime,
      string method)
    {
      ActiveAlertDAL activeAlertDal = new ActiveAlertDAL();
      IEnumerable<int> alertObjectIds1 = activeAlertDal.LimitAlertAckStateUpdateCandidates(alertObjectIds, true);
      List<INotification> inotificationList = new List<INotification>();
      inotificationList.AddRange((IEnumerable<INotification>) activeAlertDal.GetAlertUpdatedNotificationPropertiesByAlertObjectIds(alertObjectIds1, accountId, notes, acknowledgeDateTime, true, method));
      DataTable byAlertObjectIds = activeAlertDal.GetAlertResetOrUpdateNotificationPropertiesTableByAlertObjectIds(alertObjectIds);
      foreach (int alertObjectId in alertObjectIds)
      {
        DataRow[] dataRowArray = byAlertObjectIds.Select("AlertObjectID=" + (object) alertObjectId);
        PropertyBag propertyBag = new PropertyBag();
        if (dataRowArray.Length != 0)
        {
          ((Dictionary<string, object>) propertyBag).Add("Acknowledged", dataRowArray[0]["Acknowledged"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["Acknowledged"]) : (object) "False");
          ((Dictionary<string, object>) propertyBag).Add("AcknowledgedBy", dataRowArray[0]["AcknowledgedBy"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["AcknowledgedBy"]) : (object) string.Empty);
          ((Dictionary<string, object>) propertyBag).Add("AcknowledgedDateTime", dataRowArray[0]["AcknowledgedDateTime"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["AcknowledgedDateTime"]) : (object) string.Empty);
          ((Dictionary<string, object>) propertyBag).Add("AlertNote", dataRowArray[0]["AlertNote"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["AlertNote"]) : (object) string.Empty);
        }
        inotificationList.Add((INotification) new CommonNotification(IndicationHelper.GetIndicationType((IndicationType) 2), accountId, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "AlertObjectID",
            (object) alertObjectId
          },
          {
            "Acknowledged",
            (object) "True"
          },
          {
            "AcknowledgedBy",
            (object) accountId
          },
          {
            "AcknowledgedDateTime",
            (object) acknowledgeDateTime
          },
          {
            "AlertNote",
            (object) notes
          },
          {
            "PreviousProperties",
            (object) propertyBag
          },
          {
            "InstanceType",
            (object) "Orion.AlertActive"
          }
        }));
      }
      int num = activeAlertDal.AcknowledgeActiveAlerts(alertObjectIds1, accountId, notes, acknowledgeDateTime);
      if (num <= 0)
        return num;
      PublisherClient.get_Instance().Publish((IReadOnlyCollection<INotification>) inotificationList);
      return num;
    }

    public AlertAcknowledgeResult AcknowledgeAlert(
      string alertId,
      string netObjectId,
      string objectType,
      string accountId,
      string notes,
      string method)
    {
      AlertAcknowledgeResult acknowledgeResult = (AlertAcknowledgeResult) 0;
      if (!netObjectId.StartsWith("swis://"))
      {
        CoreBusinessLayerService.log.WarnFormat("Unable to acknowledge alert {0} for net object {1}. Old alerts aren't supported.", (object) alertId, (object) netObjectId);
      }
      else
      {
        int alertObjectId = this.GetAlertObjectId(alertId, netObjectId, objectType);
        if (alertObjectId > 0)
          acknowledgeResult = this.AcknowledgeAlertsV2((IEnumerable<int>) new List<int>()
          {
            alertObjectId
          }, accountId, notes, DateTime.UtcNow) == 1 ? (AlertAcknowledgeResult) 0 : (AlertAcknowledgeResult) -1;
      }
      return acknowledgeResult;
    }

    [Obsolete("Old alerting will be removed", true)]
    public void ClearTriggeredAlerts(List<string> alertKeys)
    {
      this.FireResetNotification((IEnumerable<string>) alertKeys, (string) IndicationConstants.SystemAccountId);
      AlertDAL.ClearTriggeredAlert(alertKeys);
    }

    public void ClearTriggeredAlertsV2(IEnumerable<int> alertObjectIds, string accountId)
    {
      ActiveAlertDAL activeAlertDal = new ActiveAlertDAL();
      List<INotification> inotificationList = new List<INotification>();
      IEnumerable<AlertClearedNotificationProperties> byAlertObjectIds = activeAlertDal.GetAlertClearedNotificationPropertiesByAlertObjectIds(alertObjectIds);
      string str = !string.IsNullOrEmpty(accountId) ? accountId : (string) IndicationConstants.SystemAccountId;
      using (IEnumerator<AlertClearedNotificationProperties> enumerator = byAlertObjectIds.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          AlertClearedNotificationProperties current = enumerator.Current;
          inotificationList.Add((INotification) new AlertClearedNotification(str, (AlertNotificationProperties) current));
        }
      }
      foreach (int alertObjectId in alertObjectIds)
      {
        AlertDeletedNotificationProperties notificationProperties1 = new AlertDeletedNotificationProperties();
        notificationProperties1.set_AlertObjectId(alertObjectId);
        AlertDeletedNotificationProperties notificationProperties2 = notificationProperties1;
        CommonNotification commonNotification = new CommonNotification(IndicationHelper.GetIndicationType((IndicationType) 1), accountId, notificationProperties2.CreateDictionary());
        inotificationList.Add((INotification) commonNotification);
      }
      activeAlertDal.ClearTriggeredActiveAlerts(alertObjectIds, accountId);
      PublisherClient.get_Instance().Publish((IReadOnlyCollection<INotification>) inotificationList);
    }

    [Obsolete("Old alerting will be removed")]
    public int EnableAdvancedAlert(Guid alertDefID, bool enable)
    {
      return AlertDAL.EnableAdvancedAlert(alertDefID, enable);
    }

    [Obsolete("Old alerting will be removed")]
    public int EnableAdvancedAlerts(List<string> alertDefIDs, bool enable, bool enableAll)
    {
      return AlertDAL.EnableAdvancedAlerts(alertDefIDs, enable, enableAll);
    }

    [Obsolete("Old alerting will be removed")]
    public int RemoveAdvancedAlert(Guid alertDefID)
    {
      return AlertDAL.RemoveAdvancedAlert(alertDefID);
    }

    public int RemoveAdvancedAlerts(List<string> alertDefIDs, bool deleteAll)
    {
      return AlertDAL.RemoveAdvancedAlerts(alertDefIDs, deleteAll);
    }

    [Obsolete("Old alerting will be removed")]
    public int UpdateAlertDef(
      Guid alertDefID,
      string alertName,
      string alertDescr,
      bool enabled,
      int evInterval,
      string dow,
      DateTime startTime,
      DateTime endTime,
      bool ignoreTimeout)
    {
      return AlertDAL.UpdateAlertDef(alertDefID, alertName, alertDescr, enabled, evInterval, dow, startTime, endTime, ignoreTimeout);
    }

    [Obsolete("Old alerting will be removed")]
    public DataTable GetAdvancedAlerts()
    {
      return AlertDAL.GetAdvancedAlerts();
    }

    [Obsolete("Old alerting will be removed")]
    public DataTable GetPagebleAdvancedAlerts(
      string sortColumn,
      string sortDirection,
      int startRowNumber,
      int pageSize)
    {
      return AlertDAL.GetPagebleAdvancedAlerts(sortColumn, sortDirection, startRowNumber, pageSize);
    }

    public ActiveAlertPage GetPageableActiveAlerts(
      PageableActiveAlertRequest pageableRequest,
      ActiveAlertsRequest activeAlertsRequest = null)
    {
      return AlertDAL.GetPageableActiveAlerts(pageableRequest, activeAlertsRequest);
    }

    public ActiveAlertObjectPage GetPageableActiveAlertObjects(
      PageableActiveAlertObjectRequest request)
    {
      return new ActiveAlertDAL().GetPageableActiveAlertObjects(request);
    }

    public ActiveAlert GetActiveAlert(
      ActiveAlertUniqueidentifier activeAlertUniqIdentifier,
      IEnumerable<int> limitationIDs)
    {
      return new ActiveAlertDAL().GetActiveAlert(activeAlertUniqIdentifier.get_AlertObjectID(), limitationIDs, true);
    }

    public AlertHistoryPage GetActiveAlertHistory(
      int alertObjectId,
      PageableActiveAlertRequest request)
    {
      return new AlertHistoryDAL().GetActiveAlertHistory(alertObjectId, request);
    }

    [Obsolete("Old alerting will be removed")]
    public int AdvAlertsCount()
    {
      return AlertDAL.AdvAlertsCount();
    }

    [Obsolete("Old alerting will be removed")]
    public DataTable GetAdvancedAlert(Guid alertDefID)
    {
      return AlertDAL.GetAdvancedAlert(alertDefID);
    }

    private int GetAlertObjectId(string alertDefId, string activeObject, string objectType)
    {
      int objectId;
      this.GetAlertObjectIdAndAlertNote(alertDefId, activeObject, objectType, out objectId, out string _);
      return objectId;
    }

    private void GetAlertObjectIdAndAlertNote(
      string alertDefId,
      string activeObject,
      string objectType,
      out int objectId,
      out string note)
    {
      objectId = 0;
      note = string.Empty;
      string str = "SELECT AO.AlertObjectID, AO.AlertNote FROM Orion.AlertObjects AO \r\n                                    INNER JOIN Orion.AlertConfigurations AC ON AO.AlertID=AC.AlertID\r\n                                    WHERE EntityUri=@entityUri AND EntityType=@objectType AND AC.AlertRefID=@alertDefId";
      using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator()).Create())
      {
        DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query(str, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "entityUri",
            (object) activeObject
          },
          {
            nameof (objectType),
            (object) objectType
          },
          {
            nameof (alertDefId),
            (object) alertDefId
          }
        });
        if (dataTable.Rows.Count <= 0)
          return;
        objectId = dataTable.Rows[0]["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(dataTable.Rows[0]["AlertObjectID"]) : 0;
        note = dataTable.Rows[0]["AlertNote"] != DBNull.Value ? Convert.ToString(dataTable.Rows[0]["AlertNote"]) : string.Empty;
      }
    }

    public int AppendNoteToAlert(
      string alertDefId,
      string activeObject,
      string objectType,
      string note)
    {
      string accountId = (string) (AuditMessageInspector.UserContext ?? IndicationConstants.SystemAccountId);
      int num = 0;
      if (!activeObject.StartsWith("swis://"))
      {
        CoreBusinessLayerService.log.WarnFormat("Unable to append Note to alert {0}. Old alerts aren't supported.", (object) activeObject);
      }
      else
      {
        int objectId;
        string note1;
        this.GetAlertObjectIdAndAlertNote(alertDefId, activeObject, objectType, out objectId, out note1);
        if (objectId > 0)
          num = this.SetAlertNote(objectId, accountId, note, DateTime.UtcNow, note1) ? 1 : 0;
      }
      return num;
    }

    [Obsolete("Old alerting will be removed")]
    public int UpdateAdvancedAlertNote(
      string alerfDefID,
      string activeObject,
      string objectType,
      string notes)
    {
      int num = AlertDAL.UpdateAdvancedAlertNote(alerfDefID, activeObject, objectType, notes);
      this.FireUpdateNotification((IEnumerable<string>) new string[1]
      {
        AlertsHelper.GetAlertKey(alerfDefID, activeObject, objectType)
      }, (AlertUpdatedNotificationType) 2, (string) IndicationConstants.SystemAccountId);
      return num;
    }

    [Obsolete("Old alerting will be removed")]
    public AlertNotificationSettings GetAlertNotificationSettings(
      string alertDefID,
      string netObjectType,
      string alertName)
    {
      return AlertDAL.GetAlertNotificationSettings(alertDefID, netObjectType, alertName);
    }

    [Obsolete("Old alerting will be removed")]
    public void SetAlertNotificationSettings(string alertDefID, AlertNotificationSettings settings)
    {
      AlertDAL.SetAlertNotificationSettings(alertDefID, settings);
    }

    [Obsolete("Old alerting will be removed")]
    public AlertNotificationSettings GetBasicAlertNotificationSettings(
      int alertID,
      string netObjectType,
      int propertyID,
      string alertName)
    {
      return AlertDAL.GetBasicAlertNotificationSettings(alertID, netObjectType, propertyID, alertName);
    }

    public void SetBasicAlertNotificationSettings(int alertID, AlertNotificationSettings settings)
    {
      AlertDAL.SetBasicAlertNotificationSettings(alertID, settings);
    }

    [Obsolete("Old alerting will be removed", true)]
    private void FireUpdateNotification(
      IEnumerable<string> alertKeys,
      AlertUpdatedNotificationType type,
      string accountId)
    {
      this.FireNotification<AlertUpdatedNotificationProperties, AlertUpdatedNotification>(alertKeys, accountId, "Alert Update", (Action<AlertNotificationDetails, AlertUpdatedNotificationProperties>) ((notificationDetails, notificationProperties) =>
      {
        notificationProperties.set_Type(type);
        notificationProperties.set_Acknowledged(notificationDetails.get_Acknowledged());
        notificationProperties.set_AcknowledgedBy(notificationDetails.get_AcknowledgedBy());
        notificationProperties.set_AcknowledgedMethod(notificationDetails.get_AcknowledgedMethod());
        notificationProperties.set_Notes(notificationDetails.get_Notes());
      }));
    }

    [Obsolete("Old alerting will be removed", true)]
    private void FireResetNotification(IEnumerable<string> alertKeys, string accountId)
    {
      this.FireNotification<AlertResetNotificationProperties, AlertResetNotification>(alertKeys, accountId, "Alert Reset", (Action<AlertNotificationDetails, AlertResetNotificationProperties>) ((notificationDetails, notificationProperties) => notificationProperties.set_ResetTime(DateTime.UtcNow)));
    }

    [Obsolete("Old alerting will be removed")]
    private void FireNotification<TProperties, TNotification>(
      IEnumerable<string> alertKeys,
      string accountId,
      string name,
      Action<AlertNotificationDetails, TProperties> customNotificationPropertiesHandler)
      where TProperties : AlertNotificationProperties, new()
      where TNotification : AlertNotification
    {
      CoreBusinessLayerService.log.DebugFormat("Firing {0} notifications", (object) name);
      MacroParser macroParser = new MacroParser(new Action<string, int>(BusinessLayerOrionEvent.WriteEvent));
      SqlConnection connection;
      macroParser.set_MyDBConnection(connection = DatabaseFunctions.CreateConnection(false));
      using (connection)
      {
        foreach (string alertKey in alertKeys)
        {
          try
          {
            string alertDefID;
            string activeObject;
            string objectType;
            if (!AlertsHelper.TryParseAlertKey(alertKey, ref alertDefID, ref activeObject, ref objectType))
            {
              CoreBusinessLayerService.log.WarnFormat("Error firing notification for {0} because of invalid alert key {1}", (object) name, (object) alertKey);
            }
            else
            {
              AlertNotificationDetails detailsForNotification = AlertDAL.GetAlertDetailsForNotification(alertDefID, activeObject, objectType);
              if (detailsForNotification != null)
              {
                if (detailsForNotification.get_NotificationSettings().get_Enabled())
                {
                  macroParser.set_ObjectType(detailsForNotification.get_ObjectType());
                  macroParser.set_ActiveObject(detailsForNotification.get_ActiveObject());
                  macroParser.set_ObjectName(detailsForNotification.get_ObjectName());
                  macroParser.set_AlertID(new Guid(detailsForNotification.get_AlertDefinitionId()));
                  macroParser.set_AlertName(detailsForNotification.get_AlertName());
                  macroParser.set_AlertMessage(detailsForNotification.get_AlertMessage());
                  macroParser.set_AlertTriggerTime(detailsForNotification.get_TriggerTimeStamp().ToLocalTime());
                  macroParser.set_AlertTriggerCount(detailsForNotification.get_TriggerCount());
                  macroParser.set_Acknowledged(detailsForNotification.get_Acknowledged());
                  macroParser.set_AcknowledgedBy(detailsForNotification.get_AcknowledgedBy());
                  macroParser.set_AcknowledgedTime(detailsForNotification.get_AcknowledgedTime().ToLocalTime());
                  TProperties notificationProperties = new AlertNotificationSettingsProvider().GetAlertNotificationProperties<TProperties>((Func<string, string>) (s => macroParser.ParseMacros(s, false)), detailsForNotification.get_ActiveObject(), detailsForNotification.get_ObjectType(), detailsForNotification.get_ObjectName(), new Guid(detailsForNotification.get_AlertDefinitionId()), detailsForNotification.get_AlertName(), detailsForNotification.get_TriggerTimeStamp(), detailsForNotification.get_NotificationSettings());
                  customNotificationPropertiesHandler(detailsForNotification, notificationProperties);
                  PublisherClient.get_Instance().Publish((INotification) Activator.CreateInstance(typeof (TNotification), (object) accountId, (object) notificationProperties));
                }
              }
            }
          }
          catch (Exception ex)
          {
            CoreBusinessLayerService.log.ErrorFormat(string.Format("Error firing {0} notification", (object) name), (object) ex);
          }
        }
      }
    }

    public IEnumerable<AlertScopeItem> GetObjectsInAlertScope(
      int[] alertIds)
    {
      List<AlertScopeItem> alertScopeItemList1 = new List<AlertScopeItem>();
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      IAlertDefinitionsDAL ialertDefinitionsDal = AlertDefinitionsDAL.Create((IConditionTypeProvider) ConditionTypeProvider.Create((IInformationServiceProxyCreator) creator), (IInformationServiceProxyCreator) creator);
      foreach (int alertId in alertIds)
      {
        if (!ialertDefinitionsDal.Exist(alertId))
        {
          if (CoreBusinessLayerService.log.get_IsDebugEnabled())
            CoreBusinessLayerService.log.DebugFormat("There is no AlertDefinition with AlertId={0}", (object) alertId);
        }
        else
        {
          AlertDefinition alertDefinition = ialertDefinitionsDal.Get(alertId);
          if (alertDefinition.get_Trigger().get_Conditions()[0].get_Type() is IConditionEntityScope type)
          {
            IEnumerable<EntityInstance> list = (IEnumerable<EntityInstance>) type.GetScope(alertDefinition.get_Trigger().get_Conditions()[0].get_Condition(), alertDefinition.get_Trigger().get_Conditions()[0].get_ObjectType()).ToList<EntityInstance>();
            if (list.Any<EntityInstance>())
            {
              string str = (string) null;
              Entity entityByObjectType = alertDefinition.get_Trigger().get_Conditions()[0].get_Type().get_EntityProvider().GetEntityByObjectType(alertDefinition.get_Trigger().get_Conditions()[0].get_ObjectType());
              if (entityByObjectType != null)
                str = entityByObjectType.get_FullName();
              using (IEnumerator<EntityInstance> enumerator = list.GetEnumerator())
              {
                while (((IEnumerator) enumerator).MoveNext())
                {
                  EntityInstance current = enumerator.Current;
                  List<AlertScopeItem> alertScopeItemList2 = alertScopeItemList1;
                  AlertScopeItem alertScopeItem = new AlertScopeItem();
                  alertScopeItem.set_InstanceName(current.get_DisplayName());
                  alertScopeItem.set_ObjectId(current.get_Uri());
                  alertScopeItem.set_AlertId(alertId);
                  alertScopeItem.set_EntityType(str);
                  alertScopeItemList2.Add(alertScopeItem);
                }
              }
            }
          }
        }
      }
      return (IEnumerable<AlertScopeItem>) alertScopeItemList1;
    }

    public IEnumerable<AlertScopeItem> GetAllAlertsInObjectScopeWithParams(
      string entityType,
      string[] objectIds,
      bool loadAction,
      bool loadSchedules)
    {
      List<AlertScopeItem> resultItems = new List<AlertScopeItem>();
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      IEnumerable<AlertScopeItem> whichCanAffectObject = this.GetAlertsWhichCanAffectObject(entityType, (IInformationServiceProxyCreator) creator, loadAction, loadSchedules);
      List<AlertScopeItem> alertScopeItems = new List<AlertScopeItem>();
      int num = 0;
      using (IEnumerator<AlertScopeItem> enumerator = whichCanAffectObject.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          AlertScopeItem current = enumerator.Current;
          bool flag = false;
          if (current.get_ScopeQuery() == null || current.get_ScopeQuery().get_Params() == null || current.get_ScopeQuery().get_Params().Count == 0)
          {
            alertScopeItems.Add(current);
            flag = true;
          }
          else if (current.get_ScopeQuery().get_Params().Count + num < 2000)
          {
            alertScopeItems.Add(current);
            num += current.get_ScopeQuery().get_Params().Count;
            flag = true;
          }
          if (!flag)
          {
            this.GetAlertsForTheBulk(alertScopeItems, entityType, objectIds, creator, resultItems);
            alertScopeItems.Clear();
            alertScopeItems.Add(current);
            num = current.get_ScopeQuery().get_Params().Count;
          }
        }
      }
      this.GetAlertsForTheBulk(alertScopeItems, entityType, objectIds, creator, resultItems);
      return (IEnumerable<AlertScopeItem>) resultItems;
    }

    public IEnumerable<AlertScopeItem> GetAllAlertsInObjectScope(
      string entityType,
      string[] objectIds)
    {
      return this.GetAllAlertsInObjectScopeWithParams(entityType, objectIds, true, true);
    }

    private void GetAlertsForTheBulk(
      List<AlertScopeItem> alertScopeItems,
      string entityType,
      string[] objectIds,
      ISwisConnectionProxyCreator swisCreator,
      List<AlertScopeItem> resultItems)
    {
      if (alertScopeItems == null)
        return;
      if (alertScopeItems.Count == 0)
        return;
      try
      {
        List<AlertScopeItem> alertsForQueries = this.GetAlertsForQueries((IEnumerable<AlertScopeItem>) alertScopeItems, entityType, objectIds, swisCreator);
        if (alertsForQueries == null || alertsForQueries.Count <= 0)
          return;
        using (List<AlertScopeItem>.Enumerator enumerator = alertsForQueries.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            AlertScopeItem current = enumerator.Current;
            resultItems.Add(current);
          }
        }
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.ErrorFormat("Error occurred during validating alert scope queries for {0} type {1} objects and {2} AlertScope elements", (object) entityType, (object) objectIds.Length, (object) alertScopeItems.Count);
      }
    }

    private List<AlertScopeItem> GetAlertsForQueries(
      IEnumerable<AlertScopeItem> alertScopeItems,
      string entityType,
      string[] objectIds,
      ISwisConnectionProxyCreator swisCreator)
    {
      List<AlertScopeItem> alertScopeItemList1 = new List<AlertScopeItem>();
      Tuple<string, IDictionary<string, object>> tuple = this.PrepareQueryForAlerts(entityType, alertScopeItems, objectIds);
      if (!string.IsNullOrEmpty(tuple.Item1))
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) swisCreator).Create())
        {
          DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query(tuple.Item1, tuple.Item2);
          if (dataTable != null)
          {
            if (dataTable.Rows.Count > 0)
            {
              Dictionary<int, string> dictionary = alertScopeItems.ToDictionary<AlertScopeItem, int, string>((Func<AlertScopeItem, int>) (p => p.get_AlertId()), (Func<AlertScopeItem, string>) (q => q.get_InstanceName()));
              foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
              {
                int int32 = Convert.ToInt32(row["AlertId"]);
                string str1 = dictionary[int32];
                int num = int32;
                string str2 = row["ObjectId"].ToString();
                List<AlertScopeItem> alertScopeItemList2 = alertScopeItemList1;
                AlertScopeItem alertScopeItem = new AlertScopeItem();
                alertScopeItem.set_AlertId(num);
                alertScopeItem.set_EntityType(entityType);
                alertScopeItem.set_InstanceName(str1);
                alertScopeItem.set_ObjectId(str2);
                alertScopeItemList2.Add(alertScopeItem);
              }
            }
          }
        }
      }
      return alertScopeItemList1;
    }

    private Tuple<string, IDictionary<string, object>> PrepareQueryForAlerts(
      string entityType,
      IEnumerable<AlertScopeItem> alertScopeItems,
      string[] objectIds)
    {
      string str1 = string.Join(",", ((IEnumerable<string>) objectIds).Select<string, string>((Func<string, string>) (p => "'" + p + "'")));
      if (string.IsNullOrEmpty("Uri"))
        throw new InvalidInputException(string.Format("Orion.Alert can't recognize {0} entity, check Orion.NetObjectTypesExt if exists", (object) entityType));
      int num1 = 1;
      IDictionary<string, object> dictionary = (IDictionary<string, object>) new Dictionary<string, object>();
      StringBuilder stringBuilder = new StringBuilder();
      using (IEnumerator<AlertScopeItem> enumerator = alertScopeItems.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          AlertScopeItem current = enumerator.Current;
          string str2 = string.Format("SELECT {0} AS AlertId, '{1}' AS ObjectId FROM {2} AS E0 WHERE E0.{3} IN ({4})", (object) current.get_AlertId(), (object) objectIds[0], (object) entityType, (object) "Uri", (object) str1);
          if (string.IsNullOrEmpty(current.get_ScopeQuery().get_Query()))
          {
            CoreBusinessLayerService.log.Warn((object) "Object scope can be evaluated because ScopeQuery (query for evaluation) is not provided (null)");
          }
          else
          {
            int num2 = current.get_ScopeQuery().get_Query().IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
            if (num2 < 0)
            {
              int num3 = stringBuilder.Length == 0 ? 1 : 0;
              if (num3 == 0)
                stringBuilder.AppendLine("UNION (");
              stringBuilder.AppendLine(str2);
              if (num3 == 0)
                stringBuilder.AppendLine(")");
            }
            else
            {
              string str3 = Regex.Replace(current.get_ScopeQuery().get_Query().Substring(num2 + 5).Trim(), "\\s+", " ") + " ";
              foreach (string key in current.get_ScopeQuery().get_Params().Keys)
              {
                string oldValue = string.Format("@{0} ", (object) key);
                string newValue = string.Format("@{0}no{1} ", (object) key, (object) num1);
                str3 = str3.Replace(oldValue, newValue);
                dictionary.Add(key + "no" + (object) num1, current.get_ScopeQuery().get_Params()[key]);
              }
              int num3 = stringBuilder.Length == 0 ? 1 : 0;
              if (num3 == 0)
                stringBuilder.AppendLine("UNION (");
              stringBuilder.AppendLine(string.Format("{0} AND {1}", (object) str2, (object) str3));
              ++num1;
              if (num3 == 0)
                stringBuilder.AppendLine(")");
            }
          }
        }
      }
      string str4 = stringBuilder.ToString();
      if (CoreBusinessLayerService.log.get_IsDebugEnabled())
        CoreBusinessLayerService.log.DebugFormat("Provided query to evaluation of alert scope: {0}", (object) str4);
      return Tuple.Create<string, IDictionary<string, object>>(str4, dictionary);
    }

    private IEnumerable<AlertScopeItem> GetAlertsWhichCanAffectObject(
      string entityType,
      IInformationServiceProxyCreator swisCreator,
      bool loadActions,
      bool loadSchedules)
    {
      List<AlertScopeItem> results = new List<AlertScopeItem>();
      ConditionTypeProvider conditionTypeProvider = ConditionTypeProvider.Create(swisCreator);
      IAlertDefinitionsDAL ialertDefinitionsDal = AlertDefinitionsDAL.Create((IConditionTypeProvider) conditionTypeProvider, swisCreator);
      string objectType = ((ConditionTypeBase) conditionTypeProvider.Get("Core.Dynamic")).get_EntityProvider().GetObjectTypeByEntityFullName(entityType);
      IEnumerable<AlertDefinition> objectTypeWithParams = ialertDefinitionsDal.GetValidItemsOfObjectTypeWithParams(objectType, loadActions, loadSchedules);
      if (objectTypeWithParams != null)
      {
        IEnumerable<AlertDefinition> source = objectTypeWithParams.Where<AlertDefinition>((Func<AlertDefinition, bool>) (p => p.get_Enabled()));
        object obj = new object();
        Parallel.ForEach<AlertDefinition>(source, (Action<AlertDefinition>) (definition =>
        {
          if (((ICollection<ConditionChainItem>) definition.get_Trigger().get_Conditions()).Count <= 0 || !(definition.get_Trigger().get_Conditions()[0].get_Type() is IConditionEntityScope type))
            return;
          QueryResult scopeQuery = type.GetScopeQuery(definition.get_Trigger().get_Conditions()[0].get_Condition(), objectType);
          AlertScopeItem alertScopeItem1 = new AlertScopeItem();
          alertScopeItem1.set_AlertId(Convert.ToInt32((object) definition.get_AlertID()));
          alertScopeItem1.set_InstanceName(definition.get_Name());
          alertScopeItem1.set_ScopeQuery(scopeQuery);
          alertScopeItem1.set_EntityType(entityType);
          AlertScopeItem alertScopeItem2 = alertScopeItem1;
          lock (obj)
            results.Add(alertScopeItem2);
        }));
      }
      return (IEnumerable<AlertScopeItem>) results;
    }

    public AlertScopeItemDetails GetAlertScopeDetails(int alertId)
    {
      DataTable dataTable1;
      DataTable dataTable2;
      using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator()).Create())
      {
        dataTable1 = ((IInformationServiceProxy) iinformationServiceProxy2).Query("SELECT Field \r\n\t                                                  FROM Orion.CustomProperty \r\n\t                                                  WHERE TargetEntity = 'Orion.AlertConfigurationsCustomProperties'\r\n\t                                                  AND Table = 'AlertConfigurationsCustomProperties'");
        StringBuilder stringBuilder = new StringBuilder();
        if (dataTable1.Rows.Count > 0)
        {
          foreach (DataRow row in (InternalDataCollectionBase) dataTable1.Rows)
            stringBuilder.Append(", [CP]." + row[0].ToString());
        }
        string str = "SELECT AC.AlertID, AC.Name, AC.Description, AC.Severity {columnNames} \r\n\t                                     FROM Orion.AlertConfigurations AS [AC] \r\n\t                                     INNER JOIN Orion.AlertConfigurationsCustomProperties AS [CP] \r\n\t                                     ON AC.AlertID = CP.AlertID WHERE AC.AlertID = @alertID".Replace("{columnNames}", stringBuilder.ToString());
        dataTable2 = ((IInformationServiceProxy) iinformationServiceProxy2).Query(str, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "alertID",
            (object) alertId
          }
        });
      }
      AlertScopeItemDetails scopeItemDetails = (AlertScopeItemDetails) null;
      if (dataTable2 != null && dataTable2.Rows.Count == 1)
      {
        scopeItemDetails = new AlertScopeItemDetails();
        scopeItemDetails.set_AlertId(alertId);
        scopeItemDetails.set_AlertName(dataTable2.Rows[0]["Name"].ToString());
        scopeItemDetails.set_Description(dataTable2.Rows[0]["Description"].ToString());
        scopeItemDetails.set_Severity((AlertSeverity) int.Parse(dataTable2.Rows[0]["Severity"].ToString()));
        scopeItemDetails.set_CustomProperties(new Dictionary<string, string>());
        foreach (DataRow row in (InternalDataCollectionBase) dataTable1.Rows)
        {
          string key = row[0].ToString();
          scopeItemDetails.get_CustomProperties().Add(key, dataTable2.Rows[0][key].ToString());
        }
      }
      return scopeItemDetails;
    }

    public bool UnacknowledgeAlerts(int[] alertObjectIds, string accountId)
    {
      ActiveAlertDAL activeAlertDal = new ActiveAlertDAL();
      IEnumerable<int> source = activeAlertDal.LimitAlertAckStateUpdateCandidates((IEnumerable<int>) alertObjectIds, false);
      List<INotification> inotificationList = new List<INotification>();
      IEnumerable<AlertUpdatedNotification> byAlertObjectIds1 = activeAlertDal.GetAlertUpdatedNotificationPropertiesByAlertObjectIds((IEnumerable<int>) alertObjectIds, accountId, string.Empty, DateTime.UtcNow, false);
      inotificationList.AddRange((IEnumerable<INotification>) byAlertObjectIds1);
      DataTable byAlertObjectIds2 = activeAlertDal.GetAlertResetOrUpdateNotificationPropertiesTableByAlertObjectIds((IEnumerable<int>) alertObjectIds);
      foreach (int alertObjectId in alertObjectIds)
      {
        DataRow[] dataRowArray = byAlertObjectIds2.Select("AlertObjectID=" + (object) alertObjectId);
        PropertyBag propertyBag = new PropertyBag();
        if (dataRowArray.Length != 0)
        {
          ((Dictionary<string, object>) propertyBag).Add("Acknowledged", dataRowArray[0]["Acknowledged"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["Acknowledged"]) : (object) "False");
          ((Dictionary<string, object>) propertyBag).Add("AcknowledgedBy", dataRowArray[0]["AcknowledgedBy"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["AcknowledgedBy"]) : (object) string.Empty);
          ((Dictionary<string, object>) propertyBag).Add("AcknowledgedDateTime", dataRowArray[0]["AcknowledgedDateTime"] != DBNull.Value ? (object) Convert.ToString(dataRowArray[0]["AcknowledgedDateTime"]) : (object) string.Empty);
        }
        inotificationList.Add((INotification) new CommonNotification(IndicationHelper.GetIndicationType((IndicationType) 2), accountId, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "AlertObjectID",
            (object) alertObjectId
          },
          {
            "Acknowledged",
            (object) "False"
          },
          {
            "AcknowledgedBy",
            (object) string.Empty
          },
          {
            "AcknowledgedDateTime",
            (object) string.Empty
          },
          {
            "PreviousProperties",
            (object) propertyBag
          },
          {
            "InstanceType",
            (object) "Orion.AlertActive"
          }
        }));
      }
      int num = ActiveAlertDAL.UnacknowledgeAlerts(source.ToArray<int>(), accountId) ? 1 : 0;
      if (num == 0)
        return num != 0;
      PublisherClient.get_Instance().Publish((IReadOnlyCollection<INotification>) inotificationList);
      return num != 0;
    }

    public string GetAlertNote(int alertObjectId)
    {
      return new ActiveAlertDAL().GetAlertNote(alertObjectId);
    }

    public bool SetAlertNote(
      int alertObjectId,
      string accountId,
      string note,
      DateTime modificationDateTime,
      string previousNote)
    {
      ActiveAlertDAL activeAlertDal1 = new ActiveAlertDAL();
      if (!activeAlertDal1.SetAlertNote(alertObjectId, accountId, note, modificationDateTime))
        return false;
      List<INotification> inotificationList1 = new List<INotification>();
      string indicationType = IndicationHelper.GetIndicationType((IndicationType) 2);
      string str = accountId;
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      dictionary.Add("AlertObjectID", (object) alertObjectId);
      dictionary.Add("AlertNote", (object) note);
      PropertyBag propertyBag = new PropertyBag();
      ((Dictionary<string, object>) propertyBag).Add("AlertNote", (object) previousNote);
      dictionary.Add("PreviousProperties", (object) propertyBag);
      dictionary.Add("InstanceType", (object) "Orion.AlertObjects");
      inotificationList1.Add((INotification) new CommonNotification(indicationType, str, (IDictionary<string, object>) dictionary));
      List<INotification> inotificationList2 = inotificationList1;
      ActiveAlertDAL activeAlertDal2 = activeAlertDal1;
      List<int> intList = new List<int>();
      intList.Add(alertObjectId);
      string accountId1 = accountId;
      string notes = note;
      DateTime utcNow = DateTime.UtcNow;
      IEnumerable<AlertUpdatedNotification> byAlertObjectIds = activeAlertDal2.GetAlertUpdatedNotificationPropertiesByAlertObjectIds((IEnumerable<int>) intList, accountId1, notes, utcNow, false);
      if (byAlertObjectIds.Any<AlertUpdatedNotification>())
      {
        IDictionary<string, object> instanceProperties = ((CommonNotification) byAlertObjectIds.ElementAt<AlertUpdatedNotification>(0)).get_SourceInstanceProperties();
        if (instanceProperties.ContainsKey("Acknowledged"))
          instanceProperties.Remove("Acknowledged");
        if (instanceProperties.ContainsKey("AcknowledgedBy"))
          instanceProperties.Remove("AcknowledgedBy");
        if (instanceProperties.ContainsKey("AcknowledgedMethod"))
          instanceProperties.Remove("AcknowledgedMethod");
        if (!instanceProperties.ContainsKey("Notes"))
          instanceProperties.Add("Notes", (object) note);
        INotification inotification = (INotification) new CommonNotification(IndicationHelper.GetIndicationType((IndicationType) 10), accountId, instanceProperties);
        inotificationList2.Add(inotification);
      }
      PublisherClient.get_Instance().Publish((IReadOnlyCollection<INotification>) inotificationList2);
      return true;
    }

    public bool SetAlertNote(
      int alertObjectId,
      string accountId,
      string note,
      DateTime modificationDateTime)
    {
      return this.SetAlertNote(alertObjectId, accountId, note, modificationDateTime, string.Empty);
    }

    public bool AppendAlertNote(
      int alertObjectId,
      string accountId,
      string note,
      DateTime modificationDateTime)
    {
      string alertNote = this.GetAlertNote(alertObjectId);
      note = string.IsNullOrWhiteSpace(alertNote) ? note : alertNote + Environment.NewLine + note;
      return this.SetAlertNote(alertObjectId, accountId, note, modificationDateTime, alertNote);
    }

    public AlertImportResult ImportAlert(
      string fileContent,
      string userName,
      bool generateNewGuid,
      bool importIfExists,
      bool importSmtpServer)
    {
      return this.ImportAlertConfiguration(fileContent, userName, generateNewGuid, importIfExists, importSmtpServer, false, string.Empty);
    }

    public AlertImportResult ImportAlertConfiguration(
      string fileContent,
      string user,
      bool generateNewGuid,
      bool importIfExists,
      bool importSmtpServer,
      bool stripSensitiveData,
      string protectionPassword)
    {
      try
      {
        AlertDefinition alertDefinition = new AlertImporter((IAlertMigrationPluginProvider) AlertMigrationPluginProvider.Create((ComposablePartCatalog) AppDomainCatalogSingleton.get_Instance()), (IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator(), (INetObjectTypeSource) new CoreNetObjectTypeSource((ICoreBusinessLayer) this), generateNewGuid, importIfExists, false, false).ImportAlert(XElement.Parse(fileContent), (string) null, false, true, (ICoreBusinessLayer) this, (List<CannedAlertImportResult>) null, stripSensitiveData, protectionPassword);
        AlertImportResult alertImportResult = new AlertImportResult();
        alertImportResult.set_AlertId(new int?(alertDefinition.get_AlertID().Value));
        alertImportResult.set_Name(alertDefinition.get_Name());
        alertImportResult.set_MigrationMessage("Alert imported successfully");
        return alertImportResult;
      }
      catch (CryptographicException ex)
      {
        AlertImportResult alertImportResult = new AlertImportResult();
        alertImportResult.set_MigrationMessage(string.Format("Alert import failed with error: {0}", (object) ex.Message));
        alertImportResult.set_IncorrectPasswordForDecryptSensitiveData(true);
        return alertImportResult;
      }
      catch (Exception ex)
      {
        AlertImportResult alertImportResult = new AlertImportResult();
        alertImportResult.set_MigrationMessage(string.Format("Alert import failed with error: {0}", (object) ex.Message));
        return alertImportResult;
      }
    }

    [Obsolete("Old alerting will be removed")]
    public bool RevertMigratedAlert(Guid alertRefId, bool enableInOldAlerting)
    {
      return AlertDAL.RevertMigratedAlert(alertRefId, enableInOldAlerting);
    }

    public string ExportAlert(int alertId)
    {
      return new AlertExporter().ExportAlert(alertId);
    }

    public string ExportAlertConfiguration(
      int alertId,
      bool stripSensitiveData,
      string protectionPassword)
    {
      return new AlertExporter().ExportAlert(alertId, stripSensitiveData, protectionPassword);
    }

    public AlertMigrationResult MigrateAdvancedAlertFromDB(
      string definitionIdGuid)
    {
      using (AlertsMigrator alertsMigrator = new AlertsMigrator())
        return alertsMigrator.MigrateAdvancedAlertFromDB(definitionIdGuid);
    }

    public AlertMigrationResult[] MigrateAllAdvancedAlertsFromDB()
    {
      using (AlertsMigrator alertsMigrator = new AlertsMigrator())
        return alertsMigrator.MigrateAllAdvancedAlertsFromDB(false);
    }

    public AlertMigrationResult[] MigrateAdvancedAlertFromXML(
      string xmlOldAlertDefinition)
    {
      using (AlertsMigrator alertsMigrator = new AlertsMigrator())
        return alertsMigrator.MigrateAdvancedAlertFromXML(xmlOldAlertDefinition);
    }

    public AlertMigrationResult MigrateBasicAlertFromDB(int alertId)
    {
      using (AlertsMigrator alertsMigrator = new AlertsMigrator())
        return alertsMigrator.MigrateBasicAlertFromDB(alertId);
    }

    public AlertMigrationResult[] MigrateAllBasicAlertsFromDB()
    {
      using (AlertsMigrator alertsMigrator = new AlertsMigrator())
        return alertsMigrator.MigrateAllBasicAlertsFromDB(false);
    }

    public AlertMigrationResult[] GetAlertMigrationResults(string migrationId)
    {
      return new AlertMigrationLogDAL().GetAlertMigrationResults(migrationId).ToArray<AlertMigrationResult>();
    }

    public CannedAlertImportResult[] GetCannedAlertImportResults(
      string importId)
    {
      return new ImportedCannedAlertDAL().GetCannedAlertImportResults(importId).ToArray<CannedAlertImportResult>();
    }

    public List<AuditActionTypeInfo> GetAuditingActionTypes()
    {
      AuditingDAL auditingDal = new AuditingDAL();
      auditingDal.LoadKeys();
      return auditingDal.GetAuditingActionTypes();
    }

    public DataTable GetAuditingTable(
      int maxRecords,
      int netObjectId,
      string netObjectType,
      int nodeId,
      string actionTypeIds,
      DateTime startTime,
      DateTime endTime)
    {
      return this.GetAuditingTableWithHtmlMessages(AuditingDAL.GetAuditingTable(maxRecords, netObjectId, netObjectType, nodeId, actionTypeIds, startTime, endTime));
    }

    private DataTable GetAuditingTableWithHtmlMessages(DataTable originTable)
    {
      DataTable dataTable = new DataTable()
      {
        TableName = "AuditingTableWithHtmlMessages"
      };
      dataTable.Columns.Add("DateTime", typeof (DateTime));
      dataTable.Columns.Add("AccountID", typeof (string));
      dataTable.Columns.Add("Message", typeof (string));
      if (originTable == null || originTable.Rows.Count == 0)
        return dataTable;
      foreach (IGrouping<int, DataRow> source in originTable.Rows.Cast<DataRow>().GroupBy<DataRow, int>((Func<DataRow, int>) (it => it.Field<int>("AuditEventID"))))
      {
        int actionTypeId = source.First<DataRow>().Field<int>("ActionTypeID");
        string account = source.First<DataRow>().Field<string>("AccountID");
        DateTime dateTime = source.First<DataRow>().Field<DateTime>("DateTime");
        Dictionary<string, string> dictionary = source.Select(it => new
        {
          Key = it.Field<string>("ArgsKey"),
          Value = it.Field<string>("ArgsValue")
        }).Where(it => it.Key != null).ToDictionary(it => it.Key, it => it.Value);
        string storedMessage = source.First<DataRow>().Field<string>("Message");
        dataTable.Rows.Add((object) dateTime, (object) account, (object) this.RetrieveHtmlMessage(actionTypeId, account, dictionary, storedMessage));
      }
      return dataTable;
    }

    private string RetrieveHtmlMessage(
      int actionTypeId,
      string account,
      Dictionary<string, string> args,
      string storedMessage)
    {
      AuditActionType typeFromActionId = this.auditingDal.GetActionTypeFromActionId(actionTypeId);
      IAuditing2 instancesOfActionType = this._auditPluginManager.GetAuditingInstancesOfActionType(typeFromActionId);
      return instancesOfActionType != null ? ((IAuditing) instancesOfActionType).GetHTMLMessage(new AuditDataContainer(typeFromActionId, args, account)) : storedMessage;
    }

    public DataTable GetAuditingTypesTable()
    {
      return AuditingDAL.GetAuditingTypesTable();
    }

    private static BlogItem DalToWfc(BlogItemDAL dal)
    {
      return dal == null ? (BlogItem) null : new BlogItem(dal.Id, dal.Title, dal.Description, dal.CreatedAt, dal.Ignored, dal.Url, dal.AcknowledgedAt, dal.AcknowledgedBy, dal.PostGuid, dal.PostId, dal.Owner, dal.PublicationDate, dal.CommentsUrl, dal.CommentsCount);
    }

    public BlogItem GetBlogNotificationItem(Guid blogId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for BlogItemDAL.GetBlogById.");
      try
      {
        return CoreBusinessLayerService.DalToWfc(BlogItemDAL.GetItemById(blogId));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining blog notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_25(), (object) blogId));
      }
    }

    public List<BlogItem> GetBlogNotificationItems(
      int maxResultsCount,
      bool includeIgnored)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for BlogItemDAL.GetItems.");
      try
      {
        List<BlogItem> blogItemList = new List<BlogItem>();
        foreach (BlogItemDAL dal in (IEnumerable<BlogItemDAL>) BlogItemDAL.GetItems(new BlogFilter(true, includeIgnored, maxResultsCount)))
          blogItemList.Add(CoreBusinessLayerService.DalToWfc(dal));
        return blogItemList;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when obtaining blog notification items: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_26());
      }
    }

    public void ForceBlogUpdatesCheck()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for CoreHelper.CheckOrionProductTeamBlog.");
      try
      {
        CoreHelper.CheckOrionProductTeamBlog();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error forcing blog notification items update: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_27());
      }
    }

    public BlogItem GetBlogNotificationItemForPost(Guid postGuid, long postId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for BlogItemDAL.GetBlogItemForPos.");
      try
      {
        return CoreBusinessLayerService.DalToWfc(BlogItemDAL.GetBlogItemForPost(postGuid, postId));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining blog notification item for post: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_28(), (object) postGuid, (object) postId));
      }
    }

    [Obsolete("removed", true)]
    public List<ServiceURI> GetAllVMwareServiceURIs()
    {
      throw new NotSupportedException(nameof (GetAllVMwareServiceURIs));
    }

    [Obsolete("removed", true)]
    public VMCredential GetVMwareCredential(long vmwareCredentialsID)
    {
      throw new NotSupportedException(nameof (GetVMwareCredential));
    }

    [Obsolete("removed", true)]
    public void InsertUpdateVMHostNode(VMHostNode node)
    {
      throw new NotSupportedException(nameof (InsertUpdateVMHostNode));
    }

    [Obsolete("removed", true)]
    public VMHostNode GetVMHostNode(int nodeId)
    {
      throw new NotSupportedException(nameof (GetVMHostNode));
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public Dictionary<int, bool> RunNowGeolocation()
    {
      CoreBusinessLayerService.log.DebugFormat("Running job(s)", Array.Empty<object>());
      Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
      string[] availableForGeolocation = WorldMapPointsDAL.GetEntitiesAvailableForGeolocation();
      int key = 1;
      foreach (string str1 in availableForGeolocation)
      {
        string str2;
        if (!WebSettingsDAL.TryGet(string.Format("{0}_GeolocationField", (object) str1), ref str2) || string.IsNullOrWhiteSpace(str2))
          return dictionary;
        ActionDefinition actionDefinition = new ActionDefinition();
        actionDefinition.set_ActionTypeID("Geolocation");
        actionDefinition.set_Enabled(true);
        ActionProperties actionProperties = new ActionProperties();
        actionProperties.Add("StreetAddress", "Location");
        actionProperties.Add("Entity", str1);
        actionProperties.Add("MapQuestApiKey", WorldMapPointsDAL.GetMapQuestKey());
        actionDefinition.set_Properties(actionProperties);
        bool flag = this.ExecuteAction(actionDefinition, (ActionContextBase) new GeolocationActionContext()).get_Status() == 1;
        if (!dictionary.Keys.Contains<int>(key))
          dictionary.Add(key, flag);
        else
          dictionary[key] = flag;
      }
      return dictionary;
    }

    public List<MacroPickerDefinition> GetMacroPickerDefinition(
      MacroContext context)
    {
      return new MacroParser().GetMacroPickerDefinition(context).ToList<MacroPickerDefinition>();
    }

    public string FormatMacroValue(string formatter, string value, string dataType)
    {
      return new MacroParser().FormatValue(formatter, value, dataType);
    }

    public NetObjectTypes GetNetObjectTypes()
    {
      return NetObjectTypeMgr.GetNetObjectTypes();
    }

    public Dictionary<string, string> GetNetObjectData()
    {
      return NetObjectTypeMgr.GetNetObjectData();
    }

    public TestJobResult TestSnmpCredentialsOnAgent(
      int nodeId,
      uint snmpAgentPort,
      SnmpCredentialsV2 credentials)
    {
      AgentInfo agentInfoByNode = this._agentInfoDal.GetAgentInfoByNode(nodeId);
      if (agentInfoByNode == null || agentInfoByNode.get_ConnectionStatus() != 1)
      {
        CoreBusinessLayerService.log.WarnFormat("SNMP credential test could not start because agent for node {0} is not connected", (object) nodeId);
        TestJobResult testJobResult = new TestJobResult();
        testJobResult.set_Success(false);
        testJobResult.set_Message(Resources.get_TestErrorAgentNotConnected());
        return testJobResult;
      }
      TimeSpan testJobTimeout = BusinessLayerSettings.Instance.TestJobTimeout;
      SnmpSettings snmpSettings1 = new SnmpSettings();
      snmpSettings1.set_AgentPort((int) snmpAgentPort);
      snmpSettings1.set_TargetIP(IPAddress.Loopback);
      SnmpSettings snmpSettings2 = snmpSettings1;
      JobDescription jobDescription = new JobDescription();
      jobDescription.set_TypeName("SolarWinds.Orion.Core.TestSnmpCredentialsJob");
      jobDescription.set_JobDetailConfiguration(SerializationHelper.ToJson((object) snmpSettings2));
      jobDescription.set_JobNamespace("orion");
      jobDescription.set_ResultTTL(testJobTimeout);
      jobDescription.set_Timeout(testJobTimeout);
      jobDescription.set_TargetNode(new HostAddress(IPAddress.Loopback.ToString(), (AddressType) 4));
      jobDescription.set_EndpointAddress(agentInfoByNode.get_AgentGuid().ToString());
      jobDescription.set_SupportedRoles((PackageType) 7);
      string errorMessage;
      TestJobResult result = this.ExecuteJobAndGetResult<TestJobResult>(jobDescription, (CredentialBase) credentials, JobResultDataFormatType.Json, "SNMP", out errorMessage);
      if (result.get_Success())
      {
        CoreBusinessLayerService.log.InfoFormat("SNMP credential test finished. Success: {0}, Message: {1}", (object) result.get_Success(), (object) result.get_Message());
        return result;
      }
      TestJobResult testJobResult1 = new TestJobResult();
      testJobResult1.set_Success(false);
      testJobResult1.set_Message(errorMessage);
      return testJobResult1;
    }

    internal T ExecuteJobAndGetResult<T>(
      JobDescription jobDescription,
      CredentialBase jobCredential,
      JobResultDataFormatType resultDataFormat,
      string jobType,
      out string errorMessage)
      where T : TestJobResult, new()
    {
      this.GetCurrentServiceInstance().RouteJobToEngine(jobDescription);
      using (OneTimeJobRawResult timeJobRawResult = this._oneTimeJobManager.ExecuteJob(jobDescription, jobCredential))
      {
        errorMessage = timeJobRawResult.Error;
        if (!timeJobRawResult.Success)
        {
          CoreBusinessLayerService.log.WarnFormat(jobType + " credential test failed: " + timeJobRawResult.Error, Array.Empty<object>());
          string messageFromException = this.GetLocalizedErrorMessageFromException(timeJobRawResult.ExceptionFromJob);
          T obj = new T();
          ((TestJobResult) (object) obj).set_Success(false);
          ((TestJobResult) (object) obj).set_Message(string.IsNullOrEmpty(messageFromException) ? errorMessage : messageFromException);
          return obj;
        }
        try
        {
          if (resultDataFormat != JobResultDataFormatType.Xml)
            return SerializationHelper.Deserialize<T>(timeJobRawResult.JobResultStream);
          using (XmlTextReader xmlTextReader = new XmlTextReader(timeJobRawResult.JobResultStream))
          {
            xmlTextReader.Namespaces = false;
            return (T) new XmlSerializer(typeof (T)).Deserialize((XmlReader) xmlTextReader);
          }
        }
        catch (Exception ex)
        {
          CoreBusinessLayerService.log.Error((object) string.Format("Failed to deserialize {0} credential test job result: {1}", (object) jobType, (object) ex));
          T obj = new T();
          ((TestJobResult) (object) obj).set_Success(false);
          ((TestJobResult) (object) obj).set_Message(this.GetLocalizedErrorMessageFromException(timeJobRawResult.ExceptionFromJob));
          return obj;
        }
      }
    }

    private string GetLocalizedErrorMessageFromException(Exception exception)
    {
      return exception != null && exception is FaultException<JobEngineConnectionFault> ? Resources.get_LIBCODE_PS0_20() : string.Empty;
    }

    public void UpdateOrionFeatures()
    {
      ((OrionFeatureResolver) this.ServiceContainer.GetService<OrionFeatureResolver>()).Resolve();
    }

    public void UpdateOrionFeaturesForProvider(string provider)
    {
      ((OrionFeatureResolver) this.ServiceContainer.GetService<OrionFeatureResolver>()).Resolve(provider);
    }

    public void DeleteOrionServerByEngineId(int engineId)
    {
      new OrionServerDAL().DeleteOrionServerByEngineId(engineId);
    }

    public IEnumerable<SolarWinds.Orion.Core.Common.Models.Technology.Technology> GetTechnologyList()
    {
      return (IEnumerable<SolarWinds.Orion.Core.Common.Models.Technology.Technology>) TechnologyManager.Instance.TechnologyFactory.Items().Select<ITechnology, SolarWinds.Orion.Core.Common.Models.Technology.Technology>((Func<ITechnology, SolarWinds.Orion.Core.Common.Models.Technology.Technology>) (t =>
      {
        SolarWinds.Orion.Core.Common.Models.Technology.Technology technology = new SolarWinds.Orion.Core.Common.Models.Technology.Technology();
        technology.set_DisplayName(t.get_DisplayName());
        technology.set_TargetEntity(t.get_TargetEntity());
        technology.set_TechnologyID(t.get_TechnologyID());
        return technology;
      })).ToList<SolarWinds.Orion.Core.Common.Models.Technology.Technology>();
    }

    public IEnumerable<TechnologyPolling> GetTechnologyPollingList()
    {
      return (IEnumerable<TechnologyPolling>) TechnologyManager.Instance.TechnologyPollingFactory.Items().Select<ITechnologyPolling, TechnologyPolling>((Func<ITechnologyPolling, TechnologyPolling>) (t =>
      {
        TechnologyPolling technologyPolling = new TechnologyPolling();
        technologyPolling.set_DisplayName(t.get_DisplayName());
        technologyPolling.set_TechnologyID(t.get_TechnologyID());
        technologyPolling.set_Priority(t.get_Priority());
        technologyPolling.set_TechnologyPollingID(t.get_TechnologyPollingID());
        return technologyPolling;
      })).ToList<TechnologyPolling>();
    }

    public int[] EnableDisableTechnologyPollingAssignmentOnNetObjects(
      string technologyPollingID,
      bool enable,
      int[] netObjectIDs)
    {
      return TechnologyManager.Instance.TechnologyPollingFactory.EnableDisableAssignments(technologyPollingID, enable, netObjectIDs);
    }

    public int[] EnableDisableTechnologyPollingAssignment(string technologyPollingID, bool enable)
    {
      return TechnologyManager.Instance.TechnologyPollingFactory.EnableDisableAssignments(technologyPollingID, enable, (int[]) null);
    }

    public IEnumerable<TechnologyPollingAssignment> GetTechnologyPollingAssignmentsOnNetObjects(
      string technologyPollingID,
      int[] netObjectIDs)
    {
      return (IEnumerable<TechnologyPollingAssignment>) TechnologyManager.Instance.TechnologyPollingFactory.GetAssignments(technologyPollingID, netObjectIDs).ToList<TechnologyPollingAssignment>();
    }

    public IEnumerable<TechnologyPollingAssignment> GetTechnologyPollingAssignments(
      string technologyPollingID)
    {
      return (IEnumerable<TechnologyPollingAssignment>) TechnologyManager.Instance.TechnologyPollingFactory.GetAssignments(technologyPollingID).ToList<TechnologyPollingAssignment>();
    }

    public ICollection<TechnologyPollingAssignment> GetTechnologyPollingAssignmentsFiltered(
      string[] technologyPollingIDsFilter,
      int[] netObjectIDsFilter,
      string[] targetEntitiesFilter,
      bool[] enabledFilter)
    {
      return (ICollection<TechnologyPollingAssignment>) TechnologyManager.Instance.TechnologyPollingFactory.GetAssignmentsFiltered(technologyPollingIDsFilter, netObjectIDsFilter, targetEntitiesFilter, enabledFilter).ToList<TechnologyPollingAssignment>();
    }

    public List<TimeFrame> GetAllTimeFrames(string timeFrameName = null)
    {
      return TimeFramesDAL.GetAllTimeFrames(timeFrameName);
    }

    public List<TimeFrame> GetCoreTimeFrames(string timeFrameName = null)
    {
      return TimeFramesDAL.GetCoreTimeFrames(timeFrameName);
    }

    public IList<PredefinedCustomProperty> GetPredefinedCustomProperties(
      string targetEntity,
      bool includeAdvanced)
    {
      return CustomPropertyDAL.GetPredefinedPropertiesForTable(targetEntity, includeAdvanced);
    }

    public bool IsSystemProperty(string tableName, string propName)
    {
      return CustomPropertyDAL.IsSystemProperty(tableName, propName);
    }

    public bool IsReservedWord(string propName)
    {
      return CustomPropertyDAL.IsReservedWord(propName);
    }

    public bool IsColumnExists(string table, string name)
    {
      return CustomPropertyDAL.IsColumnExists(table, name);
    }

    public List<string> GetSystemPropertyNamesFromDb(string table)
    {
      return CustomPropertyDAL.GetSystemPropertyNamesFromDb(table);
    }

    [Obsolete("Use IDependencyProxy class", true)]
    public int DeleteDependencies(List<int> listIds)
    {
      CoreBusinessLayerService.log.Error((object) "Unexpected call to deprecated method DeleteDependencies.");
      throw new InvalidOperationException("Use IDependencyProxy class");
    }

    [Obsolete("Use IDependencyProxy class", true)]
    public Dependency GetDependency(int id)
    {
      CoreBusinessLayerService.log.Error((object) "Unexpected call to deprecated method GetDependency");
      throw new InvalidOperationException("Use IDependencyProxy class");
    }

    [Obsolete("Use IDependencyProxy class", true)]
    public void UpdateDependency(Dependency dependency)
    {
      CoreBusinessLayerService.log.Error((object) "Unexpected call to deprecated method UpdateDependency");
      throw new InvalidOperationException("Use IDependencyProxy class");
    }

    [Obsolete("Use DeleteOrionDiscoveryProfile", true)]
    public void DeleteDiscoveryProfile(int profileID)
    {
    }

    public DiscoveryConfiguration GetDiscoveryConfigurationByProfile(
      int profileID)
    {
      return DiscoveryDatabase.GetDiscoveryConfiguration(profileID);
    }

    public bool TryConnectionWithJobScheduler(out string errorMessage)
    {
      try
      {
        using (IJobSchedulerHelper instance = JobScheduler.GetInstance())
        {
          ((IJobScheduler) instance).PolicyExists("Nothing");
          errorMessage = string.Empty;
          return true;
        }
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("{0}: {1}", (object) ex.GetType().Name, (object) ex.Message);
        return false;
      }
    }

    public IJobFactory JobFactory
    {
      get
      {
        return this._jobFactory ?? (this._jobFactory = (IJobFactory) new OrionDiscoveryJobFactory());
      }
      set
      {
        this._jobFactory = value;
      }
    }

    public IPersistentDiscoveryCache PersistentDiscoveryCache
    {
      get
      {
        return this._persistentDiscoveryCache ?? (this._persistentDiscoveryCache = (IPersistentDiscoveryCache) new SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache.PersistentDiscoveryCache());
      }
      set
      {
        this._persistentDiscoveryCache = value;
      }
    }

    public Guid CreateOneTimeAgentDiscoveryJob(
      int nodeId,
      int engineId,
      int? profileId,
      List<Credential> credentials)
    {
      OneTimeDiscoveryJobConfiguration jobConfiguration = new OneTimeDiscoveryJobConfiguration();
      jobConfiguration.set_NodeId(new int?(nodeId));
      jobConfiguration.set_IpAddress(IPAddress.Loopback);
      jobConfiguration.set_EngineId(engineId);
      jobConfiguration.set_Credentials(credentials);
      jobConfiguration.set_ProfileId(profileId);
      return this.CreateOneTimeDiscoveryJobWithCache(jobConfiguration, (DiscoveryCacheConfiguration) 2);
    }

    public Guid CreateOneTimeDiscoveryJob(
      int? nodeId,
      uint? snmpPort,
      SNMPVersion? preferredSnmpVersion,
      IPAddress ip,
      int engineId,
      List<Credential> credentials)
    {
      OneTimeDiscoveryJobConfiguration jobConfiguration = new OneTimeDiscoveryJobConfiguration();
      jobConfiguration.set_NodeId(nodeId);
      jobConfiguration.set_SnmpPort(snmpPort);
      jobConfiguration.set_PreferredSnmpVersion(preferredSnmpVersion);
      jobConfiguration.set_IpAddress(ip);
      jobConfiguration.set_EngineId(engineId);
      jobConfiguration.set_Credentials(credentials);
      return this.CreateOneTimeDiscoveryJobWithCache(jobConfiguration, (DiscoveryCacheConfiguration) 2);
    }

    public Guid CreateOneTimeDiscoveryJobWithCache(
      int? nodeId,
      uint? snmpPort,
      SNMPVersion? preferredSnmpVersion,
      IPAddress ip,
      int engineId,
      List<Credential> credentials,
      DiscoveryCacheConfiguration cacheConfiguration)
    {
      OneTimeDiscoveryJobConfiguration jobConfiguration = new OneTimeDiscoveryJobConfiguration();
      jobConfiguration.set_NodeId(nodeId);
      jobConfiguration.set_SnmpPort(snmpPort);
      jobConfiguration.set_PreferredSnmpVersion(preferredSnmpVersion);
      jobConfiguration.set_IpAddress(ip);
      jobConfiguration.set_EngineId(engineId);
      jobConfiguration.set_Credentials(credentials);
      return this.CreateOneTimeDiscoveryJobWithCache(jobConfiguration, cacheConfiguration);
    }

    public Guid CreateOneTimeDiscoveryJobWithCache(
      OneTimeDiscoveryJobConfiguration jobConfiguration,
      DiscoveryCacheConfiguration cacheConfiguration)
    {
      CoreBusinessLayerService.log.DebugFormat("Creating one shot discovery job. Caching policy is {0}", (object) cacheConfiguration);
      if (jobConfiguration.get_NodeId().HasValue && cacheConfiguration == 2)
      {
        CoreBusinessLayerService.log.DebugFormat("Scanning cache for discovery for Node {0}", (object) jobConfiguration.get_NodeId());
        DiscoveryResultItem resultForNode = this.PersistentDiscoveryCache.GetResultForNode(jobConfiguration.get_NodeId().Value);
        if (resultForNode != null && resultForNode.get_Age() < (TimeSpan) DiscoverySettings.OneTimeJobResultMaximalAge)
        {
          DiscoveryResultCache.get_Instance().AddOrReplaceResult(resultForNode);
          return resultForNode.get_JobId();
        }
      }
      else
        CoreBusinessLayerService.log.DebugFormat("Bypassing discovery cache. ", Array.Empty<object>());
      DiscoveryConfiguration discoveryConfiguration = new DiscoveryConfiguration();
      ((DiscoveryConfigurationBase) discoveryConfiguration).set_ProfileId(jobConfiguration.get_ProfileId());
      ((DiscoveryConfigurationBase) discoveryConfiguration).set_EngineId(jobConfiguration.get_EngineId());
      discoveryConfiguration.set_HopCount(0);
      discoveryConfiguration.set_SearchTimeout((TimeSpan) DiscoverySettings.DefaultSearchTimeout);
      discoveryConfiguration.set_SnmpTimeout(TimeSpan.FromMilliseconds((double) this._settingsDal.GetCurrentInt("SWNetPerfMon-Settings-SNMP Timeout", 2500)));
      discoveryConfiguration.set_SnmpRetries(this._settingsDal.GetCurrentInt("SWNetPerfMon-Settings-SNMP Retries", 2));
      discoveryConfiguration.set_DefaultProbes(jobConfiguration.get_DefaultProbes());
      discoveryConfiguration.set_TagFilter(jobConfiguration.get_TagFilter());
      discoveryConfiguration.set_DisableICMP(jobConfiguration.get_DisableIcmp());
      DiscoveryConfiguration configuration = discoveryConfiguration;
      DiscoveryPollingEngineType? pollingEngineType = OrionDiscoveryJobFactory.GetDiscoveryPollingEngineType(jobConfiguration.get_EngineId(), this._engineDal);
      SolarWinds.Orion.Core.Common.Models.Node node = (SolarWinds.Orion.Core.Common.Models.Node) null;
      int? nodeId1 = jobConfiguration.get_NodeId();
      if (nodeId1.HasValue)
      {
        INodeBLDAL nodeBlDal = this._nodeBlDal;
        nodeId1 = jobConfiguration.get_NodeId();
        int nodeId2 = nodeId1.Value;
        node = nodeBlDal.GetNode(nodeId2);
        if (node == null)
        {
          Log log = CoreBusinessLayerService.log;
          nodeId1 = jobConfiguration.get_NodeId();
          // ISSUE: variable of a boxed type
          __Boxed<int> local = (System.ValueType) nodeId1.Value;
          log.ErrorFormat("Unable to get node {0}", (object) local);
        }
      }
      if (jobConfiguration.get_SnmpPort().HasValue)
        configuration.set_SnmpPort(jobConfiguration.get_SnmpPort().Value);
      else if (node != null)
      {
        configuration.set_SnmpPort(node.get_SNMPPort());
      }
      else
      {
        configuration.set_SnmpPort(161U);
        Log log = CoreBusinessLayerService.log;
        nodeId1 = jobConfiguration.get_NodeId();
        // ISSUE: variable of a boxed type
        __Boxed<int> local = (System.ValueType) (nodeId1 ?? -1);
        IPAddress ipAddress = jobConfiguration.get_IpAddress();
        log.InfoFormat("Unable to determine SNMP port node {0} IP {1}, using default 161", (object) local, (object) ipAddress);
      }
      if (jobConfiguration.get_PreferredSnmpVersion().HasValue)
        configuration.set_PreferredSnmpVersion(jobConfiguration.get_PreferredSnmpVersion().Value);
      else if (node != null)
      {
        configuration.set_PreferredSnmpVersion((SNMPVersion) node.get_SNMPVersion());
      }
      else
      {
        configuration.set_PreferredSnmpVersion((SNMPVersion) 2);
        Log log = CoreBusinessLayerService.log;
        nodeId1 = jobConfiguration.get_NodeId();
        // ISSUE: variable of a boxed type
        __Boxed<int> local = (System.ValueType) (nodeId1 ?? -1);
        IPAddress ipAddress = jobConfiguration.get_IpAddress();
        log.InfoFormat("Unable to determine preffered SNMP version node {0} IP {1}, using default v2c", (object) local, (object) ipAddress);
      }
      List<Credential> credentials = jobConfiguration.get_Credentials() ?? new List<Credential>();
      AgentInfo updateConfiguration = this.TryGetAgentInfoAndUpdateConfiguration(node, credentials, configuration);
      List<string> agentPlugins = new List<string>();
      bool flag = RegistrySettings.IsFreePoller();
      List<DiscoveryPluginInfo> discoveryPluginInfos = DiscoveryPluginFactory.GetDiscoveryPluginInfos();
      IList<IDiscoveryPlugin> discoveryPlugins = DiscoveryHelper.GetOrderedDiscoveryPlugins();
      IDictionary<IDiscoveryPlugin, DiscoveryPluginInfo> pairsPluginAndInfo = DiscoveryPluginHelper.CreatePairsPluginAndInfo((IEnumerable<IDiscoveryPlugin>) discoveryPlugins, (IEnumerable<DiscoveryPluginInfo>) discoveryPluginInfos);
      using (IEnumerator<IDiscoveryPlugin> enumerator = ((IEnumerable<IDiscoveryPlugin>) discoveryPlugins).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveryPlugin current = enumerator.Current;
          if (flag && !(current is ISupportFreeEngine))
            CoreBusinessLayerService.log.DebugFormat("Discovery plugin {0} is not supported on FPE machine", (object) ((object) current).GetType().FullName);
          else if (!(current is IOneTimeJobSupport ioneTimeJobSupport))
          {
            CoreBusinessLayerService.log.DebugFormat("N/A one time job for {0}", (object) current);
          }
          else
          {
            if (jobConfiguration.get_TagFilter() != null && jobConfiguration.get_TagFilter().Any<string>())
            {
              if (!(current is IDiscoveryPluginTags idiscoveryPluginTags))
              {
                CoreBusinessLayerService.log.DebugFormat("Discovery job for tags requested, however plugin {0} doesn't support tags, skipping.", (object) current);
                continue;
              }
              if (!jobConfiguration.get_TagFilter().Intersect<string>(idiscoveryPluginTags.get_Tags() ?? Enumerable.Empty<string>(), (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase).Any<string>())
              {
                if (CoreBusinessLayerService.log.get_IsDebugEnabled())
                {
                  CoreBusinessLayerService.log.DebugFormat("Discovery job for tags [{0}] requested, however plugin {1} doesn't support any of the tags requested, skipping.", (object) string.Join(",", (IEnumerable<string>) jobConfiguration.get_TagFilter()), (object) current);
                  continue;
                }
                continue;
              }
            }
            if (updateConfiguration == null || this.DoesPluginSupportAgent(current, updateConfiguration, agentPlugins))
            {
              if (pollingEngineType.HasValue && !OrionDiscoveryJobFactory.IsDiscoveryPluginSupportedForDiscoveryPollingEngineType(current, pollingEngineType.Value, pairsPluginAndInfo))
              {
                if (CoreBusinessLayerService.log.get_IsDebugEnabled())
                  CoreBusinessLayerService.log.DebugFormat(string.Format("Plugin {0} is not supported for polling engine {1} of type {2}", (object) ((object) current).GetType().FullName, (object) configuration.get_EngineID(), (object) pollingEngineType.Value), Array.Empty<object>());
              }
              else
              {
                DiscoveryPluginConfigurationBase jobConfiguration1 = ioneTimeJobSupport.GetOneTimeJobConfiguration(jobConfiguration.get_NodeId(), jobConfiguration.get_IpAddress(), credentials);
                ((DiscoveryConfigurationBase) configuration).AddDiscoveryPluginConfiguration(jobConfiguration1);
                CoreBusinessLayerService.log.DebugFormat("added one time job for {0}", (object) current);
              }
            }
          }
        }
      }
      configuration.set_AgentPlugins(agentPlugins.ToArray());
      ScheduledJob discoveryJob = this.JobFactory.CreateDiscoveryJob(configuration);
      if (discoveryJob == null)
      {
        CoreBusinessLayerService.log.WarnFormat("Cannot create Discovery Job for NodeID {0}", (object) jobConfiguration.get_NodeId());
        return Guid.Empty;
      }
      Guid localEngine = this.JobFactory.SubmitScheduledJobToLocalEngine(Guid.Empty, discoveryJob, true);
      CoreBusinessLayerService.log.DebugFormat("Adding one time job with ID {0} into result cache", (object) localEngine);
      DiscoveryResultCache.get_Instance().AddOrReplaceResult(jobConfiguration.get_TagFilter() == null ? new DiscoveryResultItem(localEngine, jobConfiguration.get_NodeId(), cacheConfiguration) : new DiscoveryResultItem(localEngine, jobConfiguration.get_NodeId(), cacheConfiguration, (IReadOnlyCollection<string>) jobConfiguration.get_TagFilter()));
      return localEngine;
    }

    private AgentInfo TryGetAgentInfoAndUpdateConfiguration(
      SolarWinds.Orion.Core.Common.Models.Node node,
      List<Credential> credentials,
      DiscoveryConfiguration configuration)
    {
      AgentInfo nodeOrCredentials = this.TryGetAgentInfoFromNodeOrCredentials(node, credentials);
      if (nodeOrCredentials != null)
      {
        this.EnsureDiscoveryPluginsOnAgent(node, credentials, ref nodeOrCredentials);
        configuration.set_AgentAddress(nodeOrCredentials.get_AgentGuid().ToString());
        configuration.set_IsAgentJob(true);
        configuration.set_UseJsonFormat(nodeOrCredentials.get_UseJsonFormat());
      }
      return nodeOrCredentials;
    }

    private void EnsureDiscoveryPluginsOnAgent(
      SolarWinds.Orion.Core.Common.Models.Node node,
      List<Credential> credentials,
      ref AgentInfo agentInfo)
    {
      try
      {
        using (IEnumerator<IAgentPluginJobSupport> enumerator = ((IEnumerable) ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IOneTimeJobSupport>()).OfType<IAgentPluginJobSupport>().GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            IAgentPluginJobSupport plugin = enumerator.Current;
            AgentPluginInfo agentPluginInfo = agentInfo.get_Plugins().SingleOrDefault<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (ap => ap.get_PluginId() == plugin.get_PluginId()));
            if (agentPluginInfo == null || !((List<int>) AgentInfo.PluginDeploymentFinishedStatuses).Contains(agentPluginInfo.get_Status()))
            {
              CoreBusinessLayerService.log.DebugFormat("Found plugin '{0}' that is required for agent discovery but is missing on agent {1} ({2}) NodeId {3}", new object[4]
              {
                (object) plugin.get_PluginId(),
                (object) agentInfo.get_HostName(),
                (object) agentInfo.get_IPAddress(),
                (object) agentInfo.get_NodeID()
              });
              Task<AgentDeploymentStatus> task = this.DeployAgentDiscoveryPluginsAsync(agentInfo.get_AgentId());
              TimeSpan deploymentTimeLimit = BusinessLayerSettings.Instance.AgentDiscoveryPluginsDeploymentTimeLimit;
              if (!task.Wait(deploymentTimeLimit))
                CoreBusinessLayerService.log.WarnFormat("Plugin deployment on agent {0} ({1}) NodeId {2} hasn't finished in {3}.", new object[4]
                {
                  (object) agentInfo.get_HostName(),
                  (object) agentInfo.get_IPAddress(),
                  (object) agentInfo.get_NodeID(),
                  (object) deploymentTimeLimit
                });
              else if (task.Result == 1)
                CoreBusinessLayerService.log.DebugFormat("Plugin deployment on agent {0} ({1}) NodeId {2} finished successfuly.", (object) agentInfo.get_HostName(), (object) agentInfo.get_IPAddress(), (object) agentInfo.get_NodeID());
              else
                CoreBusinessLayerService.log.WarnFormat("Plugin deployment on agent {0} ({1}) NodeId {2} finished with status {3}.", new object[4]
                {
                  (object) agentInfo.get_HostName(),
                  (object) agentInfo.get_IPAddress(),
                  (object) agentInfo.get_NodeID(),
                  (object) task.Result
                });
              agentInfo = this.TryGetAgentInfoFromNodeOrCredentials(node, credentials);
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.ErrorFormat("Error during EnsureDiscoveryPluginsOnAgent for agent {0} ({1}) NodeId {2}. {3}", new object[4]
        {
          (object) agentInfo.get_HostName(),
          (object) agentInfo.get_IPAddress(),
          (object) agentInfo.get_NodeID(),
          (object) ex
        });
      }
    }

    private AgentInfo TryGetAgentInfoFromNodeOrCredentials(
      SolarWinds.Orion.Core.Common.Models.Node node,
      List<Credential> credentials)
    {
      if (credentials == null)
        throw new ArgumentNullException(nameof (credentials));
      AgentInfo agentInfo = (AgentInfo) null;
      AgentManagementCredential managementCredential1 = ((IEnumerable) credentials).OfType<AgentManagementCredential>().SingleOrDefault<AgentManagementCredential>();
      if (managementCredential1 != null)
      {
        agentInfo = this._agentInfoDal.GetAgentInfo(managementCredential1.get_AgentId());
        if (agentInfo == null)
          throw new InvalidOperationException(string.Format("No AgentManagement record found for AgentID {0}", (object) managementCredential1.get_AgentId()));
      }
      if (agentInfo == null && node != null && node.get_NodeSubType() == 4)
      {
        agentInfo = this._agentInfoDal.GetAgentInfoByNode(node.get_ID());
        if (agentInfo == null)
          throw new InvalidOperationException(string.Format("No AgentManagement record found for NodeID {0}", (object) node.get_ID()));
        AgentManagementCredential managementCredential2 = new AgentManagementCredential();
        managementCredential2.set_AgentId(agentInfo.get_AgentId());
        managementCredential2.set_AgentGuid(agentInfo.get_AgentGuid());
        managementCredential2.set_Plugins(agentInfo.get_Plugins().Select<AgentPluginInfo, string>((Func<AgentPluginInfo, string>) (p => p.get_PluginId())).ToArray<string>());
        AgentManagementCredential managementCredential3 = managementCredential2;
        credentials.Add((Credential) managementCredential3);
      }
      return agentInfo;
    }

    private bool DoesPluginSupportAgent(
      IDiscoveryPlugin plugin,
      AgentInfo agentInfo,
      List<string> agentPlugins)
    {
      IAgentPluginJobSupport agentSupport = plugin as IAgentPluginJobSupport;
      if (agentSupport == null)
      {
        CoreBusinessLayerService.log.DebugFormat("Agent discovery plugin jobs not supported for {0} on agent {1} ({2}) NodeId {3}", new object[4]
        {
          (object) plugin,
          (object) agentInfo.get_HostName(),
          (object) agentInfo.get_IPAddress(),
          (object) agentInfo.get_NodeID()
        });
        return false;
      }
      if (agentPlugins.Contains(agentSupport.get_PluginId()))
        return true;
      AgentPluginInfo agentPluginInfo = agentInfo.get_Plugins().SingleOrDefault<AgentPluginInfo>((Func<AgentPluginInfo, bool>) (ap => ap.get_PluginId() == agentSupport.get_PluginId()));
      if (agentPluginInfo == null || agentPluginInfo.get_Status() != 1)
      {
        CoreBusinessLayerService.log.WarnFormat("Agent plugin {0} on agent {1} ({2}) NodeId {3} not deployed for discovery. Plugin status: {4}. ", new object[5]
        {
          (object) agentSupport.get_PluginId(),
          (object) agentInfo.get_HostName(),
          (object) agentInfo.get_IPAddress(),
          (object) agentInfo.get_NodeID(),
          (object) (agentPluginInfo != null ? agentPluginInfo.get_Status() : 0)
        });
        return false;
      }
      agentPlugins.Add(agentPluginInfo.get_PluginId());
      return true;
    }

    public CreateDiscoveryJobResult CreateOrionDiscoveryJob(
      int profileID,
      bool executeImmediately)
    {
      CoreBusinessLayerService.log.DebugFormat("Creating discovery job for profile {0} where executeImmediately is {1}.", (object) profileID, (object) executeImmediately);
      DiscoveryConfiguration discoveryConfiguration = ((IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>()).GetDiscoveryConfiguration(profileID);
      if (discoveryConfiguration == null)
        throw new ArgumentNullException("configuration");
      DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(discoveryConfiguration.get_ProfileID().Value);
      if (profileById == null)
        throw new ArgumentNullException("profile");
      if (discoveryConfiguration.get_Status().Status == 1 && discoveryConfiguration.get_JobID() != Guid.Empty)
        return (CreateDiscoveryJobResult) 2;
      if (profileById.get_JobID() != Guid.Empty)
      {
        CoreBusinessLayerService.log.DebugFormat("Deleting old job for profile {0}.", (object) profileID);
        if (this.JobFactory.DeleteJob(profileById.get_JobID()))
          profileById.set_JobID(Guid.Empty);
        else
          throw new CoreBusinessLayerService.DicoveryDeletingJobError(Resources.get_DiscoveryBL_DicoveryDeletingJobError(), new object[1]
          {
            (object) discoveryConfiguration.get_JobID()
          });
      }
      ScheduledJob discoveryJob = this.JobFactory.CreateDiscoveryJob(discoveryConfiguration);
      if (discoveryJob == null)
        return (CreateDiscoveryJobResult) 1;
      if (!executeImmediately)
      {
        if (discoveryConfiguration.get_CronSchedule() != null)
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 5, string.Empty));
        else if (discoveryConfiguration.get_ScheduleRunAtTime() != DateTime.MinValue || discoveryConfiguration.get_ScheduleRunFrequency() != TimeSpan.Zero)
        {
          DateTime dateTime = DateTime.Now;
          dateTime = dateTime.ToUniversalTime();
          int int32_1 = Convert.ToInt32(dateTime.TimeOfDay.TotalMinutes);
          int minutes = 0;
          DateTime scheduleRunAtTime = profileById.get_ScheduleRunAtTime();
          TimeSpan timeSpan;
          if (!scheduleRunAtTime.Equals(DateTime.MinValue))
          {
            scheduleRunAtTime = profileById.get_ScheduleRunAtTime();
            timeSpan = scheduleRunAtTime.TimeOfDay;
            int int32_2 = Convert.ToInt32(timeSpan.TotalMinutes);
            minutes = int32_1 >= int32_2 ? 1440 - (int32_1 - int32_2) : int32_2 - int32_1;
          }
          timeSpan = discoveryConfiguration.get_ScheduleRunFrequency();
          if (!timeSpan.Equals(TimeSpan.Zero))
            minutes = profileById.get_ScheduleRunFrequency();
          discoveryJob.set_InitialWait(new TimeSpan(0, minutes, 0));
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 5, string.Empty));
        }
        else
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 4, string.Empty));
      }
      else
        profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 1, string.Empty));
      if (profileById.get_Status().Status != 4)
      {
        CoreBusinessLayerService.log.DebugFormat("Submiting job for profile {0}.", (object) profileID);
        Guid localEngine;
        try
        {
          localEngine = this.JobFactory.SubmitScheduledJobToLocalEngine(Guid.Empty, discoveryJob, executeImmediately);
        }
        catch (FaultException ex)
        {
          CoreBusinessLayerService.log.Error((object) string.Format("Failed to create job for scheduled discovery profile {0}, rescheduler will keep trying", (object) profileID), (Exception) ex);
          this.parent.RunRescheduleEngineDiscoveryJobsTask(profileById.get_EngineID());
          throw;
        }
        profileById.set_JobID(localEngine);
      }
      else
      {
        CoreBusinessLayerService.log.DebugFormat("No job for profile {0} will be created.", (object) profileID);
        profileById.set_JobID(Guid.Empty);
      }
      CoreBusinessLayerService.log.DebugFormat("Updating profile {0}.", (object) profileID);
      profileById.Update();
      CoreBusinessLayerService.log.DebugFormat("Job for profile {0} created.", (object) profileID);
      return (CreateDiscoveryJobResult) 1;
    }

    public OrionDiscoveryJobProgressInfo GetOrionDiscoveryJobProgress(
      int profileID)
    {
      OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = OrionDiscoveryJobSchedulerEventsService.GetProgressInfo(profileID);
      if (discoveryJobProgressInfo == null)
      {
        discoveryJobProgressInfo = new OrionDiscoveryJobProgressInfo();
        DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(profileID);
        discoveryJobProgressInfo.set_Status(profileById.get_Status());
        discoveryJobProgressInfo.set_Starting(true);
        discoveryJobProgressInfo.set_IsAutoImport(profileById.get_IsAutoImport());
      }
      if (discoveryJobProgressInfo.get_Status().Status == 3)
      {
        CoreBusinessLayerService.log.WarnFormat("GetOrionDiscoveryJobProgress(): Error status on profile Id {0}", (object) profileID);
        throw new Exception("Error state received from discovery: " + ((object) discoveryJobProgressInfo).ToString());
      }
      return discoveryJobProgressInfo;
    }

    [Obsolete("Method from old discovery", true)]
    public void CancelDiscovery(int profileID)
    {
    }

    public List<DiscoveryResult> GetDiscoveryResultsList(
      DiscoveryNodeStatus status,
      DiscoveryResultsFilterType filterType,
      object filterValue,
      bool selectOnlyTopX,
      out bool thereIsMoreNodes,
      bool loadInterfacesAndVolumes)
    {
      CoreBusinessLayerService.log.DebugFormat("Sending request for results to DAL for status: {0}, filter type: {1}, filter: {2}.", (object) status, (object) filterType, filterValue == null ? (object) "null" : filterValue);
      List<DiscoveryResult> discoveryResultsList = DiscoveryDatabase.GetDiscoveryResultsList(status, filterType, filterValue, selectOnlyTopX, ref thereIsMoreNodes, loadInterfacesAndVolumes);
      CoreBusinessLayerService.log.DebugFormat("Results recieved from DAL for status: {0}, filter type: {1}, filter: {2}.", (object) status, (object) filterType, filterValue == null ? (object) "null" : filterValue);
      if (filterType == 2)
      {
        int profileId = Convert.ToInt32(filterValue);
        if (!((IEnumerable<DiscoveryResult>) discoveryResultsList).Any<DiscoveryResult>((Func<DiscoveryResult, bool>) (item => ((DiscoveryResultBase) item).get_ProfileID() == profileId)))
        {
          DiscoveryResult discoveryResult = new DiscoveryResult(profileId);
          discoveryResultsList.Add(discoveryResult);
        }
      }
      CoreBusinessLayerService.log.DebugFormat("Converting old discovery result to new one.", Array.Empty<object>());
      List<DiscoveryResult> discoveryResultList = this.ConvertScheduledDiscoveryResults(discoveryResultsList);
      CoreBusinessLayerService.log.DebugFormat("Converting old discovery result to new one finished.", Array.Empty<object>());
      CoreBusinessLayerService.log.DebugFormat("Sending list of results back for status: {0}, filter type: {1}, filter: {2}.", (object) status, (object) filterType, filterValue == null ? (object) "null" : filterValue);
      return discoveryResultList;
    }

    public DiscoveryNode GetVolumesAndInterfacesForDiscoveryNode(
      DiscoveryNode discoveryNode)
    {
      CoreBusinessLayerService.log.DebugFormat("Sending request for load interfaces and volumes to BL for nodeID: {0}", (object) discoveryNode.get_NodeID());
      IEnumerable<IScheduledDiscoveryImport> ischeduledDiscoveryImports = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryImport>();
      DiscoveryDatabase.LoadInterfacesAndVolumesForNode(discoveryNode, ischeduledDiscoveryImports);
      CoreBusinessLayerService.log.DebugFormat("Request received for load interfaces and volumes to BL for nodeID: {0}", (object) discoveryNode.get_NodeID());
      return discoveryNode;
    }

    public int GetCountOfDiscoveryResults(DiscoveryNodeStatus status)
    {
      return DiscoveryNodeEntry.GetCountOfNodes(status);
    }

    public List<DateTime> GetDiscoveryResultListOfDates(DiscoveryNodeStatus status)
    {
      return DiscoveryNodeEntry.GetListOfDatesByStatus(status);
    }

    public List<int> GetDiscoveryResultListOfProfiles(DiscoveryNodeStatus status)
    {
      return DiscoveryNodeEntry.GetListOfProfilesByStatus(status);
    }

    public List<string> GetDiscoveryResultListOfMachineTypes(DiscoveryNodeStatus status)
    {
      return DiscoveryNodeEntry.GetListOfMachineTypesByStatus(status);
    }

    public void DeleteDiscoveryResultsByProfile(int profileID)
    {
      DiscoveryDatabase.DeleteResultsByProfile(profileID);
    }

    public DiscoveredObjectTreeWcfWrapper GetOneTimeJobResult(
      Guid jobId)
    {
      DiscoveryResultItem result = (DiscoveryResultItem) null;
      if (!DiscoveryResultCache.get_Instance().TryGetResultItem(jobId, ref result) || result == null || result.get_ResultTree() == null)
        return (DiscoveredObjectTreeWcfWrapper) null;
      CoreBusinessLayerService.log.DebugFormat("Recieved one time job {0} result from cache", (object) jobId);
      if (result.get_CacheConfiguration() != null && result.get_nodeId().HasValue)
      {
        CoreBusinessLayerService.log.DebugFormat("Storing the result into cache", Array.Empty<object>());
        this.PersistentDiscoveryCache.StoreResultForNode(result.get_nodeId().Value, result);
      }
      if (result.get_nodeId().HasValue && result.get_isCached())
      {
        using (IEnumerator<IOneTimeJobSupport> enumerator = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IOneTimeJobSupport>().GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            IOneTimeJobSupport current = enumerator.Current;
            try
            {
              current.GetDiscoveredResourcesManagedStatus(result.get_ResultTree(), result.get_nodeId().Value);
            }
            catch (Exception ex)
            {
              CoreBusinessLayerService.log.WarnFormat("Error occurred while updating selections in Resource tree with plugin {0}. Ex: {1}", (object) ((object) current).GetType(), (object) ex);
            }
          }
        }
      }
      DiscoveryResultCache.get_Instance().RemoveResult(jobId);
      return new DiscoveredObjectTreeWcfWrapper(result.get_ResultTree(), (DateTime) result.timeOfCreation, result.get_isCached());
    }

    public OrionDiscoveryJobProgressInfo GetOneTimeJobProgress(
      Guid jobId)
    {
      DiscoveryResultItem discoveryResultItem = (DiscoveryResultItem) null;
      if (!DiscoveryResultCache.get_Instance().TryGetResultItem(jobId, ref discoveryResultItem) || discoveryResultItem == null || discoveryResultItem.get_Progress() == null)
        return (OrionDiscoveryJobProgressInfo) null;
      CoreBusinessLayerService.log.DebugFormat("Recieved one time job {0} progress from cache", (object) jobId);
      return discoveryResultItem.get_Progress();
    }

    public Dictionary<string, int> AutoImportOneTimeJobResult(Guid jobId, int nodeId)
    {
      DiscoveredObjectTreeWcfWrapper oneTimeJobResult = this.GetOneTimeJobResult(jobId);
      if (oneTimeJobResult?.get_Tree() == null)
        throw new ApplicationException(string.Format("Unable to get job result for jobId:{0}", (object) jobId));
      using (IEnumerator<IDiscoveredObject> enumerator = ((IEnumerable<IDiscoveredObject>) oneTimeJobResult.get_Tree().GetAllTreeObjectsOfType<IDiscoveredObject>()).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveredObject current = enumerator.Current;
          current.set_IsSelected(true);
          CoreBusinessLayerService.log.Debug((object) ("Selected DiscoveryObject: " + current.get_DisplayName()));
        }
      }
      return this.ImportOneTimeJobResult(oneTimeJobResult, nodeId);
    }

    public Dictionary<string, int> ImportOneTimeJobResult(
      DiscoveredObjectTreeWcfWrapper treeOfSelection,
      int nodeId)
    {
      ICollection<IOneTimeJobSupport> discoveryPlugins = (ICollection<IOneTimeJobSupport>) DiscoveryHelper.GetOrderedOneTimeJobDiscoveryPlugins();
      return this.ImportOneTimeJobResultInternal(treeOfSelection, nodeId, discoveryPlugins);
    }

    public Dictionary<string, int> ImportOneTimeJobResult(
      DiscoveredObjectTreeWcfWrapper treeOfSelection,
      int nodeId,
      IReadOnlyCollection<string> tags)
    {
      List<IOneTimeJobSupport> discoveryPlugins = DiscoveryHelper.GetOrderedOneTimeJobDiscoveryPlugins(tags);
      return this.ImportOneTimeJobResultInternal(treeOfSelection, nodeId, (ICollection<IOneTimeJobSupport>) discoveryPlugins);
    }

    public List<DiscoveryItemGroupDefinition> GetDiscoveryScheduledImportGroupDefinitions()
    {
      return ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryImport>().SelectMany<IScheduledDiscoveryImport, IDiscoveredObjectGroupScheduledImport>((Func<IScheduledDiscoveryImport, IEnumerable<IDiscoveredObjectGroupScheduledImport>>) (n => n.GetScheduledDiscoveryObjectGroups())).Select<IDiscoveredObjectGroupScheduledImport, DiscoveryItemGroupDefinition>((Func<IDiscoveredObjectGroupScheduledImport, DiscoveryItemGroupDefinition>) (n =>
      {
        DiscoveryItemGroupDefinition itemGroupDefinition = new DiscoveryItemGroupDefinition();
        ((DiscoveredObjectGroupWcf<IDiscoveredObjectGroupScheduledImport>) itemGroupDefinition).set_Group(n);
        return itemGroupDefinition;
      })).ToList<DiscoveryItemGroupDefinition>();
    }

    public List<DiscoveryIgnoredNode> GetDiscoveryIgnoredNodes()
    {
      List<DiscoveryIgnoredNode> discoveryIgnoredNodeList = new List<DiscoveryIgnoredNode>();
      bool flag = ((IModuleManager) SolarWinds.Orion.Core.Common.ModuleManager.ModuleManager.InstanceWithCache).IsThereModule("NPM");
      IDictionary<int, ICollection<DiscoveryIgnoredInterfaceEntry>> ignoredInterfacesDict = DiscoveryIgnoredInterfaceEntry.GetIgnoredInterfacesDict();
      IDictionary<int, ICollection<DiscoveryIgnoredVolumeEntry>> ignoredVolumesDict = DiscoveryIgnoredVolumeEntry.GetIgnoredVolumesDict();
      IEnumerable<IScheduledDiscoveryIgnore> list = (IEnumerable<IScheduledDiscoveryIgnore>) ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().ToList<IScheduledDiscoveryIgnore>();
      using (IEnumerator<DiscoveryIgnoredNodeEntry> enumerator1 = ((IEnumerable<DiscoveryIgnoredNodeEntry>) DiscoveryIgnoredNodeEntry.GetIgnoredNodesList()).GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          DiscoveryIgnoredNodeEntry current1 = enumerator1.Current;
          DiscoveryIgnoredNode discoveryIgnoredNode = new DiscoveryIgnoredNode(current1.get_ID(), current1.get_EngineID(), current1.get_IPAddress(), current1.get_Caption(), current1.get_IsIgnored(), current1.get_DateAdded());
          if (ignoredInterfacesDict.ContainsKey(current1.get_ID()))
          {
            using (IEnumerator<DiscoveryIgnoredInterfaceEntry> enumerator2 = ((IEnumerable<DiscoveryIgnoredInterfaceEntry>) ignoredInterfacesDict[current1.get_ID()]).GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                DiscoveryIgnoredInterfaceEntry current2 = enumerator2.Current;
                discoveryIgnoredNode.get_IgnoredInterfaces().Add(new DiscoveryIgnoredInterface(current2.get_ID(), current2.get_IgnoredNodeID(), current2.get_PhysicalAddress(), current2.get_Description(), current2.get_Caption(), current2.get_Type(), current2.get_IfxName(), current2.get_DateAdded()));
              }
            }
          }
          if (ignoredVolumesDict.ContainsKey(current1.get_ID()))
          {
            using (IEnumerator<DiscoveryIgnoredVolumeEntry> enumerator2 = ((IEnumerable<DiscoveryIgnoredVolumeEntry>) ignoredVolumesDict[current1.get_ID()]).GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                DiscoveryIgnoredVolumeEntry current2 = enumerator2.Current;
                discoveryIgnoredNode.get_IgnoredVolumes().Add(new DiscoveryIgnoredVolume(current2.get_ID(), current2.get_IgnoredNodeID(), current2.get_Description(), (VolumeType) current2.get_Type(), current2.get_DateAdded()));
              }
            }
          }
          using (IEnumerator<IScheduledDiscoveryIgnore> enumerator2 = list.GetEnumerator())
          {
            while (((IEnumerator) enumerator2).MoveNext())
            {
              DiscoveryPluginResultBase pluginResultBase = enumerator2.Current.LoadIgnoredResults(current1.get_ID());
              discoveryIgnoredNode.get_NodeResult().get_PluginResults().Add(pluginResultBase);
            }
          }
          if ((flag || current1.get_IsIgnored() || discoveryIgnoredNode.get_IgnoredVolumes().Count != 0 ? 0 : (!((IEnumerable<DiscoveryPluginResultBase>) discoveryIgnoredNode.get_NodeResult().get_PluginResults()).Any<DiscoveryPluginResultBase>((Func<DiscoveryPluginResultBase, bool>) (n => n.GetDiscoveredObjects().Any<IDiscoveredObject>())) ? 1 : 0)) == 0)
            discoveryIgnoredNodeList.Add(discoveryIgnoredNode);
        }
      }
      return discoveryIgnoredNodeList;
    }

    public string AddDiscoveryIgnoredNode(DiscoveryNode discoveryNode)
    {
      int num = this.AddDiscoveryIgnoredNodeOnly(discoveryNode);
      if (num == -1)
      {
        CoreBusinessLayerService.log.ErrorFormat("Discovery Node(NodeID:{0},ProfileID:{1}) could not be ignored", (object) discoveryNode.get_NodeID(), (object) discoveryNode.get_ProfileID());
        return string.Format(Resources.get_WEBCODE_ET_01(), (object) discoveryNode.get_Name());
      }
      if (!discoveryNode.get_IsSelected())
      {
        using (IEnumerator<DiscoveryInterface> enumerator = ((IEnumerable<DiscoveryInterface>) discoveryNode.get_Interfaces()).Where<DiscoveryInterface>((Func<DiscoveryInterface, bool>) (n => n.get_IsSelected())).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            DiscoveryInterface current = enumerator.Current;
            if (!this.AddDiscoveryIgnoredInterface(discoveryNode, current))
              CoreBusinessLayerService.log.WarnFormat("Discovery Interface(InterfaceID:{0}) could not be ignored, because it is already ignored", (object) current.get_InterfaceID());
          }
        }
        using (IEnumerator<DiscoveryVolume> enumerator = ((IEnumerable<DiscoveryVolume>) discoveryNode.get_Volumes()).Where<DiscoveryVolume>((Func<DiscoveryVolume, bool>) (n => n.get_IsSelected())).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            DiscoveryVolume current = enumerator.Current;
            if (!this.AddDiscoveryIgnoredVolume(discoveryNode, current))
              CoreBusinessLayerService.log.WarnFormat("Discovery Volume(VolumeID:{0}) could not be ignored, because it is already ignored", (object) current.get_VolumeID());
          }
        }
        DiscoveredNode discoveredNode = new DiscoveredNode();
        discoveredNode.set_IgnoredNodeId(new int?(num));
        discoveredNode.set_NodeID(discoveryNode.get_NodeID());
        discoveredNode.set_OrionNodeId(new int?(discoveryNode.get_ManagedNodeId()));
        CoreDiscoveryPluginResult discoveryPluginResult1 = new CoreDiscoveryPluginResult();
        CoreDiscoveryPluginResult discoveryPluginResult2 = discoveryPluginResult1;
        List<DiscoveredNode> discoveredNodeList = new List<DiscoveredNode>();
        discoveredNodeList.Add(discoveredNode);
        discoveryPluginResult2.set_DiscoveredNodes(discoveredNodeList);
        discoveryNode.get_NodeResult().get_PluginResults().Add((DiscoveryPluginResultBase) discoveryPluginResult1);
        using (IEnumerator<IScheduledDiscoveryIgnore> enumerator = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
            enumerator.Current.StoreResultsToIgnoreList(discoveryNode.get_NodeResult());
        }
      }
      return string.Empty;
    }

    private int AddDiscoveryIgnoredNodeOnly(DiscoveryNode discoveryNode)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for insert ignored node to DAL.");
      try
      {
        return DiscoveryIgnoredNodeEntry.Insert(discoveryNode.get_EngineID(), discoveryNode.get_IPAddress(), discoveryNode.get_Name(), discoveryNode.get_IsSelected(), discoveryNode.get_ProfileID());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when inserting ignored node: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryInsertingIgnoredNodeError(Resources.get_DiscoveryBL_DiscoveryInsertingIgnoredNodeError(), new object[1]
        {
          (object) discoveryNode.get_IPAddress()
        }, ex);
      }
    }

    public bool AddDiscoveryIgnoredNodesNewDiscovery(IEnumerable<DiscoveredNode> discoveredNodes)
    {
      IEnumerable<IScheduledDiscoveryIgnore> list = (IEnumerable<IScheduledDiscoveryIgnore>) ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().ToList<IScheduledDiscoveryIgnore>();
      IDictionary<int, int> dictionary = (IDictionary<int, int>) new Dictionary<int, int>();
      bool flag = ((IModuleManager) SolarWinds.Orion.Core.Common.ModuleManager.ModuleManager.InstanceWithCache).IsThereModule("NPM");
      using (IEnumerator<DiscoveredNode> enumerator1 = discoveredNodes.GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          DiscoveredNode current1 = enumerator1.Current;
          CoreBusinessLayerService.log.Debug((object) "Sending request for insert ignored node to DAL.");
          try
          {
            int engineId;
            if (dictionary.ContainsKey(current1.get_ProfileID()))
            {
              engineId = dictionary[current1.get_ProfileID()];
            }
            else
            {
              engineId = DiscoveryProfileEntry.GetProfileByID(current1.get_ProfileID()).get_EngineID();
              dictionary[current1.get_ProfileID()] = engineId;
            }
            string stringIp = IPAddressHelper.ToStringIp(current1.get_IP());
            string displayName = ((DiscoveredObjectBase) current1).get_DisplayName();
            int num = DiscoveryIgnoredNodeEntry.Insert(engineId, stringIp, displayName, true, current1.get_ProfileID());
            using (IEnumerator<DiscoveryIgnoredDAL.VolumeInfo> enumerator2 = DiscoveryIgnoredDAL.GetDiscoveredVolumesForNode(current1).GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                DiscoveryIgnoredDAL.VolumeInfo current2 = enumerator2.Current;
                DiscoveryIgnoredVolumeEntry.Insert(engineId, stringIp, displayName, current2.get_VolumeDescription(), current2.get_VolumeType(), current1.get_NodeID(), current1.get_ProfileID(), num);
              }
            }
            if (flag)
            {
              using (IEnumerator<DiscoveryIgnoredDAL.InterfaceInfo> enumerator2 = DiscoveryIgnoredDAL.GetDiscoveredInterfacesForNode(current1).GetEnumerator())
              {
                while (((IEnumerator) enumerator2).MoveNext())
                {
                  DiscoveryIgnoredDAL.InterfaceInfo current2 = enumerator2.Current;
                  DiscoveryIgnoredInterfaceEntry.Insert(engineId, stringIp, displayName, current2.get_PhysicalAddress(), current2.get_InterfaceName(), current2.get_InterfaceName(), current2.get_InterfaceType(), current2.get_IfName(), current1.get_NodeID(), current1.get_ProfileID(), num);
                }
              }
            }
            DiscoveryResultBase discoveryResultBase = new DiscoveryResultBase();
            CoreDiscoveryPluginResult discoveryPluginResult1 = new CoreDiscoveryPluginResult();
            CoreDiscoveryPluginResult discoveryPluginResult2 = discoveryPluginResult1;
            List<DiscoveredNode> discoveredNodeList = new List<DiscoveredNode>();
            discoveredNodeList.Add(current1);
            discoveryPluginResult2.set_DiscoveredNodes(discoveredNodeList);
            current1.set_IgnoredNodeId(new int?(num));
            discoveryResultBase.get_PluginResults().Add((DiscoveryPluginResultBase) discoveryPluginResult1);
            using (IEnumerator<IScheduledDiscoveryIgnore> enumerator2 = list.GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                IScheduledDiscoveryIgnore current2 = enumerator2.Current;
                DiscoveryPluginResultBase pluginResultBase = ((IScheduledDiscoveryImport) current2).LoadResults(current1.get_ProfileID(), current1.get_NodeID());
                discoveryResultBase.get_PluginResults().Add(pluginResultBase);
                current2.StoreResultsToIgnoreList(discoveryResultBase);
              }
            }
          }
          catch (Exception ex)
          {
            CoreBusinessLayerService.log.Error((object) ("Error when inserting ignored node: " + ex.ToString()));
            throw new CoreBusinessLayerService.DiscoveryInsertingIgnoredNodeError(Resources.get_DiscoveryBL_DiscoveryInsertingIgnoredNodeError(), new object[1]
            {
              (object) IPAddressHelper.ToStringIp(current1.get_IP())
            }, ex);
          }
        }
      }
      return true;
    }

    public bool AddDiscoveryIgnoredNodeNewDiscovery(DiscoveredNode discoveredNode)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for insert ignored node to DAL.");
      try
      {
        int engineId = DiscoveryProfileEntry.GetProfileByID(discoveredNode.get_ProfileID()).get_EngineID();
        string stringIp = IPAddressHelper.ToStringIp(discoveredNode.get_IP());
        string displayName = ((DiscoveredObjectBase) discoveredNode).get_DisplayName();
        int num = DiscoveryIgnoredNodeEntry.Insert(engineId, stringIp, displayName, true, discoveredNode.get_ProfileID());
        bool flag = num != -1;
        using (IEnumerator<DiscoveryIgnoredDAL.VolumeInfo> enumerator = DiscoveryIgnoredDAL.GetDiscoveredVolumesForNode(discoveredNode).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            DiscoveryIgnoredDAL.VolumeInfo current = enumerator.Current;
            DiscoveryIgnoredVolumeEntry.Insert(engineId, stringIp, displayName, current.get_VolumeDescription(), current.get_VolumeType(), discoveredNode.get_NodeID(), discoveredNode.get_ProfileID(), num);
          }
        }
        if (((IModuleManager) SolarWinds.Orion.Core.Common.ModuleManager.ModuleManager.InstanceWithCache).IsThereModule("NPM"))
        {
          using (IEnumerator<DiscoveryIgnoredDAL.InterfaceInfo> enumerator = DiscoveryIgnoredDAL.GetDiscoveredInterfacesForNode(discoveredNode).GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              DiscoveryIgnoredDAL.InterfaceInfo current = enumerator.Current;
              DiscoveryIgnoredInterfaceEntry.Insert(engineId, stringIp, displayName, current.get_PhysicalAddress(), current.get_InterfaceName(), current.get_InterfaceName(), current.get_InterfaceType(), current.get_IfName(), discoveredNode.get_NodeID(), discoveredNode.get_ProfileID(), num);
            }
          }
        }
        DiscoveryResultBase discoveryResultBase = new DiscoveryResultBase();
        CoreDiscoveryPluginResult discoveryPluginResult1 = new CoreDiscoveryPluginResult();
        CoreDiscoveryPluginResult discoveryPluginResult2 = discoveryPluginResult1;
        List<DiscoveredNode> discoveredNodeList = new List<DiscoveredNode>();
        discoveredNodeList.Add(discoveredNode);
        discoveryPluginResult2.set_DiscoveredNodes(discoveredNodeList);
        discoveredNode.set_IgnoredNodeId(new int?(num));
        discoveryResultBase.get_PluginResults().Add((DiscoveryPluginResultBase) discoveryPluginResult1);
        using (IEnumerator<IScheduledDiscoveryIgnore> enumerator = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            IScheduledDiscoveryIgnore current = enumerator.Current;
            DiscoveryPluginResultBase pluginResultBase = ((IScheduledDiscoveryImport) current).LoadResults(discoveredNode.get_ProfileID(), discoveredNode.get_NodeID());
            discoveryResultBase.get_PluginResults().Add(pluginResultBase);
            current.StoreResultsToIgnoreList(discoveryResultBase);
          }
        }
        return flag;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when inserting ignored node: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryInsertingIgnoredNodeError(Resources.get_DiscoveryBL_DiscoveryInsertingIgnoredNodeError(), new object[1]
        {
          (object) IPAddressHelper.ToStringIp(discoveredNode.get_IP())
        }, ex);
      }
    }

    public void DeleteDiscoveryIgnoredNodes(IEnumerable<DiscoveryIgnoredNode> nodes)
    {
      if (nodes == null)
        throw new ArgumentNullException(nameof (nodes));
      IEnumerable<IScheduledDiscoveryIgnore> list = (IEnumerable<IScheduledDiscoveryIgnore>) ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().ToList<IScheduledDiscoveryIgnore>();
      List<int> intList1 = new List<int>();
      List<int> intList2 = new List<int>();
      using (IEnumerator<DiscoveryIgnoredNode> enumerator1 = nodes.GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          DiscoveryIgnoredNode current1 = enumerator1.Current;
          if (current1 != null)
          {
            using (IEnumerator<DiscoveryIgnoredVolume> enumerator2 = ((IEnumerable<DiscoveryIgnoredVolume>) current1.get_IgnoredVolumes()).Where<DiscoveryIgnoredVolume>((Func<DiscoveryIgnoredVolume, bool>) (n => n.get_IsSelected())).GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
                this.DeleteDiscoveryIgnoredVolume(enumerator2.Current);
            }
            using (IEnumerator<DiscoveryIgnoredInterface> enumerator2 = ((IEnumerable<DiscoveryIgnoredInterface>) current1.get_IgnoredInterfaces()).Where<DiscoveryIgnoredInterface>((Func<DiscoveryIgnoredInterface, bool>) (n => n.get_IsSelected())).GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
                this.DeleteDiscoveryIgnoredInterface(enumerator2.Current);
            }
            bool flag = true;
            using (IEnumerator<IScheduledDiscoveryIgnore> enumerator2 = list.GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                IScheduledDiscoveryIgnore current2 = enumerator2.Current;
                current2.RemoveResultsFromIgnoreList(current1.get_NodeResult());
                if (flag && current2.LoadIgnoredResults(current1.get_ID()).GetDiscoveredObjects().Any<IDiscoveredObject>())
                  flag = false;
              }
            }
            if (flag)
              intList2.Add(current1.get_ID());
            else
              intList1.Add(current1.get_ID());
          }
        }
      }
      if (intList2.Count > 0)
        DiscoveryIgnoredNodeEntry.DeleteByListID(intList2);
      if (intList1.Count <= 0)
        return;
      DiscoveryIgnoredNodeEntry.DisableIsIgnoredList(intList1);
    }

    public void DeleteDiscoveryIgnoredNode(DiscoveryIgnoredNode node)
    {
      if (node == null)
        throw new ArgumentNullException(nameof (node));
      using (IEnumerator<DiscoveryIgnoredVolume> enumerator = ((IEnumerable<DiscoveryIgnoredVolume>) node.get_IgnoredVolumes()).Where<DiscoveryIgnoredVolume>((Func<DiscoveryIgnoredVolume, bool>) (n => n.get_IsSelected())).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
          this.DeleteDiscoveryIgnoredVolume(enumerator.Current);
      }
      using (IEnumerator<DiscoveryIgnoredInterface> enumerator = ((IEnumerable<DiscoveryIgnoredInterface>) node.get_IgnoredInterfaces()).Where<DiscoveryIgnoredInterface>((Func<DiscoveryIgnoredInterface, bool>) (n => n.get_IsSelected())).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
          this.DeleteDiscoveryIgnoredInterface(enumerator.Current);
      }
      bool flag = true;
      using (IEnumerator<IScheduledDiscoveryIgnore> enumerator = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryIgnore>().GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IScheduledDiscoveryIgnore current = enumerator.Current;
          current.RemoveResultsFromIgnoreList(node.get_NodeResult());
          if (flag && current.LoadIgnoredResults(node.get_ID()).GetDiscoveredObjects().Any<IDiscoveredObject>())
            flag = false;
        }
      }
      if (flag)
      {
        DiscoveryIgnoredNodeEntry.DeleteByID(node.get_ID());
      }
      else
      {
        if (!node.get_IsSelected() || !node.get_IsIgnored())
          return;
        DiscoveryIgnoredNodeEntry.DisableIsIgnored(node.get_ID());
      }
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public void DeleteDiscoveryIgnoredNode(int ignoredNodeID)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for delete to DAL.");
      try
      {
        DiscoveryIgnoredNodeEntry.DeleteByID(ignoredNodeID);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when deleting ignored node: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryDeletingIgnoredNodeError(Resources.get_DiscoveryBL_DiscoveryDeletingIgnoredNodeError(), new object[1]
        {
          (object) ignoredNodeID
        }, ex);
      }
    }

    public void RemoveDiscoveryNodeFromIgnored(DiscoveryNode discoveryNode)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for delete to DAL.");
      try
      {
        DiscoveryIgnoredNodeEntry.DeleteByKeyColums(discoveryNode.get_EngineID(), discoveryNode.get_IPAddress(), discoveryNode.get_Name());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when deleting ignored node: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryDeletingIgnoredNodeError(Resources.get_DiscoveryBL_DiscoveryDeletingIgnoredNodeError_Engine(), new object[2]
        {
          (object) discoveryNode.get_EngineID(),
          (object) discoveryNode.get_IPAddress()
        }, ex);
      }
    }

    public bool AddDiscoveryIgnoredInterface(
      DiscoveryNode discoveryNode,
      DiscoveryInterface discoveryInterface)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for insert ignored interface to DAL.");
      try
      {
        return DiscoveryIgnoredInterfaceEntry.Insert(discoveryNode.get_EngineID(), discoveryNode.get_IPAddress(), discoveryNode.get_Name(), discoveryInterface.get_PhysicalAddress(), discoveryInterface.get_InterfaceDescription(), discoveryInterface.get_InterfaceCaption(), discoveryInterface.get_InterfaceType(), discoveryInterface.get_IfxName(), discoveryNode.get_NodeID(), discoveryNode.get_ProfileID(), 0);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when inserting ignored interface: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryInsertingIgnoredInterfaceError(Resources.get_DiscoveryBL_DiscoveryInsertingIgnoredInterfaceError(), new object[1]
        {
          (object) discoveryNode.get_IPAddress()
        }, ex);
      }
    }

    public bool DeleteDiscoveryIgnoredInterface(
      DiscoveryIgnoredInterface discoveryIgnoredInterface)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for delete ignored interface to DAL.");
      try
      {
        return DiscoveryIgnoredInterfaceEntry.DeleteByID(discoveryIgnoredInterface.get_ID());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when deleting ignored interface: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryDeletingIgnoredInterfaceError(Resources.get_DiscoveryBL_DiscoveryDeletingIgnoredInterfaceError(), new object[1]
        {
          (object) discoveryIgnoredInterface.get_Description()
        }, ex);
      }
    }

    public bool AddDiscoveryIgnoredVolume(
      DiscoveryNode discoveryNode,
      DiscoveryVolume discoveryVolume)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for insert ignored volume to DAL.");
      try
      {
        return DiscoveryIgnoredVolumeEntry.Insert(discoveryNode.get_EngineID(), discoveryNode.get_IPAddress(), discoveryNode.get_Name(), discoveryVolume.get_VolumeDescription(), (int) discoveryVolume.get_VolumeType(), discoveryNode.get_NodeID(), discoveryNode.get_ProfileID(), 0);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when inserting ignored volume: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryInsertingIgnoredVolumeError(Resources.get_DiscoveryBL_DiscoveryInsertingIgnoredVolumeError(), new object[1]
        {
          (object) discoveryNode.get_IPAddress()
        }, ex);
      }
    }

    public bool DeleteDiscoveryIgnoredVolume(DiscoveryIgnoredVolume discoveryIgnoredVolume)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for delete ignored volume to DAL.");
      try
      {
        return DiscoveryIgnoredVolumeEntry.DeleteByID(discoveryIgnoredVolume.get_ID());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when deleting ignored volume: " + ex.ToString()));
        throw new CoreBusinessLayerService.DiscoveryDeletingIgnoredVolumeError(Resources.get_DiscoveryBL_DiscoveryDeletingIgnoredVolumeError(), new object[1]
        {
          (object) discoveryIgnoredVolume.get_Description()
        }, ex);
      }
    }

    public string ValidateBulkList(IEnumerable<string> bulkList)
    {
      StringBuilder errors = new StringBuilder();
      foreach (string normalizeHostName in HostListNormalizer.NormalizeHostNames(bulkList))
        this.ValidateHostAddress(normalizeHostName, errors);
      return errors.ToString();
    }

    public List<Subnet> FindRouterSubnets(
      string router,
      List<SnmpEntry> credentials,
      int engineId,
      out string errorMessage)
    {
      StringBuilder errors = new StringBuilder();
      if (!this.ValidateHostAddress(router, errors))
      {
        errorMessage = errors.ToString();
        return (List<Subnet>) null;
      }
      errorMessage = (string) null;
      string str = ((IEnumerable<IPAddress>) Dns.GetHostAddresses(router)).FirstOrDefault<IPAddress>((Func<IPAddress, bool>) (ipaddress => ipaddress.AddressFamily == AddressFamily.InterNetwork)).ToString();
      if (str == null)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("IP address for host {0} is missing", (object) router));
        throw new CoreBusinessLayerService.DiscoveryHostAddressMissingError(Resources.get_DiscoveryBL_DiscoveryHostAddressMissingError(), new object[1]
        {
          (object) router
        });
      }
      Dictionary<string, string> dictionary;
      SNMPHelper.SNMPQueryForIp(str, "1.3.6.1.2.1.4.21.1.11", credentials, "getsubtree", ref dictionary);
      List<Subnet> subnetList = new List<Subnet>();
      foreach (KeyValuePair<string, string> keyValuePair in dictionary)
      {
        uint num;
        if (!(keyValuePair.Value == "255.255.255.255") && !(keyValuePair.Value == "0.0.0.0") && (!(keyValuePair.Key == "127.0.0.0") && HostHelper.IsIpAddress(keyValuePair.Key)) && (HostHelper.IsIpAddress(keyValuePair.Value) && Subnet.GetSubnetClass(keyValuePair.Key, ref num) != null))
        {
          Subnet subnet = new Subnet(keyValuePair.Key, keyValuePair.Value);
          subnetList.Add(subnet);
        }
      }
      if (subnetList.Count != 0)
        return subnetList;
      errorMessage = Resources.get_CODE_VB0_1();
      return (List<Subnet>) null;
    }

    [Obsolete("There is no longer a sigle license count value.  Use FeatureManager to get the license limits for each element type.", true)]
    public int GetLicenseCount()
    {
      return 0;
    }

    public void ValidateProfilesTimeout()
    {
      new List<DiscoveryProfileEntry>((IEnumerable<DiscoveryProfileEntry>) DiscoveryProfileEntry.GetAllProfiles()).ForEach((Action<DiscoveryProfileEntry>) (profile =>
      {
        if (!(DateTime.MinValue != profile.get_LastRun()) || ((DateTime.Now - profile.get_LastRun().ToLocalTime()).TotalMinutes <= (double) profile.get_JobTimeout() || profile.get_Status().Status != 1))
          return;
        CoreBusinessLayerService.log.Warn((object) string.Format("Discovery profile {0} end during timeout {1}", (object) profile.get_ProfileID(), (object) profile.get_JobTimeout()));
        profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "LIBCODE_TM0_25"));
        profile.Update();
      }));
    }

    public Intervals GetSettingsPollingIntervals()
    {
      return DiscoveryDAL.GetSettingsPollingIntervals();
    }

    public List<SnmpEntry> GetAllCredentials()
    {
      return DiscoveryDAL.GetAllCredentials();
    }

    internal bool UpdateDiscoveryJobs(int engineId)
    {
      return this.UpdateSelectedDiscoveryJobs((List<int>) null, engineId);
    }

    public void UpdateSelectedDiscoveryJobs(List<int> profileIdsFilter)
    {
      int operationEngineId = this.GetCurrentOperationEngineId();
      this.UpdateSelectedDiscoveryJobs(profileIdsFilter, operationEngineId);
    }

    private bool UpdateSelectedDiscoveryJobs(List<int> profileIdsFilter, int engineId)
    {
      try
      {
        CoreBusinessLayerService.log.Debug((object) "Updating scheduled discovery jobs.");
        if (profileIdsFilter != null && profileIdsFilter.Count == 0)
          return true;
        string errorMessage;
        if (this.TryConnectionWithJobSchedulerV2(out errorMessage))
        {
          ICollection<DiscoveryProfileEntry> allProfiles = DiscoveryProfileEntry.GetAllProfiles();
          CoreBusinessLayerService.log.Debug((object) "Filtering old profiles");
          List<DiscoveryProfileEntry> list = ((IEnumerable<DiscoveryProfileEntry>) allProfiles).Where<DiscoveryProfileEntry>((Func<DiscoveryProfileEntry, bool>) (p => p.get_SIPPort() == 0)).Select<DiscoveryProfileEntry, DiscoveryProfileEntry>((Func<DiscoveryProfileEntry, DiscoveryProfileEntry>) (p => p)).ToList<DiscoveryProfileEntry>();
          if (profileIdsFilter != null)
            list = ((IEnumerable<DiscoveryProfileEntry>) list).Where<DiscoveryProfileEntry>((Func<DiscoveryProfileEntry, bool>) (p => profileIdsFilter.Contains(p.get_ProfileID()))).ToList<DiscoveryProfileEntry>();
          using (List<DiscoveryProfileEntry>.Enumerator enumerator = list.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              DiscoveryProfileEntry current = enumerator.Current;
              DiscoveryConfiguration discoveryConfiguration = ((IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>()).GetDiscoveryConfiguration(current.get_ProfileID());
              if (((DiscoveryConfigurationBase) discoveryConfiguration).get_EngineId() == engineId && current.get_IsScheduled())
                this.UpdateDiscoveryJob(current, discoveryConfiguration);
            }
          }
          return true;
        }
        CoreBusinessLayerService.log.WarnFormat("Can't update scheduled jobs, JobScheduler is not running. - {0}", (object) errorMessage);
        return false;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Unhandled exception occured when rescheduling discovery jobs", ex);
        return false;
      }
    }

    private void UpdateDiscoveryJob(
      DiscoveryProfileEntry profile,
      DiscoveryConfiguration configuration)
    {
      CoreBusinessLayerService.log.DebugFormat("Updating discovery job. ProfileId={0}.", (object) profile.get_ProfileID());
      if (configuration == null)
      {
        CoreBusinessLayerService.log.ErrorFormat("Discovery Configuration wasn't found. ProfileId={0}", (object) profile.get_ProfileID());
      }
      else
      {
        ScheduledJob discoveryJob = this.JobFactory.CreateDiscoveryJob(configuration);
        if (discoveryJob == null)
          return;
        discoveryJob.set_InitialWait(CoreBusinessLayerService.CalculateJobInitialWait(profile));
        CoreBusinessLayerService.log.DebugFormat("Submiting job for profile {0}. CronExpression={1}, Frequency={2}, InitialWait={3}", new object[4]
        {
          (object) profile.get_ProfileID(),
          (object) discoveryJob.get_CronExpression(),
          (object) discoveryJob.get_Frequency(),
          (object) discoveryJob.get_InitialWait()
        });
        Guid localEngine = this.JobFactory.SubmitScheduledJobToLocalEngine(profile.get_JobID(), discoveryJob, false);
        if (localEngine != profile.get_JobID())
        {
          CoreBusinessLayerService.log.DebugFormat("Updating profile, ProfileId={0}.", (object) profile.get_ProfileID());
          profile.set_JobID(localEngine);
          profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 5, string.Empty));
          profile.Update();
        }
        CoreBusinessLayerService.log.DebugFormat("Discovery job was updated successfully, ProfileId={0}.", (object) profile.get_ProfileID());
      }
    }

    private static TimeSpan CalculateJobInitialWait(DiscoveryProfileEntry profile)
    {
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.ToUniversalTime();
      int int32_1 = Convert.ToInt32(dateTime.TimeOfDay.TotalMinutes);
      int minutes = 0;
      if (!profile.get_ScheduleRunAtTime().Equals(DateTime.MinValue))
      {
        int int32_2 = Convert.ToInt32(profile.get_ScheduleRunAtTime().TimeOfDay.TotalMinutes);
        minutes = int32_1 >= int32_2 ? 1440 - (int32_1 - int32_2) : int32_2 - int32_1;
      }
      if (profile.get_ScheduleRunFrequency() != 0)
        minutes = profile.get_ScheduleRunFrequency();
      return new TimeSpan(0, minutes, 0);
    }

    private bool ValidateHostAddress(string hostNameOrAddress, StringBuilder errors)
    {
      try
      {
        Dns.GetHostAddresses(hostNameOrAddress);
        return true;
      }
      catch (ArgumentOutOfRangeException ex)
      {
        errors.AppendFormat(Resources.get_LIBCODE_TM0_12() + "<br />", (object) hostNameOrAddress);
      }
      catch (ArgumentException ex)
      {
        errors.AppendFormat(Resources.get_LIBCODE_TM0_13() + "<br />", (object) hostNameOrAddress);
      }
      catch (Exception ex)
      {
        errors.AppendFormat(Resources.get_LIBCODE_TM0_14() + "<br/>", (object) hostNameOrAddress);
      }
      return false;
    }

    private Dictionary<string, int> ImportOneTimeJobResultInternal(
      DiscoveredObjectTreeWcfWrapper treeOfSelection,
      int nodeId,
      ICollection<IOneTimeJobSupport> pluginsWithOneTimeJobSupport)
    {
      if (treeOfSelection == null)
        throw new ArgumentNullException(nameof (treeOfSelection));
      if (treeOfSelection.get_Tree() == null)
        throw new NullReferenceException("treeOfSelection::Tree");
      if (pluginsWithOneTimeJobSupport == null)
        throw new ArgumentNullException(nameof (pluginsWithOneTimeJobSupport));
      CoreBusinessLayerService.log.DebugFormat("Importing List of Discovered Resources for node with id '{0}'", (object) nodeId);
      Dictionary<string, int> dictionary1 = new Dictionary<string, int>();
      if (!((IEnumerable<IOneTimeJobSupport>) pluginsWithOneTimeJobSupport).Any<IOneTimeJobSupport>())
      {
        CoreBusinessLayerService.log.DebugFormat("There are no discovery plugins to be updated with this result. Skipping.", Array.Empty<object>());
        return dictionary1;
      }
      DateTime now = DateTime.Now;
      Action action = (Action) null;
      if (nodeId > 0)
        TechnologyPollingIndicator.AuditTechnologiesChanges((IEnumerable<IDiscoveredObject>) treeOfSelection.get_Tree().GetAllTreeObjectsOfType<IDiscoveredObjectWithTechnology>(), nodeId);
      using (IEnumerator<IOneTimeJobSupport> enumerator = ((IEnumerable<IOneTimeJobSupport>) pluginsWithOneTimeJobSupport).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IOneTimeJobSupport current = enumerator.Current;
          try
          {
            if (CoreBusinessLayerService.log.get_IsDebugEnabled())
              CoreBusinessLayerService.log.DebugFormat("Updating List of Discovered Resources in plugin '{0}' for node with id '{1}'", (object) ((object) current).GetType(), (object) nodeId);
            Dictionary<string, int> dictionary2 = current.UpdateDiscoveredResourcesManagedStatus(treeOfSelection.get_Tree(), nodeId);
            if (dictionary2 != null)
            {
              if (dictionary2.Count > 0)
              {
                foreach (KeyValuePair<string, int> keyValuePair in dictionary2)
                  dictionary1.Add(keyValuePair.Key, keyValuePair.Value);
              }
            }
          }
          catch (Exception ex)
          {
            CoreBusinessLayerService.log.Error((object) string.Format("Unhandled exception occured when importing one time job result with plugin {0}", (object) ((object) current).GetType()), ex);
          }
        }
      }
      if (action != null)
        action();
      CoreBusinessLayerService.log.DebugFormat("Completed updating of Discovered Resources for node with id '{0}'. Total execution time: {1} ms", (object) nodeId, (object) (DateTime.Now - now).Milliseconds);
      return dictionary1;
    }

    internal void ForceDiscoveryPluginsToLoadTypes()
    {
      CoreBusinessLayerService.log.Debug((object) "Start loading plugins known types");
      IList<IDiscoveryPlugin> discoveryPlugins = DiscoveryHelper.GetOrderedDiscoveryPlugins();
      CoreBusinessLayerService.log.DebugFormat("Number of found plugins:", (object) ((ICollection<IDiscoveryPlugin>) discoveryPlugins).Count);
      System.Type[] array = ((IEnumerable<IDiscoveryPlugin>) discoveryPlugins).SelectMany<IDiscoveryPlugin, System.Type>((Func<IDiscoveryPlugin, IEnumerable<System.Type>>) (plugin => (IEnumerable<System.Type>) plugin.GetKnownTypes())).ToArray<System.Type>();
      CoreBusinessLayerService.log.DebugFormat("Number of found known types:", (object) array.Length);
      CoreBusinessLayerService.log.Debug((object) "Finish loading plugins known types");
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public void StartDiscoveryImport(int profileId)
    {
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public void StoreDiscoveryConfiguration(DiscoveryConfiguration configuration)
    {
      throw new NotImplementedException();
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public DiscoveryConfiguration LoadDiscoveryConfiguration(int profileID)
    {
      throw new NotImplementedException();
    }

    public DiscoveryResultBase GetDiscoveryResult(int profileId)
    {
      return DiscoveryResultManager.GetDiscoveryResult(profileId, DiscoveryHelper.GetOrderedDiscoveryPlugins());
    }

    public DiscoveryImportProgressInfo GetOrionDiscoveryImportProgress(
      Guid importID)
    {
      return DiscoveryImportManager.GetImportProgress(importID);
    }

    public StartImportStatus ImportOrionDiscoveryResults(
      Guid importId,
      DiscoveryResultBase result)
    {
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins = DiscoveryPluginHelper.GetOrderedPlugins(DiscoveryHelper.GetOrderedDiscoveryPlugins(), (IList<DiscoveryPluginInfo>) DiscoveryHelper.GetDiscoveryPluginInfos());
      return DiscoveryImportManager.StartImport(importId, result, orderedPlugins, false, (DiscoveryImportManager.CallbackDiscoveryImportFinished) ((_result, importJobID, StartImportStatus) =>
      {
        try
        {
          DiscoveryLogs discoveryLog = new DiscoveryLogs();
          DiscoveryImportManager.FillDiscoveryLogEntity(discoveryLog, _result, StartImportStatus);
          discoveryLog.set_AutoImport(false);
          using (CoreSwisContext systemContext = SwisContextFactory.CreateSystemContext())
            discoveryLog.Create((SwisContext) systemContext);
          CoreBusinessLayerService.log.InfoFormat("DiscoveryLog created for ProfileID:{0}", (object) discoveryLog.get_ProfileID());
        }
        catch (Exception ex)
        {
          CoreBusinessLayerService.log.Error((object) "Unable to create discovery import log", ex);
        }
      }));
    }

    public int CreateOrionDiscoveryProfileFromConfigurationStrings(
      DiscoveryConfiguration configurationWithoutPluginInformation,
      List<string> discoveryPluginConfigurationBaseItems)
    {
      using (IEnumerator<DiscoveryPluginConfigurationBase> enumerator = this.discoveryLogic.DeserializePluginConfigurationItems(discoveryPluginConfigurationBaseItems).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          DiscoveryPluginConfigurationBase current = enumerator.Current;
          ((DiscoveryConfigurationBase) configurationWithoutPluginInformation).get_PluginConfigurations().Add(current);
        }
      }
      return this.CreateOrionDiscoveryProfile(configurationWithoutPluginInformation, true);
    }

    protected int CreateOrionDiscoveryProfile(
      DiscoveryConfiguration configuration,
      bool refreshCredentialsFromDb)
    {
      CoreBusinessLayerService.log.Debug((object) "Creating new discovery profile.");
      List<int> usedCredentials;
      int newConfiguration = ((IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>()).CreateNewConfiguration(configuration, refreshCredentialsFromDb, ref usedCredentials);
      if (usedCredentials.Count > 0)
        this.RescheduleJobsWithUsingCredentials(usedCredentials, newConfiguration);
      CoreBusinessLayerService.log.DebugFormat("Discovery profile {0} was successfully created.", (object) newConfiguration);
      return newConfiguration;
    }

    public int CreateOrionDiscoveryProfile(DiscoveryConfiguration configuration)
    {
      return this.CreateOrionDiscoveryProfile(configuration, false);
    }

    public void UpdateOrionDiscoveryProfile(DiscoveryConfiguration configuration)
    {
      if (configuration.get_ProfileID().HasValue)
        CoreBusinessLayerService.log.DebugFormat("Updating configuration for profile {0}", (object) configuration.get_ProfileID());
      else
        CoreBusinessLayerService.log.Warn((object) "Trying to update configuration for profile with no ID");
      List<int> usedCredentials = ((IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>()).StoreDiscoveryConfiguration(configuration);
      if (usedCredentials.Count > 0)
        this.RescheduleJobsWithUsingCredentials(usedCredentials, ((DiscoveryConfigurationBase) configuration).get_ProfileId().Value);
      CoreBusinessLayerService.log.DebugFormat("Configuration for profile {0} updated.", (object) configuration.get_ProfileID());
    }

    private void RescheduleJobsWithUsingCredentials(
      List<int> usedCredentials,
      int excludedProfileId)
    {
      List<int> list = this.GetProfileIDsUsingCredentials(usedCredentials).Where<int>((Func<int, bool>) (id => id != excludedProfileId)).ToList<int>();
      CoreBusinessLayerService.log.InfoFormat("Rescheduling discovery profiles [{0}] because credential it is refefencing were changed.", (object) string.Join(", ", list.Select<int, string>((Func<int, string>) (id => id.ToString())).ToArray<string>()));
      new DiscoveryJobRescheduler().RescheduleJobsForProfiles(list);
    }

    public List<int> GetProfileIDsUsingCredentials(List<int> credentialIdList)
    {
      if (credentialIdList == null)
        throw new ArgumentNullException(nameof (credentialIdList));
      List<int> result = new List<int>();
      if (credentialIdList.Count == 0)
        return result;
      IDiscoveryDAL service = (IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>();
      List<int> allProfileIds = service.GetAllProfileIDs();
      List<DiscoveryConfiguration> discoveryConfigurationList = new List<DiscoveryConfiguration>();
      foreach (int num in allProfileIds)
        discoveryConfigurationList.Add(service.GetDiscoveryConfiguration(num));
      using (List<DiscoveryConfiguration>.Enumerator enumerator1 = discoveryConfigurationList.GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          DiscoveryConfiguration configuration = enumerator1.Current;
          using (IEnumerator<ICredentialStorage> enumerator2 = ((IEnumerable) ((DiscoveryConfigurationBase) configuration).get_PluginConfigurations()).OfType<ICredentialStorage>().GetEnumerator())
          {
            while (((IEnumerator) enumerator2).MoveNext())
              enumerator2.Current.GetCredentialList().ToList<int>().ForEach((Action<int>) (c =>
              {
                if (!credentialIdList.Contains(c))
                  return;
                List<int> intList1 = result;
                int? profileId = ((DiscoveryConfigurationBase) configuration).get_ProfileId();
                int num1 = profileId.Value;
                if (intList1.Contains(num1))
                  return;
                List<int> intList2 = result;
                profileId = ((DiscoveryConfigurationBase) configuration).get_ProfileId();
                int num2 = profileId.Value;
                intList2.Add(num2);
              }));
          }
        }
      }
      return result;
    }

    public void DeleteOrionDiscoveryProfile(int profileID)
    {
      this.discoveryLogic.DeleteOrionDiscoveryProfile(profileID);
    }

    public void DeleteHiddenOrionDiscoveryProfilesByName(string profileName)
    {
      this.discoveryLogic.DeleteHiddenOrionDiscoveryProfilesByName(profileName);
    }

    public DiscoveryConfiguration GetOrionDiscoveryConfigurationByProfile(
      int profileID)
    {
      return ((IDiscoveryDAL) this.ServiceContainer.GetService<IDiscoveryDAL>()).GetDiscoveryConfiguration(profileID);
    }

    public void CancelOrionDiscovery(int profileID)
    {
      DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(profileID);
      if (profileById.get_Status().Status != 1 || !(profileById.get_JobID() != Guid.Empty))
        return;
      using (IJobSchedulerHelper localInstance = JobScheduler.GetLocalInstance())
      {
        CoreBusinessLayerService.log.DebugFormat("Checking if job {0} exists in is really running", (object) profileById.get_JobID());
        OrionDiscoveryJobProgressInfo progressInfo = OrionDiscoveryJobSchedulerEventsService.GetProgressInfo(profileID);
        if (progressInfo == null)
        {
          string str = "An error has occurred during Network Discovery cancellation: there are no Network Discovery jobs to cancel.\r\nThis error may be due to either a lost database connection or a business layer fault condition.";
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "WEBDATA_TP0_ERROR_DURING_DISCOVERY"));
          profileById.Update();
          CoreBusinessLayerService.log.ErrorFormat("Job {0}: {1}", (object) profileById.get_JobID(), (object) str);
          throw new CoreBusinessLayerService.DiscoveryJobCancellationError(Resources.get_DiscoveryBL_DiscoveryJobCancellationError(), new object[1]
          {
            (object) profileById.get_JobID()
          });
        }
        CoreBusinessLayerService.log.DebugFormat("Cancelling job {0}", (object) profileById.get_JobID());
        try
        {
          OrionDiscoveryJobSchedulerEventsService.CancelDiscoveryJob(profileById.get_ProfileID());
          ((IJobScheduler) localInstance).CancelJob(profileById.get_JobID());
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 7, "WEBDATA_TP0_DISCOVERY_CANCELLED_BY_USER"));
          profileById.Update();
        }
        catch (Exception ex)
        {
          string str = "An error has occurred during Network Discovery cancellation: " + ex.Message;
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "WEBDATA_TP0_ERROR_DURING_DISCOVER_NO_JOB"));
          profileById.Update();
          progressInfo?.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "WEBDATA_TP0_ERROR_DURING_DISCOVER_NO_JOB"));
          CoreBusinessLayerService.log.ErrorFormat("Job {0}: {1}", (object) profileById.get_JobID(), (object) str);
          throw;
        }
      }
    }

    public bool TryConnectionWithJobSchedulerV2(out string errorMessage)
    {
      try
      {
        using (IJobSchedulerHelper localInstance = JobScheduler.GetLocalInstance())
        {
          ((IJobScheduler) localInstance).PolicyExists("Nothing");
          errorMessage = string.Empty;
          return true;
        }
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("{0}: {1}", (object) ex.GetType().Name, (object) ex.Message);
        return false;
      }
    }

    public List<SnmpCredentialsV2> GetSharedSnmpV2Credentials(string owner)
    {
      List<SnmpCredentialsV2> snmpCredentialsV2List = new List<SnmpCredentialsV2>();
      try
      {
        snmpCredentialsV2List = ((IEnumerable<SnmpCredentialsV2>) new CredentialManager().GetCredentials<SnmpCredentialsV2>(owner)).ToList<SnmpCredentialsV2>();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Unhandled exception occured when loading shared credentials.", ex);
      }
      return snmpCredentialsV2List;
    }

    public List<SnmpCredentialsV3> GetSharedSnmpV3Credentials(string owner)
    {
      List<SnmpCredentialsV3> snmpCredentialsV3List = new List<SnmpCredentialsV3>();
      try
      {
        snmpCredentialsV3List = ((IEnumerable<SnmpCredentialsV3>) new CredentialManager().GetCredentials<SnmpCredentialsV3>(owner)).ToList<SnmpCredentialsV3>();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Unhandled exception occured when loading shared credentials.", ex);
      }
      return snmpCredentialsV3List;
    }

    public List<UsernamePasswordCredential> GetSharedWmiCredentials(
      string owner)
    {
      List<UsernamePasswordCredential> passwordCredentialList = new List<UsernamePasswordCredential>();
      try
      {
        passwordCredentialList = ((IEnumerable<UsernamePasswordCredential>) new CredentialManager().GetCredentials<UsernamePasswordCredential>(owner)).ToList<UsernamePasswordCredential>();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Unhandled exception occured when loading shared credentials.", ex);
      }
      return passwordCredentialList;
    }

    public Dictionary<string, int> GetElementsManagedCount()
    {
      return LicenseSaturationLogic.GetElementsManagedCount();
    }

    public Dictionary<string, int> GetAlreadyManagedElementCount(
      List<DiscoveryResultBase> discoveryResults,
      IList<IDiscoveryPlugin> plugins)
    {
      Dictionary<string, int> source = new Dictionary<string, int>();
      using (IEnumerator<IDiscoveryPlugin> enumerator = ((IEnumerable<IDiscoveryPlugin>) plugins).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveryPlugin current = enumerator.Current;
          if (current is IGetManagedElements)
          {
            Dictionary<string, int> pluginAlreadyManaged = ((IGetManagedElements) current).GetAlreadyManagedElements(discoveryResults);
            source = pluginAlreadyManaged.Concat<KeyValuePair<string, int>>(source.Where<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (kvp => !pluginAlreadyManaged.ContainsKey(kvp.Key)))).ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (v => v.Key.ToLower()), (Func<KeyValuePair<string, int>, int>) (v => v.Value));
          }
        }
      }
      return source;
    }

    public Dictionary<string, int> GetAlreadyManagedElementCount(
      List<DiscoveryResultBase> discoveryResults)
    {
      IList<IDiscoveryPlugin> discoveryPlugins = DiscoveryHelper.GetOrderedDiscoveryPlugins();
      return this.GetAlreadyManagedElementCount(discoveryResults, discoveryPlugins);
    }

    public DiscoveryResultBase FilterIgnoredItems(
      DiscoveryResultBase discoveryResult)
    {
      return this.discoveryLogic.FilterIgnoredItems(discoveryResult);
    }

    public List<DiscoveryResult> ConvertScheduledDiscoveryResults(
      List<DiscoveryResult> scheduledResults)
    {
      List<DiscoveryResult> discoveryResultList = new ScheduledDiscoveryResultConvertor().ConvertScheduledDiscoveryResults(scheduledResults);
      using (List<DiscoveryResult>.Enumerator enumerator1 = discoveryResultList.GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          DiscoveryResult current1 = enumerator1.Current;
          DiscoveryFilterResultByTechnology.FilterByPriority((DiscoveryResultBase) current1, TechnologyManager.Instance);
          List<DiscoveryPluginResultBase> list = ((IEnumerable<DiscoveryPluginResultBase>) ((DiscoveryResultBase) current1).get_PluginResults()).ToList<DiscoveryPluginResultBase>();
          ((DiscoveryResultBase) current1).get_PluginResults().Clear();
          using (List<DiscoveryPluginResultBase>.Enumerator enumerator2 = list.GetEnumerator())
          {
            while (enumerator2.MoveNext())
            {
              DiscoveryPluginResultBase current2 = enumerator2.Current;
              ((DiscoveryResultBase) current1).get_PluginResults().Add(current2.GetFilteredPluginResult());
            }
          }
        }
      }
      return discoveryResultList;
    }

    public void RequestScheduledDiscoveryNetObjectStatusUpdateAsync()
    {
      DiscoveryNetObjectStatusManager.Instance.RequestUpdateAsync((Action) null, TimeSpan.Zero);
    }

    public void ImportDiscoveryResultForProfile(int profileID, bool deleteProfileAfterImport)
    {
      this.discoveryLogic.ImportDiscoveryResultForProfile(profileID, deleteProfileAfterImport, (DiscoveryImportManager.CallbackDiscoveryImportFinished) null, false, new Guid?());
    }

    public Guid ImportDiscoveryResultsForConfiguration(DiscoveryImportConfiguration importCfg)
    {
      Guid importID = Guid.NewGuid();
      ThreadPool.QueueUserWorkItem((WaitCallback) (callback =>
      {
        try
        {
          this.discoveryLogic.ImportDiscoveryResultsForConfiguration(importCfg, importID);
        }
        catch (Exception ex)
        {
          CoreBusinessLayerService.log.Error((object) "Error in ImportDiscoveryResultsForConfiguration", ex);
        }
      }));
      return importID;
    }

    public ValidationResult ValidateActiveDirectoryAccess(
      ActiveDirectoryAccess access)
    {
      if (access == null)
        throw new ArgumentNullException(nameof (access));
      if (access.get_Credential() == null)
      {
        UsernamePasswordCredential credential = (UsernamePasswordCredential) new CredentialManager().GetCredential<UsernamePasswordCredential>(access.get_CredentialID());
        access.set_Credential(credential);
      }
      return new ActiveDirectoryDiscovery(access.get_HostName(), access.get_Credential()).ValidateConnection();
    }

    public List<OrganizationalUnit> GetActiveDirectoryOrganizationUnits(
      ActiveDirectoryAccess access)
    {
      return this.GetActiveDirectoryDiscovery(access).GetAllOrganizationalUnits().ToList<OrganizationalUnit>();
    }

    public List<OrganizationalUnitCountOfComputers> GetCountOfComputers(
      ActiveDirectoryAccess access)
    {
      return this.GetActiveDirectoryDiscovery(access).GetCountOfStationsInAD();
    }

    private ActiveDirectoryDiscovery GetActiveDirectoryDiscovery(
      ActiveDirectoryAccess access)
    {
      if (access == null)
        throw new ArgumentNullException(nameof (access));
      if (access.get_Credential() == null)
      {
        UsernamePasswordCredential credential = (UsernamePasswordCredential) new CredentialManager().GetCredential<UsernamePasswordCredential>(access.get_CredentialID());
        access.set_Credential(credential);
      }
      return new ActiveDirectoryDiscovery(access.get_HostName(), access.get_Credential());
    }

    public IPAddress GetHostAddress(string hostName, AddressFamily preferredAddressFamily)
    {
      GetHostAddressJobResult result = this.ExecuteJobAndGetResult<GetHostAddressJobResult>(GetHostAddressJob.CreateJobDescription(hostName, BusinessLayerSettings.Instance.TestJobTimeout), (CredentialBase) null, JobResultDataFormatType.Xml, "GetHostAddressJob", out string _);
      if (!((TestJobResult) result).get_Success())
        throw new ResolveHostAddressException("Can not resolve IP address for host " + hostName + ".");
      IPAddress hostAddress = CommonHelper.GetHostAddress(result.get_IpAddresses().Select<string, IPAddress>((Func<string, IPAddress>) (item => IPAddress.Parse(item))), preferredAddressFamily, (IPAddress) null);
      CoreBusinessLayerService.log.InfoFormat(string.Format("IPAddress for host {0} is {1}", (object) hostName, (object) hostAddress), Array.Empty<object>());
      return hostAddress;
    }

    public void CleanOneTimeJobResults()
    {
      using (IEnumerator<IOneTimeJobCleanup> enumerator = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IOneTimeJobCleanup>().GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IOneTimeJobCleanup current = enumerator.Current;
          try
          {
            CoreBusinessLayerService.log.DebugFormat("Cleaning one time job results within plugin '{0}'", (object) ((object) current).GetType());
            current.CleanOneTimeJobResults();
          }
          catch (Exception ex)
          {
            CoreBusinessLayerService.log.Error((object) string.Format("Exception occured when cleaning one time job results within plugin {0}", (object) ((object) current).GetType()), ex);
          }
        }
      }
    }

    public Dictionary<string, Dictionary<int, string>> GetEngines()
    {
      return EngineDAL.GetEngines();
    }

    public void DeleteEngine(int engineID)
    {
      EngineDAL.DeleteEngine(engineID);
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public string GetCurrentEngineIPAddress()
    {
      return EngineDAL.GetEngineIpAddressByServerName(Environment.MachineName);
    }

    public DataTable GetEventTypesTable()
    {
      return EventsWebDAL.GetEventTypesTable();
    }

    public DataTable GetEventsTable(GetEventsParameter param)
    {
      return EventsWebDAL.GetEventsTable(param);
    }

    public DataTable GetEvents(GetEventsParameter param)
    {
      return EventsWebDAL.GetEvents(param);
    }

    public void AcknowledgeEvents(List<int> events)
    {
      EventsWebDAL.AcknowledgeEvents(events);
    }

    public DataTable GetEventSummaryTable(
      int netObjectID,
      string netObjectType,
      DateTime fromDate,
      DateTime toDate,
      List<int> limitationIDs)
    {
      return EventsWebDAL.GetEventSummaryTable(netObjectID, netObjectType, fromDate, toDate, limitationIDs);
    }

    public bool Blow(bool generateException, string exceptionType, string message)
    {
      if (!generateException)
        return true;
      Exception exception;
      if (CoreBusinessLayerService.TryGetException(exceptionType, message, out exception))
        throw exception;
      return false;
    }

    private static bool TryGetException(
      string exceptionType,
      string message,
      out Exception exception)
    {
      try
      {
        if (string.Equals(exceptionType, "SolarWinds.Orion.Core.Common.CoreFaultContract", StringComparison.OrdinalIgnoreCase))
        {
          exception = (Exception) MessageUtilities.NewFaultException<CoreFaultContract>((Exception) new ApplicationException(message));
        }
        else
        {
          System.Type type = System.Type.GetType(exceptionType);
          exception = (Exception) Activator.CreateInstance(type, (object) message);
        }
        return true;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Failed to fake exception. Returning false.", ex);
        exception = (Exception) null;
        return false;
      }
    }

    public List<JobEngineInfo> EnumerateJobEngines()
    {
      return JobEngineDAL.EnumerateJobEngine();
    }

    public JobEngineInfo GetEngine(int engineId)
    {
      return JobEngineDAL.GetEngine(engineId);
    }

    [Obsolete("Use GetEngine method. Obsolete since Core 2018.2 Pacman.")]
    public JobEngineInfo GetEngineWithPollingSettings(int engineId)
    {
      return JobEngineDAL.GetEngineWithPollingSettings(engineId);
    }

    public int GetEngineIdForNetObject(string netObject)
    {
      return JobEngineDAL.GetEngineIdForNetObject(netObject);
    }

    internal INotification RemoveNetObjectInternal(string netobject)
    {
      if (string.IsNullOrEmpty(netobject))
        return (INotification) null;
      string[] strArray = netobject.Split(':');
      if (strArray.Length != 2)
        return (INotification) null;
      try
      {
        int result;
        if (int.TryParse(strArray[1], out result))
        {
          if (strArray[0].Equals("N", StringComparison.OrdinalIgnoreCase))
          {
            using (IEnumerator<Volume> enumerator = ((Collection<int, Volume>) VolumeDAL.GetNodeVolumes(result)).GetEnumerator())
            {
              while (((IEnumerator) enumerator).MoveNext())
                this.DeleteVolume(enumerator.Current);
            }
            this.DeleteNode(result);
          }
          else if (strArray[0].Equals("V", StringComparison.OrdinalIgnoreCase))
          {
            Volume volume = VolumeDAL.GetVolume(result);
            if (volume != null)
              this.DeleteVolume(volume);
          }
          else if (strArray[0].Equals("I", StringComparison.OrdinalIgnoreCase))
          {
            if (this.AreInterfacesSupported)
            {
              Interface @interface = new Interface();
              @interface.set_InterfaceID(result);
              return (INotification) new InterfaceNotification(IndicationHelper.GetIndicationType((IndicationType) 1), @interface);
            }
          }
        }
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ex);
        throw;
      }
      return (INotification) null;
    }

    public void RemoveNetObjects(List<string> netObjectIds)
    {
      List<INotification> notifications = new List<INotification>(netObjectIds.Count);
      netObjectIds.ForEach((Action<string>) (nodeId =>
      {
        INotification inotification = this.RemoveNetObjectInternal(nodeId);
        if (inotification == null)
          return;
        notifications.Add(inotification);
      }));
      if (notifications.Count <= 0)
        return;
      PublisherClient.get_Instance().Publish((IReadOnlyCollection<INotification>) notifications);
    }

    public void RemoveNetObject(string netObjectId)
    {
      INotification inotification = this.RemoveNetObjectInternal(netObjectId);
      if (inotification == null)
        return;
      PublisherClient.get_Instance().Publish(inotification);
    }

    private static void BizLayerErrorHandler(Exception ex)
    {
      CoreBusinessLayerService.log.Error((object) "Exception occurred when communication with VIM Business Layer");
    }

    public void PollNow(string netObjectId)
    {
      using (IPollingControllerServiceHelper instance = PollingController.GetInstance())
        ((IPollingControllerService) instance).PollNow(netObjectId);
    }

    public void JobNowByJobKey(string netObjectId, string jobKey)
    {
      JobExecutionCondition executionCondition1 = new JobExecutionCondition();
      executionCondition1.set_EntityIdentifier(netObjectId);
      executionCondition1.set_JobKey(jobKey);
      JobExecutionCondition executionCondition2 = executionCondition1;
      using (IPollingControllerServiceHelper instance = PollingController.GetInstance())
        ((IPollingControllerService) instance).JobNow(executionCondition2);
    }

    public string PollNodeNow(string netObjectId)
    {
      string empty = string.Empty;
      using (IPollingControllerServiceHelper instance = PollingController.GetInstance())
        ((IPollingControllerService) instance).PollNow(netObjectId);
      return empty;
    }

    public void CancelNow(string netObjectId)
    {
      using (IPollingControllerServiceHelper instance = PollingController.GetInstance())
      {
        IPollingControllerServiceHelper controllerServiceHelper = instance;
        JobExecutionCondition executionCondition = new JobExecutionCondition();
        executionCondition.set_JobType((PollerJobType) 7);
        executionCondition.set_EntityIdentifier(netObjectId);
        ((IPollingControllerService) controllerServiceHelper).CancelJob(executionCondition);
      }
    }

    public void Rediscover(string netObjectId)
    {
      using (IPollingControllerServiceHelper instance = PollingController.GetInstance())
        ((IPollingControllerService) instance).RediscoverNow(netObjectId);
    }

    public void RefreshSettingsFromDatabase()
    {
      new SettingsToRegistry().Synchronize();
    }

    public void ApplyPollingIntervals(
      int nodePollInterval,
      int interfacePollInterval,
      int volumePollInterval,
      int rediscoveryInterval)
    {
      CoreBusinessLayerService.log.ErrorFormat("NotImplemented ApplyPollingIntervals", Array.Empty<object>());
    }

    public void ApplyStatPollingIntervals(
      int nodePollInterval,
      int interfacePollInterval,
      int volumePollInterval)
    {
      CoreBusinessLayerService.log.ErrorFormat("NotImplemented ApplyStatPollingIntervals", Array.Empty<object>());
    }

    public int UpdateNodesPollingEngine(int engineId, int[] nodeIds)
    {
      return JobEngineDAL.UpdateNodesPollingEngine(engineId, nodeIds);
    }

    public string GetLicenseSWID()
    {
      IProductLicense[] activeLicenses = ((ILicenseManager) LicenseManager.GetInstance()).GetActiveLicenses(true);
      return (((IEnumerable<IProductLicense>) activeLicenses).FirstOrDefault<IProductLicense>((Func<IProductLicense, bool>) (l => l.get_LicenseType() != 1 && l.get_LicenseType() > 0)) ?? ((IEnumerable<IProductLicense>) activeLicenses).FirstOrDefault<IProductLicense>()).get_CustomerId();
    }

    public bool ActivateOfflineLicense(string fileNamePath)
    {
      if (string.IsNullOrEmpty(fileNamePath))
        throw new ArgumentNullException(nameof (fileNamePath));
      if (!System.IO.File.Exists(fileNamePath))
      {
        CoreBusinessLayerService.log.DebugFormat("File {0} doesn't exists.", (object) fileNamePath);
        return false;
      }
      string str = System.IO.File.ReadAllText(fileNamePath);
      using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetSystemCreator()).Create())
      {
        PropertyBag propertyBag = (PropertyBag) iinformationServiceProxy2.Invoke<PropertyBag>("Orion.Licensing.Licenses", "ActivateOffline", new object[1]
        {
          (object) str
        });
        return ((Dictionary<string, object>) propertyBag).ContainsKey("Success") && Convert.ToBoolean(((Dictionary<string, object>) propertyBag)["Success"]);
      }
    }

    public Oid GetOid(string oidValue)
    {
      return this.mibDAL.GetOid(oidValue);
    }

    public bool IsMibDatabaseAvailable()
    {
      return this.mibDAL.IsMibDatabaseAvailable();
    }

    public Oids GetChildOids(string parentOid)
    {
      return this.mibDAL.GetChildOids(parentOid);
    }

    public MemoryStream GetIcon(string oid)
    {
      return this.mibDAL.GetIcon(oid);
    }

    public Dictionary<string, MemoryStream> GetIcons()
    {
      return this.mibDAL.GetIcons();
    }

    public Oids GetSearchingOidsByDescription(string searchCriteria, string searchMIBsCriteria)
    {
      return this.mibDAL.GetSearchingOidsByDescription(searchCriteria, searchMIBsCriteria);
    }

    public Oids GetSearchingOidsByName(string searchCriteria)
    {
      return this.mibDAL.GetSearchingOidsByName(searchCriteria);
    }

    public void CancelRunningCommand()
    {
      this.mibDAL.CancelRunningCommand();
    }

    public bool IsModuleInstalled(string moduleTag)
    {
      return ModulesCollector.IsModuleInstalled(moduleTag);
    }

    public bool IsModuleInstalledbyTabName(string moduleTabName)
    {
      return ModulesCollector.IsModuleInstalledbyTabName(moduleTabName);
    }

    public List<ModuleInfo> GetInstalledModules()
    {
      return ModulesCollector.GetInstalledModules();
    }

    public List<ModuleLicenseInfo> GetModuleLicenseInformation()
    {
      return ModuleLicenseInfoProvider.GetModuleLicenseInformation();
    }

    public Version GetModuleVersion(string moduleTag)
    {
      return ModulesCollector.GetModuleVersion(moduleTag);
    }

    public List<ModuleLicenseSaturationInfo> GetModuleSaturationInformation()
    {
      return LicenseSaturationLogic.GetModulesSaturationInfo(new int?(SolarWinds.Orion.Core.BusinessLayer.Settings.LicenseSaturationPercentage));
    }

    public List<SolarWinds.Orion.Core.Common.Models.Node> GetNetworkDevices(
      CorePageType pageType,
      List<int> limitationIDs)
    {
      return NetworkDeviceDAL.Instance.GetNetworkDevices(pageType, limitationIDs);
    }

    public Dictionary<int, string> GetNetworkDeviceNamesForPage(
      CorePageType pageType,
      List<int> limitationIDs)
    {
      return NetworkDeviceDAL.Instance.GetNetworkDeviceNamesForPage(pageType, limitationIDs);
    }

    public Dictionary<int, string> GetDeviceNamesForPage(
      CorePageType pageType,
      List<int> limitationIDs,
      bool includeBasic)
    {
      return NetworkDeviceDAL.Instance.GetNetworkDeviceNamesForPage(pageType, limitationIDs, includeBasic);
    }

    public Dictionary<string, string> GetNetworkDeviceTypes(List<int> limitationIDs)
    {
      return NetworkDeviceDAL.Instance.GetNetworkDeviceTypes(limitationIDs);
    }

    public List<string> GetAllVendors(List<int> limitationIDs)
    {
      return NetworkDeviceDAL.Instance.GetAllVendors(limitationIDs);
    }

    private static MaintenanceRenewalItem DalToWfc(
      MaintenanceRenewalItemDAL dal)
    {
      return dal == null ? (MaintenanceRenewalItem) null : new MaintenanceRenewalItem(dal.Id, dal.Title, dal.Description, dal.CreatedAt, dal.Ignored, dal.Url, dal.AcknowledgedAt, dal.AcknowledgedBy, dal.ProductTag, dal.DateReleased, dal.NewVersion);
    }

    public MaintenanceRenewalItem GetMaintenanceRenewalNotificationItem(
      Guid renewalId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for MaintenanceRenewalItemDAL.GetItemById.");
      try
      {
        return CoreBusinessLayerService.DalToWfc(MaintenanceRenewalItemDAL.GetItemById(renewalId));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining maintenance renewal notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_22(), (object) renewalId));
      }
    }

    public List<MaintenanceRenewalItem> GetMaintenanceRenewalNotificationItems(
      bool includeIgnored)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for MaintenanceRenewalItemDAL.GetItems.");
      try
      {
        List<MaintenanceRenewalItem> maintenanceRenewalItemList = new List<MaintenanceRenewalItem>();
        foreach (MaintenanceRenewalItemDAL dal in (IEnumerable<MaintenanceRenewalItemDAL>) MaintenanceRenewalItemDAL.GetItems(new MaintenanceRenewalFilter(true, includeIgnored, (string) null)))
          maintenanceRenewalItemList.Add(CoreBusinessLayerService.DalToWfc(dal));
        return maintenanceRenewalItemList;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when obtaining maintenance renewals notification items: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_23());
      }
    }

    public MaintenanceRenewalItem GetLatestMaintenanceRenewalItem()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for MaintenanceRenewalItemDAL.GetLatestItem.");
      try
      {
        return CoreBusinessLayerService.DalToWfc(MaintenanceRenewalItemDAL.GetLatestItem((NotificationItemFilter) new MaintenanceRenewalFilter(true, false, (string) null)));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error when obtaining maintenance renewals notification item: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_23());
      }
    }

    public MaintenanceRenewalsCheckStatus GetMaintenanceRenewalsCheckStatus()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for MaintenanceRenewalsCheckStatusDAL.GetCheckStatus.");
      try
      {
        MaintenanceRenewalsCheckStatusDAL checkStatus = MaintenanceRenewalsCheckStatusDAL.GetCheckStatus();
        return checkStatus == null ? (MaintenanceRenewalsCheckStatus) null : new MaintenanceRenewalsCheckStatus(checkStatus.LastUpdateCheck, checkStatus.NextUpdateCheck);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining maintenance renewals status: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_20());
      }
    }

    public void ForceMaintenanceRenewalsCheck()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for CoreHelper.CheckMaintenanceRenewals.");
      try
      {
        CoreHelper.CheckMaintenanceRenewals(false);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error while checking maintenance renewals: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_21());
      }
    }

    public Nodes GetAllNodes()
    {
      return NodeBLDAL.GetNodes();
    }

    public List<int> GetSortedNodeIDs()
    {
      return NodeBLDAL.GetSortedNodeIDs();
    }

    public List<string> GetNodeFields()
    {
      return NodeBLDAL.GetFields();
    }

    public List<string> GetCustomPropertyFields(bool includeMemo)
    {
      return new List<string>((IEnumerable<string>) CustomPropertyMgr.GetPropNamesForTable("NodesCustomProperties", includeMemo));
    }

    public Dictionary<string, string> GetVendors()
    {
      return NodeBLDAL.GetVendors();
    }

    public void DeleteNode(int nodeId)
    {
      SolarWinds.Orion.Core.Common.Models.Node node1 = this.GetNode(nodeId);
      if (node1 == null)
        return;
      Dictionary<string, object> nodeInfo = CoreBusinessLayerService.GetNodeInfo(node1);
      NodeBLDAL.DeleteNode(node1);
      NodeSettingsDAL.DeleteNodeSettings(nodeId);
      NodeNotesDAL.DeleteNodeNotes(nodeId);
      string indicationType = IndicationHelper.GetIndicationType((IndicationType) 1);
      SolarWinds.Orion.Core.Common.Models.Node node2 = new SolarWinds.Orion.Core.Common.Models.Node();
      node2.set_Id(nodeId);
      node2.set_Caption(nodeInfo["DisplayName"].ToString());
      node2.set_IpAddress(node1.get_IpAddress());
      node2.set_Status(node1.get_Status());
      string accountId = AccountContext.GetAccountID();
      NodeNotification nodeNotification = new NodeNotification(indicationType, node2, accountId);
      foreach (KeyValuePair<string, object> keyValuePair in nodeInfo)
        ((Dictionary<string, object>) nodeNotification.get_NodeProperties())[keyValuePair.Key] = keyValuePair.Value;
      PublisherClient.get_Instance().Publish((INotification) nodeNotification);
    }

    private static Dictionary<string, object> GetNodeInfo(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return new SwisEntityHelper(CoreBusinessLayerService.CreateProxy()).GetProperties("Orion.Nodes", node.get_ID(), new string[2]
      {
        "DisplayName",
        "Uri"
      });
    }

    [Obsolete("This is a temporary solution. Don't use this method in modules")]
    public SolarWinds.Orion.Core.Common.Models.Node InsertNodeWithFaultContract(
      SolarWinds.Orion.Core.Common.Models.Node node,
      bool allowDuplicates,
      bool reportIndication)
    {
      try
      {
        return this.InsertNode(node, allowDuplicates, reportIndication);
      }
      catch (LicenseException ex)
      {
        throw MessageUtilities.NewFaultException<CoreFaultContract>((Exception) ex);
      }
    }

    [Obsolete("This is a temporary solution. Don't use this method in modules")]
    public SolarWinds.Orion.Core.Common.Models.Node InsertNodeWithFaultContract(
      SolarWinds.Orion.Core.Common.Models.Node node,
      bool allowDuplicates)
    {
      return this.InsertNodeWithFaultContract(node, allowDuplicates, true);
    }

    [Obsolete("This is a temporary solution. Don't use this method in modules")]
    public SolarWinds.Orion.Core.Common.Models.Node InsertNodeWithFaultContract(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return this.InsertNode(node, false);
    }

    public SolarWinds.Orion.Core.Common.Models.Node InsertNode(
      SolarWinds.Orion.Core.Common.Models.Node node,
      bool allowDuplicates,
      bool reportIndication)
    {
      int maxElementCount = new FeatureManager().GetMaxElementCount(WellKnownElementTypes.get_Nodes());
      if (NodeBLDAL.GetNodeCount() >= maxElementCount)
        throw LicenseException.FromElementsExceeded(maxElementCount);
      node = NodeBLDAL.InsertNode(node, allowDuplicates);
      NodeBLDAL.PopulateWebCommunityStrings();
      if (!reportIndication)
        return node;
      PublisherClient.get_Instance().Publish((INotification) new NodeNotification(IndicationHelper.GetIndicationType((IndicationType) 0), node, AccountContext.GetAccountID()));
      return node;
    }

    public SolarWinds.Orion.Core.Common.Models.Node InsertNode(SolarWinds.Orion.Core.Common.Models.Node node, bool allowDuplicates)
    {
      return this.InsertNode(node, allowDuplicates, true);
    }

    public SolarWinds.Orion.Core.Common.Models.Node InsertNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return this.InsertNode(node, false);
    }

    public SolarWinds.Orion.Core.Common.Models.Node InsertNodeAndGenericPollers(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return this.nodeHelper.InsertNodeAndGenericPollers(node);
    }

    public void AddPollersForNode(int nodeId, string[] newPollers)
    {
      if (nodeId <= 0)
        throw new ArgumentException("Argument nodeId cannot be less then or equal zero");
      SolarWinds.Orion.Core.Common.Models.Node node = NodeDAL.GetNode(nodeId);
      if (node == null)
        throw new ArgumentException("Node doesn't exist");
      if (newPollers == null || newPollers.Length == 0)
        return;
      this.nodeHelper.AddPollersForNode(node, newPollers);
    }

    public void AddBasicPollersForNode(int nodeId, NodeSubType nodeSubtType)
    {
      if (nodeId <= 0)
        throw new ArgumentException(nameof (nodeId));
      SolarWinds.Orion.Core.Common.Models.Node node = NodeDAL.GetNode(nodeId);
      if (node == null)
        throw new ArgumentException("Node doesn't exist");
      this.nodeHelper.AddBasicPollersForNode(node);
    }

    public void RemoveBasicPollersForNode(int nodeId, NodeSubType nodeSubType)
    {
      CoreBusinessLayerService.log.DebugFormat("Removing basic pollers for NodeID = {0}, SubType = {1} ....", (object) nodeId, (object) nodeSubType);
      PollersDAL pollersDal = new PollersDAL();
      List<string> stringList = new List<string>();
      stringList.AddRange((IEnumerable<string>) NodeResponseTimeIcmpPoller.SubPollerTypes);
      if (nodeSubType != 2)
      {
        if (nodeSubType == 1)
        {
          stringList.Add("N.Details.SNMP.Generic");
          stringList.Add("N.Uptime.SNMP.Generic");
        }
        else if (nodeSubType == 3)
        {
          stringList.Add((string) NodeDetailsPollerGeneric.PollerType);
          stringList.Add((string) NodeUptimePollerGeneric.PollerType);
        }
      }
      int num = nodeId;
      string[] array = stringList.ToArray();
      pollersDal.Delete("N", num, array);
      CoreBusinessLayerService.log.DebugFormat("Basic pollers count = {0}, removed for NodeID = {1}, SubType = {2}", (object) stringList.Count, (object) nodeId, (object) nodeSubType);
    }

    public void UpdateNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      DateTime utcNow = DateTime.UtcNow;
      if (utcNow > node.get_UnManageFrom() && utcNow < node.get_UnManageUntil())
      {
        node.set_UnManaged(true);
        node.set_Status("9");
        node.set_PolledStatus(9);
        node.set_GroupStatus("Unmanaged.gif");
        node.set_StatusDescription("Node status is Unmanaged.");
      }
      else if (node.get_Status().Trim() == "9")
      {
        node.set_UnManaged(false);
        node.set_Status("0");
        node.set_PolledStatus(0);
        node.set_GroupStatus("Unknown.gif");
        node.set_StatusDescription("Node status is Unknown.");
      }
      if (node.get_UnPluggable())
      {
        node.set_Status("11");
        node.set_PolledStatus(11);
        node.set_GroupStatus("External.gif");
        node.set_StatusDescription("Node status is External.");
      }
      else if (node.get_Status().Trim() == "11")
      {
        node.set_Status("0");
        node.set_PolledStatus(0);
        node.set_GroupStatus("Unknown.gif");
        node.set_StatusDescription("Node status is Unknown.");
      }
      SolarWinds.Orion.Core.Common.Models.Node node1 = NodeBLDAL.GetNode(node.get_Id());
      NodeBLDAL.UpdateNode(node);
      NodeBLDAL.PopulateWebCommunityStrings();
      NodeNotification nodeNotification = new NodeNotification(IndicationHelper.GetIndicationType((IndicationType) 2), node, node1, AccountContext.GetAccountID());
      if (((Dictionary<string, object>) nodeNotification.get_ChangedProperties()).ContainsKey("ObjectSubType"))
      {
        bool flag = CoreBusinessLayerService.WmiCompatibleNodeSubTypes.Contains(node.get_NodeSubType()) && CoreBusinessLayerService.WmiCompatibleNodeSubTypes.Contains(node1.get_NodeSubType());
        if (!flag)
        {
          using (IEnumerator<Volume> enumerator = ((Collection<int, Volume>) VolumeDAL.GetNodeVolumes(node.get_Id())).GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
              VolumeDAL.DeleteVolume(enumerator.Current);
          }
        }
        PollersDAL pollersDal = new PollersDAL();
        pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "%SNMP%"));
        if (flag)
        {
          pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "N.Uptime.%"));
          pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "N.Details.%"));
        }
        else
          pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "%WMI%"));
        pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "%Agent%"));
        if (node.get_NodeSubType() == 4)
          pollersDal.Delete(new PollerAssignment("N", node.get_ID(), "%ICMP%"));
        if (!(node.get_EntityType() ?? "").Contains(".VIM."))
          NodeBLDAL.UpdateNodeProperty((IDictionary<string, object>) new Dictionary<string, object>()
          {
            {
              "CPULoad",
              (object) -2
            },
            {
              "TotalMemory",
              (object) -2
            },
            {
              "MemoryUsed",
              (object) -2
            },
            {
              "PercentMemoryUsed",
              (object) -2
            },
            {
              "BufferNoMemThisHour",
              (object) -2
            },
            {
              "BufferNoMemToday",
              (object) -2
            },
            {
              "BufferSmMissThisHour",
              (object) -2
            },
            {
              "BufferSmMissToday",
              (object) -2
            },
            {
              "BufferMdMissThisHour",
              (object) -2
            },
            {
              "BufferMdMissToday",
              (object) -2
            },
            {
              "BufferBgMissThisHour",
              (object) -2
            },
            {
              "BufferBgMissToday",
              (object) -2
            },
            {
              "BufferLgMissThisHour",
              (object) -2
            },
            {
              "BufferLgMissToday",
              (object) -2
            },
            {
              "BufferHgMissThisHour",
              (object) -2
            },
            {
              "BufferHgMissToday",
              (object) -2
            }
          }, node.get_ID());
        NodeBLDAL.UpdateNodeProperty((IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "LastBoot",
            (object) null
          },
          {
            "SystemUpTime",
            (object) null
          },
          {
            "LastSystemUpTimePollUtc",
            (object) null
          }
        }, node.get_ID());
      }
      if (!nodeNotification.get_AnyChange())
        return;
      PublisherClient.get_Instance().Publish((INotification) nodeNotification);
    }

    public SolarWinds.Orion.Core.Common.Models.Node GetNode(int nodeId)
    {
      return NodeBLDAL.GetNode(nodeId);
    }

    public bool IsNodeWireless(int nodeId)
    {
      return NodeBLDAL.IsNodeWireless(nodeId);
    }

    public bool IsNodeEnergyWise(int nodeId)
    {
      return NodeBLDAL.IsNodeEnergyWise(nodeId);
    }

    public SolarWinds.Orion.Core.Common.Models.Node GetNodeWithOptions(
      int nodeId,
      bool getInterfaces,
      bool getVolumes)
    {
      return NodeBLDAL.GetNodeWithOptions(nodeId, getInterfaces, getVolumes);
    }

    public Resources ListResources(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return ResourceLister.ListResources(node);
    }

    public Guid BeginListResources(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return ResourceLister.BeginListResources(node);
    }

    public Guid BeginCoreListResources(SolarWinds.Orion.Core.Common.Models.Node node, bool includeInterfaces)
    {
      return ResourceLister.BeginListResources(node, includeInterfaces);
    }

    public ListResourcesStatus GetListResourcesStatus(
      Guid listResourcesOperationId)
    {
      return ResourceLister.GetListResourcesStatus(listResourcesOperationId);
    }

    public float GetAvailability(int nodeID, DateTime startDate, DateTime endDate)
    {
      return NodeBLDAL.GetAvailability(nodeID, startDate, endDate);
    }

    public Dictionary<string, int> GetValuesAndCountsForNodePropertyFiltered(
      string property,
      string accountId,
      Dictionary<string, object> filters)
    {
      return NodeBLDAL.GetValuesAndCountsForPropertyFiltered(property, accountId, filters);
    }

    public Dictionary<string, int> GetValuesAndCountsForNodeProperty(
      string property,
      string accountId)
    {
      return NodeBLDAL.GetValuesAndCountsForProperty(property, accountId);
    }

    public Dictionary<string, int> GetCultureSpecificValuesAndCountsForNodeProperty(
      string property,
      string accountId,
      CultureInfo culture)
    {
      return NodeBLDAL.GetValuesAndCountsForProperty(property, accountId, culture);
    }

    public Nodes GetNodesFiltered(
      Dictionary<string, object> filterValues,
      bool includeInterfaces,
      bool includeVolumes)
    {
      return NodeBLDAL.GetNodesFiltered(filterValues, includeInterfaces, includeVolumes);
    }

    public Nodes GetNodesByIds(int[] nodeIds)
    {
      return NodeBLDAL.GetNodesByIds(nodeIds);
    }

    public Dictionary<string, string> GetVendorIconFileNames()
    {
      return NodeBLDAL.GetVendorIconFileNames();
    }

    public List<string> GetNodeDistinctValuesForField(string fieldName)
    {
      return NodeBLDAL.GetNodeDistinctValuesForField(fieldName);
    }

    public NodeHardwareType GetNodeHardwareType(int nodeId)
    {
      return NodeBLDAL.GetNodeHardwareType(nodeId);
    }

    public void BulkUpdateNodePollingInterval(int pollInterval, int engineId)
    {
      NodeBLDAL.BulkUpdateNodePollingInterval(pollInterval, engineId);
    }

    public Dictionary<string, object> GetNodeCustomProperties(
      int nodeId,
      ICollection<string> properties)
    {
      return NodeBLDAL.GetNodeCustomProperties(nodeId, properties);
    }

    public DataTable GetPagebleNodes(
      string property,
      string type,
      string val,
      string column,
      string direction,
      int number,
      int size,
      string searchText)
    {
      return NodeBLDAL.GetPagebleNodes(property, type, val, column, direction, number, size, searchText);
    }

    public int GetNodesCount(string property, string type, string val, string searchText)
    {
      return NodeBLDAL.GetNodesCount(property, type, val, searchText);
    }

    public DataTable GetGroupsByNodeProperty(string property, string propertyType)
    {
      return NodeBLDAL.GetGroupsByNodeProperty(property, propertyType);
    }

    public void ReflowAllNodeChildStatus()
    {
      NodeChildStatusParticipationDAL.ReflowAllNodeChildStatus();
    }

    public string ResolveNodeNameByIP(string ipAddress)
    {
      return this.ExecuteJobAndGetResult<ResolveHostNameByIpJobResult>(ResolveHostNameByIpJob.CreateJobDescription(ipAddress, BusinessLayerSettings.Instance.TestJobTimeout), (CredentialBase) null, JobResultDataFormatType.Xml, "ResolveHostNameByIpJob", out string _).get_HostName();
    }

    public DataTable GetNodeCPUsByPercentLoad(int nodeId, int pageNumber, int pageSize)
    {
      return NodeBLDAL.GetNodeCPUsByPercentLoad(nodeId, pageNumber, pageSize);
    }

    public DataTable GetNodesCpuIndexCounts(List<string> nodeIds)
    {
      return NodeBLDAL.GetNodesCpuIndexCounts(nodeIds);
    }

    public bool AddNodeNote(
      int nodeId,
      string accountId,
      string note,
      DateTime modificationDateTime)
    {
      return NodeBLDAL.AddNodeNote(nodeId, accountId, note, modificationDateTime);
    }

    public NodeNotesPage GetNodeNotes(PageableNodeNoteRequest request)
    {
      return new NodeNotesDAL().GetNodeNotes(request);
    }

    public void UpdateSpecificSettingForAllNodes(
      string settingName,
      string settingValue,
      string whereClause)
    {
      NodeSettingsDAL.UpdateSpecificSettingForAllNodes(settingName, settingValue, whereClause);
    }

    public int GetAvailableNotificationItemsCountByType(Guid typeId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.GetNotificationsCountByType.");
      try
      {
        return NotificationItemDAL.GetNotificationsCountByType(typeId, new NotificationItemFilter(false, false));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't get notification items count by type: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_8(), (object) typeId));
      }
    }

    public Dictionary<Guid, int> GetAvailableNotificationItemsCounts()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.GetNotificationsCounts.");
      try
      {
        return NotificationItemDAL.GetNotificationsCounts();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't get notification items count for all types: " + (object) ex));
        throw new Exception(Resources.get_LIBCODE_JM0_9());
      }
    }

    public void IgnoreNotificationItem(Guid notificationId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.IgnoreItem.");
      try
      {
        NotificationItemDAL.IgnoreItem(notificationId);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't ignore notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_10(), (object) notificationId));
      }
    }

    public void IgnoreNotificationItems(List<Guid> notificationIds)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.IgnoreItems.");
      try
      {
        NotificationItemDAL.IgnoreItems((ICollection<Guid>) notificationIds);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't ignore multiple notification items: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_JM0_11());
      }
    }

    public void AcknowledgeNotificationItem(
      Guid notificationId,
      string byAccountId,
      DateTime createdBefore)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for .NotificationItemDAL.AcknowledgeItem.");
      try
      {
        NotificationItemDAL.AcknowledgeItem(notificationId, byAccountId, createdBefore);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't acknowledge notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_12(), (object) notificationId, (object) byAccountId));
      }
    }

    public void AcknowledgeNotificationItemsByType(
      Guid typeId,
      string byAccountId,
      DateTime createdBefore)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for .NotificationItemDAL.AcknowledgeItemsByType.");
      try
      {
        NotificationItemDAL.AcknowledgeItemsByType(typeId, byAccountId, createdBefore);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't acknowledge notification items by type: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_13(), (object) typeId, (object) byAccountId));
      }
    }

    public void AcknowledgeAllNotificationItems(string byAccountId, DateTime createdBefore)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.AcknowledgeAllItems.");
      try
      {
        NotificationItemDAL.AcknowledgeAllItems(byAccountId, createdBefore);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't acknowledge all notification items: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_14(), (object) byAccountId));
      }
    }

    public List<HeaderNotificationItem> GetLatestNotificationItemsWithCount()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.GetLatestItemByType.");
      try
      {
        List<HeaderNotificationItem> ret = new List<HeaderNotificationItem>();
        NotificationItemDAL.GetLatestItemsWithCount(new NotificationItemFilter(false, false), (Action<NotificationItemDAL, int>) ((item, count) => ret.Add(new HeaderNotificationItem(item.Id, item.Title, item.Description, item.CreatedAt, item.Ignored, item.TypeId, item.Url, item.AcknowledgedAt, item.AcknowledgedBy, count))));
        return ret;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't obtain latest notification items: " + ex.ToString()));
        throw new Exception(Resources.get_LIBCODE_LK0_1());
      }
    }

    public List<NotificationItem> GetNotificationItemsByType(
      Guid typeId,
      bool includeIgnored)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.GetItemsByTypeId.");
      try
      {
        ICollection<NotificationItemDAL> itemsByTypeId = NotificationItemDAL.GetItemsByTypeId(typeId, new NotificationItemFilter(true, includeIgnored));
        List<NotificationItem> notificationItemList = new List<NotificationItem>();
        foreach (NotificationItemDAL notificationItemDal in (IEnumerable<NotificationItemDAL>) itemsByTypeId)
        {
          NotificationItem notificationItem = new NotificationItem(notificationItemDal.Id, notificationItemDal.Title, notificationItemDal.Description, notificationItemDal.CreatedAt, notificationItemDal.Ignored, notificationItemDal.TypeId, notificationItemDal.Url, notificationItemDal.AcknowledgedAt, notificationItemDal.AcknowledgedBy);
          notificationItemList.Add(notificationItem);
        }
        return notificationItemList;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't obtain latest notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_15(), (object) typeId));
      }
    }

    public void InsertNotificationItem(NotificationItem item)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.InsertNotificationItem.");
      try
      {
        NotificationItemDAL.Insert(item.get_Id(), item.get_TypeId(), item.get_Title(), item.get_Description(), item.get_Ignored(), item.get_Url(), item.get_AcknowledgedAt(), item.get_AcknowledgedBy());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Unable to insert notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_17(), (object) item.get_Id(), (object) item.get_TypeId()));
      }
    }

    public void UpdateNotificationItem(NotificationItem item)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.UpdateNotificationItem.");
      try
      {
        NotificationItemDAL.Update(item.get_Id(), item.get_TypeId(), item.get_Title(), item.get_Description(), item.get_Ignored(), item.get_Url(), item.get_CreatedAt(), item.get_AcknowledgedAt(), item.get_AcknowledgedBy());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Unable to update notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_16(), (object) item.get_Id(), (object) item.get_TypeId()));
      }
    }

    public bool DeleteNotificationItemById(Guid itemId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.Delete.");
      try
      {
        return NotificationItemDAL.Delete(itemId);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Unable to delete notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_18(), (object) itemId));
      }
    }

    public NotificationItem GetNotificationItemById(Guid itemId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemDAL.GetItemById.");
      try
      {
        NotificationItemDAL itemById = NotificationItemDAL.GetItemById<NotificationItemDAL>(itemId);
        return itemById != null ? new NotificationItem(itemById.Id, itemById.Title, itemById.Description, itemById.CreatedAt, itemById.Ignored, itemById.TypeId, itemById.Url, itemById.AcknowledgedAt, itemById.AcknowledgedBy) : (NotificationItem) null;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Can't obtain notification item: " + ex.ToString()));
        throw new Exception(string.Format(Resources.get_LIBCODE_JM0_19(), (object) itemId));
      }
    }

    public DataTable GetOrionMessagesTable(OrionMessagesFilter filter)
    {
      return OrionMessagesDAL.GetOrionMessagesTable(filter);
    }

    public PollerAssignment GetPoller(int pollerID)
    {
      return PollerDAL.GetPoller(pollerID);
    }

    public void DeletePoller(int pollerID)
    {
      PollerDAL.DeletePoller(pollerID);
    }

    public int InsertPoller(PollerAssignment poller)
    {
      return PollerDAL.InsertPoller(poller);
    }

    public PollerAssignments GetPollersForNode(int nodeId)
    {
      return PollerDAL.GetPollersForNode(nodeId);
    }

    public PollerAssignments GetAllPollersForNode(int nodeId)
    {
      return PollerDAL.GetAllPollersForNode(nodeId, this.AreInterfacesSupported);
    }

    public int AddReportJob(ReportJobConfiguration configuration)
    {
      List<int> intList1 = new List<int>();
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      List<int> intList2 = new FrequenciesDAL((IInformationServiceProxyCreator) creator).SaveFrequencies(new List<ISchedule>((IEnumerable<ISchedule>) configuration.get_Schedules()));
      int num1 = ReportJobDAL.AddReportJob(configuration);
      int num2 = num1;
      ReportSchedulesDAL.ScheduleReportJobWithFrequencies(intList2, num2);
      new ActionsDAL((IInformationServiceProxyCreator) creator).SaveActionsForAssignments(num1, ((ActionEnviromentType) 1).ToString(), (IEnumerable<ActionDefinition>) configuration.get_Actions());
      configuration.set_ReportJobID(num1);
      this.AddBLScheduler(configuration);
      return configuration.get_ReportJobID();
    }

    public void ChangeReportJobStatus(int jobId, bool enable)
    {
      ReportJobDAL.ChangeReportJobStatus(jobId, enable);
      ReportJobConfiguration reportJob = ReportJobDAL.GetReportJob(jobId);
      this.RemoveBLScheduler(reportJob);
      this.AddBLScheduler(reportJob);
    }

    public void AssignJobsToReport(int reportId, List<int> schedulesIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Assigning jobs for report", Array.Empty<object>());
      List<int> jobsIdsWithReport = ReportJobDAL.GetJobsIdsWithReport(reportId);
      jobsIdsWithReport.AddRange((IEnumerable<int>) schedulesIds);
      ReportJobDAL.AssignJobsToReport(reportId, schedulesIds);
      if (jobsIdsWithReport.Count == 0)
        return;
      using (List<ReportJobConfiguration>.Enumerator enumerator = ReportJobDAL.GetJobsByIds(jobsIdsWithReport).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ReportJobConfiguration current = enumerator.Current;
          this.RemoveBLScheduler(current);
          this.AddBLScheduler(current);
        }
      }
    }

    public void AssignJobsToReports(List<int> reportIds, List<int> schedulesIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Assigning jobs for report", Array.Empty<object>());
      List<int> intList = new List<int>();
      if (reportIds.Count > 0)
        intList = ReportJobDAL.GetJobsIdsWithReports(reportIds);
      intList.AddRange((IEnumerable<int>) schedulesIds);
      ReportJobDAL.AssignJobsToReports(reportIds, schedulesIds);
      using (List<ReportJobConfiguration>.Enumerator enumerator = ReportJobDAL.GetJobsByIds(intList).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ReportJobConfiguration current = enumerator.Current;
          this.RemoveBLScheduler(current);
          this.AddBLScheduler(current);
        }
      }
    }

    public void UpdateReportJob(ReportJobConfiguration configuration, int[] allowedReportIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Updating report job", Array.Empty<object>());
      ReportJobConfiguration reportJob = ReportJobDAL.GetReportJob(configuration.get_ReportJobID());
      this.RemoveBLScheduler(reportJob);
      bool flag = true;
      if (reportJob.get_Schedules() != null && configuration.get_Schedules() != null && reportJob.get_Schedules().Count == configuration.get_Schedules().Count)
      {
        using (List<ReportSchedule>.Enumerator enumerator = reportJob.get_Schedules().GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            ReportSchedule current = enumerator.Current;
            ReportSchedule reportSchedule1 = current;
            DateTime startTime = current.get_StartTime();
            DateTime localTime = startTime.ToLocalTime();
            reportSchedule1.set_StartTime(localTime);
            ReportSchedule reportSchedule2 = current;
            DateTime? endTime = current.get_EndTime();
            DateTime? nullable;
            if (endTime.HasValue)
            {
              endTime = current.get_EndTime();
              startTime = endTime.Value;
              nullable = new DateTime?(startTime.ToLocalTime());
            }
            else
              nullable = current.get_EndTime();
            reportSchedule2.set_EndTime(nullable);
          }
        }
        flag = !((IEnumerable<ReportSchedule>) reportJob.get_Schedules()).SequenceEqual<ReportSchedule>((IEnumerable<ReportSchedule>) configuration.get_Schedules());
      }
      List<int> intList = new List<int>();
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      if (flag)
      {
        FrequenciesDAL frequenciesDal = new FrequenciesDAL((IInformationServiceProxyCreator) creator);
        intList = frequenciesDal.SaveFrequencies(new List<ISchedule>((IEnumerable<ISchedule>) configuration.get_Schedules()));
        List<int> list = ((IEnumerable<ReportSchedule>) this.GetReportJob(configuration.get_ReportJobID()).get_Schedules()).Select<ReportSchedule, int>((Func<ReportSchedule, int>) (x => x.get_FrequencyId())).Except<int>((IEnumerable<int>) intList).ToList<int>();
        if (list.Count > 0)
          frequenciesDal.DeleteFrequencies(list);
      }
      ReportJobDAL.UpdateReportJob(configuration, allowedReportIds, flag);
      ReportSchedulesDAL.ScheduleReportJobWithFrequencies(intList, configuration.get_ReportJobID());
      List<ActionDefinition> actionDefinitionList = new List<ActionDefinition>();
      using (List<ActionDefinition>.Enumerator enumerator = reportJob.get_Actions().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ActionDefinition actionDefinition = enumerator.Current;
          if (!((IEnumerable<ActionDefinition>) configuration.get_Actions()).Any<ActionDefinition>((Func<ActionDefinition, bool>) (a =>
          {
            int? id1 = a.get_ID();
            int? id2 = actionDefinition.get_ID();
            return id1.GetValueOrDefault() == id2.GetValueOrDefault() & id1.HasValue == id2.HasValue;
          })) && !actionDefinition.get_IsShared())
            actionDefinitionList.Add(actionDefinition);
        }
      }
      ActionsDAL actionsDal = new ActionsDAL((IInformationServiceProxyCreator) creator);
      actionsDal.SaveActionsForAssignments(configuration.get_ReportJobID(), ((ActionEnviromentType) 1).ToString(), (IEnumerable<ActionDefinition>) configuration.get_Actions());
      using (List<ActionDefinition>.Enumerator enumerator = actionDefinitionList.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ActionDefinition current = enumerator.Current;
          actionsDal.DeleteAction(Convert.ToInt32((object) current.get_ID()));
        }
      }
      this.AddBLScheduler(configuration);
    }

    public void DeleteReportJobs(List<int> reportJobIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Deleting report job", Array.Empty<object>());
      List<ReportJobConfiguration> jobsByIds = ReportJobDAL.GetJobsByIds(reportJobIds);
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      ActionsDAL actionsDal = new ActionsDAL((IInformationServiceProxyCreator) creator);
      using (List<ReportJobConfiguration>.Enumerator enumerator = jobsByIds.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ReportJobConfiguration current = enumerator.Current;
          this.RemoveBLScheduler(current);
          actionsDal.SaveActionsForAssignments(current.get_ReportJobID(), ((ActionEnviromentType) 1).ToString(), (IEnumerable<ActionDefinition>) new List<ActionDefinition>());
        }
      }
      List<int> intList = new List<int>();
      using (List<ReportJobConfiguration>.Enumerator enumerator = jobsByIds.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ReportJobConfiguration current = enumerator.Current;
          if (current.get_Schedules() != null)
            intList.AddRange(((IEnumerable<ReportSchedule>) current.get_Schedules()).Select<ReportSchedule, int>((Func<ReportSchedule, int>) (x => x.get_FrequencyId())));
        }
      }
      new FrequenciesDAL((IInformationServiceProxyCreator) creator).DeleteFrequencies(intList);
      ReportJobDAL.DeleteReportJobs(reportJobIds);
    }

    private void RemoveBLScheduler(ReportJobConfiguration configuration)
    {
      if (configuration.get_Schedules() == null)
        return;
      for (int index = 0; index < configuration.get_Schedules().Count; ++index)
        Scheduler.get_Instance().Remove(string.Format("ReportJob-{0}_{1}", (object) configuration.get_ReportJobID().ToString(), (object) index));
    }

    private void AddBLScheduler(ReportJobConfiguration configuration)
    {
      ReportJobInitializer.AddActionsToScheduler(configuration, this);
    }

    public List<ReportJobConfiguration> GetSchedulesWithReport(
      int reportId)
    {
      return ReportJobDAL.GetJobsWithReport(reportId);
    }

    public int DublicateReportJob(int reportJobId, string jobName, int[] allowedReportIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Dublicate report job", Array.Empty<object>());
      int num = ReportJobDAL.DublicateReportJob(reportJobId, jobName, allowedReportIds);
      ReportJobConfiguration reportJob = this.GetReportJob(reportJobId);
      reportJob.set_ReportJobID(num);
      ISwisConnectionProxyCreator creator = SwisConnectionProxyPool.GetCreator();
      ActionsDAL actionsDal = new ActionsDAL((IInformationServiceProxyCreator) creator);
      using (List<ReportSchedule>.Enumerator enumerator = reportJob.get_Schedules().GetEnumerator())
      {
        while (enumerator.MoveNext())
          enumerator.Current.set_FrequencyId(0);
      }
      ReportSchedulesDAL.ScheduleReportJobWithFrequencies(new FrequenciesDAL((IInformationServiceProxyCreator) creator).SaveFrequencies(new List<ISchedule>((IEnumerable<ISchedule>) reportJob.get_Schedules())), num);
      using (List<ActionDefinition>.Enumerator enumerator = reportJob.get_Actions().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ActionDefinition current = enumerator.Current;
          current.set_Title(actionsDal.GetUniqueNameForAction(current.get_Title()));
          current.set_IsShared(false);
        }
      }
      actionsDal.SaveActionsForAssignments(num, ((ActionEnviromentType) 1).ToString(), (IEnumerable<ActionDefinition>) reportJob.get_Actions());
      this.AddBLScheduler(reportJob);
      return num;
    }

    public ReportJobConfiguration GetReportJob(int jobId)
    {
      CoreBusinessLayerService.log.DebugFormat("Extracting report job by ID", Array.Empty<object>());
      return ReportJobDAL.GetReportJob(jobId);
    }

    public void UnAssignReportsFromJob(int jobId, List<int> reportIds)
    {
      ReportJobDAL.UnAssignReportsFromJob(jobId, reportIds);
      ReportJobConfiguration reportJob = ReportJobDAL.GetReportJob(jobId);
      this.RemoveBLScheduler(reportJob);
      this.AddBLScheduler(reportJob);
    }

    public Dictionary<int, bool> RunNow(List<int> schedulesIds)
    {
      CoreBusinessLayerService.log.DebugFormat("Running job(s)", Array.Empty<object>());
      Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
      List<ReportJobConfiguration> jobsByIds = ReportJobDAL.GetJobsByIds(schedulesIds);
      for (int index = 0; index < jobsByIds.Count; ++index)
      {
        using (List<ActionDefinition>.Enumerator enumerator = jobsByIds[index].get_Actions().GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            ActionDefinition current = enumerator.Current;
            ReportingActionContext reportingActionContext1 = new ReportingActionContext();
            reportingActionContext1.set_AccountID(jobsByIds[index].get_AccountID());
            reportingActionContext1.set_UrlsGroupedByLeftPart(ReportJobInitializer.GroupUrls(jobsByIds[index]));
            reportingActionContext1.set_WebsiteID(jobsByIds[index].get_WebsiteID());
            ReportingActionContext reportingActionContext2 = reportingActionContext1;
            MacroContext macroContext = ((ActionContextBase) reportingActionContext2).get_MacroContext();
            ReportingContext reportingContext = new ReportingContext();
            reportingContext.set_AccountID(jobsByIds[index].get_AccountID());
            reportingContext.set_ScheduleName(jobsByIds[index].get_Name());
            reportingContext.set_ScheduleDescription(jobsByIds[index].get_Description());
            reportingContext.set_LastRun(jobsByIds[index].get_LastRun());
            reportingContext.set_WebsiteID(jobsByIds[index].get_WebsiteID());
            macroContext.Add((ContextBase) reportingContext);
            ((ActionContextBase) reportingActionContext2).get_MacroContext().Add((ContextBase) new GenericContext());
            if (!dictionary.Keys.Contains<int>(jobsByIds[index].get_ReportJobID()))
            {
              dictionary.Add(jobsByIds[index].get_ReportJobID(), this.ExecuteAction(current, (ActionContextBase) reportingActionContext2).get_Status() == 1);
            }
            else
            {
              ActionResult actionResult = this.ExecuteAction(current, (ActionContextBase) reportingActionContext2);
              dictionary[jobsByIds[index].get_ReportJobID()] = actionResult.get_Status() == 1 && dictionary[jobsByIds[index].get_ReportJobID()];
            }
          }
        }
        jobsByIds[index].set_LastRun(new DateTime?(DateTime.Now.ToUniversalTime()));
        ReportJobDAL.UpdateLastRun(jobsByIds[index].get_ReportJobID(), jobsByIds[index].get_LastRun());
      }
      return dictionary;
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public int[] GetListOfAllowedReports()
    {
      try
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator()).Create())
        {
          DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query("SELECT ReportID FROM Orion.Report");
          List<int> intList = new List<int>();
          if (dataTable != null)
            intList.AddRange(dataTable.Rows.Cast<DataRow>().Select<DataRow, int>((Func<DataRow, int>) (row => (int) row["ReportID"])));
          return intList.ToArray();
        }
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "GetLimitedReportIds failed.", ex);
        throw;
      }
    }

    public List<SmtpServer> GetAvailableSmtpServers()
    {
      return SmtpServerDAL.GetAvailableSmtpServers();
    }

    public bool DefaultSmtpServerExists()
    {
      return SmtpServerDAL.DefaultSmtpServerExists();
    }

    public int InsertSmtpServer(SmtpServer server)
    {
      return SmtpServerDAL.InsertSmtpServer(server);
    }

    public SmtpServer GetSmtpServer(int id)
    {
      return SmtpServerDAL.GetSmtpServer(id);
    }

    public SmtpServer GetSmtpServerByAddress(string address)
    {
      return SmtpServerDAL.GetSmtpServerByAddress(address);
    }

    public bool UpdateSmtpServer(SmtpServer server)
    {
      return SmtpServerDAL.UpdateSmtpServer(server);
    }

    public bool DeleteSmtpServer(int id)
    {
      return SmtpServerDAL.DeleteSmtpServer(id);
    }

    public void SetSmtpServerAsDefault(int id)
    {
      SmtpServerDAL.SetSmtpServerAsDefault(id);
    }

    public bool SNMPQuery(
      int nodeId,
      string oid,
      string snmpGetType,
      out Dictionary<string, string> response)
    {
      return SNMPHelper.SNMPQuery(nodeId, snmpGetType, oid, ref response);
    }

    private static bool AreRelatedOids(string queryOid, string returnedOid)
    {
      if (queryOid.Equals(returnedOid))
        return true;
      string[] strArray1 = queryOid.Split('.');
      string[] strArray2 = returnedOid.Split('.');
      if (strArray1.Length > strArray2.Length)
        return false;
      for (int index = 0; index < strArray1.Length; ++index)
      {
        if (strArray1[index].CompareTo(strArray2[index]) != 0)
          return false;
      }
      return true;
    }

    private static string GetOidValueFromXmlNodes(XmlNode[] xmlNodes)
    {
      string empty = string.Empty;
      XmlText xmlText = xmlNodes != null ? (XmlText) ((IEnumerable<XmlNode>) xmlNodes).FirstOrDefault<XmlNode>((Func<XmlNode, bool>) (item => item is XmlText)) : (XmlText) null;
      if (xmlText != null)
        empty = xmlText.Value;
      return empty;
    }

    private static bool NodeSnmpQueryProcessGetOrGetNextResult(
      string oid,
      Dictionary<string, string> response,
      SnmpJobResults jobResult)
    {
      string valueFromXmlNodes = CoreBusinessLayerService.GetOidValueFromXmlNodes(((List<SnmpOID>) jobResult.get_Results()[0].get_OIDs())[0].get_Value() as XmlNode[]);
      if (string.IsNullOrEmpty(valueFromXmlNodes))
      {
        response["swerror"] = Resources.get_LIBCODE_PS0_18();
        return false;
      }
      if (CoreBusinessLayerService.AreRelatedOids(oid, ((List<SnmpOID>) jobResult.get_Results()[0].get_OIDs())[0].get_OID()))
      {
        response["0"] = valueFromXmlNodes;
        return true;
      }
      response["swerror"] = Resources.get_LIBCODE_PS0_19();
      return false;
    }

    private static bool NodeSnmpQueryProcessSubtreeResult(
      string oid,
      Dictionary<string, string> response,
      SnmpJobResults jobResult)
    {
      if (((List<SnmpOID>) jobResult.get_Results()[0].get_OIDs()).Count == 0)
        return false;
      string str1 = oid;
      string str2 = oid;
      bool flag = true;
      using (List<SnmpOID>.Enumerator enumerator = ((List<SnmpOID>) jobResult.get_Results()[0].get_OIDs()).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          SnmpOID current = enumerator.Current;
          if (current.get_OID().StartsWith(str1 + ".") && str2 != current.get_OID())
          {
            str2 = current.get_OID();
            string index = "0";
            if (current.get_OID().Length > str1.Length + 1)
              index = current.get_OID().Substring(str1.Length + 1);
            string valueFromXmlNodes = CoreBusinessLayerService.GetOidValueFromXmlNodes(current.get_Value() as XmlNode[]);
            if (string.IsNullOrEmpty(valueFromXmlNodes))
            {
              response[index] = Resources.get_LIBCODE_PS0_19();
              flag = false;
            }
            else
              response[index] = valueFromXmlNodes;
          }
        }
      }
      return flag;
    }

    public bool NodeSNMPQuery(
      SolarWinds.Orion.Core.Common.Models.Node node,
      string oid,
      string snmpGetType,
      out Dictionary<string, string> response)
    {
      SnmpRequestType result1;
      if (!System.Enum.TryParse<SnmpRequestType>(snmpGetType, true, out result1))
        result1 = (SnmpRequestType) 0;
      List<SnmpRequest> snmpRequestList1 = new List<SnmpRequest>();
      List<SnmpRequest> snmpRequestList2 = snmpRequestList1;
      SnmpRequest snmpRequest = new SnmpRequest();
      snmpRequest.set_OID(oid);
      snmpRequest.set_IsTransform(false);
      snmpRequest.set_RequestType(result1);
      snmpRequestList2.Add(snmpRequest);
      int num = SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP Timeout", 2500) / 1000;
      JobDescription jobDescription = SnmpJob.CreateJobDescription(node.get_IpAddress(), node.get_SNMPPort(), num, node.get_SNMPVersion(), snmpRequestList1, BusinessLayerSettings.Instance.TestJobTimeout);
      SnmpCredentialsV2 snmpCredentialsV2 = new SnmpCredentialsV2(node.get_ReadOnlyCredentials());
      response = new Dictionary<string, string>();
      SnmpJobResults result2 = this.ExecuteJobAndGetResult<SnmpJobResults>(jobDescription, (CredentialBase) snmpCredentialsV2, JobResultDataFormatType.Xml, "SNMP", out string _);
      if (!((TestJobResult) result2).get_Success() || result2.get_Results().Count == 0)
        return false;
      if (result2.get_Results()[0].get_ResultType() != null)
      {
        response["swerror"] = result2.get_Results()[0].get_ErrorMessage();
        return false;
      }
      return result1 == null || result1 == 1 ? CoreBusinessLayerService.NodeSnmpQueryProcessGetOrGetNextResult(oid, response, result2) : CoreBusinessLayerService.NodeSnmpQueryProcessSubtreeResult(oid, response, result2);
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public void SNMPQueryForIp(
      string ip,
      string oid,
      List<SnmpEntry> credentials,
      string snmpGetType,
      out Dictionary<string, string> response)
    {
      SNMPHelper.SNMPQueryForIp(ip, oid, credentials, snmpGetType, ref response);
    }

    public Dictionary<string, Dictionary<string, string>> GetColumns(
      string tableOID,
      int nodeId)
    {
      return SNMPHelper.GetColumns(tableOID, nodeId);
    }

    public bool ValidateSNMP(
      SNMPVersion snmpVersion,
      string ip,
      uint snmpPort,
      string community,
      string authKey,
      bool authKeyIsPwd,
      SNMPv3AuthType authType,
      SNMPv3PrivacyType privacyType,
      string privacyPassword,
      bool privKeyIsPwd,
      string context,
      string username)
    {
      return this.ValidateSNMPWithErrorMessage(snmpVersion, ip, snmpPort, community, authKey, authKeyIsPwd, authType, privacyType, privacyPassword, privKeyIsPwd, context, username, out string _);
    }

    public bool ValidateSNMPWithErrorMessage(
      SNMPVersion snmpVersion,
      string ip,
      uint snmpPort,
      string community,
      string authKey,
      bool authKeyIsPwd,
      SNMPv3AuthType authType,
      SNMPv3PrivacyType privacyType,
      string privacyPassword,
      bool privKeyIsPwd,
      string context,
      string username,
      out string localizedErrorMessage)
    {
      List<SnmpRequest> snmpRequestList1 = new List<SnmpRequest>();
      List<SnmpRequest> snmpRequestList2 = snmpRequestList1;
      SnmpRequest snmpRequest = new SnmpRequest();
      snmpRequest.set_OID("1.3.6.1.2.1.1.2.0");
      snmpRequest.set_IsTransform(false);
      snmpRequest.set_OIDLabel("sysObjectID");
      snmpRequest.set_RequestType((SnmpRequestType) 0);
      snmpRequestList2.Add(snmpRequest);
      localizedErrorMessage = string.Empty;
      int num = SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP Timeout", 2500) / 1000;
      JobDescription jobDescription = SnmpJob.CreateJobDescription(ip, snmpPort, num, snmpVersion, snmpRequestList1, BusinessLayerSettings.Instance.TestJobTimeout);
      SnmpCredentialsV2 snmpCredentialsV2_1 = new SnmpCredentialsV2();
      snmpCredentialsV2_1.set_CommunityString(community);
      snmpCredentialsV2_1.set_CredentialName("");
      snmpCredentialsV2_1.set_SNMPV3AuthKeyIsPwd(authKeyIsPwd);
      snmpCredentialsV2_1.set_SNMPv3AuthType(authType);
      snmpCredentialsV2_1.set_SNMPv3AuthPassword(authKey);
      snmpCredentialsV2_1.set_SNMPv3PrivacyType(privacyType);
      snmpCredentialsV2_1.set_SNMPv3PrivacyPassword(privacyPassword);
      snmpCredentialsV2_1.set_SNMPV3PrivKeyIsPwd(privKeyIsPwd);
      snmpCredentialsV2_1.set_SnmpV3Context(context);
      snmpCredentialsV2_1.set_SNMPv3UserName(username);
      SnmpCredentialsV2 snmpCredentialsV2_2 = snmpCredentialsV2_1;
      SnmpJobResults result = this.ExecuteJobAndGetResult<SnmpJobResults>(jobDescription, (CredentialBase) snmpCredentialsV2_2, JobResultDataFormatType.Xml, "SNMP", out string _);
      if (!((TestJobResult) result).get_Success())
      {
        localizedErrorMessage = ((TestJobResult) result).get_Message();
        return false;
      }
      bool flag = result.get_Results().Count > 0 && result.get_Results()[0].get_ResultType() == 0;
      CoreBusinessLayerService.log.InfoFormat("SNMP credential test finished. Success: {0}", (object) flag);
      return flag;
    }

    public bool ValidateReadWriteSNMP(
      SNMPVersion snmpVersion,
      string ip,
      uint snmpPort,
      string community,
      string authKey,
      bool authKeyIsPwd,
      SNMPv3AuthType authType,
      SNMPv3PrivacyType privacyType,
      string privacyPassword,
      bool privKeyIsPwd,
      string context,
      string username)
    {
      int num = SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP Timeout", 2500) / 1000;
      JobDescription jobDescription = SnmpReadWriteCredentialValidateJob.CreateJobDescription(ip, snmpPort, num, snmpVersion, BusinessLayerSettings.Instance.TestJobTimeout);
      SnmpCredentialsV2 snmpCredentialsV2_1 = new SnmpCredentialsV2();
      snmpCredentialsV2_1.set_CommunityString(community);
      snmpCredentialsV2_1.set_CredentialName("");
      snmpCredentialsV2_1.set_SNMPV3AuthKeyIsPwd(authKeyIsPwd);
      snmpCredentialsV2_1.set_SNMPv3AuthType(authType);
      snmpCredentialsV2_1.set_SNMPv3AuthPassword(authKey);
      snmpCredentialsV2_1.set_SNMPv3PrivacyType(privacyType);
      snmpCredentialsV2_1.set_SNMPv3PrivacyPassword(privacyPassword);
      snmpCredentialsV2_1.set_SNMPV3PrivKeyIsPwd(privKeyIsPwd);
      snmpCredentialsV2_1.set_SnmpV3Context(context);
      snmpCredentialsV2_1.set_SNMPv3UserName(username);
      SnmpCredentialsV2 snmpCredentialsV2_2 = snmpCredentialsV2_1;
      ValidateJobResult result = this.ExecuteJobAndGetResult<ValidateJobResult>(jobDescription, (CredentialBase) snmpCredentialsV2_2, JobResultDataFormatType.Xml, "SNMP", out string _);
      CoreBusinessLayerService.log.InfoFormat(string.Format("SNMP read/write credential test finished. Success: {0}.", (object) result.get_IsValid()), Array.Empty<object>());
      return result.get_IsValid();
    }

    private void SnmpEncodingSettingsChanged(object sender, SettingsChangedEventArgs e)
    {
      try
      {
        if (SnmpSettings.get_Instance().get_Encoding() == SNMPEncodingExtension.GetWebName((SNMPEncoding) 0))
        {
          string autoEncoding = SNMPHelper.GetAutoEncoding();
          CoreBusinessLayerService.log.InfoFormat("Set encoding {0} for primary locale {1}", (object) autoEncoding, (object) LocaleConfiguration.get_PrimaryLocale());
          SNMPEncodingSettings.get_Instance().ChangeEncoding(Encoding.GetEncoding(autoEncoding));
        }
        else
          SNMPEncodingSettings.get_Instance().ChangeEncoding(Encoding.GetEncoding(SnmpSettings.get_Instance().get_Encoding()));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Unable to save text encoding setting for snmp. Encoding will be system default: " + Encoding.Default.WebName), ex);
      }
    }

    [Obsolete("Use GetSnmpV3CredentialsSet method")]
    public List<string> GetCredentialsSet()
    {
      return this.GetSnmpV3CredentialsSet().Values.ToList<string>();
    }

    public IDictionary<int, string> GetSnmpV3CredentialsSet()
    {
      try
      {
        return new CredentialManager().GetCredentialNames<SnmpCredentialsV3>("Orion");
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Error getting Snmp v3 credentials", ex);
        throw;
      }
    }

    [Obsolete("Use InsertSnmpV3Credentials method")]
    public void InsertCredentials(SnmpCredentials crendentials)
    {
      this.InsertSnmpV3Credentials((SnmpCredentialsV3) CredentialHelper.ParseCredentials(crendentials));
    }

    public int? InsertSnmpV3Credentials(SnmpCredentialsV3 credentials)
    {
      try
      {
        new CredentialManager().AddCredential<SnmpCredentialsV3>("Orion", (M0) credentials);
        return ((Credential) credentials).get_ID();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Error inserting Snmp v3 credentials", ex);
        throw;
      }
    }

    [Obsolete("Use DeleteSnmpV3Credentials method")]
    public void DeleteCredentials(string CredentialName)
    {
      this.DeleteSnmpV3Credentials(CredentialName);
    }

    public void DeleteSnmpV3Credentials(string CredentialName)
    {
      try
      {
        CredentialManager credentialManager = new CredentialManager();
        credentialManager.DeleteCredential<SnmpCredentialsV3>("Orion", (M0) ((IEnumerable<SnmpCredentialsV3>) credentialManager.GetCredentials<SnmpCredentialsV3>("Orion", CredentialName)).FirstOrDefault<SnmpCredentialsV3>());
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("Error deleting Snmp v3 credentials by name {0}", (object) CredentialName), ex);
        throw;
      }
    }

    public void DeleteSnmpV3CredentialsByID(int CredentialID)
    {
      try
      {
        new CredentialManager().DeleteCredential("Orion", CredentialID);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("Error deleting Snmp v3 credentials by id {0}", (object) CredentialID), ex);
        throw;
      }
    }

    [Obsolete("Use GetSnmpV3Credentials method")]
    public SnmpCredentials GetCredentials(string CredentialName)
    {
      try
      {
        return SnmpCredentials.CreateSnmpCredentials(CredentialHelper.GetSnmpEntry((SnmpCredentials) ((IEnumerable<SnmpCredentialsV3>) new CredentialManager().GetCredentials<SnmpCredentialsV3>("Orion", CredentialName)).FirstOrDefault<SnmpCredentialsV3>()));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("Error getting Snmp v3 credentials by name {0}", (object) CredentialName), ex);
        throw;
      }
    }

    public SnmpCredentialsV3 GetSnmpV3Credentials(string CredentialName)
    {
      try
      {
        return ((IEnumerable<SnmpCredentialsV3>) new CredentialManager().GetCredentials<SnmpCredentialsV3>("Orion", CredentialName)).FirstOrDefault<SnmpCredentialsV3>();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("Error getting Snmp v3 credentials by name {0}", (object) CredentialName), ex);
        throw;
      }
    }

    public SnmpCredentialsV3 GetSnmpV3CredentialsByID(int CredentialID)
    {
      try
      {
        return ((IEnumerable<SnmpCredentialsV3>) new CredentialManager().GetCredentials<SnmpCredentialsV3>((IEnumerable<int>) new List<int>()
        {
          CredentialID
        })).FirstOrDefault<SnmpCredentialsV3>();
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) string.Format("Error getting Snmp v3 credentials by id {0}", (object) CredentialID), ex);
        throw;
      }
    }

    [Obsolete("Use UpdateSnmpV3Credentials method")]
    public void UpdateCredentials(SnmpCredentials credentials)
    {
      try
      {
        new CredentialManager().UpdateCredential<SnmpCredentialsV3>("Orion", (M0) CredentialHelper.ParseCredentials(credentials));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Error updating Snmp v3 credentials", ex);
        throw;
      }
    }

    public void UpdateSnmpV3Credentials(SnmpCredentialsV3 credentials)
    {
      try
      {
        new CredentialManager().UpdateCredential<SnmpCredentialsV3>("Orion", (M0) credentials);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) "Error updating Snmp v3 credentials", ex);
        throw;
      }
    }

    public StringDictionary GetSeverities()
    {
      return SysLogDAL.GetSeverities();
    }

    public StringDictionary GetFacilities()
    {
      return SysLogDAL.GetFacilities();
    }

    public BaselineValues GetBaselineValues(string thresholdName, int instanceId)
    {
      return ThresholdProcessingManager.Instance.Engine.GetBaselineValues(thresholdName, instanceId);
    }

    public List<BaselineValues> GetBaselineValuesForAllTimeFrames(
      string thresholdName,
      int instanceId)
    {
      return ThresholdProcessingManager.Instance.Engine.GetBaselineValuesForAllTimeFrames(thresholdName, instanceId);
    }

    public ThresholdComputationResult ComputeThresholds(
      string thresholdName,
      int instanceId,
      string warningFormula,
      string criticalFormula,
      BaselineValues baselineValues,
      ThresholdOperatorEnum thresholdOperator)
    {
      return ThresholdProcessingManager.Instance.Engine.ComputeThresholds(thresholdName, instanceId, warningFormula, criticalFormula, baselineValues, thresholdOperator);
    }

    public ThresholdComputationResult ProcessThresholds(
      double warningThreshold,
      double criticalThreshold,
      ThresholdOperatorEnum oper,
      ThresholdMinMaxValue minMaxValues)
    {
      return ThresholdProcessingManager.Instance.Engine.ProcessThresholds(warningThreshold, criticalThreshold, oper, minMaxValues);
    }

    public ThresholdComputationResult ProcessThresholds(
      bool warningEnabled,
      double warningThreshold,
      bool criticalEnabled,
      double criticalThreshold,
      ThresholdOperatorEnum oper,
      ThresholdMinMaxValue minMaxValues)
    {
      return ThresholdProcessingManager.Instance.Engine.ProcessThresholds(warningEnabled, warningThreshold, criticalEnabled, criticalThreshold, oper, minMaxValues);
    }

    public ValidationResult IsFormulaValid(
      string thresholdName,
      string formula,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      return ThresholdProcessingManager.Instance.Engine.IsFormulaValid(thresholdName, formula, level, thresholdOperator);
    }

    public ThresholdMinMaxValue GetThresholdMinMaxValues(
      string thresholdName,
      int instanceId)
    {
      return ThresholdProcessingManager.Instance.Engine.GetThresholdMinMaxValues(thresholdName, instanceId);
    }

    public int SetThreshold(Threshold threshold)
    {
      return ThresholdProcessingManager.Instance.Engine.SetThreshold(threshold);
    }

    public StatisticalDataHistogram[] GetHistogramForStatisticalData(
      string thresholdName,
      int instanceId)
    {
      return ThresholdProcessingManager.Instance.Engine.GetHistogramForStatisticalData(thresholdName, instanceId);
    }

    public string GetStatisticalDataChartName(string thresholdName)
    {
      return ThresholdProcessingManager.Instance.Engine.GetStatisticalDataChartName(thresholdName);
    }

    public string GetThresholdInstanceName(string thresholdName, int instanceId)
    {
      return ThresholdProcessingManager.Instance.Engine.GetThresholdInstanceName(thresholdName, instanceId);
    }

    public TracerouteResult TraceRoute(string destinationHostNameOrIpAddress)
    {
      return CoreBusinessLayerService.CreateTraceRouteProvider().TraceRoute(destinationHostNameOrIpAddress);
    }

    private static ITraceRouteProvider CreateTraceRouteProvider()
    {
      return (ITraceRouteProvider) new TraceRouteProviderSync();
    }

    public Views GetSummaryDetailsViews()
    {
      return ViewsDAL.GetSummaryDetailsViews();
    }

    public void DeleteVolume(Volume volume)
    {
      Dictionary<string, object> volumeBaseInfo = CoreBusinessLayerService.GetVolumeBaseInfo(volume);
      VolumeDAL.DeleteVolume(volume);
      VolumeNotification volumeNotification = new VolumeNotification(IndicationHelper.GetIndicationType((IndicationType) 1), volume, AccountContext.GetAccountID());
      foreach (KeyValuePair<string, object> keyValuePair in volumeBaseInfo)
        volumeNotification.AddSourceInstanceProperty(keyValuePair.Key, keyValuePair.Value);
      volumeNotification.AddIndicationProperty("SourceInstanceUri", volumeBaseInfo["Uri"]);
      PublisherClient.get_Instance().Publish((INotification) volumeNotification);
    }

    public int InsertVolume(Volume volume)
    {
      int maxElementCount = new FeatureManager().GetMaxElementCount(WellKnownElementTypes.get_Volumes());
      if (VolumeDAL.GetVolumeCount() >= maxElementCount)
        throw LicenseException.FromElementsExceeded(maxElementCount);
      int num = VolumeDAL.InsertVolume(volume);
      CoreBusinessLayerService.FireVolumeNotification((IndicationType) 0, volume, (PropertyBag) null);
      return num;
    }

    public void UpdateVolume(Volume volume)
    {
      DateTime utcNow = DateTime.UtcNow;
      if (utcNow > volume.get_UnManageFrom() && utcNow < volume.get_UnManageUntil())
      {
        volume.set_UnManaged(true);
        volume.set_Status(9);
        volume.set_StatusLED("Unmanaged.gif");
      }
      else if (volume.get_Status() == 9)
      {
        volume.set_UnManaged(false);
        volume.set_Status(0);
        volume.set_StatusLED("Unknown.gif");
      }
      PropertyBag changedProperties = VolumeDAL.UpdateVolume(volume);
      CoreBusinessLayerService.FireVolumeNotification((IndicationType) 2, volume, changedProperties);
    }

    public Volume GetVolume(int volumeID)
    {
      return VolumeDAL.GetVolume(volumeID);
    }

    public void BulkUpdateVolumePollingInterval(int pollingInterval, int engineId)
    {
      VolumeDAL.BulkUpdateVolumePollingInterval(pollingInterval, engineId);
    }

    public Dictionary<string, object> GetVolumeCustomProperties(
      int volumeId,
      ICollection<string> properties)
    {
      return VolumeDAL.GetVolumeCustomProperties(volumeId, properties);
    }

    public Volumes GetVolumesByIds(int[] volumeIds)
    {
      return VolumeDAL.GetVolumesByIds(volumeIds);
    }

    private static void FireVolumeNotification(
      IndicationType indicationType,
      Volume volume,
      PropertyBag changedProperties = null)
    {
      try
      {
        Dictionary<string, object> volumeBaseInfo = CoreBusinessLayerService.GetVolumeBaseInfo(volume);
        VolumeNotification volumeNotification = new VolumeNotification(IndicationHelper.GetIndicationType(indicationType), volume, AccountContext.GetAccountID());
        if (indicationType <= 2)
          volumeNotification.AddIndicationProperty("SourceInstanceUri", volumeBaseInfo["Uri"]);
        foreach (KeyValuePair<string, object> keyValuePair in volumeBaseInfo)
          volumeNotification.AddSourceInstanceProperty(keyValuePair.Key, keyValuePair.Value);
        if (changedProperties != null)
        {
          foreach (KeyValuePair<string, object> changedProperty in (Dictionary<string, object>) changedProperties)
            volumeNotification.AddSourceInstanceProperty(changedProperty.Key, changedProperty.Value);
        }
        PublisherClient.get_Instance().Publish((INotification) volumeNotification);
      }
      catch (Exception ex)
      {
        string str = string.Format("Error delivering indication {0} for Volume '{1}' with id {2}.", (object) indicationType, (object) volume.get_ID(), (object) volume.get_Caption());
        CoreBusinessLayerService.log.Error((object) str, ex);
      }
    }

    private static Dictionary<string, object> GetVolumeBaseInfo(Volume volume)
    {
      return new SwisEntityHelper(CoreBusinessLayerService.CreateProxy()).GetProperties("Orion.Volumes", volume.get_VolumeId(), new string[2]
      {
        "DisplayName",
        "Uri"
      });
    }

    public ExternalWebsites GetExternalWebsites()
    {
      return ExternalWebsitesDAL.GetAll();
    }

    public ExternalWebsite GetExternalWebsite(int id)
    {
      return ExternalWebsitesDAL.Get(id);
    }

    public int CreateExternalWebsite(ExternalWebsite site)
    {
      return ExternalWebsitesDAL.Insert(site);
    }

    public void UpdateExternalWebsite(ExternalWebsite site)
    {
      ExternalWebsitesDAL.Update(site);
    }

    public void DeleteExternalWebsite(int id)
    {
      ExternalWebsitesDAL.Delete(id);
    }

    public void AddNewWebMenuItemToMenubar(WebMenuItem item, string menubarName)
    {
      int itemId = WebMenubarDAL.InsertItem(item);
      WebMenubarDAL.AppendItemToMenu(menubarName, itemId);
    }

    public void DeleteWebMenuItemByLink(string link)
    {
      WebMenubarDAL.DeleteItemByLink(link);
    }

    public void RenameWebMenuItemByLink(
      string newName,
      string newDescription,
      string newMenuBar,
      string link)
    {
      WebMenubarDAL.RenameItemByLink(newName, newDescription, newMenuBar, link);
    }

    public bool MenuItemExists(string link)
    {
      return WebMenubarDAL.MenuItemExists(link);
    }

    public RemoteAccessToken GetUserWebIntegrationToken(string username)
    {
      return new RemoteAuthManager().GetUserToken(username);
    }

    public bool IsUserWebIntegrationAvailable(string username)
    {
      return new RemoteAuthManager().IsUserAvailable(username);
    }

    public void DisableUserWebIntegration(string username)
    {
      new RemoteAuthManager().DisableUser(username);
    }

    public RemoteAccessToken ConfigureUserWebIntegration(
      string username,
      string clientId,
      string clientPassword)
    {
      RemoteAuthManager remoteAuthManager = new RemoteAuthManager();
      try
      {
        return remoteAuthManager.ConfigureUser(username, clientId, clientPassword);
      }
      catch (Exception ex)
      {
        throw MessageUtilities.NewFaultException<CoreFaultContract>(ex);
      }
    }

    public IEnumerable<MaintenanceStatus> GetMaintenanceInfoFromCustomerPortal(
      string username)
    {
      try
      {
        return this.MaintenanceInfoCache.get_Item(username);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ex);
        throw;
      }
    }

    public LicenseAndManagementInfo GetLicenseAndMaintenanceSummary(
      string username)
    {
      return this.LAMInfoCache.get_Item(username);
    }

    public IEnumerable<SupportCase> GetSupportCases(string username)
    {
      try
      {
        return this.SupportCasesCache.get_Item(username);
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ex);
        throw;
      }
    }

    private IEnumerable<MaintenanceStatus> GetMaintenanceInfoFromCustomerPortalInternal(
      string username)
    {
      return (IEnumerable<MaintenanceStatus>) new RemoteMaintenanceClient(username).GetMaintenanceInfo().Select<WebMaintenanceStatus, MaintenanceStatus>((Func<WebMaintenanceStatus, MaintenanceStatus>) (i => i.ToMaintenanceStatus())).ToList<MaintenanceStatus>();
    }

    private LicenseAndManagementInfo GetLicenseAndMaintenanceSummaryInternal(
      string username)
    {
      int num1;
      int num2;
      int num3;
      if (this.IsUserWebIntegrationAvailable(username))
      {
        IEnumerable<MaintenanceStatus> fromCustomerPortal = this.GetMaintenanceInfoFromCustomerPortal(username);
        IEnumerable<MaintenanceStatus> source = fromCustomerPortal == null ? (IEnumerable<MaintenanceStatus>) new List<MaintenanceStatus>() : (IEnumerable<MaintenanceStatus>) fromCustomerPortal.ToList<MaintenanceStatus>();
        num1 = source.Count<MaintenanceStatus>((Func<MaintenanceStatus, bool>) (m => (int) (m.get_ExpirationDate() - DateTime.UtcNow.Date).TotalDays >= 90));
        num2 = source.Count<MaintenanceStatus>((Func<MaintenanceStatus, bool>) (m => (int) (m.get_ExpirationDate() - DateTime.UtcNow.Date).TotalDays < 90 && (int) (m.get_ExpirationDate() - DateTime.UtcNow.Date).TotalDays > 0));
        num3 = source.Count<MaintenanceStatus>((Func<MaintenanceStatus, bool>) (m => (int) (m.get_ExpirationDate() - DateTime.UtcNow.Date).TotalDays <= 0));
      }
      else
      {
        List<ModuleLicenseInfo> list = ((IEnumerable<ModuleLicenseInfo>) this.GetModuleLicenseInformation()).Where<ModuleLicenseInfo>((Func<ModuleLicenseInfo, bool>) (m => !string.Equals("DPI", m.get_ModuleName(), StringComparison.OrdinalIgnoreCase) && !m.get_IsEval())).ToList<ModuleLicenseInfo>();
        num1 = ((IEnumerable<ModuleLicenseInfo>) list).Count<ModuleLicenseInfo>((Func<ModuleLicenseInfo, bool>) (m =>
        {
          DateTime dateTime = m.get_MaintenanceExpiration();
          DateTime date1 = dateTime.Date;
          dateTime = DateTime.UtcNow;
          DateTime date2 = dateTime.Date;
          return (int) (date1 - date2).TotalDays >= 90;
        }));
        num2 = ((IEnumerable<ModuleLicenseInfo>) list).Count<ModuleLicenseInfo>((Func<ModuleLicenseInfo, bool>) (m =>
        {
          DateTime dateTime1 = m.get_MaintenanceExpiration();
          DateTime date1 = dateTime1.Date;
          dateTime1 = DateTime.UtcNow;
          DateTime date2 = dateTime1.Date;
          if ((int) (date1 - date2).TotalDays >= 90)
            return false;
          DateTime dateTime2 = m.get_MaintenanceExpiration();
          DateTime date3 = dateTime2.Date;
          dateTime2 = DateTime.UtcNow;
          DateTime date4 = dateTime2.Date;
          return (int) (date3 - date4).TotalDays > 0;
        }));
        num3 = ((IEnumerable<ModuleLicenseInfo>) list).Count<ModuleLicenseInfo>((Func<ModuleLicenseInfo, bool>) (m =>
        {
          DateTime dateTime = m.get_MaintenanceExpiration();
          DateTime date1 = dateTime.Date;
          dateTime = DateTime.UtcNow;
          DateTime date2 = dateTime.Date;
          return (int) (date1 - date2).TotalDays <= 0;
        }));
      }
      int num4 = ((IEnumerable<ModuleLicenseInfo>) this.GetModuleLicenseInformation()).Count<ModuleLicenseInfo>((Func<ModuleLicenseInfo, bool>) (m => !string.Equals("DPI", m.get_ModuleName(), StringComparison.OrdinalIgnoreCase) && m.get_IsEval() && !m.get_IsRC()));
      int count1 = this.GetModuleSaturationInformation().Count;
      int count2 = this.GetMaintenanceRenewalNotificationItems(false).Count;
      return new LicenseAndManagementInfo()
      {
        UpdatesAvailableCount = (__Null) count2,
        LicenseLimitReachedCount = (__Null) count1,
        EvaluationExpiringCount = (__Null) num4,
        MaintenanceExpiringCount = (__Null) num2,
        MaintenanceActiveCount = (__Null) num1,
        MaintenanceExpiredCount = (__Null) num3
      };
    }

    private IEnumerable<SupportCase> GetSupportCasesInternal(string username)
    {
      return (IEnumerable<SupportCase>) new RemoteSupportCasesClient(username).GetSupportCases().Select<WebSupportCase, SupportCase>((Func<WebSupportCase, SupportCase>) (c => c.ToSupportCase())).ToList<SupportCase>();
    }

    private ExpirableCache<string, IEnumerable<MaintenanceStatus>> MaintenanceInfoCache
    {
      get
      {
        return this._maintenanceInfoCache ?? (this._maintenanceInfoCache = new ExpirableCache<string, IEnumerable<MaintenanceStatus>>(TimeSpan.FromMinutes(5.0), new Func<string, IEnumerable<MaintenanceStatus>>(this.GetMaintenanceInfoFromCustomerPortalInternal)));
      }
    }

    private ExpirableCache<string, LicenseAndManagementInfo> LAMInfoCache
    {
      get
      {
        return this._LAMInfoCache ?? (this._LAMInfoCache = new ExpirableCache<string, LicenseAndManagementInfo>(TimeSpan.FromMinutes(1.0), new Func<string, LicenseAndManagementInfo>(this.GetLicenseAndMaintenanceSummaryInternal)));
      }
    }

    private ExpirableCache<string, IEnumerable<SupportCase>> SupportCasesCache
    {
      get
      {
        return this._supportCasesCache ?? (this._supportCasesCache = new ExpirableCache<string, IEnumerable<SupportCase>>(TimeSpan.FromMinutes(5.0), new Func<string, IEnumerable<SupportCase>>(this.GetSupportCasesInternal)));
      }
    }

    public WebResources GetSpecificResources(int viewID, string queryFilterString)
    {
      return WebResourcesDAL.GetSpecificResources(viewID, queryFilterString);
    }

    public void DeleteResource(int resourceId)
    {
      WebResourcesDAL.DeleteResource(resourceId);
    }

    public void DeleteResourceProperties(int resourceId)
    {
      WebResourcesDAL.DeleteResourceProperties(resourceId);
    }

    public int InsertNewResource(WebResource resource, int viewID)
    {
      return WebResourcesDAL.InsertNewResource(resource, viewID);
    }

    public void InsertNewResourceProperty(
      int resourceID,
      string propertyName,
      string propertyValue)
    {
      WebResourcesDAL.InsertNewResourceProperty(resourceID, propertyName, propertyValue);
    }

    public string GetSpecificResourceProperty(int resourceID, string queryFilterString)
    {
      return WebResourcesDAL.GetSpecificResourceProperty(resourceID, queryFilterString);
    }

    public void UpdateResourceProperty(int resourceID, string propertyName, string propertyValue)
    {
      WebResourcesDAL.UpdateResourceProperty(resourceID, propertyName, propertyValue);
    }

    public void SaveWebPerformanceMeasurements(IEnumerable<PerformanceMeasurement> measurements)
    {
      if (measurements == null)
        throw new ArgumentNullException(nameof (measurements));
      if (!measurements.Any<PerformanceMeasurement>())
        return;
      M0 service1 = this.ServiceContainer.GetService<ITimeSeriesDataInsertor>();
      if (service1 == null)
        throw new NullReferenceException("ITimeSeriesDataInsertor");
      M0 service2 = this.ServiceContainer.GetService<IPerformanceResourceIndex>();
      if (service2 == null)
        throw new NullReferenceException("IPerformanceResourceIndex");
      IPerformanceResourceIndex resourceIndex = (IPerformanceResourceIndex) service2;
      IEnumerable<object[]> objArrays = measurements.Select<PerformanceMeasurement, object[]>((Func<PerformanceMeasurement, object[]>) (m => new object[18]
      {
        (object) DateTime.UtcNow,
        (object) 1,
        (object) (int) m.get_ResourceKey().get_ResourceType(),
        (object) this.GetResourceId(m, resourceIndex),
        (object) (int) m.get_OperationType(),
        (object) (int) m.ExecutionCount,
        (object) (int) m.ExecutionTimeSum,
        (object) (long) m.ExecutionTimeSquaredSum,
        (object) (int) m.Bucket_0,
        (object) (int) m.Bucket_50,
        (object) (int) m.Bucket_100,
        (object) (int) m.Bucket_200,
        (object) (int) m.Bucket_500,
        (object) (int) m.Bucket_1000,
        (object) (int) m.Bucket_2000,
        (object) (int) m.Bucket_5000,
        (object) (int) m.Bucket_10000,
        (object) (int) m.Bucket_20000
      }));
      ((ITimeSeriesDataInsertor) service1).InsertData("WebsiteLoadTime_CS", CoreBusinessLayerService.dataColumns, (IEnumerable<IReadOnlyList<object>>) objArrays);
    }

    private int GetResourceId(
      PerformanceMeasurement measurement,
      IPerformanceResourceIndex resourceIndex)
    {
      if (measurement == null)
        throw new ArgumentNullException(nameof (measurement));
      switch (measurement.get_ResourceKey())
      {
        case IntKeyBase intKeyBase:
          return intKeyBase.get_Id();
        case StringKeyBase resourceId:
          return resourceIndex.GetResourceId(resourceId);
        default:
          throw new NotSupportedException("Measurement id of type " + ((object) measurement.get_ResourceKey()).GetType().Name + " is not supported");
      }
    }

    public void UpdateWebsiteInfo(string serverName, string ipAddress, int port)
    {
      WebsitesDAL.UpdateWebsiteInfo(serverName, ipAddress, port);
    }

    public string GetSiteAddress()
    {
      return WebsitesDAL.GetSiteAddress();
    }

    public bool IsHttpsUsed()
    {
      return WebsitesDAL.IsHttpsUsed();
    }

    private static NotificationItemType DalToWfc(NotificationItemTypeDAL dal)
    {
      return dal == null ? (NotificationItemType) null : new NotificationItemType(dal.Id, dal.TypeName, dal.Module, dal.Caption, dal.DetailsUrl, dal.DetailsCaption, dal.Icon, dal.Description, dal.DisplayAs, dal.RequiredRoles.ToArray(), dal.CustomDismissButtonText, dal.HideDismissButton);
    }

    public NotificationItemType GetNotificationItemTypeById(Guid typeId)
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemTypeDAL.GetTypeById.");
      try
      {
        return CoreBusinessLayerService.DalToWfc(NotificationItemTypeDAL.GetTypeById(typeId));
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining notification item type: " + ex.ToString()));
        throw new Exception(string.Format("Error obtaining notification item type: ID={0}.", (object) typeId));
      }
    }

    public List<NotificationItemType> GetNotificationItemTypes()
    {
      CoreBusinessLayerService.log.Debug((object) "Sending request for NotificationItemTypeDAL.GetTypes.");
      try
      {
        List<NotificationItemType> notificationItemTypeList = new List<NotificationItemType>();
        foreach (NotificationItemTypeDAL type in (IEnumerable<NotificationItemTypeDAL>) NotificationItemTypeDAL.GetTypes())
          notificationItemTypeList.Add(CoreBusinessLayerService.DalToWfc(type));
        return notificationItemTypeList;
      }
      catch (Exception ex)
      {
        CoreBusinessLayerService.log.Error((object) ("Error obtaining notification item types collection: " + ex.ToString()));
        throw new Exception("Error obtaining notification item types collection.");
      }
    }

    public Dictionary<string, string> GetServicesDisplayNames(List<string> servicesNames)
    {
      return ServiceManager.Instance.GetServicesDisplayNames(servicesNames);
    }

    public Dictionary<string, WindowsServiceRestartState> GetServicesStates(
      List<string> servicesNames)
    {
      return ServiceManager.Instance.GetServicesStates(servicesNames);
    }

    public void RestartServices(List<string> servicesNames)
    {
      ServiceManager.Instance.RestartServices(servicesNames);
    }

    public bool ValidateWMI(string ip, string userName, string password)
    {
      return this.ValidateWMIWithErrorMessage(ip, userName, password, out string _);
    }

    public bool ValidateWMIWithErrorMessage(
      string ip,
      string userName,
      string password,
      out string localizedErrorMessage)
    {
      JobDescription jobDescription = WmiJob<WmiValidateCredentialJobResults>.CreateJobDescription<WmiValidateCredentialJob>(ip, SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retries", 0), Convert.ToBoolean(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Auto Correct Reverse DNS", 0)), SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Default Root Namespace Override Index", 0), SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retry Interval", 0), BusinessLayerSettings.Instance.TestJobTimeout);
      WmiCredentials wmiCredentials1 = new WmiCredentials();
      wmiCredentials1.set_Password(password);
      wmiCredentials1.set_UserName(userName);
      WmiCredentials wmiCredentials2 = wmiCredentials1;
      localizedErrorMessage = string.Empty;
      WmiValidateCredentialJobResults result = this.ExecuteJobAndGetResult<WmiValidateCredentialJobResults>(jobDescription, (CredentialBase) wmiCredentials2, JobResultDataFormatType.Xml, "WMI", out string _);
      if (!((TestJobResult) result).get_Success())
      {
        localizedErrorMessage = ((TestJobResult) result).get_Message();
        return false;
      }
      CoreBusinessLayerService.log.InfoFormat("WMI credential test finished. Success: {0}", (object) result.get_CredentialsValid());
      return result.get_CredentialsValid();
    }

    public int? InsertWmiCredential(UsernamePasswordCredential credential, string owner)
    {
      new CredentialManager().AddCredential<UsernamePasswordCredential>(owner, (M0) credential);
      return ((Credential) credential).get_ID();
    }

    public UsernamePasswordCredential GetWmiCredential(int credentialID)
    {
      return (UsernamePasswordCredential) new CredentialManager().GetCredential<UsernamePasswordCredential>(credentialID);
    }

    public string GetWmiSysName(string ip, string userName, string password)
    {
      JobDescription jobDescription = WmiJob<GetSysNameJobResult>.CreateJobDescription<WmiGetSysNameJob>(ip, SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retries", 0), Convert.ToBoolean(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Auto Correct Reverse DNS", 0)), SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Default Root Namespace Override Index", 0), SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retry Interval", 0), BusinessLayerSettings.Instance.TestJobTimeout);
      WmiCredentials wmiCredentials1 = new WmiCredentials();
      wmiCredentials1.set_Password(password);
      wmiCredentials1.set_UserName(userName);
      WmiCredentials wmiCredentials2 = wmiCredentials1;
      GetSysNameJobResult result = this.ExecuteJobAndGetResult<GetSysNameJobResult>(jobDescription, (CredentialBase) wmiCredentials2, JobResultDataFormatType.Xml, "WMI", out string _);
      CoreBusinessLayerService.log.InfoFormat("Wmi GetSysName job finished. SysName: {0}", (object) result.get_SysName());
      return result.get_SysName();
    }

    private IServiceProvider ServiceContainer
    {
      get
      {
        return this._serviceContainer ?? this.parent.ServiceContainer;
      }
    }

    public CoreBusinessLayerService(
      CoreBusinessLayerPlugin pluginParent,
      IOneTimeJobManager oneTimeJobManager,
      int engineId)
      : this(pluginParent, (IPackageManager) SolarWinds.Orion.Core.Common.PackageManager.PackageManager.InstanceWithCache, (INodeBLDAL) new NodeBLDAL(), (IAgentInfoDAL) new AgentInfoDAL(), (ISettingsDAL) new SettingsDAL(), oneTimeJobManager, (IEngineDAL) new EngineDAL(), (IEngineIdentityProvider) new EngineIdentityProvider(), engineId)
    {
    }

    internal CoreBusinessLayerService(
      CoreBusinessLayerPlugin pluginParent,
      IPackageManager packageManager,
      INodeBLDAL nodeBlDal,
      IAgentInfoDAL agentInfoDal,
      ISettingsDAL settingsDal,
      IOneTimeJobManager oneTimeJobManager,
      IEngineDAL engineDal,
      IEngineIdentityProvider engineIdentityProvider,
      int engineId)
    {
      if (nodeBlDal == null)
        throw new ArgumentNullException(nameof (nodeBlDal));
      if (agentInfoDal == null)
        throw new ArgumentNullException(nameof (agentInfoDal));
      if (settingsDal == null)
        throw new ArgumentNullException(nameof (settingsDal));
      if (oneTimeJobManager == null)
        throw new ArgumentNullException(nameof (oneTimeJobManager));
      if (engineDal == null)
        throw new ArgumentNullException(nameof (engineDal));
      if (engineIdentityProvider == null)
        throw new ArgumentNullException(nameof (engineIdentityProvider));
      this.parent = pluginParent;
      this._nodeBlDal = nodeBlDal;
      this._agentInfoDal = agentInfoDal;
      this._settingsDal = settingsDal;
      this._auditPluginManager.Initialize();
      this._areInterfacesSupported = packageManager.IsPackageInstalled("Orion.Interfaces");
      this._oneTimeJobManager = oneTimeJobManager;
      this._engineDal = engineDal;
      this._engineIdentityProvider = engineIdentityProvider;
      this._serviceLogicalInstanceId = CoreBusinessLayerConfiguration.GetLogicalInstanceId(engineId);
      SnmpSettings.get_Instance().add_Changed(new EventHandler<SettingsChangedEventArgs>(this.SnmpEncodingSettingsChanged));
    }

    internal CoreBusinessLayerService(IServiceProvider serviceContainer)
    {
      this._serviceContainer = serviceContainer;
    }

    public EventWaitHandle ShutdownWaitHandle
    {
      get
      {
        return (EventWaitHandle) this.shutdownEvent;
      }
    }

    public void Shutdown()
    {
      if (!(JobScheduler.GetInstance() is IChannel instance))
        return;
      MessageUtilities.ShutdownCommunicationObject((ICommunicationObject) instance);
    }

    internal bool AreInterfacesSupported
    {
      get
      {
        return this._areInterfacesSupported;
      }
    }

    public void CheckBLConnection()
    {
    }

    public void Dispose()
    {
    }

    public IEnumerable<ServiceEndpointDescriptor> GetServiceEndpointDescriptors(
      ServiceDescription serviceDescription)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<ServiceEndpointDescriptor>) new CoreBusinessLayerService.\u003CGetServiceEndpointDescriptors\u003Ed__520(-2)
      {
        \u003C\u003E3__serviceDescription = serviceDescription
      };
    }

    private CoreBusinessLayerServiceInstance GetCurrentServiceInstance()
    {
      return this.parent.GetServiceInstance(this.GetCurrentOperationEngineId());
    }

    private int GetCurrentOperationEngineId()
    {
      IEngineIdentity iengineIdentity;
      if (!this._engineIdentityProvider.TryGetCurrent(ref iengineIdentity))
        throw new InvalidOperationException("Failed to retrieve current EngineId from the operation context.");
      return iengineIdentity.get_EngineId();
    }

    public string ServiceLogicalInstanceId
    {
      get
      {
        CoreBusinessLayerService.log.Info((object) ("Registering to service directory, ServiceId: Core.BusinessLayer, ServiceLogicalInstanceId: " + this._serviceLogicalInstanceId));
        return this._serviceLogicalInstanceId;
      }
    }

    public Version ServiceInstanceVersion
    {
      get
      {
        return CoreBusinessLayerService.CoreBusinessLayerServiceVersion;
      }
    }

    static CoreBusinessLayerService()
    {
      Dictionary<string, Action<PollingEngineStatus, object>> dictionary = new Dictionary<string, Action<PollingEngineStatus, object>>();
      dictionary.Add("NetPerfMon Engine:Network Node Elements", (Action<PollingEngineStatus, object>) ((s, o) => s.set_NetworkNodeElements(Convert.ToInt32(o))));
      dictionary.Add("NetPerfMon Engine:Interface Elements", (Action<PollingEngineStatus, object>) ((s, o) => s.set_InterfaceElements(Convert.ToInt32(o))));
      dictionary.Add("NetPerfMon Engine:Volume Elements", (Action<PollingEngineStatus, object>) ((s, o) => s.set_VolumeElements(Convert.ToInt32(o))));
      dictionary.Add("NetPerfMon Engine:Date Time", (Action<PollingEngineStatus, object>) ((s, o) => s.set_DateTime(Convert.ToDateTime(o))));
      dictionary.Add("NetPerfMon Engine:Paused", (Action<PollingEngineStatus, object>) ((s, o) => s.set_Paused(Convert.ToBoolean(o))));
      dictionary.Add("Max Outstanding Polls", (Action<PollingEngineStatus, object>) ((s, o) => s.set_MaxOutstandingPolls(Convert.ToInt32(o))));
      dictionary.Add("Status Pollers:ICMP Status Polling Index", (Action<PollingEngineStatus, object>) ((s, o) => s.set_ICMPStatusPollingIndex(o.ToString())));
      dictionary.Add("Status Pollers:SNMP Status Polling Index", (Action<PollingEngineStatus, object>) ((s, o) => s.set_SNMPStatusPollingIndex(o.ToString())));
      dictionary.Add("Status Pollers:ICMP Polls per second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_ICMPStatusPollsPerSecond(Convert.ToDouble(o))));
      dictionary.Add("Status Pollers:SNMP Polls per second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_SNMPStatusPollsPerSecond(Convert.ToDouble(o))));
      dictionary.Add("Packet Queues:DNS Outstanding", (Action<PollingEngineStatus, object>) ((s, o) => s.set_DNSOutstanding(Convert.ToInt32(o))));
      dictionary.Add("Packet Queues:ICMP Outstanding", (Action<PollingEngineStatus, object>) ((s, o) => s.set_ICMPOutstanding(Convert.ToInt32(o))));
      dictionary.Add("Packet Queues:SNMP Outstanding", (Action<PollingEngineStatus, object>) ((s, o) => s.set_SNMPOutstanding(Convert.ToInt32(o))));
      dictionary.Add("Statistics Pollers:ICMP Statistic Polling Index", (Action<PollingEngineStatus, object>) ((s, o) => s.set_ICMPStatisticPollingIndex(o.ToString())));
      dictionary.Add("Statistics Pollers:SNMP Statistic Polling Index", (Action<PollingEngineStatus, object>) ((s, o) => s.set_SNMPStatisticPollingIndex(o.ToString())));
      dictionary.Add("Statistics Pollers:ICMP Polls per second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_ICMPStatisticPollsPerSecond(Convert.ToDouble(o))));
      dictionary.Add("Statistics Pollers:SNMP Polls per second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_SNMPStatisticPollsPerSecond(Convert.ToDouble(o))));
      dictionary.Add("Status Pollers:Max Status Polls Per Second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_MaxStatusPollsPerSecond(Convert.ToInt32(o))));
      dictionary.Add("Statistics Pollers:Max Statistic Polls Per Second", (Action<PollingEngineStatus, object>) ((s, o) => s.set_MaxStatisticPollsPerSecond(Convert.ToInt32(o))));
      CoreBusinessLayerService.statusParsers = dictionary;
      List<NodeSubType> nodeSubTypeList = new List<NodeSubType>();
      nodeSubTypeList.Add((NodeSubType) 4);
      nodeSubTypeList.Add((NodeSubType) 3);
      CoreBusinessLayerService.WmiCompatibleNodeSubTypes = nodeSubTypeList;
      CoreBusinessLayerService.dataColumns = (IReadOnlyList<string>) new List<string>()
      {
        "Timestamp",
        "Weight",
        "WebResourceTypeId",
        "ResourceId",
        "OperationTypeId",
        "ExecutionCount",
        "ExecutionTimeSum",
        "ExecutionTimeSquaredSum",
        "Bucket_0",
        "Bucket_50",
        "Bucket_100",
        "Bucket_200",
        "Bucket_500",
        "Bucket_1000",
        "Bucket_2000",
        "Bucket_5000",
        "Bucket_10000",
        "Bucket_20000"
      }.AsReadOnly();
      CoreBusinessLayerService.CoreBusinessLayerServiceVersion = typeof (CoreBusinessLayerService).Assembly.GetName().Version;
      CoreBusinessLayerService.log = new Log();
    }

    public class DiscoveryBusinessLayerError : Exception
    {
      internal DiscoveryBusinessLayerError(string format, object[] args, Exception inner)
        : base(string.Format((IFormatProvider) CultureInfo.CurrentUICulture, format ?? string.Empty, args ?? new object[0]), inner)
      {
      }

      internal DiscoveryBusinessLayerError(string format, params object[] args)
        : base(string.Format((IFormatProvider) CultureInfo.CurrentUICulture, format ?? string.Empty, args ?? new object[0]))
      {
      }

      internal DiscoveryBusinessLayerError(string message, Exception inner)
        : base(message, inner)
      {
      }

      internal DiscoveryBusinessLayerError(string message)
        : base(message)
      {
      }
    }

    public class DicoveryDeletingJobError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DicoveryDeletingJobError(string format, params object[] args)
        : base(format, args)
      {
      }
    }

    [Obsolete("Core-Split cleanup. If you need this member please contact Core team", true)]
    public class DicoveryStateError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DicoveryStateError(string format, params object[] args)
        : base(format, args)
      {
      }
    }

    public class DiscoveryInsertingIgnoredNodeError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryInsertingIgnoredNodeError(string format, object[] args, Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryDeletingIgnoredNodeError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryDeletingIgnoredNodeError(string format, object[] args, Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryInsertingIgnoredInterfaceError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryInsertingIgnoredInterfaceError(
        string format,
        object[] args,
        Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryDeletingIgnoredInterfaceError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryDeletingIgnoredInterfaceError(
        string format,
        object[] args,
        Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryInsertingIgnoredVolumeError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryInsertingIgnoredVolumeError(string format, object[] args, Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryDeletingIgnoredVolumeError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryDeletingIgnoredVolumeError(string format, object[] args, Exception inner)
        : base(format, args, inner)
      {
      }
    }

    public class DiscoveryHostAddressMissingError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryHostAddressMissingError(string format, params object[] args)
        : base(format, args)
      {
      }
    }

    public class DiscoveryJobCancellationError : CoreBusinessLayerService.DiscoveryBusinessLayerError
    {
      internal DiscoveryJobCancellationError(string format, params object[] args)
        : base(format, args)
      {
      }
    }
  }
}
