// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TechnologyPollingByPollers
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Core.Models.Technology;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class TechnologyPollingByPollers : ITechnologyPolling
  {
    private PollersDAL pollersDAL = new PollersDAL();
    protected string[] pollerTypePatterns;

    public TechnologyPollingByPollers(
      string technologyID,
      string technologyPollingID,
      string displayName,
      int priority,
      string[] pollerTypePatterns)
    {
      this.TechnologyID = technologyID;
      this.TechnologyPollingID = technologyPollingID;
      this.DisplayName = displayName;
      this.Priority = priority;
      this.pollerTypePatterns = pollerTypePatterns;
    }

    public string TechnologyID { get; set; }

    public string TechnologyPollingID { get; set; }

    public string DisplayName { get; set; }

    public int Priority { get; set; }

    public int[] EnableDisableAssignment(bool enable, int[] netObjectIDs)
    {
      return ((IEnumerable<PollerAssignment>) this.pollersDAL.UpdatePollersStatus(this.pollerTypePatterns, enable, netObjectIDs)).Select<PollerAssignment, int>((Func<PollerAssignment, int>) (p => p.get_NetObjectID())).Distinct<int>().ToArray<int>();
    }

    public int[] EnableDisableAssignment(bool enable)
    {
      return ((IEnumerable<PollerAssignment>) this.pollersDAL.UpdatePollersStatus(this.pollerTypePatterns, enable, (int[]) null)).Select<PollerAssignment, int>((Func<PollerAssignment, int>) (p => p.get_NetObjectID())).Distinct<int>().ToArray<int>();
    }

    public IEnumerable<TechnologyPollingAssignment> GetAssignments()
    {
      return this.GetAssignments((int[]) null);
    }

    public IEnumerable<TechnologyPollingAssignment> GetAssignments(
      int[] netObjectIDs)
    {
      return this.pollersDAL.GetPollersAssignmentsGroupedByNetObjectId(this.TechnologyPollingID, this.pollerTypePatterns, netObjectIDs);
    }
  }
}
