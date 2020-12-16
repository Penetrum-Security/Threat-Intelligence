// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TechnologyPollingFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Core.Models.Technology;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class TechnologyPollingFactory
  {
    private static readonly Log log = new Log();
    private TechnologyPollingIndicator changesIndicator = new TechnologyPollingIndicator();
    internal List<ITechnologyPollingProvider> providers;

    public TechnologyPollingFactory(ComposablePartCatalog catalog)
    {
      this.providers = TechnologyPollingFactory.InitializeMEF(catalog).ToList<ITechnologyPollingProvider>();
      if (((IEnumerable<ITechnologyPollingProvider>) this.providers).Any<ITechnologyPollingProvider>())
        TechnologyPollingFactory.log.Info((object) ("Technology loader found technology polling providers: " + string.Join(",", ((IEnumerable<ITechnologyPollingProvider>) this.providers).Select<ITechnologyPollingProvider, string>((Func<ITechnologyPollingProvider, string>) (t => ((object) t).GetType().FullName)).ToArray<string>())));
      else
        TechnologyPollingFactory.log.Error((object) "Technology loader found 0 technology polling providers");
    }

    protected static IEnumerable<ITechnologyPollingProvider> InitializeMEF(
      ComposablePartCatalog catalog)
    {
      using (CompositionContainer compositionContainer = new CompositionContainer(catalog, Array.Empty<ExportProvider>()))
        return (IEnumerable<ITechnologyPollingProvider>) compositionContainer.GetExports<ITechnologyPollingProvider>().Select<Lazy<ITechnologyPollingProvider>, ITechnologyPollingProvider>((Func<Lazy<ITechnologyPollingProvider>, ITechnologyPollingProvider>) (n => n.Value)).ToList<ITechnologyPollingProvider>();
    }

    public IEnumerable<ITechnologyPolling> Items()
    {
      return ((IEnumerable<ITechnologyPollingProvider>) this.providers).SelectMany<ITechnologyPollingProvider, ITechnologyPolling>((Func<ITechnologyPollingProvider, IEnumerable<ITechnologyPolling>>) (n => n.get_Items()));
    }

    public IEnumerable<ITechnologyPolling> ItemsByTechnology(
      string technologyID)
    {
      if (string.IsNullOrEmpty(technologyID))
        throw new ArgumentNullException(nameof (technologyID));
      return this.Items().Where<ITechnologyPolling>((Func<ITechnologyPolling, bool>) (n => n.get_TechnologyID() == technologyID));
    }

    public ITechnologyPolling GetTechnologyPolling(string technologyPollingID)
    {
      if (string.IsNullOrEmpty(technologyPollingID))
        throw new ArgumentNullException(nameof (technologyPollingID));
      return this.Items().Single<ITechnologyPolling>((Func<ITechnologyPolling, bool>) (n => n.get_TechnologyPollingID() == technologyPollingID));
    }

    public int[] EnableDisableAssignments(
      string technologyPollingID,
      bool enable,
      int[] netObjectIDs = null)
    {
      if (string.IsNullOrEmpty(technologyPollingID))
        throw new ArgumentNullException(nameof (technologyPollingID));
      ITechnologyPolling technologyPolling = this.GetTechnologyPolling(technologyPollingID);
      int[] netObjectsInstanceIDs = (netObjectIDs == null ? technologyPolling.EnableDisableAssignment(enable) : technologyPolling.EnableDisableAssignment(enable, netObjectIDs)) ?? new int[0];
      TechnologyPollingFactory.log.DebugFormat("{0} TechnologyPolling:'{1}' of Technology:'{2}' on NetObjects:'{3}'", new object[4]
      {
        enable ? (object) "Enabled" : (object) "Disabled",
        (object) technologyPollingID,
        (object) technologyPolling.get_TechnologyID(),
        netObjectsInstanceIDs == null ? (object) "" : (object) string.Join<int>(",", (IEnumerable<int>) netObjectsInstanceIDs)
      });
      if (enable)
      {
        using (IEnumerator<ITechnologyPolling> enumerator = this.ItemsByTechnology(technologyPolling.get_TechnologyID()).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            ITechnologyPolling current = enumerator.Current;
            if (!technologyPollingID.Equals(current.get_TechnologyPollingID(), StringComparison.Ordinal))
            {
              int[] numArray = current.EnableDisableAssignment(false, netObjectsInstanceIDs) ?? new int[0];
              TechnologyPollingFactory.log.DebugFormat("{0} TechnologyPolling:'{1}' of Technology:'{2}' on NetObjects:'{3}'", new object[4]
              {
                (object) "Disabled",
                (object) current.get_TechnologyPollingID(),
                (object) current.get_TechnologyID(),
                numArray == null ? (object) "" : (object) string.Join<int>(",", (IEnumerable<int>) numArray)
              });
            }
          }
        }
      }
      if (BusinessLayerSettings.Instance.EnableTechnologyPollingAssignmentsChangesAuditing)
        this.changesIndicator.ReportTechnologyPollingAssignmentIndication(technologyPolling, netObjectsInstanceIDs, enable);
      return netObjectsInstanceIDs;
    }

    public IEnumerable<TechnologyPollingAssignment> GetAssignments(
      string technologyPollingID)
    {
      if (string.IsNullOrEmpty(technologyPollingID))
        throw new ArgumentNullException(nameof (technologyPollingID));
      return this.GetTechnologyPolling(technologyPollingID).GetAssignments();
    }

    public IEnumerable<TechnologyPollingAssignment> GetAssignments(
      string technologyPollingID,
      int[] netObjectIDs)
    {
      if (string.IsNullOrEmpty(technologyPollingID))
        throw new ArgumentNullException(nameof (technologyPollingID));
      return this.GetTechnologyPolling(technologyPollingID).GetAssignments(netObjectIDs);
    }

    public IEnumerable<TechnologyPollingAssignment> GetAssignmentsFiltered(
      string[] technologyPollingIDsFilter,
      int[] netObjectIDsFilter,
      string[] targetEntitiesFilter,
      bool[] enabledFilter)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<TechnologyPollingAssignment>) new TechnologyPollingFactory.\u003CGetAssignmentsFiltered\u003Ed__11(-2)
      {
        \u003C\u003E4__this = this,
        \u003C\u003E3__technologyPollingIDsFilter = technologyPollingIDsFilter,
        \u003C\u003E3__netObjectIDsFilter = netObjectIDsFilter,
        \u003C\u003E3__targetEntitiesFilter = targetEntitiesFilter,
        \u003C\u003E3__enabledFilter = enabledFilter
      };
    }
  }
}
