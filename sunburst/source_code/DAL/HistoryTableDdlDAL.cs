// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.HistoryTableDdlDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal static class HistoryTableDdlDAL
  {
    private static readonly Log log = new Log();

    public static void EnsureHistoryTables()
    {
      int days = DateTime.UtcNow.Subtract(DateTime.FromOADate((double) SettingsDAL.GetCurrent<double>("SWNetPerfMon-Settings-Last Archive", (M0) new DateTime(1900, 1, 1).ToOADate()))).Days;
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT ObjectName, ObjectType FROM [dbo].[HistoryTableDDL] WHERE NumberOfDaysAhead < @lastMaintDays"))
      {
        textCommand.Parameters.AddWithValue("lastMaintDays", (object) days);
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      HistoryTableDdlDAL.log.InfoFormat("Days since last maintenance: {0}; Creating {1} tables", (object) days, (object) dataTable.Rows.Count);
      foreach (DataRow row in dataTable.Rows.Cast<DataRow>())
      {
        string str1 = row.Field<string>(0);
        string str2 = row.Field<string>(1);
        HistoryTableDdlDAL.log.DebugFormat("Creating table {0}-{1}", (object) str1, (object) str2);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("Exec [dbo].[dbm_SlidePartitionedView] @objectName, @objectType, @dropOldTables"))
        {
          textCommand.Parameters.AddWithValue("objectName", (object) str1);
          textCommand.Parameters.AddWithValue("objectType", (object) str2);
          textCommand.Parameters.AddWithValue("dropOldTables", (object) false);
          SqlHelper.ExecuteNonQuery(textCommand);
        }
      }
    }
  }
}
