// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MibHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.NPM.Common.Models;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Models.Mib;
using System;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public static class MibHelper
  {
    private static object connectionLock = new object();
    private static Log log = new Log();
    private static OleDbConnection connection;
    private static string dbConnectionString;

    private static OleDbConnection CurrentConnection
    {
      get
      {
        lock (MibHelper.connectionLock)
          return MibHelper.connection;
      }
      set
      {
        lock (MibHelper.connectionLock)
          MibHelper.connection = value;
      }
    }

    public static void ForceConnectionClose()
    {
      OleDbConnection.ReleaseObjectPool();
      MibHelper.CurrentConnection.Close();
    }

    public static void CleanupDescription(Oid oid)
    {
      oid.set_Description(oid.get_Description().Replace("\r\n", "\n"));
      oid.set_Description(oid.get_Description().Replace("\r", "\n"));
      oid.set_Description(oid.get_Description().Replace("\n", "\r\n"));
    }

    public static void SetTypeInfo(Oid oid)
    {
      if (((Collection<string, OidEnum>) oid.get_Enums()).get_Count() > 0)
      {
        oid.set_PollType((CustomPollerType) 2);
        oid.set_VariableType((OidVariableType) 12);
      }
      else
      {
        switch (oid.get_StringType().ToUpper())
        {
          case "BITS":
          case "DISPLAYSTRING":
          case "DISPLAY_STRING":
          case "OCTECTSTRING":
          case "OPAQUE":
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 1);
            break;
          case "COUNTER":
          case "COUNTER32":
          case "COUNTER64":
            oid.set_PollType((CustomPollerType) 0);
            oid.set_VariableType((OidVariableType) 5);
            break;
          case "GAUGE":
          case "GAUGE32":
            oid.set_PollType((CustomPollerType) 1);
            oid.set_VariableType((OidVariableType) 6);
            break;
          case "INTEGER":
          case "UINTEGER32":
          case "UNSIGNEDINTEGER32":
            oid.set_PollType((CustomPollerType) 1);
            oid.set_VariableType((OidVariableType) 6);
            break;
          case "IP":
          case "IP ADDRESS":
          case "IP-ADDRESS":
          case "IP_ADDRESS":
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 4);
            break;
          case "OBJECT IDENTIFIER":
          case "OBJECT-IDENTIFIER":
          case "OBJECTIDENTIFIER":
          case "OBJECT_IDENTIFIER":
          case "OID":
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 1);
            break;
          case "SEQUENCE":
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 17);
            break;
          case "TIMETICKS":
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 13);
            break;
          default:
            oid.set_PollType((CustomPollerType) 2);
            oid.set_VariableType((OidVariableType) 0);
            break;
        }
      }
    }

    public static OleDbConnection GetDBConnection()
    {
      if (string.IsNullOrEmpty(MibHelper.dbConnectionString))
      {
        StringBuilder stringBuilder = new StringBuilder("Provider=Microsoft.Jet.OLEDB.4.0;");
        stringBuilder.Append("Data Source=");
        stringBuilder.Append(MibHelper.FindMibDbPath() + "MIBs.cfg");
        stringBuilder.Append(";Mode=Read;OLE DB Services=-1;Persist Security Info=False;Jet OLEDB:Database ");
        stringBuilder.Append("Password=SW_MIBs");
        MibHelper.dbConnectionString = stringBuilder.ToString();
      }
      return MibHelper.CurrentConnection = new OleDbConnection(MibHelper.dbConnectionString);
    }

    private static string FindMibDbPath()
    {
      string str = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\SolarWinds\\";
      if (File.Exists(str + "MIBs.cfg"))
        return str;
      MibHelper.log.DebugFormat("Could not find MIBs Database. Please, download MIBs Database from http://solarwinds.s3.amazonaws.com/solarwinds/Release/MIB-Database/MIBs.zip and decompress the MIBs.cfg file to " + str + " to correct this problem", Array.Empty<object>());
      throw new ApplicationException("Unable to determine Mibs.cfg location");
    }

    public static bool IsMIBDatabaseAvailable()
    {
      try
      {
        MibHelper.FindMibDbPath();
        return true;
      }
      catch (ApplicationException ex)
      {
        return false;
      }
    }

    public static string FormatSearchCriteria(string searchCriteria)
    {
      string str1 = string.Empty;
      string str2 = searchCriteria;
      char[] chArray = new char[1]{ ' ' };
      foreach (string str3 in str2.Split(chArray))
      {
        if (!string.IsNullOrEmpty(str3.Trim()))
          str1 = str1 + str3.Replace("*", "%") + " ";
      }
      return str1.TrimEnd();
    }
  }
}
