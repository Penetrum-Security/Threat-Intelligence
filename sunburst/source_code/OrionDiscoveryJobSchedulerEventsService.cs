// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionDiscoveryJobSchedulerEventsService
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Linq;
using SolarWinds.InformationService.Linq.Plugins;
using SolarWinds.InformationService.Linq.Plugins.Core.Orion;
using SolarWinds.JobEngine;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Common.IO;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.BusinessLayer.Discovery;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Agent;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Common.JobEngine;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.Core.Discovery;
using SolarWinds.Orion.Core.Discovery.DAL;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using SolarWinds.Orion.Core.Models.DiscoveredObjects;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Enums;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Core.Models.OldDiscoveryModels;
using SolarWinds.Orion.Core.SharedCredentials;
using SolarWinds.Orion.Core.Strings;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Framework.Pluggability;
using SolarWinds.Orion.Discovery.Job;
using SolarWinds.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  [ServiceBehavior(AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
  public class OrionDiscoveryJobSchedulerEventsService : JobSchedulerEventServicev2
  {
    protected static readonly Log resultLog = new Log("DiscoveryResultLog");
    private static Dictionary<int, OrionDiscoveryJobProgressInfo> profileProgress = new Dictionary<int, OrionDiscoveryJobProgressInfo>();
    private readonly TimeSpan agentPartialResultsTimeout = BusinessLayerSettings.Instance.AgentDiscoveryJobTimeout.Add(TimeSpan.FromMinutes(1.0));
    private readonly DiscoveryLogic discoveryLogic = new DiscoveryLogic();
    private readonly IOneTimeAgentDiscoveryJobFactory oneTimeAgentJobFactory;
    private readonly IAgentInfoDAL agentInfoDal;
    private readonly PartialDiscoveryResultsContainer partialResultsContainer;
    private readonly CultureInfo PrimaryLocale;
    internal Func<IJobSchedulerHelper> JobSchedulerHelperFactory;

    private static CultureInfo GetPrimaryLocale()
    {
      try
      {
        string primaryLocale = LocaleConfiguration.get_PrimaryLocale();
        JobSchedulerEventServicev2.log.Verbose((object) ("Primary locale set to " + primaryLocale));
        return new CultureInfo(primaryLocale);
      }
      catch (Exception ex)
      {
        JobSchedulerEventServicev2.log.Error((object) "Error while getting primary locale CultureInfo", ex);
      }
      return (CultureInfo) null;
    }

    public OrionDiscoveryJobSchedulerEventsService(
      CoreBusinessLayerPlugin parent,
      IOneTimeAgentDiscoveryJobFactory oneTimeAgentJobFactory)
      : this(parent, oneTimeAgentJobFactory, (IAgentInfoDAL) new AgentInfoDAL())
    {
    }

    public OrionDiscoveryJobSchedulerEventsService(
      CoreBusinessLayerPlugin parent,
      IOneTimeAgentDiscoveryJobFactory oneTimeAgentJobFactory,
      IAgentInfoDAL agentInfoDal)
      : base((IServiceStateProvider) parent)
    {
      this.oneTimeAgentJobFactory = oneTimeAgentJobFactory;
      this.agentInfoDal = agentInfoDal;
      this.resultsManager.set_HandleResultsOfCancelledJobs(true);
      this.PrimaryLocale = OrionDiscoveryJobSchedulerEventsService.GetPrimaryLocale();
      this.partialResultsContainer = new PartialDiscoveryResultsContainer();
      this.partialResultsContainer.DiscoveryResultsComplete += new EventHandler<DiscoveryResultsCompletedEventArgs>(this.partialResultsContainer_DiscoveryResultsComplete);
      this.partialResultsContainer.ClearStore();
      this.JobSchedulerHelperFactory = (Func<IJobSchedulerHelper>) (() => JobScheduler.GetLocalInstance());
    }

    private void partialResultsContainer_DiscoveryResultsComplete(
      object sender,
      DiscoveryResultsCompletedEventArgs e)
    {
      this.ProcessMergedPartialResults(e.CompleteResult, e.OrderedPlugins, e.ScheduledJobId, e.JobState, e.ProfileId);
    }

    protected override void ProcessJobProgress(JobProgress jobProgress)
    {
      Thread.CurrentThread.CurrentUICulture = this.PrimaryLocale ?? Thread.CurrentThread.CurrentUICulture;
      if (!string.IsNullOrEmpty(jobProgress.get_Progress()))
      {
        try
        {
          OrionDiscoveryJobProgressInfo progress = (OrionDiscoveryJobProgressInfo) SerializationHelper.FromXmlString<OrionDiscoveryJobProgressInfo>(jobProgress.get_Progress());
          progress.set_JobId(jobProgress.get_JobId());
          StringBuilder stringBuilder = new StringBuilder();
          foreach (KeyValuePair<string, int> discoveredNetObject in progress.get_DiscoveredNetObjects())
            stringBuilder.AppendFormat(" {0} {1};", (object) discoveredNetObject.Value, (object) discoveredNetObject.Key);
          if (progress.get_ProfileID().HasValue)
          {
            OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = OrionDiscoveryJobSchedulerEventsService.UpdateProgress(progress);
            JobSchedulerEventServicev2.log.DebugFormat("Got Discovery progress for profile {0} Status: {1} Discovered: {2} ", (object) progress.get_ProfileID(), (object) (DiscoveryStatus) progress.get_Status().Status, (object) stringBuilder);
            if (discoveryJobProgressInfo != null)
              return;
            JobSchedulerEventServicev2.log.DebugFormat("First progress of discovery profile {0}", (object) progress.get_ProfileID().Value);
            DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(progress.get_ProfileID().Value);
            if (profileById == null)
              return;
            profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 1, string.Empty));
            profileById.set_LastRun(DateTime.Now.ToUniversalTime());
            profileById.Update();
          }
          else
          {
            JobSchedulerEventServicev2.log.DebugFormat("Recieved progress for one shot discovery job [{0}] Discovered: {1} ", (object) jobProgress.get_JobId(), (object) stringBuilder);
            DiscoveryResultItem discoveryResultItem;
            if (DiscoveryResultCache.get_Instance().TryGetResultItem(jobProgress.get_JobId(), ref discoveryResultItem))
              discoveryResultItem.set_Progress(progress);
            else
              JobSchedulerEventServicev2.log.ErrorFormat("Unable to get result item {0}", (object) jobProgress.get_JobId());
          }
        }
        catch (Exception ex)
        {
          JobSchedulerEventServicev2.log.Error((object) "Exception occured when parsing job progress info.", ex);
        }
      }
      else
        JobSchedulerEventServicev2.log.Error((object) "Job progress not found");
    }

    protected override void ProcessJobFailure(FinishedJobInfo jobResult)
    {
      Thread.CurrentThread.CurrentUICulture = this.PrimaryLocale ?? Thread.CurrentThread.CurrentUICulture;
      if (this.partialResultsContainer.IsResultExpected(jobResult.get_ScheduledJobId()))
      {
        JobSchedulerEventServicev2.log.WarnFormat("Partial agent discovery job {0} failed with error '{1}'. It will be removed from discovery results.", (object) jobResult.get_ScheduledJobId(), (object) jobResult.get_Result().get_Error());
        this.partialResultsContainer.RemoveExpectedPartialResult(jobResult.get_ScheduledJobId());
      }
      else
      {
        int result;
        if (!int.TryParse(jobResult.get_State(), out result))
          return;
        string str = string.Format("A Network Discovery job has failed to complete.\r\nState: {0}\r\nProfile id: {1}.\r\nThe Job Scheduler is reporting the following error:\r\n{2}", (object) jobResult.get_Result().get_State(), (object) jobResult.get_State(), (object) jobResult.get_Result().get_Error());
        JobSchedulerEventServicev2.log.Error((object) str);
        OrionDiscoveryJobSchedulerEventsService.RemoveProgressInfo(result);
        try
        {
          DiscoveryProfileEntry profile = this.GetProfile(result, jobResult.get_ScheduledJobId());
          if (profile == null)
            return;
          if (!profile.get_IsScheduled())
            profile.set_JobID(Guid.Empty);
          profile.set_RuntimeInSeconds(0);
          profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, string.Format(Resources.get_LIBCODE_AK0_8(), (object) jobResult.get_Result().get_State(), (object) jobResult.get_State(), (object) jobResult.get_Result().get_Error())));
          profile.Update();
          if (!profile.get_IsHidden())
            return;
          this.discoveryLogic.DeleteOrionDiscoveryProfile(profile.get_ProfileID());
        }
        catch (Exception ex)
        {
          JobSchedulerEventServicev2.log.Error((object) string.Format("Unable to update profile {0}", (object) result), ex);
        }
      }
    }

    protected override void ProcessJobResult(FinishedJobInfo jobResult)
    {
      Thread.CurrentThread.CurrentUICulture = this.PrimaryLocale ?? Thread.CurrentThread.CurrentUICulture;
      JobSchedulerEventServicev2.log.DebugFormat("Recieved discovery results", Array.Empty<object>());
      if (jobResult.get_Result().get_State() == 5)
      {
        JobSchedulerEventServicev2.log.Error((object) "Job failed");
      }
      else
      {
        if (jobResult.get_Result().get_State() != 6 && jobResult.get_Result().get_State() != 4)
          return;
        int? nullable = new int?();
        using (IJobSchedulerHelper scheduler = this.JobSchedulerHelperFactory())
        {
          try
          {
            Stream resultStream = this.GetResultStream(jobResult, scheduler);
            List<DiscoveryPluginInfo> discoveryPluginInfos = DiscoveryPluginFactory.GetDiscoveryPluginInfos();
            List<IDiscoveryPlugin> idiscoveryPluginList = new List<IDiscoveryPlugin>((IEnumerable<IDiscoveryPlugin>) new DiscoveryPluginFactory().GetPlugins((IList<DiscoveryPluginInfo>) discoveryPluginInfos));
            List<Type> knownTypes = new List<Type>();
            DiscoveryResultItem discoveryResultItem;
            DiscoveryResultCache.get_Instance().TryGetResultItem(jobResult.get_ScheduledJobId(), ref discoveryResultItem);
            if (discoveryResultItem != null && discoveryResultItem.get_HasTags())
            {
              idiscoveryPluginList = DiscoveryHelper.FilterDiscoveryPluginsByTags((IEnumerable<IDiscoveryPlugin>) idiscoveryPluginList, discoveryResultItem.get_Tags()).ToList<IDiscoveryPlugin>();
              if (!((IEnumerable<IDiscoveryPlugin>) idiscoveryPluginList).Any<IDiscoveryPlugin>())
              {
                JobSchedulerEventServicev2.log.Warn((object) ("There is no plugin with any of requested tags [" + string.Join(",", (IEnumerable<string>) discoveryResultItem.get_Tags()) + "].This happens only when the schedulled job requests plugins with tags which do not exist in the poller."));
                return;
              }
            }
            else if (discoveryResultItem == null)
              JobSchedulerEventServicev2.log.Info((object) string.Format("There is no result item with job ID {0} in the discovery result cache.", (object) jobResult));
            idiscoveryPluginList.ForEach((Action<IDiscoveryPlugin>) (plugin => plugin.GetKnownTypes().ForEach((Action<Type>) (t => knownTypes.Add(t)))));
            SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins = DiscoveryPluginHelper.GetOrderedPlugins((IList<IDiscoveryPlugin>) idiscoveryPluginList, (IList<DiscoveryPluginInfo>) discoveryPluginInfos);
            List<IDiscoveryPlugin> orderedPluginList = DiscoveryPluginHelper.GetOrderedPluginList(orderedPlugins);
            if (resultStream != null && resultStream.Length > 0L)
            {
              int num = resultStream.ReadByte();
              resultStream.Position = 0L;
              OrionDiscoveryJobResult result = num == 123 || num == 91 ? (OrionDiscoveryJobResult) SerializationHelper.Deserialize<OrionDiscoveryJobResult>(resultStream) : (OrionDiscoveryJobResult) SerializationHelper.FromXmlStream<OrionDiscoveryJobResult>(resultStream, (IEnumerable<Type>) knownTypes);
              if (result.get_ProfileId().HasValue)
                nullable = new int?(result.get_ProfileId().Value);
              if (OrionDiscoveryJobSchedulerEventsService.resultLog.get_IsDebugEnabled())
              {
                if (resultStream.CanSeek)
                  resultStream.Seek(0L, SeekOrigin.Begin);
                OrionDiscoveryJobSchedulerEventsService.resultLog.DebugFormat("Discovery Job {0} Result for ProfileID = {1}:", (object) jobResult.get_ScheduledJobId(), (object) (nullable ?? -1));
                OrionDiscoveryJobSchedulerEventsService.resultLog.Debug((object) new StreamReader(resultStream, Encoding.UTF8).ReadToEnd());
              }
              if (result.get_DiscoverAgentNodes())
                this.PersistResultsAndDiscoverAgentNodes(result, orderedPlugins, jobResult.get_ScheduledJobId(), jobResult.get_Result().get_State(), result.get_AgentsFilterQuery());
              else
                this.ProcessDiscoveryJobResult(result, orderedPlugins, jobResult.get_ScheduledJobId(), jobResult.get_Result().get_State());
            }
            else
            {
              JobSchedulerEventServicev2.log.Error((object) "Job result is empty, job was killed before it was able to report results.");
              this.UpdateTimeoutedProfile(jobResult.get_ScheduledJobId(), orderedPluginList);
            }
            if (nullable.HasValue)
              JobSchedulerEventServicev2.log.DebugFormat("Processing of discovery results for profile {0} completed", (object) nullable);
            else
              JobSchedulerEventServicev2.log.DebugFormat("Processing of discovery results for one time job {0} completed", (object) jobResult.get_ScheduledJobId());
          }
          catch (Exception ex1)
          {
            JobSchedulerEventServicev2.log.Error((object) "Exception occured when parsing job result.", ex1);
            try
            {
              if (!nullable.HasValue)
                return;
              JobSchedulerEventServicev2.log.DebugFormat("Updating discovery prfile with ID {0}", (object) nullable);
              DiscoveryProfileEntry profile = this.GetProfile(nullable.Value, jobResult.get_ScheduledJobId());
              if (profile == null)
                return;
              profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "Parsing of discovery result failed."));
              profile.Update();
            }
            catch (Exception ex2)
            {
              JobSchedulerEventServicev2.log.Error((object) string.Format("Exception updating discovery profile {0}", (object) nullable), ex2);
            }
          }
          finally
          {
            ((IJobScheduler) scheduler).DeleteJobResult(jobResult.get_Result().get_JobId());
          }
        }
      }
    }

    private Stream GetResultStream(FinishedJobInfo jobResult, IJobSchedulerHelper scheduler)
    {
      Stream destination = (Stream) null;
      if (jobResult.get_Result().get_IsResultStreamed())
      {
        using (Stream jobResultStream = ((IJobScheduler) scheduler).GetJobResultStream(jobResult.get_Result().get_JobId(), "SolarWinds.Orion.Discovery.Job.Results"))
        {
          destination = (Stream) new DynamicStream();
          jobResultStream.CopyTo(destination);
          destination.Position = 0L;
        }
      }
      else if (jobResult.get_Result().get_Output() != null)
        destination = (Stream) new MemoryStream(jobResult.get_Result().get_Output());
      return destination;
    }

    private void PersistResultsAndDiscoverAgentNodes(
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid scheduledJobId,
      JobState state,
      string agentsFilterQuery)
    {
      JobSchedulerEventServicev2.log.DebugFormat("Received discovery job result {0} that requests agent nodes discovery. Persisting partial result and scheduling agent discovery jobs.", (object) scheduledJobId);
      this.partialResultsContainer.CreatePartialResult(scheduledJobId, result, orderedPlugins, state);
      List<AgentInfo> list = this.agentInfoDal.GetAgentsByNodesFilter(((DiscoveryResultBase) result).get_EngineId(), result.get_AgentsFilterQuery()).ToList<AgentInfo>();
      if (result.get_AgentsAddresses().Count > 0)
        list = ((IEnumerable<AgentInfo>) list).Where<AgentInfo>((Func<AgentInfo, bool>) (x => result.get_AgentsAddresses().Contains<string>(x.get_AgentGuid().ToString(), (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase))).ToList<AgentInfo>();
      List<Task> taskList = new List<Task>(list.Count);
      using (List<AgentInfo>.Enumerator enumerator = list.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          AgentInfo current = enumerator.Current;
          taskList.Add(Task.Factory.StartNew((Action<object>) (data => this.ScheduleAgentDiscoveryJob((OrionDiscoveryJobSchedulerEventsService.AgentDiscoveryJobSchedulingData) data)), (object) new OrionDiscoveryJobSchedulerEventsService.AgentDiscoveryJobSchedulingData(scheduledJobId, ((DiscoveryResultBase) result).get_EngineId(), result.get_ProfileId(), current)));
        }
      }
      Task.WaitAll(taskList.ToArray());
      this.partialResultsContainer.AllExpectedResultsRegistered(scheduledJobId);
    }

    private void ScheduleAgentDiscoveryJob(
      OrionDiscoveryJobSchedulerEventsService.AgentDiscoveryJobSchedulingData data)
    {
      try
      {
        if (data == null)
          throw new ArgumentNullException(nameof (data));
        int? nodeId1 = data.AgentInfo.get_NodeID();
        if (!nodeId1.HasValue)
          throw new ArgumentException("AgentInfo does not contain valid NodeID. Discovery job will not be scheduled.");
        IOneTimeAgentDiscoveryJobFactory timeAgentJobFactory = this.oneTimeAgentJobFactory;
        nodeId1 = data.AgentInfo.get_NodeID();
        int nodeId2 = nodeId1.Value;
        int engineId = data.EngineId;
        int? profileId = data.ProfileId;
        List<Credential> credentials = new List<Credential>();
        Guid agentDiscoveryJob = timeAgentJobFactory.CreateOneTimeAgentDiscoveryJob(nodeId2, engineId, profileId, credentials);
        this.partialResultsContainer.ExpectPartialResult(data.MainJobId, agentDiscoveryJob, this.agentPartialResultsTimeout);
      }
      catch (Exception ex)
      {
        JobSchedulerEventServicev2.log.WarnFormat("Can't create one-time discovery job for agent {0}. Agent is probably not accessible. {1}", data != null ? (object) data.AgentInfo.get_AgentGuid().ToString() : (object) "unknown", (object) ex);
      }
    }

    private void ProcessMergedPartialResults(
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid scheduledJobId,
      JobState jobState,
      int? profileId)
    {
      JobSchedulerEventServicev2.log.DebugFormat("Received all partial results for discovery job {0}. Triggering result processing.", (object) scheduledJobId);
      try
      {
        this.ProcessDiscoveryJobResult(result, orderedPlugins, scheduledJobId, jobState);
      }
      catch (Exception ex1)
      {
        JobSchedulerEventServicev2.log.Error((object) "Exception occured when parsing job result.", ex1);
        try
        {
          if (!profileId.HasValue)
            return;
          JobSchedulerEventServicev2.log.DebugFormat("Updating discovery profile with ID {0}", (object) profileId);
          DiscoveryProfileEntry profile = this.GetProfile(profileId.Value, scheduledJobId);
          if (profile == null)
            return;
          profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 3, "Parsing of discovery result failed."));
          profile.Update();
        }
        catch (Exception ex2)
        {
          JobSchedulerEventServicev2.log.Error((object) string.Format("Exception updating discovery profile {0}", (object) profileId), ex2);
        }
      }
    }

    private bool ResultForPluginIsContained(IDiscoveryPlugin plugin, OrionDiscoveryJobResult result)
    {
      try
      {
        string pluginTypeName = ((object) plugin).GetType().FullName;
        return ((IEnumerable<DiscoveryPluginResultBase>) ((DiscoveryResultBase) result).get_PluginResults()).Any<DiscoveryPluginResultBase>((Func<DiscoveryPluginResultBase, bool>) (pluginRes => pluginTypeName.Equals(pluginRes.get_PluginTypeName(), StringComparison.OrdinalIgnoreCase)));
      }
      catch (Exception ex)
      {
        JobSchedulerEventServicev2.log.Error((object) ex);
        return false;
      }
    }

    private void ProcessPluginsWithInterface<T>(
      string actionName,
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Action<T> processAction)
    {
      if (result == null || ((DiscoveryResultBase) result).get_PluginResults() == null)
      {
        JobSchedulerEventServicev2.log.Error((object) "Empty discovery result received. Nothing to process.");
      }
      else
      {
        ((DiscoveryResultBase) result).get_PluginResults().RemoveAll((Predicate<DiscoveryPluginResultBase>) (pluginResult =>
        {
          try
          {
            pluginResult.GetDiscoveredObjects();
            return false;
          }
          catch (Exception ex)
          {
            JobSchedulerEventServicev2.log.ErrorFormat("Failed to get discovered objects from plugin result {0} ({1}), result will be discarded: {2}", (object) ((object) pluginResult).GetType().Name, (object) pluginResult.get_PluginTypeName(), (object) ex);
            return true;
          }
        }));
        JobSchedulerEventServicev2.log.DebugFormat("Result processing [{0}] - Start", (object) actionName);
        using (SortedDictionary<int, List<IDiscoveryPlugin>>.KeyCollection.Enumerator enumerator1 = orderedPlugins.Keys.GetEnumerator())
        {
          while (enumerator1.MoveNext())
          {
            int current1 = enumerator1.Current;
            JobSchedulerEventServicev2.log.DebugFormat("Processing level {0} plugins", (object) current1);
            using (List<IDiscoveryPlugin>.Enumerator enumerator2 = orderedPlugins[current1].GetEnumerator())
            {
              while (enumerator2.MoveNext())
              {
                IDiscoveryPlugin current2 = enumerator2.Current;
                string str = ((IEnumerable<string>) ((object) current2).GetType().AssemblyQualifiedName.Split(',')).First<string>();
                if (current2 is T)
                {
                  JobSchedulerEventServicev2.log.DebugFormat("Plugin {0} is of type {1}", (object) current2, (object) typeof (T));
                  if (this.ResultForPluginIsContained(current2, result))
                  {
                    try
                    {
                      JobSchedulerEventServicev2.log.DebugFormat("Processing {0}", (object) str);
                      processAction((T) current2);
                    }
                    catch (Exception ex)
                    {
                      JobSchedulerEventServicev2.log.Error((object) string.Format("Processing of discovery result for profile {0} failed for plugin {1}", (object) result.get_ProfileId(), (object) ((object) current2).GetType()), ex);
                    }
                  }
                  else
                    JobSchedulerEventServicev2.log.WarnFormat("Result for plugin {0} doesnt exist.", (object) str);
                }
                else
                  JobSchedulerEventServicev2.log.DebugFormat("Plugin {0} is not of type {1}", (object) current2, (object) typeof (T));
              }
            }
          }
        }
        JobSchedulerEventServicev2.log.DebugFormat("Result processing [{0}] - End", (object) actionName);
      }
    }

    internal void ProcessDiscoveryJobResult(
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid jobId,
      JobState jobState)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      JobSchedulerEventServicev2.log.DebugFormat("Processing discovery result for profile {0}", (object) result.get_ProfileId());
      if (this.partialResultsContainer.IsResultExpected(jobId))
      {
        this.partialResultsContainer.AddExpectedPartialResult(jobId, result);
      }
      else
      {
        int? profileId = result.get_ProfileId();
        if (profileId.HasValue && result.get_IsFromAgent())
        {
          JobSchedulerEventServicev2.log.DebugFormat("Received job result from Agent discovery job {0} that is no longer expected. Discarding.", (object) jobId);
        }
        else
        {
          this.ProcessPluginsWithInterface<IBussinessLayerPostProcessing>("ProcessDiscoveryResult", result, orderedPlugins, (Action<IBussinessLayerPostProcessing>) (p => p.ProcessDiscoveryResult((DiscoveryResultBase) result)));
          profileId = result.get_ProfileId();
          if (!profileId.HasValue)
            this.ImportOneShotDiscovery(result, orderedPlugins, jobId, jobState);
          else
            this.ImportProfileResults(result, orderedPlugins, jobId, jobState);
        }
      }
    }

    private void ImportOneShotDiscovery(
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid jobId,
      JobState jobState)
    {
      DiscoveryResultItem resultItem;
      if (!DiscoveryResultCache.get_Instance().TryGetResultItem(jobId, ref resultItem))
      {
        JobSchedulerEventServicev2.log.ErrorFormat("Unable to find resultItem for job {0} in cache", (object) jobId);
      }
      else
      {
        List<IDiscoveredObjectGroup> groups = new List<IDiscoveredObjectGroup>();
        List<ISelectionType> selectionTypes = new List<ISelectionType>();
        this.ProcessPluginsWithInterface<IOneTimeJobSupport>("GetDiscoveredObjectGroups, GetSelectionTypes", result, orderedPlugins, (Action<IOneTimeJobSupport>) (p =>
        {
          groups.AddRange(p.GetDiscoveredObjectGroups());
          selectionTypes.AddRange(p.GetSelectionTypes());
        }));
        TechnologyManager instance = TechnologyManager.Instance;
        groups.AddRange(DiscoveryFilterResultByTechnology.GetDiscoveryGroups(instance));
        DiscoveredObjectTree resultTree = new DiscoveredObjectTree((DiscoveryResultBase) result, (IEnumerable<IDiscoveredObjectGroup>) groups, (IEnumerable<ISelectionType>) selectionTypes);
        using (IEnumerator<DiscoveredVolume> enumerator = ((IEnumerable<DiscoveredVolume>) resultTree.GetAllTreeObjectsOfType<DiscoveredVolume>()).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            DiscoveredVolume current = enumerator.Current;
            if (current.get_VolumeType() == 5 || current.get_VolumeType() == 7 || (current.get_VolumeType() == 6 || current.get_VolumeType() == null))
              ((DiscoveredObjectBase) current).set_IsSelected(false);
          }
        }
        if (!resultItem.get_nodeId().HasValue)
        {
          this.ProcessPluginsWithInterface<IDefaultTreeState>("SetTreeDefault", result, orderedPlugins, (Action<IDefaultTreeState>) (p => p.SetTreeDefaultState(resultTree)));
          DiscoveryFilterResultByTechnology.FilterByPriority((DiscoveryResultBase) result, instance);
        }
        else
        {
          this.ProcessPluginsWithInterface<IOneTimeJobSupport>("GetDiscoveredResourcesManagedStatus", result, orderedPlugins, (Action<IOneTimeJobSupport>) (p => p.GetDiscoveredResourcesManagedStatus(resultTree, resultItem.get_nodeId().Value)));
          DiscoveryFilterResultByTechnology.FilterMandatoryByPriority((DiscoveryResultBase) result, instance);
        }
        resultItem.set_ResultTree(resultTree);
        this.UpdateOneTimeJobResultProgress(resultItem);
      }
    }

    private void UpdateOneTimeJobResultProgress(DiscoveryResultItem item)
    {
      if (item.get_ResultTree() == null)
        return;
      if (item.get_Progress() == null)
      {
        DiscoveryResultItem discoveryResultItem = item;
        OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = new OrionDiscoveryJobProgressInfo();
        discoveryJobProgressInfo.set_JobId(item.get_JobId());
        discoveryResultItem.set_Progress(discoveryJobProgressInfo);
      }
      item.get_Progress().set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 8, "Ready for import", (DiscoveryPhase) 1));
    }

    private void ImportProfileResults(
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid jobId,
      JobState jobState)
    {
      this.ProcessPluginsWithInterface<IDiscoveryPlugin>("StoreDiscoveryResult", result, orderedPlugins, (Action<IDiscoveryPlugin>) (p => p.StoreDiscoveryResult((DiscoveryResultBase) result)));
      JobSchedulerEventServicev2.log.DebugFormat("Updating information about ignored items stored in profile {0}", (object) result.get_ProfileId().Value);
      DiscoveryIgnoredDAL.UpdateIgnoreInformationForProfile(result.get_ProfileId().Value);
      bool flag = true;
      DiscoveryLogs discoveryLog = new DiscoveryLogs();
      DiscoveryProfileEntry profile = (DiscoveryProfileEntry) null;
      try
      {
        JobSchedulerEventServicev2.log.DebugFormat("Updating discovery profile with ID {0}", (object) result.get_ProfileId().Value);
        profile = this.GetProfile(result.get_ProfileId().Value, jobId);
        if (profile == null)
          return;
        if (!profile.get_IsScheduled())
          profile.set_JobID(Guid.Empty);
        DateTime utcNow = DateTime.UtcNow;
        discoveryLog.set_FinishedTimeStamp(utcNow.AddTicks(-(utcNow.Ticks % 10000000L)));
        discoveryLog.set_ProfileID(profile.get_ProfileID());
        discoveryLog.set_AutoImport(profile.get_IsAutoImport());
        discoveryLog.set_Result(0);
        discoveryLog.set_ResultDescription(Resources2.get_DiscoveryLogResult_DiscoveryFinished());
        discoveryLog.set_BatchID(new Guid?(Guid.NewGuid()));
        profile.set_RuntimeInSeconds((int) (DateTime.Now - profile.get_LastRun().ToLocalTime()).TotalSeconds);
        if (jobState == 4)
        {
          discoveryLog.set_Result(1);
          discoveryLog.set_ResultDescription(Resources2.get_DiscoveryLogResult_DiscoveryFailed());
          discoveryLog.set_ErrorMessage(Resources2.get_DiscoveryLogError_Cancelled());
          this.UpdateCanceledProfileStatus(profile);
          profile.Update();
          OrionDiscoveryJobSchedulerEventsService.RemoveProgressInfo(profile.get_ProfileID());
        }
        else if (profile.get_IsAutoImport())
        {
          flag = false;
          profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) profile.get_Status().Status, (string) profile.get_Status().Description, (DiscoveryPhase) 4));
          profile.Update();
          JobSchedulerEventServicev2.log.InfoFormat("Starting AutoImport of Profile:{0}", (object) profile.get_ProfileID());
          bool isHidden = profile.get_IsHidden();
          this.discoveryLogic.ImportDiscoveryResultForProfile(profile.get_ProfileID(), isHidden, (DiscoveryImportManager.CallbackDiscoveryImportFinished) ((_result, importJobID, StartImportStatus) =>
          {
            DiscoveryAutoImportNotificationItemDAL.Show(_result, StartImportStatus);
            this.ImportResultFinished(_result, importJobID, StartImportStatus);
            DiscoveryImportManager.FillDiscoveryLogEntity(discoveryLog, _result, StartImportStatus);
            JobSchedulerEventServicev2.log.InfoFormat("AutoImport of Profile:{0} finished with result:{1}", (object) discoveryLog.get_ProfileID(), (object) discoveryLog.get_Result().ToString());
            try
            {
              using (CoreSwisContext systemContext = SwisContextFactory.CreateSystemContext())
                discoveryLog.Create((SwisContext) systemContext);
              JobSchedulerEventServicev2.log.InfoFormat("DiscoveryLog created for ProfileID:{0}", (object) discoveryLog.get_ProfileID());
            }
            catch (Exception ex)
            {
              JobSchedulerEventServicev2.log.Error((object) "Unable to create discovery import log", ex);
            }
          }), true, new Guid?(jobId));
        }
        else
        {
          if (profile.get_IsScheduled())
            profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 5, string.Empty));
          else
            profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 2, string.Empty));
          OrionDiscoveryJobSchedulerEventsService.GenerateDiscoveryFinishedEvent(profile.get_EngineID(), profile.get_Name());
          OrionDiscoveryJobSchedulerEventsService.RemoveProgressInfo(profile.get_ProfileID());
          profile.Update();
        }
      }
      catch (Exception ex)
      {
        JobSchedulerEventServicev2.log.Error((object) string.Format("Unable to update profile {0}", (object) result.get_ProfileId().Value), ex);
        if (profile != null)
          OrionDiscoveryJobSchedulerEventsService.GenerateDiscoveryFailedEvent(profile.get_EngineID(), profile.get_Name());
        if (!flag)
          return;
        discoveryLog.set_Result(1);
        discoveryLog.set_ResultDescription(Resources2.get_DiscoveryLogResult_DiscoveryFailed());
        discoveryLog.set_ErrorMessage(Resources2.get_DiscoveryLogError_SeeLog());
      }
      finally
      {
        if (profile != null)
        {
          if (profile.get_Status().Status == 5)
            DiscoveryNetObjectStatusManager.Instance.RequestUpdateForProfileAsync(profile.get_ProfileID(), new Action(OrionDiscoveryJobSchedulerEventsService.FireScheduledDiscoveryNotification), TimeSpan.Zero);
          else
            DiscoveryNetObjectStatusManager.Instance.RequestUpdateForProfileAsync(profile.get_ProfileID(), (Action) null, TimeSpan.Zero);
        }
        if (flag)
        {
          using (CoreSwisContext systemContext = SwisContextFactory.CreateSystemContext())
            discoveryLog.Create((SwisContext) systemContext);
        }
      }
    }

    private void ImportResultFinished(
      DiscoveryResultBase result,
      Guid importJobID,
      StartImportStatus status)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(result.get_ProfileID());
      if (profileById == null)
        return;
      if (status == 4)
      {
        profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 2, string.Empty));
        OrionDiscoveryJobSchedulerEventsService.GenerateImportFailedEvent(result.get_EngineId(), profileById.get_Name(), status);
      }
      else if (status == 5)
      {
        profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 8, string.Empty));
        OrionDiscoveryJobSchedulerEventsService.GenerateImportFailedEvent(result.get_EngineId(), profileById.get_Name(), status);
      }
      else
      {
        if (profileById.get_IsScheduled())
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 5, string.Empty));
        else
          profileById.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 2, string.Empty));
        OrionDiscoveryJobSchedulerEventsService.GenerateImportFinishedEvent(result.get_EngineId(), profileById.get_Name());
      }
      profileById.Update();
      OrionDiscoveryJobSchedulerEventsService.RemoveProgressInfo(profileById.get_ProfileID());
    }

    private static void GenerateImportFinishedEvent(int engineId, string profileName)
    {
      EventsDAL.InsertEvent(0, 0, string.Empty, 70, string.Format(Resources.get_Discovery_Succeeded_Text_Run_Import(), (object) profileName), engineId);
    }

    private static void GenerateImportFailedEvent(
      int engineId,
      string profileName,
      StartImportStatus importStatus)
    {
      if (importStatus == 5)
        EventsDAL.InsertEvent(0, 0, string.Empty, 71, string.Format(Resources.get_Discovery_Failed_Text_Import_License(), (object) profileName), engineId);
      else
        EventsDAL.InsertEvent(0, 0, string.Empty, 71, string.Format(Resources.get_Discovery_Failed_Text_Import(), (object) profileName), engineId);
    }

    private static void GenerateDiscoveryFinishedEvent(int engineId, string profileName)
    {
      EventsDAL.InsertEvent(0, 0, string.Empty, 70, string.Format(Resources.get_Discovery_Succeeded_Text_Run(), (object) profileName), engineId);
    }

    private static void GenerateDiscoveryFailedEvent(int engineId, string profileName)
    {
      EventsDAL.InsertEvent(0, 0, string.Empty, 71, string.Format(Resources.get_Discovery_Failed_Text_Run(), (object) profileName), engineId);
    }

    private static void FireScheduledDiscoveryNotification()
    {
      int countOfNodes1 = DiscoveryNodeEntry.GetCountOfNodes((DiscoveryNodeStatus) 56);
      JobSchedulerEventServicev2.log.DebugFormat("SD: New nodes found: {0}", (object) countOfNodes1);
      int countOfNodes2 = DiscoveryNodeEntry.GetCountOfNodes((DiscoveryNodeStatus) 42);
      JobSchedulerEventServicev2.log.DebugFormat("SD: Changed nodes found: {0}", (object) countOfNodes2);
      string url = string.Format("/Orion/Discovery/Results/ScheduledDiscoveryResults.aspx?Status={0}", (object) 58);
      ScheduledDiscoveryNotificationItemDAL.Create(OrionDiscoveryJobSchedulerEventsService.ComposeNotificationMessage(countOfNodes1, countOfNodes2), url);
    }

    private static string ComposeNotificationMessage(int newNodes, int changedNodes)
    {
      StringBuilder stringBuilder = new StringBuilder(Resources.get_LIBCODE_PCC_18());
      stringBuilder.Append(" ");
      if (newNodes == 1)
        stringBuilder.Append(Resources.get_LIBCODE_PCC_19());
      else if (newNodes >= 0)
        stringBuilder.AppendFormat(Resources.get_LIBCODE_PCC_20(), (object) newNodes);
      if (changedNodes > 0)
      {
        if (newNodes >= 0)
          stringBuilder.Append(" ");
        if (changedNodes == 1)
          stringBuilder.Append(Resources.get_LIBCODE_PCC_21());
        else
          stringBuilder.AppendFormat(Resources.get_LIBCODE_PCC_22(), (object) changedNodes);
      }
      return stringBuilder.ToString();
    }

    public void UpdateTimeoutedProfile(Guid jobId, List<IDiscoveryPlugin> orderedPlugins)
    {
      DiscoveryProfileEntry profile = ((IEnumerable<DiscoveryProfileEntry>) DiscoveryProfileEntry.GetAllProfiles()).Where<DiscoveryProfileEntry>((Func<DiscoveryProfileEntry, bool>) (p => p.get_JobID() == jobId)).FirstOrDefault<DiscoveryProfileEntry>();
      if (profile == null)
      {
        JobSchedulerEventServicev2.log.ErrorFormat("Unable to find profile with job id {0}", (object) jobId);
      }
      else
      {
        try
        {
          orderedPlugins.ForEach((Action<IDiscoveryPlugin>) (p =>
          {
            if (!(p is IResultManagement))
              return;
            ((IResultManagement) p).DeleteResultsForProfile(profile.get_ProfileID());
          }));
          if (!profile.get_IsScheduled())
            profile.set_JobID(Guid.Empty);
          this.UpdateCanceledProfileStatus(profile);
          profile.Update();
          OrionDiscoveryJobSchedulerEventsService.RemoveProgressInfo(profile.get_ProfileID());
        }
        catch (Exception ex)
        {
          JobSchedulerEventServicev2.log.Error((object) string.Format("Unhandled exception occured when updating profile {0}", (object) profile.get_ProfileID()), ex);
        }
      }
    }

    public static OrionDiscoveryJobProgressInfo GetProgressInfo(
      int profileId)
    {
      lock (OrionDiscoveryJobSchedulerEventsService.profileProgress)
      {
        if (!OrionDiscoveryJobSchedulerEventsService.profileProgress.ContainsKey(profileId))
          return (OrionDiscoveryJobProgressInfo) null;
        OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = OrionDiscoveryJobSchedulerEventsService.profileProgress[profileId];
        discoveryJobProgressInfo.set_ImportProgress(DiscoveryImportManager.GetImportProgress(discoveryJobProgressInfo.get_JobId()));
        if (discoveryJobProgressInfo.get_ImportProgress() != null)
          discoveryJobProgressInfo.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 1, string.Empty, (DiscoveryPhase) 4));
        return discoveryJobProgressInfo;
      }
    }

    public static OrionDiscoveryJobProgressInfo UpdateProgress(
      OrionDiscoveryJobProgressInfo progress)
    {
      OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = (OrionDiscoveryJobProgressInfo) null;
      lock (((ICollection) OrionDiscoveryJobSchedulerEventsService.profileProgress).SyncRoot)
      {
        OrionDiscoveryJobSchedulerEventsService.profileProgress.TryGetValue(progress.get_ProfileID().Value, out discoveryJobProgressInfo);
        if (discoveryJobProgressInfo == null)
          OrionDiscoveryJobSchedulerEventsService.profileProgress[progress.get_ProfileID().Value] = progress;
        else if (!discoveryJobProgressInfo.get_CanceledByUser())
          discoveryJobProgressInfo.MergeWithNewProgress(progress);
      }
      return discoveryJobProgressInfo;
    }

    public static void RemoveProgressInfo(int profileID)
    {
      lock (((ICollection) OrionDiscoveryJobSchedulerEventsService.profileProgress).SyncRoot)
        OrionDiscoveryJobSchedulerEventsService.profileProgress.Remove(profileID);
    }

    private DiscoveryProfileEntry GetProfile(
      int profileID,
      Guid scheduledJobID)
    {
      JobSchedulerEventServicev2.log.DebugFormat("Loading info for profile {0}.", (object) profileID);
      DiscoveryProfileEntry discoveryProfileEntry = DiscoveryProfileEntry.GetProfileByID(profileID);
      if (discoveryProfileEntry == null)
        JobSchedulerEventServicev2.log.ErrorFormat("Profile: {0} doesn't exists. Deleting job ID: {1}", (object) profileID, (object) scheduledJobID);
      else if (discoveryProfileEntry.get_JobID() != scheduledJobID)
      {
        JobSchedulerEventServicev2.log.ErrorFormat("Profile: {0} exists but has different JobId: {1}. Deleting job ID: {2}", (object) profileID, (object) discoveryProfileEntry.get_JobID(), (object) scheduledJobID);
        discoveryProfileEntry = (DiscoveryProfileEntry) null;
      }
      if (discoveryProfileEntry == null)
      {
        if (!new OrionDiscoveryJobFactory().DeleteJob(scheduledJobID))
          JobSchedulerEventServicev2.log.Error((object) ("Error when deleting job: " + (object) scheduledJobID));
        JobSchedulerEventServicev2.log.ErrorFormat("Job ID: {0} for ProfileID: {1} was deleted.", (object) scheduledJobID, (object) profileID);
      }
      else
        JobSchedulerEventServicev2.log.DebugFormat("Profile info for profile {0} loaded.", (object) profileID);
      return discoveryProfileEntry;
    }

    internal static void CancelDiscoveryJob(int profileID)
    {
      DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(profileID);
      lock (OrionDiscoveryJobSchedulerEventsService.profileProgress)
      {
        OrionDiscoveryJobProgressInfo discoveryJobProgressInfo;
        if (!OrionDiscoveryJobSchedulerEventsService.profileProgress.TryGetValue(profileID, out discoveryJobProgressInfo))
        {
          discoveryJobProgressInfo = new OrionDiscoveryJobProgressInfo();
          discoveryJobProgressInfo.set_Status(profileById.get_Status());
          OrionDiscoveryJobSchedulerEventsService.profileProgress.Add(profileID, discoveryJobProgressInfo);
        }
        discoveryJobProgressInfo.set_CanceledByUser(true);
      }
    }

    private void UpdateCanceledProfileStatus(DiscoveryProfileEntry profile)
    {
      if (profile.get_Status().Status == 7)
        profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 6, "WEBDATA_TP0_DISCOVERY_CANCELLED_BY_USER"));
      else
        profile.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 6, "WEBDATA_TP0_DISCOVERY_INTERRUPTED_BY_TIMEOUT"));
    }

    private class AgentDiscoveryJobSchedulingData
    {
      public AgentDiscoveryJobSchedulingData(
        Guid mainJobId,
        int engineId,
        int? profileId,
        AgentInfo agentInfo)
      {
        this.MainJobId = mainJobId;
        this.EngineId = engineId;
        this.ProfileId = profileId;
        this.AgentInfo = agentInfo;
      }

      public Guid MainJobId { get; private set; }

      public int EngineId { get; private set; }

      public int? ProfileId { get; set; }

      public AgentInfo AgentInfo { get; private set; }
    }
  }
}
