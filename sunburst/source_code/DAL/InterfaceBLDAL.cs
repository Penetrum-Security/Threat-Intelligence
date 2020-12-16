// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.InterfaceBLDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.PackageManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class InterfaceBLDAL
  {
    private static readonly Log log = new Log();
    private static object insertInterfaceLock = new object();
    private static bool _areInterfacesAllowed = ((IPackageManager) SolarWinds.Orion.Core.Common.PackageManager.PackageManager.InstanceWithCache).IsPackageInstalled("Orion.Interfaces");

    public static Interfaces GetNodesInterfaces(IEnumerable<int> nodeIDs)
    {
      if (!InterfaceBLDAL._areInterfacesAllowed)
        return new Interfaces();
      StringBuilder stringBuilder = new StringBuilder("SELECT * FROM Interfaces WHERE NodeID IN (");
      foreach (int nodeId in nodeIDs)
      {
        stringBuilder.Append(nodeId);
        stringBuilder.Append(',');
      }
      --stringBuilder.Length;
      stringBuilder.Append(')');
      // ISSUE: method pointer
      return (Interfaces) Collection<int, Interface>.FillCollection<Interfaces>(new Collection<int, Interface>.CreateElement((object) null, __methodptr(CreateNodeInterface)), stringBuilder.ToString(), Array.Empty<SqlParameter>());
    }

    internal static Interface CreateNodeInterface(IDataReader reader)
    {
      Interface @interface = new Interface();
      for (int i = 0; i < reader.FieldCount; ++i)
      {
        string name = reader.GetName(i);
        switch (name)
        {
          case "AdminStatus":
            @interface.set_AdminStatus(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "AdminStatusLED":
            @interface.set_AdminStatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "Caption":
            @interface.set_Caption(DatabaseFunctions.GetString(reader, i));
            break;
          case "CollectAvailability":
            @interface.set_CollectAvailability(reader.IsDBNull(i) || DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "Counter64":
            @interface.set_Counter64(DatabaseFunctions.GetString(reader, i));
            break;
          case "CustomBandwidth":
            @interface.set_CustomBandwidth(DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "FullName":
            @interface.set_FullName(DatabaseFunctions.GetString(reader, i));
            break;
          case "IfName":
            @interface.set_IfName(DatabaseFunctions.GetString(reader, i));
            break;
          case "InBandwidth":
            @interface.set_InBandwidth(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "InDiscardsThisHour":
            @interface.set_InDiscardsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InDiscardsToday":
            @interface.set_InDiscardsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InErrorsThisHour":
            @interface.set_InErrorsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InErrorsToday":
            @interface.set_InErrorsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InMcastPps":
            @interface.set_InMcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InPercentUtil":
            @interface.set_InPercentUtil(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InPktSize":
            @interface.set_InPktSize(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "InPps":
            @interface.set_InPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InUcastPps":
            @interface.set_InUcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "Inbps":
            @interface.set_InBps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InterfaceAlias":
            @interface.set_InterfaceAlias(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceID":
            @interface.set_InterfaceID(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceIcon":
            @interface.set_InterfaceIcon(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceIndex":
            @interface.set_InterfaceIndex(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceLastChange":
            @interface.set_InterfaceLastChange(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "InterfaceMTU":
            @interface.set_InterfaceMTU(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceName":
            @interface.set_InterfaceName(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceSpeed":
            @interface.set_InterfaceSpeed(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "InterfaceSubType":
            @interface.set_InterfaceSubType(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceType":
            @interface.set_InterfaceType(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceTypeDescription":
            @interface.set_InterfaceTypeDescription(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceTypeName":
            @interface.set_InterfaceTypeName(DatabaseFunctions.GetString(reader, i));
            break;
          case "LastSync":
            @interface.set_LastSync(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxInBpsTime":
            @interface.set_MaxInBpsTime(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxInBpsToday":
            @interface.set_MaxInBpsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "MaxOutBpsTime":
            @interface.set_MaxOutBpsTime(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxOutBpsToday":
            @interface.set_MaxOutBpsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "NextPoll":
            @interface.set_NextPoll(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NextRediscovery":
            @interface.set_NextRediscovery(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NodeID":
            @interface.set_NodeID(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "ObjectSubType":
            @interface.set_ObjectSubType(DatabaseFunctions.GetString(reader, i));
            break;
          case "OperStatus":
            @interface.set_OperStatus(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "OperStatusLED":
            @interface.set_OperStatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "OutBandwidth":
            @interface.set_OutBandwidth(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "OutDiscardsThisHour":
            @interface.set_OutDiscardsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutDiscardsToday":
            @interface.set_OutDiscardsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutErrorsThisHour":
            @interface.set_OutErrorsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutErrorsToday":
            @interface.set_OutErrorsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutMcastPps":
            @interface.set_OutMcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutPercentUtil":
            @interface.set_OutPercentUtil(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutPktSize":
            @interface.set_OutPktSize(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "OutPps":
            @interface.set_OutPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutUcastPps":
            @interface.set_OutUcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "Outbps":
            @interface.set_OutBps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "PhysicalAddress":
            @interface.set_PhysicalAddress(DatabaseFunctions.GetString(reader, i));
            break;
          case "PollInterval":
            @interface.set_PollInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "RediscoveryInterval":
            @interface.set_RediscoveryInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "Severity":
            @interface.set_Severity(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "StatCollection":
            @interface.set_StatCollection(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "Status":
            @interface.set_Status(DatabaseFunctions.GetString(reader, i));
            break;
          case "StatusLED":
            @interface.set_StatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "UnManageFrom":
            @interface.set_UnManageFrom(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManageUntil":
            @interface.set_UnManageUntil(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManaged":
            @interface.set_UnManaged(DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "UnPluggable":
            @interface.set_UnPluggable(DatabaseFunctions.GetBoolean(reader, i));
            break;
          default:
            if (CustomPropertyMgr.IsCustom("Interfaces", name))
            {
              @interface.get_CustomProperties()[name] = reader[i];
              break;
            }
            InterfaceBLDAL.log.DebugFormat("Skipping Interface property {0}, value {1}", (object) name, reader[i]);
            break;
        }
      }
      return @interface;
    }

    public static int CreateNewInterface(Interface _interface, bool checkIfAlreadyExists)
    {
      return InterfaceBLDAL.CreateNewInterface(_interface, checkIfAlreadyExists ? InterfaceBLDAL.SqlManagedInterfaceExists : string.Empty);
    }

    public static int CreateNewInterface(Interface _interface, string checkQuery)
    {
      string str = "\r\n        INSERT INTO [Interfaces]\r\n        ([NodeID]\r\n        ,[ObjectSubType]\r\n        ,[InterfaceName]\r\n        ,[InterfaceIndex]\r\n        ,[InterfaceType]\r\n        ,[InterfaceSubType]\r\n        ,[InterfaceTypeName]\r\n        ,[InterfaceTypeDescription]\r\n        ,[InterfaceSpeed]\r\n        ,[InterfaceMTU]\r\n        ,[InterfaceLastChange]\r\n        ,[PhysicalAddress]\r\n        ,[UnManaged]\r\n        ,[UnManageFrom]\r\n        ,[UnManageUntil]\r\n        ,[AdminStatus]\r\n        ,[OperStatus]\r\n        ,[InBandwidth]\r\n        ,[OutBandwidth]\r\n        ,[Caption]\r\n        ,[PollInterval]\r\n        ,[RediscoveryInterval]\r\n        ,[FullName]\r\n        ,[Status]\r\n        ,[StatusLED]\r\n        ,[AdminStatusLED]\r\n        ,[OperStatusLED]\r\n        ,[InterfaceIcon]\r\n        ,[Outbps]\r\n        ,[Inbps]\r\n        ,[OutPercentUtil]\r\n        ,[InPercentUtil]\r\n        ,[OutPps]\r\n        ,[InPps]\r\n        ,[InPktSize]\r\n        ,[OutPktSize]\r\n        ,[OutUcastPps]\r\n        ,[OutMcastPps]\r\n        ,[InUcastPps]\r\n        ,[InMcastPps]\r\n        ,[InDiscardsThisHour]\r\n        ,[InDiscardsToday]\r\n        ,[InErrorsThisHour]\r\n        ,[InErrorsToday]\r\n        ,[OutDiscardsThisHour]\r\n        ,[OutDiscardsToday]\r\n        ,[OutErrorsThisHour]\r\n        ,[OutErrorsToday]\r\n        ,[MaxInBpsToday]\r\n        ,[MaxInBpsTime]\r\n        ,[MaxOutBpsToday]\r\n        ,[MaxOutBpsTime]\r\n        ,[NextRediscovery]\r\n        ,[NextPoll]\r\n        ,[Counter64]\r\n        ,[StatCollection]\r\n        ,[LastSync]\r\n        ,[InterfaceAlias]\r\n        ,[IfName]\r\n        ,[Severity]\r\n        ,[CustomBandwidth]\r\n        ,[UnPluggable]\r\n        ,[CollectAvailability]\r\n        )\r\n        VALUES\r\n        (@NodeID\r\n        ,@ObjectSubType\r\n        ,@InterfaceName\r\n        ,@InterfaceIndex\r\n        ,@InterfaceType\r\n        ,@InterfaceSubType\r\n        ,@InterfaceTypeName\r\n        ,@InterfaceTypeDescription\r\n        ,@InterfaceSpeed\r\n        ,@InterfaceMTU\r\n        ,@InterfaceLastChange\r\n        ,@PhysicalAddress\r\n        ,@UnManaged\r\n        ,@UnManageFrom\r\n        ,@UnManageUntil\r\n        ,@AdminStatus\r\n        ,@OperStatus\r\n        ,@InBandwidth\r\n        ,@OutBandwidth\r\n        ,@Caption\r\n        ,@PollInterval\r\n        ,@RediscoveryInterval\r\n        ,@FullName\r\n        ,@Status\r\n        ,@StatusLED\r\n        ,@AdminStatusLED\r\n        ,@OperStatusLED\r\n        ,@InterfaceIcon\r\n        ,@Outbps\r\n        ,@Inbps\r\n        ,@OutPercentUtil\r\n        ,@InPercentUtil\r\n        ,@OutPps\r\n        ,@InPps\r\n        ,@InPktSize\r\n        ,@OutPktSize\r\n        ,@OutUcastPps\r\n        ,@OutMcastPps\r\n        ,@InUcastPps\r\n        ,@InMcastPps\r\n        ,@InDiscardsThisHour\r\n        ,@InDiscardsToday\r\n        ,@InErrorsThisHour\r\n        ,@InErrorsToday\r\n        ,@OutDiscardsThisHour\r\n        ,@OutDiscardsToday\r\n        ,@OutErrorsThisHour\r\n        ,@OutErrorsToday\r\n        ,@MaxInBpsToday\r\n        ,@MaxInBpsTime\r\n        ,@MaxOutBpsToday\r\n        ,@MaxOutBpsTime\r\n        ,@NextRediscovery\r\n        ,@NextPoll\r\n        ,@Counter64\r\n        ,@StatCollection\r\n        ,@LastSync\r\n        ,@InterfaceAlias\r\n        ,@IfName\r\n        ,@Severity\r\n        ,@CustomBandwidth\r\n        ,@UnPluggable\r\n        ,@CollectAvailability\r\n        )\r\n\r\n        SELECT Scope_Identity();";
      if (!string.IsNullOrEmpty(checkQuery))
        str = "\r\n                " + checkQuery + "\r\n                BEGIN " + str + " \r\n                END\r\n                ELSE\r\n                BEGIN\r\n                    SELECT -1;\r\n                END";
      SqlCommand textCommand = SqlHelper.GetTextCommand(str);
      _interface = new DALHelper<Interface>().Initialize(_interface);
      textCommand.Parameters.AddWithValue("NodeID", (object) _interface.get_NodeID());
      textCommand.Parameters.AddWithValue("ObjectSubType", (object) _interface.get_ObjectSubType());
      textCommand.Parameters.AddWithValue("InterfaceName", (object) _interface.get_InterfaceName());
      textCommand.Parameters.Add("@InterfaceIndex", SqlDbType.Int, 4).Value = (object) _interface.get_InterfaceIndex();
      textCommand.Parameters.AddWithValue("InterfaceType", (object) _interface.get_InterfaceType());
      textCommand.Parameters.AddWithValue("InterfaceSubType", (object) _interface.get_InterfaceSubType());
      textCommand.Parameters.AddWithValue("InterfaceTypeName", (object) _interface.get_InterfaceTypeName());
      textCommand.Parameters.AddWithValue("InterfaceTypeDescription", (object) _interface.get_InterfaceTypeDescription());
      textCommand.Parameters.AddWithValue("InterfaceSpeed", (object) _interface.get_InterfaceSpeed());
      textCommand.Parameters.AddWithValue("InterfaceMTU", (object) _interface.get_InterfaceMTU());
      if (_interface.get_InterfaceLastChange() == DateTime.MinValue)
        textCommand.Parameters.Add("@InterfaceLastChange", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@InterfaceLastChange", SqlDbType.DateTime).Value = (object) _interface.get_InterfaceLastChange();
      textCommand.Parameters.AddWithValue("PhysicalAddress", (object) _interface.get_PhysicalAddress());
      textCommand.Parameters.AddWithValue("AdminStatus", (object) _interface.get_AdminStatus());
      textCommand.Parameters.AddWithValue("OperStatus", (object) _interface.get_OperStatus());
      textCommand.Parameters.AddWithValue("InBandwidth", (object) _interface.get_InBandwidth());
      textCommand.Parameters.AddWithValue("OutBandwidth", (object) _interface.get_OutBandwidth());
      textCommand.Parameters.AddWithValue("Caption", (object) _interface.get_Caption());
      textCommand.Parameters.AddWithValue("PollInterval", (object) _interface.get_PollInterval());
      textCommand.Parameters.AddWithValue("RediscoveryInterval", (object) _interface.get_RediscoveryInterval());
      textCommand.Parameters.AddWithValue("FullName", (object) _interface.get_FullName());
      textCommand.Parameters.AddWithValue("Status", (object) _interface.get_Status());
      textCommand.Parameters.AddWithValue("StatusLED", (object) _interface.get_StatusLED());
      textCommand.Parameters.AddWithValue("AdminStatusLED", (object) _interface.get_AdminStatusLED());
      textCommand.Parameters.AddWithValue("OperStatusLED", (object) _interface.get_OperStatusLED());
      textCommand.Parameters.AddWithValue("InterfaceIcon", (object) _interface.get_InterfaceIcon());
      textCommand.Parameters.AddWithValue("Outbps", (object) _interface.get_OutBps());
      textCommand.Parameters.AddWithValue("Inbps", (object) _interface.get_InBps());
      textCommand.Parameters.AddWithValue("OutPercentUtil", (object) _interface.get_OutPercentUtil());
      textCommand.Parameters.AddWithValue("InPercentUtil", (object) _interface.get_InPercentUtil());
      textCommand.Parameters.AddWithValue("OutPps", (object) _interface.get_OutPps());
      textCommand.Parameters.AddWithValue("InPps", (object) _interface.get_InPps());
      textCommand.Parameters.AddWithValue("InPktSize", (object) _interface.get_InPktSize());
      textCommand.Parameters.AddWithValue("OutPktSize", (object) _interface.get_OutPktSize());
      textCommand.Parameters.AddWithValue("OutUcastPps", (object) _interface.get_OutUcastPps());
      textCommand.Parameters.AddWithValue("OutMcastPps", (object) _interface.get_OutMcastPps());
      textCommand.Parameters.AddWithValue("InUcastPps", (object) _interface.get_InUcastPps());
      textCommand.Parameters.AddWithValue("InMcastPps", (object) _interface.get_InMcastPps());
      textCommand.Parameters.AddWithValue("InDiscardsThisHour", (object) _interface.get_InDiscardsThisHour());
      textCommand.Parameters.AddWithValue("InDiscardsToday", (object) _interface.get_InDiscardsToday());
      textCommand.Parameters.AddWithValue("InErrorsThisHour", (object) _interface.get_InErrorsThisHour());
      textCommand.Parameters.AddWithValue("InErrorsToday", (object) _interface.get_InErrorsToday());
      textCommand.Parameters.AddWithValue("OutDiscardsThisHour", (object) _interface.get_OutDiscardsThisHour());
      textCommand.Parameters.AddWithValue("OutDiscardsToday", (object) _interface.get_OutDiscardsToday());
      textCommand.Parameters.AddWithValue("OutErrorsThisHour", (object) _interface.get_OutErrorsThisHour());
      textCommand.Parameters.AddWithValue("OutErrorsToday", (object) _interface.get_OutErrorsToday());
      textCommand.Parameters.AddWithValue("MaxInBpsToday", (object) _interface.get_MaxInBpsToday());
      if (_interface.get_MaxInBpsTime() == DateTime.MinValue)
        textCommand.Parameters.Add("@MaxInBpsTime", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@MaxInBpsTime", SqlDbType.DateTime).Value = (object) _interface.get_MaxInBpsTime();
      textCommand.Parameters.AddWithValue("MaxOutBpsToday", (object) _interface.get_MaxOutBpsToday());
      if (_interface.get_MaxOutBpsTime() == DateTime.MinValue)
        textCommand.Parameters.Add("@MaxOutBpsTime", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@MaxOutBpsTime", SqlDbType.DateTime).Value = (object) _interface.get_MaxOutBpsTime();
      if (_interface.get_NextRediscovery() == DateTime.MinValue)
        textCommand.Parameters.Add("@NextRediscovery", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@NextRediscovery", SqlDbType.DateTime).Value = (object) _interface.get_NextRediscovery();
      if (_interface.get_NextPoll() == DateTime.MinValue)
        textCommand.Parameters.Add("@NextPoll", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@NextPoll", SqlDbType.DateTime).Value = (object) _interface.get_NextPoll();
      textCommand.Parameters.AddWithValue("Counter64", (object) _interface.get_Counter64());
      textCommand.Parameters.AddWithValue("StatCollection", (object) _interface.get_StatCollection());
      if (_interface.get_LastSync() == DateTime.MinValue)
        textCommand.Parameters.Add("@LastSync", SqlDbType.DateTime).Value = (object) DBNull.Value;
      else
        textCommand.Parameters.Add("@LastSync", SqlDbType.DateTime).Value = (object) _interface.get_LastSync();
      textCommand.Parameters.AddWithValue("InterfaceAlias", (object) _interface.get_InterfaceAlias());
      textCommand.Parameters.AddWithValue("IfName", (object) _interface.get_IfName());
      textCommand.Parameters.AddWithValue("Severity", (object) _interface.get_Severity());
      textCommand.Parameters.AddWithValue("CustomBandwidth", (object) _interface.get_CustomBandwidth());
      textCommand.Parameters.AddWithValue("UnPluggable", (object) _interface.get_UnPluggable());
      textCommand.Parameters.AddWithValue("UnManaged", (object) _interface.get_UnManaged());
      textCommand.Parameters.AddWithValue("UnManageFrom", CommonHelper.GetDateTimeValue(_interface.get_UnManageFrom()));
      textCommand.Parameters.AddWithValue("UnManageUntil", CommonHelper.GetDateTimeValue(_interface.get_UnManageUntil()));
      textCommand.Parameters.AddWithValue("CollectAvailability", (object) _interface.get_CollectAvailability());
      InterfaceBLDAL.log.DebugFormat("Inserting interface. Locking thread. NodeID: {0}, FullName: {1}", (object) _interface.get_NodeID(), (object) _interface.get_FullName());
      lock (InterfaceBLDAL.insertInterfaceLock)
      {
        InterfaceBLDAL.log.DebugFormat("Inserting interface. Thread locked. NodeID: {0}, FullName: {1}", (object) _interface.get_NodeID(), (object) _interface.get_FullName());
        _interface.set_InterfaceID(Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand)));
        InterfaceBLDAL.log.DebugFormat("Interface inserted with ID: {0}. NodeID: {1}, FullName: {2}", (object) _interface.get_InterfaceID(), (object) _interface.get_NodeID(), (object) _interface.get_FullName());
      }
      return _interface.get_ID();
    }

    private static string SqlManagedInterfaceExists
    {
      get
      {
        return "IF NOT EXISTS (SELECT * FROM Interfaces WHERE \r\n                    NodeID = @NodeID AND \r\n                    PhysicalAddress = @PhysicalAddress AND\r\n                    InterfaceName = @InterfaceName AND\r\n                    InterfaceType = @InterfaceType AND\r\n                    InterfaceSubType = @InterfaceSubType AND\r\n                    (IfName = @IfName OR @IfName = '')\r\n                    )\r\n";
      }
    }

    internal static Interfaces GetInterfaces()
    {
      // ISSUE: method pointer
      return !InterfaceBLDAL._areInterfacesAllowed ? new Interfaces() : (Interfaces) Collection<int, Interface>.FillCollection<Interfaces>(new Collection<int, Interface>.CreateElement((object) null, __methodptr(CreateInterface)), "SELECT * FROM Interfaces", (SqlParameter[]) null);
    }

    internal static Interfaces GetNodeInterfaces(int nodeID)
    {
      if (!InterfaceBLDAL._areInterfacesAllowed)
        return new Interfaces();
      // ISSUE: method pointer
      return (Interfaces) Collection<int, Interface>.FillCollection<Interfaces>(new Collection<int, Interface>.CreateElement((object) null, __methodptr(CreateInterface)), "SELECT * FROM Interfaces WHERE NodeID=@NodeId", new SqlParameter[1]
      {
        new SqlParameter("@NodeId", (object) nodeID)
      });
    }

    internal static Interface GetInterface(int interfaceID)
    {
      // ISSUE: method pointer
      return Collection<int, Interface>.GetCollectionItem<Interfaces>(new Collection<int, Interface>.CreateElement((object) null, __methodptr(CreateInterface)), "SELECT * FROM Interfaces WHERE InterfaceID=@InterfaceID", new SqlParameter[1]
      {
        new SqlParameter("@InterfaceID", (object) interfaceID)
      });
    }

    [Obsolete("NPM module handles deleting interfaces. Core just sends SWIS InterfaceIndication.", true)]
    internal static void DeleteInterface(int interfaceID)
    {
      InterfaceBLDAL.DeleteInterface(InterfaceBLDAL.GetInterface(interfaceID));
    }

    [Obsolete("NPM module handles deleting interfaces. Core just sends SWIS InterfaceIndication.", true)]
    internal static void DeleteInterface(Interface _interface)
    {
      SqlCommand storedProcCommand = SqlHelper.GetStoredProcCommand("swsp_DeleteInterface");
      storedProcCommand.Parameters.Add("@id", SqlDbType.Int).Value = (object) _interface.get_ID();
      SqlHelper.ExecuteNonQuery(storedProcCommand);
      SqlCommand textCommand;
      using (textCommand = SqlHelper.GetTextCommand("delete from Pollers where NetObject = @NetObject"))
      {
        textCommand.Parameters.Add("@NetObject", SqlDbType.VarChar, 50).Value = (object) ("I:" + (object) _interface.get_ID());
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    internal static Interface CreateInterface(IDataReader reader)
    {
      Interface @interface = new Interface();
      for (int i = 0; i < reader.FieldCount; ++i)
      {
        string name = reader.GetName(i);
        switch (name)
        {
          case "AdminStatus":
            @interface.set_AdminStatus(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "AdminStatusLED":
            @interface.set_AdminStatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "Caption":
            @interface.set_Caption(DatabaseFunctions.GetString(reader, i));
            break;
          case "CollectAvailability":
            @interface.set_CollectAvailability(reader.IsDBNull(i) || DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "Counter64":
            @interface.set_Counter64(DatabaseFunctions.GetString(reader, i));
            break;
          case "CustomBandwidth":
            @interface.set_CustomBandwidth(DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "FullName":
            @interface.set_FullName(DatabaseFunctions.GetString(reader, i));
            break;
          case "IfName":
            @interface.set_IfName(DatabaseFunctions.GetString(reader, i));
            break;
          case "InBandwidth":
            @interface.set_InBandwidth(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "InDiscardsThisHour":
            @interface.set_InDiscardsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InDiscardsToday":
            @interface.set_InDiscardsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InErrorsThisHour":
            @interface.set_InErrorsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InErrorsToday":
            @interface.set_InErrorsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InMcastPps":
            @interface.set_InMcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InPercentUtil":
            @interface.set_InPercentUtil(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InPktSize":
            @interface.set_InPktSize(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "InPps":
            @interface.set_InPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InUcastPps":
            @interface.set_InUcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "Inbps":
            @interface.set_InBps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "InterfaceAlias":
            @interface.set_InterfaceAlias(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceID":
            @interface.set_InterfaceID(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceIcon":
            @interface.set_InterfaceIcon(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceIndex":
            @interface.set_InterfaceIndex(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceLastChange":
            @interface.set_InterfaceLastChange(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "InterfaceMTU":
            @interface.set_InterfaceMTU(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceName":
            @interface.set_InterfaceName(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceSpeed":
            @interface.set_InterfaceSpeed(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "InterfaceSubType":
            @interface.set_InterfaceSubType(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceType":
            @interface.set_InterfaceType(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "InterfaceTypeDescription":
            @interface.set_InterfaceTypeDescription(DatabaseFunctions.GetString(reader, i));
            break;
          case "InterfaceTypeName":
            @interface.set_InterfaceTypeName(DatabaseFunctions.GetString(reader, i));
            break;
          case "LastSync":
            @interface.set_LastSync(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxInBpsTime":
            @interface.set_MaxInBpsTime(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxInBpsToday":
            @interface.set_MaxInBpsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "MaxOutBpsTime":
            @interface.set_MaxOutBpsTime(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "MaxOutBpsToday":
            @interface.set_MaxOutBpsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "NextPoll":
            @interface.set_NextPoll(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NextRediscovery":
            @interface.set_NextRediscovery(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "NodeID":
            @interface.set_NodeID(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "ObjectSubType":
            @interface.set_ObjectSubType(DatabaseFunctions.GetString(reader, i));
            break;
          case "OperStatus":
            @interface.set_OperStatus(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "OperStatusLED":
            @interface.set_OperStatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "OutBandwidth":
            @interface.set_OutBandwidth(DatabaseFunctions.GetDouble(reader, i));
            break;
          case "OutDiscardsThisHour":
            @interface.set_OutDiscardsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutDiscardsToday":
            @interface.set_OutDiscardsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutErrorsThisHour":
            @interface.set_OutErrorsThisHour(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutErrorsToday":
            @interface.set_OutErrorsToday(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutMcastPps":
            @interface.set_OutMcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutPercentUtil":
            @interface.set_OutPercentUtil(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutPktSize":
            @interface.set_OutPktSize(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "OutPps":
            @interface.set_OutPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "OutUcastPps":
            @interface.set_OutUcastPps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "Outbps":
            @interface.set_OutBps(DatabaseFunctions.GetFloat(reader, i));
            break;
          case "PhysicalAddress":
            @interface.set_PhysicalAddress(DatabaseFunctions.GetString(reader, i));
            break;
          case "PollInterval":
            @interface.set_PollInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "RediscoveryInterval":
            @interface.set_RediscoveryInterval(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "Severity":
            @interface.set_Severity(DatabaseFunctions.GetInt32(reader, i));
            break;
          case "StatCollection":
            @interface.set_StatCollection(DatabaseFunctions.GetInt16(reader, i));
            break;
          case "Status":
            @interface.set_Status(DatabaseFunctions.GetString(reader, i));
            break;
          case "StatusLED":
            @interface.set_StatusLED(DatabaseFunctions.GetString(reader, i));
            break;
          case "UnManageFrom":
            @interface.set_UnManageFrom(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManageUntil":
            @interface.set_UnManageUntil(DatabaseFunctions.GetDateTime(reader, i));
            break;
          case "UnManaged":
            @interface.set_UnManaged(DatabaseFunctions.GetBoolean(reader, i));
            break;
          case "UnPluggable":
            @interface.set_UnPluggable(DatabaseFunctions.GetBoolean(reader, i));
            break;
          default:
            if (CustomPropertyMgr.IsCustom("Interfaces", name))
            {
              @interface.get_CustomProperties()[name] = reader[i];
              break;
            }
            InterfaceBLDAL.log.DebugFormat("Skipping Interface property {0}, value {1}", (object) name, reader[i]);
            break;
        }
      }
      return @interface;
    }
  }
}
