// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.CoreThresholdDataProvider
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.Thresholds;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  [Export(typeof (ThresholdDataProvider))]
  public class CoreThresholdDataProvider : ThresholdDataProvider
  {
    private const string PercentMemoryUsedName = "Nodes.Stats.PercentMemoryUsed";
    private const string ResponseTimeName = "Nodes.Stats.ResponseTime";
    private const string PercentLossName = "Nodes.Stats.PercentLoss";
    private const string CpuLoadName = "Nodes.Stats.CpuLoad";
    private const string PercentDiskUsedName = "Volumes.Stats.PercentDiskUsed";
    private const string PercentMemoryUsedChartName = "HostAvgPercentMemoryUsed";
    private const string ResponseTimeChartName = "MinMaxAvgRT";
    private const string PercentLossChartName = "PacketLossLine";
    private const string CpuLoadChartName = "CiscoMMAvgCPULoad";
    private const string PercentDiskUsedChartName = "PercentDiskUsage";

    public virtual IEnumerable<string> GetKnownThresholdNames()
    {
      yield return "Nodes.Stats.PercentMemoryUsed";
      yield return "Nodes.Stats.ResponseTime";
      yield return "Nodes.Stats.PercentLoss";
      yield return "Nodes.Stats.CpuLoad";
      yield return "Volumes.Stats.PercentDiskUsed";
    }

    public virtual Type GetThresholdDataProcessor()
    {
      return typeof (CoreThresholdProcessor);
    }

    public virtual StatisticalTableMetadata GetStatisticalTableMetadata(
      string thresholdName)
    {
      if (CoreThresholdDataProvider.IsResponseTime(thresholdName))
      {
        StatisticalTableMetadata statisticalTableMetadata = new StatisticalTableMetadata();
        statisticalTableMetadata.set_TableName("ResponseTime_Statistics");
        statisticalTableMetadata.set_InstanceIdColumnName("NodeID");
        statisticalTableMetadata.set_MeanColumnName("AvgResponseTimeMean");
        statisticalTableMetadata.set_StdDevColumnName("AvgResponseTimeStDev");
        statisticalTableMetadata.set_MinColumnName("AvgResponseTimeMin");
        statisticalTableMetadata.set_MaxColumnName("AvgResponseTimeMax");
        statisticalTableMetadata.set_CountColumnName("AvgResponseTimeCount");
        statisticalTableMetadata.set_MinDateTime("MinDateTime");
        statisticalTableMetadata.set_MaxDateTime("MaxDateTime");
        statisticalTableMetadata.set_Timestamp("Timestamp");
        return statisticalTableMetadata;
      }
      if (CoreThresholdDataProvider.IsPercentLoss(thresholdName))
      {
        StatisticalTableMetadata statisticalTableMetadata = new StatisticalTableMetadata();
        statisticalTableMetadata.set_TableName("ResponseTime_Statistics");
        statisticalTableMetadata.set_InstanceIdColumnName("NodeID");
        statisticalTableMetadata.set_MeanColumnName("PercentLossMean");
        statisticalTableMetadata.set_StdDevColumnName("PercentLossStDev");
        statisticalTableMetadata.set_MinColumnName("PercentLossMin");
        statisticalTableMetadata.set_MaxColumnName("PercentLossMax");
        statisticalTableMetadata.set_CountColumnName("PercentLossCount");
        statisticalTableMetadata.set_MinDateTime("MinDateTime");
        statisticalTableMetadata.set_MaxDateTime("MaxDateTime");
        statisticalTableMetadata.set_Timestamp("Timestamp");
        return statisticalTableMetadata;
      }
      if (CoreThresholdDataProvider.IsCpuLoad(thresholdName))
      {
        StatisticalTableMetadata statisticalTableMetadata = new StatisticalTableMetadata();
        statisticalTableMetadata.set_TableName("CPULoad_Statistics");
        statisticalTableMetadata.set_InstanceIdColumnName("NodeID");
        statisticalTableMetadata.set_MeanColumnName("AvgLoadMean");
        statisticalTableMetadata.set_StdDevColumnName("AvgLoadStDev");
        statisticalTableMetadata.set_MinColumnName("AvgLoadMin");
        statisticalTableMetadata.set_MaxColumnName("AvgLoadMax");
        statisticalTableMetadata.set_CountColumnName("AvgLoadCount");
        statisticalTableMetadata.set_MinDateTime("MinDateTime");
        statisticalTableMetadata.set_MaxDateTime("MaxDateTime");
        statisticalTableMetadata.set_Timestamp("Timestamp");
        return statisticalTableMetadata;
      }
      if (CoreThresholdDataProvider.IsPercentMemoryUsage(thresholdName))
      {
        StatisticalTableMetadata statisticalTableMetadata = new StatisticalTableMetadata();
        statisticalTableMetadata.set_TableName("CPULoad_Statistics");
        statisticalTableMetadata.set_InstanceIdColumnName("NodeID");
        statisticalTableMetadata.set_MeanColumnName("AvgPercentMemoryUsedMean");
        statisticalTableMetadata.set_StdDevColumnName("AvgPercentMemoryUsedStDev");
        statisticalTableMetadata.set_MinColumnName("AvgPercentMemoryUsedMin");
        statisticalTableMetadata.set_MaxColumnName("AvgPercentMemoryUsedMax");
        statisticalTableMetadata.set_CountColumnName("AvgPercentMemoryUsedCount");
        statisticalTableMetadata.set_MinDateTime("MinDateTime");
        statisticalTableMetadata.set_MaxDateTime("MaxDateTime");
        statisticalTableMetadata.set_Timestamp("Timestamp");
        return statisticalTableMetadata;
      }
      if (!CoreThresholdDataProvider.IsPercentDiskUsed(thresholdName))
        throw new InvalidOperationException(string.Format("Threshold name '{0}' is not supported.", (object) thresholdName));
      StatisticalTableMetadata statisticalTableMetadata1 = new StatisticalTableMetadata();
      statisticalTableMetadata1.set_TableName("VolumeUsage_Statistics");
      statisticalTableMetadata1.set_InstanceIdColumnName("VolumeID");
      statisticalTableMetadata1.set_MeanColumnName("PercentDiskUsedMean");
      statisticalTableMetadata1.set_StdDevColumnName("PercentDiskUsedStDev");
      statisticalTableMetadata1.set_MinColumnName("PercentDiskUsedMin");
      statisticalTableMetadata1.set_MaxColumnName("PercentDiskUsedMax");
      statisticalTableMetadata1.set_CountColumnName("PercentDiskUsedCount");
      statisticalTableMetadata1.set_MinDateTime("MinDateTime");
      statisticalTableMetadata1.set_MaxDateTime("MaxDateTime");
      statisticalTableMetadata1.set_Timestamp("Timestamp");
      return statisticalTableMetadata1;
    }

    public virtual ThresholdMinMaxValue GetThresholdMinMaxValues(
      string thresholdName,
      int instanceId)
    {
      if (CoreThresholdDataProvider.IsResponseTime(thresholdName))
      {
        ThresholdMinMaxValue thresholdMinMaxValue = new ThresholdMinMaxValue();
        thresholdMinMaxValue.set_Min(0.0);
        thresholdMinMaxValue.set_Max(100000.0);
        thresholdMinMaxValue.set_DataType(typeof (int));
        return thresholdMinMaxValue;
      }
      if (CoreThresholdDataProvider.IsPercentLoss(thresholdName))
      {
        ThresholdMinMaxValue thresholdMinMaxValue = new ThresholdMinMaxValue();
        thresholdMinMaxValue.set_Min(0.0);
        thresholdMinMaxValue.set_Max(100.0);
        thresholdMinMaxValue.set_DataType(typeof (int));
        return thresholdMinMaxValue;
      }
      if (CoreThresholdDataProvider.IsCpuLoad(thresholdName))
      {
        ThresholdMinMaxValue thresholdMinMaxValue = new ThresholdMinMaxValue();
        thresholdMinMaxValue.set_Min(0.0);
        thresholdMinMaxValue.set_Max(100.0);
        thresholdMinMaxValue.set_DataType(typeof (int));
        return thresholdMinMaxValue;
      }
      if (CoreThresholdDataProvider.IsPercentMemoryUsage(thresholdName))
      {
        ThresholdMinMaxValue thresholdMinMaxValue = new ThresholdMinMaxValue();
        thresholdMinMaxValue.set_Min(0.0);
        thresholdMinMaxValue.set_Max(100.0);
        thresholdMinMaxValue.set_DataType(typeof (double));
        return thresholdMinMaxValue;
      }
      if (!CoreThresholdDataProvider.IsPercentDiskUsed(thresholdName))
        throw new InvalidOperationException(string.Format("Threshold name '{0}' is not supported.", (object) thresholdName));
      ThresholdMinMaxValue thresholdMinMaxValue1 = new ThresholdMinMaxValue();
      thresholdMinMaxValue1.set_Min(0.0);
      thresholdMinMaxValue1.set_Max(100.0);
      thresholdMinMaxValue1.set_DataType(typeof (int));
      return thresholdMinMaxValue1;
    }

    public virtual StatisticalData[] GetStatisticalData(
      string thresholdName,
      int instanceId,
      DateTime minDateTimeInUtc,
      DateTime maxDateTimeInUtc)
    {
      string str;
      if (CoreThresholdDataProvider.IsResponseTime(thresholdName))
        str = "SELECT AvgResponseTime, [TimeStamp] FROM ResponseTime_CS_Detail WHERE NodeID = @nodeId AND ([TimeStamp] between @start and @end)";
      else if (CoreThresholdDataProvider.IsPercentLoss(thresholdName))
        str = "SELECT PercentLoss, [TimeStamp] FROM ResponseTime_CS_Detail WHERE NodeID = @nodeId AND ([TimeStamp] between @start and @end)";
      else if (CoreThresholdDataProvider.IsCpuLoad(thresholdName))
        str = "SELECT AvgLoad, [TimeStamp] FROM CPULoad_CS_Detail WHERE NodeID = @nodeId AND ([TimeStamp] between @start and @end)";
      else if (CoreThresholdDataProvider.IsPercentMemoryUsage(thresholdName))
      {
        str = "SELECT PercentMemoryUsed, [TimeStamp] FROM CPULoad_CS_Detail WHERE NodeID = @nodeId AND ([TimeStamp] between @start and @end)";
      }
      else
      {
        if (!CoreThresholdDataProvider.IsPercentDiskUsed(thresholdName))
          throw new InvalidOperationException(string.Format("Threshold name '{0}' is not supported.", (object) thresholdName));
        str = "SELECT PercentDiskUsed, [TimeStamp] FROM VolumeUsage_CS_Detail WHERE VolumeID = @instanceId AND ([TimeStamp] between @start and @end)";
      }
      List<StatisticalData> statisticalDataList = new List<StatisticalData>();
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Parameters.AddWithValue(nameof (instanceId), (object) instanceId).SqlDbType = SqlDbType.Int;
          textCommand.Parameters.AddWithValue("start", (object) minDateTimeInUtc.ToLocalTime()).SqlDbType = SqlDbType.DateTime;
          textCommand.Parameters.AddWithValue("end", (object) maxDateTimeInUtc.ToLocalTime()).SqlDbType = SqlDbType.DateTime;
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
          {
            while (dataReader.Read())
            {
              if (!dataReader.IsDBNull(0) && !dataReader.IsDBNull(1))
                statisticalDataList.Add(new StatisticalData()
                {
                  Value = (__Null) Convert.ToDouble(dataReader[0]),
                  Date = (__Null) DatabaseFunctions.GetDateTime(dataReader, 1, DateTimeKind.Local)
                });
            }
          }
        }
      }
      return statisticalDataList.ToArray();
    }

    public virtual string GetThresholdInstanceName(string thresholdName, int instanceId)
    {
      string str = !CoreThresholdDataProvider.IsPercentDiskUsed(thresholdName) ? "SELECT [Caption] FROM [NodesData] WHERE [NodeId] = @instanceId" : "SELECT [Caption] FROM [Volumes] WHERE [VolumeId] = @instanceId";
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Connection = connection;
          textCommand.Parameters.AddWithValue(nameof (instanceId), (object) instanceId).SqlDbType = SqlDbType.Int;
          object obj = textCommand.ExecuteScalar();
          return obj != null && obj != DBNull.Value ? obj.ToString() : string.Empty;
        }
      }
    }

    public virtual string GetStatisticalDataChartName(string thresholdName)
    {
      if (CoreThresholdDataProvider.IsResponseTime(thresholdName))
        return "MinMaxAvgRT";
      if (CoreThresholdDataProvider.IsPercentLoss(thresholdName))
        return "PacketLossLine";
      if (CoreThresholdDataProvider.IsCpuLoad(thresholdName))
        return "CiscoMMAvgCPULoad";
      if (CoreThresholdDataProvider.IsPercentMemoryUsage(thresholdName))
        return "HostAvgPercentMemoryUsed";
      if (CoreThresholdDataProvider.IsPercentDiskUsed(thresholdName))
        return "PercentDiskUsage";
      throw new InvalidOperationException(string.Format("Threshold name '{0}' is not supported.", (object) thresholdName));
    }

    private static bool IsResponseTime(string thresholdName)
    {
      return string.Equals(thresholdName, "Nodes.Stats.ResponseTime", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPercentLoss(string thresholdName)
    {
      return string.Equals(thresholdName, "Nodes.Stats.PercentLoss", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCpuLoad(string thresholdName)
    {
      return string.Equals(thresholdName, "Nodes.Stats.CpuLoad", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPercentMemoryUsage(string thresholdName)
    {
      return string.Equals(thresholdName, "Nodes.Stats.PercentMemoryUsed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPercentDiskUsed(string thresholdName)
    {
      return string.Equals(thresholdName, "Volumes.Stats.PercentDiskUsed", StringComparison.OrdinalIgnoreCase);
    }

    public CoreThresholdDataProvider()
    {
      base.\u002Ector();
    }
  }
}
