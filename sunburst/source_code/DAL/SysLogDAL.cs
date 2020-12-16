// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.SysLogDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class SysLogDAL
  {
    public static StringDictionary GetSeverities()
    {
      StringDictionary stringDictionary = new StringDictionary();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("Select SeverityCode, SeverityName From SysLogSeverities WITH(NOLOCK) Order By SeverityCode"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringDictionary.Add(DatabaseFunctions.GetByte(dataReader, "SeverityCode").ToString(), DatabaseFunctions.GetString(dataReader, "SeverityName"));
        }
      }
      return stringDictionary;
    }

    public static StringDictionary GetFacilities()
    {
      StringDictionary stringDictionary = new StringDictionary();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("Select FacilityCode, FacilityName From SysLogFacilities WITH(NOLOCK) Order By FacilityCode"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringDictionary.Add(DatabaseFunctions.GetByte(dataReader, "FacilityCode").ToString(), DatabaseFunctions.GetString(dataReader, "FacilityName"));
        }
      }
      return stringDictionary;
    }
  }
}
