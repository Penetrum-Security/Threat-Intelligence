// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.ActiveAlertDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Contract2;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Alerting.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Federation;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Models.Alerts;
using SolarWinds.Orion.Core.Common.Notification;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.Core.Models.Alerting;
using SolarWinds.Orion.Core.Strings;
using SolarWinds.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class ActiveAlertDAL
  {
    private static readonly Log log = new Log();
    private readonly Lazy<AlertObjectPropertiesProvider> _alertPropertiesProvider;
    private readonly IInformationServiceProxyCreator _swisProxyCreator;
    private readonly IAlertHistoryDAL _alertHistoryDAL;

    public ActiveAlertDAL()
      : this((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator())
    {
    }

    public ActiveAlertDAL(IInformationServiceProxyCreator swisProxyCreator)
      : this(swisProxyCreator, (IAlertHistoryDAL) new AlertHistoryDAL(swisProxyCreator))
    {
    }

    public ActiveAlertDAL(
      IInformationServiceProxyCreator swisProxyCreator,
      IAlertHistoryDAL alertHistoryDAL)
    {
      this._swisProxyCreator = swisProxyCreator;
      this._alertHistoryDAL = alertHistoryDAL;
      this._alertPropertiesProvider = new Lazy<AlertObjectPropertiesProvider>((Func<AlertObjectPropertiesProvider>) (() => new AlertObjectPropertiesProvider(swisProxyCreator)));
      StatusInfo.Init((IStatusInfoProvider) new DefaultStatusInfoProvider(), ActiveAlertDAL.log);
    }

    public int AcknowledgeActiveAlerts(
      IEnumerable<int> alertObjectIds,
      string accountId,
      string notes,
      DateTime acknowledgeDateTime)
    {
      if (!alertObjectIds.Any<int>())
        return 0;
      int num1 = 0;
      bool flag = !string.IsNullOrEmpty(notes);
      string format1 = "UPDATE AlertObjects SET AlertNote = CASE WHEN (AlertNote IS NULL) THEN @alertNote ELSE AlertNote + CHAR(13) + CHAR(10) + @alertNote END WHERE AlertObjectId IN ({0})";
      string format2 = "UPDATE AlertActive SET Acknowledged=1, AcknowledgedBy=@acknowledgedBy, AcknowledgedDateTime=@acknowledgedDateTime" + " WHERE AlertObjectID IN ({0})";
      string empty = string.Empty;
      int num2 = 0;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(format2))
      {
        foreach (int alertObjectId in alertObjectIds)
        {
          string parameterName = string.Format("@alertObjectID{0}", (object) num2++);
          if (!string.IsNullOrEmpty(empty))
            empty += ",";
          if (num2 < 1000)
          {
            textCommand.Parameters.AddWithValue(parameterName, (object) alertObjectId);
            empty += parameterName;
          }
          else
            empty += (string) (object) alertObjectId;
        }
        textCommand.Parameters.AddWithValue("@acknowledgedBy", (object) accountId);
        textCommand.Parameters.AddWithValue("@acknowledgedDateTime", (object) acknowledgeDateTime.ToUniversalTime());
        using (SqlConnection connection = DatabaseFunctions.CreateConnection())
        {
          using (SqlTransaction sqlTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
          {
            textCommand.CommandText = string.Format(format2, (object) empty);
            SqlHelper.ExecuteNonQuery(textCommand, connection, sqlTransaction);
            if (flag)
            {
              textCommand.Parameters.AddWithValue("@alertNote", (object) notes);
              textCommand.CommandText = string.Format(format1, (object) empty);
              SqlHelper.ExecuteNonQuery(textCommand, connection, sqlTransaction);
            }
            textCommand.CommandText = string.Format("SELECT AlertObjectID, AlertActiveID FROM AlertActive WHERE AlertObjectID IN ({0})", (object) empty);
            DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand, connection, (DataTable) null);
            num1 = dataTable.Rows.Count;
            foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
            {
              AlertHistory alertHistory = new AlertHistory();
              alertHistory.set_EventType((EventType) 2);
              alertHistory.set_AccountID(accountId);
              alertHistory.set_Message(notes);
              alertHistory.set_TimeStamp(acknowledgeDateTime.ToUniversalTime());
              int num3 = row["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(row["AlertObjectID"]) : 0;
              long num4 = row["AlertActiveID"] != DBNull.Value ? Convert.ToInt64(row["AlertActiveID"]) : 0L;
              this._alertHistoryDAL.InsertHistoryItem(alertHistory, num4, num3, connection, sqlTransaction);
            }
            sqlTransaction.Commit();
          }
        }
      }
      return num1;
    }

    public static bool UnacknowledgeAlerts(int[] alertObjectIds, string accountId)
    {
      bool flag = true;
      foreach (int alertObjectId in alertObjectIds)
      {
        if (!ActiveAlertDAL.UnacknowledgeAlert(alertObjectId, accountId))
          flag = false;
      }
      return flag;
    }

    private static bool UnacknowledgeAlert(int alertObjectId, string accountId)
    {
      string str = "UPDATE AlertActive SET Acknowledged= null, \r\n                                     AcknowledgedBy=null, \r\n                                     AcknowledgedDateTime = null\r\n                                     WHERE [AlertObjectID] = @alertObjectId";
      AlertHistoryDAL alertHistoryDal = new AlertHistoryDAL();
      int num1 = -1;
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          using (SqlTransaction sqlTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
          {
            textCommand.Parameters.AddWithValue("@alertObjectId", (object) alertObjectId);
            num1 = SqlHelper.ExecuteNonQuery(textCommand, connection, sqlTransaction);
            textCommand.CommandText = "SELECT AlertObjectID, AlertActiveID FROM AlertActive WHERE [AlertObjectID] = @alertObjectId";
            foreach (DataRow row in (InternalDataCollectionBase) SqlHelper.ExecuteDataTable(textCommand, connection, (DataTable) null).Rows)
            {
              int num2 = row["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(row["AlertObjectID"]) : 0;
              long num3 = row["AlertActiveID"] != DBNull.Value ? Convert.ToInt64(row["AlertActiveID"]) : 0L;
              AlertHistory alertHistory = new AlertHistory();
              alertHistory.set_EventType((EventType) 7);
              alertHistory.set_AccountID(accountId);
              alertHistory.set_TimeStamp(DateTime.UtcNow);
              alertHistoryDal.InsertHistoryItem(alertHistory, num3, num2, connection, sqlTransaction);
            }
            sqlTransaction.Commit();
          }
        }
      }
      return num1 == 1;
    }

    private string GetActiveAlertQuery(
      IEnumerable<CustomProperty> customProperties,
      bool includeNotTriggered = false)
    {
      string empty = string.Empty;
      if (customProperties != null)
      {
        StringBuilder stringBuilder = new StringBuilder();
        using (IEnumerator<CustomProperty> enumerator = customProperties.GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            CustomProperty current = enumerator.Current;
            stringBuilder.AppendFormat(", AlertConfigurations.CustomProperties.[{0}]", (object) current.get_PropertyName());
          }
        }
        empty = stringBuilder.ToString();
      }
      string str1 = "CASE WHEN IsNull(AlertObjects.EntityCaption,'') <> '' AND IsNull(AlertObjects.RelatedNodeCaption, '') <> '' AND IsNull(AlertObjects.EntityType, '') <> 'Orion.Nodes'\r\n                                                                        THEN CONCAT(AlertObjects.EntityCaption, '" + Resources.get_TriggeringObjectOnNode() + "', AlertObjects.RelatedNodeCaption) \r\n                                                                        ELSE CASE WHEN IsNull(AlertObjects.EntityCaption,'') <> '' THEN AlertObjects.EntityCaption ELSE AlertObjects.RelatedNodeCaption END\r\n                                                                       END AS ObjectTriggeredThisAlertDisplayName";
      string str2 = " SELECT DISTINCT OrionSite.SiteID, OrionSite.Name AS SiteName,\r\n                                 Data.AlertActiveID, Data.AlertObjectID, Data.Name,\r\n                                Data.AlertConfigurationMessage, Data.Severity, Data.ObjectType, Data.ObjectTriggeredThisAlertDisplayName,\r\n                                Data.EntityUri, Data.EntityType, Data.EntityCaption, Data.EntityDetailsUrl,\r\n                                Data.RelatedNodeUri, Data.RelatedNodeDetailsUrl, Data.RelatedNodeCaption, Data.AlertID, \r\n                                Data.TriggeredDateTime, Data.LastTriggeredDateTime, Data.Message, Data.AccountID, \r\n                                Data.LastExecutedEscalationLevel, Data.AcknowledgedDateTime, Data.Acknowledged, Data.AcknowledgedBy, Data.NumberOfNotes, \r\n                                Data.TriggeredCount, Data.AcknowledgedNote, Data.Canned, Data.Category {1},\r\n                                '' AS IncidentNumber, '' AS IncidentUrl, '' AS AssignedTo\r\n                                FROM (\r\n\r\n                                SELECT AlertActive.InstanceSiteID, AlertActive.InstanceSiteID AS SiteID, AlertActive.AlertActiveID, AlertObjects.AlertObjectID, AlertObjects.AlertObjectID AS ActiveAlertID, AlertConfigurations.Name, AlertConfigurations.Name AS AlertName,\r\n                                AlertConfigurations.AlertMessage AS AlertConfigurationMessage, AlertConfigurations.Severity, AlertConfigurations.Severity AS SeverityOrder, AlertConfigurations.ObjectType,\r\n                                AlertObjects.EntityUri, AlertObjects.EntityUri AS TriggeringObjectEntityUri, AlertObjects.EntityType, AlertObjects.EntityType AS TriggeringObjectEntityName, AlertObjects.EntityCaption, AlertObjects.EntityDetailsUrl,\r\n                                AlertObjects.RelatedNodeUri, AlertObjects.RelatedNodeUri AS RelatedNodeEntityUri, AlertObjects.RelatedNodeDetailsUrl, \r\n                                AlertObjects.RelatedNodeCaption, AlertObjects.RelatedNodeCaption AS RelatedNode, AlertObjects.AlertID, AlertObjects.AlertID AS AlertDefId, \r\n                                AlertActive.TriggeredDateTime, AlertActive.TriggeredDateTime AS TriggerTime,\r\n                                SecondDiff(AlertActive.TriggeredDateTime, getUtcDate()) AS ActiveTimeDisplay, SecondDiff(AlertActive.TriggeredDateTime, getUtcDate()) AS ActiveTimeSort,\r\n                                AlertObjects.LastTriggeredDateTime, AlertActive.TriggeredMessage AS Message,\r\n                                AlertActive.TriggeredMessage AS AlertMessage, AlertActive.AcknowledgedBy AS AccountID, \r\n                                AlertActive.LastExecutedEscalationLevel, AlertActive.AcknowledgedDateTime, AlertActive.AcknowledgedDateTime AS AcknowledgeTime, AlertActive.Acknowledged, AlertActive.AcknowledgedBy, AlertActive.NumberOfNotes, {2},\r\n                                AlertObjects.TriggeredCount, AlertObjects.TriggeredCount AS TriggerCount, AlertObjects.AlertNote as AcknowledgedNote, AlertObjects.AlertNote as Notes, AlertConfigurations.Canned, AlertConfigurations.Category {0}\r\n                                FROM Orion.AlertObjects AlertObjects";
      return string.Format((!includeNotTriggered ? str2 + " INNER JOIN Orion.AlertActive (nolock=true) AlertActive ON AlertObjects.AlertObjectID=AlertActive.AlertObjectID AND AlertObjects.InstanceSiteID=AlertActive.InstanceSiteID" : str2 + " LEFT JOIN Orion.AlertActive (nolock=true) AlertActive ON AlertObjects.AlertObjectID=AlertActive.AlertObjectID AND AlertObjects.InstanceSiteID=AlertActive.InstanceSiteID") + " INNER JOIN Orion.AlertConfigurations (nolock=true) AlertConfigurations ON AlertConfigurations.AlertID=AlertObjects.AlertID AND AlertConfigurations.InstanceSiteID=AlertObjects.InstanceSiteID" + ") AS Data" + " LEFT JOIN Orion.Sites AS OrionSite ON OrionSite.SiteID=Data.InstanceSiteID", (object) empty, (object) empty.Replace("AlertConfigurations.CustomProperties", "Data"), (object) str1);
    }

    private string GetActiveAlertTableByDateQuery()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("SELECT AlertHistory.AlertHistoryID, AlertHistory.TimeStamp, AlertObjects.AlertID, AlertObjects.EntityCaption, AlertActive.AlertObjectID, AlertActive.AlertActiveID, AlertActive.Acknowledged, AlertActive.AcknowledgedBy, AlertActive.AcknowledgedDateTime, AlertConfigurations.ObjectType,");
      stringBuilder.Append(" AlertConfigurations.Name, AlertConfigurations.AlertMessage, AlertConfigurations.AlertRefID, AlertConfigurations.Description, AlertObjects.EntityType, AlertObjects.EntityDetailsUrl, AlertActive.TriggeredDateTime, AlertObjects.EntityUri, AlertActiveObjects.EntityUri as ActiveObjectEntityUri, AlertObjects.RelatedNodeUri,");
      stringBuilder.Append(" Actions.ActionTypeID, AlertConfigurations.LastEdit, AlertConfigurations.Severity, ActionsProperties.PropertyName, ActionsProperties.PropertyValue, AlertActive.AcknowledgedNote, AlertConfigurations.Canned, AlertConfigurations.Category ");
      stringBuilder.Append(" FROM Orion.AlertObjects AlertObjects");
      stringBuilder.Append(" LEFT JOIN Orion.AlertActive (nolock=true) AlertActive ON AlertObjects.AlertObjectID=AlertActive.AlertObjectID");
      stringBuilder.Append(" INNER JOIN Orion.AlertHistory (nolock=true) AlertHistory ON AlertObjects.AlertObjectID=AlertHistory.AlertObjectID");
      stringBuilder.Append(" INNER JOIN Orion.Actions (nolock=true) Actions ON AlertHistory.ActionID = Actions.ActionID");
      stringBuilder.Append(" INNER JOIN Orion.ActionsProperties (nolock=true) ActionsProperties ON Actions.ActionID = ActionsProperties.ActionID");
      stringBuilder.Append(" INNER JOIN Orion.AlertConfigurations (nolock=true) AlertConfigurations ON AlertConfigurations.AlertID=AlertObjects.AlertID");
      stringBuilder.Append(" LEFT JOIN Orion.AlertActiveObjects (nolock=true) AlertActiveObjects ON AlertActiveObjects.AlertActiveID=AlertActive.AlertActiveID");
      stringBuilder.Append(" WHERE Actions.ActionTypeID IN ('PlaySound', 'TextToSpeech') AND ActionsProperties.PropertyName IN ('Message', 'Text') AND (AlertActive.Acknowledged IS NULL OR AlertActive.Acknowledged = false)");
      return stringBuilder.ToString();
    }

    private ActiveAlert GetActiveAlertFromDataRow(
      DataRow rActiveAlert,
      IEnumerable<CustomProperty> customProperties)
    {
      ActiveAlert activeAlert1 = new ActiveAlert();
      activeAlert1.set_CustomProperties(new Dictionary<string, object>());
      activeAlert1.set_TriggeringObjectEntityUri(rActiveAlert["EntityUri"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityUri"]) : string.Empty);
      activeAlert1.set_SiteID(rActiveAlert["SiteID"] != DBNull.Value ? Convert.ToInt32(rActiveAlert["SiteID"]) : -1);
      activeAlert1.set_SiteName(rActiveAlert["SiteName"] != DBNull.Value ? Convert.ToString(rActiveAlert["SiteName"]) : string.Empty);
      string linkPrefix = FederationUrlHelper.GetLinkPrefix(activeAlert1.get_SiteID());
      activeAlert1.set_TriggerDateTime(rActiveAlert["TriggeredDateTime"] != DBNull.Value ? DateTime.SpecifyKind(Convert.ToDateTime(rActiveAlert["TriggeredDateTime"]), DateTimeKind.Utc).ToLocalTime() : DateTime.MinValue);
      activeAlert1.set_ActiveTime(DateTime.Now - activeAlert1.get_TriggerDateTime());
      ActiveAlert activeAlert2 = activeAlert1;
      DateTime dateTime1;
      DateTime dateTime2;
      if (rActiveAlert["LastTriggeredDateTime"] == DBNull.Value)
      {
        dateTime2 = DateTime.MinValue;
      }
      else
      {
        dateTime1 = Convert.ToDateTime(rActiveAlert["LastTriggeredDateTime"]);
        dateTime2 = dateTime1.ToLocalTime();
      }
      activeAlert2.set_LastTriggeredDateTime(dateTime2);
      activeAlert1.set_ActiveTimeDisplay(this.ActiveTimeToDisplayFormat(activeAlert1.get_ActiveTime()));
      activeAlert1.set_TriggeringObjectCaption(rActiveAlert["EntityCaption"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityCaption"]) : string.Empty);
      activeAlert1.set_TriggeringObjectDetailsUrl(linkPrefix + (rActiveAlert["EntityDetailsUrl"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityDetailsUrl"]) : string.Empty));
      activeAlert1.set_TriggeringObjectEntityName(rActiveAlert["EntityType"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityType"]) : string.Empty);
      activeAlert1.set_RelatedNodeCaption(rActiveAlert["RelatedNodeCaption"] != DBNull.Value ? Convert.ToString(rActiveAlert["RelatedNodeCaption"]) : string.Empty);
      activeAlert1.set_RelatedNodeDetailsUrl(linkPrefix + (rActiveAlert["RelatedNodeDetailsUrl"] != DBNull.Value ? Convert.ToString(rActiveAlert["RelatedNodeDetailsUrl"]) : string.Empty));
      activeAlert1.set_RelatedNodeEntityUri(rActiveAlert["RelatedNodeUri"] != DBNull.Value ? Convert.ToString(rActiveAlert["RelatedNodeUri"]) : string.Empty);
      activeAlert1.set_ObjectTriggeredThisAlertDisplayName(rActiveAlert["ObjectTriggeredThisAlertDisplayName"] != DBNull.Value ? Convert.ToString(rActiveAlert["ObjectTriggeredThisAlertDisplayName"]) : string.Empty);
      int num = rActiveAlert["Acknowledged"] == DBNull.Value ? 0 : (Convert.ToBoolean(rActiveAlert["Acknowledged"]) ? 1 : 0);
      activeAlert1.set_Canned(rActiveAlert["Canned"] != DBNull.Value && Convert.ToBoolean(rActiveAlert["Canned"]));
      activeAlert1.set_Category(rActiveAlert["Category"] != DBNull.Value ? Convert.ToString(rActiveAlert["Category"]) : string.Empty);
      if (num != 0)
      {
        activeAlert1.set_AcknowledgedBy(rActiveAlert["AcknowledgedBy"] != DBNull.Value ? Convert.ToString(rActiveAlert["AcknowledgedBy"]) : string.Empty);
        activeAlert1.set_AcknowledgedByFullName(activeAlert1.get_AcknowledgedBy());
        ActiveAlert activeAlert3 = activeAlert1;
        DateTime dateTime3;
        if (rActiveAlert["AcknowledgedDateTime"] == DBNull.Value)
        {
          dateTime3 = DateTime.MinValue;
        }
        else
        {
          dateTime1 = DateTime.SpecifyKind(Convert.ToDateTime(rActiveAlert["AcknowledgedDateTime"]), DateTimeKind.Utc);
          dateTime3 = dateTime1.ToLocalTime();
        }
        activeAlert3.set_AcknowledgedDateTime(dateTime3);
        activeAlert1.set_Notes(rActiveAlert["AcknowledgedNote"] != DBNull.Value ? Convert.ToString(rActiveAlert["AcknowledgedNote"]) : string.Empty);
      }
      activeAlert1.set_NumberOfNotes(rActiveAlert["NumberOfNotes"] != DBNull.Value ? Convert.ToInt32(rActiveAlert["NumberOfNotes"]) : 0);
      activeAlert1.set_Id(rActiveAlert["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(rActiveAlert["AlertObjectID"]) : 0);
      activeAlert1.set_AlertDefId(rActiveAlert["AlertID"] != DBNull.Value ? Convert.ToString(rActiveAlert["AlertID"]) : string.Empty);
      activeAlert1.set_LegacyAlert(false);
      activeAlert1.set_Message(rActiveAlert["Message"] != DBNull.Value ? Convert.ToString(rActiveAlert["Message"]) : string.Empty);
      activeAlert1.set_Name(rActiveAlert["Name"] != DBNull.Value ? Convert.ToString(rActiveAlert["Name"]) : string.Empty);
      activeAlert1.set_ObjectType(rActiveAlert["ObjectType"] != DBNull.Value ? Convert.ToString(rActiveAlert["ObjectType"]) : string.Empty);
      activeAlert1.set_Severity(rActiveAlert["Severity"] != DBNull.Value ? (ActiveAlertSeverity) Convert.ToInt32(rActiveAlert["Severity"]) : (ActiveAlertSeverity) 1);
      activeAlert1.set_TriggeringObjectEntityName(rActiveAlert["EntityType"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityType"]) : string.Empty);
      activeAlert1.set_TriggeringObjectCaption(rActiveAlert["EntityCaption"] != DBNull.Value ? Convert.ToString(rActiveAlert["EntityCaption"]) : string.Empty);
      activeAlert1.set_Status(rActiveAlert["AlertActiveID"] != DBNull.Value ? (ActiveAlertStatus) 1 : (ActiveAlertStatus) 0);
      if (activeAlert1.get_Status() == null)
        activeAlert1.set_ActiveTimeDisplay(Resources.get_LIBCODE_PS0_11());
      activeAlert1.set_RelatedNodeEntityUri(rActiveAlert["RelatedNodeUri"] != DBNull.Value ? Convert.ToString(rActiveAlert["RelatedNodeUri"]) : string.Empty);
      activeAlert1.set_TriggerCount(rActiveAlert["TriggeredCount"] != DBNull.Value ? Convert.ToInt32(rActiveAlert["TriggeredCount"]) : 0);
      activeAlert1.set_EscalationLevel(rActiveAlert["LastExecutedEscalationLevel"] != DBNull.Value ? Convert.ToInt32(rActiveAlert["LastExecutedEscalationLevel"]) : 0);
      activeAlert1.set_IncidentNumber(rActiveAlert["IncidentNumber"] != DBNull.Value ? Convert.ToString(rActiveAlert["IncidentNumber"]) : string.Empty);
      activeAlert1.set_IncidentUrl(rActiveAlert["IncidentUrl"] != DBNull.Value ? Convert.ToString(rActiveAlert["IncidentUrl"]) : string.Empty);
      activeAlert1.set_AssignedTo(rActiveAlert["AssignedTo"] != DBNull.Value ? Convert.ToString(rActiveAlert["AssignedTo"]) : string.Empty);
      this.FillCustomPropertiesFromRow(rActiveAlert, customProperties, activeAlert1);
      return activeAlert1;
    }

    private void FillCustomPropertiesFromRow(
      DataRow rActiveAlert,
      IEnumerable<CustomProperty> customProperties,
      ActiveAlert activeAlert)
    {
      using (IEnumerator<CustomProperty> enumerator = customProperties.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          CustomProperty current = enumerator.Current;
          object obj = (object) null;
          if (rActiveAlert[current.get_PropertyName()] != DBNull.Value)
          {
            if (current.get_PropertyType() == typeof (string))
              obj = (object) Convert.ToString(rActiveAlert[current.get_PropertyName()]);
            else if (current.get_PropertyType() == typeof (DateTime))
              obj = (object) DateTime.SpecifyKind(Convert.ToDateTime(rActiveAlert[current.get_PropertyName()]), DateTimeKind.Local);
            else if (current.get_PropertyType() == typeof (int))
              obj = (object) Convert.ToInt32(rActiveAlert[current.get_PropertyName()]);
            else if (current.get_PropertyType() == typeof (float))
              obj = (object) Convert.ToSingle(rActiveAlert[current.get_PropertyName()]);
            else if (current.get_PropertyType() == typeof (bool))
              obj = (object) Convert.ToBoolean(rActiveAlert[current.get_PropertyName()]);
          }
          activeAlert.get_CustomProperties().Add(current.get_PropertyName(), obj);
        }
      }
    }

    private int GetAlertObjectIdByAlertActiveId(long alertActiveId)
    {
      int num = 0;
      string str = "SELECT TOP 1 AlertObjectID FROM Orion.AlertHistory (nolock=true) WHERE AlertActiveID=@alertActiveID";
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisProxyCreator.Create())
      {
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, str, new Dictionary<string, object>()
        {
          {
            "alertActiveID",
            (object) alertActiveId
          }
        });
        if (dataTable.Rows.Count > 0)
          num = dataTable.Rows[0]["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(dataTable.Rows[0]["AlertObjectID"]) : 0;
      }
      return num;
    }

    private ActiveAlertStatus GetTriggeredStatusForActiveAlert(int alertObjectId)
    {
      return !SqlHelper.ExecuteExistsParams("SELECT AlertActiveID FROM dbo.AlertActive WITH(NOLOCK) WHERE AlertObjectID=@alertObjectId", new SqlParameter[1]
      {
        new SqlParameter("@alertObjectId", (object) alertObjectId)
      }) ? (ActiveAlertStatus) 0 : (ActiveAlertStatus) 1;
    }

    internal DataTable GetAlertResetOrUpdateNotificationPropertiesTableByAlertObjectIds(
      IEnumerable<int> alertObjectIds)
    {
      return this._alertPropertiesProvider.Value.GetAlertNotificationProperties(alertObjectIds);
    }

    private string GetQueryWithViewLimitations(string query, int viewLimitationId)
    {
      return viewLimitationId != 0 ? string.Format("{0} WITH LIMITATION {1}", (object) query, (object) viewLimitationId) : query;
    }

    private IEnumerable<string> GetUrisForGlobalAlert(int id, bool federationEnabled)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2_1 = this._swisProxyCreator.Create())
      {
        IInformationServiceProxy2 iinformationServiceProxy2_2 = iinformationServiceProxy2_1;
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary.Add("objectId", (object) id);
        int num = federationEnabled ? 1 : 0;
        return (IEnumerable<string>) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2_2, "SELECT a.AlertActiveObjects.EntityUri\r\n                                              FROM Orion.AlertActive (nolock=true) AS a\r\n                                              WHERE a.AlertObjectID=@objectId", dictionary, num != 0).AsEnumerable().Where<DataRow>((Func<DataRow, bool>) (item => item["EntityUri"] != DBNull.Value)).Select<DataRow, string>((Func<DataRow, string>) (item => Convert.ToString(item["EntityUri"])));
      }
    }

    public ActiveAlert GetActiveAlert(
      int alertObjectId,
      IEnumerable<int> limitationIDs,
      bool includeNotTriggered = true)
    {
      ActiveAlert activeAlert = (ActiveAlert) null;
      IEnumerable<CustomProperty> propertiesForEntity = CustomPropertyMgr.GetCustomPropertiesForEntity("Orion.AlertConfigurationsCustomProperties");
      string str1 = this.GetActiveAlertQuery(propertiesForEntity, includeNotTriggered) + " WHERE Data.AlertObjectID=@alertObjectId";
      using (IInformationServiceProxy2 swisProxy = this._swisProxyCreator.Create())
      {
        DataTable dataTable1 = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) swisProxy, str1, new Dictionary<string, object>()
        {
          {
            nameof (alertObjectId),
            (object) alertObjectId
          }
        });
        if (dataTable1.Rows.Count > 0)
        {
          activeAlert = this.GetActiveAlertFromDataRow(dataTable1.Rows[0], propertiesForEntity);
          AlertIncidentCache.Build(swisProxy, new int?(activeAlert.get_Id()), false).FillIncidentInfo(activeAlert);
          if (!string.IsNullOrEmpty(activeAlert.get_RelatedNodeEntityUri()))
          {
            string str2 = "SELECT NodeID, Status FROM Orion.Nodes (nolock=true) WHERE Uri=@uri";
            DataTable dataTable2 = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) swisProxy, str2, new Dictionary<string, object>()
            {
              {
                "uri",
                (object) activeAlert.get_RelatedNodeEntityUri()
              }
            });
            if (dataTable2.Rows.Count > 0)
            {
              activeAlert.set_RelatedNodeID(dataTable2.Rows[0]["NodeID"] != DBNull.Value ? Convert.ToInt32(dataTable2.Rows[0]["NodeID"]) : 0);
              activeAlert.set_RelatedNodeStatus(dataTable2.Rows[0]["Status"] != DBNull.Value ? Convert.ToInt32(dataTable2.Rows[0]["Status"]) : 0);
              activeAlert.set_RelatedNodeDetailsUrl(string.Format("/Orion/View.aspx?NetObject=N:{0}", (object) activeAlert.get_RelatedNodeID()));
            }
          }
          if (activeAlert.get_TriggeringObjectEntityName() == "Orion.Nodes")
            activeAlert.set_ActiveNetObject(Convert.ToString(activeAlert.get_RelatedNodeID()));
          if (!string.IsNullOrEmpty(activeAlert.get_TriggeringObjectEntityUri()))
          {
            string str2 = "SELECT TME.Status, TME.Uri FROM System.ManagedEntity (nolock=true) TME WHERE TME.Uri=@uri";
            DataTable dataTable2 = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) swisProxy, str2, new Dictionary<string, object>()
            {
              {
                "uri",
                (object) activeAlert.get_TriggeringObjectEntityUri()
              }
            });
            if (dataTable2.Rows.Count > 0)
              activeAlert.set_TriggeringObjectStatus(dataTable2.Rows[0]["Status"] != DBNull.Value ? Convert.ToInt32(dataTable2.Rows[0]["Status"]) : 0);
          }
          else
            activeAlert.set_TriggeringObjectStatus(this.GetRollupStatusForGlobalAlert(activeAlert.get_TriggeringObjectEntityName(), alertObjectId, false));
          activeAlert.set_Status(this.GetTriggeredStatusForActiveAlert(alertObjectId));
        }
      }
      return activeAlert;
    }

    private int GetRollupStatusForGlobalAlert(
      string entity,
      int alertObjectId,
      bool federationEnabled = false)
    {
      if (!this.EntityHasStatusProperty(entity, federationEnabled))
        return 0;
      IEnumerable<string> urisForGlobalAlert = this.GetUrisForGlobalAlert(alertObjectId, federationEnabled);
      if (!urisForGlobalAlert.Any<string>())
        return 0;
      List<int> statuses = new List<int>();
      StringBuilder sbCondition = new StringBuilder();
      Action<Dictionary<string, object>> action = (Action<Dictionary<string, object>>) (swqlParameters =>
      {
        string str = string.Format("SELECT Status FROM {0} (nolock=true) WHERE {1}", (object) entity, (object) sbCondition);
        using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisProxyCreator.Create())
        {
          foreach (DataRow row in (InternalDataCollectionBase) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, str, swqlParameters, federationEnabled).Rows)
          {
            int result = 0;
            if (row["Status"] == DBNull.Value || !int.TryParse(Convert.ToString(row["Status"]), out result))
              result = 0;
            statuses.Add(result);
          }
        }
      });
      int num = 0;
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      foreach (string str in urisForGlobalAlert)
      {
        if (num > 0)
          sbCondition.Append(" OR ");
        sbCondition.AppendFormat("Uri=@uri{0}", (object) num);
        dictionary.Add(string.Format("uri{0}", (object) num), (object) str);
        ++num;
        if (num % 1000 == 0)
        {
          action(dictionary);
          dictionary = new Dictionary<string, object>();
          sbCondition.Clear();
          num = 0;
        }
      }
      if (num > 0)
        action(dictionary);
      return StatusInfo.RollupStatus((IEnumerable<int>) statuses, (EvaluationMethod) 2);
    }

    private bool EntityHasStatusProperty(string entity, bool federationEnabled = false)
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2_1 = this._swisProxyCreator.Create())
      {
        IInformationServiceProxy2 iinformationServiceProxy2_2 = iinformationServiceProxy2_1;
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary.Add("entityName", (object) entity);
        int num = federationEnabled ? 1 : 0;
        return InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2_2, "SELECT Name FROM Metadata.Property WHERE EntityName=@entityName AND Name='Status'", dictionary, num != 0).Rows.Count > 0;
      }
    }

    public Tuple<IEnumerable<ActiveAlert>, int> GetActiveAlerts(
      IEnumerable<CustomProperty> customProperties,
      IEnumerable<int> limitationIDs,
      out List<ErrorMessage> errors,
      bool federationEnabled,
      bool includeNotTriggered = false,
      bool showAcknowledgedAlerts = true,
      bool showOnlyAcknowledgedAlerts = false,
      string relatedNodeEntityUri = "",
      string filterStatement = "",
      string orderByClause = "",
      int fromRow = 1,
      int toRow = 2147483647)
    {
      List<ActiveAlert> activeAlertList = new List<ActiveAlert>();
      string query = this.GetActiveAlertQuery(customProperties, includeNotTriggered) + " WHERE 1=1";
      if (!string.IsNullOrEmpty(filterStatement))
        query = query + " AND (" + filterStatement + ")";
      if (OrionConfiguration.get_IsDemoServer())
        query += " AND (Data.TriggeredDateTime <= GETUTCDATE())";
      if (!string.IsNullOrEmpty(relatedNodeEntityUri))
        query = query + " AND RelatedNodeEntityUri='" + relatedNodeEntityUri + "'";
      if (!showAcknowledgedAlerts)
        query = !showOnlyAcknowledgedAlerts ? query + " AND (AcknowledgedBy IS NULL OR AcknowledgedBy = '')" : query + " AND (AcknowledgedBy IS NOT NULL AND AcknowledgedBy <> '')";
      if (string.IsNullOrEmpty(orderByClause))
        orderByClause = "AlertObjectID";
      if (fromRow != 1 || toRow != int.MaxValue)
        query += string.Format(" ORDER BY {0} WITH ROWS {1} TO {2} WITH TOTALROWS", (object) orderByClause, (object) fromRow, (object) toRow);
      string withViewLimitations = this.GetQueryWithViewLimitations(query, Limitation.GetOnlyViewLimitation(limitationIDs, AccountContext.GetAccountID(), this._swisProxyCreator, federationEnabled));
      int num = 0;
      using (IInformationServiceProxy2 swisProxy = this._swisProxyCreator.Create())
      {
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) swisProxy, withViewLimitations, federationEnabled);
        errors = FederatedResultTableParser.GetErrorsFromDataTable(dataTable);
        num = Convert.ToInt32(dataTable.ExtendedProperties[(object) "TotalRows"]);
        AlertIncidentCache alertIncidentCache = AlertIncidentCache.Build(swisProxy, new int?(), false);
        foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
        {
          ActiveAlert alertFromDataRow = this.GetActiveAlertFromDataRow(row, customProperties);
          alertIncidentCache.FillIncidentInfo(alertFromDataRow);
          if (string.IsNullOrEmpty(alertFromDataRow.get_TriggeringObjectEntityUri()))
            alertFromDataRow.set_TriggeringObjectStatus(this.GetRollupStatusForGlobalAlert(alertFromDataRow.get_TriggeringObjectEntityName(), alertFromDataRow.get_Id(), false));
          activeAlertList.Add(alertFromDataRow);
        }
      }
      return new Tuple<IEnumerable<ActiveAlert>, int>((IEnumerable<ActiveAlert>) activeAlertList, num > 0 ? num : activeAlertList.Count);
    }

    public List<ActiveAlertDetailed> GetAlertTableByDate(
      DateTime dateTime,
      int? lastAlertHistoryId,
      List<int> limitationIDs)
    {
      List<ActiveAlert> activeAlertList = new List<ActiveAlert>();
      StringBuilder stringBuilder = new StringBuilder(this.GetActiveAlertTableByDateQuery());
      object obj;
      if (lastAlertHistoryId.HasValue)
      {
        obj = (object) lastAlertHistoryId.Value;
        stringBuilder.Append("AND (AlertHistory.AlertHistoryID > @param)");
      }
      else
      {
        obj = (object) dateTime;
        stringBuilder.Append("AND (AlertHistory.TimeStamp > @param)");
      }
      string withViewLimitations = this.GetQueryWithViewLimitations(stringBuilder.ToString(), Limitation.GetOnlyViewLimitation((IEnumerable<int>) limitationIDs, AccountContext.GetAccountID(), this._swisProxyCreator, false));
      using (IInformationServiceProxy2 iinformationServiceProxy2_1 = this._swisProxyCreator.Create())
      {
        IInformationServiceProxy2 iinformationServiceProxy2_2 = iinformationServiceProxy2_1;
        string str = withViewLimitations;
        foreach (DataRow row in (InternalDataCollectionBase) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2_2, str, new Dictionary<string, object>()
        {
          {
            "param",
            obj
          }
        }).Rows)
        {
          ActiveAlertDetailed activeAlertDetailed1 = new ActiveAlertDetailed();
          ((ActiveAlert) activeAlertDetailed1).set_Id(row["AlertActiveID"] != DBNull.Value ? Convert.ToInt32(row["AlertActiveID"]) : 0);
          activeAlertDetailed1.set_AlertHistoryId(row["AlertHistoryID"] != DBNull.Value ? Convert.ToInt32(row["AlertHistoryID"]) : 0);
          ((ActiveAlert) activeAlertDetailed1).set_AlertDefId(row["AlertID"] != DBNull.Value ? Convert.ToString(row["AlertID"]) : string.Empty);
          activeAlertDetailed1.set_AlertRefID(row["AlertRefID"] != DBNull.Value ? new Guid(Convert.ToString(row["AlertRefID"])) : Guid.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_ActiveNetObject(row["AlertObjectID"] != DBNull.Value ? Convert.ToString(row["AlertObjectID"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_ObjectType(row["ObjectType"] != DBNull.Value ? Convert.ToString(row["ObjectType"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_Name(row["Name"] != DBNull.Value ? Convert.ToString(row["Name"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_TriggeringObjectDetailsUrl(row["EntityDetailsUrl"] != DBNull.Value ? Convert.ToString(row["EntityDetailsUrl"]) : string.Empty);
          DateTime dateTime1 = row["TriggeredDateTime"] != DBNull.Value ? DateTime.SpecifyKind(Convert.ToDateTime(row["TriggeredDateTime"]), DateTimeKind.Utc) : (row["TimeStamp"] != DBNull.Value ? Convert.ToDateTime(row["TimeStamp"]) : DateTime.MinValue);
          ((ActiveAlert) activeAlertDetailed1).set_TriggerDateTime(dateTime1.ToLocalTime());
          ((ActiveAlert) activeAlertDetailed1).set_TriggeringObjectEntityUri(row["EntityUri"] != DBNull.Value ? Convert.ToString(row["EntityUri"]) : (row["ActiveObjectEntityUri"] != DBNull.Value ? Convert.ToString(row["ActiveObjectEntityUri"]) : string.Empty));
          ((ActiveAlert) activeAlertDetailed1).set_RelatedNodeEntityUri(row["RelatedNodeUri"] != DBNull.Value ? Convert.ToString(row["RelatedNodeUri"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_TriggeringObjectEntityName(row["EntityType"] != DBNull.Value ? Convert.ToString(row["EntityType"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_Message(row["PropertyValue"] != DBNull.Value ? Convert.ToString(row["PropertyValue"]) : string.Empty);
          ((ActiveAlert) activeAlertDetailed1).set_TriggeringObjectCaption(row["EntityCaption"] != DBNull.Value ? Convert.ToString(row["EntityCaption"]) : string.Empty);
          activeAlertDetailed1.set_ActionType(row["ActionTypeID"] != DBNull.Value ? Convert.ToString(row["ActionTypeID"]) : string.Empty);
          activeAlertDetailed1.set_AlertDefDescription(row["Description"] != DBNull.Value ? Convert.ToString(row["Description"]) : string.Empty);
          activeAlertDetailed1.set_AlertDefLastEdit(row["LastEdit"] != DBNull.Value ? DateTime.SpecifyKind(Convert.ToDateTime(row["LastEdit"]), DateTimeKind.Utc) : DateTime.MinValue);
          activeAlertDetailed1.set_AlertDefSeverity(row["Severity"] != DBNull.Value ? Convert.ToInt32(row["Severity"]) : 2);
          ((ActiveAlert) activeAlertDetailed1).set_Severity(row["Severity"] != DBNull.Value ? (ActiveAlertSeverity) Convert.ToInt32(row["Severity"]) : (ActiveAlertSeverity) 2);
          activeAlertDetailed1.set_AlertDefMessage(row["AlertMessage"] != DBNull.Value ? Convert.ToString(row["AlertMessage"]) : string.Empty);
          ActiveAlertDetailed activeAlertDetailed2 = activeAlertDetailed1;
          if ((row["Acknowledged"] != DBNull.Value ? (Convert.ToBoolean(row["Acknowledged"]) ? 1 : 0) : 0) != 0)
          {
            ((ActiveAlert) activeAlertDetailed2).set_AcknowledgedBy(row["AcknowledgedBy"] != DBNull.Value ? Convert.ToString(row["AcknowledgedBy"]) : string.Empty);
            ((ActiveAlert) activeAlertDetailed2).set_AcknowledgedByFullName(((ActiveAlert) activeAlertDetailed2).get_AcknowledgedBy());
            ActiveAlertDetailed activeAlertDetailed3 = activeAlertDetailed2;
            DateTime dateTime2;
            if (row["AcknowledgedDateTime"] == DBNull.Value)
            {
              dateTime2 = DateTime.MinValue;
            }
            else
            {
              dateTime1 = DateTime.SpecifyKind(Convert.ToDateTime(row["AcknowledgedDateTime"]), DateTimeKind.Utc);
              dateTime2 = dateTime1.ToLocalTime();
            }
            ((ActiveAlert) activeAlertDetailed3).set_AcknowledgedDateTime(dateTime2);
            ((ActiveAlert) activeAlertDetailed2).set_Notes(row["AcknowledgedNote"] != DBNull.Value ? Convert.ToString(row["AcknowledgedNote"]) : string.Empty);
          }
          activeAlertList.Add((ActiveAlert) activeAlertDetailed2);
        }
      }
      return ((IEnumerable) activeAlertList).Cast<ActiveAlertDetailed>().ToList<ActiveAlertDetailed>();
    }

    public int ClearTriggeredActiveAlerts(IEnumerable<int> alertObjectIds, string accountId)
    {
      if (!alertObjectIds.Any<int>())
        return 0;
      string empty = string.Empty;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Empty))
      {
        using (SqlConnection connection = DatabaseFunctions.CreateConnection())
        {
          using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
          {
            textCommand.Transaction = transaction;
            for (int index = 0; index < alertObjectIds.Count<int>(); ++index)
            {
              if (index < 1000)
              {
                string parameterName = string.Format("@alertObjectID{0}", (object) index);
                textCommand.Parameters.AddWithValue(parameterName, (object) alertObjectIds.ElementAt<int>(index));
                if (empty != string.Empty)
                  empty += ",";
                empty += parameterName;
              }
              else
              {
                if (empty != string.Empty)
                  empty += ",";
                empty += (string) (object) alertObjectIds.ElementAt<int>(index);
              }
            }
            textCommand.CommandText = string.Format("SELECT AlertObjectID, AlertActiveID FROM AlertActive WHERE [AlertObjectID] IN ({0})", (object) empty);
            foreach (DataRow row in (InternalDataCollectionBase) SqlHelper.ExecuteDataTable(textCommand, connection, (DataTable) null).Rows)
            {
              int num1 = row["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(row["AlertObjectID"]) : 0;
              long num2 = row["AlertActiveID"] != DBNull.Value ? Convert.ToInt64(row["AlertActiveID"]) : 0L;
              AlertHistory alertHistory = new AlertHistory();
              alertHistory.set_EventType((EventType) 8);
              alertHistory.set_AccountID(accountId);
              alertHistory.set_TimeStamp(DateTime.UtcNow);
              this._alertHistoryDAL.InsertHistoryItem(alertHistory, num2, num1, connection, transaction);
            }
            foreach (int alertObjectId in alertObjectIds)
              this.UpdateAlertCaptionAfterReset(alertObjectId, transaction);
            textCommand.CommandText = string.Format("DELETE FROM AlertActive WHERE AlertObjectID IN ({0})", (object) empty);
            int num = SqlHelper.ExecuteNonQuery(textCommand, connection, transaction);
            transaction.Commit();
            return num;
          }
        }
      }
    }

    public IEnumerable<AlertClearedNotificationProperties> GetAlertClearedNotificationPropertiesByAlertObjectIds(
      IEnumerable<int> alertObjectIds)
    {
      if (!alertObjectIds.Any<int>())
        return Enumerable.Empty<AlertClearedNotificationProperties>();
      List<AlertClearedNotificationProperties> notificationPropertiesList = new List<AlertClearedNotificationProperties>();
      foreach (DataRow row in (InternalDataCollectionBase) this.GetAlertResetOrUpdateNotificationPropertiesTableByAlertObjectIds(alertObjectIds).Rows)
      {
        AlertClearedNotificationProperties notificationProperties = new AlertClearedNotificationProperties();
        notificationProperties.set_ClearedTime(DateTime.UtcNow);
        AlertSeverity alertSeverity = row["Severity"] != DBNull.Value ? (AlertSeverity) Convert.ToInt32(row["Severity"]) : (AlertSeverity) 2;
        ((AlertNotificationProperties) notificationProperties).set_AlertId(row["AlertID"] != DBNull.Value ? Convert.ToInt32(row["AlertID"]) : 0);
        ((AlertNotificationProperties) notificationProperties).set_AlertName(row["Name"] != DBNull.Value ? Convert.ToString(row["Name"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_AlertObjectId(row["AlertObjectID"] != DBNull.Value ? Convert.ToInt32(row["AlertObjectID"]) : 0);
        ((AlertNotificationProperties) notificationProperties).set_AlertDefinitionId(row["AlertRefID"] != DBNull.Value ? new Guid(Convert.ToString(row["AlertRefID"])) : Guid.Empty);
        ((AlertNotificationProperties) notificationProperties).set_DetailsUrl(row["EntityDetailsUrl"] != DBNull.Value ? Convert.ToString(row["EntityDetailsUrl"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_Message(row["TriggeredMessage"] != DBNull.Value ? Convert.ToString(row["TriggeredMessage"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_ObjectId(row["EntityUri"] != DBNull.Value ? Convert.ToString(row["EntityUri"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_ObjectName(row["EntityType"] != DBNull.Value ? Convert.ToString(row["EntityType"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_NetObject(row["EntityNetObjectId"] != DBNull.Value ? Convert.ToString(row["EntityNetObjectId"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_EntityCaption(row["EntityCaption"] != DBNull.Value ? Convert.ToString(row["EntityCaption"]) : string.Empty);
        ((AlertNotificationProperties) notificationProperties).set_ObjectType(((AlertNotificationProperties) notificationProperties).get_ObjectName());
        ((AlertNotificationProperties) notificationProperties).set_TriggerTimeStamp(row["TriggeredDateTime"] != DBNull.Value ? DateTime.SpecifyKind(Convert.ToDateTime(row["TriggeredDateTime"]), DateTimeKind.Utc) : DateTime.MinValue);
        notificationProperties.set_ObjectUris(string.IsNullOrWhiteSpace(((AlertNotificationProperties) notificationProperties).get_ObjectId()) ? this.GetGlobalAlertRelatedUris(((AlertNotificationProperties) notificationProperties).get_AlertId()) : (IEnumerable<string>) new List<string>());
        ((AlertNotificationProperties) notificationProperties).set_Severity(alertSeverity.ToString());
        notificationPropertiesList.Add(notificationProperties);
      }
      return (IEnumerable<AlertClearedNotificationProperties>) notificationPropertiesList;
    }

    private IEnumerable<string> GetGlobalAlertRelatedUris(int AlertID)
    {
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT ObjectId FROM [AlertConditionState] WHERE [AlertID]=@alertID AND [Type] = 0 AND [Resolved] = 1"))
      {
        textCommand.Parameters.AddWithValue("alertID", (object) AlertID);
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      List<string> stringList = new List<string>();
      foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
        stringList.Add(row[0].ToString());
      return (IEnumerable<string>) stringList;
    }

    public IEnumerable<AlertUpdatedNotification> GetAlertUpdatedNotificationPropertiesByAlertObjectIds(
      IEnumerable<int> alertObjectIds,
      string accountId,
      string notes,
      DateTime acknowledgedDateTime,
      bool acknowledge)
    {
      return !alertObjectIds.Any<int>() ? Enumerable.Empty<AlertUpdatedNotification>() : this._alertPropertiesProvider.Value.GetAlertUpdatedNotificationProperties(alertObjectIds, accountId, notes, acknowledgedDateTime, acknowledge);
    }

    public IEnumerable<AlertUpdatedNotification> GetAlertUpdatedNotificationPropertiesByAlertObjectIds(
      IEnumerable<int> alertObjectIds,
      string accountId,
      string notes,
      DateTime acknowledgedDateTime,
      bool acknowledge,
      string method)
    {
      return !alertObjectIds.Any<int>() ? Enumerable.Empty<AlertUpdatedNotification>() : this._alertPropertiesProvider.Value.GetAlertUpdatedNotificationProperties(alertObjectIds, accountId, notes, acknowledgedDateTime, acknowledge, method);
    }

    public IEnumerable<int> LimitAlertAckStateUpdateCandidates(
      IEnumerable<int> alertObjectIds,
      bool requestedAcknowledgedState)
    {
      return !alertObjectIds.Any<int>() ? Enumerable.Empty<int>() : (IEnumerable<int>) this.GetAlertResetOrUpdateNotificationPropertiesTableByAlertObjectIds(alertObjectIds).Rows.Cast<DataRow>().Where<DataRow>((Func<DataRow, bool>) (row => row["AlertObjectId"] != DBNull.Value)).Select(row => new
      {
        row = row,
        ackState = row["Acknowledged"] != DBNull.Value && Convert.ToBoolean(row["Acknowledged"])
      }).Where(_param1 => _param1.ackState != requestedAcknowledgedState).Select(_param1 => Convert.ToInt32(_param1.row["AlertObjectId"])).ToList<int>();
    }

    internal string ActiveTimeToDisplayFormat(TimeSpan activeTime)
    {
      string str = string.Empty;
      if (activeTime.Days > 0)
        str = str + (object) activeTime.Days + Resources.get_WEBCODE_PS0_30() + " ";
      if (activeTime.Hours > 0)
        str = str + (object) activeTime.Hours + Resources.get_WEBCODE_PS0_31() + " ";
      if (activeTime.Minutes > 0)
        str = str + (object) activeTime.Minutes + Resources.get_WEBCODE_PS0_32() + " ";
      return str;
    }

    public ActiveAlertObjectPage GetPageableActiveAlertObjects(
      PageableActiveAlertObjectRequest request)
    {
      ActiveAlertObjectPage activeAlertObjectPage = new ActiveAlertObjectPage();
      List<ActiveAlertObject> objects = new List<ActiveAlertObject>();
      int int32;
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisProxyCreator.Create())
      {
        string withViewLimitations = this.GetQueryWithViewLimitations(string.Format("SELECT a.AlertActiveObjects.EntityUri, a.AlertActiveObjects.EntityCaption, a.AlertActiveObjects.EntityDetailsUrl,\r\n                                             a.AcknowledgedBy, a.TriggeredDateTime, SecondDiff(a.TriggeredDateTime, getUtcDate()) AS ActiveTime\r\n                                              FROM Orion.AlertActive (nolock=true) AS a\r\n                                              WHERE a.AlertObjectID=@objectId\r\n                                              {0}", !string.IsNullOrEmpty(request.get_OrderByClause()) ? (object) ("ORDER BY " + request.get_OrderByClause()) : (object) "ORDER BY AlertObjectID") + string.Format(" WITH ROWS {0} TO {1} WITH TOTALROWS", (object) (request.get_StartRow() + 1), (object) (request.get_StartRow() + request.get_PageSize() + 1)), Limitation.GetOnlyViewLimitation(request.get_LimitationIDs(), AccountContext.GetAccountID(), this._swisProxyCreator, false));
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, withViewLimitations, new Dictionary<string, object>()
        {
          {
            "objectId",
            (object) request.get_AlertObjectId()
          }
        });
        foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
        {
          if (row["EntityUri"] != DBNull.Value)
          {
            string str1 = Convert.ToString(row["EntityUri"]);
            string str2 = row["EntityCaption"] != DBNull.Value ? Convert.ToString(row["EntityCaption"]) : "";
            string str3 = row["EntityDetailsUrl"] != DBNull.Value ? Convert.ToString(row["EntityDetailsUrl"]) : "";
            ActiveAlertObject activeAlertObject1 = new ActiveAlertObject();
            activeAlertObject1.set_Caption(str2);
            activeAlertObject1.set_Uri(str1);
            activeAlertObject1.set_DetailsUrl(str3);
            activeAlertObject1.set_Entity(request.get_TriggeringEntityName());
            ActiveAlertObject activeAlertObject2 = activeAlertObject1;
            objects.Add(activeAlertObject2);
          }
        }
        int32 = Convert.ToInt32(dataTable.ExtendedProperties[(object) "TotalRows"]);
      }
      this.FillAlertObjectStatus(request.get_TriggeringEntityName(), objects);
      activeAlertObjectPage.set_TotalRow(int32);
      activeAlertObjectPage.set_ActiveAlertObjects((IEnumerable<ActiveAlertObject>) objects);
      return activeAlertObjectPage;
    }

    private void FillAlertObjectStatus(string entity, List<ActiveAlertObject> objects)
    {
      if (!((IEnumerable<ActiveAlertObject>) objects).Any<ActiveAlertObject>() || !this.EntityHasStatusProperty(entity, false))
        return;
      Dictionary<string, ActiveAlertObject> lookupUri = ((IEnumerable<ActiveAlertObject>) objects).ToDictionary<ActiveAlertObject, string, ActiveAlertObject>((Func<ActiveAlertObject, string>) (item => item.get_Uri()), (Func<ActiveAlertObject, ActiveAlertObject>) (item => item));
      StringBuilder sbCondition = new StringBuilder();
      Action<Dictionary<string, object>> action = (Action<Dictionary<string, object>>) (swqlParameters =>
      {
        string str = string.Format("SELECT Status, Uri FROM {0} (nolock=true) WHERE {1}", (object) entity, (object) sbCondition);
        using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisProxyCreator.Create())
        {
          foreach (DataRow row in (InternalDataCollectionBase) InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, str, swqlParameters).Rows)
          {
            int num = row["Status"] != DBNull.Value ? Convert.ToInt32(row["Status"]) : 0;
            ActiveAlertObject activeAlertObject;
            if (lookupUri.TryGetValue(row["Uri"] != DBNull.Value ? Convert.ToString(row["Uri"]) : "", out activeAlertObject))
              activeAlertObject.set_Status(num);
          }
        }
      });
      int num1 = 0;
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      using (List<ActiveAlertObject>.Enumerator enumerator = objects.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ActiveAlertObject current = enumerator.Current;
          if (num1 > 0)
            sbCondition.Append(" OR ");
          sbCondition.AppendFormat("Uri=@uri{0}", (object) num1);
          dictionary.Add(string.Format("uri{0}", (object) num1), (object) current.get_Uri());
          ++num1;
          if (num1 % 1000 == 0)
          {
            action(dictionary);
            dictionary = new Dictionary<string, object>();
            sbCondition.Clear();
            num1 = 0;
          }
        }
      }
      if (num1 <= 0)
        return;
      action(dictionary);
    }

    private bool UpdateAlertCaptionAfterReset(int alertObjectId, SqlTransaction transaction)
    {
      string str1;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT TOP 1 [EntityType] FROM [AlertObjects]\r\n                WHERE [AlertObjectID] = @alertObjectId\r\n                AND [EntityUri] IS NULL"))
      {
        textCommand.Parameters.AddWithValue(nameof (alertObjectId), (object) alertObjectId);
        object obj = SqlHelper.ExecuteScalar(textCommand, transaction.Connection, transaction);
        if (obj == DBNull.Value)
          return false;
        str1 = obj as string;
        if (string.IsNullOrWhiteSpace(str1))
          return false;
      }
      string str2 = "SELECT DisplayNamePlural FROM Metadata.Entity WHERE FullName = @entityName";
      string str3;
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisProxyCreator.Create())
      {
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, str2, new Dictionary<string, object>()
        {
          {
            "entityName",
            (object) str1
          }
        });
        if (dataTable.Rows.Count <= 0)
          return false;
        str3 = string.Format("{0} {1}", (object) 0, (object) (dataTable.Rows[0][0] as string));
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("UPDATE [AlertObjects] SET EntityCaption = '{0}' \r\n                                                                           WHERE EntityUri IS NULL AND AlertObjectID = @alertObjectId", (object) str3)))
      {
        textCommand.Parameters.AddWithValue(nameof (alertObjectId), (object) alertObjectId);
        return SqlHelper.ExecuteNonQuery(textCommand, transaction.Connection, transaction) == 1;
      }
    }

    public string GetAlertNote(int alertObjectId)
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT AlertNote FROM AlertObjects WHERE AlertObjectID = @alertObjectId");
        textCommand.Parameters.AddWithValue("@alertObjectId", (object) alertObjectId);
        DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand, connection, (DataTable) null);
        return dataTable.Rows.Count > 0 ? dataTable.Rows[0][0] as string : string.Empty;
      }
    }

    public bool SetAlertNote(
      int alertObjectId,
      string accountId,
      string note,
      DateTime modificationDateTime)
    {
      if (alertObjectId < 0 || string.IsNullOrEmpty(accountId))
        return false;
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction sqlTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
        {
          using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE AlertObjects SET AlertNote = @AlertNote WHERE AlertObjectId=@alertObjectId"))
          {
            textCommand.Parameters.AddWithValue("@alertObjectId", (object) alertObjectId);
            textCommand.Parameters.AddWithValue("@AlertNote", (object) note);
            SqlHelper.ExecuteNonQuery(textCommand, connection, sqlTransaction);
            textCommand.CommandText = "SELECT AlertActiveId FROM AlertActive WHERE AlertObjectId=@alertObjectId";
            object obj = SqlHelper.ExecuteScalar(textCommand, connection);
            AlertHistory alertHistory = new AlertHistory();
            int num = 0;
            if (obj != null && obj != DBNull.Value)
              num = Convert.ToInt32(obj);
            alertHistory.set_EventType((EventType) 3);
            alertHistory.set_AccountID(accountId);
            alertHistory.set_Message(note);
            alertHistory.set_TimeStamp(modificationDateTime.ToUniversalTime());
            this._alertHistoryDAL.InsertHistoryItem(alertHistory, (long) num, alertObjectId, connection, sqlTransaction);
          }
          sqlTransaction.Commit();
          return true;
        }
      }
    }
  }
}
