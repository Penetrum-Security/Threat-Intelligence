// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.QueuedTaskScheduler`1
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using Amib.Threading;
using Amib.Threading.Internal;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.Extensions;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Strings;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class QueuedTaskScheduler<TTask> : IDisposable where TTask : class
  {
    private static readonly Log log = new Log(nameof (QueuedTaskScheduler<TTask>));
    private SmartThreadPool processingThreadPool;
    private IWorkItemsGroup processingGroup;
    private STPStartInfo processingStartInfo;
    private QueuedTaskScheduler<TTask>.TaskProcessingRoutine processingRoutine;
    private volatile bool isRunning;
    private bool disposed;

    public event EventHandler TaskProcessingFinished;

    public bool IsRunning
    {
      get
      {
        return this.isRunning;
      }
    }

    public int QueueSize
    {
      get
      {
        return ((WorkItemsGroupBase) this.processingThreadPool).get_WaitingCallbacks() + this.processingGroup.get_WaitingCallbacks();
      }
    }

    public QueuedTaskScheduler(
      QueuedTaskScheduler<TTask>.TaskProcessingRoutine routine,
      int paralleltasksCount)
    {
      this.isRunning = false;
      this.processingRoutine = routine;
      STPStartInfo stpStartInfo = new STPStartInfo();
      stpStartInfo.set_MaxWorkerThreads(paralleltasksCount);
      stpStartInfo.set_MinWorkerThreads(0);
      ((WIGStartInfo) stpStartInfo).set_StartSuspended(true);
      this.processingStartInfo = stpStartInfo;
      this.processingThreadPool = new SmartThreadPool(this.processingStartInfo);
      this.processingGroup = this.processingThreadPool.CreateWorkItemsGroup(paralleltasksCount);
      // ISSUE: method pointer
      this.processingGroup.add_OnIdle(new WorkItemsGroupIdleHandler((object) this, __methodptr(processingGroup_OnIdle)));
    }

    public void EnqueueTask(TTask task)
    {
      // ISSUE: method pointer
      this.processingGroup.QueueWorkItem(new WorkItemCallback((object) this, __methodptr(ThreadPoolCallBack)), (object) task);
    }

    public void Start()
    {
      if (this.IsRunning)
        throw new InvalidOperationException(Resources.get_LIBCODE_JM0_30());
      if (this.QueueSize > 0)
      {
        this.isRunning = true;
        this.processingGroup.Start();
        ((WorkItemsGroupBase) this.processingThreadPool).Start();
        QueuedTaskScheduler<TTask>.log.InfoFormat("Queued tasks processing started: QueuedTasksCount = {0}, ParallelTasksCount = {1}", (object) this.QueueSize, (object) this.processingGroup.get_Concurrency());
      }
      else
      {
        this.isRunning = true;
        QueuedTaskScheduler<TTask>.log.InfoFormat("Queued tasks processing started: Queue is empty", Array.Empty<object>());
        if (this.TaskProcessingFinished != null)
          this.TaskProcessingFinished((object) this, new EventArgs());
        this.isRunning = false;
      }
    }

    private void processingGroup_OnIdle(IWorkItemsGroup workItemsGroup)
    {
      if (!this.isRunning)
        return;
      this.isRunning = false;
      SmartThreadPoolExtensions.Suspend(this.processingGroup);
      SmartThreadPoolExtensions.Suspend((IWorkItemsGroup) this.processingThreadPool);
      if (this.TaskProcessingFinished == null)
        return;
      this.TaskProcessingFinished((object) this, new EventArgs());
    }

    public void Cancel()
    {
      this.processingGroup.Cancel(false);
      QueuedTaskScheduler<TTask>.log.InfoFormat("Task processing recieved cancel signal, there are {0} active threads", (object) this.processingThreadPool.get_ActiveThreads());
      ((WorkItemsGroupBase) this.processingThreadPool).WaitForIdle();
    }

    private object ThreadPoolCallBack(object state)
    {
      if (state is TTask task)
      {
        try
        {
          if (!SmartThreadPool.get_IsWorkItemCanceled())
          {
            using (LocaleThreadState.EnsurePrimaryLocale())
              this.processingRoutine(task);
          }
        }
        catch (Exception ex)
        {
          QueuedTaskScheduler<TTask>.log.Error((object) "Unhandled exception in queued task processing:", ex);
        }
      }
      return (object) null;
    }

    public bool IsTaskCanceled
    {
      get
      {
        return SmartThreadPool.get_IsWorkItemCanceled();
      }
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
      {
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

    ~QueuedTaskScheduler()
    {
      this.Dispose(false);
    }

    public delegate void TaskProcessingRoutine(TTask task) where TTask : class;
  }
}
