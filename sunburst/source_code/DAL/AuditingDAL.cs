// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.AuditingDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Auditing;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Auditing;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class AuditingDAL
  {
    protected static readonly Log log = new Log();
    private static readonly IInformationServiceProxyCreator creator = (IInformationServiceProxyCreator) SwisConnectionProxyPool.GetCreator();
    protected readonly object locker = new object();
    protected readonly Dictionary<AuditActionType, int> actionTypes = new Dictionary<AuditActionType, int>();

    public List<AuditActionTypeInfo> GetAuditingActionTypes()
    {
      lock (this.locker)
      {
        if (this.actionTypes.Count == 0)
          this.LoadKeys();
        return ((IEnumerable<KeyValuePair<AuditActionType, int>>) this.actionTypes).Select<KeyValuePair<AuditActionType, int>, AuditActionTypeInfo>((Func<KeyValuePair<AuditActionType, int>, AuditActionTypeInfo>) (i =>
        {
          AuditActionTypeInfo auditActionTypeInfo = new AuditActionTypeInfo();
          auditActionTypeInfo.set_ActionType(((object) i.Key).ToString());
          auditActionTypeInfo.set_ActionTypeId(i.Value);
          return auditActionTypeInfo;
        })).ToList<AuditActionTypeInfo>();
      }
    }

    public bool LoadKeys()
    {
      AuditingDAL.log.Verbose((object) "LoadKeys...");
      bool flag = false;
      lock (this.locker)
      {
        this.actionTypes.Clear();
        try
        {
          using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT ActionTypeID, ActionType FROM AuditingActionTypes;"))
          {
            using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
            {
              while (dataReader.Read())
              {
                try
                {
                  this.actionTypes.Add(new AuditActionType(dataReader["ActionType"].ToString()), Convert.ToInt32(dataReader["ActionTypeID"]));
                }
                catch (ArgumentException ex)
                {
                  AuditingDAL.log.ErrorFormat("AuditingDAL had problems with loading list of actionTypes. Exception: {0}", (object) ex);
                }
                flag = true;
              }
            }
          }
        }
        catch (Exception ex)
        {
          AuditingDAL.log.ErrorFormat("AuditingDAL couldn't get list of actionTypes. Exception: {0}", (object) ex);
        }
      }
      AuditingDAL.log.Verbose((object) "LoadKeys finished.");
      return flag;
    }

    protected int GetActionIdFromActionType(AuditActionType actionType)
    {
      if (AuditingDAL.log.get_IsDebugEnabled())
        AuditingDAL.log.DebugFormat("GetActionIdFromActionType for {0}", (object) actionType);
      int num = -1;
      lock (this.locker)
      {
        if (this.actionTypes.TryGetValue(actionType, out num))
          return num;
      }
      if (this.LoadKeys())
      {
        lock (this.locker)
        {
          if (this.actionTypes.TryGetValue(actionType, out num))
            return num;
        }
      }
      throw new ArgumentException(string.Format("ActionType {0} was not found in dictionary.", (object) actionType));
    }

    public AuditActionType GetActionTypeFromActionId(int actionTypeId)
    {
      if (AuditingDAL.log.get_IsDebugEnabled())
        AuditingDAL.log.DebugFormat("GetActionTypeFromActionId for {0}", (object) actionTypeId);
      lock (this.locker)
      {
        using (IEnumerator<KeyValuePair<AuditActionType, int>> enumerator = ((IEnumerable<KeyValuePair<AuditActionType, int>>) this.actionTypes).Where<KeyValuePair<AuditActionType, int>>((Func<KeyValuePair<AuditActionType, int>, bool>) (actionType => actionType.Value == actionTypeId)).GetEnumerator())
        {
          if (((IEnumerator) enumerator).MoveNext())
            return enumerator.Current.Key;
        }
      }
      if (this.LoadKeys())
      {
        lock (this.locker)
        {
          using (IEnumerator<KeyValuePair<AuditActionType, int>> enumerator = ((IEnumerable<KeyValuePair<AuditActionType, int>>) this.actionTypes).Where<KeyValuePair<AuditActionType, int>>((Func<KeyValuePair<AuditActionType, int>, bool>) (actionType => actionType.Value == actionTypeId)).GetEnumerator())
          {
            if (((IEnumerator) enumerator).MoveNext())
              return enumerator.Current.Key;
          }
        }
      }
      throw new ArgumentException(string.Format("ActionTypeId {0} was not found in dictionary.", (object) actionTypeId));
    }

    public static string GetNodeCaption(int nodeId)
    {
      string str = string.Format("SELECT Caption FROM Nodes WHERE NodeId = @NodeId");
      try
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Parameters.AddWithValue("@NodeId", (object) nodeId);
          return (string) SqlHelper.ExecuteScalar(textCommand);
        }
      }
      catch (Exception ex)
      {
        AuditingDAL.log.Warn((object) "GetNodeCaption failed.", ex);
        return string.Empty;
      }
    }

    public static KeyValuePair<string, string> GetNodeCaptionAndStatus(int nodeId)
    {
      string str = string.Format("SELECT Caption, Status FROM Nodes WHERE NodeId = @NodeId");
      try
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Parameters.AddWithValue("@NodeId", (object) nodeId);
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
          {
            if (dataReader.Read())
              return new KeyValuePair<string, string>(dataReader["Caption"].ToString(), dataReader["Status"].ToString());
          }
        }
      }
      catch (Exception ex)
      {
        AuditingDAL.log.Warn((object) "GetNodeCaptionAndStatus failed.", ex);
      }
      return new KeyValuePair<string, string>();
    }

    public int StoreNotification(AuditDatabaseDecoratedContainer container)
    {
      if (container == null)
        throw new ArgumentNullException(nameof (container));
      return this.StoreNotification(container, this.GetActionIdFromActionType(container.get_ActionType()));
    }

    private AuditingDAL.NetObjectInfo GetNetObjectInfo(
      IDictionary<string, string> arguments)
    {
      int? nullable1 = new int?();
      int? nullable2 = new int?();
      string str = (string) null;
      if (arguments.ContainsKey((string) KnownKeys.NetObject) && arguments[(string) KnownKeys.NetObject] != null)
      {
        string[] netObject = NetObjectHelper.ParseNetObject(arguments[(string) KnownKeys.NetObject]);
        str = netObject[0];
        nullable2 = new int?(int.Parse(netObject[1]));
      }
      if (arguments.ContainsKey((string) KnownKeys.NodeID))
        nullable1 = new int?(int.Parse(arguments[(string) KnownKeys.NodeID]));
      else if (str != null && str.Equals("N", StringComparison.OrdinalIgnoreCase))
        nullable1 = nullable2;
      return new AuditingDAL.NetObjectInfo()
      {
        NetObjectID = nullable2,
        NetObjectType = str,
        NetworkNodeID = nullable1
      };
    }

    protected int StoreNotification(
      AuditDatabaseDecoratedContainer decoratedDecoratedContainer,
      int actionTypeId)
    {
      if (AuditingDAL.log.get_IsTraceEnabled())
        AuditingDAL.log.Trace((object) ("StoreNotification actionTypeId: " + (object) actionTypeId));
      int count = decoratedDecoratedContainer.get_Args().Count;
      if (AuditingDAL.log.get_IsDebugEnabled())
        AuditingDAL.log.Debug((object) ("args.Count: " + (object) count));
      AuditingDAL.NetObjectInfo netObjectInfo = this.GetNetObjectInfo((IDictionary<string, string>) decoratedDecoratedContainer.get_Args());
      StringBuilder stringBuilder = new StringBuilder("\r\nDECLARE @msg VARCHAR(max), @sev INT, @st INT;\r\n\r\n    INSERT INTO [dbo].[AuditingEvents] \r\n    (\r\n        [TimeLoggedUtc], \r\n        [AccountID], \r\n        [ActionTypeID], \r\n        [AuditEventMessage],\r\n        [NetworkNode],\r\n        [NetObjectID],\r\n        [NetObjectType]\r\n    )\r\n    VALUES\r\n    (\r\n        @TimeLoggedUtc, \r\n        @AccountID, \r\n        @ActionTypeID, \r\n        @AuditEventMessage,\r\n        @NetworkNode,\r\n        @NetObjectID,\r\n        @NetObjectType\r\n    );\r\n");
      if (count > 0)
      {
        stringBuilder.Append("  SELECT @lastID = @@IDENTITY;\r\n\r\n    INSERT INTO [dbo].[AuditingArguments] \r\n    ([AuditEventID], [ArgsKey], [ArgsValue])\r\n     ");
        stringBuilder.Append(string.Join(" UNION ALL ", Enumerable.Range(0, count).Select<int, string>((Func<int, string>) (i => string.Format(" SELECT @lastID, @ArgsKey{0}, @ArgsValue{0} ", (object) i)))));
      }
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction sqlTransaction = connection.BeginTransaction())
        {
          using (SqlCommand textCommand = SqlHelper.GetTextCommand(stringBuilder.ToString()))
          {
            try
            {
              textCommand.Parameters.AddWithValue("@TimeLoggedUtc", (object) decoratedDecoratedContainer.IndicationTime.ToUniversalTime());
              textCommand.Parameters.AddWithValue("@AccountID", (object) decoratedDecoratedContainer.AccountId.ToLower());
              textCommand.Parameters.AddWithValue("@ActionTypeID", (object) actionTypeId);
              textCommand.Parameters.AddWithValue("@AuditEventMessage", (object) decoratedDecoratedContainer.Message);
              SqlParameterCollection parameters1 = textCommand.Parameters;
              int? nullable = netObjectInfo.NetworkNodeID;
              object obj1;
              if (!nullable.HasValue)
              {
                obj1 = (object) DBNull.Value;
              }
              else
              {
                nullable = netObjectInfo.NetworkNodeID;
                obj1 = (object) nullable.Value;
              }
              parameters1.AddWithValue("@NetworkNode", obj1);
              SqlParameterCollection parameters2 = textCommand.Parameters;
              nullable = netObjectInfo.NetObjectID;
              object obj2;
              if (!nullable.HasValue)
              {
                obj2 = (object) DBNull.Value;
              }
              else
              {
                nullable = netObjectInfo.NetObjectID;
                obj2 = (object) nullable.Value;
              }
              parameters2.AddWithValue("@NetObjectID", obj2);
              textCommand.Parameters.AddWithValue("@NetObjectType", netObjectInfo.NetObjectType != null ? (object) netObjectInfo.NetObjectType : (object) DBNull.Value);
              SqlParameterCollection parameters3 = textCommand.Parameters;
              SqlParameter sqlParameter = new SqlParameter("@lastID", SqlDbType.Int);
              sqlParameter.Direction = ParameterDirection.InputOutput;
              sqlParameter.Value = (object) 0;
              parameters3.Add(sqlParameter);
              for (int index = 0; index < count; ++index)
              {
                string key = decoratedDecoratedContainer.get_Args().ElementAt<KeyValuePair<string, string>>(index).Key;
                string str = decoratedDecoratedContainer.get_Args().ElementAt<KeyValuePair<string, string>>(index).Value;
                if (AuditingDAL.log.get_IsDebugEnabled())
                  AuditingDAL.log.DebugFormat("Adding argument: '{0}':'{1}'", (object) key, (object) str);
                textCommand.Parameters.AddWithValue(string.Format("@ArgsKey{0}", (object) index), (object) key);
                textCommand.Parameters.AddWithValue(string.Format("@ArgsValue{0}", (object) index), (object) (str ?? string.Empty));
              }
              AuditingDAL.log.Trace((object) "Executing query.");
              SqlHelper.ExecuteScalar(textCommand, connection, sqlTransaction);
              sqlTransaction.Commit();
              int result = 0;
              int.TryParse(textCommand.Parameters["@lastID"].Value.ToString(), out result);
              return result;
            }
            catch (Exception ex)
            {
              sqlTransaction.Rollback();
              AuditingDAL.log.Error((object) "Error while storing notification.", ex);
              throw;
            }
          }
        }
      }
    }

    internal static DataTable GetAuditingTable(
      int maxRecords,
      int netObjectId,
      string netObjectType,
      int nodeId,
      string actionTypeIds,
      DateTime startTime,
      DateTime endTime)
    {
      string format = "\r\n;WITH TOPNAUDITS\r\nAS\r\n(\r\n\tSELECT TOP (@parTopLimit)\r\n\t\tAE.TimeLoggedUtc AS DateTime,\r\n\t\tAE.AccountID,\r\n\t\tAE.AuditEventMessage AS Message,\r\n\t\tAE.AuditEventID,\r\n\t\tAE.ActionTypeID    \r\n\tFROM AuditingEvents AS AE WITH(NOLOCK)\r\n\tWHERE\r\n\t{0}        \r\n\tORDER BY\r\n\t\tAE.TimeLoggedUtc DESC\r\n)\r\nSELECT \r\n    TNA.DateTime,\r\n    TNA.AccountID,\r\n    TNA.Message,\r\n    TNA.AuditEventID,\r\n    TNA.ActionTypeID,\r\n    AAT.ActionType,\r\n    ARGS.ArgsKey,\r\n    ARGS.ArgsValue\r\nFROM TOPNAUDITS AS TNA\r\nLEFT JOIN AuditingActionTypes AS AAT WITH(NOLOCK)\r\nON\r\n    TNA.ActionTypeID = AAT.ActionTypeID\r\nLEFT JOIN AuditingArguments AS ARGS WITH(NOLOCK)\r\n\tON TNA.AuditEventID = ARGS.AuditEventID\r\nORDER BY TNA.[DateTime] DESC\r\n";
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Empty))
      {
        List<string> stringList = new List<string>();
        stringList.Add("1 = 1");
        textCommand.Parameters.AddWithValue("@parTopLimit", (object) maxRecords);
        stringList.Add("AE.TimeLoggedUtc >= @parStartTime");
        textCommand.Parameters.AddWithValue("@parStartTime", (object) startTime);
        stringList.Add("AE.TimeLoggedUtc <= @parEndTime");
        textCommand.Parameters.AddWithValue("@parEndTime", (object) endTime);
        if (nodeId >= 0 && netObjectId < 0)
        {
          stringList.Add("AE.NetworkNode = @parNodeID");
          textCommand.Parameters.Add(new SqlParameter("@parNodeID", (object) nodeId));
        }
        if (netObjectId >= 0)
        {
          stringList.Add("AE.NetObjectID = @parNetObjectID");
          textCommand.Parameters.Add(new SqlParameter("@parNetObjectID", (object) netObjectId));
        }
        if (!string.IsNullOrEmpty(netObjectType))
        {
          stringList.Add("AE.NetObjectType LIKE @parNetObjectType");
          SqlParameterCollection parameters = textCommand.Parameters;
          SqlParameter sqlParameter = new SqlParameter("@parNetObjectType", SqlDbType.Char, 10);
          sqlParameter.Value = (object) netObjectType;
          parameters.Add(sqlParameter);
        }
        if (!string.IsNullOrEmpty(actionTypeIds))
          stringList.Add(" AE.ActionTypeID IN (" + actionTypeIds + ")");
        string str1 = string.Join(" AND " + Environment.NewLine, (IEnumerable<string>) stringList);
        string str2 = string.Format(format, (object) str1);
        textCommand.CommandText = str2;
        DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand);
        dataTable.TableName = "AuditingTable";
        return dataTable;
      }
    }

    public static DataTable GetAuditingTypesTable()
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = AuditingDAL.creator.Create())
      {
        DataTable dataTable = InformationServiceProxyExtensions.QueryWithAppendedErrors((IInformationServiceProxy) iinformationServiceProxy2, "SELECT DISTINCT ActionTypeID, ActionType, ActionTypeDisplayName FROM [Orion].[AuditingActionTypes] (nolock=true)", SwisFederationInfo.get_IsFederationEnabled());
        dataTable.TableName = "AuditingTypesTable";
        return dataTable;
      }
    }

    public AuditDataContainer GetAuditDataContainer(int auditEventId)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT [ArgsKey], [ArgsValue] FROM [dbo].[AuditingArguments] WITH(NOLOCK) WHERE [AuditEventID] = @AuditEventID;"))
      {
        textCommand.Parameters.AddWithValue("@AuditEventID", (object) auditEventId);
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            dictionary.Add(dataReader["ArgsKey"].ToString(), dataReader["ArgsValue"].ToString());
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT TOP 1 [AccountID], [ActionTypeID] FROM [dbo].[AuditingEvents] WITH(NOLOCK) WHERE [AuditEventID] = @AuditEventID;"))
      {
        textCommand.Parameters.AddWithValue("@AuditEventID", (object) auditEventId);
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          dataReader.Read();
          return new AuditDataContainer(this.GetActionTypeFromActionId((int) dataReader["ActionTypeID"]), dictionary, dataReader["AccountID"].ToString());
        }
      }
    }

    private class NetObjectInfo
    {
      public int? NetworkNodeID { get; set; }

      public int? NetObjectID { get; set; }

      public string NetObjectType { get; set; }
    }
  }
}
