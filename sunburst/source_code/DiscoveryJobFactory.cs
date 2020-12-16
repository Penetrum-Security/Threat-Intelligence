// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DiscoveryJobFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Logging;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public static class DiscoveryJobFactory
  {
    private static readonly Log log = new Log();

    public static bool DeleteJob(Guid jobId)
    {
      using (IJobSchedulerHelper instance = JobScheduler.GetInstance())
      {
        try
        {
          ((IJobScheduler) instance).RemoveJob(jobId);
          return true;
        }
        catch
        {
          DiscoveryJobFactory.log.DebugFormat("Unable to delete job in Job Engine({0}", (object) jobId);
          return false;
        }
      }
    }
  }
}
