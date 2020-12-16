// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MaintenanceModePlanDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Extensions;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Models.MaintenanceMode;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class MaintenanceModePlanDAL : IMaintenanceModePlanDAL
  {
    private static readonly Log log = new Log();
    private const string entityName = "Orion.MaintenancePlan";
    private IInformationServiceProxyCreator _SwisFactory;

    internal static Dictionary<string, object> RemoveKeysFromDictionary(
      Dictionary<string, object> source,
      params string[] keysToRemove)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (keysToRemove == null)
        throw new ArgumentNullException(nameof (keysToRemove));
      return source.Where<KeyValuePair<string, object>>((Func<KeyValuePair<string, object>, bool>) (kvp => !((IEnumerable<string>) keysToRemove).Contains<string>(kvp.Key))).ToDictionary<KeyValuePair<string, object>, string, object>((Func<KeyValuePair<string, object>, string>) (kvp => kvp.Key), (Func<KeyValuePair<string, object>, object>) (kvp => kvp.Value));
    }

    internal static T GetValue<T>(
      DataRow dataRow,
      string columnName,
      Func<object, T> convertFunction,
      T defaultValue)
    {
      return !dataRow.Table.Columns.Contains(columnName) || dataRow[columnName] == DBNull.Value ? defaultValue : convertFunction(dataRow[columnName]);
    }

    internal static MaintenancePlan DataRowToPlan(DataRow dataRow)
    {
      if (dataRow == null)
        throw new ArgumentNullException(nameof (dataRow));
      int int32 = Convert.ToInt32(dataRow["ID"]);
      string str1 = MaintenanceModePlanDAL.GetValue<string>(dataRow, "AccountID", new Func<object, string>(Convert.ToString), (string) null);
      string str2 = MaintenanceModePlanDAL.GetValue<string>(dataRow, "Name", new Func<object, string>(Convert.ToString), (string) null);
      string str3 = MaintenanceModePlanDAL.GetValue<string>(dataRow, "Reason", new Func<object, string>(Convert.ToString), (string) null);
      bool flag1 = MaintenanceModePlanDAL.GetValue<bool>(dataRow, "KeepPolling", new Func<object, bool>(Convert.ToBoolean), false);
      bool flag2 = MaintenanceModePlanDAL.GetValue<bool>(dataRow, "Favorite", new Func<object, bool>(Convert.ToBoolean), false);
      bool flag3 = MaintenanceModePlanDAL.GetValue<bool>(dataRow, "Enabled", new Func<object, bool>(Convert.ToBoolean), false);
      DateTime dateTime1 = MaintenanceModePlanDAL.GetValue<DateTime>(dataRow, "UnmanageDate", new Func<object, DateTime>(Convert.ToDateTime), DateTime.MinValue);
      DateTime dateTime2 = MaintenanceModePlanDAL.GetValue<DateTime>(dataRow, "RemanageDate", new Func<object, DateTime>(Convert.ToDateTime), DateTime.MinValue);
      MaintenancePlan maintenancePlan = new MaintenancePlan();
      maintenancePlan.set_AccountID(str1);
      maintenancePlan.set_Enabled(flag3);
      maintenancePlan.set_Favorite(flag2);
      maintenancePlan.set_ID(int32);
      maintenancePlan.set_KeepPolling(flag1);
      maintenancePlan.set_Name(str2);
      maintenancePlan.set_Reason(str3);
      maintenancePlan.set_RemanageDate(dateTime2);
      maintenancePlan.set_UnmanageDate(dateTime1);
      return maintenancePlan;
    }

    public IInformationServiceProxyCreator SwisFactory
    {
      get
      {
        if (this._SwisFactory == null)
          this._SwisFactory = (IInformationServiceProxyCreator) new InformationServiceProxyFactory();
        return this._SwisFactory;
      }
      set
      {
        this._SwisFactory = value;
      }
    }

    public string Create(MaintenancePlan plan)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.SwisFactory.Create())
      {
        Dictionary<string, object> dictionary = MaintenanceModePlanDAL.RemoveKeysFromDictionary(ObjectExtensions.ToDictionary<MaintenancePlan>((M0) plan), "ID");
        return ((IInformationServiceProxy) iinformationServiceProxy2).Create("Orion.MaintenancePlan", (IDictionary<string, object>) dictionary);
      }
    }

    public void Update(string entityUri, MaintenancePlan plan)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.SwisFactory.Create())
      {
        Dictionary<string, object> dictionary = MaintenanceModePlanDAL.RemoveKeysFromDictionary(ObjectExtensions.ToDictionary<MaintenancePlan>((M0) plan), "ID");
        ((IInformationServiceProxy) iinformationServiceProxy2).Update(entityUri, (IDictionary<string, object>) dictionary);
      }
    }

    public MaintenancePlan Get(string entityUri)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.SwisFactory.Create())
      {
        DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query("\r\n                SELECT TOP 1 ID, AccountID, Name, Reason, KeepPolling, Favorite, Enabled, UnmanageDate, RemanageDate\r\n                FROM Orion.MaintenancePlan\r\n                WHERE Uri = @EntityUri", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "EntityUri",
            (object) entityUri
          }
        });
        if (dataTable != null)
        {
          if (dataTable.Rows.Count == 1)
            return MaintenanceModePlanDAL.DataRowToPlan(dataTable.Rows.Cast<DataRow>().FirstOrDefault<DataRow>());
        }
      }
      return (MaintenancePlan) null;
    }

    public MaintenancePlan Get(int planID)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this.SwisFactory.Create())
      {
        DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query("\r\n                SELECT TOP 1 ID, AccountID, Name, Reason, KeepPolling, Favorite, Enabled, UnmanageDate, RemanageDate\r\n                FROM Orion.MaintenancePlan\r\n                WHERE ID = @PlanID", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "PlanID",
            (object) planID
          }
        });
        if (dataTable != null)
        {
          if (dataTable.Rows.Count == 1)
            return MaintenanceModePlanDAL.DataRowToPlan(dataTable.Rows.Cast<DataRow>().FirstOrDefault<DataRow>());
        }
      }
      return (MaintenancePlan) null;
    }
  }
}
