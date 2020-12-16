// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.AlertIncidentCache
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models.Alerts;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class AlertIncidentCache
  {
    private static readonly Log log = new Log();
    private static bool? isIncidentIntegrationAvailable = new bool?();
    internal Dictionary<int, List<AlertIncidentCache.IncidentInfo>> incidentInfoByAlertObjectId = new Dictionary<int, List<AlertIncidentCache.IncidentInfo>>();
    internal const string AlertUrlFormat = "/Orion/View.aspx?NetObject=AAT:{0}";

    private AlertIncidentCache()
    {
    }

    public static AlertIncidentCache Build(
      IInformationServiceProxy2 swisProxy,
      int? alertObjectId = null,
      bool detectIntegration = false)
    {
      if (swisProxy == null)
        throw new ArgumentNullException(nameof (swisProxy));
      AlertIncidentCache alertIncidentCache = new AlertIncidentCache();
      try
      {
        if (!AlertIncidentCache.isIncidentIntegrationAvailable.HasValue | detectIntegration)
        {
          DataTable dataTable = ((IInformationServiceProxy) swisProxy).Query("\r\nSELECT COUNT(EntityName) AS Cnt\r\nFROM Metadata.EntityMetadata\r\nWHERE EntityName = 'Orion.ESI.AlertIncident'");
          if (dataTable == null || dataTable.Rows.Count == 0 || Convert.ToUInt32(dataTable.Rows[0][0]) == 0U)
          {
            AlertIncidentCache.log.Debug((object) "Incident integration not found");
            AlertIncidentCache.isIncidentIntegrationAvailable = new bool?(false);
          }
          else
          {
            AlertIncidentCache.log.Debug((object) "Incident integration found");
            AlertIncidentCache.isIncidentIntegrationAvailable = new bool?(true);
          }
        }
        if (!AlertIncidentCache.isIncidentIntegrationAvailable.Value)
          return alertIncidentCache;
        DataTable dataTable1;
        if (alertObjectId.HasValue)
        {
          string str = string.Format("\r\nSELECT AlertObjectID, IncidentNumber, IncidentUrl, AssignedTo\r\nFROM Orion.ESI.AlertIncident\r\nWHERE AlertTriggerState > 0 {0}", (object) "AND AlertObjectID = @aoId");
          dataTable1 = ((IInformationServiceProxy) swisProxy).Query(str, (IDictionary<string, object>) new Dictionary<string, object>()
          {
            {
              "aoId",
              (object) alertObjectId.Value
            }
          });
        }
        else
        {
          string str = string.Format("\r\nSELECT AlertObjectID, IncidentNumber, IncidentUrl, AssignedTo\r\nFROM Orion.ESI.AlertIncident\r\nWHERE AlertTriggerState > 0 {0}", (object) string.Empty);
          dataTable1 = ((IInformationServiceProxy) swisProxy).Query(str);
        }
        foreach (DataRow row in (InternalDataCollectionBase) dataTable1.Rows)
        {
          int key = (int) row[0];
          AlertIncidentCache.IncidentInfo incidentInfo = new AlertIncidentCache.IncidentInfo()
          {
            Number = AlertIncidentCache.Get<string>(row, 1) ?? string.Empty,
            Url = AlertIncidentCache.Get<string>(row, 2) ?? string.Empty,
            AssignedTo = AlertIncidentCache.Get<string>(row, 3) ?? string.Empty
          };
          List<AlertIncidentCache.IncidentInfo> incidentInfoList;
          if (!alertIncidentCache.incidentInfoByAlertObjectId.TryGetValue(key, out incidentInfoList))
            alertIncidentCache.incidentInfoByAlertObjectId[key] = incidentInfoList = new List<AlertIncidentCache.IncidentInfo>();
          incidentInfoList.Add(incidentInfo);
        }
      }
      catch (Exception ex)
      {
        AlertIncidentCache.log.Error((object) ex);
      }
      return alertIncidentCache;
    }

    public void FillIncidentInfo(ActiveAlert activeAlert)
    {
      if (activeAlert == null)
        throw new ArgumentNullException(nameof (activeAlert));
      List<AlertIncidentCache.IncidentInfo> source;
      if (!this.incidentInfoByAlertObjectId.TryGetValue(activeAlert.get_Id(), out source) || source.Count == 0)
        return;
      if (source.Count == 1)
      {
        activeAlert.set_IncidentNumber(source[0].Number);
        activeAlert.set_IncidentUrl(source[0].Url);
        activeAlert.set_AssignedTo(source[0].AssignedTo);
      }
      else
      {
        activeAlert.set_IncidentNumber(string.Format((IFormatProvider) CultureInfo.InvariantCulture, Resources2.get_ActiveAlertsGrid_IncidentsClomun_ValueFormat(), (object) source.Count));
        activeAlert.set_IncidentUrl(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "/Orion/View.aspx?NetObject=AAT:{0}", (object) activeAlert.get_Id()));
        List<string> list = source.Select<AlertIncidentCache.IncidentInfo, string>((Func<AlertIncidentCache.IncidentInfo, string>) (i => i.AssignedTo)).Where<string>((Func<string, bool>) (u => !string.IsNullOrEmpty(u))).Distinct<string>().ToList<string>();
        if (list.Count == 1)
          activeAlert.set_AssignedTo(list.First<string>());
        else
          activeAlert.set_AssignedTo(Resources2.get_ActiveAlertsGrid_IncidentAssignee_MultiUser());
      }
    }

    private static T Get<T>(DataRow row, int colIndex)
    {
      return row[colIndex] != DBNull.Value ? (T) row[colIndex] : default (T);
    }

    internal class IncidentInfo
    {
      public string Number { get; set; }

      public string Url { get; set; }

      public string AssignedTo { get; set; }
    }
  }
}
