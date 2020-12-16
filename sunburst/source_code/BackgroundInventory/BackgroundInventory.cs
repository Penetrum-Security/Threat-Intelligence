// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Models.Enums;
using SolarWinds.Orion.Core.SharedCredentials;
using SolarWinds.Orion.Pollers.Framework;
using SolarWinds.Orion.Pollers.Framework.SNMP;
using SolarWinds.Orion.Pollers.Framework.WMI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory
{
  public class BackgroundInventory : IDisposable
  {
    private static readonly Log log = new Log();
    private IPollersDAL pollersDAL = (IPollersDAL) new PollersDAL();
    private QueuedTaskScheduler<SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask> scheduler;
    private Dictionary<string, object> inventories;
    private SnmpGlobalSettings snmpGlobals;
    private WmiGlobalSettings wmiGlobals;
    private bool disposed;

    public bool IsRunning
    {
      get
      {
        return this.scheduler.IsRunning;
      }
    }

    public int QueueSize
    {
      get
      {
        return this.scheduler.QueueSize;
      }
    }

    public virtual bool IsScheduledTaskCanceled()
    {
      return this.scheduler.IsTaskCanceled;
    }

    public BackgroundInventory(int parallelTasksCount, Dictionary<string, object> plugins)
    {
      if (plugins == null)
        throw new ArgumentNullException(nameof (plugins));
      this.scheduler = new QueuedTaskScheduler<SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask>(new QueuedTaskScheduler<SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask>.TaskProcessingRoutine(this.DoInventory), parallelTasksCount);
      this.scheduler.TaskProcessingFinished += new EventHandler(this.scheduler_TaskProcessingFinished);
      this.snmpGlobals = new SnmpGlobalSettings();
      this.snmpGlobals.set_MaxReplies(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP MaxReps", 5));
      this.snmpGlobals.set_RequestTimeout(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP Timeout", 2500));
      this.snmpGlobals.set_RequestRetries(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-SNMP Retries", 2));
      this.snmpGlobals.set_HsrpEnabled((bool) SettingsDAL.GetCurrent<bool>("SWNetPerfMon-Settings-SNMP HSRPEnabled", (M0) 1));
      this.wmiGlobals = new WmiGlobalSettings();
      this.wmiGlobals.set_UserImpersonationLevel((ImpersonationLevel) SettingsDAL.GetCurrent<ImpersonationLevel>("SWNetPerfMon-Settings-Wmi UserImpersonationLevel", (M0) 0));
      this.wmiGlobals.set_ConnectionRationMode((WmiConnectionRationMode) SettingsDAL.GetCurrent<WmiConnectionRationMode>("SWNetPerfMon-Settings-Wmi ConnectionRationMode", (M0) 1));
      this.wmiGlobals.set_MaxRationedConnections(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-Wmi MaxRationedConnections", 0));
      this.wmiGlobals.set_KillProcessExcessiveError((bool) SettingsDAL.GetCurrent<bool>("SWNetPerfMon-Settings-Wmi KillProcessExcessiveError", (M0) 1));
      this.wmiGlobals.set_ExcessiveErrorThreshhold(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-Wmi ExcessiveErrorThreshhold", 50));
      this.wmiGlobals.set_WmiRetries(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retries", 0));
      this.wmiGlobals.set_WmiRetryInterval(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Retry Interval", 0));
      this.wmiGlobals.set_WmiAutoCorrectRDNSInconsistency(Convert.ToBoolean(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Auto Correct Reverse DNS", 0)));
      this.wmiGlobals.set_WmiDefaultRootNamespaceOverrideIndex(SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-WMI Default Root Namespace Override Index", 0));
      this.inventories = plugins;
    }

    private void scheduler_TaskProcessingFinished(object sender, EventArgs e)
    {
      SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.Info((object) "Background Inventorying Finished");
    }

    public virtual void Enqueue(
      int nodeID,
      int objectID,
      string objectType,
      int nodeSettingID,
      string settings,
      string inventorySettingName)
    {
      this.scheduler.EnqueueTask(new SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask(nodeID, objectID, objectType, nodeSettingID, settings, inventorySettingName, SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource.InventorySettings));
    }

    public virtual void Enqueue(
      int nodeID,
      int nodeSettingID,
      string settings,
      string inventorySettingName)
    {
      this.scheduler.EnqueueTask(new SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask(nodeID, -1, string.Empty, nodeSettingID, settings, inventorySettingName, SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource.NodeSettings));
    }

    public void Start()
    {
      this.scheduler.Start();
    }

    public void Cancel()
    {
      this.scheduler.Cancel();
    }

    public virtual Node GetNode(int nodeId)
    {
      return NodeBLDAL.GetNode(nodeId);
    }

    public virtual Credential GetCredentialsForNode(Node node)
    {
      return CredentialHelper.ParseCredentialsFromNode(node);
    }

    public void DoInventory(SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask task)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      Node node = this.GetNode(task.NodeID);
      if (node == null || node.get_PolledStatus() != 1)
      {
        SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Skipping inventorying of Node {0}, status is not UP.", (object) task.NodeID);
      }
      else
      {
        Credential credentialsForNode = this.GetCredentialsForNode(node);
        GlobalSettingsBase globals = (GlobalSettingsBase) this.snmpGlobals;
        if (node.get_NodeSubType() == 3)
          globals = (GlobalSettingsBase) this.wmiGlobals;
        if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
          SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Starting inventorying of Node {0}, NeedsInventory = '{1}'", (object) task.NodeID, (object) task.Settings);
        string[] strArray = task.Settings.Split(':');
        List<string> failedTasks = new List<string>();
        List<string> completedTasks = new List<string>();
        foreach (string key in strArray)
        {
          SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Attempting to inventory with plugin '{0}' on Node {1}", (object) key, (object) task.NodeID);
          if (!this.inventories.ContainsKey(key))
          {
            failedTasks.Add(key);
            if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsErrorEnabled())
              SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.ErrorFormat("Unable to inventory '{0}' on Node {1}", (object) key, (object) task.NodeID);
          }
          else
          {
            if (this.IsScheduledTaskCanceled())
            {
              if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
                SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Inventorying of Node {0} was canceled. ElapsedTime = {1}", (object) task.NodeID, (object) stopwatch.ElapsedMilliseconds);
              stopwatch.Stop();
              return;
            }
            if (!this.IsValidPlugin(this.inventories[key]))
            {
              failedTasks.Add(key);
              if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsErrorEnabled())
                SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.ErrorFormat("No plugins are available to execute Inventory '{0}' on Node {1} returned null result", (object) key, (object) task.NodeID);
            }
            else
            {
              InventoryResultBase result = this.DoInventory(this.inventories[key], task, globals, credentialsForNode, node);
              if (result == null)
              {
                failedTasks.Add(key);
                if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsErrorEnabled())
                  SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.ErrorFormat("Inventory '{0}' on Node {1} returned null result", (object) key, (object) task.NodeID);
              }
              else
              {
                if (result.get_Outcome() == 1)
                {
                  bool flag = false;
                  try
                  {
                    flag = this.ProcessResults(this.inventories[key], task, result, node);
                  }
                  catch (Exception ex)
                  {
                    SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.Error((object) string.Format("Inventory '{0}' failed to import results for {1}", (object) task, (object) key), ex);
                  }
                  if (flag)
                    completedTasks.Add(key);
                  else
                    failedTasks.Add(key);
                }
                else
                {
                  failedTasks.Add(key);
                  if (result.get_Error() != null)
                  {
                    if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsWarnEnabled())
                      SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.WarnFormat("Inventory '{0}' on Node {1} failed with code {2}", (object) key, (object) task, (object) result.get_Error().get_ErrorCode());
                    if (result.get_Error().get_ErrorCode() != 31002U)
                    {
                      List<string> list = ((IEnumerable<string>) strArray).Where<string>((Func<string, bool>) (n => !completedTasks.Contains(n) && !failedTasks.Contains(n))).ToList<string>();
                      if (list.Count > 0)
                      {
                        failedTasks.AddRange((IEnumerable<string>) list);
                        if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsWarnEnabled())
                        {
                          SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.WarnFormat("Skipping inventory for '{0}' on Node {1}", (object) string.Join(":", list.ToArray()), (object) task.NodeID);
                          break;
                        }
                        break;
                      }
                    }
                  }
                  else if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsWarnEnabled())
                    SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.WarnFormat("Inventory '{0}' on Node {1} failed on unknown error", (object) key, (object) task.NodeID);
                }
                SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Inventory with plugin '{0}' on Node {1} is completed", (object) key, (object) task.NodeID);
              }
            }
          }
        }
        string settingsForTask = this.GetSettingsForTask(task);
        if ((string.IsNullOrEmpty(settingsForTask) || !settingsForTask.Equals(task.Settings, StringComparison.OrdinalIgnoreCase)) && SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
        {
          SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Skipping inventory result processing for {0}, NeedsInventory flag changed. OldValue = '{1}', NewValue = '{2}'.", (object) task, (object) task.Settings, (object) settingsForTask);
        }
        else
        {
          if (failedTasks.Count == 0)
          {
            if (task.InventoryInput == SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource.NodeSettings)
              NodeSettingsDAL.DeleteSpecificSettings(task.ObjectSettingID, task.InventorySettingName);
            else
              InventorySettingsDAL.DeleteSpecificSettings(task.ObjectSettingID, task.InventorySettingName);
            if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
              SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Inventorying of {0} completed in {1}ms.", (object) task, (object) stopwatch.ElapsedMilliseconds);
          }
          else if (failedTasks.Count < strArray.Length)
          {
            string str = string.Join(":", failedTasks.ToArray());
            if (task.InventoryInput == SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource.NodeSettings)
              NodeSettingsDAL.UpdateSettingValue(task.ObjectSettingID, task.InventorySettingName, (object) str);
            else
              InventorySettingsDAL.UpdateSettingValue(task.ObjectSettingID, task.InventorySettingName, (object) str);
            if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
              SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Inventorying of {0} partially completed in {1}ms. NeedsInventory updated to '{2}'", (object) task, (object) stopwatch.ElapsedMilliseconds, (object) str);
          }
          else if (SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.get_IsInfoEnabled())
            SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.log.InfoFormat("Inventorying of {0} failed. Elapsed time {1}ms.", (object) task, (object) stopwatch.ElapsedMilliseconds);
          stopwatch.Stop();
        }
      }
    }

    private bool IsValidPlugin(object plugin)
    {
      IBackgroundInventoryPlugin ibackgroundInventoryPlugin = plugin as IBackgroundInventoryPlugin;
      IBackgroundInventoryPlugin2 inventoryPlugin2 = plugin as IBackgroundInventoryPlugin2;
      return ibackgroundInventoryPlugin != null || inventoryPlugin2 != null;
    }

    private InventoryResultBase DoInventory(
      object plugin,
      SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask task,
      GlobalSettingsBase globals,
      Credential credentials,
      Node node)
    {
      switch (plugin)
      {
        case IBackgroundInventoryPlugin ibackgroundInventoryPlugin:
          return ibackgroundInventoryPlugin.DoInventory(globals, credentials, node);
        case IBackgroundInventoryPlugin2 inventoryPlugin2:
          return inventoryPlugin2.DoInventory(globals, credentials, new BackgroundInventoryObject(node, task.ObjectID, task.ObjectType));
        default:
          return (InventoryResultBase) null;
      }
    }

    private bool ProcessResults(
      object plugin,
      SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask task,
      InventoryResultBase result,
      Node node)
    {
      switch (plugin)
      {
        case IBackgroundInventoryPlugin ibackgroundInventoryPlugin:
          return ibackgroundInventoryPlugin.ProcessResults(result, node);
        case IBackgroundInventoryPlugin2 inventoryPlugin2:
          return inventoryPlugin2.ProcessResults(result, new BackgroundInventoryObject(node, task.ObjectID, task.ObjectType));
        default:
          return false;
      }
    }

    private string GetSettingsForTask(SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask task)
    {
      return task.InventoryInput != SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource.NodeSettings ? InventorySettingsDAL.GetInventorySettings(task.ObjectSettingID, task.InventorySettingName) : NodeSettingsDAL.GetNodeSettings(task.ObjectSettingID, task.InventorySettingName);
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing && this.scheduler != null)
      {
        this.scheduler.Dispose();
        this.scheduler = (QueuedTaskScheduler<SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask>) null;
      }
      this.disposed = true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~BackgroundInventory()
    {
      this.Dispose(false);
    }

    public class InventoryTask
    {
      public int NodeID;
      public int ObjectSettingID;
      public string Settings;
      public string InventorySettingName;
      public int ObjectID;
      public string ObjectType;
      public SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource InventoryInput;

      public InventoryTask(
        int nodeID,
        int objectID,
        string objectType,
        int objectSettingID,
        string settings,
        string inventorySettingName,
        SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.BackgroundInventory.InventoryTask.InventoryInputSource inventoryInputSource)
      {
        this.NodeID = nodeID;
        this.ObjectSettingID = objectSettingID;
        this.Settings = settings;
        this.InventorySettingName = inventorySettingName;
        this.ObjectID = objectID;
        this.ObjectType = objectType;
        this.InventoryInput = inventoryInputSource;
      }

      public override string ToString()
      {
        return string.Format("NodeID = {0}, NodeSettingID = {1}, Settings = {2}, InventorySettingName = {3}, ObjectID = {4}, ObjectType = {5}", (object) this.NodeID, (object) this.ObjectSettingID, (object) this.Settings, (object) this.InventorySettingName, (object) this.ObjectID, (object) this.ObjectType);
      }

      public enum InventoryInputSource
      {
        NodeSettings,
        InventorySettings,
      }
    }
  }
}
