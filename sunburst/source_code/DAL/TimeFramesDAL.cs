// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.TimeFramesDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class TimeFramesDAL
  {
    public static List<TimeFrame> GetCoreTimeFrames(string timeFrameName = null)
    {
      string sql = "\r\nSELECT tf.TimeFrameID, tf.Name, tf.StartTime, tf.EndTime, tf.IsDisabled, tfd.DayOfWeek, tfd.WholeDay\r\nFROM TimeFrames AS tf\r\nINNER JOIN TimeFrameDays AS tfd ON tf.TimeFrameID = tfd.TimeFrameID\r\nWHERE Name LIKE 'Core_%'\r\n";
      return TimeFramesDAL.GetTimeFrames(timeFrameName, sql);
    }

    public static List<TimeFrame> GetAllTimeFrames(string timeFrameName = null)
    {
      string sql = "\r\nSELECT tf.TimeFrameID, tf.Name, tf.StartTime, tf.EndTime, tf.IsDisabled, tfd.DayOfWeek, tfd.WholeDay\r\nFROM TimeFrames AS tf\r\nINNER JOIN TimeFrameDays AS tfd ON tf.TimeFrameID = tfd.TimeFrameID\r\n";
      return TimeFramesDAL.GetTimeFrames(timeFrameName, sql);
    }

    private static List<TimeFrame> GetTimeFrames(string timeFrameName, string sql)
    {
      List<TimeFrame> timeFrameList = new List<TimeFrame>();
      if (timeFrameName != null)
        sql = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} AND Name = '{1}'", (object) sql, (object) timeFrameName);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(sql))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
          {
            int id = DatabaseFunctions.GetInt32(dataReader, "TimeFrameID");
            TimeFrame timeFrame = timeFrameList.Find((Predicate<TimeFrame>) (t => t.get_Id().Equals(id)));
            if (timeFrame == null)
            {
              timeFrame = new TimeFrame();
              timeFrame.set_Id(id);
              timeFrame.set_Name(DatabaseFunctions.GetString(dataReader, "Name"));
              if (dataReader["StartTime"] != DBNull.Value)
                timeFrame.set_StartTime(new DateTime?(DatabaseFunctions.GetDateTime(dataReader, "StartTime", DateTimeKind.Utc)));
              if (dataReader["EndTime"] != DBNull.Value)
                timeFrame.set_EndTime(new DateTime?(DatabaseFunctions.GetDateTime(dataReader, "EndTime", DateTimeKind.Utc)));
              timeFrame.set_WeekDays((IDictionary<int, bool>) new Dictionary<int, bool>());
              timeFrameList.Add(timeFrame);
            }
            timeFrame.get_WeekDays().Add(DatabaseFunctions.GetInt32(dataReader, "DayOfWeek"), DatabaseFunctions.GetBoolean(dataReader, "WholeDay"));
          }
        }
      }
      return timeFrameList;
    }
  }
}
