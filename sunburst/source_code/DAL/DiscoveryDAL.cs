// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.DiscoveryDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using SolarWinds.Orion.Core.Models.Enums;
using SolarWinds.Orion.Core.Models.OldDiscoveryModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public static class DiscoveryDAL
  {
    public const string subTypeICMP = "ICMP";
    public const string subTypeSNMP = "SNMP";
    public const int UNDEFINED_VALUE = -2;

    [Obsolete("This method belongs to old discovery process.", true)]
    public static StartImportStatus ImportDiscoveryResults(
      Guid importID,
      List<DiscoveryResult> discoveryResults)
    {
      return (StartImportStatus) 2;
    }

    [Obsolete("This method belongs to old discovery process.", true)]
    public static bool IsImportInProgress(int discoveryProfileID)
    {
      return false;
    }

    [Obsolete("This method belongs to old discovery process.", true)]
    public static string GetCPUPollerTypeByOID(string oid)
    {
      return string.Empty;
    }

    [Obsolete("This method belongs to old discovery process.", true)]
    public static Intervals GetEnginesPollingIntervals(int engineID)
    {
      return new Intervals();
    }

    public static Intervals GetSettingsPollingIntervals()
    {
      return new Intervals()
      {
        RediscoveryInterval = (__Null) int.Parse(SettingsDAL.Get("SWNetPerfMon-Settings-Default Rediscovery Interval")),
        NodePollInterval = (__Null) int.Parse(SettingsDAL.Get("SWNetPerfMon-Settings-Default Node Poll Interval")),
        VolumePollInterval = (__Null) int.Parse(SettingsDAL.Get("SWNetPerfMon-Settings-Default Volume Poll Interval")),
        NodeStatPollInterval = (__Null) int.Parse(SettingsDAL.Get("SWNetPerfMon-Settings-Default Node Stat Poll Interval")),
        VolumeStatPollInterval = (__Null) int.Parse(SettingsDAL.Get("SWNetPerfMon-Settings-Default Volume Stat Poll Interval"))
      };
    }

    public static List<SnmpEntry> GetAllCredentials()
    {
      SqlCommand textCommand = SqlHelper.GetTextCommand("\r\n    Select Distinct 1 As SnmpVersion, CommunityString, Null as SNMPUser, Null as Context, Null as AuthPassword, Null as EncryptPassword, \r\n0 as AuthLevel, Null as AuthMethod, 0 as EncryptMethod From dbo.DiscoverySNMPCredentials\r\nUnion\r\n(\r\n\tSELECT 3 As SnmpVersion, Null as CommunityString, SNMPUser, Context, AuthPassword, EncryptPassword, AuthLevel, AuthMethod, EncryptMethod \r\n\tFROM DiscoverySNMPCredentialsV3\r\n)");
      List<SnmpEntry> snmpEntryList1 = new List<SnmpEntry>();
      SnmpEntry snmpEntry1 = new SnmpEntry();
      snmpEntry1.set_Name("public");
      snmpEntry1.set_Version((SNMPVersion) 1);
      snmpEntry1.set_Selected(true);
      snmpEntryList1.Add(snmpEntry1);
      SnmpEntry snmpEntry2 = new SnmpEntry();
      snmpEntry2.set_Name("private");
      snmpEntry2.set_Version((SNMPVersion) 1);
      snmpEntry2.set_Selected(true);
      snmpEntryList1.Add(snmpEntry2);
      List<SnmpEntry> snmpEntryList2 = snmpEntryList1;
      using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
      {
        while (dataReader.Read())
        {
          string str = DatabaseFunctions.GetString(dataReader, "CommunityString");
          int int32_1 = DatabaseFunctions.GetInt32(dataReader, "SnmpVersion");
          if (!str.Equals("public", StringComparison.OrdinalIgnoreCase) && !str.Equals("private", StringComparison.OrdinalIgnoreCase))
          {
            if (int32_1 == 3)
            {
              DiscoverySNMPCredentialsV3Entry.AuthenticationMethods int32_2 = (DiscoverySNMPCredentialsV3Entry.AuthenticationMethods) DatabaseFunctions.GetInt32(dataReader, "AuthMethod");
              DiscoverySNMPCredentialsV3Entry.EncryptionMethods int32_3 = (DiscoverySNMPCredentialsV3Entry.EncryptionMethods) DatabaseFunctions.GetInt32(dataReader, "EncryptMethod");
              SnmpEntry snmpEntry3 = new SnmpEntry();
              snmpEntry3.set_UserName(DatabaseFunctions.GetString(dataReader, "SNMPUser"));
              snmpEntry3.set_Context(DatabaseFunctions.GetString(dataReader, "Context"));
              snmpEntry3.set_AuthPassword(DatabaseFunctions.GetString(dataReader, "AuthPassword"));
              snmpEntry3.set_PrivPassword(DatabaseFunctions.GetString(dataReader, "EncryptPassword"));
              snmpEntry3.set_AuthLevel((SnmpAuthenticationLevel) DatabaseFunctions.GetInt32(dataReader, "AuthLevel"));
              snmpEntry3.set_AuthMethod(int32_2 == 2 ? (SnmpAuthMethod) 1 : (SnmpAuthMethod) 0);
              snmpEntry3.set_Version((SNMPVersion) 3);
              snmpEntry3.set_Selected(true);
              SnmpEntry snmpEntry4 = snmpEntry3;
              switch (int32_3 - 2)
              {
                case 0:
                  snmpEntry4.set_PrivMethod((SnmpPrivMethod) 1);
                  break;
                case 1:
                  snmpEntry4.set_PrivMethod((SnmpPrivMethod) 2);
                  break;
                case 2:
                  snmpEntry4.set_PrivMethod((SnmpPrivMethod) 3);
                  break;
                default:
                  snmpEntry4.set_PrivMethod((SnmpPrivMethod) 0);
                  break;
              }
              snmpEntryList2.Add(snmpEntry4);
            }
            else
            {
              List<SnmpEntry> snmpEntryList3 = snmpEntryList2;
              SnmpEntry snmpEntry3 = new SnmpEntry();
              snmpEntry3.set_Name(str);
              snmpEntry3.set_Version((SNMPVersion) 1);
              snmpEntry3.set_Selected(true);
              snmpEntryList3.Add(snmpEntry3);
            }
          }
        }
      }
      return snmpEntryList2;
    }
  }
}
