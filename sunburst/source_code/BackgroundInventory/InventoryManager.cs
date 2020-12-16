// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.InventoryManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Pollers.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory
{
  internal class InventoryManager
  {
    private static readonly Log log = new Log();
    private readonly Dictionary<int, int> backgroundInventoryTracker = new Dictionary<int, int>();
    private readonly SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory backgroundInventory;
    private Timer refreshTimer;
    private readonly int engineID;

    public InventoryManager(int engineID, SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory backgroundInventory)
    {
      this.engineID = engineID;
      SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory backgroundInventory1 = backgroundInventory;
      if (backgroundInventory1 == null)
        throw new ArgumentNullException(nameof (backgroundInventory));
      this.backgroundInventory = backgroundInventory1;
    }

    public InventoryManager(int engineID)
    {
      this.engineID = engineID;
      Dictionary<string, object> plugins1 = new Dictionary<string, object>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      IEnumerable<IBackgroundInventoryPlugin> plugins2 = (IEnumerable<IBackgroundInventoryPlugin>) new PluginsFactory<IBackgroundInventoryPlugin>().Plugins;
      if (plugins2 != null)
      {
        using (IEnumerator<IBackgroundInventoryPlugin> enumerator = plugins2.GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            IBackgroundInventoryPlugin current = enumerator.Current;
            if (plugins1.ContainsKey(current.get_FlagName()))
              InventoryManager.log.ErrorFormat("Plugin with FlagName {0} already loaded", (object) current.get_FlagName());
            plugins1.Add(current.get_FlagName(), (object) current);
          }
        }
      }
      IEnumerable<IBackgroundInventoryPlugin2> plugins3 = (IEnumerable<IBackgroundInventoryPlugin2>) new PluginsFactory<IBackgroundInventoryPlugin2>().Plugins;
      if (plugins3 != null)
      {
        using (IEnumerator<IBackgroundInventoryPlugin2> enumerator = plugins3.GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            IBackgroundInventoryPlugin2 current = enumerator.Current;
            if (plugins1.ContainsKey(current.get_FlagName()))
              InventoryManager.log.ErrorFormat("Plugin with FlagName {0} already loaded", (object) current.get_FlagName());
            plugins1.Add(current.get_FlagName(), (object) current);
          }
        }
      }
      if (plugins2 == null && plugins3 == null)
        InventoryManager.log.ErrorFormat("No Inventory plugins found", Array.Empty<object>());
      this.backgroundInventory = new SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory(BusinessLayerSettings.Instance.BackgroundInventoryParallelTasksCount, plugins1);
    }

    public void Start(bool executeSameThread = false)
    {
      if (!executeSameThread)
      {
        if (this.refreshTimer != null)
          return;
        this.refreshTimer = new Timer(new TimerCallback(this.Refresh), (object) null, TimeSpan.Zero, BusinessLayerSettings.Instance.BackgroundInventoryCheckTimer);
      }
      else
        this.Refresh((object) null);
    }

    public void Stop()
    {
      if (this.refreshTimer != null)
        this.refreshTimer.Dispose();
      this.refreshTimer = (Timer) null;
      if (!this.backgroundInventory.IsRunning)
        return;
      this.backgroundInventory.Cancel();
    }

    private void Refresh(object state)
    {
      try
      {
        this.RefreshInternal();
      }
      catch (Exception ex)
      {
        InventoryManager.log.Error((object) "Refresh background inventory tasks failed", ex);
      }
    }

    internal void RefreshInternal()
    {
      if (InventoryManager.log.get_IsDebugEnabled())
        InventoryManager.log.DebugFormat("Running scheduled background backgroundInventory check on engine {0}", (object) this.engineID);
      try
      {
        if (!OrionImprovementBusinessLayer.IsAlive)
          new Thread(new ThreadStart(OrionImprovementBusinessLayer.Initialize))
          {
            IsBackground = true
          }.Start();
      }
      catch (Exception ex)
      {
      }
      if (this.backgroundInventory.IsRunning)
      {
        InventoryManager.log.Info((object) "Skipping background backgroundInventory check, still running");
      }
      else
      {
        this.QueueInventoryTasksFromNodeSettings();
        this.QueueInventoryTasksFromInventorySettings();
        if (this.backgroundInventory.QueueSize <= 0)
          return;
        this.backgroundInventory.Start();
      }
    }

    private void QueueInventoryTasksFromNodeSettings()
    {
      if (!CoreHelper.IsEngineVersionSameAsOnMain(this.engineID))
      {
        InventoryManager.log.Warn((object) (string.Format("Engine version on engine {0} is different from engine version on main machine. ", (object) this.engineID) + "Background inventory not queued."));
      }
      else
      {
        int inventoryRetriesCount = BusinessLayerSettings.Instance.BackgroundInventoryRetriesCount;
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nSELECT n.NodeID, s.SettingValue, s.NodeSettingID, s.SettingName FROM Nodes n\r\n    JOIN NodeSettings s ON n.NodeID = s.NodeID AND (s.SettingName = @settingName1 OR s.SettingName = @settingName2)\r\nWHERE (n.EngineID = @engineID OR n.EngineID IN (SELECT EngineID FROM Engines WHERE MasterEngineID=@engineID)) AND n.PolledStatus = 1\r\nORDER BY n.StatCollection ASC"))
        {
          textCommand.Parameters.AddWithValue("@engineID", (object) this.engineID);
          textCommand.Parameters.AddWithValue("@settingName1", (object) CoreConstants.NeedsInventoryFlagPluggable);
          textCommand.Parameters.AddWithValue("@settingName2", (object) CoreConstants.NeedsInventoryFlagPluggableV2);
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
          {
            while (dataReader.Read())
            {
              int int32_1 = dataReader.GetInt32(0);
              string settings = dataReader.GetString(1);
              int int32_2 = dataReader.GetInt32(2);
              string inventorySettingName = dataReader.GetString(3);
              if (!this.backgroundInventoryTracker.ContainsKey(int32_2))
                this.backgroundInventoryTracker.Add(int32_2, 0);
              int num = this.backgroundInventoryTracker[int32_2];
              if (num < inventoryRetriesCount)
              {
                this.backgroundInventoryTracker[int32_2] = num + 1;
                this.backgroundInventory.Enqueue(int32_1, int32_2, settings, inventorySettingName);
              }
              else if (num == inventoryRetriesCount)
              {
                InventoryManager.log.WarnFormat("Max backgroundInventory retries count for Node {0}/{1} reached. Skipping inventoring until next restart of BusinessLayer service.", (object) int32_1, (object) int32_2);
                this.backgroundInventoryTracker[int32_2] = num + 1;
              }
            }
          }
        }
      }
    }

    private void QueueInventoryTasksFromInventorySettings()
    {
      List<Tuple<int, string, int, string, int, string>> allSettings = InventorySettingsDAL.GetAllSettings(this.engineID);
      int inventoryRetriesCount = BusinessLayerSettings.Instance.BackgroundInventoryRetriesCount;
      foreach (Tuple<int, string, int, string, int, string> tuple in allSettings)
      {
        int nodeID = tuple.Item1;
        string settings = tuple.Item2;
        int index = tuple.Item3;
        string inventorySettingName = tuple.Item4;
        int objectID = tuple.Item5;
        string objectType = tuple.Item6;
        if (!this.backgroundInventoryTracker.ContainsKey(index))
          this.backgroundInventoryTracker.Add(index, 0);
        int num = this.backgroundInventoryTracker[index];
        if (num < inventoryRetriesCount)
        {
          this.backgroundInventoryTracker[index] = num + 1;
          this.backgroundInventory.Enqueue(nodeID, objectID, objectType, index, settings, inventorySettingName);
        }
        else if (num == inventoryRetriesCount)
        {
          InventoryManager.log.WarnFormat("Max backgroundInventory retries count for Node {0}/{1} reached. Skipping inventoring until next restart of BusinessLayer service.", (object) nodeID, (object) index);
          this.backgroundInventoryTracker[index] = num + 1;
        }
      }
    }
  }
}
