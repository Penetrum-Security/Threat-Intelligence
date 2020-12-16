// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.RescheduleDiscoveryJobsTask
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery
{
  internal class RescheduleDiscoveryJobsTask : IDisposable
  {
    private static readonly Log Log = new Log();
    private readonly object _reschedulingAttemptLock = new object();
    private readonly object _reschedulingStartStopControlLock = new object();
    private DateTime _lastSuccess = DateTime.MinValue;
    private readonly Func<int, bool> _updateDiscoveryJobsDelegate;
    private readonly int _engineId;
    private readonly bool _keepRunning;
    private readonly TimeSpan _periodicRetryInterval;
    private CancellationTokenSource _isPeriodicReschedulingEnabled;
    private volatile bool _isDisposed;

    public bool IsPeriodicRescheduleTaskRunning
    {
      get
      {
        lock (this._reschedulingStartStopControlLock)
          return this._isPeriodicReschedulingEnabled != null && !this._isPeriodicReschedulingEnabled.IsCancellationRequested;
      }
    }

    public RescheduleDiscoveryJobsTask(
      Func<int, bool> updateDiscoveryJobsDelegate,
      int engineId,
      bool keepRunning,
      TimeSpan periodicRetryInterval)
    {
      if (periodicRetryInterval <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(nameof (periodicRetryInterval), (object) periodicRetryInterval, "Periodic retry interval has to be greater than zero");
      Func<int, bool> func = updateDiscoveryJobsDelegate;
      if (func == null)
        throw new ArgumentNullException(nameof (updateDiscoveryJobsDelegate));
      this._updateDiscoveryJobsDelegate = func;
      this._engineId = engineId;
      this._keepRunning = keepRunning;
      this._periodicRetryInterval = periodicRetryInterval;
    }

    public void StartPeriodicRescheduleTask()
    {
      lock (this._reschedulingStartStopControlLock)
      {
        if (this._isDisposed)
          return;
        if (!this.IsPeriodicRescheduleTaskRunning)
        {
          this._isPeriodicReschedulingEnabled = new CancellationTokenSource();
          RescheduleDiscoveryJobsTask.Log.InfoFormat("Starting periodic discovery jobs rescheduling for engine {0}", (object) this._engineId);
          Task.Run((Action) (() => this.PeriodicReschedule(this._isPeriodicReschedulingEnabled))).ContinueWith(new Action<Task>(this.LogTaskUnhandledException));
        }
        else
          RescheduleDiscoveryJobsTask.Log.WarnFormat("Periodic discovery jobs rescheduling is already running for engine {0}", (object) this._engineId);
      }
    }

    public void StopPeriodicRescheduleTask()
    {
      lock (this._reschedulingStartStopControlLock)
      {
        this._isPeriodicReschedulingEnabled?.Cancel();
        this._isPeriodicReschedulingEnabled = (CancellationTokenSource) null;
      }
    }

    private void PeriodicReschedule(
      CancellationTokenSource isPeriodicReschedulingEnabled)
    {
      while (!isPeriodicReschedulingEnabled.IsCancellationRequested)
      {
        lock (this._reschedulingAttemptLock)
        {
          if (this.TryRescheduleDiscoveryJobsTask())
          {
            bool keepRunning = this._keepRunning;
            RescheduleDiscoveryJobsTask.Log.DebugFormat("Periodic discovery jobs rescheduling  for engine {0} successful. Keep running: {1}", (object) this._engineId, (object) keepRunning);
            if (!keepRunning)
            {
              isPeriodicReschedulingEnabled.Cancel();
              break;
            }
          }
          else
            RescheduleDiscoveryJobsTask.Log.DebugFormat("Periodic discovery jobs rescheduling for engine {0} failed - next attempt in {1}.", (object) this._engineId, (object) this._periodicRetryInterval);
        }
        Task.Delay(this._periodicRetryInterval, isPeriodicReschedulingEnabled.Token).ContinueWith(new Action<Task>(this.LogTaskUnhandledException)).Wait();
      }
      RescheduleDiscoveryJobsTask.Log.DebugFormat("Periodic discovery jobs rescheduling stopped for engine {0}", (object) this._engineId);
    }

    public void QueueRescheduleAttempt()
    {
      lock (this._reschedulingStartStopControlLock)
      {
        if (this._isDisposed)
          return;
        DateTime invocationTime = DateTime.UtcNow;
        RescheduleDiscoveryJobsTask.Log.DebugFormat("Invoking manual discovery jobs rescheduling for engine {0}", (object) this._engineId);
        Task.Run((Action) (() => this.ManualRescheduleAttempt(invocationTime))).ContinueWith(new Action<Task>(this.LogTaskUnhandledException));
      }
    }

    private void ManualRescheduleAttempt(DateTime invocationTime)
    {
      lock (this._reschedulingAttemptLock)
      {
        if (invocationTime > this._lastSuccess)
        {
          if (this.TryRescheduleDiscoveryJobsTask() || this.IsPeriodicRescheduleTaskRunning)
            return;
          this.StartPeriodicRescheduleTask();
        }
        else
          RescheduleDiscoveryJobsTask.Log.DebugFormat("Manual discovery jobs rescheduling skipped. Last success is newer than invocation time", Array.Empty<object>());
      }
    }

    private bool TryRescheduleDiscoveryJobsTask()
    {
      if (this._isDisposed)
        return false;
      try
      {
        if (this._updateDiscoveryJobsDelegate(this._engineId))
        {
          this._lastSuccess = DateTime.UtcNow;
          RescheduleDiscoveryJobsTask.Log.DebugFormat("Discovery jobs rescheduling finished for engine {0}", (object) this._engineId);
          return true;
        }
        RescheduleDiscoveryJobsTask.Log.DebugFormat("Discovery jobs rescheduling failed for engine {0}", (object) this._engineId);
      }
      catch (Exception ex)
      {
        RescheduleDiscoveryJobsTask.Log.Error((object) string.Format("RescheduleDiscoveryJobsTask.TryRescheduleDiscoveryJobsTask failed for engine {0}", (object) this._engineId), ex);
      }
      return false;
    }

    private void LogTaskUnhandledException(Task task)
    {
      if (!task.IsFaulted)
        return;
      RescheduleDiscoveryJobsTask.Log.Error((object) "Task faulted with unhandled exception", (Exception) task.Exception);
    }

    public void Dispose()
    {
      this._isDisposed = true;
      this.StopPeriodicRescheduleTask();
    }
  }
}
