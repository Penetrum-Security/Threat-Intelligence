// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MibDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Models.Mib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class MibDAL
  {
    private static readonly Log _myLog = new Log();
    private static object _tokenLock = new object();
    private const string TreeColumns = "Index, MIB, Name, Primary, OID, Description, Access, Status, Units, Enum, TypeS";
    private const string EnumColumns = "Name, Value, Enum";
    private static CancellationTokenSource _cancellationTokenSource;

    private static CancellationTokenSource CancellationTokenSource
    {
      get
      {
        lock (MibDAL._tokenLock)
          return MibDAL._cancellationTokenSource;
      }
      set
      {
        lock (MibDAL._tokenLock)
          MibDAL._cancellationTokenSource = value;
      }
    }

    public Oid GetOid(string oid)
    {
      return this.GetOid(oid, true) ?? this.GetOid(oid, false);
    }

    private Oid GetOid(string oid, bool clean)
    {
      using (OleDbConnection dbConnection = MibHelper.GetDBConnection())
      {
        dbConnection.Open();
        return this.GetOid(oid, dbConnection, clean);
      }
    }

    private Oid GetOid(string oid, OleDbConnection connection, bool clean)
    {
      Oid oid1 = (Oid) null;
      using (OleDbCommand oleDbCommand = new OleDbCommand())
      {
        string empty = string.Empty;
        string str = !clean ? string.Format("Select TOP 1 {0} from Tree WHERE Primary = -1 AND OID=@Oid;", (object) "Index, MIB, Name, Primary, OID, Description, Access, Status, Units, Enum, TypeS") : string.Format("Select TOP 1 {0} from Tree WHERE Primary = -1 AND OID=@Oid AND Description <> 'unknown';", (object) "Index, MIB, Name, Primary, OID, Description, Access, Status, Units, Enum, TypeS");
        oleDbCommand.CommandText = str;
        oleDbCommand.Parameters.AddWithValue("Oid", (object) oid);
        using (IDataReader reader = OleDbHelper.ExecuteReader(oleDbCommand, connection))
        {
          if (reader.Read())
            oid1 = this.CreateOid(reader, connection);
        }
      }
      return oid1;
    }

    public MemoryStream GetIcon(string oid)
    {
      throw new NotImplementedException();
    }

    public Dictionary<string, MemoryStream> GetIcons()
    {
      byte[] numArray = new byte[0];
      Dictionary<string, MemoryStream> dictionary = new Dictionary<string, MemoryStream>();
      using (OleDbConnection dbConnection = MibHelper.GetDBConnection())
      {
        using (OleDbCommand oleDbCommand = new OleDbCommand())
        {
          dbConnection.Open();
          oleDbCommand.CommandText = "Select OID, [Small Icon] From Icons";
          using (IDataReader dataReader = OleDbHelper.ExecuteReader(oleDbCommand, dbConnection))
          {
            while (dataReader.Read())
            {
              if (!(dataReader["Small Icon"] is DBNull))
              {
                byte[] buffer = (byte[]) dataReader["Small Icon"];
                dictionary.Add(dataReader["OID"].ToString(), new MemoryStream(buffer, true));
              }
            }
          }
        }
      }
      return dictionary;
    }

    public Oids GetChildOids(string parentOid)
    {
      List<string> uniqueChildOids = this.GetUniqueChildOids(parentOid);
      Oids oids = new Oids();
      using (OleDbConnection dbConnection = MibHelper.GetDBConnection())
      {
        dbConnection.Open();
        foreach (string oid1 in uniqueChildOids)
        {
          Oid oid2 = this.GetOid(oid1, dbConnection, true) ?? this.GetOid(oid1, dbConnection, false);
          ((Collection<string, Oid>) oids).Add((object) oid2);
        }
      }
      return oids;
    }

    public List<string> GetUniqueChildOids(string parentOid)
    {
      List<string> stringList = new List<string>();
      using (OleDbConnection dbConnection = MibHelper.GetDBConnection())
      {
        dbConnection.Open();
        using (OleDbCommand oleDbCommand = new OleDbCommand())
        {
          oleDbCommand.CommandText = string.Format("Select DISTINCT Name, OID, Index from Tree WHERE Primary = -1 AND ParentOID=@parentOid order by index;", (object) "Index, MIB, Name, Primary, OID, Description, Access, Status, Units, Enum, TypeS");
          oleDbCommand.Parameters.AddWithValue(nameof (parentOid), (object) parentOid);
          using (IDataReader dataReader = OleDbHelper.ExecuteReader(oleDbCommand, dbConnection))
          {
            while (dataReader.Read())
              stringList.Add(DatabaseFunctions.GetString(dataReader, "OID"));
          }
        }
      }
      return stringList;
    }

    public OidEnums GetEnums(string enumName)
    {
      OidEnums oidEnums = new OidEnums();
      if (string.IsNullOrEmpty(enumName))
        return oidEnums;
      using (OleDbConnection dbConnection = MibHelper.GetDBConnection())
      {
        dbConnection.Open();
        using (OleDbCommand oleDbCommand = new OleDbCommand())
        {
          oleDbCommand.CommandText = string.Format("Select {0} from Enums WHERE Name=@name order by Value;", (object) "Name, Value, Enum");
          oleDbCommand.Parameters.AddWithValue("name", (object) enumName);
          using (IDataReader dataReader = OleDbHelper.ExecuteReader(oleDbCommand, dbConnection))
          {
            while (dataReader.Read())
            {
              OidEnum oidEnum = new OidEnum();
              oidEnum.set_Id(DatabaseFunctions.GetDouble(dataReader, 1).ToString());
              oidEnum.set_Name(DatabaseFunctions.GetString(dataReader, 2));
              ((Collection<string, OidEnum>) oidEnums).Add((object) oidEnum);
            }
          }
        }
      }
      return oidEnums;
    }

    public Oids GetSearchingOidsByDescription(string searchCriteria, string searchMIBsCriteria)
    {
      throw new NotImplementedException();
    }

    public void CancelRunningCommand()
    {
      if (MibDAL.CancellationTokenSource == null)
        return;
      try
      {
        MibDAL.CancellationTokenSource.Cancel();
      }
      catch (AggregateException ex)
      {
      }
    }

    public Oids GetSearchingOidsByName(string searchCriteria)
    {
      List<string> stringList = new List<string>();
      Oids oids = new Oids();
      using (OleDbConnection connection = MibHelper.GetDBConnection())
      {
        connection.Open();
        MibDAL.CancellationTokenSource = new CancellationTokenSource();
        using (OleDbCommand oleDbCommand = new OleDbCommand())
        {
          oleDbCommand.CommandText = string.Format("SELECT TOP 250 {0} FROM Tree WHERE (Primary = -1) AND ( Name LIKE @SearchValue OR Description LIKE '%' + @SearchValue + '%' OR Mib LIKE @SearchValue)", (object) "Index, MIB, Name, Primary, OID, Description, Access, Status, Units, Enum, TypeS");
          oleDbCommand.Parameters.AddWithValue("@SearchValue", (object) searchCriteria);
          using (IDataReader reader = OleDbHelper.ExecuteReader(oleDbCommand, connection))
          {
            using (IEnumerator<Oid> enumerator = ((Collection<string, Oid>) Task.Factory.StartNew<Oids>((Func<Oids>) (() => this.getOidsFromReader(reader, connection)), MibDAL.CancellationTokenSource.Token).Result).GetEnumerator())
            {
              while (((IEnumerator) enumerator).MoveNext())
              {
                Oid current = enumerator.Current;
                ((Collection<string, Oid>) oids).Add((object) current);
              }
            }
          }
        }
      }
      return oids;
    }

    private Oids getOidsFromReader(IDataReader reader, OleDbConnection connection)
    {
      Oids oids = new Oids();
      while (reader.Read())
      {
        MibDAL.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        Oid oid = this.CreateOid(reader, connection);
        ((Collection<string, Oid>) oids).Add((object) oid);
      }
      return oids;
    }

    public bool IsMibDatabaseAvailable()
    {
      return MibHelper.IsMIBDatabaseAvailable();
    }

    private Oid CreateOid(IDataReader reader, OleDbConnection connection)
    {
      Oid oid = new Oid();
      oid.set_ID(DatabaseFunctions.GetString(reader, 4));
      oid.set_Name(DatabaseFunctions.GetString(reader, 2));
      oid.set_Description(DatabaseFunctions.GetString(reader, 5));
      oid.set_MIB(DatabaseFunctions.GetString(reader, 1));
      oid.set_Access((AccessType) (int) DatabaseFunctions.GetByte(reader, 6));
      oid.set_Status((StatusType) (int) DatabaseFunctions.GetByte(reader, 7));
      oid.set_Units(DatabaseFunctions.GetString(reader, 8));
      oid.set_StringType(DatabaseFunctions.GetString(reader, 10));
      oid.set_HasChildren(this.HasChildren(oid.get_ID(), connection));
      oid.set_Enums(this.GetEnums(DatabaseFunctions.GetString(reader, 9)));
      oid.set_TreeIndex(DatabaseFunctions.GetInt32(reader, 0).ToString());
      MibHelper.CleanupDescription(oid);
      MibHelper.SetTypeInfo(oid);
      return oid;
    }

    private bool HasChildren(string oid, OleDbConnection connection)
    {
      using (OleDbCommand oleDbCommand = new OleDbCommand())
      {
        oleDbCommand.CommandText = "Select COUNT(*) from Tree WHERE Primary = -1 AND ParentOID=@oid;";
        oleDbCommand.Parameters.AddWithValue("parentOid", (object) oid);
        if ((int) OleDbHelper.ExecuteScalar(oleDbCommand, connection) > 0)
          return true;
      }
      return false;
    }

    private enum EnumColumnOrder
    {
      EnumName,
      EnumValue,
      EnumEnum,
    }
  }
}
