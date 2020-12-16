// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TechnologyPollingIndicator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Contract2;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common.Auditing;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Core.Models.Technology;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.PubSub.Implementation.Publisher;
using SolarWinds.Orion.Swis.PubSub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class TechnologyPollingIndicator
  {
    private readonly IInformationServiceProxyFactory swisFactory;
    private readonly IPublisherManager publishManager;

    public static Action AuditTechnologiesChanges(
      IEnumerable<IDiscoveredObject> discoveredObjects,
      int nodeId)
    {
      if (!BusinessLayerSettings.Instance.EnableTechnologyPollingAssignmentsChangesAuditing)
        return (Action) (() => {});
      Dictionary<string, ITechnology> technologies = TechnologyManager.Instance.TechnologyFactory.Items().ToDictionary<ITechnology, string, ITechnology>((Func<ITechnology, string>) (k => k.get_TechnologyID()), (Func<ITechnology, ITechnology>) (v => v), (IEqualityComparer<string>) StringComparer.Ordinal);
      Dictionary<string, string> dictionary1 = TechnologyManager.Instance.TechnologyPollingFactory.Items().Where<ITechnologyPolling>((Func<ITechnologyPolling, bool>) (tp => technologies.ContainsKey(tp.get_TechnologyID()))).ToDictionary<ITechnologyPolling, string, string>((Func<ITechnologyPolling, string>) (k => k.get_TechnologyPollingID()), (Func<ITechnologyPolling, string>) (v => technologies[v.get_TechnologyID()].get_TargetEntity()), (IEqualityComparer<string>) StringComparer.Ordinal);
      IEnumerable<IDiscoveredObjectWithTechnology> objectWithTechnologies = ((IEnumerable) discoveredObjects).OfType<IDiscoveredObjectWithTechnology>();
      List<TechnologyPollingAssignment> changedAssignments = new List<TechnologyPollingAssignment>();
      using (IEnumerator<IDiscoveredObjectWithTechnology> enumerator = objectWithTechnologies.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveredObjectWithTechnology current = enumerator.Current;
          if (dictionary1.ContainsKey(current.get_TechnologyPollingID()) && "Orion.Nodes".Equals(dictionary1[current.get_TechnologyPollingID()], StringComparison.Ordinal))
          {
            int num1 = nodeId;
            TechnologyPollingAssignment pollingAssignment1 = TechnologyManager.Instance.TechnologyPollingFactory.GetAssignments(current.get_TechnologyPollingID(), new int[1]
            {
              num1
            }).FirstOrDefault<TechnologyPollingAssignment>();
            int num2 = pollingAssignment1 == null ? 0 : (pollingAssignment1.get_Enabled() ? 1 : 0);
            bool isSelected = ((IDiscoveredObject) current).get_IsSelected();
            int num3 = isSelected ? 1 : 0;
            if (num2 != num3)
            {
              List<TechnologyPollingAssignment> pollingAssignmentList = changedAssignments;
              TechnologyPollingAssignment pollingAssignment2 = new TechnologyPollingAssignment();
              pollingAssignment2.set_TechnologyPollingID(current.get_TechnologyPollingID());
              pollingAssignment2.set_NetObjectID(num1);
              pollingAssignment2.set_Enabled(isSelected);
              pollingAssignmentList.Add(pollingAssignment2);
            }
          }
        }
      }
      return (Action) (() =>
      {
        if (changedAssignments.Count == 0)
          return;
        Dictionary<string, ITechnologyPolling> dictionary2 = TechnologyManager.Instance.TechnologyPollingFactory.Items().ToDictionary<ITechnologyPolling, string, ITechnologyPolling>((Func<ITechnologyPolling, string>) (k => k.get_TechnologyPollingID()), (Func<ITechnologyPolling, ITechnologyPolling>) (v => v), (IEqualityComparer<string>) StringComparer.Ordinal);
        TechnologyPollingIndicator pollingIndicator = new TechnologyPollingIndicator();
        using (List<TechnologyPollingAssignment>.Enumerator enumerator = changedAssignments.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            TechnologyPollingAssignment current = enumerator.Current;
            pollingIndicator.ReportTechnologyPollingAssignmentIndication(dictionary2[current.get_TechnologyPollingID()], new int[1]
            {
              current.get_NetObjectID()
            }, (current.get_Enabled() ? 1 : 0) != 0);
          }
        }
      });
    }

    public TechnologyPollingIndicator()
      : this((IInformationServiceProxyFactory) new InformationServiceProxyFactory(), PublisherClient.get_Instance())
    {
    }

    public TechnologyPollingIndicator(
      IInformationServiceProxyFactory swisFactory,
      IPublisherManager indicationReporter)
    {
      if (swisFactory == null)
        throw new ArgumentNullException(nameof (swisFactory));
      if (indicationReporter == null)
        throw new ArgumentNullException(nameof (indicationReporter));
      this.swisFactory = swisFactory;
      this.publishManager = indicationReporter;
    }

    public void ReportTechnologyPollingAssignmentIndication(
      ITechnologyPolling technologyPolling,
      int[] netObjectsInstanceIDs,
      bool enabledStateChangedTo)
    {
      if (technologyPolling == null)
        throw new ArgumentNullException(nameof (technologyPolling));
      if (netObjectsInstanceIDs == null)
        throw new ArgumentNullException(nameof (netObjectsInstanceIDs));
      if (netObjectsInstanceIDs.Length == 0)
        return;
      ITechnology technology = TechnologyManager.Instance.TechnologyFactory.GetTechnology(technologyPolling.get_TechnologyID());
      string netObjectPrefix = NetObjectTypesDAL.GetNetObjectPrefix(this.swisFactory, technology.get_TargetEntity());
      string entityName = NetObjectTypesDAL.GetEntityName(this.swisFactory, technology.get_TargetEntity());
      Dictionary<int, string> netObjectsCaptions = NetObjectTypesDAL.GetNetObjectsCaptions(this.swisFactory, technology.get_TargetEntity(), netObjectsInstanceIDs);
      foreach (int objectsInstanceId in netObjectsInstanceIDs)
      {
        PropertyBag propertyBag1 = new PropertyBag();
        ((Dictionary<string, object>) propertyBag1).Add("InstanceType", (object) "Orion.TechnologyPollingAssignments");
        ((Dictionary<string, object>) propertyBag1).Add("InstanceID", (object) objectsInstanceId.ToString());
        ((Dictionary<string, object>) propertyBag1).Add("TechnologyPollingID", (object) technologyPolling.get_TechnologyPollingID());
        ((Dictionary<string, object>) propertyBag1).Add("Enabled", (object) enabledStateChangedTo);
        ((Dictionary<string, object>) propertyBag1).Add("TargetEntity", (object) technology.get_TargetEntity());
        ((Dictionary<string, object>) propertyBag1).Add("TechPollingDispName", (object) technologyPolling.get_DisplayName());
        ((Dictionary<string, object>) propertyBag1).Add("TechnologyDispName", (object) technology.get_DisplayName());
        PropertyBag propertyBag2 = propertyBag1;
        string str;
        if (netObjectsCaptions.TryGetValue(objectsInstanceId, out str))
          ((Dictionary<string, object>) propertyBag2).Add("NetObjectCaption", (object) str);
        if (netObjectPrefix != null)
        {
          ((Dictionary<string, object>) propertyBag2).Add("NetObjectPrefix", (object) netObjectPrefix);
          ((Dictionary<string, object>) propertyBag2).Add((string) KnownKeys.NetObject, (object) string.Format("{0}:{1}", (object) netObjectPrefix, (object) objectsInstanceId));
        }
        if (entityName != null)
          ((Dictionary<string, object>) propertyBag2).Add("NetObjectName", (object) entityName);
        this.publishManager.Publish(PublishHelper.ConvertIndication((IIndication) new TechnologyPollingAssignmentIndication((IndicationType) 2, propertyBag2)));
      }
    }
  }
}
