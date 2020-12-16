// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.VolumeDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Snmp;
using SolarWinds.InformationService.Contract2;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.MacroProcessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class VolumeDAL
  {
    private static readonly Log log = new Log();
    private static object insertVolumeLock = new object();

    private static SqlCommand LoadCommandParams(
      Volume volume,
      SqlCommand command,
      bool includeID)
    {
      if (VolumeDAL.log.get_IsDebugEnabled())
        VolumeDAL.log.DebugFormat("Loading command parameters for volume {0}", (object) volume);
      command.Parameters.Add("@NodeID", SqlDbType.Int, 100).Value = (object) volume.get_NodeID();
      command.Parameters.Add("@VolumeIndex", SqlDbType.Int, 4).Value = (object) volume.get_VolumeIndex();
      command.Parameters.Add("@Caption", SqlDbType.NVarChar, 75).Value = (object) volume.get_Caption();
      command.Parameters.Add("@PollInterval", SqlDbType.Int, 4).Value = (object) volume.get_PollInterval();
      command.Parameters.Add("@StatCollection", SqlDbType.Int, 4).Value = (object) volume.get_StatCollection();
      command.Parameters.Add("@RediscoveryInterval", SqlDbType.Int, 4).Value = (object) volume.get_RediscoveryInterval();
      command.Parameters.Add("@UnManaged", SqlDbType.Bit).Value = (object) volume.get_UnManaged();
      command.Parameters.Add("@UnManageFrom", SqlDbType.DateTime).Value = volume.get_UnManageFrom() == new DateTime() ? (object) DBNull.Value : (object) volume.get_UnManageFrom();
      command.Parameters.Add("@UnManageUntil", SqlDbType.DateTime).Value = volume.get_UnManageUntil() == new DateTime() ? (object) DBNull.Value : (object) volume.get_UnManageUntil();
      command.Parameters.Add("@VolumeDescription", SqlDbType.NVarChar, 512).Value = (object) volume.get_VolumeDescription();
      command.Parameters.Add("@VolumeTypeID", SqlDbType.Int, 4).Value = (object) (int) volume.get_VolumeTypeID();
      command.Parameters.Add("@VolumeType", SqlDbType.NVarChar, 40).Value = (object) volume.get_VolumeType();
      command.Parameters.Add("@VolumeTypeIcon", SqlDbType.VarChar, 20).Value = (object) volume.get_VolumeTypeIcon();
      command.Parameters.Add("@VolumePercentUsed", SqlDbType.Real).Value = (object) volume.get_VolumePercentUsed();
      command.Parameters.Add("@VolumeSpaceUsed", SqlDbType.Float).Value = (object) volume.get_VolumeSpaceUsed();
      command.Parameters.Add("@VolumeSpaceAvailable", SqlDbType.Float, 4).Value = (object) volume.get_VolumeSpaceAvailable();
      command.Parameters.Add("@VolumeSize", SqlDbType.Float, 4).Value = (object) volume.get_VolumeSize();
      command.Parameters.Add("@Status", SqlDbType.Int, 4).Value = (object) volume.get_Status();
      command.Parameters.Add("@StatusLED", SqlDbType.VarChar, 20).Value = (object) volume.get_StatusLED();
      command.Parameters.Add("@VolumeResponding", SqlDbType.Char, 1).Value = (object) (char) (volume.get_VolumeResponding() ? 89 : 78);
      command.Parameters.Add("@VolumeAllocationFailuresThisHour", SqlDbType.Int, 4).Value = (object) volume.get_VolumeAllocationFailuresThisHour();
      command.Parameters.Add("@VolumeAllocationFailuresToday", SqlDbType.Int, 4).Value = (object) volume.get_VolumeAllocationFailuresToday();
      command.Parameters.Add("@NextPoll", SqlDbType.DateTime).Value = (object) volume.get_NextPoll();
      command.Parameters.Add("@NextRediscovery", SqlDbType.DateTime).Value = (object) volume.get_NextRediscovery();
      command.Parameters.Add("@FullName", SqlDbType.NVarChar, (int) byte.MaxValue).Value = (object) volume.get_FullName();
      SqlParameter sqlParameter1 = command.Parameters.Add("@DiskQueueLength", SqlDbType.Float, 4);
      double? nullable = volume.get_DiskQueueLength();
      object obj1 = nullable.HasValue ? (object) nullable.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter1.Value = obj1;
      SqlParameter sqlParameter2 = command.Parameters.Add("@DiskTransfer", SqlDbType.Float, 4);
      nullable = volume.get_DiskTransfer();
      object obj2 = nullable.HasValue ? (object) nullable.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter2.Value = obj2;
      SqlParameter sqlParameter3 = command.Parameters.Add("@DiskReads", SqlDbType.Float, 4);
      nullable = volume.get_DiskReads();
      object obj3 = nullable.HasValue ? (object) nullable.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter3.Value = obj3;
      SqlParameter sqlParameter4 = command.Parameters.Add("@DiskWrites", SqlDbType.Float, 4);
      nullable = volume.get_DiskWrites();
      object obj4 = nullable.HasValue ? (object) nullable.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter4.Value = obj4;
      SqlParameter sqlParameter5 = command.Parameters.Add("@TotalDiskIOPS", SqlDbType.Float, 4);
      nullable = volume.get_TotalDiskIOPS();
      object obj5 = nullable.HasValue ? (object) nullable.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter5.Value = obj5;
      command.Parameters.Add("@DeviceId", SqlDbType.NVarChar, 512).Value = !string.IsNullOrWhiteSpace(volume.get_DeviceId()) ? (object) volume.get_DeviceId() : (object) DBNull.Value;
      command.Parameters.Add("@DiskSerialNumber", SqlDbType.NVarChar, (int) byte.MaxValue).Value = !string.IsNullOrWhiteSpace(volume.get_DiskSerialNumber()) ? (object) volume.get_DiskSerialNumber() : (object) DBNull.Value;
      command.Parameters.Add("@InterfaceType", SqlDbType.NVarChar, 20).Value = !string.IsNullOrWhiteSpace(volume.get_InterfaceType()) ? (object) volume.get_InterfaceType() : (object) DBNull.Value;
      SqlParameter sqlParameter6 = command.Parameters.Add("@SCSITargetId", SqlDbType.Int, 4);
      int? scsiTargetId = volume.get_SCSITargetId();
      object obj6 = scsiTargetId.HasValue ? (object) scsiTargetId.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter6.Value = obj6;
      SqlParameter sqlParameter7 = command.Parameters.Add("@SCSIPortId", SqlDbType.Int, 4);
      int? scsiPortId = volume.get_SCSIPortId();
      object obj7 = scsiPortId.HasValue ? (object) scsiPortId.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter7.Value = obj7;
      SqlParameter sqlParameter8 = command.Parameters.Add("@SCSILunId", SqlDbType.Int, 4);
      int? scsiLunId = volume.get_SCSILunId();
      object obj8 = scsiLunId.HasValue ? (object) scsiLunId.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter8.Value = obj8;
      command.Parameters.Add("@SCSIControllerId", SqlDbType.NVarChar, (int) byte.MaxValue).Value = !string.IsNullOrWhiteSpace(volume.get_SCSIControllerId()) ? (object) volume.get_SCSIControllerId() : (object) DBNull.Value;
      SqlParameter sqlParameter9 = command.Parameters.Add("@SCSIPortOffset", SqlDbType.Int, 4);
      int? scsiPortOffset = volume.get_SCSIPortOffset();
      object obj9 = scsiPortOffset.HasValue ? (object) scsiPortOffset.GetValueOrDefault() : (object) DBNull.Value;
      sqlParameter9.Value = obj9;
      if (volume.get_LastSync() == DateTime.MinValue)
        command.Parameters.Add("@LastSync", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        command.Parameters.Add("@LastSync", SqlDbType.DateTime).Value = (object) volume.get_LastSync();
      if (includeID)
        command.Parameters.AddWithValue("VolumeID", (object) volume.get_ID());
      return command;
    }

    public static void DeleteVolume(Volume volume)
    {
      SqlCommand textCommand1 = SqlHelper.GetTextCommand("swsp_DeleteVolume");
      textCommand1.CommandType = CommandType.StoredProcedure;
      textCommand1.Parameters.Add("@id", SqlDbType.Int).Value = (object) volume.get_ID();
      SqlHelper.ExecuteNonQuery(textCommand1);
      SqlCommand textCommand2;
      using (textCommand2 = SqlHelper.GetTextCommand("Delete FROM Pollers WHERE NetObject = @NetObject"))
      {
        textCommand2.Parameters.Add("@NetObject", SqlDbType.VarChar, 50).Value = (object) ("V:" + (object) volume.get_ID());
        SqlHelper.ExecuteNonQuery(textCommand2);
      }
    }

    public static int GetVolumeCount()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT COUNT(*) FROM Volumes"))
        return (int) SqlHelper.ExecuteScalar(textCommand);
    }

    public static int InsertVolume(Volume volume)
    {
      SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nIF NOT EXISTS (SELECT * FROM Volumes WHERE NodeID = @NodeID AND VolumeDescription = @VolumeDescription)\r\nBEGIN \r\n    INSERT INTO Volumes\r\n        ([NodeID]\r\n        ,[LastSync]\r\n        ,[VolumeIndex]\r\n        ,[Caption]\r\n        ,[PollInterval]\r\n        ,[StatCollection]\r\n        ,[RediscoveryInterval]\r\n        ,[UnManaged]\r\n        ,[UnManageFrom]\r\n        ,[UnManageUntil]\r\n        ,[VolumeDescription]\r\n        ,[VolumeTypeID]\r\n        ,[VolumeType]\r\n        ,[VolumeTypeIcon]\r\n        ,[VolumePercentUsed]\r\n        ,[VolumeSpaceUsed]\r\n        ,[VolumeSpaceAvailable]\r\n        ,[VolumeSize]\r\n        ,[Status]\r\n        ,[StatusLED]\r\n        ,[VolumeResponding]\r\n        ,[VolumeAllocationFailuresThisHour]\r\n        ,[VolumeAllocationFailuresToday]\r\n        ,[NextPoll]\r\n        ,[NextRediscovery]\r\n        ,[FullName]\r\n        ,[DiskQueueLength]\r\n        ,[DiskTransfer]\r\n        ,[DiskReads]\r\n        ,[DiskWrites]\r\n        ,[TotalDiskIOPS]\r\n        ,[DeviceId]\r\n        ,[DiskSerialNumber]\r\n\t\t,[InterfaceType]\r\n        ,[SCSITargetId]\r\n        ,[SCSIPortId]\r\n        ,[SCSILunId]\r\n        ,[SCSIControllerId]\r\n        ,[SCSIPortOffset])\r\n    VALUES \r\n        (@NodeID \r\n        ,@LastSync\r\n        ,@VolumeIndex\r\n        ,@Caption\r\n        ,@PollInterval\r\n        ,@StatCollection\r\n        ,@RediscoveryInterval\r\n\t\t,@UnManaged\r\n\t\t,@UnManageFrom\r\n\t\t,@UnManageUntil\r\n\t\t,@VolumeDescription\r\n        ,@VolumeTypeID\r\n        ,@VolumeType\r\n        ,@VolumeTypeIcon\r\n        ,@VolumePercentUsed\r\n        ,@VolumeSpaceUsed\r\n        ,@VolumeSpaceAvailable\r\n        ,@VolumeSize\r\n        ,@Status\r\n        ,@StatusLED\r\n        ,@VolumeResponding\r\n        ,@VolumeAllocationFailuresThisHour\r\n        ,@VolumeAllocationFailuresToday\r\n        ,@NextPoll\r\n        ,@NextRediscovery\r\n        ,@FullName\r\n        ,@DiskQueueLength\r\n        ,@DiskTransfer\r\n        ,@DiskReads\r\n        ,@DiskWrites\r\n        ,@TotalDiskIOPS\r\n        ,@DeviceId\r\n        ,@DiskSerialNumber\r\n\t\t,@InterfaceType\r\n        ,@SCSITargetId\r\n        ,@SCSIPortId\r\n        ,@SCSILunId\r\n        ,@SCSIControllerId\r\n        ,@SCSIPortOffset);\r\n\r\n    SELECT scope_identity();\r\nEND\r\nELSE\r\nBEGIN\r\n    SELECT -1;\r\nEND\r\n");
      volume = new DALHelper<Volume>().Initialize(volume);
      VolumeDAL.LoadCommandParams(volume, textCommand, true);
      VolumeDAL.log.TraceFormat("Inserting volume. Locking thread. NodeID: {0}, Name: {1}", new object[2]
      {
        (object) volume.get_NodeID(),
        (object) volume.get_VolumeDescription()
      });
      lock (VolumeDAL.insertVolumeLock)
      {
        VolumeDAL.log.TraceFormat("Inserting volume. Thread locked. NodeID: {0}, Name: {1}", new object[2]
        {
          (object) volume.get_NodeID(),
          (object) volume.get_VolumeDescription()
        });
        volume.set_VolumeId(Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand)));
        if (volume.get_VolumeId() > 0)
          VolumeDAL.log.DebugFormat("Volume [{0}] inserted with ID {1} on node {2}", (object) volume.get_VolumeDescription(), (object) volume.get_VolumeId(), (object) volume.get_NodeID());
        else
          VolumeDAL.log.DebugFormat("Volume [{0}] managed already on node {1}", (object) volume.get_VolumeDescription(), (object) volume.get_NodeID());
      }
      return volume.get_VolumeId();
    }

    public static PropertyBag UpdateVolume(Volume volume)
    {
      PropertyBag propertyBag1 = new PropertyBag();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\n                DECLARE @tempTable TABLE (Caption nvarchar(75), FullName nvarchar(255), Status int, PollInterval int, StatCollection int, RediscoveryInterval int, VolumeIndex int, VolumeType nvarchar(40), VolumeDescription nvarchar(512), VolumeSize float, VolumeResponding char(1));\r\n\t\t\t\tUPDATE [Volumes] SET \r\n\t\t\t\t[LastSync] = @LastSync     \r\n\t\t\t\t,[VolumeIndex] = @VolumeIndex\r\n\t\t\t\t,[Caption] = @Caption\r\n\t\t\t\t,[PollInterval] = @PollInterval\r\n\t\t\t\t,[StatCollection] = @StatCollection\r\n\t\t\t\t,[RediscoveryInterval] = @RediscoveryInterval\r\n                ,[UnManaged] = @UnManaged\r\n                ,[UnManageFrom] = @UnManageFrom\r\n                ,[UnManageUntil] = @UnManageUntil\r\n\t\t\t\t,[VolumeDescription] = @VolumeDescription\r\n                ,[VolumeTypeID] = @VolumeTypeID\r\n\t\t\t\t,[VolumeType] = @VolumeType\r\n\t\t\t\t,[VolumeTypeIcon] = @VolumeTypeIcon\r\n\t\t\t\t,[VolumePercentUsed] = @VolumePercentUsed\r\n\t\t\t\t,[VolumeSpaceUsed] = @VolumeSpaceUsed\r\n\t\t\t\t,[VolumeSpaceAvailable] = @VolumeSpaceAvailable\r\n\t\t\t\t,[VolumeSize] = @VolumeSize\r\n\t\t\t\t,[Status] = @Status\r\n\t\t\t\t,[StatusLED] = @StatusLED\r\n\t\t\t\t,[VolumeResponding] = @VolumeResponding\r\n\t\t\t\t,[VolumeAllocationFailuresThisHour] = @VolumeAllocationFailuresThisHour\r\n\t\t\t\t,[VolumeAllocationFailuresToday] = @VolumeAllocationFailuresToday\r\n\t\t\t\t,[NextPoll] = @NextPoll\r\n\t\t\t\t,[NextRediscovery] = @NextRediscovery\r\n\t\t\t\t,[FullName] = @FullName\r\n                ,[DiskQueueLength] = @DiskQueueLength\r\n                ,[DiskTransfer] = @DiskTransfer\r\n                ,[DiskReads] = @DiskReads\r\n                ,[DiskWrites] = @DiskWrites\r\n                ,[TotalDiskIOPS] = @TotalDiskIOPS\r\n                ,[DeviceId] = @DeviceId\r\n                ,[DiskSerialNumber] = @DiskSerialNumber\r\n\t\t\t\t,[InterfaceType] = @InterfaceType\r\n                ,[SCSITargetId] = @SCSITargetId\r\n                ,[SCSIPortId] = @SCSIPortId\r\n                ,[SCSILunId] = @SCSILunId\r\n                ,[SCSIControllerId] = @SCSIControllerId\r\n                ,[SCSIPortOffset] = @SCSIPortOffset\r\n                OUTPUT DELETED.Caption, \r\n                       DELETED.FullName, \r\n                       DELETED.Status, \r\n                       DELETED.PollInterval, \r\n                       DELETED.StatCollection,\r\n                       DELETED.RediscoveryInterval,\r\n                       DELETED.VolumeIndex,\r\n                       DELETED.VolumeType,\r\n                       DELETED.VolumeDescription,\r\n                       DELETED.VolumeSize,\r\n                       DELETED.VolumeResponding INTO @tempTable\r\n\t\t\t\tWHERE VolumeID = @VolumeID;\r\n                SELECT * FROM @tempTable;"))
      {
        VolumeDAL.LoadCommandParams(volume, textCommand, true);
        using (DataTable dataTable = SqlHelper.ExecuteDataTable(textCommand))
        {
          if (dataTable.Rows.Count == 1)
          {
            VolumeDAL.UpdateCustomProperties(volume);
            foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
            {
              PropertyBag propertyBag2 = new PropertyBag();
              foreach (DataColumn column in (InternalDataCollectionBase) dataTable.Columns)
              {
                object obj1 = row[column] == DBNull.Value ? (object) null : row[column];
                object obj2 = (object) null;
                bool? nullable = new bool?();
                if (column.ColumnName.Equals("Caption", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_Caption();
                else if (column.ColumnName.Equals("FullName", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_FullName();
                else if (column.ColumnName.Equals("Status", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_Status();
                else if (column.ColumnName.Equals("PollInterval", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_PollInterval();
                else if (column.ColumnName.Equals("StatCollection", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_StatCollection();
                else if (column.ColumnName.Equals("RediscoveryInterval", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_RediscoveryInterval();
                else if (column.ColumnName.Equals("VolumeDescription", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_VolumeDescription();
                else if (column.ColumnName.Equals("VolumeSize", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_VolumeSize();
                else if (column.ColumnName.Equals("VolumeIndex", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_VolumeIndex();
                else if (column.ColumnName.Equals("VolumeType", StringComparison.OrdinalIgnoreCase))
                  obj2 = (object) volume.get_VolumeType();
                else if (column.ColumnName.Equals("VolumeResponding", StringComparison.OrdinalIgnoreCase))
                {
                  obj2 = (object) volume.get_VolumeResponding();
                  bool flag = obj1 != null && string.Equals(obj1.ToString(), "Y", StringComparison.OrdinalIgnoreCase);
                  nullable = new bool?((bool) obj2 == flag);
                }
                if (obj1 == null && obj2 != null || obj1 != null && obj2 == null || !nullable.HasValue && obj2 != null && (obj1 != null && !string.Equals(obj2.ToString(), obj1.ToString(), StringComparison.Ordinal)) || nullable.HasValue && !nullable.Value)
                {
                  ((Dictionary<string, object>) propertyBag1).Add(column.ColumnName, obj2);
                  ((Dictionary<string, object>) propertyBag2).Add(column.ColumnName, obj1);
                }
              }
              if (((Dictionary<string, object>) propertyBag2).Count != 0)
                ((Dictionary<string, object>) propertyBag1).Add("PreviousProperties", (object) propertyBag2);
            }
          }
        }
      }
      return propertyBag1;
    }

    private static void UpdateCustomProperties(Volume _volume)
    {
      IDictionary<string, object> customProperties = _volume.get_CustomProperties();
      if (customProperties.Count == 0)
        return;
      List<string> stringList = new List<string>(customProperties.Count);
      List<SqlParameter> sqlParameterList = new List<SqlParameter>(customProperties.Count);
      int num = 0;
      foreach (string key in (IEnumerable<string>) customProperties.Keys)
      {
        string parameterName = string.Format("p{0}", (object) num);
        ++num;
        stringList.Add(string.Format("[{0}]=@{1}", (object) key, (object) parameterName));
        if (customProperties[key] == null || customProperties[key] == DBNull.Value || string.IsNullOrEmpty(customProperties[key].ToString()))
          sqlParameterList.Add(new SqlParameter(parameterName, (object) DBNull.Value));
        else
          sqlParameterList.Add(new SqlParameter(parameterName, customProperties[key]));
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("UPDATE Volumes SET {0} WHERE VolumeID=@VolumeID", (object) string.Join(", ", stringList.ToArray()))))
      {
        foreach (SqlParameter sqlParameter in sqlParameterList)
          textCommand.Parameters.Add(sqlParameter);
        textCommand.Parameters.AddWithValue("VolumeID", (object) _volume.get_ID());
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static Volumes GetNodeVolumes(int nodeID)
    {
      Volumes volumes = new Volumes();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM Volumes WHERE NodeID=@NodeId"))
      {
        textCommand.Parameters.Add("@NodeId", SqlDbType.Int).Value = (object) nodeID;
        using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
        {
          while (reader.Read())
            ((Collection<int, Volume>) volumes).Add(DatabaseFunctions.GetInt32(reader, "VolumeID"), VolumeDAL.CreateVolume(reader));
        }
      }
      return volumes;
    }

    public static Volumes GetNodesVolumes(IEnumerable<int> nodeIDs)
    {
      StringBuilder stringBuilder = new StringBuilder("SELECT * FROM Volumes WHERE NodeID IN (");
      foreach (int nodeId in nodeIDs)
      {
        stringBuilder.Append(nodeId);
        stringBuilder.Append(',');
      }
      --stringBuilder.Length;
      stringBuilder.Append(')');
      // ISSUE: method pointer
      return (Volumes) Collection<int, Volume>.FillCollection<Volumes>(new Collection<int, Volume>.CreateElement((object) null, __methodptr(CreateVolume)), stringBuilder.ToString(), Array.Empty<SqlParameter>());
    }

    public static Volume GetVolume(int volumeID)
    {
      // ISSUE: method pointer
      return Collection<int, Volume>.GetCollectionItem<Volumes>(new Collection<int, Volume>.CreateElement((object) null, __methodptr(CreateVolume)), "SELECT * FROM Volumes WHERE VolumeID=@VolumeID", new SqlParameter[1]
      {
        new SqlParameter("@VolumeID", (object) volumeID)
      });
    }

    public static Volumes GetVolumes()
    {
      // ISSUE: method pointer
      return (Volumes) Collection<int, Volume>.FillCollection<Volumes>(new Collection<int, Volume>.CreateElement((object) null, __methodptr(CreateVolume)), "SELECT * FROM Volumes", (SqlParameter[]) null);
    }

    public static Volumes GetVolumesByIds(int[] volumeIds)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = string.Empty;
      foreach (int volumeId in volumeIds)
      {
        stringBuilder.AppendFormat("{0}{1}", (object) str, (object) volumeId);
        str = ",";
      }
      // ISSUE: method pointer
      return (Volumes) Collection<int, Volume>.FillCollection<Volumes>(new Collection<int, Volume>.CreateElement((object) null, __methodptr(CreateVolume)), string.Format("SELECT * FROM Volumes WHERE Volumes.VolumeID in ({0})", (object) stringBuilder), (SqlParameter[]) null);
    }

    public static Volume CreateVolume(IDataReader reader)
    {
      Volume volume = new Volume();
      for (int i = 0; i < reader.FieldCount; ++i)
      {
        string name = reader.GetName(i);
        switch (name)
        {
          case "Caption":
            volume.set_Caption(DatabaseFunctions.GetString(reader, i));
            break;
          case "DeviceId":
            volume.set_DeviceId(DatabaseFunctions.GetString(reader, i));
            break;
          case "DiskQueueLength":
            volume.set_DiskQueueLength(DatabaseFunctions.GetNullableDouble(reader, i));
            break;
          case "DiskReads":
            volume.set_DiskReads(DatabaseFunctions.GetNullableDouble(reader, i));
            break;
          case "DiskSerialNumber":
            volume.set_DiskSerialNumber(DatabaseFunctions.GetString(reader, i));
            break;
          case "DiskTransfer":
            volume.set_DiskTransfer(DatabaseFunctions.GetNullableDouble(reader, i));
            break;
          case "DiskWrites":
            volume.set_DiskWrites(DatabaseFunctions.GetNullableDouble(reader, i));
            break;
          case "FullName":
            volume.set_FullName(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceType":
            volume.set_InterfaceType(DatabaseFunctions.GetString(reader, i));
            break;
          case "LastSync":
            volume.set_LastSync(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NextPoll":
            volume.set_NextPoll(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NextRediscovery":
            volume.set_NextRediscovery(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NodeID":
            volume.set_NodeID(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "PollInterval":
            volume.set_PollInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "RediscoveryInterval":
            volume.set_RediscoveryInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "SCSIControllerId":
            volume.set_SCSIControllerId(DatabaseFunctions.GetString(reader, i));
            break;
          case "SCSILunId":
            volume.set_SCSILunId(new int?(DatabaseFunctions.GetInt32(reader, i)));
            break;
          case "SCSIPortId":
            volume.set_SCSIPortId(new int?(DatabaseFunctions.GetInt32(reader, i)));
            break;
          case "SCSIPortOffset":
            volume.set_SCSIPortOffset(new int?(DatabaseFunctions.GetInt32(reader, i)));
            break;
          case "SCSITargetId":
            volume.set_SCSITargetId(new int?(DatabaseFunctions.GetInt32(reader, i)));
            break;
          case "StatCollection":
            volume.set_StatCollection(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "Status":
            volume.set_Status(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "StatusLED":
            volume.set_StatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "TotalDiskIOPS":
            volume.set_TotalDiskIOPS(DatabaseFunctions.GetNullableDouble(reader, i));
            break;
          case "UnManageFrom":
            volume.set_UnManageFrom(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManageUntil":
            volume.set_UnManageUntil(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManaged":
            volume.set_UnManaged(DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "VolumeAllocationFailuresThisHour":
            volume.set_VolumeAllocationFailuresThisHour(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "VolumeAllocationFailuresToday":
            volume.set_VolumeAllocationFailuresToday(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "VolumeDescription":
            volume.set_VolumeDescription(DatabaseFunctions.GetString(reader, i));
            break;
          case "VolumeID":
            volume.set_VolumeId(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "VolumeIndex":
            volume.set_VolumeIndex(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "VolumePercentUsed":
            volume.set_VolumePercentUsed(Convert.ToDecimal(DatabaseFunctions.GetFloat(reader, i)));
            break;
          case "VolumeResponding":
            volume.set_VolumeResponding(DatabaseFunctions.GetString(reader, i) == "Y");
            break;
          case "VolumeSize":
            volume.set_VolumeSize(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "VolumeSpaceAvailable":
            volume.set_VolumeSpaceAvailable(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "VolumeSpaceUsed":
            volume.set_VolumeSpaceUsed(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "VolumeType":
            volume.set_VolumeType(DatabaseFunctions.GetString(reader, i));
            break;
          case "VolumeTypeID":
            volume.set_VolumeTypeID((VolumeType) DatabaseFunctions.GetInt32(reader, i));
            break;
          case "VolumeTypeIcon":
            volume.set_VolumeTypeIcon(DatabaseFunctions.GetString(reader, i));
            break;
          default:
            if (CustomPropertyMgr.IsCustom("Volumes", name))
            {
              volume.get_CustomProperties()[name] = reader[i];
              break;
            }
            VolumeDAL.log.DebugFormat("Skipping Volume property {0}, value {1}", (object) name, reader[i]);
            break;
        }
      }
      return volume;
    }

    public static void BulkUpdateVolumePollingInterval(int pollInterval, int engineId)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("IF (@EngineID <= 0) UPDATE Volumes SET PollInterval = @PollInterval ELSE UPDATE Volumes SET PollInterval = @PollInterval WHERE NodeID IN (SELECT NodeID FROM Nodes WITH (NOLOCK) WHERE EngineID = @EngineID)"))
      {
        textCommand.Parameters.Add("@PollInterval", SqlDbType.Int, 4).Value = (object) pollInterval;
        textCommand.Parameters.Add("@engineID", SqlDbType.Int, 4).Value = (object) engineId;
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static Dictionary<string, object> GetVolumeCustomProperties(
      int volumeId,
      ICollection<string> properties)
    {
      Volume volume = VolumeDAL.GetVolume(volumeId);
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      if (properties == null || properties.Count == 0)
        properties = volume.get_CustomProperties().Keys;
      MacroParser macroParser1 = new MacroParser(new Action<string, int>(BusinessLayerOrionEvent.WriteEvent));
      macroParser1.set_ObjectType("Volume");
      macroParser1.set_ActiveObject(volume.get_ID().ToString());
      macroParser1.set_NetObjectID(volume.get_ID().ToString());
      macroParser1.set_NetObjectName(volume.get_FullName());
      macroParser1.set_NodeID(volume.get_NodeID());
      macroParser1.set_NodeName(NodeDAL.GetNode(volume.get_NodeID()).get_Name());
      MacroParser macroParser2 = macroParser1;
      SqlConnection connection;
      macroParser2.set_MyDBConnection(connection = DatabaseFunctions.CreateConnection());
      using (connection)
      {
        foreach (string property in (IEnumerable<string>) properties)
        {
          string key = property.Trim();
          if (volume.get_CustomProperties().ContainsKey(key))
          {
            object customProperty = volume.get_CustomProperties()[key];
            dictionary[key] = customProperty == null || !customProperty.ToString().Contains("${") ? customProperty : (object) macroParser2.ParseMacros(customProperty.ToString(), false);
          }
        }
      }
      return dictionary;
    }
  }
}
