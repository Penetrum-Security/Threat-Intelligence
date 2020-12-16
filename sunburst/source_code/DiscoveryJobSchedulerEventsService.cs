// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DiscoveryJobSchedulerEventsService
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Orion.Core.Strings;
using System;
using System.ServiceModel;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  [ServiceBehavior(AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
  internal class DiscoveryJobSchedulerEventsService : JobSchedulerEventsService
  {
    public DiscoveryJobSchedulerEventsService(CoreBusinessLayerPlugin parent)
      : base(parent)
    {
      this.resultsManager.set_HandleResultsOfCancelledJobs(true);
    }

    protected override void ProcessJobProgress(JobProgress jobProgress)
    {
      this.RemoveOldDiscoveryJob(jobProgress.get_JobId());
    }

    protected override void ProcessJobResult(FinishedJobInfo jobInfo)
    {
      this.RemoveOldDiscoveryJob(jobInfo.get_ScheduledJobId());
    }

    protected override void ProcessJobFailure(FinishedJobInfo jobInfo)
    {
      this.RemoveOldDiscoveryJob(jobInfo.get_ScheduledJobId());
    }

    private string ComposeNotificationMessage(int newNodes, int changedNodes)
    {
      StringBuilder stringBuilder = new StringBuilder(Resources.get_LIBCODE_PCC_18());
      stringBuilder.Append(" ");
      if (newNodes == 1)
        stringBuilder.Append(Resources.get_LIBCODE_PCC_19());
      else if (newNodes > 1)
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

    private void RemoveOldDiscoveryJob(Guid jobId)
    {
      if (jobId == Guid.Empty)
      {
        JobSchedulerEventsService.log.ErrorFormat("Unable to identify id of old discovery job to delete.", Array.Empty<object>());
      }
      else
      {
        try
        {
          JobSchedulerEventsService.log.InfoFormat("Deleting old discovery job [{0}]", (object) jobId);
          if (DiscoveryJobFactory.DeleteJob(jobId))
            return;
          JobSchedulerEventsService.log.ErrorFormat("Error when deleting old discovery job [{0}]", (object) jobId);
        }
        catch (Exception ex)
        {
          JobSchedulerEventsService.log.Error((object) string.Format("Exception occured when deleting old discovery job [{0}]", (object) jobId), ex);
        }
      }
    }
  }
}
