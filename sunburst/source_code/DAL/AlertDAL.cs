// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.AlertDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Contract2;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Alerting.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Alerting;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Models.Alerts;
using SolarWinds.Orion.Core.Common.PackageManager;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.Core.Models.Alerting;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class AlertDAL
  {
    private static readonly Regex _ackRegex = new Regex("AlertStatus.Acknowledged=[0-1]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    internal static Func<bool> IsInterfacesAllowed = (Func<bool>) (() => ((IPackageManager) SolarWinds.Orion.Core.Common.PackageManager.PackageManager.InstanceWithCache).IsPackageInstalled("Orion.Interfaces"));
    private static readonly Log Log = new Log();
    private static bool _enableLimitationReplacement = BusinessLayerSettings.Instance.EnableLimitationReplacement;
    private static int _limitationSqlExaggeration = BusinessLayerSettings.Instance.LimitationSqlExaggeration;
    internal static IInformationServiceProxyCreator SwisCreator = (IInformationServiceProxyCreator) new SwisConnectionProxyCreator((Func<SwisConnectionProxy>) (() => new SwisConnectionProxyFactory().CreateConnection()));
    internal static IInformationServiceProxy2 SwisProxy = AlertDAL.SwisCreator.Create();
    private const int CLRWhereClausesLimit = 100;

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    public static extern int CLSIDFromString(string sz, out Guid clsid);

    public static bool TryStrToGuid(string s, out Guid value)
    {
      switch (s)
      {
        case "":
        case null:
          value = Guid.Empty;
          return false;
        default:
          s = string.Format("{{{0}}}", (object) s);
          if (AlertDAL.CLSIDFromString(s, out value) >= 0)
            return true;
          value = Guid.Empty;
          return false;
      }
    }

    public static List<KeyValuePair<string, string>> GetAlertList()
    {
      Dictionary<string, string> data = new Dictionary<string, string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertID, Name FROM AlertConfigurations WITH(NOLOCK)"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            data.Add("AA-" + (object) DatabaseFunctions.GetInt32(dataReader, "AlertID"), DatabaseFunctions.GetString(dataReader, "Name"));
        }
      }
      return AlertDAL.SortList(data);
    }

    [Obsolete("Old alerting will be removed. Use GetAlertList() method instead.")]
    public static List<KeyValuePair<string, string>> GetAlertList(bool includeBasic)
    {
      Dictionary<string, string> data = new Dictionary<string, string>();
      if (includeBasic)
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nSelect al.AlertID, a.AlertName\r\nFrom ActiveAlerts al\r\nINNER JOIN Alerts a WITH(NOLOCK) ON al.AlertID = a.AlertID\r\nGroup By al.AlertID, a.AlertName\r\nOrder By AlertName \r\n"))
        {
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
          {
            while (dataReader.Read())
              data.Add(DatabaseFunctions.GetInt32(dataReader, "AlertID").ToString(), DatabaseFunctions.GetString(dataReader, "AlertName"));
          }
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertDefID, AlertName FROM AlertDefinitions WITH(NOLOCK)\r\n                 Where AlertDefID NOT IN (SELECT AlertRefID As AlertDefID FROM AlertConfigurations)\r\n             ORDER BY AlertName"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            data.Add(DatabaseFunctions.GetGuid(dataReader, "AlertDefID").ToString(), DatabaseFunctions.GetString(dataReader, "AlertName"));
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertID, Name FROM AlertConfigurations WITH(NOLOCK)"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            data.Add("AA-" + (object) DatabaseFunctions.GetInt32(dataReader, "AlertID"), DatabaseFunctions.GetString(dataReader, "Name"));
        }
      }
      return AlertDAL.SortList(data);
    }

    private static List<KeyValuePair<string, string>> SortList(
      Dictionary<string, string> data)
    {
      List<KeyValuePair<string, string>> keyValuePairList = new List<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) data);
      keyValuePairList.Sort((Comparison<KeyValuePair<string, string>>) ((first, second) => first.Value.CompareTo(second.Value)));
      return keyValuePairList;
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static DataTable GetAlertTable(
      string netObject,
      string deviceType,
      string alertID,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs)
    {
      return AlertDAL.GetAlertTable(netObject, deviceType, alertID, maxRecords, showAcknowledged, limitationIDs, true);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static DataTable GetAlertTable(
      string netObject,
      string deviceType,
      string alertID,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs,
      bool includeBasic)
    {
      return AlertDAL.GetSortableAlertTable(netObject, deviceType, alertID, " ObjectName, AlertName ", maxRecords, showAcknowledged, limitationIDs, includeBasic);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static DataTable GetSortableAlertTable(
      string netObject,
      string deviceType,
      string alertID,
      string orderByClause,
      int maxRecords,
      bool showAcknowledged,
      List<int> limitationIDs,
      bool includeBasic)
    {
      string str1 = string.Empty;
      List<string> stringList = new List<string>();
      int result = 0;
      bool flag = false;
      string str2 = string.Empty;
      string str3 = string.Empty;
      Regex regex = new Regex("^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", RegexOptions.Compiled);
      string messageTypeString1 = OrionMessagesHelper.GetMessageTypeString((OrionMessageType) 0);
      string messageTypeString2 = OrionMessagesHelper.GetMessageTypeString((OrionMessageType) 1);
      if (alertID.Equals(messageTypeString1, StringComparison.OrdinalIgnoreCase) || alertID.Equals(messageTypeString2, StringComparison.OrdinalIgnoreCase))
      {
        str2 = alertID;
        alertID = string.Empty;
      }
      if (!string.IsNullOrEmpty(netObject))
      {
        int length = netObject.IndexOf(':', 0);
        if (length != 0)
        {
          string s = netObject.Substring(length + 1);
          if (!int.TryParse(s, out result))
          {
            flag = true;
            str3 = s;
          }
          else
            result = Convert.ToInt32(s);
          str1 = netObject.Substring(0, length);
          using (List<NetObjectType>.Enumerator enumerator = ((List<NetObjectType>) ModuleAlertsMap.get_NetObjectTypes().Items).GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              NetObjectType current = enumerator.Current;
              if (current.get_Prefix().ToUpper() == str1.ToUpper())
                stringList.Add(current.get_Name());
            }
          }
        }
      }
      StringBuilder stringBuilder1 = new StringBuilder(" AND (AlertStatus.State=2 OR AlertStatus.State=3) ");
      StringBuilder stringBuilder2 = new StringBuilder();
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(""))
      {
        if (!showAcknowledged)
          stringBuilder1.Append(" AND AlertStatus.Acknowledged=0 ");
        if (!string.IsNullOrEmpty(netObject) && (uint) result > 0U | flag && (!string.IsNullOrEmpty(str1) && stringList.Count > 0))
        {
          if (str1.Equals("N", StringComparison.OrdinalIgnoreCase) && result != 0)
          {
            stringBuilder1.AppendFormat(" AND Nodes.NodeID={0} ", (object) result);
          }
          else
          {
            StringBuilder stringBuilder3 = new StringBuilder();
            string str4 = string.Empty;
            foreach (string str5 in stringList)
            {
              stringBuilder3.AppendFormat(" {1} AlertStatus.ObjectType='{0}' ", (object) str5, (object) str4);
              str4 = "OR";
            }
            stringBuilder2.AppendFormat(" AND (({0}) AND AlertStatus.ActiveObject=", (object) stringBuilder3);
            if (flag)
              stringBuilder2.AppendFormat("'{0}') ", (object) str3);
            else
              stringBuilder2.AppendFormat("{0}) ", (object) result);
          }
        }
        else if (!string.IsNullOrEmpty(deviceType))
        {
          stringBuilder1.Append(" AND (Nodes.MachineType Like @machineType) ");
          textCommand.Parameters.AddWithValue("@machineType", (object) deviceType);
        }
        if (regex.IsMatch(alertID))
        {
          stringBuilder1.Append(" AND (AlertStatus.AlertDefID=@alertDefID) ");
          textCommand.Parameters.AddWithValue("@alertDefID", (object) alertID);
        }
        else if (!string.IsNullOrEmpty(alertID))
          stringBuilder1.AppendFormat(" AND (AlertStatus.AlertDefID='{0}') ", (object) Guid.Empty);
        string str6 = "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes\r\nSELECT Nodes.* INTO #Nodes FROM Nodes WHERE 1=1;";
        string str7 = Limitation.LimitSQL(str6, (IEnumerable<int>) limitationIDs);
        int num = !AlertDAL._enableLimitationReplacement ? 0 : (str7.Length / str6.Length > AlertDAL._limitationSqlExaggeration ? 1 : 0);
        string str8 = num != 0 ? str7 : string.Empty;
        string str9 = num != 0 ? "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes" : string.Empty;
        if (((str2.Equals(messageTypeString1, StringComparison.OrdinalIgnoreCase) ? 1 : (!includeBasic ? 1 : 0)) | (flag ? 1 : 0)) != 0)
          textCommand.CommandText = string.Format("{3}SELECT TOP {0} a.*, WebCommunityStrings.[GUID] AS [GUID] FROM ( {1} )a LEFT OUTER JOIN WebCommunityStrings WITH(NOLOCK) ON WebCommunityStrings.CommunityString = a.Community Order By {2} \r\n{4}", (object) maxRecords, (object) AlertDAL.GetAdvAlertSwql(stringBuilder1.ToString(), stringBuilder2.ToString(), netObject, messageTypeString1, limitationIDs, true, true), (object) orderByClause, (object) str8, (object) str9);
        else if (str2.Equals(messageTypeString2, StringComparison.OrdinalIgnoreCase))
        {
          textCommand.CommandText = string.Format("{3}SELECT TOP {0} a.*, WebCommunityStrings.[GUID] AS [GUID] FROM ( {1} )a LEFT OUTER JOIN WebCommunityStrings WITH(NOLOCK) ON WebCommunityStrings.CommunityString = a.Community Order By {2} \r\n{4}", (object) maxRecords, (object) AlertDAL.GetBasicAlertSwql(netObject, deviceType, alertID, limitationIDs, true, true), (object) orderByClause, (object) str8, (object) str9);
        }
        else
        {
          string advAlertSwql = AlertDAL.GetAdvAlertSwql(stringBuilder1.ToString(), stringBuilder2.ToString(), netObject, messageTypeString1, limitationIDs, true, true);
          string basicAlertSwql = AlertDAL.GetBasicAlertSwql(netObject, deviceType, alertID, limitationIDs, true, true);
          string empty = string.Empty;
          string str4 = !string.IsNullOrEmpty(advAlertSwql) ? string.Format("( {0}  Union ( {1} ))", (object) advAlertSwql, (object) basicAlertSwql) : string.Format("({0})", (object) basicAlertSwql);
          textCommand.CommandText = string.Format("{3}SELECT TOP {0} a.*, WebCommunityStrings.[GUID] AS [GUID] FROM ({1})a LEFT OUTER JOIN WebCommunityStrings WITH(NOLOCK) ON WebCommunityStrings.CommunityString = a.Community Order By {2} \r\n{4}", (object) maxRecords, (object) str4, (object) orderByClause, (object) str8, (object) str9);
        }
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      dataTable.TableName = "History";
      return dataTable;
    }

    private static DataTable GetAvailableObjectTypes(bool federationEnabled = false)
    {
      string str = "SELECT EntityType, Name, Prefix, KeyProperty, KeyPropertyIndex FROM Orion.NetObjectTypes (nolock=true)";
      return InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str, federationEnabled);
    }

    private static Dictionary<string, int> GetStatusesForSwisEntities(
      string entityName,
      string entityIdName,
      IEnumerable<string> entityIds,
      bool federationEnabled = false)
    {
      Dictionary<string, int> dictionary = new Dictionary<string, int>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      if (entityIds.Count<string>() > 0)
      {
        string format = "SELECT Status, {1} AS Id FROM {0} (nolock=true) WHERE {1} IN ({2})";
        string str1 = string.Join(",", entityIds);
        string str2 = string.Format(format, (object) entityName, (object) entityIdName, (object) str1);
        foreach (DataRow row in (InternalDataCollectionBase) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str2, federationEnabled).Rows)
        {
          string key = row["Id"] != DBNull.Value ? Convert.ToString(row["id"]) : string.Empty;
          int num = row["Status"] != DBNull.Value ? Convert.ToInt32(row["Status"]) : 0;
          if (!string.IsNullOrEmpty(key) && !dictionary.ContainsKey(key))
            dictionary.Add(key, num);
        }
      }
      return dictionary;
    }

    private static string GetAlertNote(string alertDefID, string activeObject, string objectType)
    {
      string str1 = string.Empty;
      if (new Regex("^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", RegexOptions.Compiled).IsMatch(alertDefID))
      {
        string str2 = "SELECT Notes FROM Orion.AlertStatus (nolock=true) WHERE AlertDefID=@alertDefID AND ActiveObject=@activeObject AND ObjectType=@objectType";
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str2, new Dictionary<string, object>()
        {
          {
            nameof (alertDefID),
            (object) alertDefID
          },
          {
            nameof (activeObject),
            (object) activeObject
          },
          {
            nameof (objectType),
            (object) objectType
          }
        });
        if (dataTable.Rows.Count > 0)
          str1 = dataTable.Rows[0]["Notes"] != DBNull.Value ? Convert.ToString(dataTable.Rows[0]["Notes"]) : string.Empty;
      }
      else
      {
        string str2 = "SELECT AlertID, ObjectID AS ActiveObject, ObjectType, AlertNotes FROM Orion.ActiveAlerts (nolock=true)" + " WHERE AlertID=@alertID AND ObjectID=@objectID AND ObjectType=@objectType ";
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str2, new Dictionary<string, object>()
        {
          {
            "alertID",
            (object) alertDefID
          },
          {
            "objectID",
            (object) activeObject
          },
          {
            nameof (objectType),
            (object) objectType
          }
        });
        if (dataTable.Rows.Count > 0)
          str1 = dataTable.Rows[0]["AlertNotes"] != DBNull.Value ? Convert.ToString(dataTable.Rows[0]["AlertNotes"]) : string.Empty;
      }
      return str1;
    }

    public static Dictionary<string, string> GetNotesForActiveAlerts(
      IEnumerable<ActiveAlert> activeAlerts)
    {
      Dictionary<string, string> res = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      string strCondition = string.Empty;
      StringBuilder stringBuilder = new StringBuilder();
      bool flag1 = true;
      string sqlQuery = string.Empty;
      Action<SqlParameter[]> action1 = (Action<SqlParameter[]>) (parameters =>
      {
        if (string.IsNullOrEmpty(strCondition))
          return;
        sqlQuery = string.Format("SELECT AlertDefID, ActiveObject, ObjectType, Notes FROM AlertStatus WITH(NOLOCK) WHERE {0}", (object) strCondition);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(sqlQuery))
        {
          if (parameters != null)
            textCommand.Parameters.AddRange(parameters);
          foreach (DataRow row in (InternalDataCollectionBase) SqlHelper.ExecuteDataTable(textCommand).Rows)
          {
            string str1 = row["AlertDefID"] != DBNull.Value ? Convert.ToString(row["AlertDefID"]) : string.Empty;
            string str2 = row["ActiveObject"] != DBNull.Value ? Convert.ToString(row["ActiveObject"]) : string.Empty;
            string str3 = row["ObjectType"] != DBNull.Value ? Convert.ToString(row["ObjectType"]) : string.Empty;
            string str4 = row["Notes"] != DBNull.Value ? Convert.ToString(row["Notes"]) : string.Empty;
            string key = string.Format("{0}|{1}|{2}", (object) str1, (object) str2, (object) str3);
            if (!res.ContainsKey(key))
              res.Add(key, str4);
          }
        }
      });
      Action<SqlParameter[]> action2 = (Action<SqlParameter[]>) (parameters =>
      {
        if (string.IsNullOrEmpty(strCondition))
          return;
        sqlQuery = string.Format("SELECT AlertID, ObjectID AS ActiveObject, ObjectType, AlertNotes FROM ActiveAlerts WITH(NOLOCK) WHERE {0}", (object) strCondition);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(sqlQuery))
        {
          if (parameters != null)
            textCommand.Parameters.AddRange(parameters);
          foreach (DataRow row in (InternalDataCollectionBase) SqlHelper.ExecuteDataTable(textCommand).Rows)
          {
            string str1 = row["AlertID"] != DBNull.Value ? Convert.ToString(row["AlertID"]) : string.Empty;
            string str2 = row["ActiveObject"] != DBNull.Value ? Convert.ToString(row["ActiveObject"]) : string.Empty;
            string str3 = row["ObjectType"] != DBNull.Value ? Convert.ToString(row["ObjectType"]) : string.Empty;
            string str4 = row["AlertNotes"] != DBNull.Value ? Convert.ToString(row["AlertNotes"]) : string.Empty;
            string key = string.Format("{0}|{1}|{2}", (object) str1, (object) str2, (object) str3);
            if (!res.ContainsKey(key))
              res.Add(key, str4);
          }
        }
      });
      IEnumerable<ActiveAlert> activeAlerts1 = activeAlerts.Where<ActiveAlert>((Func<ActiveAlert, bool>) (item => item.get_AlertType() == 0));
      int num1 = 0;
      List<SqlParameter> sqlParameterList = new List<SqlParameter>();
      using (IEnumerator<ActiveAlert> enumerator = activeAlerts1.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          ActiveAlert current = enumerator.Current;
          if (!flag1)
            stringBuilder.Append(" OR ");
          stringBuilder.AppendFormat("(AlertDefID=@alertDefID{0} AND ActiveObject=@activeObject{0} AND ObjectType=@objectType{0})", (object) num1);
          sqlParameterList.Add(new SqlParameter(string.Format("@alertDefID{0}", (object) num1), (object) current.get_AlertDefId()));
          sqlParameterList.Add(new SqlParameter(string.Format("@activeObject{0}", (object) num1), (object) current.get_ActiveNetObject()));
          sqlParameterList.Add(new SqlParameter(string.Format("@objectType{0}", (object) num1), (object) current.get_ObjectType()));
          flag1 = false;
          ++num1;
          if (num1 % 520 == 0)
          {
            strCondition = stringBuilder.ToString();
            stringBuilder.Clear();
            action1(sqlParameterList.ToArray());
            sqlParameterList.Clear();
            num1 = 0;
            strCondition = string.Empty;
            flag1 = true;
          }
        }
      }
      strCondition = stringBuilder.ToString();
      if (!string.IsNullOrEmpty(strCondition))
        action1(sqlParameterList.ToArray());
      stringBuilder.Clear();
      int num2 = 0;
      strCondition = string.Empty;
      bool flag2 = true;
      sqlParameterList.Clear();
      using (IEnumerator<ActiveAlert> enumerator = activeAlerts.Where<ActiveAlert>((Func<ActiveAlert, bool>) (item => item.get_AlertType() == 1)).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          ActiveAlert current = enumerator.Current;
          if (!flag2)
            stringBuilder.Append(" OR ");
          stringBuilder.AppendFormat("(AlertID=@alertID{0} AND ObjectID=@objectID{0} AND ObjectType=@objectType{0})", (object) num2);
          sqlParameterList.Add(new SqlParameter(string.Format("@alertID{0}", (object) num2), (object) current.get_AlertDefId()));
          sqlParameterList.Add(new SqlParameter(string.Format("@objectID{0}", (object) num2), (object) current.get_ActiveNetObject()));
          sqlParameterList.Add(new SqlParameter(string.Format("@objectType{0}", (object) num2), (object) current.get_ObjectType()));
          flag2 = false;
          ++num2;
          if (num2 % 520 == 0)
          {
            strCondition = stringBuilder.ToString();
            stringBuilder.Clear();
            action2(sqlParameterList.ToArray());
            sqlParameterList.Clear();
            num2 = 0;
            strCondition = string.Empty;
          }
        }
      }
      strCondition = stringBuilder.ToString();
      action2(sqlParameterList.ToArray());
      return res;
    }

    private static Dictionary<string, int> GetTriggerCountForActiveAlerts(
      IEnumerable<ActiveAlert> activeAlerts)
    {
      Dictionary<string, int> res = new Dictionary<string, int>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      string strCondition = string.Empty;
      bool flag = true;
      StringBuilder stringBuilder = new StringBuilder();
      ActiveAlert[] array = activeAlerts.Where<ActiveAlert>((Func<ActiveAlert, bool>) (item => item.get_AlertType() == 0)).ToArray<ActiveAlert>();
      int num1 = 0;
      Action<SqlParameter[]> action = (Action<SqlParameter[]>) (parameters =>
      {
        if (string.IsNullOrEmpty(strCondition))
          return;
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT AlertDefID, ActiveObject, ObjectType, TriggerCount FROM AlertStatus WITH(NOLOCK) WHERE {0}", (object) strCondition)))
        {
          if (parameters != null)
            textCommand.Parameters.AddRange(parameters);
          foreach (DataRow row in (InternalDataCollectionBase) SqlHelper.ExecuteDataTable(textCommand).Rows)
          {
            string str1 = row["AlertDefID"] != DBNull.Value ? Convert.ToString(row["AlertDefID"]) : string.Empty;
            string str2 = row["ActiveObject"] != DBNull.Value ? Convert.ToString(row["ActiveObject"]) : string.Empty;
            string str3 = row["ObjectType"] != DBNull.Value ? Convert.ToString(row["ObjectType"]) : string.Empty;
            int num2 = row["TriggerCount"] != DBNull.Value ? Convert.ToInt32(row["TriggerCount"]) : 0;
            string key = string.Format("{0}|{1}|{2}", (object) str1, (object) str2, (object) str3);
            if (!res.ContainsKey(key))
              res.Add(key, num2);
          }
        }
      });
      List<SqlParameter> sqlParameterList = new List<SqlParameter>();
      foreach (ActiveAlert activeAlert in array)
      {
        if (!flag)
          stringBuilder.Append(" OR ");
        stringBuilder.AppendFormat("(AlertDefID=@alertDefID{0} AND ActiveObject=@activeObject{0} AND ObjectType=@objectType{0})", (object) num1);
        sqlParameterList.Add(new SqlParameter(string.Format("@alertDefID{0}", (object) num1), (object) activeAlert.get_AlertDefId()));
        sqlParameterList.Add(new SqlParameter(string.Format("@activeObject{0}", (object) num1), (object) activeAlert.get_ActiveNetObject()));
        sqlParameterList.Add(new SqlParameter(string.Format("@objectType{0}", (object) num1), (object) activeAlert.get_ObjectType()));
        ++num1;
        flag = false;
        if (num1 % 520 == 0)
        {
          strCondition = stringBuilder.ToString();
          stringBuilder.Clear();
          action(sqlParameterList.ToArray());
          sqlParameterList.Clear();
          num1 = 0;
          strCondition = string.Empty;
          flag = true;
        }
      }
      action(sqlParameterList.ToArray());
      using (IEnumerator<ActiveAlert> enumerator = activeAlerts.Where<ActiveAlert>((Func<ActiveAlert, bool>) (item => item.get_AlertType() == 1)).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          ActiveAlert current = enumerator.Current;
          string key = string.Format("{0}|{1}|{2}", (object) current.get_AlertDefId(), (object) current.get_ActiveNetObject(), (object) current.get_ObjectType());
          if (!res.ContainsKey(key))
            res.Add(key, 1);
        }
      }
      return res;
    }

    private static Dictionary<string, string> GetFullUserNames(
      IEnumerable<string> accountIDs,
      bool federationEnabled = false)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      if (accountIDs.Any<string>())
      {
        List<string> list = accountIDs.Distinct<string>().ToList<string>();
        string empty = string.Empty;
        foreach (string str in list)
        {
          if (!string.IsNullOrEmpty(empty))
            empty += " OR ";
          empty += string.Format("AccountID='{0}'", (object) str);
        }
        string str1 = "SELECT AccountID, DisplayName FROM Orion.Accounts (nolock=true) WHERE " + empty;
        foreach (DataRow row in (InternalDataCollectionBase) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str1, federationEnabled).Rows)
        {
          string key = row["AccountID"] != DBNull.Value ? Convert.ToString(row["AccountID"]) : string.Empty;
          string str2 = row["DisplayName"] != DBNull.Value ? Convert.ToString(row["DisplayName"]) : string.Empty;
          if (!string.IsNullOrEmpty(key) && !dictionary.ContainsKey(key))
            dictionary.Add(key, str2);
        }
      }
      return dictionary;
    }

    private static string GetFilterFromTriggeringObjectEntityNamesUrisAndGlobalAlertIdsAndInstanceSiteIds(
      string[] triggeringObjectEntityNames,
      string[] triggeringObjectEntityUris,
      IEnumerable<int> alertActiveIdsGlobal,
      IEnumerable<int> instanceSiteIds)
    {
      string str = string.Empty;
      if (triggeringObjectEntityUris != null && ((IEnumerable<string>) triggeringObjectEntityUris).Any<string>())
      {
        foreach (string triggeringObjectEntityUri in triggeringObjectEntityUris)
        {
          if (!string.IsNullOrEmpty(str))
            str += " OR ";
          str = str + "TriggeringObjectEntityUri = '" + triggeringObjectEntityUri + "' OR TriggeringObjectEntityUri LIKE '" + triggeringObjectEntityUri + "/%'";
        }
        if (((IEnumerable<string>) triggeringObjectEntityNames).Any<string>() && string.Compare(triggeringObjectEntityNames[0], "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0 && (alertActiveIdsGlobal != null && alertActiveIdsGlobal.Any<int>()))
          str = str + " OR " + "AlertObjectID IN (" + string.Join<int>(",", alertActiveIdsGlobal) + ")";
      }
      else if (triggeringObjectEntityNames != null && ((IEnumerable<string>) triggeringObjectEntityNames).Any<string>())
        str = "TriggeringObjectEntityName IN (" + string.Join(",", ((IEnumerable<string>) triggeringObjectEntityNames).Select<string, string>((Func<string, string>) (item => "'" + item + "'"))) + ")";
      if (instanceSiteIds != null && instanceSiteIds.Any<int>())
      {
        if (!string.IsNullOrEmpty(str))
          str = "(" + str + ") AND ";
        str = str + "SiteID in (" + string.Join<int>(",", instanceSiteIds) + ")";
      }
      return str;
    }

    internal static void GetActiveAlertsPagingAndLoadingParameters(
      PageableActiveAlertRequest pageableRequest,
      ActiveAlertsRequest request,
      out bool showAcknowledgedAlerts,
      out bool showOnlyAcknowledgedAlerts,
      out string filterStatement,
      out string orderByClause,
      out int fromRow,
      out int toRow)
    {
      showAcknowledgedAlerts = true;
      showOnlyAcknowledgedAlerts = false;
      filterStatement = string.Empty;
      orderByClause = string.Empty;
      fromRow = 1;
      toRow = int.MaxValue;
      List<string> source = new List<string>()
      {
        "Severity",
        "SeverityOrder",
        "AlertMessage",
        "AlertName",
        "TriggeringObjectEntityName",
        "LastTriggeredDateTime",
        "RelatedNodeEntityUri",
        "AcknowledgedBy",
        "Acknowledged",
        "SiteID",
        "Category",
        "AlertMessage",
        "Notes",
        "TriggerCount",
        "RelatedNode",
        "ActiveTimeDisplay",
        "ActiveTimeSort",
        "AcknowledgeTime",
        "TriggerTime"
      };
      List<string> allowedFilterByPropertyNameForQuickLoad = new List<string>()
      {
        "Severity",
        "AlertName",
        "AlertMessage",
        "Notes",
        "AcknowledgedBy",
        "TriggeringObjectEntityName",
        "Category",
        "ObjectTriggeredThisAlertDisplayName",
        "TriggeringObjectEntityName",
        "RelatedNodeEntityUri",
        "AlertDefId",
        "RelatedNode"
      };
      if (!string.IsNullOrEmpty(pageableRequest.get_OrderByClause()))
      {
        string columToOrder = pageableRequest.get_OrderByClause().Trim().Split(' ')[0];
        columToOrder = columToOrder.TrimStart('[').TrimEnd(']');
        if (source.Any<string>((Func<string, bool>) (item => item.Equals(columToOrder, StringComparison.OrdinalIgnoreCase))))
          orderByClause = pageableRequest.get_OrderByClause();
      }
      if (string.IsNullOrEmpty(pageableRequest.get_OrderByClause()) || !string.IsNullOrEmpty(orderByClause))
      {
        string searchValue = pageableRequest.get_SearchValue();
        string str;
        if (searchValue == null)
          str = (string) null;
        else
          str = searchValue.Trim('%');
        if (string.IsNullOrEmpty(str) && (!pageableRequest.get_SecondaryFilters().Any<ActiveAlertFilter>() || pageableRequest.get_SecondaryFilters().Any<ActiveAlertFilter>((Func<ActiveAlertFilter, bool>) (secondaryFilter => allowedFilterByPropertyNameForQuickLoad.Any<string>((Func<string, bool>) (allowedFilterPropertyName => secondaryFilter.get_FieldName().Equals(allowedFilterPropertyName, StringComparison.OrdinalIgnoreCase)))))) && (string.IsNullOrEmpty(pageableRequest.get_FilterByPropertyName()) || allowedFilterByPropertyNameForQuickLoad.Contains(pageableRequest.get_FilterByPropertyName()) || pageableRequest.get_FilterByPropertyName().StartsWith("CP_", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(pageableRequest.get_FilterByPropertyValue()) && pageableRequest.get_FilterByPropertyValue().Equals("[All]", StringComparison.OrdinalIgnoreCase)))
        {
          bool flag = true;
          if (request != null)
          {
            int num = 0;
            if (request.get_TriggeringObjectEntityUris() != null && ((IEnumerable<string>) request.get_TriggeringObjectEntityUris()).Any<string>())
              num += request.get_TriggeringObjectEntityUris().Length;
            else if (request.get_TriggeringObjectEntityNames() != null && ((IEnumerable<string>) request.get_TriggeringObjectEntityNames()).Any<string>())
              num += request.get_TriggeringObjectEntityNames().Length;
            if (request.get_TriggeringObjectEntityNames() != null && ((IEnumerable<string>) request.get_TriggeringObjectEntityNames()).Any<string>() && (string.Compare(request.get_TriggeringObjectEntityNames()[0], "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0 && request.get_AlertActiveIdsGlobal() != null) && request.get_AlertActiveIdsGlobal().Any<int>())
              num += request.get_AlertActiveIdsGlobal().Count<int>();
            if (request.get_InstanceSiteIds() != null && request.get_InstanceSiteIds().Any<int>())
              num += request.get_InstanceSiteIds().Count<int>();
            flag = num < 1000;
          }
          if (flag)
          {
            if (!string.IsNullOrEmpty(pageableRequest.get_FilterByPropertyName()))
            {
              string filterByPropertyName = pageableRequest.get_FilterByPropertyName();
              pageableRequest.set_FilterByPropertyName(pageableRequest.get_FilterByPropertyName().Replace("CP_", string.Empty));
              filterStatement = AlertDAL.GetActiveAlertsFilterCondition(pageableRequest);
              pageableRequest.set_FilterByPropertyName(filterByPropertyName);
            }
            else if (!string.IsNullOrEmpty(pageableRequest.get_FilterStatement()))
              filterStatement = pageableRequest.get_FilterStatement();
            string filterCondition = AlertDAL.GetFilterCondition(pageableRequest.get_SecondaryFilters(), string.Empty, pageableRequest.get_SecondaryFilterOperator());
            if (!string.IsNullOrEmpty(filterStatement) && !string.IsNullOrEmpty(filterCondition))
              filterStatement = filterStatement + " AND (" + filterCondition + ")";
            else if (!string.IsNullOrEmpty(filterCondition))
              filterStatement = filterCondition;
            if (request != null)
            {
              string andInstanceSiteIds = AlertDAL.GetFilterFromTriggeringObjectEntityNamesUrisAndGlobalAlertIdsAndInstanceSiteIds(request.get_TriggeringObjectEntityNames(), request.get_TriggeringObjectEntityUris(), request.get_AlertActiveIdsGlobal(), request.get_InstanceSiteIds());
              if (!string.IsNullOrEmpty(filterStatement) && !string.IsNullOrEmpty(andInstanceSiteIds))
                filterStatement = filterStatement + " AND (" + andInstanceSiteIds + ")";
              else if (!string.IsNullOrEmpty(andInstanceSiteIds))
                filterStatement = andInstanceSiteIds;
              showAcknowledgedAlerts = request.get_ShowAcknowledgedAlerts();
              showOnlyAcknowledgedAlerts = request.get_ShowOnlyAcknowledgedAlerts();
            }
            fromRow = pageableRequest.get_StartRow() + 1;
            toRow = pageableRequest.get_StartRow() + pageableRequest.get_PageSize();
            AlertDAL.Log.Debug((object) "Will be used quick optimized loading for PageableActiveAlerts.");
            return;
          }
          AlertDAL.Log.Debug((object) "Will be used slow not-optimized loading for PageableActiveAlerts.");
          return;
        }
      }
      AlertDAL.Log.Debug((object) "Will be used slow not-optimized loading for PageableActiveAlerts.");
    }

    public static ActiveAlertPage GetPageableActiveAlerts(
      PageableActiveAlertRequest pageableRequest,
      ActiveAlertsRequest request = null)
    {
      IEnumerable<CustomProperty> propertiesForEntity = CustomPropertyMgr.GetCustomPropertiesForEntity("Orion.AlertConfigurationsCustomProperties");
      bool showAcknowledgedAlerts;
      bool showOnlyAcknowledgedAlerts;
      string filterStatement;
      string orderByClause;
      int fromRow;
      int toRow;
      AlertDAL.GetActiveAlertsPagingAndLoadingParameters(pageableRequest, request, out showAcknowledgedAlerts, out showOnlyAcknowledgedAlerts, out filterStatement, out orderByClause, out fromRow, out toRow);
      List<ErrorMessage> errors;
      Tuple<IEnumerable<ActiveAlert>, int> activeAlerts = AlertDAL.GetActiveAlerts(pageableRequest, propertiesForEntity, request?.get_RelatedNodeEntityUri(), showAcknowledgedAlerts, showOnlyAcknowledgedAlerts, filterStatement, orderByClause, fromRow, toRow, out errors);
      DataTable table = AlertDAL.ConvertActiveAlertsToTable((IEnumerable<ActiveAlert>) activeAlerts.Item1.ToList<ActiveAlert>(), propertiesForEntity);
      DataTable filteredTable = AlertDAL.GetFilteredTable(pageableRequest, table, request);
      IEnumerable<DataRow> filteredAlertRows = AlertDAL.GetFilteredAlertRows(pageableRequest, filteredTable, propertiesForEntity);
      List<ActiveAlert> list = AlertDAL.ConvertRowsToActiveAlerts(AlertDAL.GetSortedAlerts(pageableRequest, filteredAlertRows), propertiesForEntity).ToList<ActiveAlert>();
      ActiveAlertPage activeAlertPage = new ActiveAlertPage();
      activeAlertPage.set_TotalRow(activeAlerts.Item2);
      if (fromRow == 1 && toRow == int.MaxValue)
      {
        activeAlertPage.set_TotalRow(list.Count);
        list = ((IEnumerable<ActiveAlert>) list).Skip<ActiveAlert>(pageableRequest.get_StartRow()).Take<ActiveAlert>(pageableRequest.get_PageSize()).ToList<ActiveAlert>();
      }
      activeAlertPage.set_ActiveAlerts((IEnumerable<ActiveAlert>) list);
      activeAlertPage.set_ErrorsMessages((IEnumerable<ErrorMessage>) errors);
      return activeAlertPage;
    }

    private static Tuple<IEnumerable<ActiveAlert>, int> GetActiveAlerts(
      PageableActiveAlertRequest request,
      IEnumerable<CustomProperty> customProperties,
      string relatedNodeEntityUri,
      bool showAcknowledgedAlerts,
      bool showOnlyAcknowledgedAlerts,
      string filterStatement,
      string orderByClause,
      int fromRow,
      int toRow,
      out List<ErrorMessage> errors)
    {
      return new ActiveAlertDAL().GetActiveAlerts(customProperties, request.get_LimitationIDs(), out errors, request.get_FederationEnabled(), request.get_IncludeNotTriggered(), showAcknowledgedAlerts, showOnlyAcknowledgedAlerts, relatedNodeEntityUri, filterStatement, orderByClause, fromRow, toRow);
    }

    private static DataTable ConvertActiveAlertsToTable(
      IEnumerable<ActiveAlert> alerts,
      IEnumerable<CustomProperty> customProperties)
    {
      DataTable dataTable = new DataTable();
      dataTable.Locale = CultureInfo.InvariantCulture;
      dataTable.Columns.Add("ActiveAlertID", typeof (string));
      dataTable.Columns.Add("AlertDefID", typeof (string));
      dataTable.Columns.Add("ActiveAlertType", typeof (ActiveAlertType));
      dataTable.Columns.Add("AlertName", typeof (string));
      dataTable.Columns.Add("AlertMessage", typeof (string));
      dataTable.Columns.Add("TriggeringObject", typeof (string));
      dataTable.Columns.Add("TriggeringObjectEntityName", typeof (string));
      dataTable.Columns.Add("TriggeringObjectStatus", typeof (string));
      dataTable.Columns.Add("TriggeringObjectDetailsUrl", typeof (string));
      dataTable.Columns.Add("TriggeringObjectEntityUri", typeof (string));
      dataTable.Columns.Add("RelatedNode", typeof (string));
      dataTable.Columns.Add("RelatedNodeID", typeof (int));
      dataTable.Columns.Add("RelatedNodeEntityUri", typeof (string));
      dataTable.Columns.Add("RelatedNodeDetailsUrl", typeof (string));
      dataTable.Columns.Add("RelatedNodeStatus", typeof (int));
      dataTable.Columns.Add("ActiveTime", typeof (TimeSpan));
      dataTable.Columns.Add("ActiveTimeSort", typeof (string));
      dataTable.Columns.Add("ActiveTimeDisplay", typeof (string));
      dataTable.Columns.Add("TriggerTime", typeof (DateTime));
      dataTable.Columns.Add("TriggerTimeDisplay", typeof (string));
      dataTable.Columns.Add("LastTriggeredDateTime", typeof (DateTime));
      dataTable.Columns.Add("TriggerCount", typeof (int));
      dataTable.Columns.Add("AcknowledgedBy", typeof (string));
      dataTable.Columns.Add("AcknowledgedByFullName", typeof (string));
      dataTable.Columns.Add("AcknowledgeTime", typeof (DateTime));
      dataTable.Columns.Add("AcknowledgeTimeDisplay", typeof (string));
      dataTable.Columns.Add("Notes", typeof (string));
      dataTable.Columns.Add("NumberOfNotes", typeof (int));
      dataTable.Columns.Add("Severity", typeof (ActiveAlertSeverity));
      dataTable.Columns.Add("SeverityOrder", typeof (int));
      dataTable.Columns.Add("SeverityText", typeof (string));
      dataTable.Columns.Add("ActiveNetObject", typeof (string));
      dataTable.Columns.Add("ObjectType", typeof (string));
      dataTable.Columns.Add("LegacyAlert", typeof (bool));
      dataTable.Columns.Add("ObjectTriggeredThisAlertDisplayName", typeof (string));
      dataTable.Columns.Add("Canned", typeof (bool));
      dataTable.Columns.Add("Category", typeof (string));
      dataTable.Columns.Add("IncidentNumber", typeof (string));
      dataTable.Columns.Add("IncidentUrl", typeof (string));
      dataTable.Columns.Add("Assignee", typeof (string));
      dataTable.Columns.Add("SiteID", typeof (int));
      dataTable.Columns.Add("SiteName", typeof (string));
      using (IEnumerator<CustomProperty> enumerator = customProperties.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          CustomProperty current = enumerator.Current;
          dataTable.Columns.Add(string.Format("CP_{0}", (object) current.get_PropertyName()), current.get_PropertyType());
          if (current.get_PropertyType() == typeof (bool) || current.get_PropertyType() == typeof (float) || (current.get_PropertyType() == typeof (int) || current.get_PropertyType() == typeof (DateTime)))
            dataTable.Columns.Add(string.Format("CP_{0}_Display", (object) current.get_PropertyName()), typeof (string));
        }
      }
      using (IEnumerator<ActiveAlert> enumerator1 = alerts.GetEnumerator())
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          ActiveAlert current1 = enumerator1.Current;
          DataRow row = dataTable.NewRow();
          row["ActiveAlertID"] = (object) current1.get_Id();
          row["AlertDefID"] = (object) current1.get_AlertDefId();
          row["ActiveAlertType"] = (object) current1.get_AlertType();
          row["AlertName"] = (object) current1.get_Name();
          row["AlertMessage"] = (object) current1.get_Message();
          row["TriggeringObject"] = (object) current1.get_TriggeringObjectCaption();
          row["TriggeringObjectEntityName"] = (object) current1.get_TriggeringObjectEntityName();
          row["TriggeringObjectStatus"] = (object) current1.get_TriggeringObjectStatus();
          row["TriggeringObjectDetailsUrl"] = (object) current1.get_TriggeringObjectDetailsUrl();
          row["TriggeringObjectEntityUri"] = (object) current1.get_TriggeringObjectEntityUri();
          row["RelatedNode"] = (object) current1.get_RelatedNodeCaption();
          row["RelatedNodeID"] = (object) current1.get_RelatedNodeID();
          row["RelatedNodeEntityUri"] = (object) current1.get_RelatedNodeEntityUri();
          row["RelatedNodeDetailsUrl"] = (object) current1.get_RelatedNodeDetailsUrl();
          row["RelatedNodeStatus"] = (object) current1.get_RelatedNodeStatus();
          row["ActiveTime"] = (object) current1.get_ActiveTime();
          row["ActiveTimeSort"] = (object) current1.get_ActiveTime().ToString("d\\.hh\\:mm\\:ss", (IFormatProvider) CultureInfo.InvariantCulture);
          row["ActiveTimeDisplay"] = (object) current1.get_ActiveTimeDisplay();
          row["TriggerTime"] = (object) DateTime.SpecifyKind(current1.get_TriggerDateTime(), DateTimeKind.Utc);
          row["TriggerTimeDisplay"] = (object) DateTimeHelper.ConvertToDisplayDate(current1.get_TriggerDateTime());
          row["LastTriggeredDateTime"] = (object) current1.get_LastTriggeredDateTime();
          row["TriggerCount"] = (object) current1.get_TriggerCount();
          row["AcknowledgedBy"] = (object) current1.get_AcknowledgedBy();
          row["AcknowledgedByFullName"] = (object) current1.get_AcknowledgedByFullName();
          row["AcknowledgeTime"] = (object) DateTime.SpecifyKind(current1.get_AcknowledgedDateTime(), DateTimeKind.Utc);
          row["AcknowledgeTimeDisplay"] = !(current1.get_AcknowledgedDateTime() != DateTime.MinValue) || current1.get_AcknowledgedDateTime().Year == 1899 ? (object) Resources.get_WEBJS_PS0_59() : (object) DateTimeHelper.ConvertToDisplayDate(current1.get_AcknowledgedDateTime());
          row["Notes"] = (object) current1.get_Notes();
          row["NumberOfNotes"] = (object) current1.get_NumberOfNotes();
          row["Severity"] = (object) current1.get_Severity();
          row["SeverityText"] = (object) current1.get_Severityi18nMessage();
          row["SeverityOrder"] = (object) current1.get_SeverityOrder();
          row["ActiveNetObject"] = (object) current1.get_ActiveNetObject();
          row["ObjectType"] = (object) current1.get_ObjectType();
          row["LegacyAlert"] = (object) current1.get_LegacyAlert();
          row["ObjectTriggeredThisAlertDisplayName"] = (object) current1.get_ObjectTriggeredThisAlertDisplayName();
          row["Canned"] = (object) current1.get_Canned();
          row["Category"] = (object) current1.get_Category();
          row["IncidentNumber"] = (object) current1.get_IncidentNumber();
          row["IncidentUrl"] = (object) current1.get_IncidentUrl();
          row["Assignee"] = (object) current1.get_AssignedTo();
          row["SiteID"] = (object) current1.get_SiteID();
          row["SiteName"] = (object) current1.get_SiteName();
          if (current1.get_CustomProperties() != null)
          {
            using (IEnumerator<CustomProperty> enumerator2 = customProperties.GetEnumerator())
            {
              while (((IEnumerator) enumerator2).MoveNext())
              {
                CustomProperty current2 = enumerator2.Current;
                if (current1.get_CustomProperties().ContainsKey(current2.get_PropertyName()))
                {
                  row[string.Format("CP_{0}", (object) current2.get_PropertyName())] = current1.get_CustomProperties()[current2.get_PropertyName()] != null ? current1.get_CustomProperties()[current2.get_PropertyName()] : (object) DBNull.Value;
                  if (current2.get_PropertyType() == typeof (bool) || current2.get_PropertyType() == typeof (float) || current2.get_PropertyType() == typeof (int))
                    row[string.Format("CP_{0}_Display", (object) current2.get_PropertyName())] = current1.get_CustomProperties()[current2.get_PropertyName()] != null ? (object) Convert.ToString(current1.get_CustomProperties()[current2.get_PropertyName()]) : (object) string.Empty;
                  else if (current2.get_PropertyType() == typeof (DateTime))
                  {
                    if (current1.get_CustomProperties()[current2.get_PropertyName()] != null)
                      row[string.Format("CP_{0}_Display", (object) current2.get_PropertyName())] = (object) DateTimeHelper.ConvertToDisplayDate((DateTime) current1.get_CustomProperties()[current2.get_PropertyName()]);
                    else
                      row[string.Format("CP_{0}_Display", (object) current2.get_PropertyName())] = (object) DBNull.Value;
                  }
                }
              }
            }
          }
          dataTable.Rows.Add(row);
        }
      }
      return dataTable;
    }

    private static IEnumerable<ActiveAlert> ConvertRowsToActiveAlerts(
      IEnumerable<DataRow> rows,
      IEnumerable<CustomProperty> customProperties)
    {
      List<ActiveAlert> activeAlertList = new List<ActiveAlert>();
      foreach (DataRow row in rows)
      {
        ActiveAlert activeAlert = new ActiveAlert();
        activeAlert.set_Id(Convert.ToInt32(row["ActiveAlertID"]));
        activeAlert.set_AlertDefId(Convert.ToString(row["AlertDefID"]));
        activeAlert.set_AlertType((ActiveAlertType) row["ActiveAlertType"]);
        activeAlert.set_Name(Convert.ToString(row["AlertName"]));
        activeAlert.set_Message(Convert.ToString(row["AlertMessage"]));
        activeAlert.set_TriggeringObjectCaption(Convert.ToString(row["TriggeringObject"]));
        activeAlert.set_TriggeringObjectEntityName(Convert.ToString(row["TriggeringObjectEntityName"]));
        activeAlert.set_TriggeringObjectStatus(Convert.ToInt32(row["TriggeringObjectStatus"]));
        activeAlert.set_TriggeringObjectDetailsUrl(Convert.ToString(row["TriggeringObjectDetailsUrl"]));
        activeAlert.set_TriggeringObjectEntityUri(Convert.ToString(row["TriggeringObjectEntityUri"]));
        activeAlert.set_RelatedNodeCaption(Convert.ToString(row["RelatedNode"]));
        activeAlert.set_RelatedNodeID(Convert.ToInt32(row["RelatedNodeID"]));
        activeAlert.set_RelatedNodeEntityUri(Convert.ToString(row["RelatedNodeEntityUri"]));
        activeAlert.set_RelatedNodeDetailsUrl(Convert.ToString(row["RelatedNodeDetailsUrl"]));
        activeAlert.set_RelatedNodeStatus(Convert.ToInt32(row["RelatedNodeStatus"]));
        activeAlert.set_ActiveTime((TimeSpan) row["ActiveTime"]);
        activeAlert.set_ActiveTimeDisplay(Convert.ToString(row["ActiveTimeDisplay"]));
        activeAlert.set_TriggerDateTime(Convert.ToDateTime(row["TriggerTime"]));
        activeAlert.set_LastTriggeredDateTime(Convert.ToDateTime(row["LastTriggeredDateTime"]));
        activeAlert.set_TriggerCount(Convert.ToInt32(row["TriggerCount"]));
        activeAlert.set_AcknowledgedBy(Convert.ToString(row["AcknowledgedBy"]));
        activeAlert.set_AcknowledgedByFullName(Convert.ToString(row["AcknowledgedByFullName"]));
        activeAlert.set_AcknowledgedDateTime(Convert.ToDateTime(row["AcknowledgeTime"]));
        activeAlert.set_Notes(Convert.ToString(row["Notes"]));
        activeAlert.set_NumberOfNotes(Convert.ToInt32(row["NumberOfNotes"]));
        activeAlert.set_Severity((ActiveAlertSeverity) row["Severity"]);
        activeAlert.set_ActiveNetObject(Convert.ToString(row["ActiveNetObject"]));
        activeAlert.set_ObjectType(Convert.ToString(row["ObjectType"]));
        activeAlert.set_LegacyAlert(Convert.ToBoolean(row["LegacyAlert"]));
        activeAlert.set_Canned(Convert.ToBoolean(row["Canned"]));
        activeAlert.set_Category(Convert.ToString(row["Category"]));
        activeAlert.set_IncidentNumber(Convert.ToString(row["IncidentNumber"]));
        activeAlert.set_IncidentUrl(Convert.ToString(row["IncidentUrl"]));
        activeAlert.set_AssignedTo(Convert.ToString(row["Assignee"]));
        activeAlert.set_SiteID(Convert.ToInt32(row["SiteID"]));
        activeAlert.set_SiteName(Convert.ToString(row["SiteName"]));
        if (!activeAlert.get_LegacyAlert())
        {
          activeAlert.set_CustomProperties(new Dictionary<string, object>());
          using (IEnumerator<CustomProperty> enumerator = customProperties.GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              CustomProperty current = enumerator.Current;
              string index = string.Format("CP_{0}", (object) current.get_PropertyName());
              if (row[index] != DBNull.Value)
                activeAlert.get_CustomProperties().Add(current.get_PropertyName(), row[index]);
              else
                activeAlert.get_CustomProperties().Add(current.get_PropertyName(), (object) null);
            }
          }
        }
        activeAlertList.Add(activeAlert);
      }
      return (IEnumerable<ActiveAlert>) activeAlertList;
    }

    internal static DataTable GetFilteredTable(
      PageableActiveAlertRequest pRequest,
      DataTable alertTable,
      ActiveAlertsRequest request = null)
    {
      string alertsFilterCondition = AlertDAL.GetActiveAlertsFilterCondition(pRequest);
      bool flag = ((IEnumerable<string>) alertsFilterCondition.Split(new string[2]
      {
        "OR",
        "AND"
      }, StringSplitOptions.None)).Count<string>() > 100;
      if (!string.IsNullOrEmpty(alertsFilterCondition) && request == null || !flag)
      {
        alertTable.CaseSensitive = true;
        DataRow[] dataRowArray = alertTable.Select(alertsFilterCondition);
        if (dataRowArray != null && dataRowArray.Length != 0)
          alertTable = ((IEnumerable<DataRow>) dataRowArray).CopyToDataTable<DataRow>();
        else
          alertTable.Rows.Clear();
      }
      else if (request != null)
      {
        alertTable.CaseSensitive = true;
        DataRow[] array = alertTable.AsEnumerable().Where<DataRow>(AlertDAL.GenerateLambdaFilter(request).Compile()).ToArray<DataRow>();
        if (array != null && array.Length != 0)
          alertTable = ((IEnumerable<DataRow>) array).CopyToDataTable<DataRow>();
        else
          alertTable.Rows.Clear();
      }
      return alertTable;
    }

    private static Expression<Func<DataRow, bool>> GenerateLambdaFilter(
      ActiveAlertsRequest request)
    {
      Expression<Func<DataRow, bool>> filterPredicate = (Expression<Func<DataRow, bool>>) null;
      if (request.get_TriggeringObjectEntityUris() != null && ((IEnumerable<string>) request.get_TriggeringObjectEntityUris()).Any<string>())
      {
        foreach (string triggeringObjectEntityUri in request.get_TriggeringObjectEntityUris())
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_0 cDisplayClass290 = new AlertDAL.\u003C\u003Ec__DisplayClass29_0();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass290.template = triggeringObjectEntityUri;
          ParameterExpression parameterExpression;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["TriggeringObjectEntityUri"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass290, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_0)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_0.template)))), parameterExpression);
          filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
        }
        foreach (string triggeringObjectEntityUri in request.get_TriggeringObjectEntityUris())
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_1 cDisplayClass291 = new AlertDAL.\u003C\u003Ec__DisplayClass29_1();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass291.template = string.Format("{0}/", (object) triggeringObjectEntityUri);
          ParameterExpression parameterExpression;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["TriggeringObjectEntityUri"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Contains)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass291, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_1)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_1.template)))), parameterExpression);
          filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
        }
        if (((IEnumerable<string>) request.get_TriggeringObjectEntityNames()).Any<string>() && string.Compare(request.get_TriggeringObjectEntityNames()[0], "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0)
        {
          foreach (int num in request.get_AlertActiveIdsGlobal())
          {
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            AlertDAL.\u003C\u003Ec__DisplayClass29_2 cDisplayClass292 = new AlertDAL.\u003C\u003Ec__DisplayClass29_2();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass292.template = num.ToString();
            ParameterExpression parameterExpression;
            // ISSUE: method reference
            // ISSUE: method reference
            // ISSUE: field reference
            Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["ActiveAlertID"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass292, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_2)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_2.template)))), parameterExpression);
            filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
          }
        }
      }
      else if (request.get_TriggeringObjectEntityNames() != null && ((IEnumerable<string>) request.get_TriggeringObjectEntityNames()).Any<string>())
      {
        foreach (string objectEntityName in request.get_TriggeringObjectEntityNames())
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_3 cDisplayClass293 = new AlertDAL.\u003C\u003Ec__DisplayClass29_3();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass293.template = objectEntityName;
          ParameterExpression parameterExpression;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["TriggeringObjectEntityName"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass293, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_3)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_3.template)))), parameterExpression);
          filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
        }
      }
      else if (request.get_RelatedNodeId() > 0 || !string.IsNullOrEmpty(request.get_RelatedNodeEntityUri()))
      {
        if (request.get_RelatedNodeId() > 0)
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_4 cDisplayClass294 = new AlertDAL.\u003C\u003Ec__DisplayClass29_4();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass294.template = request.get_RelatedNodeId().ToString();
          ParameterExpression parameterExpression;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["RelatedNodeID"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass294, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_4)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_4.template)))), parameterExpression);
          filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
        }
        IEnumerable<int> alertActiveIdsGlobal = request.get_AlertActiveIdsGlobal();
        if (!string.IsNullOrEmpty(request.get_RelatedNodeEntityUri()))
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_5 cDisplayClass295 = new AlertDAL.\u003C\u003Ec__DisplayClass29_5();
          foreach (int num in alertActiveIdsGlobal)
          {
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            AlertDAL.\u003C\u003Ec__DisplayClass29_6 cDisplayClass296 = new AlertDAL.\u003C\u003Ec__DisplayClass29_6();
            // ISSUE: reference to a compiler-generated field
            cDisplayClass296.template = num.ToString();
            ParameterExpression parameterExpression;
            // ISSUE: method reference
            // ISSUE: method reference
            // ISSUE: field reference
            Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["ActiveAlertID"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass296, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_6)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_6.template)))), parameterExpression);
            filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
          }
          // ISSUE: reference to a compiler-generated field
          cDisplayClass295.relatedNodeEntityUri = request.get_RelatedNodeEntityUri();
          ParameterExpression parameterExpression1;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression1 = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["TriggeringObjectEntityUri"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass295, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_5)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_5.relatedNodeEntityUri)))), parameterExpression1);
          ParameterExpression parameterExpression2;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          filterPredicate = AlertDAL.GetFilterPredicate(AlertDAL.GetFilterPredicate(filterPredicate, testExpression1), Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["RelatedNodeEntityUri"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass295, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_5)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_5.relatedNodeEntityUri)))), parameterExpression2));
        }
        foreach (string objectEntityName in request.get_TriggeringObjectEntityNames())
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          AlertDAL.\u003C\u003Ec__DisplayClass29_7 cDisplayClass297 = new AlertDAL.\u003C\u003Ec__DisplayClass29_7();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass297.template = objectEntityName;
          ParameterExpression parameterExpression;
          // ISSUE: method reference
          // ISSUE: method reference
          // ISSUE: field reference
          Expression<Func<DataRow, bool>> testExpression = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Call((Expression) Expression.Call(p["TriggeringObjectEntityName"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass297, typeof (AlertDAL.\u003C\u003Ec__DisplayClass29_7)), FieldInfo.GetFieldFromHandle(__fieldref (AlertDAL.\u003C\u003Ec__DisplayClass29_7.template)))), parameterExpression);
          filterPredicate = AlertDAL.GetFilterPredicate(filterPredicate, testExpression);
        }
      }
      if (!request.get_ShowAcknowledgedAlerts())
      {
        ParameterExpression parameterExpression;
        // ISSUE: method reference
        // ISSUE: method reference
        // ISSUE: method reference
        // ISSUE: field reference
        InvocationExpression invocationExpression = Expression.Invoke((Expression) Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.OrElse((Expression) Expression.Call((Expression) Expression.Call(p["AcknowledgedBy"], (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (object.ToString)), Array.Empty<Expression>()), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.Equals)), (Expression) Expression.Constant((object) "", typeof (string))), (Expression) Expression.Equal((Expression) Expression.Call((Expression) parameterExpression, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (DataRow.get_Item)), (Expression) Expression.Constant((object) "AcknowledgedBy", typeof (string))), (Expression) Expression.Field((Expression) null, FieldInfo.GetFieldFromHandle(__fieldref (DBNull.Value))))), parameterExpression), filterPredicate.Parameters.Cast<Expression>());
        filterPredicate = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.And(filterPredicate.Body, (Expression) invocationExpression), (IEnumerable<ParameterExpression>) filterPredicate.Parameters);
      }
      return filterPredicate;
    }

    private static Expression<Func<DataRow, bool>> GetFilterPredicate(
      Expression<Func<DataRow, bool>> filterPredicate,
      Expression<Func<DataRow, bool>> testExpression)
    {
      if (filterPredicate == null)
        filterPredicate = testExpression;
      InvocationExpression invocationExpression = Expression.Invoke((Expression) testExpression, filterPredicate.Parameters.Cast<Expression>());
      filterPredicate = Expression.Lambda<Func<DataRow, bool>>((Expression) Expression.Or(filterPredicate.Body, (Expression) invocationExpression), (IEnumerable<ParameterExpression>) filterPredicate.Parameters);
      return filterPredicate;
    }

    private static string GetActiveAlertsFilterCondition(PageableActiveAlertRequest request)
    {
      string empty = string.Empty;
      return string.IsNullOrEmpty(request.get_FilterStatement()) ? AlertDAL.GetFilterCondition(request.get_FilterByPropertyName(), request.get_FilterByPropertyValue(), request.get_FilterByPropertyType()) : request.get_FilterStatement();
    }

    private static string GetFilterCondition(
      string FilterByPropertyName,
      string FilterByPropertyValue,
      string FilterByPropertyType)
    {
      string empty = string.Empty;
      if (string.IsNullOrEmpty(FilterByPropertyName) || FilterByPropertyValue.Equals("[All]", StringComparison.OrdinalIgnoreCase))
        return empty;
      string str1 = "(" + FilterByPropertyName;
      string str2;
      if (string.IsNullOrEmpty(FilterByPropertyValue) && FilterByPropertyType == "System.String")
        str2 = str1 + " IS NULL OR " + FilterByPropertyName + " = ''";
      else if (string.IsNullOrEmpty(FilterByPropertyValue))
        str2 = str1 + " IS NULL";
      else if (FilterByPropertyType == "System.String")
        str2 = str1 + "='" + FilterByPropertyValue.Replace("'", "''") + "'";
      else if (FilterByPropertyType == "System.DateTime")
      {
        DateTime result = DateTime.MinValue;
        str2 = !DateTime.TryParse(FilterByPropertyValue, (IFormatProvider) Thread.CurrentThread.CurrentUICulture, DateTimeStyles.None, out result) ? (!DateTime.TryParseExact(FilterByPropertyValue, "MM/dd/yyyy h:mm tt", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? str1 + string.Format("='{0}'", (object) DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ss")) : str1 + string.Format("='{0}'", (object) result.ToString("yyyy-MM-ddTHH:mm:ss"))) : str1 + string.Format("='{0}'", (object) result.ToString("yyyy-MM-ddTHH:mm:ss"));
      }
      else
        str2 = !(FilterByPropertyType == "System.Single") ? str1 + string.Format("={0}", (object) FilterByPropertyValue) : str1 + "=" + FilterByPropertyValue.Replace(",", ".");
      return str2 + ")";
    }

    private static string GetActiveAlertsSearchCondition(
      string filterValue,
      IEnumerable<CustomProperty> customProperties)
    {
      string str1 = AlertDAL.EscapeLikeValue(filterValue);
      string str2 = string.Empty + "AlertName LIKE '" + str1 + "'" + " OR AlertMessage LIKE '" + str1 + "'" + " OR ObjectTriggeredThisAlertDisplayName LIKE '" + str1 + "'" + " OR ActiveTimeDisplay LIKE '" + str1 + "'" + " OR TriggerTimeDisplay LIKE '" + str1 + "'" + " OR AcknowledgedBy LIKE '" + str1 + "'" + " OR SeverityText LIKE '" + str1 + "'" + " OR AcknowledgeTimeDisplay LIKE '" + str1 + "'";
      using (IEnumerator<CustomProperty> enumerator = customProperties.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          CustomProperty current = enumerator.Current;
          str2 = !(current.get_PropertyType() != typeof (bool)) || !(current.get_PropertyType() != typeof (float)) || (!(current.get_PropertyType() != typeof (int)) || !(current.get_PropertyType() != typeof (DateTime))) ? str2 + string.Format(" OR CP_{0}_Display LIKE '{1}'", (object) current.get_PropertyName(), (object) str1) : str2 + string.Format(" OR CP_{0} LIKE '{1}'", (object) current.get_PropertyName(), (object) str1);
        }
      }
      return str2;
    }

    private static IEnumerable<DataRow> GetFilteredAlertRows(
      PageableActiveAlertRequest request,
      DataTable alertTable,
      IEnumerable<CustomProperty> customProperties)
    {
      string strCondition = !string.IsNullOrWhiteSpace(request.get_SearchValue()) ? AlertDAL.GetActiveAlertsSearchCondition(request.get_SearchValue(), customProperties) : string.Empty;
      string filterCondition = AlertDAL.GetFilterCondition(request.get_SecondaryFilters(), strCondition, request.get_SecondaryFilterOperator());
      return (IEnumerable<DataRow>) alertTable.Select(filterCondition);
    }

    private static IEnumerable<DataRow> GetSortedAlerts(
      PageableActiveAlertRequest request,
      IEnumerable<DataRow> alertRows)
    {
      DataRow[] dataRowArray = new DataRow[0];
      if (!alertRows.Any<DataRow>())
        return (IEnumerable<DataRow>) dataRowArray;
      string sortColumnName = string.Empty;
      SortOrder sortOrder = SortOrder.Ascending;
      if (request.get_OrderByClause().EndsWith("ASC", StringComparison.OrdinalIgnoreCase))
        sortColumnName = request.get_OrderByClause().Substring(0, request.get_OrderByClause().Length - 3).Trim().TrimStart('[').TrimEnd(']');
      else if (request.get_OrderByClause().EndsWith("DESC", StringComparison.OrdinalIgnoreCase))
      {
        sortColumnName = request.get_OrderByClause().Substring(0, request.get_OrderByClause().Length - 4).Trim().TrimStart('[').TrimEnd(']');
        sortOrder = SortOrder.Descending;
      }
      else if (!string.IsNullOrEmpty(request.get_OrderByClause()))
        sortColumnName = request.get_OrderByClause().TrimStart('[').TrimEnd(']');
      return string.IsNullOrEmpty(request.get_OrderByClause()) ? (IEnumerable<DataRow>) alertRows.ToArray<DataRow>() : (IEnumerable<DataRow>) AlertDAL.GetSortedAlerts(alertRows, sortColumnName, sortOrder).ToArray<DataRow>();
    }

    private static IEnumerable<DataRow> GetSortedAlerts(
      IEnumerable<DataRow> rows,
      string sortColumnName,
      SortOrder sortOrder)
    {
      if (!rows.Any<DataRow>())
        return (IEnumerable<DataRow>) new DataRow[0];
      if (!rows.ElementAt<DataRow>(0).Table.Columns.Contains(sortColumnName))
      {
        AlertDAL.Log.WarnFormat("Unable to sort by column '{0}', because column doesn't belong to the table. Alert grid will not be sorted. If it is custom property column please make sure, that wasn't deleted.", (object) sortColumnName);
        return rows;
      }
      Type type = rows.First<DataRow>()[sortColumnName].GetType();
      if (type == typeof (DateTime))
      {
        Func<DataRow, DateTime> keySelector = (Func<DataRow, DateTime>) (row =>
        {
          if (row[sortColumnName] == DBNull.Value)
            return new DateTime();
          DateTime? nullable = row[sortColumnName] as DateTime?;
          return !nullable.HasValue ? new DateTime() : nullable.Value;
        });
        return sortOrder != SortOrder.Ascending ? (IEnumerable<DataRow>) rows.OrderByDescending<DataRow, DateTime>(keySelector).ToArray<DataRow>() : (IEnumerable<DataRow>) rows.OrderBy<DataRow, DateTime>(keySelector).ToArray<DataRow>();
      }
      if (type == typeof (TimeSpan))
      {
        Func<DataRow, TimeSpan> keySelector = (Func<DataRow, TimeSpan>) (row =>
        {
          if (row[sortColumnName] == DBNull.Value)
            return TimeSpan.FromSeconds(0.0);
          TimeSpan? nullable = row[sortColumnName] as TimeSpan?;
          return !nullable.HasValue ? TimeSpan.FromSeconds(0.0) : nullable.Value;
        });
        return sortOrder != SortOrder.Ascending ? (IEnumerable<DataRow>) rows.OrderByDescending<DataRow, TimeSpan>(keySelector).ToArray<DataRow>() : (IEnumerable<DataRow>) rows.OrderBy<DataRow, TimeSpan>(keySelector).ToArray<DataRow>();
      }
      return sortOrder != SortOrder.Ascending ? (IEnumerable<DataRow>) rows.OrderByDescending<DataRow, string>((Func<DataRow, string>) (row => row[sortColumnName] == DBNull.Value ? string.Empty : Convert.ToString(row[sortColumnName])), (IComparer<string>) new NaturalStringComparer()).ToArray<DataRow>() : (IEnumerable<DataRow>) rows.OrderBy<DataRow, string>((Func<DataRow, string>) (row => row[sortColumnName] == DBNull.Value ? string.Empty : Convert.ToString(row[sortColumnName])), (IComparer<string>) new NaturalStringComparer()).ToArray<DataRow>();
    }

    private static ActiveAlert SortableAlertDataRowToActiveAlertObject(
      DataRow rAlert,
      Func<string, ActiveAlertType, string> getSwisEntityName,
      DateTime currentDateTime,
      List<string> nodeStatusIds,
      List<string> interfaceStatusIds,
      List<string> containerStatusIds,
      List<string> acknowledgedBy)
    {
      ActiveAlert activeAlert = new ActiveAlert();
      activeAlert.set_AlertDefId(rAlert["AlertID"] != DBNull.Value ? Convert.ToString(rAlert["AlertID"]) : string.Empty);
      if (AlertDAL.TryStrToGuid(activeAlert.get_AlertDefId(), out Guid _))
        activeAlert.set_AlertType((ActiveAlertType) 0);
      else
        activeAlert.set_AlertType((ActiveAlertType) 1);
      activeAlert.set_Name(rAlert["AlertName"] != DBNull.Value ? Convert.ToString(rAlert["AlertName"]) : string.Empty);
      activeAlert.set_TriggerDateTime(rAlert["AlertTime"] != DBNull.Value ? Convert.ToDateTime(rAlert["AlertTime"]) : DateTime.MinValue);
      activeAlert.set_Message(rAlert["EventMessage"] != DBNull.Value ? Convert.ToString(rAlert["EventMessage"]) : string.Empty);
      activeAlert.set_TriggeringObjectCaption(rAlert["ObjectName"] != DBNull.Value ? Convert.ToString(rAlert["ObjectName"]) : string.Empty);
      activeAlert.set_ActiveNetObject(rAlert["ActiveNetObject"] != DBNull.Value ? Convert.ToString(rAlert["ActiveNetObject"]) : "0");
      string str1 = rAlert["ObjectType"] != DBNull.Value ? Convert.ToString(rAlert["ObjectType"]) : string.Empty;
      string strA = getSwisEntityName(str1, activeAlert.get_AlertType());
      activeAlert.set_TriggeringObjectEntityName(strA);
      activeAlert.set_RelatedNodeCaption(rAlert["Sysname"] != DBNull.Value ? Convert.ToString(rAlert["Sysname"]) : string.Empty);
      activeAlert.set_RelatedNodeID(rAlert["NodeID"] != DBNull.Value ? Convert.ToInt32(rAlert["NodeID"]) : 0);
      if (activeAlert.get_RelatedNodeID() > 0 && !nodeStatusIds.Contains(activeAlert.get_RelatedNodeID().ToString()))
        nodeStatusIds.Add(activeAlert.get_RelatedNodeID().ToString());
      if (string.Compare(strA, "Orion.Nodes", StringComparison.OrdinalIgnoreCase) == 0)
      {
        if (!nodeStatusIds.Contains(activeAlert.get_ActiveNetObject()))
          nodeStatusIds.Add(activeAlert.get_ActiveNetObject());
        int result = 0;
        if (int.TryParse(activeAlert.get_ActiveNetObject(), out result))
        {
          activeAlert.set_RelatedNodeCaption(activeAlert.get_TriggeringObjectCaption());
          activeAlert.set_RelatedNodeID(result);
        }
      }
      else if (string.Compare(strA, "Orion.NPM.Interfaces", StringComparison.OrdinalIgnoreCase) == 0)
        interfaceStatusIds.Add(activeAlert.get_ActiveNetObject());
      else if (string.Compare(strA, "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0)
        containerStatusIds.Add(activeAlert.get_ActiveNetObject());
      string str2 = rAlert["NetObjectPrefix"] != DBNull.Value ? Convert.ToString(rAlert["NetObjectPrefix"]) : string.Empty;
      if (!string.IsNullOrEmpty(str2))
        activeAlert.set_TriggeringObjectDetailsUrl(string.Format("/Orion/View.aspx?NetObject={0}:{1}", (object) str2, (object) activeAlert.get_ActiveNetObject()));
      else
        activeAlert.set_TriggeringObjectDetailsUrl(string.Empty);
      activeAlert.set_ActiveTime(currentDateTime - activeAlert.get_TriggerDateTime());
      activeAlert.set_ActiveTimeDisplay(new ActiveAlertDAL().ActiveTimeToDisplayFormat(activeAlert.get_ActiveTime()));
      activeAlert.set_RelatedNodeDetailsUrl(string.Format("/Orion/View.aspx?NetObject=N:{0}", (object) activeAlert.get_RelatedNodeID()));
      activeAlert.set_AcknowledgedBy(rAlert["AcknowledgedBy"] != DBNull.Value ? Convert.ToString(rAlert["AcknowledgedBy"]) : string.Empty);
      acknowledgedBy.Add(activeAlert.get_AcknowledgedBy());
      activeAlert.set_AcknowledgedDateTime(rAlert["AcknowledgedTime"] != DBNull.Value ? DateTime.SpecifyKind(Convert.ToDateTime(rAlert["AcknowledgedTime"]), DateTimeKind.Local) : DateTime.MinValue);
      string str3 = string.Format("{0} - ", (object) activeAlert.get_RelatedNodeCaption());
      if (activeAlert.get_TriggeringObjectCaption().StartsWith(str3))
        activeAlert.set_TriggeringObjectCaption(activeAlert.get_TriggeringObjectCaption().Substring(str3.Length, activeAlert.get_TriggeringObjectCaption().Length - str3.Length));
      activeAlert.set_ObjectType(str1);
      activeAlert.set_Severity((ActiveAlertSeverity) 1);
      activeAlert.set_LegacyAlert(true);
      return activeAlert;
    }

    internal static string EscapeLikeValue(string value)
    {
      if (!value.StartsWith("%") || !value.EndsWith("%") || value.Length < 2)
        return AlertDAL.EscapeLikeValueInternal(value);
      value = value.Substring(1, value.Length - 2);
      return string.Format("%{0}%", (object) AlertDAL.EscapeLikeValueInternal(value));
    }

    private static string EscapeLikeValueInternal(string value)
    {
      StringBuilder stringBuilder = new StringBuilder(value.Length);
      for (int index = 0; index < value.Length; ++index)
      {
        char ch = value[index];
        switch (ch)
        {
          case '%':
          case '*':
          case '[':
          case ']':
            stringBuilder.Append("[").Append(ch).Append("]");
            break;
          case '\'':
            stringBuilder.Append("''");
            break;
          default:
            stringBuilder.Append(ch);
            break;
        }
      }
      return stringBuilder.ToString();
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static ActiveAlert GetActiveAlert(
      string activeAlertDefId,
      string activeNetObject,
      string objectType,
      IEnumerable<int> limitationIDs)
    {
      DataRow[] dataRowArray1 = AlertDAL.GetSortableAlertTable(string.Empty, string.Empty, activeAlertDefId, "AlertTime DESC", int.MaxValue, true, limitationIDs.ToList<int>(), true).Select(string.Format("ActiveNetObject='{0}' AND ObjectType='{1}'", (object) activeNetObject, (object) objectType));
      DataTable tblNetObjectTypes = AlertDAL.GetAvailableObjectTypes(false);
      Func<string, ActiveAlertType, string> getSwisEntityName = (Func<string, ActiveAlertType, string>) ((objectType2, alertType) =>
      {
        DataRow[] dataRowArray2 = alertType != null ? tblNetObjectTypes.Select("Prefix='" + objectType2 + "'") : tblNetObjectTypes.Select("Name='" + objectType2 + "'");
        return dataRowArray2.Length != 0 && dataRowArray2[0]["EntityType"] != DBNull.Value ? Convert.ToString(dataRowArray2[0]["EntityType"]) : string.Empty;
      });
      if (dataRowArray1.Length != 0)
      {
        List<string> nodeStatusIds = new List<string>();
        List<string> interfaceStatusIds = new List<string>();
        List<string> containerStatusIds = new List<string>();
        List<string> acknowledgedBy = new List<string>();
        ActiveAlert activeAlertObject = AlertDAL.SortableAlertDataRowToActiveAlertObject(dataRowArray1[0], getSwisEntityName, DateTime.Now, nodeStatusIds, interfaceStatusIds, containerStatusIds, acknowledgedBy);
        Dictionary<string, int> statusesForSwisEntities1 = AlertDAL.GetStatusesForSwisEntities("Orion.Nodes", "NodeID", (IEnumerable<string>) nodeStatusIds, false);
        Dictionary<string, int> statusesForSwisEntities2 = AlertDAL.GetStatusesForSwisEntities("Orion.NPM.Interfaces", "InterfaceID", (IEnumerable<string>) interfaceStatusIds, false);
        Dictionary<string, int> statusesForSwisEntities3 = AlertDAL.GetStatusesForSwisEntities("Orion.Groups", "ContainerID", (IEnumerable<string>) containerStatusIds, false);
        Dictionary<string, string> fullUserNames = AlertDAL.GetFullUserNames((IEnumerable<string>) acknowledgedBy, false);
        string strA = getSwisEntityName(activeAlertObject.get_ObjectType(), activeAlertObject.get_AlertType());
        if (string.Compare(strA, "Orion.Nodes", StringComparison.OrdinalIgnoreCase) == 0 && statusesForSwisEntities1.ContainsKey(activeAlertObject.get_ActiveNetObject()))
          activeAlertObject.set_TriggeringObjectStatus(statusesForSwisEntities1[activeAlertObject.get_ActiveNetObject()]);
        else if (string.Compare(strA, "Orion.NPM.Interfaces", StringComparison.OrdinalIgnoreCase) == 0 && statusesForSwisEntities2.ContainsKey(activeAlertObject.get_ActiveNetObject()))
          activeAlertObject.set_TriggeringObjectStatus(statusesForSwisEntities2[activeAlertObject.get_ActiveNetObject()]);
        else if (string.Compare(strA, "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0 && statusesForSwisEntities3.ContainsKey(activeAlertObject.get_ActiveNetObject()))
          activeAlertObject.set_TriggeringObjectStatus(statusesForSwisEntities3[activeAlertObject.get_ActiveNetObject()]);
        Dictionary<string, int> dictionary1 = statusesForSwisEntities1;
        int relatedNodeId = activeAlertObject.get_RelatedNodeID();
        string key = relatedNodeId.ToString();
        if (dictionary1.ContainsKey(key))
        {
          ActiveAlert activeAlert = activeAlertObject;
          Dictionary<string, int> dictionary2 = statusesForSwisEntities1;
          relatedNodeId = activeAlertObject.get_RelatedNodeID();
          string index = relatedNodeId.ToString();
          int num = dictionary2[index];
          activeAlert.set_RelatedNodeStatus(num);
        }
        if (fullUserNames.ContainsKey(activeAlertObject.get_AcknowledgedBy()))
          activeAlertObject.set_AcknowledgedByFullName(fullUserNames[activeAlertObject.get_AcknowledgedBy()]);
        activeAlertObject.set_Notes(AlertDAL.GetAlertNote(activeAlertObject.get_AlertDefId(), activeAlertObject.get_ActiveNetObject(), activeAlertObject.get_ObjectType()));
        activeAlertObject.set_Status((ActiveAlertStatus) 1);
        activeAlertObject.set_EscalationLevel(1);
        activeAlertObject.set_LegacyAlert(true);
        return activeAlertObject;
      }
      string str = "SELECT Name FROM Orion.AlertDefinitions WHERE AlertDefID=@alertDefId AND ObjectType=@objectType";
      DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) AlertDAL.SwisProxy, str, new Dictionary<string, object>()
      {
        {
          "alertDefId",
          (object) activeAlertDefId
        },
        {
          nameof (objectType),
          (object) objectType
        }
      });
      if (dataTable.Rows.Count <= 0)
        return (ActiveAlert) null;
      ActiveAlert activeAlert1 = new ActiveAlert();
      activeAlert1.set_LegacyAlert(false);
      activeAlert1.set_Status((ActiveAlertStatus) 0);
      activeAlert1.set_Name(dataTable.Rows[0]["Name"] != DBNull.Value ? Convert.ToString(dataTable.Rows[0]["Name"]) : string.Empty);
      activeAlert1.set_Severity((ActiveAlertSeverity) 1);
      activeAlert1.set_ObjectType(objectType);
      activeAlert1.set_TriggeringObjectEntityName(getSwisEntityName(objectType, activeAlert1.get_AlertType()));
      activeAlert1.set_ActiveNetObject(activeNetObject);
      string strA1 = getSwisEntityName(activeAlert1.get_ObjectType(), activeAlert1.get_AlertType());
      if (string.Compare(strA1, "Orion.Nodes", StringComparison.OrdinalIgnoreCase) == 0)
      {
        Dictionary<string, int> statusesForSwisEntities = AlertDAL.GetStatusesForSwisEntities("Orion.Nodes", "NodeID", (IEnumerable<string>) new List<string>()
        {
          activeNetObject
        }, false);
        if (statusesForSwisEntities.ContainsKey(activeAlert1.get_ActiveNetObject()))
          activeAlert1.set_TriggeringObjectStatus(statusesForSwisEntities[activeAlert1.get_ActiveNetObject()]);
      }
      else if (string.Compare(strA1, "Orion.NPM.Interfaces", StringComparison.OrdinalIgnoreCase) == 0)
      {
        Dictionary<string, int> statusesForSwisEntities = AlertDAL.GetStatusesForSwisEntities("Orion.NPM.Interfaces", "InterfaceID", (IEnumerable<string>) new List<string>()
        {
          activeNetObject
        }, false);
        if (statusesForSwisEntities.ContainsKey(activeAlert1.get_ActiveNetObject()))
          activeAlert1.set_TriggeringObjectStatus(statusesForSwisEntities[activeAlert1.get_ActiveNetObject()]);
      }
      else if (string.Compare(strA1, "Orion.Groups", StringComparison.OrdinalIgnoreCase) == 0)
      {
        Dictionary<string, int> statusesForSwisEntities = AlertDAL.GetStatusesForSwisEntities("Orion.Groups", "ContainerID", (IEnumerable<string>) new List<string>()
        {
          activeNetObject
        }, false);
        if (statusesForSwisEntities.ContainsKey(activeAlert1.get_ActiveNetObject()))
          activeAlert1.set_TriggeringObjectStatus(statusesForSwisEntities[activeAlert1.get_ActiveNetObject()]);
      }
      return activeAlert1;
    }

    private static string GetFilterCondition(
      IEnumerable<ActiveAlertFilter> filters,
      string strCondition,
      FilterOperator filterOperator)
    {
      if (filters.Any<ActiveAlertFilter>())
      {
        string str1 = "AND";
        if (filterOperator == 1)
          str1 = "OR";
        if (!string.IsNullOrEmpty(strCondition))
          strCondition = string.Format("({0}) {1} ", (object) strCondition, (object) str1);
        strCondition = filterOperator != null ? strCondition + " ( 1<>1" : strCondition + " ( 1=1 ";
        using (IEnumerator<ActiveAlertFilter> enumerator = filters.GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            ActiveAlertFilter current = enumerator.Current;
            if (string.Compare(current.get_FieldDataType(), "datetime", StringComparison.OrdinalIgnoreCase) == 0)
              strCondition = AlertDAL.GetDateTimeFilterCondition(strCondition, current);
            else if (string.Compare(current.get_FieldDataType(), "string", StringComparison.OrdinalIgnoreCase) == 0)
            {
              if (!string.IsNullOrEmpty(current.get_Value()))
              {
                current.set_Value(AlertDAL.EscapeLikeValue(current.get_Value()));
                strCondition = current.get_Comparison() != null ? (current.get_Comparison() != 4 ? strCondition + string.Format(" {2} {0} <> '{1}'", (object) current.get_FieldName(), (object) current.get_Value(), (object) str1) : strCondition + string.Format(" {2} {0} LIKE '%{1}%'", (object) current.get_FieldName(), (object) current.get_Value(), (object) str1)) : strCondition + string.Format(" {2} {0} = '{1}'", (object) current.get_FieldName(), (object) current.get_Value(), (object) str1);
              }
              else
                strCondition = current.get_Comparison() == 3 ? strCondition + string.Format(" {1} ({0} <> '' OR {0} IS NOT NULL)", (object) current.get_FieldName(), (object) str1) : strCondition + string.Format(" {1} ({0} = '' OR {0} IS NULL)", (object) current.get_FieldName(), (object) str1);
            }
            else if (string.Compare(current.get_FieldDataType(), "list$system.string", StringComparison.OrdinalIgnoreCase) == 0)
            {
              if (!string.IsNullOrEmpty(current.get_Value()))
              {
                if (current.get_Comparison() != 3)
                {
                  StringBuilder stringBuilder = new StringBuilder();
                  string str2 = current.get_Value();
                  string[] separator = new string[1]{ "$#" };
                  foreach (string str3 in str2.Split(separator, StringSplitOptions.None))
                    stringBuilder.Append("'" + str3 + "',");
                  if (stringBuilder.Length > 0)
                    stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
                  strCondition += string.Format(" {2} {0} in ({1})", (object) current.get_FieldName(), (object) stringBuilder, (object) str1);
                }
                else
                  strCondition += string.Format(" {2} {0} <> '{1}'", (object) current.get_FieldName(), (object) current.get_Value(), (object) str1);
              }
              else
                strCondition = current.get_Comparison() == 3 ? strCondition + string.Format(" {1} ({0} <> '' OR {0} IS NOT NULL)", (object) current.get_FieldName(), (object) str1) : strCondition + string.Format(" {1} ({0} = '' OR {0} IS NULL)", (object) current.get_FieldName(), (object) str1);
            }
            else if (current.get_FieldDataType().StartsWith("bool"))
              strCondition = current.get_Comparison() != null ? strCondition + string.Format(" {2} {0} <> {1}", (object) current.get_FieldName(), (object) Convert.ToInt32(current.get_Value()), (object) str1) : strCondition + string.Format(" {2} {0} = {1}", (object) current.get_FieldName(), (object) Convert.ToInt32(current.get_Value()), (object) str1);
            else if (string.Compare(current.get_FieldDataType(), "numeric", true) == 0)
            {
              if (current.get_Comparison() == 1)
                strCondition += string.Format(" {2} {0} < {1}", (object) current.get_FieldName(), (object) Convert.ToDouble(current.get_Value()), (object) str1);
              else if (current.get_Comparison() == 2)
                strCondition += string.Format(" {2} {0} > {1}", (object) current.get_FieldName(), (object) Convert.ToDouble(current.get_Value()), (object) str1);
              else if (current.get_Comparison() == null)
                strCondition = !string.IsNullOrEmpty(current.get_Value()) ? strCondition + string.Format(" {2} {0} = {1}", (object) current.get_FieldName(), (object) Convert.ToDouble(current.get_Value()), (object) str1) : strCondition + string.Format(" {1} {0} IS Null", (object) current.get_FieldName(), (object) str1);
            }
          }
        }
        strCondition += ")";
      }
      return strCondition;
    }

    private static string GetDateTimeFilterCondition(string strCondition, ActiveAlertFilter filter)
    {
      string str1 = DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ss");
      string str2 = new DateTime(1899, 12, 30).ToString("yyyy-MM-ddTHH:mm:ss");
      if (string.IsNullOrEmpty(filter.get_Value()))
      {
        strCondition += string.Format(" AND ({0} IS NULL OR {0} = '{1}' OR {0} = '{2}') ", (object) filter.get_FieldName(), (object) str1, (object) str2);
      }
      else
      {
        DateTime result;
        if (DateTime.TryParseExact(filter.get_Value(), "MM/dd/yyyy h:mm tt", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
        {
          string str3 = result.ToString("yyyy-MM-ddTHH:mm:ss");
          if (filter.get_Comparison() == null)
            strCondition += string.Format(" AND {0} = '{1}'", (object) filter.get_FieldName(), (object) str3);
          else if (filter.get_Comparison() == 1)
            strCondition += string.Format(" AND {0} < '{1}' AND {0} > '{2}' AND {0} > '{3}'", (object) filter.get_FieldName(), (object) str3, (object) str1, (object) str2);
          else if (filter.get_Comparison() == 2)
            strCondition += string.Format(" AND {0} > '{1}'", (object) filter.get_FieldName(), (object) str3);
          else if (filter.get_Comparison() == 3)
            strCondition += string.Format(" AND {0} <> '{1}'", (object) filter.get_FieldName(), (object) str3);
        }
      }
      return strCondition;
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static string GetAdvAlertSwql(List<int> limitationIDs)
    {
      return AlertDAL.GetAdvAlertSwql(string.Empty, string.Empty, limitationIDs, false, false);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static string GetAdvAlertSwql(
      string whereClause,
      string advAlertsLabel,
      List<int> limitationIDs,
      bool includeDefaultFields,
      bool includeToolsetFields)
    {
      return AlertDAL.GetAdvAlertSwql(whereClause, string.Empty, string.Empty, advAlertsLabel, limitationIDs, includeDefaultFields, includeToolsetFields);
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static string GetAdvAlertSwql(
      string whereClause,
      string netObjectWhereClause,
      string netObject,
      string advAlertsLabel,
      List<int> limitationIDs,
      bool includeDefaultFields,
      bool includeToolsetFields)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str1 = string.Empty;
      List<string> activeNetObjectTypes = AlertDAL.GetActiveNetObjectTypes(whereClause);
      List<NetObjectType> netObjectTypeList;
      if (activeNetObjectTypes != null && activeNetObjectTypes.Count != 0)
      {
        netObjectTypeList = (List<NetObjectType>) ModuleAlertsMap.get_NetObjectTypes().Items;
      }
      else
      {
        netObjectTypeList = new List<NetObjectType>();
        netObjectTypeList.Add(((List<NetObjectType>) ModuleAlertsMap.get_NetObjectTypes().Items)[0]);
      }
      string format = "{0}";
      using (List<NetObjectType>.Enumerator enumerator = netObjectTypeList.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NetObjectType noType = enumerator.Current;
          if (activeNetObjectTypes.Count <= 0 || activeNetObjectTypes.Any<string>((Func<string, bool>) (netObj => string.Equals(netObj, noType.get_Name(), StringComparison.OrdinalIgnoreCase))))
          {
            string triggeredAlertsQuery = ModuleAlertsMap.GetTriggeredAlertsQuery(noType.get_Name(), includeDefaultFields, includeToolsetFields);
            AlertDAL.Log.DebugFormat("Query Template for {0}  : {1} ", (object) noType.get_Name(), (object) triggeredAlertsQuery);
            if (!string.IsNullOrEmpty(triggeredAlertsQuery))
            {
              string netObjectCondition = ModuleAlertsMap.GetParentNetObjectCondition(noType.get_Name(), netObject);
              string str2 = string.Format("{0} {1}", (object) whereClause, string.IsNullOrEmpty(netObjectCondition) ? (object) netObjectWhereClause : (object) string.Format(" AND {0}", (object) netObjectCondition));
              stringBuilder.AppendLine(str1);
              string str3 = string.Format(triggeredAlertsQuery, (object) str2, (object) advAlertsLabel);
              stringBuilder.AppendFormat(format, (object) str3);
              format = "({0})";
              str1 = " UNION ";
            }
          }
        }
      }
      return stringBuilder.ToString();
    }

    [Obsolete("Old alerting will be removed")]
    private static List<string> GetActiveNetObjectTypes(string whereClause)
    {
      List<string> stringList = new List<string>();
      string str = string.Empty;
      Match match = AlertDAL._ackRegex.Match(whereClause);
      if (match.Success)
        str = string.Format("AND {0}", (object) match.Value);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("Select DISTINCT AlertStatus.ObjectType from AlertDefinitions WITH(NOLOCK)\r\nINNER JOIN AlertStatus WITH(NOLOCK) ON AlertStatus.AlertDefID = AlertDefinitions.AlertDefID Where (AlertStatus.State=2 OR AlertStatus.State=3) {0}", (object) str)))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringList.Add(DatabaseFunctions.GetString(dataReader, "ObjectType"));
        }
      }
      return stringList;
    }

    [Obsolete("Method does not return V2 alerts.")]
    public static DataTable GetPageableAlerts(
      List<int> limitationIDs,
      string period,
      int fromRow,
      int toRow,
      string type,
      string alertId,
      bool showAcknAlerts)
    {
      DataTable dataTable = (DataTable) null;
      List<SqlParameter> sqlParams = new List<SqlParameter>();
      string[] strArray = period.Split('~');
      DateTime localTime1 = DateTime.Parse(strArray[0]).ToLocalTime();
      DateTime localTime2 = DateTime.Parse(strArray[1]).ToLocalTime();
      List<SqlParameter> sqlParameterList1 = sqlParams;
      SqlParameter sqlParameter1 = new SqlParameter("@StartDate", SqlDbType.DateTime);
      sqlParameter1.Value = (object) localTime1;
      sqlParameterList1.Add(sqlParameter1);
      List<SqlParameter> sqlParameterList2 = sqlParams;
      SqlParameter sqlParameter2 = new SqlParameter("@EndDate", SqlDbType.DateTime);
      sqlParameter2.Value = (object) localTime2;
      sqlParameterList2.Add(sqlParameter2);
      string str1 = "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes\r\nSELECT * INTO #Nodes FROM Nodes WHERE 1=1;";
      string str2 = Limitation.LimitSQL(str1, (IEnumerable<int>) limitationIDs);
      int num = !AlertDAL._enableLimitationReplacement ? 0 : (str2.Length / str1.Length > AlertDAL._limitationSqlExaggeration ? 1 : 0);
      string str3 = num != 0 ? str2 : string.Empty;
      string str4 = num != 0 ? str3 : string.Empty;
      string str5 = num != 0 ? "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes" : string.Empty;
      string upper = type.ToUpper();
      string str6;
      if (!(upper == "ADVANCED"))
      {
        if (upper == "BASIC")
          str6 = string.Format("Select * from (\r\nSELECT a.AlertTime, a.AlertName, 'Basic' AS AlertType, a.ObjectType + '::' + a.ObjectName As ObjectName, \r\na.Acknowledged, a.AlertID, a.AlertID as BAlertID, a.ObjectType, a.ObjectID as ActiveObject, ROW_NUMBER() OVER (ORDER BY a.ObjectName, a.AlertName) AS Row \r\nFROM ( {0} ) a Where a.AlertTime >= @StartDate And a.AlertTime <= @EndDate\r\n) t \r\nWHERE Row BETWEEN {1} AND {2} Order By t.ObjectName, t.AlertName", (object) AlertDAL.GetBasicAlertSwql(string.Empty, string.Empty, alertId, limitationIDs, true, true), (object) fromRow, (object) toRow);
        else
          str6 = string.Format("{4}Select * from (\r\nSELECT a.AlertTime, a.AlertName, \r\na.AlertType, Case a.AlertType When 'Advanced' Then a.ObjectName Else a.ObjectType + '::' + a.ObjectName End As ObjectName, a.Acknowledged, a.AlertID, \r\na.BAlertID, a.ObjectType, a.ObjectID as ActiveObject, ROW_NUMBER() OVER (ORDER BY a.ObjectName, a.AlertName) AS Row \r\nFROM ( {0} Union ( {1} ))a Where a.AlertTime >= @StartDate And a.AlertTime <= @EndDate\r\n) t \r\nWHERE Row BETWEEN {2} AND {3} Order By t.ObjectName, t.AlertName\r\n{5}", (object) string.Format("Select *, 0 as BAlertID, 'Advanced' AS AlertType From ({0}) AAT", (object) AlertDAL.GetAdvAlertSwql(AlertDAL.GetWhereClause(alertId, showAcknAlerts, sqlParams), string.Empty, string.Empty, OrionMessagesHelper.GetMessageTypeString((OrionMessageType) 0), limitationIDs, true, true)), (object) string.Format("Select *, BAT.AlertID as BAlertID, 'Basic' AS AlertType From ({0}) BAT", (object) AlertDAL.GetBasicAlertSwql(string.Empty, string.Empty, alertId, limitationIDs, true, true)), (object) fromRow, (object) toRow, (object) str4, (object) str5);
      }
      else
        str6 = string.Format("{3}Select * from (\r\nSELECT a.AlertTime, a.AlertName, 'Advanced' AS AlertType, a.ObjectName, a.Acknowledged, a.AlertID, 0 as BAlertID, a.ObjectType, \r\na.ObjectID as ActiveObject, ROW_NUMBER() OVER (ORDER BY a.ObjectName, a.AlertName) AS Row \r\nFROM ( {0} )a Where a.AlertTime >= @StartDate And a.AlertTime <= @EndDate\r\n) t \r\nWHERE Row BETWEEN {1} AND {2} Order By t.ObjectName, t.AlertName\r\n{4}", (object) AlertDAL.GetAdvAlertSwql(AlertDAL.GetWhereClause(alertId, showAcknAlerts, sqlParams), string.Empty, string.Empty, OrionMessagesHelper.GetMessageTypeString((OrionMessageType) 0), limitationIDs, true, true), (object) fromRow, (object) toRow, (object) str4, (object) str5);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(str6))
      {
        textCommand.Parameters.AddRange(sqlParams.ToArray());
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      dataTable.TableName = "Alerts";
      return dataTable;
    }

    private static string GetWhereClause(
      string alertId,
      bool showAcknAlerts,
      List<SqlParameter> sqlParams)
    {
      StringBuilder stringBuilder = new StringBuilder(" AND (AlertStatus.State=@Triggered) ");
      List<SqlParameter> sqlParameterList1 = sqlParams;
      SqlParameter sqlParameter1 = new SqlParameter("@Triggered", SqlDbType.TinyInt);
      sqlParameter1.Value = (object) 2;
      sqlParameterList1.Add(sqlParameter1);
      if (!showAcknAlerts)
      {
        stringBuilder.Append(" AND AlertStatus.Acknowledged=@Acknowledged ");
        List<SqlParameter> sqlParameterList2 = sqlParams;
        SqlParameter sqlParameter2 = new SqlParameter("@Acknowledged", SqlDbType.TinyInt);
        sqlParameter2.Value = (object) 0;
        sqlParameterList2.Add(sqlParameter2);
      }
      Guid result;
      if (Guid.TryParse(alertId, out result))
      {
        stringBuilder.Append(" AND (AlertStatus.AlertDefID=@AlertDefID) ");
        List<SqlParameter> sqlParameterList2 = sqlParams;
        SqlParameter sqlParameter2 = new SqlParameter("@AlertDefID", SqlDbType.UniqueIdentifier);
        sqlParameter2.Value = (object) result;
        sqlParameterList2.Add(sqlParameter2);
      }
      return stringBuilder.ToString();
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static string GetBasicAlertSwql(List<int> limitationIDs)
    {
      return AlertDAL.GetBasicAlertSwql(string.Empty, string.Empty, string.Empty, limitationIDs, false, false);
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static string GetBasicAlertSwql(
      string netObject,
      string deviceType,
      string alertId,
      List<int> limitationIDs,
      bool includeDefaultFields,
      bool includeToolsetFields)
    {
      string empty = string.Empty;
      int num = 0;
      if (!string.IsNullOrEmpty(netObject))
      {
        string[] strArray = netObject.Split(':');
        if (strArray.Length == 2)
        {
          empty = strArray[0];
          num = Convert.ToInt32(strArray[1]);
        }
      }
      StringBuilder stringBuilder = new StringBuilder();
      if (num != 0)
      {
        if (empty == "N")
          stringBuilder.AppendFormat(" AND (ActiveAlerts.NodeID={0}) ", (object) num);
        else
          stringBuilder.AppendFormat(" AND (ObjectType='{0}' AND ObjectID={1}) ", (object) empty, (object) num);
      }
      else if (!string.IsNullOrEmpty(deviceType))
        stringBuilder.AppendFormat(" AND (MachineType Like '{0}') ", (object) deviceType);
      int result;
      if (int.TryParse(alertId, out result))
        stringBuilder.AppendFormat(" AND (ActiveAlerts.AlertID={0}) ", (object) result);
      else if (!string.IsNullOrEmpty(alertId))
        stringBuilder.Append(" AND (ActiveAlerts.AlertID=0) ");
      return AlertDAL.GetBasicAlertSwql(stringBuilder.ToString(), string.Empty, limitationIDs, includeDefaultFields, includeToolsetFields);
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static string GetBasicAlertSwql(
      string whereClause,
      string basicAlertsLabel,
      List<int> limitationIDs,
      bool includeDefaultFields,
      bool includeToolsetFields)
    {
      bool flag = AlertDAL.IsInterfacesAllowed();
      StringBuilder stringBuilder = new StringBuilder("SELECT ");
      if (includeDefaultFields)
        stringBuilder.AppendFormat("\r\nCAST(ActiveAlerts.AlertID AS NVARCHAR(38)) AS AlertID,\r\nAlerts.AlertName AS AlertName,\r\nActiveAlerts.AlertTime AS AlertTime, \r\nCAST(ActiveAlerts.ObjectID AS NVARCHAR(38)) AS ObjectID, \r\nCASE WHEN ActiveAlerts.ObjectType = 'N' THEN ActiveAlerts.ObjectName ELSE ActiveAlerts.NodeName + '-' + ActiveAlerts.ObjectName END AS ObjectName,\r\nActiveAlerts.ObjectType AS ObjectType,\r\n'0' AS Acknowledged,\r\n'' AS AcknowledgedBy, \r\n'18991230' AS AcknowledgedTime, \r\nCAST(ActiveAlerts.EventMessage AS NVARCHAR(1024)) AS EventMessage,\r\n{0} AS MonitoredProperty, \r\n", !string.IsNullOrEmpty(basicAlertsLabel) ? (object) string.Format("'{0}'", (object) basicAlertsLabel) : (object) "ActiveAlerts.MonitoredProperty");
      if (includeToolsetFields)
      {
        stringBuilder.Append("\r\nNodes.IP_Address AS IP_Address, \r\nNodes.DNS AS DNS, \r\nNodes.[SysName] AS [Sysname], \r\nNodes.[Community] AS [Community], \r\n");
        if (flag)
          stringBuilder.Append("\r\nInterfaces.InterfaceName AS InterfaceName, \r\nInterfaces.InterfaceIndex AS InterfaceIndex,\r\n");
        else
          stringBuilder.Append("\r\nNULL AS InterfaceName, \r\nNULL AS InterfaceIndex,\r\n");
      }
      stringBuilder.Append("\r\nActiveAlerts.CurrentValue AS CurrentValue, \r\nCAST(ActiveAlerts.ObjectID AS NVARCHAR(38)) AS ActiveNetObject, \r\nActiveAlerts.ObjectType AS NetObjectPrefix, \r\nNodes.NodeID AS NodeID\r\nFROM ActiveAlerts\r\nINNER JOIN Nodes WITH(NOLOCK) ON ActiveAlerts.NodeID = Nodes.NodeID\r\nINNER JOIN Alerts WITH(NOLOCK) ON ActiveAlerts.AlertID = Alerts.AlertID ");
      if (includeToolsetFields && flag)
        stringBuilder.Append(" LEFT OUTER JOIN Interfaces WITH(NOLOCK) ON ActiveAlerts.ObjectID = Interfaces.InterfaceID AND ActiveAlerts.ObjectType = 'I' ");
      stringBuilder.AppendLine(" WHERE 1=1 ");
      stringBuilder.Append(whereClause);
      return Limitation.LimitSQL(stringBuilder.ToString(), (IEnumerable<int>) limitationIDs);
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static List<SolarWinds.Orion.Core.Common.Models.Node> GetAlertNetObjects(
      List<int> limitationIDs)
    {
      List<SolarWinds.Orion.Core.Common.Models.Node> nodeList = new List<SolarWinds.Orion.Core.Common.Models.Node>();
      string str = string.Format("Select * FROM Nodes WITH(NOLOCK)  \r\n                                    WHERE Nodes.NodeID IN (\r\n\t\t\t\t\t\t\t\t\tSelect DISTINCT NodeID FROM({0} UNION {1}) as t1\r\n\t\t\t\t\t\t\t\t\t) Order By Caption", (object) AlertDAL.GetAdvAlertSwql(limitationIDs), (object) AlertDAL.GetBasicAlertSwql(limitationIDs));
      bool[] flagArray = new bool[2];
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            nodeList.Add(NodeDAL.CreateNode(dataReader, flagArray));
        }
      }
      return nodeList;
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static Dictionary<int, string> GetNodeData(List<int> limitationIDs)
    {
      return AlertDAL.GetNodeData(limitationIDs, true);
    }

    [Obsolete("Don't use this method anymore. Old alerts will be removed.")]
    public static Dictionary<int, string> GetNodeData(
      List<int> limitationIDs,
      bool includeBasic)
    {
      Dictionary<int, string> dictionary = new Dictionary<int, string>();
      string str1 = Limitation.LimitSQL("Select Top 1 NodeID from Nodes", (IEnumerable<int>) limitationIDs);
      int num = !AlertDAL._enableLimitationReplacement ? 0 : (str1.Length / "Select Top 1 NodeID from Nodes".Length > AlertDAL._limitationSqlExaggeration ? 1 : 0);
      string str2 = num != 0 ? "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes" : string.Empty;
      string str3 = num != 0 ? "#Nodes" : "Nodes";
      string str4 = num != 0 ? "IF OBJECT_ID('tempdb..#Nodes') IS NOT NULL\tDROP TABLE #Nodes\r\nSELECT * INTO #Nodes FROM Nodes WHERE 1=1;" : string.Empty;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("{4}\r\nSelect {3}.NodeID, {3}.Caption\r\nFROM {3} \r\nWhere {3}.NodeID IN\r\n(Select DISTINCT NodeID FROM({0}{1}) as t1) \r\nOrder By Caption \r\n{2}", (object) AlertDAL.GetAdvAlertSwql(limitationIDs), includeBasic ? (object) (" UNION " + AlertDAL.GetBasicAlertSwql(limitationIDs)) : (object) string.Empty, (object) str2, (object) str3, (object) str4)))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            dictionary.Add(DatabaseFunctions.GetInt32(dataReader, "NodeID"), DatabaseFunctions.GetString(dataReader, "Caption"));
        }
      }
      return dictionary;
    }

    [Obsolete("Old alerting will be removed")]
    public static void AcknowledgeAlertsAction(
      List<string> alertKeys,
      string accountID,
      bool fromEmail,
      string acknowledgeNotes)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, fromEmail ? (AlertAcknowledgeType) 1 : (AlertAcknowledgeType) 0, acknowledgeNotes);
    }

    [Obsolete("Old alerting will be removed")]
    public static void AcknowledgeAlertsAction(
      List<string> alertKeys,
      string accountId,
      AlertAcknowledgeType acknowledgeType,
      string acknowledgeNotes)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountId, acknowledgeNotes, AlertsHelper.GetAcknowledgeMethodDisplayString(acknowledgeType));
    }

    [Obsolete("Old alerting will be removed")]
    public static int AcknowledgeAlertsAction(
      List<string> alertKeys,
      string accountID,
      string acknowledgeNotes,
      string method)
    {
      int num = 0;
      foreach (string alertKey in alertKeys)
      {
        string alertId;
        string netObjectId;
        string objectType;
        if (AlertsHelper.TryParseAlertKey(alertKey, ref alertId, ref netObjectId, ref objectType) && AlertDAL.AcknowledgeAlert(alertId, netObjectId, objectType, accountID, acknowledgeNotes, method) == null)
          ++num;
      }
      return num;
    }

    [Obsolete("Old alerting will be removed")]
    public static AlertAcknowledgeResult AcknowledgeAlert(
      string alertId,
      string netObjectId,
      string objectType,
      string accountId,
      string acknowledgeNotes,
      string method)
    {
      string str1 = string.Empty;
      if (!string.IsNullOrEmpty(acknowledgeNotes))
        str1 = ", Notes = CAST(ISNULL(Notes, '') AS NVARCHAR(MAX)) + @Notes";
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nBEGIN TRAN\r\n\r\nDECLARE @acknowleged smallint;\r\nSET @acknowleged = -1;\r\n\r\nSELECT @acknowleged = Acknowledged  FROM [AlertStatus] \r\nWHERE AlertDefID =  @AlertDefID AND ActiveObject = @ActiveObject AND ObjectType LIKE @ObjectType\r\n\r\nIF(@acknowleged = 0)\r\nBEGIN\r\n\tUPDATE AlertStatus SET \r\n                                    Acknowledged = 1, \r\n                                    AcknowledgedBy = @AcknowledgedBy,\r\n                                    AcknowledgedTime = GETDATE(),\r\n          LastUpdate = GETDATE() {0}\r\n          WHERE AlertDefID = @AlertDefID AND ActiveObject = @ActiveObject AND ObjectType LIKE @ObjectType AND Acknowledged = 0\r\nEND\r\n\r\nSELECT @acknowleged\r\n\r\nCOMMIT", (object) str1)))
      {
        string acknowledgeUsername = AlertsHelper.GetAcknowledgeUsername(accountId, method);
        textCommand.Parameters.AddWithValue("@AcknowledgedBy", (object) acknowledgeUsername);
        textCommand.Parameters.AddWithValue("@AlertDefID", (object) alertId);
        textCommand.Parameters.AddWithValue("@ActiveObject", (object) netObjectId);
        textCommand.Parameters.AddWithValue("@ObjectType", (object) objectType);
        string str2 = string.Format(Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_7(), (object) Environment.NewLine, (object) acknowledgeNotes);
        textCommand.Parameters.AddWithValue("@Notes", (object) str2);
        return (AlertAcknowledgeResult) Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static void UnacknowledgeAlertsAction(
      List<string> alertKeys,
      string accountID,
      AlertAcknowledgeType acknowledgeType)
    {
      foreach (string alertKey in alertKeys)
      {
        string str1;
        string str2;
        string str3;
        if (AlertsHelper.TryParseAlertKey(alertKey, ref str1, ref str2, ref str3))
        {
          using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE AlertStatus SET \r\n                                    Acknowledged = 0, \r\n                                    AcknowledgedBy = @AcknowledgedBy,\r\n                                    AcknowledgedTime = GETDATE(),\r\n                                    LastUpdate = GETDATE()\r\n                                   WHERE AlertDefID = @AlertDefID\r\n                                     AND ActiveObject = @ActiveObject\r\n                                     AND ObjectType LIKE @ObjectType\r\n                                     AND Acknowledged = 1"))
          {
            textCommand.Parameters.AddWithValue("@AcknowledgedBy", (object) AlertsHelper.GetAcknowledgeUsername(accountID, acknowledgeType));
            textCommand.Parameters.AddWithValue("@AlertDefID", (object) str1);
            textCommand.Parameters.AddWithValue("@ActiveObject", (object) str2);
            textCommand.Parameters.AddWithValue("@ObjectType", (object) str3);
            SqlHelper.ExecuteNonQuery(textCommand);
          }
        }
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static void AcknowledgeAlertsAction(List<string> alertKeys, string accountID)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, false);
    }

    [Obsolete("Old alerting will be removed")]
    public static void AcknowledgeAlertsAction(
      List<string> alertKeys,
      string accountID,
      bool fromEmail)
    {
      AlertDAL.AcknowledgeAlertsAction(alertKeys, accountID, fromEmail, (string) null);
    }

    [Obsolete("Old alerting will be removed")]
    public static void ClearTriggeredAlert(List<string> alertKeys)
    {
      Regex regex = new Regex("^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", RegexOptions.Compiled);
      foreach (string alertKey in alertKeys)
      {
        string input;
        string str1;
        string str2;
        if (AlertsHelper.TryParseAlertKey(alertKey, ref input, ref str1, ref str2))
        {
          string empty = string.Empty;
          if (regex.IsMatch(input))
          {
            using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM AlertStatus WHERE AlertDefID = @AlertDefID \r\n                                    AND ActiveObject = @ActiveObject AND ObjectType LIKE @ObjectType"))
            {
              textCommand.Parameters.AddWithValue("@AlertDefID", (object) input);
              textCommand.Parameters.AddWithValue("@ActiveObject", (object) str1);
              textCommand.Parameters.AddWithValue("@ObjectType", (object) str2);
              SqlHelper.ExecuteNonQuery(textCommand);
            }
          }
          else
          {
            using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM ActiveAlerts WHERE AlertID=@alertID AND ObjectID=@activeObject AND ObjectType LIKE @objectType"))
            {
              textCommand.Parameters.AddWithValue("@alertID", (object) input);
              textCommand.Parameters.AddWithValue("@activeObject", (object) str1);
              textCommand.Parameters.AddWithValue("@objectType", (object) str2);
              SqlHelper.ExecuteNonQuery(textCommand);
            }
          }
        }
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static int EnableAdvancedAlert(Guid alertDefID, bool enabled)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nSET NOCOUNT OFF;\r\nUPDATE [AlertDefinitions]\r\n SET [Enabled]=@enabled\r\n WHERE AlertDefID = @AlertDefID"))
      {
        textCommand.Parameters.Add("@AlertDefID", SqlDbType.UniqueIdentifier).Value = (object) alertDefID;
        textCommand.Parameters.Add("@enabled", SqlDbType.Bit).Value = (object) enabled;
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static int EnableAdvancedAlerts(List<string> alertDefIDs, bool enabled, bool enableAll)
    {
      if (alertDefIDs.Count == 0)
        return 0;
      string str1 = string.Empty;
      string str2 = string.Empty;
      if (!enableAll)
      {
        foreach (string alertDefId in alertDefIDs)
        {
          str1 = string.Format("{0}{1}'{2}'", (object) str1, (object) str2, (object) alertDefId);
          str2 = ", ";
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nSET NOCOUNT OFF;\r\nUPDATE [AlertDefinitions]\r\n SET [Enabled]=@enabled\r\n{0}", enableAll ? (object) string.Empty : (object) string.Format("WHERE AlertDefID in ({0})", (object) str1))))
      {
        textCommand.Parameters.Add("@enabled", SqlDbType.Bit).Value = (object) enabled;
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static int RemoveAdvancedAlert(Guid alertDefID)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nSET NOCOUNT OFF;\r\nDelete FROM [AlertDefinitions]\r\n WHERE AlertDefID = @AlertDefID"))
      {
        textCommand.Parameters.Add("@AlertDefID", SqlDbType.UniqueIdentifier).Value = (object) alertDefID;
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static int RemoveAdvancedAlerts(List<string> alertDefIDs, bool deleteAll)
    {
      if (alertDefIDs.Count == 0)
        return 0;
      string str1 = string.Empty;
      string str2 = string.Empty;
      if (!deleteAll)
      {
        foreach (string alertDefId in alertDefIDs)
        {
          str1 = string.Format("{0}{1}'{2}'", (object) str1, (object) str2, (object) alertDefId);
          str2 = ", ";
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nSET NOCOUNT OFF;\r\nDelete FROM [AlertDefinitions]\r\n{0}", deleteAll ? (object) string.Empty : (object) string.Format("WHERE AlertDefID in ({0})", (object) str1))))
        return SqlHelper.ExecuteNonQuery(textCommand);
    }

    [Obsolete("Old alerting will be removed")]
    public static int AdvAlertsCount()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT COUNT(AlertDefID) AS TotalCount FROM [AlertDefinitions]"))
        return Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
    }

    [Obsolete("Old alerting will be removed")]
    public static DataTable GetAdvancedAlerts()
    {
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("Select * from [AlertDefinitions]"))
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      if (dataTable != null)
        dataTable.TableName = "AlertsDefinition";
      return dataTable;
    }

    [Obsolete("Old alerting will be removed")]
    public static List<AlertAction> GetAdvancedAlertActions(Guid? alertDefID = null)
    {
      return OldAlertsDAL.GetAdvancedAlertActions(alertDefID);
    }

    [Obsolete("Old alerting will be removed")]
    public static AlertDefinitionOld GetAdvancedAlertDefinition(Guid alertDefID)
    {
      return OldAlertsDAL.GetAdvancedAlertDefinition(alertDefID, true);
    }

    [Obsolete("Old alerting will be removed")]
    public static List<AlertDefinitionOld> GetAdvancedAlertDefinitions()
    {
      return OldAlertsDAL.GetAdvancedAlertDefinitions(true);
    }

    [Obsolete("Old alerting will be removed")]
    public static DataTable GetAdvancedAlert(Guid alertDefID)
    {
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT Ald.AlertDefID, Ald.AlertName, Ald.AlertDescription, Ald.StartTime, Ald.EndTime, Ald.DOW, Ald.Enabled, Ald.TriggerSustained, Ald.ExecuteInterval, Ald.IgnoreTimeout,\r\n\t\t\t\t\t\t\tAld.TriggerQuery, Ald.ResetQuery, Ald.SuppressionQuery, \r\n\t\t\t\t\t\t\tAcd.TriggerAction, Acd.SortOrder, ActionType, Title, Target, Parameter1, Parameter2, Parameter3, Parameter4\r\n\t\t\t\tFROM [AlertDefinitions] Ald\r\n\t\t\t\tLeft Join [ActionDefinitions] Acd ON Ald.AlertDefID = Acd.AlertDefID\r\nWHERE Ald.AlertDefID=@AlertDefID\r\nOrder by Ald.AlertDefID, Acd.TriggerAction, Acd.SortOrder\r\n"))
      {
        textCommand.Parameters.Add("@AlertDefID", SqlDbType.UniqueIdentifier).Value = (object) alertDefID;
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      if (dataTable != null)
        dataTable.TableName = "AlertsDefinition";
      return dataTable;
    }

    [Obsolete("Old alerting will be removed")]
    public static int UpdateAlertDef(Guid alertDefID, bool enabled)
    {
      return OldAlertsDAL.UpdateAlertDef(alertDefID, enabled);
    }

    [Obsolete("Old alerting will be removed")]
    public static int UpdateAlertDef(
      Guid alertDefID,
      string alertName,
      string alertDescr,
      bool enabled,
      int evInterval,
      string dow,
      DateTime startTime,
      DateTime endTime,
      bool ignoreTimeout)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SET NOCOUNT OFF;\r\nUpdate [AlertDefinitions]\r\n SET [AlertName] = @alertName\r\n      ,[AlertDescription] = @alertDescr\r\n      ,[Enabled] = @enabled\r\n      ,[DOW] = @dow\r\n      ,[ExecuteInterval] = @evInterval\r\n\t\t,[StartTime] = @startTime\r\n\t\t,[EndTime] = @endTime\r\n\t,[IgnoreTimeout] = @ignoreTimeout\r\n WHERE AlertDefID = @AlertDefID"))
      {
        textCommand.Parameters.Add("@AlertDefID", SqlDbType.UniqueIdentifier).Value = (object) alertDefID;
        textCommand.Parameters.Add("@alertName", SqlDbType.NVarChar).Value = (object) alertName;
        textCommand.Parameters.Add("@alertDescr", SqlDbType.NVarChar).Value = (object) alertDescr;
        textCommand.Parameters.Add("@enabled", SqlDbType.Bit).Value = (object) enabled;
        textCommand.Parameters.Add("@dow", SqlDbType.NVarChar, 16).Value = (object) dow;
        textCommand.Parameters.Add("@evInterval", SqlDbType.BigInt).Value = (object) evInterval;
        textCommand.Parameters.Add("@startTime", SqlDbType.DateTime).Value = (object) startTime;
        textCommand.Parameters.Add("@endTime", SqlDbType.DateTime).Value = (object) endTime;
        textCommand.Parameters.Add("@ignoreTimeout", SqlDbType.Bit).Value = (object) ignoreTimeout;
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static DataTable GetPagebleAdvancedAlerts(
      string column,
      string direction,
      int number,
      int size)
    {
      size = Math.Max(size, 25);
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nSelect *\r\nfrom (SELECT *, ROW_NUMBER() OVER (ORDER BY {0} {1}) RowNr from [AlertDefinitions]) t\r\nWHERE RowNr BETWEEN {2} AND {3}\r\nORDER BY {0} {1}", string.IsNullOrEmpty(column) ? (object) "AlertName" : (object) column, string.IsNullOrEmpty(direction) ? (object) "ASC" : (object) direction, (object) (number + 1), (object) (number + size))))
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      if (dataTable != null)
        dataTable.TableName = "AlertsDefinition";
      return dataTable;
    }

    public static int UpdateAdvancedAlertNote(
      string alerfDefID,
      string activeObject,
      string objectType,
      string notes)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE AlertStatus SET Notes=@Notes Where AlertDefID=@AlertDefID AND ActiveObject=@ActiveObject AND ObjectType=@ObjectType"))
      {
        textCommand.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = (object) notes;
        textCommand.Parameters.AddWithValue("@AlertDefID", (object) alerfDefID);
        textCommand.Parameters.Add("@ActiveObject", SqlDbType.VarChar).Value = (object) activeObject;
        textCommand.Parameters.Add("@ObjectType", SqlDbType.NVarChar).Value = (object) objectType;
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static int AppendNoteToAlert(
      string alertId,
      string activeObject,
      string objectType,
      string note)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE AlertStatus SET Notes =(\r\nCASE\r\nWHEN (Notes IS NULL)\r\nTHEN\r\n @Notes\r\nELSE\r\n CAST(Notes AS NVARCHAR(MAX)) + CHAR(13) + CHAR(10) + '---' + CHAR(13) + CHAR(10) + @Notes\r\nEND\r\n) Where AlertDefID=@AlertDefID AND ActiveObject=@ActiveObject AND ObjectType=@ObjectType"))
      {
        textCommand.Parameters.AddWithValue("@AlertDefID", (object) alertId);
        textCommand.Parameters.Add("@ActiveObject", SqlDbType.VarChar).Value = (object) activeObject;
        textCommand.Parameters.Add("@ObjectType", SqlDbType.NVarChar).Value = (object) objectType;
        textCommand.Parameters.AddWithValue("@Notes", (object) note);
        return SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static AlertNotificationSettings GetAlertNotificationSettings(
      string alertDefID,
      string netObjectType,
      string alertName)
    {
      try
      {
        IAlertNotificationSettingsProvider settingsProvider = (IAlertNotificationSettingsProvider) new AlertNotificationSettingsProvider();
        AlertNotificationSettings notificationSettings = (AlertNotificationSettings) null;
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertName\r\n                            ,ObjectType\r\n                            ,NotifyEnabled\r\n                            ,NotificationSettings\r\n                    FROM [AlertDefinitions]\r\n                    WHERE AlertDefID = @AlertDefinitionId"))
        {
          textCommand.Parameters.AddWithValue("@AlertDefinitionId", (object) (string.IsNullOrWhiteSpace(alertDefID) ? Guid.Empty : new Guid(alertDefID)));
          DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand);
          if (dataTable.Rows.Count != 0)
          {
            string str1 = (string) dataTable.Rows[0]["AlertName"];
            string str2 = (string) dataTable.Rows[0]["ObjectType"];
            bool flag = (bool) dataTable.Rows[0]["NotifyEnabled"];
            string str3 = dataTable.Rows[0]["NotificationSettings"] is DBNull ? (string) null : (string) dataTable.Rows[0]["NotificationSettings"];
            if (str2.Equals(netObjectType, StringComparison.OrdinalIgnoreCase))
            {
              notificationSettings = settingsProvider.GetAlertNotificationSettings(str2, str1, str3);
              notificationSettings.set_Enabled(flag);
            }
          }
          if (notificationSettings == null)
          {
            notificationSettings = settingsProvider.GetDefaultAlertNotificationSettings(netObjectType, alertName);
            notificationSettings.set_Enabled(true);
          }
        }
        return notificationSettings;
      }
      catch (Exception ex)
      {
        AlertDAL.Log.Error((object) string.Format("Error getting alert notification settings for alert {0}", (object) alertDefID), ex);
        throw;
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static void SetAlertNotificationSettings(
      string alertDefID,
      AlertNotificationSettings settings)
    {
      try
      {
        if (alertDefID == null)
          throw new ArgumentNullException(nameof (alertDefID));
        string xml = ((IAlertNotificationSettingsConverter) new AlertNotificationSettingsConverter()).ToXml(settings);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE [AlertDefinitions] SET\r\n                            NotifyEnabled = @NotifyEnabled,\r\n                            NotificationSettings = @NotificationSettings\r\n                      WHERE AlertDefID = @AlertDefinitionId"))
        {
          textCommand.Parameters.AddWithValue("@AlertDefinitionId", (object) alertDefID);
          textCommand.Parameters.AddWithValue("@NotifyEnabled", (object) settings.get_Enabled());
          textCommand.Parameters.AddWithValue("@NotificationSettings", (object) xml);
          SqlHelper.ExecuteNonQuery(textCommand);
        }
      }
      catch (Exception ex)
      {
        AlertDAL.Log.Error((object) string.Format("Error setting alert notification settings for alert {0}", (object) alertDefID), ex);
        throw;
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static AlertNotificationDetails GetAlertDetailsForNotification(
      string alertDefID,
      string activeObject,
      string objectType)
    {
      try
      {
        AlertNotificationDetails notificationDetails1 = new AlertNotificationDetails();
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT s.ActiveObject\r\n                          ,s.ObjectType\r\n                          ,s.ObjectName\r\n                          ,s.TriggerTimeStamp\r\n                          ,s.ResetTimeStamp\r\n                          ,s.Acknowledged\r\n                          ,s.AcknowledgedBy\r\n                          ,s.AcknowledgedTime\r\n                          ,s.AlertNotes\r\n                          ,s.Notes\r\n                          ,s.AlertMessage\r\n                          ,s.TriggerCount\r\n                         ,ad.AlertName\r\n                         ,ad.NotifyEnabled\r\n                         ,ad.NotificationSettings\r\n                    FROM AlertStatus s JOIN AlertDefinitions ad\r\n                      ON s.AlertDefID = ad.AlertDefID\r\n                    WHERE ad.NotifyEnabled = 1\r\n                      AND s.AlertDefID = @AlertDefinitionId\r\n                      AND s.ActiveObject=@ActiveObject \r\n                      AND s.ObjectType LIKE @ObjectType"))
        {
          textCommand.Parameters.AddWithValue("@AlertDefinitionId", (object) alertDefID);
          textCommand.Parameters.AddWithValue("@ActiveObject", (object) activeObject);
          textCommand.Parameters.AddWithValue("@ObjectType", (object) objectType);
          DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand);
          if (dataTable.Rows.Count == 0)
            return notificationDetails1;
          string str1;
          string str2;
          AlertsHelper.GetOriginalUsernameFromAcknowledgeUsername((string) dataTable.Rows[0]["AcknowledgedBy"], ref str1, ref str2);
          notificationDetails1.set_Acknowledged((byte) dataTable.Rows[0]["Acknowledged"] > (byte) 0);
          notificationDetails1.set_AcknowledgedBy(str1);
          notificationDetails1.set_AcknowledgedMethod(str2);
          notificationDetails1.set_AcknowledgedTime(((DateTime) dataTable.Rows[0]["AcknowledgedTime"]).ToUniversalTime());
          notificationDetails1.set_AlertDefinitionId(alertDefID);
          notificationDetails1.set_ActiveObject((string) dataTable.Rows[0]["ActiveObject"]);
          notificationDetails1.set_ObjectType((string) dataTable.Rows[0]["ObjectType"]);
          notificationDetails1.set_AlertName((string) dataTable.Rows[0]["AlertName"]);
          notificationDetails1.set_ObjectName((string) dataTable.Rows[0]["ObjectName"]);
          notificationDetails1.set_AlertNotes(dataTable.Rows[0]["AlertNotes"] is DBNull ? string.Empty : (string) dataTable.Rows[0]["AlertNotes"]);
          notificationDetails1.set_Notes(dataTable.Rows[0]["Notes"] is DBNull ? string.Empty : (string) dataTable.Rows[0]["Notes"]);
          notificationDetails1.set_TriggerCount((int) dataTable.Rows[0]["TriggerCount"]);
          notificationDetails1.set_AlertMessage(dataTable.Rows[0]["AlertMessage"] is DBNull ? string.Empty : (string) dataTable.Rows[0]["AlertMessage"]);
          AlertNotificationDetails notificationDetails2 = notificationDetails1;
          DateTime dateTime = (DateTime) dataTable.Rows[0]["TriggerTimeStamp"];
          DateTime universalTime1 = dateTime.ToUniversalTime();
          notificationDetails2.set_TriggerTimeStamp(universalTime1);
          AlertNotificationDetails notificationDetails3 = notificationDetails1;
          dateTime = (DateTime) dataTable.Rows[0]["ResetTimeStamp"];
          DateTime universalTime2 = dateTime.ToUniversalTime();
          notificationDetails3.set_ResetTimeStamp(universalTime2);
          IAlertNotificationSettingsProvider settingsProvider = (IAlertNotificationSettingsProvider) new AlertNotificationSettingsProvider();
          notificationDetails1.set_NotificationSettings(settingsProvider.GetAlertNotificationSettings(notificationDetails1.get_ObjectType(), notificationDetails1.get_AlertName(), dataTable.Rows[0]["NotificationSettings"] is DBNull ? (string) null : (string) dataTable.Rows[0]["NotificationSettings"]));
          notificationDetails1.get_NotificationSettings().set_Enabled((bool) dataTable.Rows[0]["NotifyEnabled"]);
        }
        return notificationDetails1;
      }
      catch (Exception ex)
      {
        AlertDAL.Log.Error((object) string.Format("Error getting alert details for notification for alert {0}", (object) alertDefID), ex);
        throw;
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static AlertNotificationSettings GetBasicAlertNotificationSettings(
      int alertID,
      string netObjectType,
      int propertyID,
      string alertName)
    {
      try
      {
        IAlertNotificationSettingsProvider settingsProvider = (IAlertNotificationSettingsProvider) new AlertNotificationSettingsProvider();
        AlertNotificationSettings notificationSettings = (AlertNotificationSettings) null;
        if (netObjectType.Equals("NetworkNode", StringComparison.OrdinalIgnoreCase))
          netObjectType = "Node";
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertName\r\n                            ,PropertyID\r\n                            ,NotifyEnabled\r\n                            ,NotificationSettings\r\n                    FROM [Alerts]\r\n                    WHERE AlertID = @AlertDefinitionId"))
        {
          textCommand.Parameters.AddWithValue("@AlertDefinitionId", (object) alertID);
          DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand);
          if (dataTable.Rows.Count != 0)
          {
            string str1 = (string) dataTable.Rows[0]["AlertName"];
            int num1 = (int) dataTable.Rows[0]["PropertyID"];
            bool flag = (bool) dataTable.Rows[0]["NotifyEnabled"];
            string str2 = dataTable.Rows[0]["NotificationSettings"] is DBNull ? (string) null : (string) dataTable.Rows[0]["NotificationSettings"];
            int num2 = propertyID;
            if (num1 == num2)
            {
              notificationSettings = settingsProvider.GetAlertNotificationSettings(netObjectType, str1.Trim(), str2);
              notificationSettings.set_Enabled(flag);
            }
          }
          if (notificationSettings == null)
          {
            notificationSettings = settingsProvider.GetDefaultAlertNotificationSettings(netObjectType, alertName);
            notificationSettings.set_Enabled(true);
          }
        }
        return notificationSettings;
      }
      catch (Exception ex)
      {
        AlertDAL.Log.Error((object) string.Format("Error getting basic alert notification settings for alert {0}", (object) alertID), ex);
        throw;
      }
    }

    public static void SetBasicAlertNotificationSettings(
      int alertID,
      AlertNotificationSettings settings)
    {
      try
      {
        string xml = ((IAlertNotificationSettingsConverter) new AlertNotificationSettingsConverter()).ToXml(settings);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE [Alerts] SET\r\n                            NotifyEnabled = @NotifyEnabled,\r\n                            NotificationSettings = @NotificationSettings\r\n                      WHERE AlertID = @AlertDefinitionId"))
        {
          textCommand.Parameters.AddWithValue("@AlertDefinitionId", (object) alertID);
          textCommand.Parameters.AddWithValue("@NotifyEnabled", (object) settings.get_Enabled());
          textCommand.Parameters.AddWithValue("@NotificationSettings", (object) xml);
          SqlHelper.ExecuteNonQuery(textCommand);
        }
      }
      catch (Exception ex)
      {
        AlertDAL.Log.Error((object) string.Format("Error setting basic alert notification settings for alert {0}", (object) alertID), ex);
        throw;
      }
    }

    [Obsolete("Old alerting will be removed")]
    public static bool RevertMigratedAlert(Guid alertRefId, bool enableInOldAlerting)
    {
      string str = "Update Alerts Set Reverted=@Reverted, Enabled=@Enabled WHERE AlertDefID=@AlertDefID";
      int num;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("Update AlertDefinitions Set Reverted=@Reverted, Enabled=@Enabled WHERE AlertDefID=@AlertDefID"))
      {
        textCommand.Parameters.AddRange(new SqlParameter[3]
        {
          new SqlParameter("Reverted", (object) true),
          new SqlParameter("Enabled", (object) enableInOldAlerting),
          new SqlParameter("AlertDefID", (object) alertRefId)
        });
        num = SqlHelper.ExecuteNonQuery(textCommand);
      }
      if (num < 1)
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Parameters.AddRange(new SqlParameter[3]
          {
            new SqlParameter("Reverted", (object) true),
            new SqlParameter("Enabled", (object) enableInOldAlerting),
            new SqlParameter("AlertDefID", (object) alertRefId)
          });
          num = SqlHelper.ExecuteNonQuery(textCommand);
        }
      }
      return num > 0;
    }

    public static int GetAlertObjectId(string alertKey)
    {
      if (string.IsNullOrWhiteSpace(alertKey))
        throw new ArgumentException("Parameter is null or empty", nameof (alertKey));
      string[] strArray = alertKey.Split(':');
      if (strArray.Length != 3)
      {
        string message = string.Format("Error getting alert key parts. Original key: '{0}'", (object) alertKey);
        AlertDAL.Log.Error((object) message);
        throw new ArgumentException(message);
      }
      Guid result1;
      if (!Guid.TryParse(strArray[0], out result1))
      {
        string message = string.Format("Error getting AlertDefId as GUID. Original key: '{0}'", (object) strArray[0]);
        AlertDAL.Log.Error((object) message);
        throw new ArgumentException(message);
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT TOP 1 AlertObjectID FROM AlertStatusView WHERE AlertDefID=@alertDefID AND ActiveObject=@activeObject"))
      {
        SqlParameterCollection parameters1 = textCommand.Parameters;
        SqlParameter sqlParameter1 = new SqlParameter("alertDefID", SqlDbType.UniqueIdentifier);
        sqlParameter1.Value = (object) result1;
        parameters1.Add(sqlParameter1);
        SqlParameterCollection parameters2 = textCommand.Parameters;
        SqlParameter sqlParameter2 = new SqlParameter("activeObject", SqlDbType.NVarChar);
        sqlParameter2.Value = (object) strArray[1];
        parameters2.Add(sqlParameter2);
        object obj = SqlHelper.ExecuteScalar(textCommand);
        int result2;
        if (obj != DBNull.Value && obj != null && int.TryParse(obj.ToString(), out result2))
          return result2;
        AlertDAL.Log.InfoFormat("AlertObjectID for alertKey: '{0}' was not found.", (object) alertKey);
        return 0;
      }
    }
  }
}
