// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.PartialDiscoveryResultsContainer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Job;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery
{
  internal class PartialDiscoveryResultsContainer : IDisposable
  {
    private static readonly Log _log = new Log();
    private readonly object _syncRoot = new object();
    private readonly Dictionary<Guid, List<PartialDiscoveryResultsContainer.PartialResult>> _resultsByMainJobId;
    private readonly Dictionary<Guid, PartialDiscoveryResultsContainer.PartialResult> _resultsByOwnJobId;
    private readonly HashSet<Guid> _mainResultsReadyForComplete;
    private readonly IPartialDiscoveryResultsPersistence _persistenceStore;
    private readonly Func<DateTime> _dateTimeProvider;
    private Timer _expirationCleanupTimer;
    private bool _disposed;

    public event EventHandler<DiscoveryResultsCompletedEventArgs> DiscoveryResultsComplete = (_param1, _param2) => {};

    public PartialDiscoveryResultsContainer()
      : this((IPartialDiscoveryResultsPersistence) new PartialDiscoveryResultsFilePersistence(), (Func<DateTime>) (() => DateTime.UtcNow), TimeSpan.FromSeconds(10.0))
    {
    }

    internal PartialDiscoveryResultsContainer(
      IPartialDiscoveryResultsPersistence persistenceStore,
      Func<DateTime> dateTimeProvider,
      TimeSpan expirationCheckFrequency)
    {
      if (persistenceStore == null)
        throw new ArgumentNullException(nameof (persistenceStore));
      if (dateTimeProvider == null)
        throw new ArgumentNullException(nameof (dateTimeProvider));
      this._persistenceStore = persistenceStore;
      this._dateTimeProvider = dateTimeProvider;
      this._resultsByMainJobId = new Dictionary<Guid, List<PartialDiscoveryResultsContainer.PartialResult>>();
      this._resultsByOwnJobId = new Dictionary<Guid, PartialDiscoveryResultsContainer.PartialResult>();
      this._mainResultsReadyForComplete = new HashSet<Guid>();
      this._expirationCleanupTimer = new Timer((TimerCallback) (x => this.RunExpirationCheck()), (object) null, expirationCheckFrequency, expirationCheckFrequency);
    }

    public void CreatePartialResult(
      Guid scheduledJobId,
      OrionDiscoveryJobResult result,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      JobState jobState)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      PartialDiscoveryResultsContainer.PartialResult partialResult = new PartialDiscoveryResultsContainer.PartialResult(scheduledJobId, scheduledJobId, orderedPlugins, jobState, result.get_ProfileId(), this._persistenceStore, DateTime.MaxValue)
      {
        Result = result
      };
      lock (this._syncRoot)
      {
        this._resultsByMainJobId[partialResult.JobId] = new List<PartialDiscoveryResultsContainer.PartialResult>();
        this._resultsByMainJobId[partialResult.JobId].Add(partialResult);
        this._resultsByOwnJobId[partialResult.JobId] = partialResult;
      }
    }

    public void AddExpectedPartialResult(Guid scheduledJobId, OrionDiscoveryJobResult result)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      List<PartialDiscoveryResultsContainer.PartialResult> completeResults;
      lock (this._syncRoot)
      {
        PartialDiscoveryResultsContainer.PartialResult partialResult;
        if (!this._resultsByOwnJobId.TryGetValue(scheduledJobId, out partialResult))
          throw new ArgumentException("Results with given job ID are not expected.", nameof (scheduledJobId));
        partialResult.Result = result;
        completeResults = this.TryGetCompleteResults(partialResult.MainJobId);
      }
      if (completeResults == null)
        return;
      this.OnDiscoveryResultsComplete(completeResults);
    }

    public void ExpectPartialResult(Guid mainScheduledJobId, Guid scheduledJobId, TimeSpan timeout)
    {
      lock (this._syncRoot)
      {
        List<PartialDiscoveryResultsContainer.PartialResult> partialResultList;
        if (!this._resultsByMainJobId.TryGetValue(mainScheduledJobId, out partialResultList))
          throw new ArgumentException("Results with given main result ID are not in container.", nameof (mainScheduledJobId));
        PartialDiscoveryResultsContainer.PartialResult partialResult = new PartialDiscoveryResultsContainer.PartialResult(scheduledJobId, mainScheduledJobId, this._persistenceStore, this._dateTimeProvider().Add(timeout));
        partialResultList.Add(partialResult);
        this._resultsByOwnJobId[partialResult.JobId] = partialResult;
      }
    }

    public bool IsResultExpected(Guid scheduledJobId)
    {
      lock (this._syncRoot)
        return this._resultsByOwnJobId.ContainsKey(scheduledJobId) && !this._resultsByMainJobId.ContainsKey(scheduledJobId);
    }

    public void AllExpectedResultsRegistered(Guid mainScheduledJobId)
    {
      List<PartialDiscoveryResultsContainer.PartialResult> completeResults;
      lock (this._syncRoot)
      {
        this._mainResultsReadyForComplete.Add(mainScheduledJobId);
        completeResults = this.TryGetCompleteResults(mainScheduledJobId);
      }
      if (completeResults == null)
        return;
      this.OnDiscoveryResultsComplete(completeResults);
    }

    public void ClearStore()
    {
      this._persistenceStore.ClearStore();
    }

    public void RemoveExpectedPartialResult(Guid scheduledJobId)
    {
      List<PartialDiscoveryResultsContainer.PartialResult> completeResults;
      lock (this._syncRoot)
      {
        PartialDiscoveryResultsContainer.PartialResult partialResult;
        if (!this._resultsByOwnJobId.TryGetValue(scheduledJobId, out partialResult))
          throw new ArgumentException("Results with given job ID are not expected.", nameof (scheduledJobId));
        this._resultsByOwnJobId.Remove(scheduledJobId);
        this._resultsByMainJobId[partialResult.MainJobId].Remove(partialResult);
        this._persistenceStore.DeleteResult(partialResult.JobId);
        completeResults = this.TryGetCompleteResults(partialResult.MainJobId);
      }
      if (completeResults == null)
        return;
      this.OnDiscoveryResultsComplete(completeResults);
    }

    private void RunExpirationCheck()
    {
      lock (this._syncRoot)
      {
        foreach (PartialDiscoveryResultsContainer.PartialResult partialResult in this._resultsByOwnJobId.Values.ToList<PartialDiscoveryResultsContainer.PartialResult>())
        {
          if (!partialResult.HasResult && partialResult.Expiration < this._dateTimeProvider())
          {
            PartialDiscoveryResultsContainer._log.WarnFormat("Expected partial discovery results for job {0} were not received in defined time and are being discarded.", (object) partialResult.JobId);
            this.RemoveExpectedPartialResult(partialResult.JobId);
          }
        }
      }
    }

    private void OnDiscoveryResultsComplete(
      List<PartialDiscoveryResultsContainer.PartialResult> results)
    {
      if (results == null || results.Count == 0)
      {
        PartialDiscoveryResultsContainer._log.WarnFormat("Attempt to report partial discovery results completion with empty results.", Array.Empty<object>());
      }
      else
      {
        OrionDiscoveryJobResult completeResult = this.MergePartialResults(results);
        lock (this._syncRoot)
          this.RemovePartialResults(results[0].MainJobId, (IEnumerable<PartialDiscoveryResultsContainer.PartialResult>) results);
        this.DiscoveryResultsComplete((object) this, new DiscoveryResultsCompletedEventArgs(completeResult, results[0].OrderedPlugins, results[0].MainJobId, results[0].JobState, results[0].ProfileId));
      }
    }

    private List<PartialDiscoveryResultsContainer.PartialResult> TryGetCompleteResults(
      Guid mainResultId)
    {
      lock (this._syncRoot)
      {
        List<PartialDiscoveryResultsContainer.PartialResult> results;
        if (!this._resultsByMainJobId.TryGetValue(mainResultId, out results))
          throw new ArgumentException("Main results for results with given ID are not in container.");
        if (this.AreAllResultsReady(results))
          return results;
      }
      return (List<PartialDiscoveryResultsContainer.PartialResult>) null;
    }

    private bool AreAllResultsReady(
      List<PartialDiscoveryResultsContainer.PartialResult> results)
    {
      bool flag = false;
      if (results.Count > 0 && results.All<PartialDiscoveryResultsContainer.PartialResult>((Func<PartialDiscoveryResultsContainer.PartialResult, bool>) (x => x.HasResult)))
        flag = this._mainResultsReadyForComplete.Contains(results[0].MainJobId);
      return flag;
    }

    private OrionDiscoveryJobResult MergePartialResults(
      List<PartialDiscoveryResultsContainer.PartialResult> results)
    {
      if (results.Count == 0)
        throw new ArgumentException("Results for merge can't be empty.", nameof (results));
      OrionDiscoveryJobResult result = results[0].Result;
      if (result == null)
        throw new ArgumentException("Main results for merge were not loaded.", nameof (results));
      OrionDiscoveryJobResult discoveryJobResult = (OrionDiscoveryJobResult) ((DiscoveryResultBase) result).Copy();
      foreach (PartialDiscoveryResultsContainer.PartialResult partialResult in results.Skip<PartialDiscoveryResultsContainer.PartialResult>(1))
        this.MergePluginResults(discoveryJobResult.get_ProfileId(), ((DiscoveryResultBase) discoveryJobResult).get_PluginResults(), ((DiscoveryResultBase) partialResult.Result).get_PluginResults());
      return discoveryJobResult;
    }

    private void MergePluginResults(
      int? profileId,
      DiscoveryPluginItems<DiscoveryPluginResultBase> results,
      DiscoveryPluginItems<DiscoveryPluginResultBase> partialResultsToMerge)
    {
      DiscoveryPluginResultObjectMapping resultObjectMapping = new DiscoveryPluginResultObjectMapping();
      List<DiscoveryPluginResultBase> pluginResultBaseList = new List<DiscoveryPluginResultBase>();
      using (IEnumerator<DiscoveryPluginResultBase> enumerator = results.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          DiscoveryPluginResultBase item = enumerator.Current;
          DiscoveryPluginResultBase pluginResultBase = ((IEnumerable<DiscoveryPluginResultBase>) partialResultsToMerge).Except<DiscoveryPluginResultBase>((IEnumerable<DiscoveryPluginResultBase>) pluginResultBaseList).FirstOrDefault<DiscoveryPluginResultBase>((Func<DiscoveryPluginResultBase, bool>) (x => ((object) x).GetType() == ((object) item).GetType()));
          if (pluginResultBase != null)
          {
            if (!(item is IDiscoveryPluginResultMerge pluginResultMerge))
            {
              PartialDiscoveryResultsContainer._log.WarnFormat("Plugin discovery results '{0}' do not implement IDiscoveryPluginResultMerge interface and will not be merged with other results instances.", (object) ((object) item).GetType());
            }
            else
            {
              pluginResultMerge.MergeResults(pluginResultBase, resultObjectMapping, profileId);
              pluginResultBaseList.Add(pluginResultBase);
            }
          }
        }
      }
      using (IEnumerator<DiscoveryPluginResultBase> enumerator = ((IEnumerable<DiscoveryPluginResultBase>) partialResultsToMerge).Except<DiscoveryPluginResultBase>((IEnumerable<DiscoveryPluginResultBase>) pluginResultBaseList).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          DiscoveryPluginResultBase current = enumerator.Current;
          results.Add(current);
        }
      }
    }

    private void RemovePartialResults(
      Guid mainResultId,
      IEnumerable<PartialDiscoveryResultsContainer.PartialResult> results)
    {
      lock (this._syncRoot)
      {
        this._mainResultsReadyForComplete.Remove(mainResultId);
        this._resultsByMainJobId.Remove(mainResultId);
        foreach (PartialDiscoveryResultsContainer.PartialResult result in results)
        {
          this._resultsByOwnJobId.Remove(result.JobId);
          this._persistenceStore.DeleteResult(result.JobId);
        }
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      if (!disposing)
        return;
      try
      {
        if (this._expirationCleanupTimer != null)
        {
          this._expirationCleanupTimer.Dispose();
          this._expirationCleanupTimer = (Timer) null;
        }
      }
      catch (Exception ex)
      {
        PartialDiscoveryResultsContainer._log.Error((object) "Error diposing PartialDiscoveryResultsContainer.", ex);
      }
      this._disposed = true;
    }

    ~PartialDiscoveryResultsContainer()
    {
      this.Dispose(false);
    }

    private class PartialResult
    {
      private readonly IPartialDiscoveryResultsPersistence _persistenceStore;
      private OrionDiscoveryJobResult _result;

      public PartialResult(
        Guid jobId,
        Guid mainJobId,
        IPartialDiscoveryResultsPersistence persistenceStore,
        DateTime expiration)
        : this(jobId, mainJobId, (SortedDictionary<int, List<IDiscoveryPlugin>>) null, (JobState) 7, new int?(), persistenceStore, expiration)
      {
      }

      public PartialResult(
        Guid jobId,
        Guid mainJobId,
        SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
        JobState jobState,
        int? profileId,
        IPartialDiscoveryResultsPersistence persistenceStore,
        DateTime expiration)
      {
        if (persistenceStore == null)
          throw new ArgumentNullException(nameof (persistenceStore));
        this._persistenceStore = persistenceStore;
        this.JobId = jobId;
        this.MainJobId = mainJobId;
        this.OrderedPlugins = orderedPlugins;
        this.JobState = jobState;
        this.ProfileId = profileId;
        this.Expiration = expiration;
      }

      public Guid MainJobId { get; private set; }

      public Guid JobId { get; private set; }

      public SortedDictionary<int, List<IDiscoveryPlugin>> OrderedPlugins { get; private set; }

      public JobState JobState { get; private set; }

      public int? ProfileId { get; private set; }

      public bool HasResult { get; private set; }

      public DateTime Expiration { get; private set; }

      public OrionDiscoveryJobResult Result
      {
        get
        {
          if (!this.HasResult)
            return (OrionDiscoveryJobResult) null;
          return this._result != null ? this._result : this._persistenceStore.LoadResult(this.JobId);
        }
        set
        {
          if (!this._persistenceStore.SaveResult(this.JobId, value))
            this._result = value;
          this.HasResult = true;
        }
      }
    }
  }
}
