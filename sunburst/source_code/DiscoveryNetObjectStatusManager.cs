// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DiscoveryNetObjectStatusManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Discovery;
using SolarWinds.Orion.Core.Discovery.DAL;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class DiscoveryNetObjectStatusManager : IDiscoveryNetObjectStatusManager
  {
    private static Log log = new Log();
    private static IDiscoveryNetObjectStatusManager instance = (IDiscoveryNetObjectStatusManager) new DiscoveryNetObjectStatusManager();
    internal IDiscoveryDAL discoveryDALProvider = (IDiscoveryDAL) new DiscoveryProfileDAL();
    private object syncLock = new object();
    private Dictionary<Guid, DiscoveryNetObjectStatusManager.CallbackInfo> awaitingCallbacks = new Dictionary<Guid, DiscoveryNetObjectStatusManager.CallbackInfo>();
    private readonly DiscoveryNetObjectStatusManager.Scheduler scheduler;

    public static IDiscoveryNetObjectStatusManager Instance
    {
      get
      {
        return DiscoveryNetObjectStatusManager.instance;
      }
      internal set
      {
        DiscoveryNetObjectStatusManager.instance = value;
      }
    }

    internal DiscoveryNetObjectStatusManager(
      DiscoveryNetObjectStatusManager.Scheduler scheduler)
    {
      this.scheduler = scheduler;
    }

    internal DiscoveryNetObjectStatusManager()
      : this(new DiscoveryNetObjectStatusManager.Scheduler(new Action<int>(DiscoveryNetObjectStatusManager.UpdateRoutine), 5, TimeSpan.FromSeconds(10.0), TimeSpan.FromMilliseconds(100.0)))
    {
    }

    public void BeginOrionDatabaseChanges()
    {
      this.scheduler.BeginChanges();
    }

    public void EndOrionDatabaseChanges()
    {
      this.scheduler.EndChanges();
    }

    public void RequestUpdateAsync(Action updateFinishedCallback, TimeSpan waitForChangesDelay)
    {
      if (DiscoveryNetObjectStatusManager.log.get_IsDebugEnabled())
        DiscoveryNetObjectStatusManager.log.DebugFormat("Global Status Update requested, waiting for changes for {0}", (object) waitForChangesDelay);
      this.RequestUpdateInternal(this.discoveryDALProvider.GetAllProfileIDs(), updateFinishedCallback, waitForChangesDelay);
    }

    public void RequestUpdateForProfileAsync(
      int profileID,
      Action updateFinishedCallback,
      TimeSpan waitForChangesDelay)
    {
      if (DiscoveryNetObjectStatusManager.log.get_IsDebugEnabled())
        DiscoveryNetObjectStatusManager.log.DebugFormat("Status Update for single discovery result requested, waiting for changes for {0}", (object) waitForChangesDelay);
      this.RequestUpdateInternal(new List<int>()
      {
        profileID
      }, updateFinishedCallback, waitForChangesDelay);
    }

    private void RequestUpdateInternal(
      List<int> profileIDs,
      Action updateFinishedCallback,
      TimeSpan waitForChangesDelay)
    {
      UpdateTaskScheduler<int, Guid>.ScheduledTaskCallback callback = (UpdateTaskScheduler<int, Guid>.ScheduledTaskCallback) null;
      if (updateFinishedCallback != null)
      {
        Guid guid = Guid.NewGuid();
        lock (this.syncLock)
          this.awaitingCallbacks.Add(guid, new DiscoveryNetObjectStatusManager.CallbackInfo(updateFinishedCallback, profileIDs.Count));
        callback = new UpdateTaskScheduler<int, Guid>.ScheduledTaskCallback(new Action<UpdateTaskScheduler<int, Guid>.ScheduledTaskCallbackEventArgs>(this.CallbackRoutine), guid);
        if (DiscoveryNetObjectStatusManager.log.get_IsDebugEnabled())
          DiscoveryNetObjectStatusManager.log.DebugFormat("Registering awaiting callback for profiles {0}, request: {1}", (object) string.Join(", ", profileIDs.Select<int, string>((Func<int, string>) (p => p.ToString())).ToArray<string>()), (object) guid);
      }
      foreach (int profileId in profileIDs)
        this.scheduler.RequestUpdateAsync(profileId, callback, waitForChangesDelay);
    }

    internal void CallbackRoutine(
      UpdateTaskScheduler<int, Guid>.ScheduledTaskCallbackEventArgs state)
    {
      Action action = (Action) null;
      lock (this.syncLock)
      {
        DiscoveryNetObjectStatusManager.CallbackInfo callbackInfo;
        if (this.awaitingCallbacks.TryGetValue(state.State, out callbackInfo))
        {
          if (callbackInfo.AwaitingCallsCount > 1)
          {
            --callbackInfo.AwaitingCallsCount;
            if (DiscoveryNetObjectStatusManager.log.get_IsDebugEnabled())
              DiscoveryNetObjectStatusManager.log.DebugFormat("Supressing callback for profile {0}, update request {1}, waiting for {2} more", (object) state.TaskKey, (object) state.State, (object) callbackInfo.AwaitingCallsCount);
          }
          else
          {
            action = callbackInfo.CallbackRoutine;
            this.awaitingCallbacks.Remove(state.State);
            if (DiscoveryNetObjectStatusManager.log.get_IsDebugEnabled())
              DiscoveryNetObjectStatusManager.log.DebugFormat("Firing callback for profile {0}, update request {1}", (object) state.TaskKey, (object) state.State);
          }
        }
        else
          DiscoveryNetObjectStatusManager.log.ErrorFormat("Callback for profile {0} with unknown update request {1} received", (object) state.TaskKey, (object) state.State);
      }
      if (action == null)
        return;
      try
      {
        action();
      }
      catch (Exception ex)
      {
        DiscoveryNetObjectStatusManager.log.Error((object) string.Format("Callback handling routine for profile {0}, update request {1} failed", (object) state.TaskKey, (object) state.State), ex);
      }
    }

    internal static void UpdateRoutine(int profileID)
    {
      if (profileID <= 0)
        throw new ArgumentException("Invalid ProfileID", nameof (profileID));
      using (LocaleThreadState.EnsurePrimaryLocale())
      {
        try
        {
          IEnumerable<IScheduledDiscoveryPlugin> source = ((IEnumerable) DiscoveryHelper.GetOrderedDiscoveryPlugins()).OfType<IScheduledDiscoveryPlugin>();
          if (source.Count<IScheduledDiscoveryPlugin>() > 0)
          {
            DiscoveryResultBase discoveryResult = DiscoveryResultManager.GetDiscoveryResult(profileID, (IList<IDiscoveryPlugin>) ((IEnumerable) source).Cast<IDiscoveryPlugin>().ToList<IDiscoveryPlugin>());
            using (IEnumerator<IScheduledDiscoveryPlugin> enumerator = source.GetEnumerator())
            {
              while (((IEnumerator) enumerator).MoveNext())
                enumerator.Current.UpdateImportStatuses(discoveryResult, (Action<string, double>) ((message, phaseProgress) =>
                {
                  if (!DiscoveryNetObjectStatusManager.log.get_IsInfoEnabled())
                    return;
                  DiscoveryNetObjectStatusManager.log.InfoFormat("Updating Discovered Net Object Statuses for profile {0}: {1} - {2}", (object) profileID, (object) phaseProgress, (object) message);
                }));
            }
          }
          else
          {
            if (!DiscoveryNetObjectStatusManager.log.get_IsInfoEnabled())
              return;
            DiscoveryNetObjectStatusManager.log.InfoFormat("Skipping Discovered Net Object Status update for profile {0}", (object) profileID);
          }
        }
        catch (Exception ex)
        {
          DiscoveryNetObjectStatusManager.log.Error((object) string.Format("Update Discovered Net Object Statuses for profile {0} failed", (object) profileID), ex);
        }
      }
    }

    internal class Scheduler : UpdateTaskScheduler<int, Guid>
    {
      internal Scheduler(
        Action<int> routine,
        int maxRunningTasks,
        TimeSpan postponeTaskDelay,
        TimeSpan mandatorySchedulerDelay)
        : base(routine, maxRunningTasks, postponeTaskDelay, mandatorySchedulerDelay)
      {
      }
    }

    private class CallbackInfo
    {
      public Action CallbackRoutine;
      public int AwaitingCallsCount;

      public CallbackInfo(Action routine, int avaitingCallsCount)
      {
        this.CallbackRoutine = routine;
        this.AwaitingCallsCount = avaitingCallsCount;
      }
    }
  }
}
