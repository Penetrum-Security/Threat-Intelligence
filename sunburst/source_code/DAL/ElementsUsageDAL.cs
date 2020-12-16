// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.ElementsUsageDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class ElementsUsageDAL
  {
    public static void Save(
      IEnumerable<ElementLicenseSaturationInfo> saturationInfoCollection)
    {
      if (saturationInfoCollection == null)
        throw new ArgumentNullException(nameof (saturationInfoCollection));
      if (!saturationInfoCollection.Any<ElementLicenseSaturationInfo>())
        return;
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(" IF NOT EXISTS (SELECT 1 FROM [dbo].[ElementUsage_Daily] WHERE Date = @Date AND ElementType = @ElementType)\r\n                INSERT INTO [dbo].[ElementUsage_Daily] (Date, ElementType, Count, MaxCount) VALUES (@Date, @ElementType, @Count, @MaxCount)"))
        {
          textCommand.Parameters.Add("@Date", SqlDbType.Date);
          textCommand.Parameters.Add("@ElementType", SqlDbType.NVarChar);
          textCommand.Parameters.Add("@Count", SqlDbType.Int);
          textCommand.Parameters.Add("@MaxCount", SqlDbType.Int);
          using (IEnumerator<ElementLicenseSaturationInfo> enumerator = saturationInfoCollection.GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              ElementLicenseSaturationInfo current = enumerator.Current;
              textCommand.Parameters["@Date"].Value = (object) DateTime.UtcNow.Date;
              textCommand.Parameters["@ElementType"].Value = (object) current.get_ElementType();
              textCommand.Parameters["@Count"].Value = (object) current.get_Count();
              textCommand.Parameters["@MaxCount"].Value = (object) current.get_MaxCount();
              SqlHelper.ExecuteNonQuery(textCommand, connection);
            }
          }
        }
      }
    }
  }
}
