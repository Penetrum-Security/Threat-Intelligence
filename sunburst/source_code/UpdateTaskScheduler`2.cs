// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.UpdateTaskScheduler`2
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using Amib.Threading;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.i18n;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class UpdateTaskScheduler<TTaskKey, TCallbackArg> : IDisposable where TTaskKey : IComparable
  {
    private Log log = new Log();
    private object syncLock = new object();
    private TimeSpan postponeTaskDelay;
    private TimeSpan mandatorySchedulerDelay;
    private Action<TTaskKey> taskRoutine;
    private Thread schedulerThread;
    private SmartThreadPool processingThreadPool;
    private IWorkItemsGroup processingGroup;
    private int ongoingChangesCounter;
    private PriorityQueue<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask> scheduledTasks;
    private HashSet<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask> ongoingTasks;
    private bool disposed;

    public bool ChangesActive
    {
      get
      {
        lock (this.syncLock)
          return this.ongoingChangesCounter > 0;
      }
    }

    public UpdateTaskScheduler(
      Action<TTaskKey> taskRoutine,
      int maxRunningTasks,
      TimeSpan postponeTaskDelay,
      TimeSpan mandatorySchedulerDelay)
    {
      this.taskRoutine = taskRoutine;
      this.postponeTaskDelay = postponeTaskDelay;
      this.mandatorySchedulerDelay = mandatorySchedulerDelay;
      this.ongoingChangesCounter = 0;
      this.ongoingTasks = new HashSet<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>((IEqualityComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>) UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer.Instance);
      this.scheduledTasks = new PriorityQueue<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>((IComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>) UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.SchedulingComparer.Instance);
      this.schedulerThread = new Thread(new ThreadStart(this.SchedulerRoutine));
      this.schedulerThread.IsBackground = true;
      this.schedulerThread.Start();
      STPStartInfo stpStartInfo = new STPStartInfo();
      stpStartInfo.set_MaxWorkerThreads(maxRunningTasks);
      stpStartInfo.set_MinWorkerThreads(0);
      ((WIGStartInfo) stpStartInfo).set_StartSuspended(false);
      this.processingThreadPool = new SmartThreadPool(stpStartInfo);
      this.processingGroup = this.processingThreadPool.CreateWorkItemsGroup(maxRunningTasks);
    }

    public void BeginChanges()
    {
      lock (this.syncLock)
      {
        ++this.ongoingChangesCounter;
        if (!this.log.get_IsDebugEnabled())
          return;
        this.log.DebugFormat("BeginChanges {0}", (object) this.ongoingChangesCounter);
      }
    }

    public void EndChanges()
    {
      lock (this.syncLock)
      {
        if (this.ongoingChangesCounter <= 0)
          throw new InvalidOperationException("Unable to find matching BeginChanges call");
        --this.ongoingChangesCounter;
        if (this.log.get_IsDebugEnabled())
          this.log.DebugFormat("EndChanges {0}", (object) this.ongoingChangesCounter);
        if (this.ongoingChangesCounter != 0)
          return;
        this.log.Debug((object) "EndChanges - waking up scheduler");
        Monitor.PulseAll(this.syncLock);
      }
    }

    public void RequestUpdateAsync(
      TTaskKey taskKey,
      UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback callback,
      TimeSpan waitForChangesDelay)
    {
      UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask scheduledTask1 = new UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask();
      scheduledTask1.TaskKey = taskKey;
      scheduledTask1.PlannedExecution = DateTime.UtcNow.Add(waitForChangesDelay);
      List<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback> scheduledTaskCallbackList;
      if (callback == null)
      {
        scheduledTaskCallbackList = (List<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback>) null;
      }
      else
      {
        scheduledTaskCallbackList = new List<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback>();
        scheduledTaskCallbackList.Add(callback);
      }
      scheduledTask1.Callbacks = scheduledTaskCallbackList;
      UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask scheduledTask2 = scheduledTask1;
      if (this.log.get_IsDebugEnabled())
        this.log.DebugFormat("RequestUpdate for Task {0} - Enter", (object) taskKey);
      lock (this.syncLock)
      {
        UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask scheduledTask3;
        if (this.scheduledTasks.TryFind(scheduledTask2, (IComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>) UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer.Instance, ref scheduledTask3))
        {
          if (scheduledTask2.PlannedExecution < scheduledTask3.PlannedExecution)
          {
            if (scheduledTask3.Callbacks != null)
            {
              if (scheduledTask2.Callbacks != null)
                scheduledTask2.Callbacks.AddRange((IEnumerable<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback>) scheduledTask3.Callbacks);
              else
                scheduledTask2.Callbacks = scheduledTask3.Callbacks;
            }
            this.scheduledTasks.Remove(scheduledTask3, (IComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>) UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer.Instance);
            this.scheduledTasks.Enqueue(scheduledTask2);
            if (this.log.get_IsInfoEnabled())
              this.log.InfoFormat("Task {0} was rescheduled from {1} to {2}", (object) scheduledTask2, (object) scheduledTask3.PlannedExecution, (object) scheduledTask2.PlannedExecution);
            Monitor.PulseAll(this.syncLock);
          }
          else
          {
            if (scheduledTask2.Callbacks != null)
            {
              if (scheduledTask3.Callbacks != null)
                scheduledTask3.Callbacks.AddRange((IEnumerable<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback>) scheduledTask2.Callbacks);
              else
                scheduledTask3.Callbacks = scheduledTask2.Callbacks;
            }
            if (this.log.get_IsInfoEnabled())
              this.log.InfoFormat("Task {0} has been scheduled already at {1}, requested time {2}", (object) scheduledTask2, (object) scheduledTask3.PlannedExecution, (object) scheduledTask2.PlannedExecution);
          }
        }
        else
        {
          this.scheduledTasks.Enqueue(scheduledTask2);
          if (this.log.get_IsInfoEnabled())
            this.log.InfoFormat("Task {0} has been scheduled for {1}", (object) scheduledTask2, (object) scheduledTask2.PlannedExecution);
          Monitor.PulseAll(this.syncLock);
        }
      }
      if (!this.log.get_IsDebugEnabled())
        return;
      this.log.DebugFormat("RequestUpdate for Task {0} - Leave", (object) taskKey);
    }

    private void SchedulerRoutine()
    {
      this.log.Debug((object) "Scheduler: scheduling thread started");
      while (true)
      {
        UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask scheduledTask = (UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask) null;
        lock (this.syncLock)
        {
          if (this.ChangesActive)
          {
            this.log.Debug((object) "Suspending Scheduler: ongoing changes detected");
            Monitor.Wait(this.syncLock);
          }
          else if (this.scheduledTasks.get_Count() == 0)
          {
            this.log.Debug((object) "Suspending Scheduler: no pending tasks to process");
            Monitor.Wait(this.syncLock);
          }
          else
          {
            DateTime utcNow = DateTime.UtcNow;
            scheduledTask = this.scheduledTasks.Peek();
            if (scheduledTask.PlannedExecution > utcNow)
            {
              TimeSpan timeout = scheduledTask.PlannedExecution.Subtract(utcNow);
              if (this.log.get_IsDebugEnabled())
                this.log.DebugFormat("Suspending Scheduler: woke up too early, next task {0} is scheduled for {1}, will be suspended for {2}", (object) scheduledTask, (object) scheduledTask.PlannedExecution, (object) timeout);
              scheduledTask = (UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask) null;
              Monitor.Wait(this.syncLock, timeout);
            }
            else if (this.ongoingTasks.Contains(scheduledTask))
            {
              this.scheduledTasks.Remove(scheduledTask, (IComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>) UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer.Instance);
              scheduledTask.PlannedExecution = utcNow.Add(this.postponeTaskDelay);
              this.scheduledTasks.Enqueue(scheduledTask);
              if (this.log.get_IsDebugEnabled())
                this.log.DebugFormat("Scheduler: task {0} is being executed, rescheduling its next execution to {1}", (object) scheduledTask, (object) scheduledTask.PlannedExecution);
              scheduledTask = (UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask) null;
            }
            else
            {
              scheduledTask = this.scheduledTasks.Dequeue();
              this.ongoingTasks.Add(scheduledTask);
              if (this.log.get_IsDebugEnabled())
                this.log.DebugFormat("Scheduler: Task {0} is planed to get executed now", (object) scheduledTask);
            }
          }
        }
        if (scheduledTask != null)
        {
          // ISSUE: method pointer
          this.processingGroup.QueueWorkItem(new WorkItemCallback((object) this, __methodptr(ThreadPoolRoutine)), (object) scheduledTask);
          if (this.log.get_IsDebugEnabled())
            this.log.DebugFormat("Scheduler: Task {0} was executed", (object) scheduledTask);
        }
        Thread.Sleep(this.mandatorySchedulerDelay);
      }
    }

    private object ThreadPoolRoutine(object state)
    {
      if (!(state is UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask scheduledTask))
        throw new ArgumentException("Unexpected state object or null", nameof (state));
      try
      {
        bool taskExecuted = false;
        Exception ex1 = (Exception) null;
        using (LocaleThreadState.EnsurePrimaryLocale())
        {
          if (!SmartThreadPool.get_IsWorkItemCanceled())
          {
            try
            {
              taskExecuted = true;
              this.taskRoutine(scheduledTask.TaskKey);
            }
            catch (Exception ex2)
            {
              ex1 = ex2;
              this.log.Error((object) string.Format("Task {0} cought unhandled exception from task routine", (object) scheduledTask), ex2);
            }
          }
          if (scheduledTask.Callbacks != null)
          {
            foreach (UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback callback in scheduledTask.Callbacks)
            {
              try
              {
                callback.Callback(new UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallbackEventArgs(scheduledTask.TaskKey, callback.State, ex1, taskExecuted));
              }
              catch (Exception ex2)
              {
                this.log.Error((object) string.Format("Task {0} callback failed", (object) scheduledTask), ex2);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.log.Error((object) string.Format("Task {0} cought unhandled exception during task processing", (object) scheduledTask), ex);
      }
      finally
      {
        lock (this.syncLock)
          this.ongoingTasks.Remove(scheduledTask);
      }
      return (object) null;
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
      {
        if (this.schedulerThread != null)
        {
          this.schedulerThread.Abort();
          this.schedulerThread = (Thread) null;
        }
        if (this.processingGroup != null)
        {
          this.processingGroup.Cancel(false);
          this.processingGroup = (IWorkItemsGroup) null;
        }
        if (this.processingThreadPool != null)
        {
          this.processingThreadPool.Dispose();
          this.processingThreadPool = (SmartThreadPool) null;
        }
      }
      this.disposed = true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~UpdateTaskScheduler()
    {
      this.Dispose(false);
    }

    public class ScheduledTaskCallbackEventArgs : EventArgs
    {
      public TTaskKey TaskKey { get; private set; }

      public TCallbackArg State { get; private set; }

      public Exception TaskException { get; private set; }

      public bool TaskExecuted { get; private set; }

      public bool TaskFailed
      {
        get
        {
          return this.TaskException != null;
        }
      }

      internal ScheduledTaskCallbackEventArgs(
        TTaskKey taskKey,
        TCallbackArg state,
        Exception ex,
        bool taskExecuted)
      {
        this.TaskKey = taskKey;
        this.State = state;
        this.TaskException = ex;
        this.TaskExecuted = taskExecuted;
      }
    }

    public class ScheduledTaskCallback
    {
      public Action<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallbackEventArgs> Callback { get; private set; }

      public TCallbackArg State { get; private set; }

      public ScheduledTaskCallback(
        Action<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallbackEventArgs> callback,
        TCallbackArg state)
      {
        this.Callback = callback;
        this.State = state;
      }
    }

    private class ScheduledTask
    {
      public TTaskKey TaskKey;
      public List<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTaskCallback> Callbacks;
      public DateTime PlannedExecution;

      public override string ToString()
      {
        return this.TaskKey.ToString();
      }

      public class IdentityComparer : Comparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>, IEqualityComparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>
      {
        public static readonly UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer Instance = new UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.IdentityComparer();

        public override int Compare(
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask x,
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask y)
        {
          return x.TaskKey.CompareTo((object) y.TaskKey);
        }

        public bool Equals(
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask x,
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask y)
        {
          return x.TaskKey.CompareTo((object) y.TaskKey) == 0;
        }

        public int GetHashCode(
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask obj)
        {
          return obj.TaskKey.GetHashCode();
        }
      }

      public class SchedulingComparer : Comparer<UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask>
      {
        public static readonly UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.SchedulingComparer Instance = new UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask.SchedulingComparer();

        public override int Compare(
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask x,
          UpdateTaskScheduler<TTaskKey, TCallbackArg>.ScheduledTask y)
        {
          return DateTime.Compare(y.PlannedExecution, x.PlannedExecution);
        }
      }
    }
  }
}
