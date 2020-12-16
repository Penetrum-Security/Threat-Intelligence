// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryResultsCompletedEventArgs
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Job;
using System;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery
{
  public class DiscoveryResultsCompletedEventArgs : EventArgs
  {
    public DiscoveryResultsCompletedEventArgs(
      OrionDiscoveryJobResult completeResult,
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins,
      Guid scheduledJobId,
      JobState jobState,
      int? profileId)
    {
      this.CompleteResult = completeResult;
      this.OrderedPlugins = orderedPlugins;
      this.ScheduledJobId = scheduledJobId;
      this.JobState = jobState;
      this.ProfileId = profileId;
    }

    public OrionDiscoveryJobResult CompleteResult { get; private set; }

    public SortedDictionary<int, List<IDiscoveryPlugin>> OrderedPlugins { get; private set; }

    public Guid ScheduledJobId { get; private set; }

    public JobState JobState { get; private set; }

    public int? ProfileId { get; private set; }
  }
}
