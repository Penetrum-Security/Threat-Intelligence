// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.NodeBLDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.EntityManager;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.PackageManager;
using SolarWinds.Orion.Core.Strings;
using SolarWinds.Orion.MacroProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class NodeBLDAL : INodeBLDAL
  {
    private static readonly Log log = new Log();
    private static readonly List<string> _fields = new List<string>()
    {
      "DynamicIP",
      "UnManaged",
      "UnManageFrom",
      "CommunityString",
      "RWCommunity",
      "Vendor",
      "SysObjectID",
      "Location",
      "Contact",
      "RediscoveryInterval",
      "PollInterval",
      "IOSImage",
      "IOSVersion",
      "StatusDescription",
      "Status",
      "MachineType",
      "Allow64BitCounters",
      "SNMPPort",
      "SNMPVersion",
      "SNMPV3Username",
      "SNMPV3Context",
      "VMwareProductName",
      "VMwareProductVersion",
      "VMPollingJobID",
      "VMServiceURIID",
      "VMCredentialID",
      "External",
      "ChildStatus",
      "IsServer"
    };
    private static bool _areInterfacesAllowed = ((IPackageManager) SolarWinds.Orion.Core.Common.PackageManager.PackageManager.InstanceWithCache).IsPackageInstalled("Orion.Interfaces");
    private const string Columns = "NodeID, Caption, DNS, Vendor, IP_Address, EngineID, Community, VendorIcon, GroupStatus, \r\n\t\t\tSNMPVersion, AgentPort, SNMPV3Username, SNMPV3AuthKey, SNMPV3PrivKey, SNMPV3AuthMethod, SNMPV3PrivMethod, \r\n\t\t\tObjectSubType, StatCollection, PollInterval, UnManaged, UnManageFrom, UnManageUntil, NextRediscovery, NextPoll, LastBoot, SysObjectID, Description, Location, Contact, IOSImage, IOSVersion, StatusDescription, Status, MachineType,\r\n\t\t\tAllow64BitCounters, DynamicIP,RediscoveryInterval, RWCommunity, SNMPV3Context, RWSNMPV3Username, RWSNMPV3Context, RWSNMPV3PrivMethod, RWSNMPV3PrivKey, RWSNMPV3AuthMethod, RWSNMPV3AuthKey, External, ChildStatus, IsServer";

    public static SolarWinds.Orion.Core.Common.Models.Node CleanInsertNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      node = NodeDAL.InsertNode(node, false);
      return node;
    }

    public static void UpdateEntityType(int nodeId, string entityType)
    {
      NodeDAL.UpdateEntityType(nodeId, entityType);
    }

    public static void CleanUpdateNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      NodeDAL.UpdateNode(node, false);
    }

    public static Nodes GetNodesWithIPs(string[] addresses)
    {
      StringBuilder stringBuilder = new StringBuilder("SELECT * FROM dbo.Nodes \r\nWHERE Nodes.IP_Address IN ('");
      stringBuilder.Append(string.Join("','", addresses));
      stringBuilder.Append("')");
      // ISSUE: method pointer
      Nodes nodes = (Nodes) Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.FillCollection<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), stringBuilder.ToString(), (SqlParameter[]) null, (M1[]) new bool[2]);
      if (((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).get_Count() > 0)
      {
        using (IEnumerator<Interface> enumerator = ((Collection<int, Interface>) InterfaceBLDAL.GetNodesInterfaces(nodes.get_NodesIDs())).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Interface current = enumerator.Current;
            ((Collection<int, Interface>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(current.get_NodeID())?.get_Interfaces()).Add((object) current);
          }
        }
        using (IEnumerator<Volume> enumerator = ((Collection<int, Volume>) VolumeDAL.GetNodesVolumes(nodes.get_NodesIDs())).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Volume current = enumerator.Current;
            ((Collection<int, Volume>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(current.get_NodeID())?.get_Volumes()).Add((object) current);
          }
        }
      }
      return nodes;
    }

    public static Nodes GetNodes()
    {
      return NodeBLDAL.GetNodes(true, true);
    }

    public static Nodes GetNodes(bool includeInterfaces, bool includeVolumes)
    {
      // ISSUE: method pointer
      Nodes nodes = (Nodes) Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.FillCollection<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), "SELECT * FROM dbo.Nodes", (SqlParameter[]) null, (M1[]) new bool[2]);
      if (includeInterfaces && NodeBLDAL._areInterfacesAllowed)
      {
        using (IEnumerator<Interface> enumerator = ((Collection<int, Interface>) InterfaceBLDAL.GetInterfaces()).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Interface current = enumerator.Current;
            ((Collection<int, Interface>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(current.get_NodeID())?.get_Interfaces()).Add((object) current);
          }
        }
      }
      if (includeVolumes)
      {
        using (IEnumerator<Volume> enumerator = ((Collection<int, Volume>) VolumeDAL.GetVolumes()).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Volume current = enumerator.Current;
            ((Collection<int, Volume>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(current.get_NodeID())?.get_Volumes()).Add((object) current);
          }
        }
      }
      return nodes;
    }

    public static Nodes GetNodesByIds(int[] nodeIds)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = string.Empty;
      foreach (int nodeId in nodeIds)
      {
        stringBuilder.AppendFormat("{0}{1}", (object) str, (object) nodeId);
        str = ",";
      }
      // ISSUE: method pointer
      return (Nodes) Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.FillCollection<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), string.Format("SELECT * FROM dbo.Nodes WHERE dbo.Nodes.NodeID in ({0})", (object) stringBuilder), (SqlParameter[]) null, (M1[]) new bool[2]);
    }

    public static int GetNodeCount()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT COUNT(*) FROM Nodes WITH (NOLOCK)"))
        return (int) SqlHelper.ExecuteScalar(textCommand);
    }

    public static List<string> GetFields()
    {
      return NodeBLDAL._fields;
    }

    public static Dictionary<string, string> GetVendors()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT DISTINCT Vendor, SysObjectID FROM Nodes WITH (NOLOCK)"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
          {
            string key = DatabaseFunctions.GetString(dataReader, "SysObjectID");
            if (!dictionary.ContainsKey(key))
              dictionary.Add(key, DatabaseFunctions.GetString(dataReader, "Vendor"));
          }
        }
      }
      return dictionary;
    }

    public static List<int> GetSortedNodeIDs()
    {
      List<int> intList = new List<int>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT NodeID FROM Nodes WITH (NOLOCK) ORDER BY Vendor, Caption"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            intList.Add(DatabaseFunctions.GetInt32(dataReader, "NodeID"));
        }
      }
      return intList;
    }

    public static SolarWinds.Orion.Core.Common.Models.Node GetNode(int nodeId)
    {
      // ISSUE: method pointer
      return Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.GetCollectionItem<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), "Select * from dbo.Nodes WHERE Nodes.NodeID=@NodeId", new SqlParameter[1]
      {
        new SqlParameter("@NodeId", (object) nodeId)
      }, (M1[]) new bool[2]{ true, true });
    }

    public static SolarWinds.Orion.Core.Common.Models.Node CreateNode(
      IDataReader reader,
      params bool[] getObjects)
    {
      return getObjects.Length > 1 ? NodeBLDAL.CreateNode(reader, getObjects[0], getObjects[1]) : (SolarWinds.Orion.Core.Common.Models.Node) null;
    }

    private static SolarWinds.Orion.Core.Common.Models.Node CreateNode(
      IDataReader reader,
      bool getInterfaces,
      bool getVolumes)
    {
      SolarWinds.Orion.Core.Common.Models.Node node = NodeDAL.CreateNode(reader, new bool[2]
      {
        getInterfaces,
        getVolumes
      });
      if (getInterfaces)
        node.set_Interfaces(InterfaceBLDAL.GetNodeInterfaces(node.get_ID()));
      if (getVolumes)
        node.set_Volumes(VolumeDAL.GetNodeVolumes(node.get_ID()));
      return node;
    }

    public static SolarWinds.Orion.Core.Common.Models.Node GetNodeWithOptions(
      int nodeId,
      bool includeInterfaces,
      bool includeVolumes)
    {
      // ISSUE: method pointer
      SolarWinds.Orion.Core.Common.Models.Node collectionItem = Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.GetCollectionItem<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), "Select * from dbo.Nodes WHERE Nodes.NodeID=@NodeId", new SqlParameter[1]
      {
        new SqlParameter("@NodeId", (object) nodeId)
      }, (M1[]) new bool[2]
      {
        includeInterfaces,
        includeVolumes
      });
      if (collectionItem != null)
        return collectionItem;
      throw new ArgumentOutOfRangeException(nameof (nodeId), string.Format("Node Id {0} does not exist", (object) nodeId));
    }

    public static SolarWinds.Orion.Core.Common.Models.Node InsertNode(
      SolarWinds.Orion.Core.Common.Models.Node node,
      bool allowDuplicates = false)
    {
      node = NodeDAL.InsertNode(node, allowDuplicates);
      new NodesCustomPropertyDAL().UpdateCustomProperties(node);
      NodeSettingsDAL.InsertSettings(node.get_ID(), node.get_NodeSettings());
      return node;
    }

    public static void UpdateNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      if (NodeDAL.UpdateNode(node, false) <= 0)
        return;
      NodeSettingsDAL.InsertSettings(node.get_ID(), node.get_NodeSettings());
      new NodesCustomPropertyDAL().UpdateCustomProperties(node);
    }

    public static void DeleteNode(int nodeId)
    {
      SolarWinds.Orion.Core.Common.Models.Node node = NodeBLDAL.GetNode(nodeId);
      if (node == null)
        throw new ArgumentOutOfRangeException(nameof (nodeId), string.Format("Node Id {0} does not exist", (object) nodeId));
      NodeBLDAL.DeleteNode(node);
      NodeSettingsDAL.DeleteNodeSettings(nodeId);
      NodeNotesDAL.DeleteNodeNotes(nodeId);
    }

    public static void DeleteNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      NodeDAL.DeleteNode(node);
      if (node.get_Volumes() != null)
      {
        using (IEnumerator<Volume> enumerator = ((Collection<int, Volume>) node.get_Volumes()).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
            VolumeDAL.DeleteVolume(enumerator.Current);
        }
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM Pollers WHERE NetObjectType=@NetObjectType AND NetObjectID=@NetObjectID"))
      {
        textCommand.Parameters.Add("@NetObjectType", SqlDbType.VarChar, 5).Value = (object) "N";
        textCommand.Parameters.Add("@NetObjectID", SqlDbType.Int).Value = (object) node.get_ID();
        SqlHelper.ExecuteNonQuery(textCommand);
      }
      EventsDAL.InsertEvent(node.get_ID(), node.get_ID(), "N", 17, string.Format(Resources.get_COREBLCODE_IB0_1(), (object) node.get_Caption()));
    }

    public static float GetAvailability(int nodeID, DateTime startDate, DateTime endDate)
    {
      SqlCommand textCommand = SqlHelper.GetTextCommand("select avg(Availability)  from [ResponseTime] where (NodeID = @NodeID) and (DateTime > @StartDate) and (DateTime < @EndDate)");
      textCommand.Parameters.Add("@NodeID", SqlDbType.Int).Value = (object) nodeID;
      textCommand.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = (object) startDate;
      textCommand.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = (object) endDate;
      float result = -1f;
      float.TryParse(SqlHelper.ExecuteScalar(textCommand).ToString(), out result);
      return result;
    }

    public static bool IsNodeWireless(int nodeId)
    {
      return NodeBLDAL.IsPollingEnabled(nodeId, "N.Wireless%");
    }

    public static bool IsNodeEnergyWise(int nodeId)
    {
      return NodeBLDAL.IsPollingEnabled(nodeId, "N.EnergyWise%");
    }

    private static bool IsPollingEnabled(int nodeId, string pollerTypePattern)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT PollerID from Pollers WHERE PollerType LIKE @pollerTypePattern AND NetObjectID = @nodeId AND NetObjectType ='N'"))
      {
        textCommand.Parameters.AddWithValue("@nodeId", (object) nodeId);
        textCommand.Parameters.AddWithValue("@pollerTypePattern", (object) pollerTypePattern);
        return Convert.ToBoolean(SqlHelper.ExecuteScalar(textCommand));
      }
    }

    public static NodeHardwareType GetNodeHardwareType(int nodeId)
    {
      return (NodeHardwareType) 0;
    }

    public static void BulkUpdateNodePollingInterval(int pollInterval, int engineId)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("IF (@engineID <=0) UPDATE NodesData SET PollInterval = @PollInterval ELSE UPDATE NodesData SET PollInterval = @PollInterval WHERE EngineID = @engineID"))
      {
        textCommand.Parameters.Add("@PollInterval", SqlDbType.Int, 4).Value = (object) pollInterval;
        textCommand.Parameters.Add("@engineID", SqlDbType.Int, 4).Value = (object) engineId;
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static List<KeyValuePair<Uri, List<int>>> EnumerateNodeEngineAssigments()
    {
      return NodeDAL.EnumerateNodeEngineAssigments();
    }

    public static List<int> EnumerateUnmanagedNodes()
    {
      return NodeDAL.EnumerateUnmanagedNodes();
    }

    internal static Dictionary<string, int> GetValuesAndCountsForPropertyFiltered(
      string property,
      string accountId,
      Dictionary<string, object> filters)
    {
      List<string> stringList = new List<string>(filters.Count);
      List<SqlParameter> sqlParameterList = new List<SqlParameter>(filters.Count);
      foreach (KeyValuePair<string, object> filter in filters)
      {
        stringList.Add(string.Format("ISNULL(Nodes.[{0}],'')=@{0}", (object) filter.Key));
        sqlParameterList.Add(new SqlParameter(filter.Key, filter.Value ?? (object) string.Empty));
      }
      string str = string.Empty;
      if (stringList.Count > 0)
        str = " WHERE " + string.Join(" AND ", stringList.ToArray());
      Dictionary<string, int> dictionary = new Dictionary<string, int>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(Limitation.LimitSQL(string.Format("SELECT rtrim(ltrim({0})), Count({0}), {0}, " + property + " FROM Nodes WITH (NOLOCK)" + str + " GROUP BY rtrim(ltrim({0})), {0}, " + property, (object) string.Format("ISNULL({0},'')", (object) property)), accountId)))
      {
        textCommand.Parameters.AddRange(sqlParameterList.ToArray());
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
          {
            if (dataReader[2] is bool)
              dictionary.Add(dataReader[2].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
            else if (dataReader[2] is int)
              dictionary.Add(dataReader[3].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
            else if (dataReader[2] is float)
            {
              if (string.IsNullOrEmpty(dataReader[3].ToString()))
                dictionary.Add(dataReader[3].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
              else
                dictionary.Add(DatabaseFunctions.GetFloat(dataReader, 2).ToString((IFormatProvider) CultureInfo.InvariantCulture.NumberFormat), DatabaseFunctions.GetInt32(dataReader, 1));
            }
            else if (dataReader[2] is DateTime)
            {
              if (dataReader[3].GetType() == typeof (DBNull))
                dictionary.Add(dataReader[3].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
              else
                dictionary.Add(dataReader[0].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
            }
            else
              dictionary.Add(dataReader[0].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
          }
        }
      }
      return dictionary;
    }

    internal static Dictionary<string, int> GetValuesAndCountsForProperty(
      string property,
      string accountId)
    {
      return NodeBLDAL.GetValuesAndCountsForProperty(property, accountId, new CultureInfo("en-us"));
    }

    internal static Dictionary<string, int> GetValuesAndCountsForProperty(
      string property,
      string accountId,
      CultureInfo culture)
    {
      Dictionary<string, int> dictionary = new Dictionary<string, int>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(Limitation.LimitSQL(string.Format("SELECT {0}, Count({0}), " + property + " FROM Nodes WITH (NOLOCK) GROUP BY {0}, " + property, (object) string.Format("ISNULL({0},'')", (object) property)), accountId)))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
          {
            if (dataReader[0].GetType() == typeof (DateTime))
              dictionary.Add(dataReader.GetDateTime(0).ToString((IFormatProvider) culture), DatabaseFunctions.GetInt32(dataReader, 1));
            else if (dataReader[0] is float)
            {
              if (string.IsNullOrEmpty(dataReader[2].ToString()))
                dictionary.Add(dataReader[2].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
              else
                dictionary.Add(dataReader.GetFloat(0).ToString((IFormatProvider) culture.NumberFormat), DatabaseFunctions.GetInt32(dataReader, 1));
            }
            else if (dataReader[0] is int || dataReader[0] is byte)
            {
              if (string.IsNullOrEmpty(dataReader[2].ToString()))
                dictionary.Add(dataReader[2].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
              else
                dictionary.Add(dataReader[0].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
            }
            else
              dictionary.Add(dataReader[0].ToString(), DatabaseFunctions.GetInt32(dataReader, 1));
          }
        }
      }
      return dictionary;
    }

    internal static List<string> GetNodeDistinctValuesForField(string fieldName)
    {
      List<string> stringList = new List<string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT DISTINCT " + fieldName + " AS Field FROM Nodes WITH (NOLOCK)"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringList.Add(dataReader["Field"].ToString());
        }
      }
      return stringList;
    }

    internal static Nodes GetNodesFiltered(
      Dictionary<string, object> filterValues,
      bool includeInterfaces,
      bool includeVolumes)
    {
      List<string> stringList = new List<string>(filterValues.Count);
      List<SqlParameter> sqlParameterList = new List<SqlParameter>(filterValues.Count);
      string str1 = "''";
      foreach (KeyValuePair<string, object> filterValue in filterValues)
      {
        if (filterValue.Value != null && !string.IsNullOrEmpty(filterValue.Value.ToString()))
        {
          if (CustomPropertyMgr.IsCustom("NodesCustomProperties", filterValue.Key))
          {
            try
            {
              string lowerInvariant = CustomPropertyMgr.GetTypeForProp("NodesCustomProperties", filterValue.Key).Name.ToLowerInvariant();
              if (!(lowerInvariant == "single") && !(lowerInvariant == "float"))
              {
                if (!(lowerInvariant == "double"))
                  goto label_14;
              }
              double result;
              if (!double.TryParse(filterValue.Value.ToString(), out result))
              {
                string s = filterValue.Value.ToString();
                if (s.Contains("."))
                  s = s.Replace(".", ",");
                else if (s.Contains(","))
                  s = s.Replace(",", ".");
                double.TryParse(s, out result);
              }
              stringList.Add(string.Format("ISNULL(Nodes.[{0}],{1})=@{0}", (object) filterValue.Key, (object) str1));
              sqlParameterList.Add(new SqlParameter(filterValue.Key, filterValue.Value));
              continue;
            }
            catch (Exception ex)
            {
              NodeBLDAL.log.Error((object) "Error while trying to convert float custom property:", ex);
            }
          }
        }
label_14:
        stringList.Add(string.Format("ISNULL(Nodes.[{0}],{1})=@{0}", (object) filterValue.Key, (object) str1));
        if (filterValue.Value != null && (filterValue.Value.ToString().ToLowerInvariant().Equals("true") || filterValue.Value.ToString().ToLowerInvariant().Equals("false")))
          sqlParameterList.Add(new SqlParameter(filterValue.Key, filterValue.Value.ToString().ToLowerInvariant().Equals("true") ? (object) "1" : (object) "0"));
        else
          sqlParameterList.Add(new SqlParameter(filterValue.Key, filterValue.Value ?? (object) string.Empty));
      }
      string str2 = string.Empty;
      if (stringList.Count > 0)
        str2 = " WHERE " + string.Join(" AND ", stringList.ToArray());
      // ISSUE: method pointer
      Nodes nodes = (Nodes) Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.FillCollection<Nodes, bool>((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<M1>) new Collection<int, SolarWinds.Orion.Core.Common.Models.Node>.CreateElementWithParams<bool>((object) null, __methodptr(CreateNode)), "SELECT * from dbo.Nodes " + str2 + " ORDER BY Caption", sqlParameterList.ToArray(), (M1[]) new bool[2]);
      if (includeInterfaces && NodeBLDAL._areInterfacesAllowed)
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT Interfaces.* FROM Interfaces \r\nINNER JOIN Nodes on Interfaces.NodeID=Nodes.NodeID" + str2 + " ORDER BY Interfaces.Caption"))
        {
          foreach (SqlParameter sqlParameter in sqlParameterList)
            textCommand.Parameters.AddWithValue(sqlParameter.ParameterName, sqlParameter.Value);
          using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
          {
            while (reader.Read())
            {
              Interface @interface = InterfaceBLDAL.CreateInterface(reader);
              ((Collection<int, Interface>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(@interface.get_NodeID()).get_Interfaces()).Add((object) @interface);
            }
          }
        }
      }
      if (includeVolumes)
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT Volumes.* FROM Volumes \r\nINNER JOIN Nodes on Volumes.NodeID=Nodes.NodeID" + str2 + " ORDER BY Volumes.Caption"))
        {
          foreach (SqlParameter sqlParameter in sqlParameterList)
            textCommand.Parameters.AddWithValue(sqlParameter.ParameterName, sqlParameter.Value);
          using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
          {
            while (reader.Read())
            {
              Volume volume = VolumeDAL.CreateVolume(reader);
              ((Collection<int, Volume>) ((Collection<int, SolarWinds.Orion.Core.Common.Models.Node>) nodes).FindByID(volume.get_NodeID()).get_Volumes()).Add((object) volume);
            }
          }
        }
      }
      return nodes;
    }

    internal static Dictionary<string, string> GetVendorIconFileNames()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT DISTINCT Vendor, VendorIcon FROM Nodes WITH (NOLOCK)"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            dictionary[DatabaseFunctions.GetString(dataReader, 0)] = DatabaseFunctions.GetString(dataReader, 1).Trim();
        }
      }
      return dictionary;
    }

    internal static void PopulateWebCommunityStrings()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT WebCommunityStrings\r\nSELECT NEWID() AS [GUID], Nodes.Community AS CommunityString FROM Nodes\r\nWHERE NOT EXISTS (SELECT * FROM WebCommunityStrings WHERE Nodes.Community = WebCommunityStrings.CommunityString)\r\nGROUP BY Nodes.Community"))
      {
        int num = SqlHelper.ExecuteNonQuery(textCommand);
        if (num > 0)
          NodeBLDAL.log.InfoFormat("Added {0} rows to WebCommunityStrings.", (object) num);
        else
          NodeBLDAL.log.Debug((object) "PopulateWebCommunityStrings - no rows added.");
      }
    }

    public static Dictionary<string, object> GetNodeCustomProperties(
      int nodeId,
      ICollection<string> properties)
    {
      SolarWinds.Orion.Core.Common.Models.Node node = NodeBLDAL.GetNode(nodeId);
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      if (properties == null || properties.Count == 0)
        properties = node.get_CustomProperties().Keys;
      MacroParser macroParser1 = new MacroParser(new Action<string, int>(BusinessLayerOrionEvent.WriteEvent));
      macroParser1.set_ObjectType("Node");
      macroParser1.set_ActiveObject(node.get_ID().ToString());
      macroParser1.set_NodeID(node.get_ID());
      macroParser1.set_NodeName(node.get_Name());
      MacroParser macroParser2 = macroParser1;
      SqlConnection connection;
      macroParser2.set_MyDBConnection(connection = DatabaseFunctions.CreateConnection());
      using (connection)
      {
        foreach (string property in (IEnumerable<string>) properties)
        {
          string key = property.Trim();
          if (node.get_CustomProperties().ContainsKey(key))
          {
            object customProperty = node.get_CustomProperties()[key];
            dictionary[key] = customProperty == null || !customProperty.ToString().Contains("${") ? customProperty : (object) macroParser2.ParseMacros(customProperty.ToString(), false);
          }
        }
      }
      return dictionary;
    }

    public static DataTable GetPagebleNodes(
      string property,
      string proptype,
      string val,
      string column,
      string direction,
      int number,
      int size,
      string searchText)
    {
      size = Math.Max(size, 15);
      bool flag = ((IEntityManager) SolarWinds.Orion.Core.Common.EntityManager.EntityManager.InstanceWithCache).IsThereEntity("Orion.NPM.EW.Entity");
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nSelect [NodeID], [IP_Address], [Caption], [Status], [StatusLED]\r\nfrom (SELECT N.*, ROW_NUMBER() OVER (ORDER BY {0} {1}) RowNr from [Nodes] N {4} WHERE {5}) t\r\nWHERE RowNr BETWEEN {2} AND {3} \r\nORDER BY {0} {1}", string.IsNullOrEmpty(column) ? (object) "Caption" : (object) column, string.IsNullOrEmpty(direction) ? (object) "ASC" : (object) direction, (object) (number + 1), (object) (number + size), flag ? (object) " LEFT JOIN NPM_NV_EW_NODES_V EW ON (EW.NodeID = N.NodeID) " : (object) "", (object) (NodeBLDAL.GetWhere("N", property, proptype, val) + (string.IsNullOrEmpty(searchText) ? string.Empty : string.Format(" And (N.Caption Like '{0}')", (object) searchText))))))
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      if (dataTable != null)
        dataTable.TableName = "PagebleNodes";
      return dataTable;
    }

    public static int GetNodesCount(
      string property,
      string proptype,
      string val,
      string searchText)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT COUNT(N.NodeID) AS TotalCount FROM [Nodes] N {0} WHERE {1}", ((IEntityManager) SolarWinds.Orion.Core.Common.EntityManager.EntityManager.InstanceWithCache).IsThereEntity("Orion.NPM.EW.Entity") ? (object) " LEFT JOIN NPM_NV_EW_NODES_V EW ON (EW.NodeID = N.NodeID) " : (object) "", (object) (NodeBLDAL.GetWhere("N", property, proptype, val) + (string.IsNullOrEmpty(searchText) ? string.Empty : string.Format(" And (N.Caption Like '{0}')", (object) searchText))))))
        return Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
    }

    public static DataTable GetGroupsByNodeProperty(string property, string propertyType)
    {
      string empty = string.Empty;
      string str = !"SYSTEM.SINGLE,SYSTEM.INT32,SYSTEM.DATETIME".Contains(propertyType.ToUpperInvariant()) ? string.Format("SELECT ISNULL(N.{0},'') AS Value, COUNT(ISNULL(N.{0},'')) AS Cnt FROM Nodes N WITH (NOLOCK) GROUP BY ISNULL(N.{0},'') ORDER BY ISNULL(N.{0},'') ASC", (object) property) : string.Format("SELECT N.{0} AS Value, COUNT(ISNULL(N.{0},'')) AS Cnt FROM Nodes N WITH (NOLOCK) GROUP BY N.{0} ORDER BY N.{0} ASC", (object) property);
      if (property.Equals("EnergyWise", StringComparison.OrdinalIgnoreCase))
        str = "SELECT EW.EnergyWise AS Value, COUNT(ISNULL(EW.EnergyWise,'')) AS Cnt FROM NPM_NV_EW_NODES_V EW LEFT JOIN Nodes N ON (EW.NodeID = N.NodeID) GROUP BY EW.EnergyWise";
      DataTable dataTable = new DataTable();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      if (dataTable != null)
        dataTable.TableName = "GroupsByNodeProperty";
      return dataTable;
    }

    public static string GetWhere(string tableAlias, string prop, string type, string value)
    {
      string str = "System.Single,System.Double,System.Int32";
      if (string.IsNullOrEmpty(prop))
        return "1=1";
      if (prop.Equals("EnergyWise", StringComparison.OrdinalIgnoreCase))
        tableAlias = "EW";
      if (prop.Equals("MachineType", StringComparison.OrdinalIgnoreCase) && (value.Equals("null", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(value.Trim())))
        return string.Format(" ({0}.{1} IS NULL OR {0}.{1}='' OR {0}.{1}='{2}') ", (object) tableAlias, (object) prop, (object) Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_8());
      if (!value.Equals("null", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value.Trim()))
        return string.Format(" {0}.{1}='{2}' ", (object) tableAlias, (object) prop, (object) value.Replace("'", "''"));
      return !string.IsNullOrEmpty(type) && str.Contains(type) ? string.Format(" ({0}.{1} IS NULL) ", (object) tableAlias, (object) prop) : string.Format(" ({0}.{1} IS NULL OR {0}.{1}='') ", (object) tableAlias, (object) prop);
    }

    public static DataTable GetNodeCPUsByPercentLoad(
      int nodeId,
      int pageNumber,
      int pageSize)
    {
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("declare @@cpuCount int\r\n\r\nSelect @@cpuCount = count(Distinct CPUIndex)  FROM CPUMultiLoad\r\nWHERE NodeID = @NodeId\r\n\r\nif @@cpuCount <= 1\r\n Select top 0 CPUIndex AS CPUName, CPUIndex, AvgLoad, TimeStampUTC from CPUMultiLoad\r\nelse\r\nSelect * from\r\n(\r\nSelect ROW_NUMBER() OVER (order by z.AvgLoad DESC) as rnbr, * from\r\n(\r\nSELECT N'{0}' + CAST(CPUIndex + 1 as varchar(6)) as CPUName, CPUIndex, AvgLoad, TimeStampUTC,\r\n DENSE_RANK() OVER (PARTITION BY CPUIndex ORDER BY TimeStampUTC desc) AS Rank\r\n FROM CPUMultiLoad\r\nWHERE NodeID = @NodeId) as z\r\nwhere z.Rank = 1\r\n) t1 where t1.rnbr >((@PageNumber-1)*@PageSize) and t1.rnbr <=((@PageNumber)*@PageSize)\r\nORDER BY AvgLoad DESC\r\n", (object) string.Format(Resources.get_LIBCODE_IB0_1(), (object) string.Empty))))
      {
        textCommand.Parameters.AddWithValue("@NodeId", (object) nodeId);
        textCommand.Parameters.AddWithValue("@PageNumber", (object) pageNumber);
        textCommand.Parameters.AddWithValue("@PageSize", (object) pageSize);
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      }
      if (dataTable != null)
        dataTable.TableName = "NodeCPUsByPercentLoad";
      return dataTable;
    }

    public static DataTable GetNodesCpuIndexCounts(List<string> nodeIds)
    {
      if (nodeIds == null || nodeIds.Count == 0)
        return (DataTable) null;
      DataTable dataTable;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT DISTINCT(z.NodeID) as ID, z.Caption, COUNT(z.CPUIndex) As Cnt from\r\n(SELECT cpu.NodeID, cpu.CPUIndex, n.Caption, cpu.TimeStampUTC,\r\n DENSE_RANK() OVER (PARTITION BY cpu.NodeID ORDER BY cpu.TimeStampUTC desc) AS [Rank]\r\n FROM CPUMultiLoad cpu\r\n INNER JOIN Nodes n ON cpu.NodeID = n.NodeID\r\nWHERE cpu.NodeID in ({0}) \r\n) as z\r\nwhere z.Rank = 1\r\nGROUP BY z.NodeID, z.Caption\r\nORDER BY z.Caption", (object) string.Join(",", nodeIds.ToArray()))))
        dataTable = SqlHelper.ExecuteDataTable(textCommand);
      if (dataTable != null)
        dataTable.TableName = "NodeCPUsByPercentLoad";
      return dataTable;
    }

    public static void UpdateNodeProperty(IDictionary<string, object> properties, int nodeId)
    {
      if (properties == null)
        throw new ArgumentNullException("Node properties must be set");
      if (properties.Count == 0)
        return;
      List<string> stringList1 = new List<string>()
      {
        "ObjectSubType",
        "IP_Address",
        "IP_Address_Type",
        "DynamicIP",
        "UnManaged",
        "UnManageFrom",
        "UnManageUntil",
        "Caption",
        "DNS",
        "Community",
        "RWCommunity",
        "SysName",
        "Vendor",
        "SysObjectID",
        "Description",
        "Location",
        "Contact",
        "RediscoveryInterval",
        "PollInterval",
        "VendorIcon",
        "IOSImage",
        "IOSVersion",
        "GroupStatus",
        "StatusDescription",
        "Status",
        "StatusLED",
        "ChildStatus",
        "EngineID",
        "MachineType",
        "Severity",
        "StatCollection",
        "Allow64BitCounters",
        "SNMPV2Only",
        "AgentPort",
        "SNMPVersion",
        "SNMPV3Username",
        "SNMPV3Context",
        "SNMPV3PrivMethod",
        "SNMPV3PrivKey",
        "SNMPV3PrivKeyIsPwd",
        "SNMPV3AuthMethod",
        "SNMPV3AuthKey",
        "SNMPV3AuthKeyIsPwd",
        "RWSNMPV3Username",
        "RWSNMPV3Context",
        "RWSNMPV3PrivMethod",
        "RWSNMPV3PrivKey",
        "RWSNMPV3PrivKeyIsPwd",
        "RWSNMPV3AuthMethod",
        "RWSNMPV3AuthKey",
        "RWSNMPV3AuthKeyIsPwd",
        "TotalMemory",
        "External",
        "EntityType",
        "CMTS",
        "BlockUntil",
        "IPAddressGUID"
      };
      List<string> stringList2 = new List<string>()
      {
        "LastBoot",
        "SystemUpTime",
        "LastSystemUpTimePollUtc",
        "ResponseTime",
        "PercentLoss",
        "AvgResponseTime",
        "MinResponseTime",
        "MaxResponseTime",
        "NextPoll",
        "LastSync",
        "NextRediscovery",
        "CPULoad",
        "MemoryUsed",
        "PercentMemoryUsed",
        "BufferNoMemThisHour",
        "BufferNoMemToday",
        "BufferSmMissThisHour",
        "BufferSmMissToday",
        "BufferMdMissThisHour",
        "BufferMdMissToday",
        "BufferBgMissThisHour",
        "BufferBgMissToday",
        "BufferLgMissThisHour",
        "BufferLgMissToday",
        "BufferHgMissThisHour",
        "BufferHgMissToday",
        "CustomPollerLastStatisticsPoll",
        "CustomPollerLastStatisticsPollSuccess"
      };
      List<string> stringList3 = new List<string>();
      List<string> stringList4 = new List<string>();
      List<SqlParameter> sqlParameterList = new List<SqlParameter>(properties.Count);
      foreach (string key in (IEnumerable<string>) properties.Keys)
      {
        if (stringList1.Contains(key))
          stringList3.Add(string.Format("[{0}]=@{0}", (object) key));
        if (stringList2.Contains(key))
          stringList4.Add(string.Format("[{0}]=@{0}", (object) key));
        if (properties[key] == null || properties[key] == DBNull.Value || string.IsNullOrEmpty(properties[key].ToString()))
          sqlParameterList.Add(new SqlParameter("@" + key, (object) DBNull.Value));
        else
          sqlParameterList.Add(new SqlParameter("@" + key, properties[key]));
      }
      string str = "";
      if (stringList3.Count > 0 && stringList4.Count > 0)
        str = string.Format("UPDATE [NodesData] SET {0} WHERE NodeID=@node; UPDATE [NodesStatistics] SET {1} WHERE NodeID=@node;", (object) string.Join(", ", stringList3.ToArray()), (object) string.Join(", ", stringList4.ToArray()));
      if (stringList3.Count > 0 && stringList4.Count == 0)
        str = string.Format("UPDATE [NodesData] SET {0} WHERE NodeID=@node", (object) string.Join(", ", stringList3.ToArray()));
      if (stringList3.Count == 0 && stringList4.Count > 0)
        str = string.Format("UPDATE [NodesStatistics] SET {0} WHERE NodeID=@node", (object) string.Join(", ", stringList4.ToArray()));
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
      {
        foreach (SqlParameter sqlParameter in sqlParameterList)
          textCommand.Parameters.Add(sqlParameter);
        textCommand.Parameters.AddWithValue("node", (object) nodeId);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static void DeleteShadowNodes(string ipAddress)
    {
      if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
        return;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM ShadowNodes WHERE IPAddress=@IPAddress"))
      {
        textCommand.Parameters.AddWithValue("IPAddress", (object) ipAddress);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    SolarWinds.Orion.Core.Common.Models.Node INodeBLDAL.GetNode(int nodeId)
    {
      return NodeBLDAL.GetNode(nodeId);
    }

    SolarWinds.Orion.Core.Common.Models.Node INodeBLDAL.GetNodeWithOptions(
      int nodeId,
      bool includeInterfaces,
      bool includeVolumes)
    {
      return NodeBLDAL.GetNodeWithOptions(nodeId, includeInterfaces, includeVolumes);
    }

    void INodeBLDAL.UpdateNode(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      NodeBLDAL.UpdateNode(node);
    }

    Nodes INodeBLDAL.GetNodes(bool includeInterfaces, bool includeVolumes)
    {
      return NodeBLDAL.GetNodes(includeInterfaces, includeVolumes);
    }

    void INodeBLDAL.UpdateNode(IDictionary<string, object> properties, int nodeId)
    {
      NodeBLDAL.UpdateNodeProperty(properties, nodeId);
    }

    public static bool AddNodeNote(
      int nodeId,
      string accountId,
      string note,
      DateTime modificationDateTime)
    {
      int? nullable = new int?();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("INSERT INTO [NodeNotes] ({0}) OUTPUT inserted.NodeNoteID VALUES ({1})", (object) "Note, NodeID, AccountID, TimeStamp", (object) "@Note, @NodeID, @AccountID, @TimeStamp")))
      {
        textCommand.Parameters.AddWithValue("@Note", (object) note);
        textCommand.Parameters.AddWithValue("@NodeID", (object) nodeId);
        textCommand.Parameters.AddWithValue("@AccountID", (object) accountId);
        textCommand.Parameters.AddWithValue("@TimeStamp", (object) modificationDateTime);
        using (SqlConnection connection = DatabaseFunctions.CreateConnection())
        {
          object obj = SqlHelper.ExecuteScalar(textCommand, connection);
          if (obj != null)
          {
            if (obj != DBNull.Value)
              nullable = new int?(Convert.ToInt32(obj));
          }
        }
      }
      return nullable.HasValue;
    }

    private enum ColumnOrder
    {
      ID,
      Name,
      DNS,
      Vendor,
      IpAddress,
      EngineID,
      Community,
      VendorIcon,
      GroupStatus,
      SNMPVersion,
      SNMPPort,
      SNMPV3Username,
      SNMPV3AuthKey,
      SNMPV3PrivKey,
      SNMPV3AuthType,
      SNMPV3PrivType,
      ObjectSubType,
      StatCollection,
      PollInterval,
      UnManaged,
      UnManageFrom,
      UnManageUntil,
      NextRediscovery,
      NextPoll,
      LastBoot,
      SysObjectID,
      Description,
      Location,
      Contact,
      IOSImage,
      IOSVersion,
      StatusDescription,
      Status,
      MachineType,
      Allow64BitCounters,
      DynamicIP,
      RediscoveryInterval,
      RWCommunity,
      SNMPV3Context,
      RWSNMPV3Username,
      RWSNMPV3Context,
      RWSNMPV3PrivMethod,
      RWSNMPV3PrivKey,
      RWSNMPV3AuthMethod,
      RWSNMPV3AuthKey,
      External,
      ChildStatus,
      IsServer,
    }
  }
}
