// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionDiscoveryJobFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.JobEngine;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Framework.Interfaces;
using SolarWinds.Orion.Discovery.Framework.Pluggability;
using SolarWinds.Orion.Discovery.Job;
using SolarWinds.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Xml;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class OrionDiscoveryJobFactory : IJobFactory
  {
    private static readonly Log log = new Log();
    private const int DefaultJobTimeout = 60;
    private const string ListenerUri = "net.pipe://localhost/orion/core/scheduleddiscoveryjobsevents2";
    private readonly IEngineDAL engineDAL;

    public OrionDiscoveryJobFactory()
      : this((IEngineDAL) new EngineDAL())
    {
    }

    internal OrionDiscoveryJobFactory(IEngineDAL engineDal)
    {
      if (engineDal == null)
        throw new ArgumentNullException(nameof (engineDal));
      this.engineDAL = engineDal;
    }

    public string GetOrionDiscoveryJobDescriptionString(
      OrionDiscoveryJobDescription discoveryJobDescription,
      List<DiscoveryPluginInfo> pluginInfos,
      bool jsonFormat = false)
    {
      if (jsonFormat)
        return SerializationHelper.ToJson((object) discoveryJobDescription);
      DiscoveryPluginInfoCollection pluginInfoCollection1 = new DiscoveryPluginInfoCollection();
      pluginInfoCollection1.set_PluginInfos(pluginInfos);
      DiscoveryPluginInfoCollection pluginInfoCollection2 = pluginInfoCollection1;
      List<Type> typeList = new List<Type>();
      using (List<DiscoveryPluginJobDescriptionBase>.Enumerator enumerator = discoveryJobDescription.get_DiscoveryPluginJobDescriptions().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DiscoveryPluginJobDescriptionBase current = enumerator.Current;
          if (!typeList.Contains(((object) current).GetType()))
            typeList.Add(((object) current).GetType());
        }
      }
      return SerializationHelper.XmlWrap(new List<string>()
      {
        SerializationHelper.ToXmlString((object) pluginInfoCollection2),
        SerializationHelper.ToXmlString((object) discoveryJobDescription, (IEnumerable<Type>) typeList)
      });
    }

    public void GetOrionDiscoveryJobDescriptionXml(
      OrionDiscoveryJobDescription discoveryJobDescription,
      List<DiscoveryPluginInfo> pluginInfos,
      XmlWriter xmlWriter)
    {
      IEnumerable<Type> types = ((IEnumerable<DiscoveryPluginJobDescriptionBase>) discoveryJobDescription.get_DiscoveryPluginJobDescriptions()).Select<DiscoveryPluginJobDescriptionBase, Type>((Func<DiscoveryPluginJobDescriptionBase, Type>) (pjd => ((object) pjd).GetType())).Distinct<Type>();
      XmlReader[] xmlReaderArray = new XmlReader[2];
      DiscoveryPluginInfoCollection pluginInfoCollection = new DiscoveryPluginInfoCollection();
      pluginInfoCollection.set_PluginInfos(pluginInfos);
      xmlReaderArray[0] = SerializationHelper.ToXmlReader((object) pluginInfoCollection);
      xmlReaderArray[1] = SerializationHelper.ToXmlReader((object) discoveryJobDescription, types);
      SerializationHelper.XmlWrap((IEnumerable<XmlReader>) xmlReaderArray, xmlWriter);
    }

    public ScheduledJob CreateDiscoveryJob(DiscoveryConfiguration configuration)
    {
      return this.CreateDiscoveryJob(configuration, (IDiscoveryPluginFactory) new DiscoveryPluginFactory());
    }

    internal static DiscoveryPollingEngineType? GetDiscoveryPollingEngineType(
      int engineId,
      IEngineDAL engineDal = null)
    {
      engineDal = engineDal ?? (IEngineDAL) new EngineDAL();
      Engine engine = engineDal.GetEngine(engineId);
      if (engine.get_ServerType().Equals("BranchOffice"))
        engine.set_ServerType("RemoteCollector");
      DiscoveryPollingEngineType result;
      if (Enum.TryParse<DiscoveryPollingEngineType>(engine.get_ServerType(), true, out result))
        return new DiscoveryPollingEngineType?(result);
      if (OrionDiscoveryJobFactory.log.get_IsErrorEnabled())
        OrionDiscoveryJobFactory.log.Error((object) ("Unable to determine DiscoveryPollingEngineType value for engine server type '" + engine.get_ServerType() + "'"));
      return new DiscoveryPollingEngineType?();
    }

    internal static bool IsDiscoveryPluginSupportedForDiscoveryPollingEngineType(
      IDiscoveryPlugin plugin,
      DiscoveryPollingEngineType discovryPollingEngineType,
      IDictionary<IDiscoveryPlugin, DiscoveryPluginInfo> pluginInfoPairs)
    {
      return pluginInfoPairs.ContainsKey(plugin) && ((IEnumerable<DiscoveryPollingEngineType>) pluginInfoPairs[plugin].get_SupportedPollingEngineTypes()).Contains<DiscoveryPollingEngineType>(discovryPollingEngineType);
    }

    public ScheduledJob CreateDiscoveryJob(
      DiscoveryConfiguration configuration,
      IDiscoveryPluginFactory pluginFactory)
    {
      if (configuration == null)
        throw new ArgumentNullException(nameof (configuration));
      Engine engine = this.engineDAL.GetEngine(((DiscoveryConfigurationBase) configuration).get_EngineId());
      DiscoveryPollingEngineType? pollingEngineType = OrionDiscoveryJobFactory.GetDiscoveryPollingEngineType(configuration.get_EngineID(), this.engineDAL);
      int result;
      if (!int.TryParse(SettingsDAL.Get("SWNetPerfMon-Settings-SNMP MaxReps"), out result))
        result = 5;
      OrionDiscoveryJobDescription discoveryJobDescription1 = new OrionDiscoveryJobDescription();
      discoveryJobDescription1.set_ProfileId(((DiscoveryConfigurationBase) configuration).get_ProfileId());
      discoveryJobDescription1.set_EngineId(((DiscoveryConfigurationBase) configuration).get_EngineId());
      discoveryJobDescription1.set_HopCount(configuration.get_HopCount());
      discoveryJobDescription1.set_IcmpTimeout(configuration.get_SearchTimeout());
      DiscoveryCommonSnmpConfiguration snmpConfiguration = new DiscoveryCommonSnmpConfiguration();
      snmpConfiguration.set_MaxSnmpReplies(result);
      snmpConfiguration.set_SnmpRetries(configuration.get_SnmpRetries());
      snmpConfiguration.set_SnmpTimeout(configuration.get_SnmpTimeout());
      snmpConfiguration.set_SnmpPort(configuration.get_SnmpPort());
      snmpConfiguration.set_PreferredSnmpVersion(configuration.get_PreferredSnmpVersion());
      discoveryJobDescription1.set_SnmpConfiguration(snmpConfiguration);
      discoveryJobDescription1.set_DisableICMP(configuration.get_DisableICMP());
      discoveryJobDescription1.set_PreferredPollingMethod(((CoreDiscoveryPluginConfiguration) ((DiscoveryConfigurationBase) configuration).GetDiscoveryPluginConfiguration<CoreDiscoveryPluginConfiguration>()).get_PreferredPollingMethod());
      discoveryJobDescription1.set_VulnerabilityCheckDisabled(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-VulnerabilityCheckDisabled", 0) == 1);
      discoveryJobDescription1.set_MaxThreadsInDetectionPhase(SettingsDAL.GetCurrentInt("Discovery-MaxThreadsInDetectionPhase", 5));
      discoveryJobDescription1.set_MaxThreadsInInventoryPhase(SettingsDAL.GetCurrentInt("Discovery-MaxThreadsInInventoryPhase", 5));
      discoveryJobDescription1.set_PreferredDnsAddressFamily(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-Default Preferred AddressFamily DHCP", 4));
      discoveryJobDescription1.set_TagFilter(configuration.get_TagFilter());
      discoveryJobDescription1.set_DefaultProbes(configuration.get_DefaultProbes());
      OrionDiscoveryJobDescription discoveryJobDescription2 = discoveryJobDescription1;
      List<DiscoveryPluginInfo> discoveryPluginInfos = DiscoveryPluginFactory.GetDiscoveryPluginInfos();
      IList<IDiscoveryPlugin> plugins = pluginFactory.GetPlugins((IList<DiscoveryPluginInfo>) discoveryPluginInfos);
      List<DiscoveryPluginInfo> pluginInfos = new List<DiscoveryPluginInfo>();
      IDictionary<IDiscoveryPlugin, DiscoveryPluginInfo> pairsPluginAndInfo = DiscoveryPluginHelper.CreatePairsPluginAndInfo((IEnumerable<IDiscoveryPlugin>) plugins, (IEnumerable<DiscoveryPluginInfo>) discoveryPluginInfos);
      bool flag1 = RegistrySettings.IsFreePoller();
      using (IEnumerator<IDiscoveryPlugin> enumerator = ((IEnumerable<IDiscoveryPlugin>) plugins).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveryPlugin current = enumerator.Current;
          if (flag1 && !(current is ISupportFreeEngine))
            OrionDiscoveryJobFactory.log.DebugFormat("Discovery plugin {0} is not supported on FPE machine", (object) current);
          else if (!((DiscoveryConfigurationBase) configuration).get_ProfileId().HasValue && !(current is IOneTimeJobSupport))
          {
            OrionDiscoveryJobFactory.log.DebugFormat("Plugin {0} is not supporting one time job and it's description woun't be added.", (object) ((object) current).GetType().FullName);
          }
          else
          {
            if (configuration.get_TagFilter() != null && configuration.get_TagFilter().Any<string>())
            {
              if (!(current is IDiscoveryPluginTags idiscoveryPluginTags))
              {
                OrionDiscoveryJobFactory.log.DebugFormat("Discovery job for tags requested, however plugin {0} doesn't support tags, skipping.", (object) current);
                continue;
              }
              if (!configuration.get_TagFilter().Intersect<string>(idiscoveryPluginTags.get_Tags() ?? Enumerable.Empty<string>(), (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase).Any<string>())
              {
                OrionDiscoveryJobFactory.log.DebugFormat("Discovery job for tags [{0}], however plugin {1} doesn't support any of the tags requested, skipping.", (object) string.Join(",", (IEnumerable<string>) configuration.get_TagFilter()), (object) current);
                continue;
              }
            }
            if (configuration.get_IsAgentJob() && (!(current is IAgentPluginJobSupport pluginJobSupport) || !((IEnumerable<string>) configuration.get_AgentPlugins()).Contains<string>(pluginJobSupport.get_PluginId())))
              OrionDiscoveryJobFactory.log.DebugFormat("Plugin {0} is not contained in supported agent plugins and will not be used.", (object) ((object) current).GetType().FullName);
            else if (pollingEngineType.HasValue && !OrionDiscoveryJobFactory.IsDiscoveryPluginSupportedForDiscoveryPollingEngineType(current, pollingEngineType.Value, pairsPluginAndInfo))
            {
              if (OrionDiscoveryJobFactory.log.get_IsDebugEnabled())
                OrionDiscoveryJobFactory.log.DebugFormat(string.Format("Plugin {0} is not supported for polling engine {1}", (object) ((object) current).GetType().FullName, (object) configuration.get_EngineID()), Array.Empty<object>());
            }
            else
            {
              Exception exception = (Exception) null;
              DiscoveryPluginJobDescriptionBase jobDescriptionBase;
              try
              {
                jobDescriptionBase = current.GetJobDescription((DiscoveryConfigurationBase) configuration);
              }
              catch (Exception ex)
              {
                jobDescriptionBase = (DiscoveryPluginJobDescriptionBase) null;
                exception = ex;
              }
              if (jobDescriptionBase == null)
              {
                string str = "Plugin " + ((object) current).GetType().FullName + " was not able found valid job description.";
                if (exception != null)
                  OrionDiscoveryJobFactory.log.Warn((object) str, exception);
                else
                  OrionDiscoveryJobFactory.log.Warn((object) str);
              }
              else
              {
                discoveryJobDescription2.get_DiscoveryPluginJobDescriptions().Add(jobDescriptionBase);
                DiscoveryPluginInfo discoveryPluginInfo = pairsPluginAndInfo[current];
                pluginInfos.Add(discoveryPluginInfo);
              }
            }
          }
        }
      }
      JobDescription jobDescription1 = new JobDescription();
      jobDescription1.set_TypeName(typeof (OrionDiscoveryJob).AssemblyQualifiedName);
      jobDescription1.set_JobDetailConfiguration(this.GetOrionDiscoveryJobDescriptionString(discoveryJobDescription2, pluginInfos, configuration.get_UseJsonFormat()));
      jobDescription1.set_JobNamespace("orion");
      jobDescription1.set_ResultTTL(TimeSpan.FromMinutes(10.0));
      jobDescription1.set_TargetNode(new HostAddress(IPAddressHelper.ToStringIp(engine.get_IP()), (AddressType) 4));
      jobDescription1.set_LegacyEngine(engine.get_ServerName().ToLowerInvariant());
      jobDescription1.set_EndpointAddress(configuration.get_IsAgentJob() ? configuration.get_AgentAddress() : (string) null);
      jobDescription1.set_SupportedRoles((PackageType) 7);
      JobDescription jobDescription2 = jobDescription1;
      jobDescription2.set_Timeout(OrionDiscoveryJobFactory.GetDiscoveryJobTimeout(configuration));
      ScheduledJob scheduledJob1;
      if (configuration.get_CronSchedule() != null)
      {
        bool flag2 = false;
        string str = configuration.get_CronSchedule().get_CronExpression();
        DateTime dateTime;
        if (string.IsNullOrWhiteSpace(str))
        {
          dateTime = configuration.get_CronSchedule().get_StartTime();
          DateTime localTime = dateTime.ToLocalTime();
          if (localTime < DateTime.Now)
          {
            OrionDiscoveryJobFactory.log.InfoFormat("Profile (ID={0}) with past Once(Cron) schedule. We should not create job for it.", (object) configuration.get_ProfileID());
            return (ScheduledJob) null;
          }
          str = string.Format("{0} {1} {2} {3} *", (object) localTime.Minute, (object) localTime.Hour, (object) localTime.Day, (object) localTime.Month);
          flag2 = true;
        }
        scheduledJob1 = new ScheduledJob(jobDescription2, str, "net.pipe://localhost/orion/core/scheduleddiscoveryjobsevents2", configuration.get_ProfileID().ToString());
        scheduledJob1.set_RunOnce(flag2);
        scheduledJob1.set_TimeZoneInfo(TimeZoneInfo.Local);
        if (!flag2)
        {
          ScheduledJob scheduledJob2 = scheduledJob1;
          dateTime = configuration.get_CronSchedule().get_StartTime();
          DateTime universalTime1 = dateTime.ToUniversalTime();
          scheduledJob2.set_Start(universalTime1);
          DateTime? endTime1 = configuration.get_CronSchedule().get_EndTime();
          dateTime = DateTime.MaxValue;
          if ((endTime1.HasValue ? (endTime1.HasValue ? (endTime1.GetValueOrDefault() != dateTime ? 1 : 0) : 0) : 1) != 0)
          {
            DateTime? endTime2 = configuration.get_CronSchedule().get_EndTime();
            if (endTime2.HasValue)
            {
              ScheduledJob scheduledJob3 = scheduledJob1;
              endTime2 = configuration.get_CronSchedule().get_EndTime();
              dateTime = endTime2.Value;
              DateTime universalTime2 = dateTime.ToUniversalTime();
              scheduledJob3.set_End(universalTime2);
            }
          }
        }
      }
      else
        scheduledJob1 = configuration.get_ScheduleRunAtTime().Equals(DateTime.MinValue) ? new ScheduledJob(jobDescription2, configuration.get_ScheduleRunFrequency(), "net.pipe://localhost/orion/core/scheduleddiscoveryjobsevents2", configuration.get_ProfileID().ToString()) : new ScheduledJob(jobDescription2, configuration.get_ScheduleRunAtTime(), "net.pipe://localhost/orion/core/scheduleddiscoveryjobsevents2", configuration.get_ProfileID().ToString());
      return scheduledJob1;
    }

    private static TimeSpan GetDiscoveryJobTimeout(DiscoveryConfiguration configuration)
    {
      if (configuration.get_IsAgentJob())
        return BusinessLayerSettings.Instance.AgentDiscoveryJobTimeout;
      return configuration.get_JobTimeout() == TimeSpan.Zero || configuration.get_JobTimeout() == TimeSpan.MinValue ? TimeSpan.FromMinutes(60.0) : configuration.get_JobTimeout();
    }

    private Guid SubmitScheduledJobToScheduler(
      Guid jobId,
      ScheduledJob job,
      bool executeImmediately,
      bool useLocal)
    {
      using (IJobSchedulerHelper ijobSchedulerHelper = useLocal ? JobScheduler.GetLocalInstance() : JobScheduler.GetMainInstance())
      {
        if (jobId == Guid.Empty)
        {
          OrionDiscoveryJobFactory.log.Debug((object) "Adding new job to Job Engine");
          return ((IJobScheduler) ijobSchedulerHelper).AddJob(job);
        }
        try
        {
          OrionDiscoveryJobFactory.log.DebugFormat("Updating job definition in Job Engine ({0})", (object) jobId);
          ((IJobScheduler) ijobSchedulerHelper).UpdateJob(jobId, job, executeImmediately);
          return jobId;
        }
        catch (FaultException<JobEngineConnectionFault> ex)
        {
          OrionDiscoveryJobFactory.log.DebugFormat("Unable to update job definition in Job Engine({0}", (object) jobId);
          throw;
        }
        catch (Exception ex)
        {
          OrionDiscoveryJobFactory.log.DebugFormat("Unable to update job definition in Job Engine({0}", (object) jobId);
        }
        OrionDiscoveryJobFactory.log.Debug((object) "Adding new job to Job Engine");
        return ((IJobScheduler) ijobSchedulerHelper).AddJob(job);
      }
    }

    public Guid SubmitScheduledJob(Guid jobId, ScheduledJob job, bool executeImmediately)
    {
      return this.SubmitScheduledJobToScheduler(jobId, job, executeImmediately, false);
    }

    public Guid SubmitScheduledJobToLocalEngine(
      Guid jobId,
      ScheduledJob job,
      bool executeImmediately)
    {
      return this.SubmitScheduledJobToScheduler(jobId, job, executeImmediately, true);
    }

    public bool DeleteJob(Guid jobId)
    {
      using (IJobSchedulerHelper localInstance = JobScheduler.GetLocalInstance())
      {
        try
        {
          ((IJobScheduler) localInstance).RemoveJob(jobId);
          return true;
        }
        catch
        {
          OrionDiscoveryJobFactory.log.DebugFormat("Unable to delete job in Job Engine({0}", (object) jobId);
          return false;
        }
      }
    }
  }
}
