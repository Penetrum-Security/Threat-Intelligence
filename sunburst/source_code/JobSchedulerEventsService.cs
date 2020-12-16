// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.JobSchedulerEventsService
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.i18n;
using System;
using System.ServiceModel;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  [ServiceBehavior(AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
  internal abstract class JobSchedulerEventsService : IJobSchedulerEvents
  {
    protected static readonly Log log = new Log();
    protected JobResultsManager resultsManager = new JobResultsManager();
    private readonly CoreBusinessLayerPlugin parent;

    public JobSchedulerEventsService(CoreBusinessLayerPlugin parent)
    {
      this.parent = parent;
      JobResultsManager resultsManager = this.resultsManager;
      // ISSUE: variable of the null type
      __Null jobFailure = resultsManager.JobFailure;
      JobSchedulerEventsService schedulerEventsService = this;
      // ISSUE: virtual method pointer
      JobResultsManager.JobFailureDelegate jobFailureDelegate = new JobResultsManager.JobFailureDelegate((object) schedulerEventsService, __vmethodptr(schedulerEventsService, ProcessJobFailure));
      resultsManager.JobFailure = (__Null) Delegate.Combine((Delegate) jobFailure, (Delegate) jobFailureDelegate);
    }

    public void OnJobProgress(JobProgress[] jobProgressInfo)
    {
      using (LocaleThreadState.EnsurePrimaryLocale())
      {
        foreach (JobProgress jobProgress in jobProgressInfo)
          this.ProcessJobProgress(jobProgress);
      }
    }

    public void OnJobFinished(FinishedJobInfo[] jobFinishedInfo)
    {
      using (LocaleThreadState.EnsurePrimaryLocale())
      {
        if (this.parent.IsServiceDown)
        {
          JobSchedulerEventsService.log.InfoFormat("Core Service Engine is in an invalid state.  Job results will be discarded.", Array.Empty<object>());
        }
        else
        {
          this.resultsManager.AddJobResults(jobFinishedInfo);
          for (FinishedJobInfo jobResult = this.resultsManager.GetJobResult(); jobResult != null; jobResult = this.resultsManager.GetJobResult())
          {
            try
            {
              this.ProcessJobResult(jobResult);
            }
            catch (Exception ex)
            {
              JobSchedulerEventsService.log.Error((object) "Error processing job", ex);
            }
            finally
            {
              this.resultsManager.FinishProcessingJobResult(jobResult);
            }
          }
        }
      }
    }

    protected abstract void ProcessJobProgress(JobProgress jobProgress);

    protected abstract void ProcessJobFailure(FinishedJobInfo jobResult);

    protected abstract void ProcessJobResult(FinishedJobInfo jobResult);

    protected void RemoveJob(Guid jobId)
    {
      try
      {
        using (IJobSchedulerHelper instance = JobScheduler.GetInstance())
        {
          JobSchedulerEventsService.log.DebugFormat("Removing job {0}", (object) jobId);
          ((IJobScheduler) instance).RemoveJob(jobId);
        }
      }
      catch (Exception ex)
      {
        JobSchedulerEventsService.log.ErrorFormat("Error removing job {0}.  Exception: {1}", (object) jobId, (object) ex.ToString());
      }
    }
  }
}
