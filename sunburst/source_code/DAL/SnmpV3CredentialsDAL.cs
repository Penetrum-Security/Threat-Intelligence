// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.SnmpV3CredentialsDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class SnmpV3CredentialsDAL
  {
    public static List<string> GetCredentialsSet()
    {
      List<string> stringList = new List<string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT CredentialName FROM SNMPV3Credentials ORDER BY CredentialName"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringList.Add(dataReader[0].ToString());
        }
      }
      return stringList;
    }

    public static void InsertCredentials(SnmpCredentials crendentials)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT INTO [SNMPV3Credentials]\r\n           ([CredentialName]\r\n           ,[SNMPV3Username]\r\n           ,[SNMPV3Context]\r\n           ,[SNMPV3PrivMethod]\r\n           ,[SNMPV3PrivKey]\r\n           ,[SNMPV3PrivKeyIsPwd]\r\n           ,[SNMPV3AuthKey]\r\n           ,[SNMPV3AuthMethod]\r\n           ,[SNMPV3AuthKeyIsPwd])\r\n     VALUES\r\n           (@CredentialName\r\n           ,@SNMPV3Username\r\n           ,@SNMPV3Context\r\n           ,@SNMPV3PrivMethod\r\n           ,@SNMPV3PrivKey\r\n           ,@SNMPV3PrivKeyIsPwd\r\n           ,@SNMPV3AuthKey\r\n           ,@SNMPV3AuthMethod\r\n           ,@SNMPV3AuthKeyIsPwd)"))
      {
        textCommand.Parameters.Add("@CredentialName", SqlDbType.NVarChar, 200).Value = (object) crendentials.get_CredentialName();
        textCommand.Parameters.Add("@SNMPV3Username", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SNMPv3UserName();
        textCommand.Parameters.Add("@SNMPV3Context", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SnmpV3Context();
        textCommand.Parameters.Add("@SNMPV3PrivMethod", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SNMPv3PrivacyType().ToString();
        textCommand.Parameters.Add("@SNMPV3PrivKey", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SNMPv3PrivacyPassword();
        textCommand.Parameters.Add("@SNMPV3PrivKeyIsPwd", SqlDbType.Bit).Value = (object) crendentials.get_SNMPV3PrivKeyIsPwd();
        textCommand.Parameters.Add("@SNMPV3AuthKey", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SNMPv3AuthPassword();
        textCommand.Parameters.Add("@SNMPV3AuthMethod", SqlDbType.NVarChar, 50).Value = (object) crendentials.get_SNMPv3AuthType().ToString();
        textCommand.Parameters.Add("@SNMPV3AuthKeyIsPwd", SqlDbType.Bit).Value = (object) crendentials.get_SNMPV3AuthKeyIsPwd();
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static void DeleteCredentials(string CredentialName)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM [SNMPV3Credentials] WHERE CredentialName = @CredentialName"))
      {
        textCommand.Parameters.Add("@CredentialName", SqlDbType.NVarChar, 200).Value = (object) CredentialName;
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static SnmpCredentials GetCredentials(string CredentialName)
    {
      SnmpCredentials snmpCredentials = new SnmpCredentials();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM SNMPV3Credentials WHERE CredentialName = @CredentialName"))
      {
        textCommand.Parameters.Add("@CredentialName", SqlDbType.NVarChar, 200).Value = (object) CredentialName;
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          if (dataReader.Read())
          {
            for (int i = 0; i < dataReader.FieldCount; ++i)
            {
              switch (dataReader.GetName(i))
              {
                case nameof (CredentialName):
                  snmpCredentials.set_CredentialName(DatabaseFunctions.GetString(dataReader, i));
                  break;
                case "SNMPV3AuthKey":
                  snmpCredentials.set_SNMPv3AuthPassword(DatabaseFunctions.GetString(dataReader, i));
                  break;
                case "SNMPV3AuthKeyIsPwd":
                  snmpCredentials.set_SNMPV3AuthKeyIsPwd(DatabaseFunctions.GetBoolean(dataReader, i));
                  break;
                case "SNMPV3AuthMethod":
                  snmpCredentials.set_SNMPv3AuthType((SNMPv3AuthType) Enum.Parse(typeof (SNMPv3AuthType), DatabaseFunctions.GetString(dataReader, i)));
                  break;
                case "SNMPV3Context":
                  snmpCredentials.set_SnmpV3Context(DatabaseFunctions.GetString(dataReader, i));
                  break;
                case "SNMPV3PrivKey":
                  snmpCredentials.set_SNMPv3PrivacyPassword(DatabaseFunctions.GetString(dataReader, i));
                  break;
                case "SNMPV3PrivKeyIsPwd":
                  snmpCredentials.set_SNMPV3PrivKeyIsPwd(DatabaseFunctions.GetBoolean(dataReader, i));
                  break;
                case "SNMPV3PrivMethod":
                  snmpCredentials.set_SNMPv3PrivacyType((SNMPv3PrivacyType) Enum.Parse(typeof (SNMPv3PrivacyType), DatabaseFunctions.GetString(dataReader, i)));
                  break;
                case "SNMPV3Username":
                  snmpCredentials.set_SNMPv3UserName(DatabaseFunctions.GetString(dataReader, i));
                  break;
              }
            }
          }
        }
      }
      return snmpCredentials;
    }

    public static void UpdateCredentials(SnmpCredentials credentials)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE [SNMPV3Credentials]\r\n\t\t\t\t\t\t\t\tSET [SNMPV3Username] = @SNMPV3Username\r\n\t\t\t\t\t\t\t\t,[SNMPV3Context] = @SNMPV3Context\r\n\t\t\t\t\t\t\t\t,[SNMPV3PrivMethod] = @SNMPV3PrivMethod\r\n\t\t\t\t\t\t\t\t,[SNMPV3PrivKey] = @SNMPV3PrivKey\r\n\t\t\t\t\t\t\t\t,[SNMPV3PrivKeyIsPwd] = @SNMPV3PrivKeyIsPwd\r\n\t\t\t\t\t\t\t\t,[SNMPV3AuthKey] = @SNMPV3AuthKey\r\n\t\t\t\t\t\t\t\t,[SNMPV3AuthMethod] = @SNMPV3AuthMethod\r\n\t\t\t\t\t\t\t\t,[SNMPV3AuthKeyIsPwd] = @SNMPV3AuthKeyIsPwd\r\n\t\t\t\t\t\t\t\tWHERE [CredentialName] = @CredentialName"))
      {
        textCommand.Parameters.Add("@CredentialName", SqlDbType.NVarChar, 200).Value = (object) credentials.get_CredentialName();
        textCommand.Parameters.Add("@SNMPV3Username", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SNMPv3UserName();
        textCommand.Parameters.Add("@SNMPV3Context", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SnmpV3Context();
        textCommand.Parameters.Add("@SNMPV3PrivMethod", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SNMPv3PrivacyType().ToString();
        textCommand.Parameters.Add("@SNMPV3PrivKey", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SNMPv3PrivacyPassword();
        textCommand.Parameters.Add("@SNMPV3PrivKeyIsPwd", SqlDbType.Bit).Value = (object) credentials.get_SNMPV3PrivKeyIsPwd();
        textCommand.Parameters.Add("@SNMPV3AuthKey", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SNMPv3AuthPassword();
        textCommand.Parameters.Add("@SNMPV3AuthMethod", SqlDbType.NVarChar, 50).Value = (object) credentials.get_SNMPv3AuthType().ToString();
        textCommand.Parameters.Add("@SNMPV3AuthKeyIsPwd", SqlDbType.Bit).Value = (object) credentials.get_SNMPV3AuthKeyIsPwd();
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }
  }
}
