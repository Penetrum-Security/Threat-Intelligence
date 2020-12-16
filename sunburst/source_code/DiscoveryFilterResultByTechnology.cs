// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DiscoveryFilterResultByTechnology
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Models.DiscoveredObjects;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class DiscoveryFilterResultByTechnology
  {
    public static IEnumerable<IDiscoveredObjectGroup> GetDiscoveryGroups(
      TechnologyManager mgr)
    {
      return (IEnumerable<IDiscoveredObjectGroup>) DiscoveryFilterResultByTechnology.GetDiscoveryGroupsInternal(mgr);
    }

    private static IEnumerable<TechnologyDiscoveryGroup> GetDiscoveryGroupsInternal(
      TechnologyManager mgr)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<TechnologyDiscoveryGroup>) new DiscoveryFilterResultByTechnology.\u003CGetDiscoveryGroupsInternal\u003Ed__1(-2)
      {
        \u003C\u003E3__mgr = mgr
      };
    }

    private static DiscoveryResultBase FilterByPriority(
      DiscoveryResultBase result,
      TechnologyManager mgr,
      bool onlyMandatory)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      if (mgr == null)
        throw new ArgumentNullException(nameof (mgr));
      ILookup<string, ITechnologyPolling> lookup = mgr.TechnologyPollingFactory.Items().ToLookup<ITechnologyPolling, string>((Func<ITechnologyPolling, string>) (tp => tp.get_TechnologyPollingID()), (IEqualityComparer<string>) StringComparer.Ordinal);
      List<IDiscoveredObject> idiscoveredObjectList = new List<IDiscoveredObject>();
      using (IEnumerator<DiscoveryPluginResultBase> enumerator = result.get_PluginResults().GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IEnumerable<IDiscoveredObject> discoveredObjects = enumerator.Current.GetDiscoveredObjects();
          idiscoveredObjectList.AddRange(discoveredObjects);
        }
      }
      List<IDiscoveredObjectWithTechnology> list1 = ((IEnumerable) idiscoveredObjectList).OfType<IDiscoveredObjectWithTechnology>().ToList<IDiscoveredObjectWithTechnology>();
      using (IEnumerator<TechnologyDiscoveryGroup> enumerator1 = DiscoveryFilterResultByTechnology.GetDiscoveryGroupsInternal(mgr).GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          TechnologyDiscoveryGroup group = enumerator1.Current;
          if (!onlyMandatory || group.get_SelectionDisabled())
          {
            IEnumerable<IDiscoveredObjectWithTechnology> list2 = (IEnumerable<IDiscoveredObjectWithTechnology>) ((IEnumerable<IDiscoveredObjectWithTechnology>) list1).Where<IDiscoveredObjectWithTechnology>((Func<IDiscoveredObjectWithTechnology, bool>) (n => group.IsMyGroupedObjectType((IDiscoveredObject) n))).ToList<IDiscoveredObjectWithTechnology>();
            List<List<IDiscoveredObjectWithTechnology>> objectWithTechnologyListList = new List<List<IDiscoveredObjectWithTechnology>>();
            using (List<IDiscoveredObject>.Enumerator enumerator2 = idiscoveredObjectList.GetEnumerator())
            {
              while (enumerator2.MoveNext())
              {
                IDiscoveredObject current1 = enumerator2.Current;
                if (((DiscoveredObjectBase) group).IsChildOf(current1))
                {
                  List<IDiscoveredObjectWithTechnology> objectWithTechnologyList = new List<IDiscoveredObjectWithTechnology>();
                  using (IEnumerator<IDiscoveredObjectWithTechnology> enumerator3 = list2.GetEnumerator())
                  {
                    while (((IEnumerator) enumerator3).MoveNext())
                    {
                      IDiscoveredObjectWithTechnology current2 = enumerator3.Current;
                      if (((IDiscoveredObject) current2).IsChildOf(current1))
                        objectWithTechnologyList.Add(current2);
                    }
                  }
                  objectWithTechnologyListList.Add(objectWithTechnologyList);
                }
              }
            }
            using (List<List<IDiscoveredObjectWithTechnology>>.Enumerator enumerator2 = objectWithTechnologyListList.GetEnumerator())
            {
              while (enumerator2.MoveNext())
              {
                List<IDiscoveredObjectWithTechnology> current = enumerator2.Current;
                if (onlyMandatory)
                {
                  if (((IEnumerable<IDiscoveredObjectWithTechnology>) current).Any<IDiscoveredObjectWithTechnology>((Func<IDiscoveredObjectWithTechnology, bool>) (to => ((IDiscoveredObject) to).get_IsSelected())))
                    continue;
                }
                else
                  current.ForEach((Action<IDiscoveredObjectWithTechnology>) (to => ((IDiscoveredObject) to).set_IsSelected(false)));
                DiscoveryFilterResultByTechnology.SelectObjectWithHigherPriority((IEnumerable<IDiscoveredObjectWithTechnology>) current, lookup);
              }
            }
          }
        }
      }
      return result;
    }

    public static DiscoveryResultBase FilterByPriority(
      DiscoveryResultBase result,
      TechnologyManager mgr)
    {
      return DiscoveryFilterResultByTechnology.FilterByPriority(result, mgr, false);
    }

    public static DiscoveryResultBase FilterMandatoryByPriority(
      DiscoveryResultBase result,
      TechnologyManager mgr)
    {
      return DiscoveryFilterResultByTechnology.FilterByPriority(result, mgr, true);
    }

    private static void SelectObjectWithHigherPriority(
      IEnumerable<IDiscoveredObjectWithTechnology> technologyObjects,
      ILookup<string, ITechnologyPolling> technologyPollingsById)
    {
      var data = technologyObjects.Select(n => new
      {
        Object = n,
        SelectionPriority = technologyPollingsById[n.get_TechnologyPollingID()].Select<ITechnologyPolling, int>((Func<ITechnologyPolling, int>) (tp => tp.get_Priority())).DefaultIfEmpty<int>(0).First<int>()
      }).OrderByDescending(n => n.SelectionPriority).FirstOrDefault();
      ((IDiscoveredObject) data?.Object).set_IsSelected(true);
    }
  }
}
