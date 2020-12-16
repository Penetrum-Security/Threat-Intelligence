// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintenanceMode.MaintenanceManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Models.MaintenanceMode;
using System;
using System.Collections.Generic;
using System.Data;

namespace SolarWinds.Orion.Core.BusinessLayer.MaintenanceMode
{
  public class MaintenanceManager : IMaintenanceManager
  {
    private const string UnmanageVerbName = "Unmanage";
    private const string RemanageVerbName = "Remanage";
    private readonly IInformationServiceProxyCreator swisProxy;
    private readonly IMaintenanceModePlanDAL maintenancePlanDAL;

    public MaintenanceManager(
      IInformationServiceProxyCreator swisProxy,
      IMaintenanceModePlanDAL maintenancePlanDAL)
    {
      this.swisProxy = swisProxy;
      this.maintenancePlanDAL = maintenancePlanDAL;
    }

    public void Unmanage(MaintenancePlanAssignment assignment)
    {
      MaintenancePlan maintenancePlan = this.maintenancePlanDAL.Get(assignment.get_MaintenancePlanID());
      if (maintenancePlan == null)
        throw new Exception(string.Format("No maintenance plan found for PlanID={0}.", (object) assignment.get_MaintenancePlanID()));
      string netObjectPrefix = this.GetNetObjectPrefix(assignment.get_EntityType());
      if (netObjectPrefix == null)
        throw new Exception(string.Format("Cannot find net object prefix for EntityType='{0}'.", (object) assignment.get_EntityType()));
      object netObjectId = this.CreateNetObjectId(netObjectPrefix, assignment.get_EntityID());
      if (netObjectId == null)
        throw new Exception(string.Format("Cannot create net object id from prefix '{0}' and id '{1}'.", (object) netObjectPrefix, (object) assignment.get_EntityID()));
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.swisProxy.Create())
        iinformationServiceProxy2.Invoke<object>(assignment.get_EntityType(), nameof (Unmanage), new object[4]
        {
          netObjectId,
          (object) maintenancePlan.get_UnmanageDate(),
          (object) maintenancePlan.get_RemanageDate(),
          (object) false
        });
    }

    public void Remanage(MaintenancePlanAssignment assignment)
    {
      string netObjectPrefix = this.GetNetObjectPrefix(assignment.get_EntityType());
      if (netObjectPrefix == null)
        throw new Exception(string.Format("Cannot find net object prefix for EntityType='{0}'.", (object) assignment.get_EntityType()));
      object netObjectId = this.CreateNetObjectId(netObjectPrefix, assignment.get_EntityID());
      if (netObjectId == null)
        throw new Exception(string.Format("Cannot create net object id from prefix '{0}' and id '{1}'.", (object) netObjectPrefix, (object) assignment.get_EntityID()));
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.swisProxy.Create())
        iinformationServiceProxy2.Invoke<object>(assignment.get_EntityType(), nameof (Remanage), new object[1]
        {
          netObjectId
        });
    }

    internal object CreateNetObjectId(string prefix, int id)
    {
      if (string.IsNullOrEmpty(prefix))
        return (object) null;
      return (object) string.Join(":", (object) prefix, (object) id);
    }

    internal string GetNetObjectPrefix(string entityName)
    {
      if (string.IsNullOrEmpty(entityName))
        return (string) null;
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.swisProxy.Create())
      {
        DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query("SELECT Prefix FROM Orion.NetObjectTypes WHERE EntityType = @entityName", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            nameof (entityName),
            (object) entityName
          }
        });
        return dataTable != null && dataTable.Rows != null && (dataTable.Rows.Count == 1 && dataTable.Rows[0][0] != null) ? dataTable.Rows[0][0].ToString() : (string) null;
      }
    }
  }
}
