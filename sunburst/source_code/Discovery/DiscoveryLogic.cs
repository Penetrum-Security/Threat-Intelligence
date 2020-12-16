// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryLogic
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Discovery;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using SolarWinds.Orion.Core.Models.DiscoveredObjects;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Interfaces;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using SolarWinds.Orion.Discovery.Framework.Pluggability;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery
{
  internal class DiscoveryLogic
  {
    private static Log log = new Log();
    private IJobFactory _jobFactory;

    internal IJobFactory JobFactory
    {
      get
      {
        return this._jobFactory ?? (this._jobFactory = (IJobFactory) new OrionDiscoveryJobFactory());
      }
      set
      {
        this._jobFactory = value;
      }
    }

    public DiscoveryResultBase FilterIgnoredItems(
      DiscoveryResultBase discoveryResult)
    {
      DiscoveryResultBase discoveryResult1 = this.FilterItems(discoveryResult, (Func<IDiscoveryPlugin, DiscoveryResultBase, DiscoveryPluginResultBase>) ((plugin, result) => plugin.FilterOutItemsFromIgnoreList(result)));
      DiscoveryConfiguration config = DiscoveryDatabase.GetDiscoveryConfiguration(((DiscoveryPluginResultBase) discoveryResult.GetPluginResultOfType<CoreDiscoveryPluginResult>()).get_ProfileId() ?? 0);
      if (config != null && config.get_IsAutoImport())
        discoveryResult1 = this.FilterItems(discoveryResult1, (Func<IDiscoveryPlugin, DiscoveryResultBase, DiscoveryPluginResultBase>) ((plugin, result) => !(plugin is ISupportAutoImport) ? (DiscoveryPluginResultBase) null : ((ISupportAutoImport) plugin).FilterAutoImportItems(result, (DiscoveryConfigurationBase) config)));
      return discoveryResult1;
    }

    private DiscoveryResultBase FilterItems(
      DiscoveryResultBase discoveryResult,
      Func<IDiscoveryPlugin, DiscoveryResultBase, DiscoveryPluginResultBase> filterFunction)
    {
      using (IEnumerator<IDiscoveryPlugin> enumerator1 = ((IEnumerable<IDiscoveryPlugin>) DiscoveryHelper.GetOrderedDiscoveryPlugins()).GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          IDiscoveryPlugin current1 = enumerator1.Current;
          DiscoveryPluginResultBase pluginResultBase = filterFunction(current1, discoveryResult);
          if (pluginResultBase != null)
          {
            pluginResultBase.set_PluginTypeName(((object) current1).GetType().FullName);
            Type returnedType = ((object) pluginResultBase).GetType();
            List<DiscoveryPluginResultBase> list = ((IEnumerable<DiscoveryPluginResultBase>) discoveryResult.get_PluginResults()).Where<DiscoveryPluginResultBase>((Func<DiscoveryPluginResultBase, bool>) (item => ((object) item).GetType() != returnedType)).ToList<DiscoveryPluginResultBase>();
            discoveryResult.get_PluginResults().Clear();
            discoveryResult.get_PluginResults().Add(pluginResultBase);
            using (List<DiscoveryPluginResultBase>.Enumerator enumerator2 = list.GetEnumerator())
            {
              while (enumerator2.MoveNext())
              {
                DiscoveryPluginResultBase current2 = enumerator2.Current;
                discoveryResult.get_PluginResults().Add(current2);
              }
            }
          }
        }
      }
      return discoveryResult;
    }

    public void DeleteOrionDiscoveryProfile(int profileID)
    {
      DiscoveryLogic.log.DebugFormat("Deleting profile {0}", (object) profileID);
      DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(profileID);
      if (profileById == null)
        throw new ArgumentNullException(string.Format("Profile {0} not found.", (object) profileID));
      this.DeleteDiscoveryProfileInternal(profileById);
    }

    public void DeleteHiddenOrionDiscoveryProfilesByName(string profileName)
    {
      DiscoveryLogic.log.DebugFormat("Deleting hidden profile '{0}'", (object) profileName);
      using (IEnumerator<DiscoveryProfileEntry> enumerator = ((IEnumerable<DiscoveryProfileEntry>) DiscoveryProfileEntry.GetProfilesByName(profileName)).Where<DiscoveryProfileEntry>((Func<DiscoveryProfileEntry, bool>) (x => x.get_IsHidden())).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
          this.DeleteDiscoveryProfileInternal(enumerator.Current);
      }
    }

    private void DeleteDiscoveryProfileInternal(DiscoveryProfileEntry profile)
    {
      if (profile.get_JobID() != Guid.Empty)
      {
        DiscoveryLogic.log.DebugFormat("Deleting job for profile {0}", (object) profile.get_ProfileID());
        try
        {
          if (this.JobFactory.DeleteJob(profile.get_JobID()))
            DiscoveryLogic.log.ErrorFormat("Error when deleting job {0}.", (object) profile.get_ProfileID());
          DiscoveryLogic.log.DebugFormat("Job for profile {0} deleted.", (object) profile.get_ProfileID());
        }
        catch (Exception ex)
        {
          DiscoveryLogic.log.ErrorFormat("Exception when deleting job {0}. Exception: {1}", (object) profile.get_ProfileID(), (object) ex);
        }
      }
      DiscoveryLogic.log.DebugFormat("Removing profile {0} from database.", (object) profile.get_ProfileID());
      DiscoveryDatabase.DeleteProfile(profile);
      DiscoveryLogic.log.DebugFormat("Profile {0} removed from database.", (object) profile.get_ProfileID());
    }

    public void ImportDiscoveryResultForProfile(
      int profileID,
      bool deleteProfileAfterImport,
      DiscoveryImportManager.CallbackDiscoveryImportFinished callback = null,
      bool checkLicenseLimits = false,
      Guid? importID = null)
    {
      IList<IDiscoveryPlugin> discoveryPlugins = DiscoveryHelper.GetOrderedDiscoveryPlugins();
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins = DiscoveryPluginHelper.GetOrderedPlugins(discoveryPlugins, (IList<DiscoveryPluginInfo>) DiscoveryHelper.GetDiscoveryPluginInfos());
      DiscoveryResultBase result1 = this.FilterIgnoredItems(DiscoveryResultManager.GetDiscoveryResult(profileID, discoveryPlugins));
      Guid importId = Guid.NewGuid();
      if (importID.HasValue)
        importId = importID.Value;
      DiscoveryImportManager.CallbackDiscoveryImportFinished callbackAfterImport = callback;
      if (deleteProfileAfterImport)
        callbackAfterImport = (DiscoveryImportManager.CallbackDiscoveryImportFinished) ((result, id, status) =>
        {
          this.DeleteOrionDiscoveryProfile(result.get_ProfileID());
          if (callback == null)
            return;
          callback(result, id, status);
        });
      DiscoveryImportManager.StartImport(importId, result1, orderedPlugins, checkLicenseLimits, callbackAfterImport);
    }

    public IEnumerable<DiscoveryPluginConfigurationBase> DeserializePluginConfigurationItems(
      List<string> discoveryPluginConfigurationBaseItems)
    {
      List<DiscoveryPluginConfigurationBase> configurationBaseList = new List<DiscoveryPluginConfigurationBase>();
      foreach (string configurationBaseItem in discoveryPluginConfigurationBaseItems)
      {
        DiscoveryPluginItems<DiscoveryPluginConfigurationBase> discoveryPluginItems = new DiscoveryPluginItems<DiscoveryPluginConfigurationBase>(configurationBaseItem);
        configurationBaseList.AddRange((IEnumerable<DiscoveryPluginConfigurationBase>) discoveryPluginItems);
      }
      return (IEnumerable<DiscoveryPluginConfigurationBase>) configurationBaseList;
    }

    public void ImportDiscoveryResultsForConfiguration(
      DiscoveryImportConfiguration importCfg,
      Guid importID)
    {
      DiscoveryLogic.log.DebugFormat("Loading discovery results.", Array.Empty<object>());
      if (DiscoveryProfileEntry.GetProfileByID(importCfg.get_ProfileID()) == null)
        throw new Exception(string.Format("Requested profile {0} not found.", (object) importCfg.get_ProfileID()));
      DiscoveryImportManager.UpdateProgress(importID, "ImportDiscoveryResults Started", "Loading Plugins", false);
      IList<IDiscoveryPlugin> discoveryPlugins = DiscoveryHelper.GetOrderedDiscoveryPlugins();
      SortedDictionary<int, List<IDiscoveryPlugin>> orderedPlugins = DiscoveryPluginHelper.GetOrderedPlugins(discoveryPlugins, (IList<DiscoveryPluginInfo>) DiscoveryHelper.GetDiscoveryPluginInfos());
      DiscoveryResultBase discoveryResult = DiscoveryResultManager.GetDiscoveryResult(importCfg.get_ProfileID(), discoveryPlugins);
      DiscoveryResultBase result1;
      if (importCfg.get_NodeIDs().Count > 0)
      {
        DiscoveryLogic.log.DebugFormat("Nodes to be imported : {0}", (object) importCfg.get_NodeIDs().Count);
        using (List<DiscoveredNode>.Enumerator enumerator = ((CoreDiscoveryPluginResult) discoveryResult.GetPluginResultOfType<CoreDiscoveryPluginResult>()).get_DiscoveredNodes().GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            DiscoveredNode current = enumerator.Current;
            if (importCfg.get_NodeIDs().Contains(current.get_NodeID()))
              ((DiscoveredObjectBase) current).set_IsSelected(true);
            else
              ((DiscoveredObjectBase) current).set_IsSelected(false);
          }
        }
        using (List<DiscoveryPluginResultBase>.Enumerator enumerator = this.Linearize((IEnumerable<DiscoveryPluginResultBase>) discoveryResult.get_PluginResults()).GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            DiscoveryPluginResultBase current = enumerator.Current;
            DiscoveryPluginResultBase pluginResultBase = !(current is IDiscoveryPluginResultContextFiltering contextFiltering) ? current.GetFilteredPluginResult() : contextFiltering.GetFilteredPluginResultFromContext(discoveryResult);
            discoveryResult.get_PluginResults().Remove(current);
            discoveryResult.get_PluginResults().Add(pluginResultBase);
            DiscoveryLogic.log.DebugFormat("Applying filters for pluggin - {0}.", (object) current.get_PluginTypeName());
          }
        }
        result1 = this.FilterIgnoredItems(discoveryResult);
      }
      else
        result1 = discoveryResult;
      result1.set_ProfileID(importCfg.get_ProfileID());
      DiscoveryLogic.log.DebugFormat("Importing started.", Array.Empty<object>());
      if (importCfg.get_DeleteProfileAfterImport())
        DiscoveryImportManager.StartImport(importID, result1, orderedPlugins, false, (DiscoveryImportManager.CallbackDiscoveryImportFinished) ((result, importId, importStatus) => this.DeleteOrionDiscoveryProfile(result.get_ProfileID())));
      else
        DiscoveryImportManager.StartImport(importID, result1, orderedPlugins);
    }

    private List<DiscoveryPluginResultBase> Linearize(
      IEnumerable<DiscoveryPluginResultBase> input)
    {
      List<DiscoveryPluginResultBase> pluginResultBaseList1 = (List<DiscoveryPluginResultBase>) Linearizer.Linearize<DiscoveryPluginResultBase>((IEnumerable<Linearizer.Input<M0>>) input.Select<DiscoveryPluginResultBase, Linearizer.Input<DiscoveryPluginResultBase>>((Func<DiscoveryPluginResultBase, Linearizer.Input<DiscoveryPluginResultBase>>) (item => (Linearizer.Input<DiscoveryPluginResultBase>) Linearizer.CreateInputItem<DiscoveryPluginResultBase>((M0) item, (IEnumerable<M0>) item.GetPrerequisites(input)))).ToArray<Linearizer.Input<DiscoveryPluginResultBase>>(), true, true);
      IEnumerable<DiscoveryPluginResultBase> collection = ((IEnumerable<DiscoveryPluginResultBase>) pluginResultBaseList1).Where<DiscoveryPluginResultBase>((Func<DiscoveryPluginResultBase, bool>) (item => item is CoreDiscoveryPluginResult && item.get_PluginTypeName() == "SolarWinds.Orion.Core.DiscoveryPlugin.CoreDiscoveryPlugin"));
      List<DiscoveryPluginResultBase> pluginResultBaseList2 = new List<DiscoveryPluginResultBase>();
      pluginResultBaseList2.AddRange(collection);
      for (int index = 0; index < pluginResultBaseList1.Count; ++index)
      {
        DiscoveryPluginResultBase pluginResultBase = pluginResultBaseList1[index];
        if (!(pluginResultBase is CoreDiscoveryPluginResult) || !(pluginResultBase.get_PluginTypeName() == "SolarWinds.Orion.Core.DiscoveryPlugin.CoreDiscoveryPlugin"))
          pluginResultBaseList2.Add(pluginResultBase);
      }
      return pluginResultBaseList2;
    }
  }
}
