// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.ThresholdProcessingEngine
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Data;
using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.Settings;
using SolarWinds.Orion.Core.Common.Thresholds;
using SolarWinds.Orion.Core.Models;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class ThresholdProcessingEngine
  {
    private static readonly Log _log = new Log();
    private int _batchSize = 10000;
    private string _baselineTimeFrame = "Core_All";
    private readonly IThresholdIndicator _thresholdIndicator;
    private readonly IThresholdDataProcessor[] _thresholdProcessors;
    private readonly ThresholdDataProvider[] _thresholdDataProviders;
    private readonly ICollectorSettings _settings;
    private readonly IDictionary<string, IThresholdDataProcessor> _dataProcessorsDictionary;
    private readonly IDictionary<string, ThresholdDataProvider> _dataProvidersDictionary;

    internal ThresholdProcessingEngine(
      IEnumerable<IThresholdDataProcessor> thresholdProcessors,
      IEnumerable<ThresholdDataProvider> thresholdDataProviders,
      IThresholdIndicator thresholdIndicator,
      ICollectorSettings settings)
    {
      IThresholdIndicator thresholdIndicator1 = thresholdIndicator;
      if (thresholdIndicator1 == null)
        throw new ArgumentNullException(nameof (thresholdIndicator));
      this._thresholdIndicator = thresholdIndicator1;
      ICollectorSettings icollectorSettings = settings;
      if (icollectorSettings == null)
        throw new ArgumentNullException(nameof (settings));
      this._settings = icollectorSettings;
      if (thresholdProcessors == null)
        throw new ArgumentNullException(nameof (thresholdProcessors));
      if (thresholdDataProviders == null)
        throw new ArgumentNullException(nameof (thresholdDataProviders));
      this._thresholdProcessors = thresholdProcessors.ToArray<IThresholdDataProcessor>();
      this._thresholdDataProviders = thresholdDataProviders.ToArray<ThresholdDataProvider>();
      if (!((IEnumerable<IThresholdDataProcessor>) this._thresholdProcessors).Any<IThresholdDataProcessor>())
        throw new InvalidOperationException("At least one threshold processor must be defined.");
      if (!((IEnumerable<ThresholdDataProvider>) this._thresholdDataProviders).Any<ThresholdDataProvider>())
        throw new InvalidOperationException("At least one threshold data provider must be defined.");
      if (((IEnumerable<ThresholdDataProvider>) this._thresholdDataProviders).SelectMany<ThresholdDataProvider, string>((Func<ThresholdDataProvider, IEnumerable<string>>) (p => p.GetKnownThresholdNames())).GroupBy<string, string>((Func<string, string>) (n => n), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Any<IGrouping<string, string>>((Func<IGrouping<string, string>, bool>) (g => g.Count<string>() > 1)))
        throw new InvalidOperationException("Threshold data providers do not provide unique known thresholds names.");
      this._dataProcessorsDictionary = (IDictionary<string, IThresholdDataProcessor>) new Dictionary<string, IThresholdDataProcessor>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._dataProvidersDictionary = (IDictionary<string, ThresholdDataProvider>) new Dictionary<string, ThresholdDataProvider>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      ThresholdProcessingEngine._log.Debug((object) "Starting _thresholdDataProviders processing.");
      foreach (ThresholdDataProvider thresholdDataProvider in this._thresholdDataProviders)
      {
        ThresholdProcessingEngine._log.DebugFormat("Processing provider '{0}'.", (object) thresholdDataProvider);
        Type processorType = thresholdDataProvider.GetThresholdDataProcessor();
        if (processorType == (Type) null || !typeof (IThresholdDataProcessor).IsAssignableFrom(processorType))
          throw new InvalidOperationException(string.Format("Invalid threshold processor type '{0}'", (object) processorType));
        IThresholdDataProcessor ithresholdDataProcessor = ((IEnumerable<IThresholdDataProcessor>) this._thresholdProcessors).FirstOrDefault<IThresholdDataProcessor>((Func<IThresholdDataProcessor, bool>) (p => ((object) p).GetType() == processorType));
        ThresholdProcessingEngine._log.DebugFormat("Getting processor '{0}'.", (object) ithresholdDataProcessor);
        if (ithresholdDataProcessor == null)
          throw new InvalidOperationException(string.Format("Cannot find any threshold processor for type '{0}'", (object) processorType));
        foreach (string knownThresholdName in thresholdDataProvider.GetKnownThresholdNames())
        {
          this._dataProcessorsDictionary.Add(knownThresholdName, ithresholdDataProcessor);
          this._dataProvidersDictionary.Add(knownThresholdName, thresholdDataProvider);
        }
      }
      ThresholdProcessingEngine._log.Debug((object) "_thresholdDataProviders processing finished.");
    }

    internal int BatchSize
    {
      get
      {
        return this._batchSize;
      }
      set
      {
        if (value <= 0 || value > 100000)
          throw new ArgumentOutOfRangeException(nameof (value), "BatchSize has valid range between 1 and 100000.");
        this._batchSize = value;
      }
    }

    internal string BaselineTimeFrame
    {
      get
      {
        return this._baselineTimeFrame;
      }
      set
      {
        if (string.IsNullOrEmpty(value))
          throw new ArgumentNullException(nameof (value));
        this._baselineTimeFrame = value;
      }
    }

    internal int BaselineCollectionDuration
    {
      get
      {
        return SettingsDAL.GetCurrentInt("SWNetPerfMon-Settings-Baseline Collection Duration", 7);
      }
    }

    internal IEnumerable<ThresholdDataProvider> ThresholdDataProviders
    {
      get
      {
        return (IEnumerable<ThresholdDataProvider>) this._thresholdDataProviders;
      }
    }

    internal IEnumerable<IThresholdDataProcessor> ThresholdDataProcessors
    {
      get
      {
        return (IEnumerable<IThresholdDataProcessor>) this._thresholdProcessors;
      }
    }

    public BaselineValues GetBaselineValues(string thresholdName, int instanceId)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        ThresholdDataProvider thresholdDataProvider = this.GetThresholdDataProvider(thresholdName, true);
        StatisticalTableMetadata statisticalTableMetadata = thresholdDataProvider.GetStatisticalTableMetadata(thresholdName);
        string projectionFromMetadata = thresholdDataProvider.CreateProjectionFromMetadata(statisticalTableMetadata);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT t.Name as TimeFrameName,\r\n                             {0} \r\n                             {1}\r\n                        FROM {2} stats\r\n                        JOIN TimeFrames t ON t.TimeFrameID = stats.TimeFrameID\r\n                       WHERE stats.{0} = @instanceId\r\n                         AND t.Name = @timeFrameName", (object) statisticalTableMetadata.get_InstanceIdColumnName(), (object) projectionFromMetadata, (object) statisticalTableMetadata.get_TableName())))
        {
          textCommand.Parameters.AddWithValue(nameof (instanceId), (object) instanceId);
          textCommand.Parameters.AddWithValue("timeFrameName", (object) this.BaselineTimeFrame);
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
            return dataReader.Read() ? thresholdDataProvider.CreateBaselineValuesFromReader(dataReader) : (BaselineValues) null;
        }
      }
    }

    public List<BaselineValues> GetBaselineValuesForAllTimeFrames(
      string thresholdName,
      int instanceId)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      List<BaselineValues> baselineValuesList = new List<BaselineValues>();
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        ThresholdDataProvider thresholdDataProvider = this.GetThresholdDataProvider(thresholdName, true);
        StatisticalTableMetadata statisticalTableMetadata = thresholdDataProvider.GetStatisticalTableMetadata(thresholdName);
        string projectionFromMetadata = thresholdDataProvider.CreateProjectionFromMetadata(statisticalTableMetadata);
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT t.Name as TimeFrameName,\r\n                             {0} \r\n                             {1}\r\n                        FROM {2} stats\r\n                        JOIN TimeFrames t ON t.TimeFrameID = stats.TimeFrameID\r\n                       WHERE stats.{0} = @instanceId", (object) statisticalTableMetadata.get_InstanceIdColumnName(), (object) projectionFromMetadata, (object) statisticalTableMetadata.get_TableName())))
        {
          textCommand.Parameters.AddWithValue(nameof (instanceId), (object) instanceId);
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
          {
            while (dataReader.Read())
            {
              BaselineValues valuesFromReader = thresholdDataProvider.CreateBaselineValuesFromReader(dataReader);
              if (valuesFromReader != null)
                baselineValuesList.Add(valuesFromReader);
            }
          }
        }
        return baselineValuesList;
      }
    }

    public ThresholdComputationResult ComputeThresholds(
      string thresholdName,
      int instanceId,
      string warningFormula,
      string criticalFormula,
      BaselineValues baselineValues,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      if (baselineValues == null)
        throw new ArgumentNullException(nameof (baselineValues));
      IThresholdDataProcessor thresholdDataProcessor = this.GetThresholdDataProcessor(thresholdName, true);
      bool warningEnabled = !string.IsNullOrEmpty(warningFormula);
      bool criticalEnabled = !string.IsNullOrEmpty(criticalFormula);
      if (warningEnabled && !thresholdDataProcessor.IsFormulaValid(warningFormula, (ThresholdLevel) 1, thresholdOperator).get_IsValid())
        throw new InvalidOperationException(string.Format("Provided formula '{0}' is not valid.", (object) warningFormula));
      if (criticalEnabled && !thresholdDataProcessor.IsFormulaValid(criticalFormula, (ThresholdLevel) 2, thresholdOperator).get_IsValid())
        throw new InvalidOperationException(string.Format("Provided formula '{0}' is not valid.", (object) criticalFormula));
      try
      {
        int disabledValue = ThresholdsHelper.GetDisabledValue(thresholdOperator);
        double warningThreshold = warningEnabled ? thresholdDataProcessor.ComputeThreshold(warningFormula, baselineValues, (ThresholdLevel) 1, thresholdOperator) : (double) disabledValue;
        double criticalThreshold = criticalEnabled ? thresholdDataProcessor.ComputeThreshold(criticalFormula, baselineValues, (ThresholdLevel) 2, thresholdOperator) : (double) disabledValue;
        ThresholdMinMaxValue thresholdMinMaxValues = this.GetThresholdDataProvider(thresholdName, true).GetThresholdMinMaxValues(thresholdName, instanceId);
        return this.ProcessThresholds(warningEnabled, warningThreshold, criticalEnabled, criticalThreshold, thresholdOperator, thresholdMinMaxValues);
      }
      catch (DivideByZeroException ex)
      {
        ThresholdComputationResult computationResult = new ThresholdComputationResult();
        computationResult.set_IsSuccess(false);
        computationResult.set_Message(Resources.get_LIBCODE_PC0_02());
        return computationResult;
      }
    }

    public ValidationResult IsFormulaValid(
      string thresholdName,
      string formula,
      ThresholdLevel level,
      ThresholdOperatorEnum thresholdOperator)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      return this.GetThresholdDataProcessor(thresholdName, true).IsFormulaValid(formula, level, thresholdOperator);
    }

    public ThresholdMinMaxValue GetThresholdMinMaxValues(
      string thresholdName,
      int instanceId)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      return this.GetThresholdDataProvider(thresholdName, true).GetThresholdMinMaxValues(thresholdName, instanceId);
    }

    public int SetThreshold(Threshold threshold)
    {
      int thresholdPollsInterval = this._settings.get_MaxThresholdPollsInterval();
      if (threshold == null)
        throw new ArgumentNullException(nameof (threshold));
      if (string.IsNullOrEmpty(threshold.get_ThresholdName()))
        throw new InvalidOperationException("Threshold name have to be set.");
      if (!string.IsNullOrEmpty(threshold.get_WarningFormula()) && !this.IsFormulaValid(threshold.get_ThresholdName(), threshold.get_WarningFormula(), (ThresholdLevel) 1, threshold.get_ThresholdOperator()).get_IsValid())
        throw new InvalidOperationException(string.Format("Warning formula '{0}' is not valid.", (object) threshold.get_WarningFormula()));
      if (!string.IsNullOrEmpty(threshold.get_CriticalFormula()) && !this.IsFormulaValid(threshold.get_ThresholdName(), threshold.get_CriticalFormula(), (ThresholdLevel) 2, threshold.get_ThresholdOperator()).get_IsValid())
        throw new InvalidOperationException(string.Format("Critical formula '{0}' is not valid.", (object) threshold.get_CriticalFormula()));
      int? nullable1 = threshold.get_WarningPolls();
      if (!nullable1.HasValue)
      {
        nullable1 = threshold.get_WarningPollsInterval();
        if (!nullable1.HasValue)
          goto label_20;
      }
      nullable1 = threshold.get_WarningPolls();
      if (nullable1.HasValue)
      {
        nullable1 = threshold.get_WarningPollsInterval();
        if (nullable1.HasValue)
        {
          nullable1 = threshold.get_WarningPolls();
          if (nullable1.Value >= 1)
          {
            nullable1 = threshold.get_WarningPollsInterval();
            if (nullable1.Value >= 1)
            {
              int num1 = thresholdPollsInterval;
              nullable1 = threshold.get_WarningPollsInterval();
              int num2 = nullable1.Value;
              if (num1 < num2)
                throw new ArgumentException(string.Format("Number of total warning polls is greater than limit: {0}.", (object) thresholdPollsInterval));
              nullable1 = threshold.get_WarningPollsInterval();
              int num3 = nullable1.Value;
              nullable1 = threshold.get_WarningPolls();
              int num4 = nullable1.Value;
              if (num3 < num4)
                throw new ArgumentException("Number of expected warning polls is greater than number of total polls");
              goto label_20;
            }
          }
          throw new ArgumentException("Values in Warning fields must be at least 1");
        }
      }
      throw new ArgumentException("Both WarningPolls and WarningPollsInterval must be set");
label_20:
      nullable1 = threshold.get_CriticalPolls();
      if (!nullable1.HasValue)
      {
        nullable1 = threshold.get_CriticalPollsInterval();
        if (!nullable1.HasValue)
          goto label_32;
      }
      nullable1 = threshold.get_CriticalPolls();
      if (nullable1.HasValue)
      {
        nullable1 = threshold.get_CriticalPollsInterval();
        if (nullable1.HasValue)
        {
          nullable1 = threshold.get_CriticalPolls();
          if (nullable1.Value >= 1)
          {
            nullable1 = threshold.get_CriticalPollsInterval();
            if (nullable1.Value >= 1)
            {
              int num1 = thresholdPollsInterval;
              nullable1 = threshold.get_CriticalPollsInterval();
              int num2 = nullable1.Value;
              if (num1 < num2)
                throw new ArgumentException(string.Format("Number of total critical polls is greater than limit: {0}.", (object) thresholdPollsInterval));
              nullable1 = threshold.get_CriticalPollsInterval();
              int num3 = nullable1.Value;
              nullable1 = threshold.get_CriticalPolls();
              int num4 = nullable1.Value;
              if (num3 < num4)
                throw new ArgumentException("Number of expected critical polls is greater than number of total polls");
              goto label_32;
            }
          }
          throw new ArgumentException("Values in Critical fields must be at least 1");
        }
      }
      throw new ArgumentException("Both CriticalPolls and CriticalPollsInterval must be set");
label_32:
      this._thresholdIndicator.LoadPreviousThresholdData(threshold.get_InstanceId(), threshold.get_ThresholdName());
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\n                    DECLARE @thresholdNameId int\r\n                    DECLARE @thresholdId int\r\n\r\n                    SELECT @thresholdNameId = Id FROM dbo.ThresholdsNames WHERE Name = @thresholdName\r\n                    SELECT @thresholdId = Id FROM dbo.Thresholds WHERE InstanceId = @instanceId AND ThresholdNameId = @thresholdNameId\r\n\r\n                    IF @thresholdId IS NULL\r\n                    BEGIN\r\n\t                    INSERT INTO dbo.Thresholds (InstanceId,ThresholdType,ThresholdNameId,ThresholdOperator,Warning,Critical\r\n\t\t\t                       ,WarningFormula,CriticalFormula,BaselineFrom,BaselineTo,BaselineApplied,BaselineApplyError\r\n                                    ,WarningPolls,WarningPollsInterval,CriticalPolls,CriticalPollsInterval,WarningEnabled,CriticalEnabled)\r\n                         VALUES (@instanceId,@thresholdType,@thresholdNameId,@thresholdOperator,@warning,@critical\r\n\t                            ,@warningFormula,@criticalFormula,@baselineFrom,@baselineTo,@baselineApplied,@baselineApplyError\r\n                                 ,@warningpolls,@warningpollsinterval,@criticalpolls,@criticalpollsinterval,@warningenabled,@criticalenabled)\r\n\t                    SET @thresholdId = SCOPE_IDENTITY()\r\n                    END\r\n                    ELSE\r\n                    BEGIN\r\n\t                    UPDATE dbo.Thresholds\r\n\t                       SET ThresholdType = @thresholdType\r\n\t\t                      ,ThresholdOperator = @thresholdOperator\r\n\t\t                      ,Warning = @warning\r\n\t\t                      ,Critical = @critical\r\n\t\t                      ,WarningFormula = @warningFormula\r\n\t\t                      ,CriticalFormula = @criticalFormula\r\n\t\t                      ,BaselineFrom = @baselineFrom\r\n\t\t                      ,BaselineTo = @baselineTo\r\n\t\t                      ,BaselineApplied = @baselineApplied\r\n\t\t                      ,BaselineApplyError  = @baselineApplyError\r\n                              ,WarningPolls = @warningpolls\r\n                              ,WarningPollsInterval = @warningpollsinterval\r\n                              ,CriticalPolls = @criticalpolls\r\n                              ,CriticalPollsInterval = @criticalpollsinterval\r\n                              ,WarningEnabled = @warningenabled\r\n                              ,CriticalEnabled = @criticalenabled\r\n\t                     WHERE Id = @thresholdId\r\n                    END\r\n                    SELECT @thresholdId AS ThresholdId"))
        {
          textCommand.Parameters.AddWithValue("instanceId", (object) threshold.get_InstanceId());
          textCommand.Parameters.AddWithValue("thresholdName", (object) threshold.get_ThresholdName());
          textCommand.Parameters.AddWithValue("thresholdType", (object) threshold.get_ThresholdType());
          textCommand.Parameters.AddWithValue("thresholdOperator", (object) threshold.get_ThresholdOperator());
          textCommand.Parameters.AddWithValue("baselineApplyError", (object) threshold.get_BaselineApplyError() ?? (object) DBNull.Value);
          SqlParameterCollection parameters1 = textCommand.Parameters;
          nullable1 = threshold.get_WarningPolls();
          // ISSUE: variable of a boxed type
          __Boxed<int> local1 = (System.ValueType) (nullable1 ?? 1);
          parameters1.AddWithValue("warningpolls", (object) local1);
          SqlParameterCollection parameters2 = textCommand.Parameters;
          nullable1 = threshold.get_WarningPollsInterval();
          // ISSUE: variable of a boxed type
          __Boxed<int> local2 = (System.ValueType) (nullable1 ?? 1);
          parameters2.AddWithValue("warningpollsinterval", (object) local2);
          SqlParameterCollection parameters3 = textCommand.Parameters;
          nullable1 = threshold.get_CriticalPolls();
          // ISSUE: variable of a boxed type
          __Boxed<int> local3 = (System.ValueType) (nullable1 ?? 1);
          parameters3.AddWithValue("criticalpolls", (object) local3);
          SqlParameterCollection parameters4 = textCommand.Parameters;
          nullable1 = threshold.get_CriticalPollsInterval();
          // ISSUE: variable of a boxed type
          __Boxed<int> local4 = (System.ValueType) (nullable1 ?? 1);
          parameters4.AddWithValue("criticalpollsinterval", (object) local4);
          textCommand.Parameters.AddWithValue("warningenabled", (object) threshold.get_WarningEnabled());
          textCommand.Parameters.AddWithValue("criticalenabled", (object) threshold.get_CriticalEnabled());
          SqlParameterCollection parameters5 = textCommand.Parameters;
          double? nullable2 = threshold.get_Warning();
          object obj1;
          if (!nullable2.HasValue)
          {
            obj1 = (object) DBNull.Value;
          }
          else
          {
            nullable2 = threshold.get_Warning();
            obj1 = (object) nullable2.Value;
          }
          parameters5.AddWithValue("warning", obj1);
          SqlParameterCollection parameters6 = textCommand.Parameters;
          nullable2 = threshold.get_Critical();
          object obj2;
          if (!nullable2.HasValue)
          {
            obj2 = (object) DBNull.Value;
          }
          else
          {
            nullable2 = threshold.get_Critical();
            obj2 = (object) nullable2.Value;
          }
          parameters6.AddWithValue("critical", obj2);
          textCommand.Parameters.AddWithValue("warningFormula", string.IsNullOrEmpty(threshold.get_WarningFormula()) ? (object) DBNull.Value : (object) threshold.get_WarningFormula());
          textCommand.Parameters.AddWithValue("criticalFormula", string.IsNullOrEmpty(threshold.get_CriticalFormula()) ? (object) DBNull.Value : (object) threshold.get_CriticalFormula());
          DateTime? nullable3 = threshold.get_BaselineApplied();
          if (nullable3.HasValue)
          {
            SqlParameterCollection parameters7 = textCommand.Parameters;
            nullable3 = threshold.get_BaselineApplied();
            // ISSUE: variable of a boxed type
            __Boxed<DateTime> local5 = (System.ValueType) nullable3.Value;
            parameters7.AddWithValue("baselineApplied", (object) local5);
            SqlParameterCollection parameters8 = textCommand.Parameters;
            nullable3 = threshold.get_BaselineFrom();
            // ISSUE: variable of a boxed type
            __Boxed<DateTime> local6 = (System.ValueType) nullable3.Value;
            parameters8.AddWithValue("baselineFrom", (object) local6);
            SqlParameterCollection parameters9 = textCommand.Parameters;
            nullable3 = threshold.get_BaselineTo();
            // ISSUE: variable of a boxed type
            __Boxed<DateTime> local7 = (System.ValueType) nullable3.Value;
            parameters9.AddWithValue("baselineTo", (object) local7);
          }
          else
          {
            textCommand.Parameters.AddWithValue("baselineFrom", (object) DBNull.Value);
            textCommand.Parameters.AddWithValue("baselineTo", (object) DBNull.Value);
            textCommand.Parameters.AddWithValue("baselineApplied", (object) DBNull.Value);
          }
          threshold.set_Id((int) SqlHelper.ExecuteScalar(textCommand, connection));
        }
      }
      this._thresholdIndicator.ReportThresholdNotification(threshold);
      return threshold.get_Id();
    }

    public void UpdateThresholds()
    {
      ThresholdProcessingEngine._log.Debug((object) "UpdateThresholds method enter.");
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        Dictionary<int, string> forRecalculation = ThresholdProcessingEngine.GetThresholdsNamesForRecalculation(connection);
        Stopwatch stopwatch = new Stopwatch();
        if (forRecalculation.Count > 0)
        {
          int defaultTimeFrameId = this.GetDefaultTimeFrameId(connection, this.BaselineTimeFrame);
          DateTime utcNow = DateTime.UtcNow;
          DateTime minBaselineDate = utcNow.AddDays((double) -this.BaselineCollectionDuration);
          int num1 = 0;
          stopwatch.Start();
          ThresholdProcessingEngine._log.InfoFormat("Update thresholds started with parameters: applyDate - '{0}', minBaselineDate - '{1}'.", (object) utcNow, (object) minBaselineDate);
          foreach (KeyValuePair<int, string> thresholdNameKvp in forRecalculation)
          {
            ThresholdProcessingEngine._log.DebugFormat("Processing of threshold name '{0}' started.", (object) thresholdNameKvp.Value);
            ThresholdDataProvider thresholdDataProvider = this.GetThresholdDataProvider(thresholdNameKvp.Value, false);
            if (thresholdDataProvider == null)
            {
              ThresholdProcessingEngine._log.ErrorFormat("Threshold data provider for threshold name '{0}' not found.", (object) thresholdNameKvp.Value);
            }
            else
            {
              IThresholdDataProcessor thresholdDataProcessor = this.GetThresholdDataProcessor(thresholdNameKvp.Value, false);
              if (thresholdDataProcessor == null)
              {
                ThresholdProcessingEngine._log.ErrorFormat("Threshold data processor for threshold name '{0}' not found.", (object) thresholdNameKvp.Value);
              }
              else
              {
                while (true)
                {
                  ThresholdProcessingEngine._log.Debug((object) "Threshold calculations started.");
                  IList<BaselineProcessingInfo> baselineProcessingInfo = this.GetBaselineProcessingInfo(connection, thresholdDataProvider, thresholdNameKvp, defaultTimeFrameId, utcNow, minBaselineDate, this.BatchSize);
                  if (baselineProcessingInfo.Count != 0)
                  {
                    foreach (BaselineProcessingInfo processingInfo in (IEnumerable<BaselineProcessingInfo>) baselineProcessingInfo)
                      this.ComputeThresholds(thresholdDataProcessor, thresholdDataProvider, processingInfo, utcNow);
                    ThresholdProcessingEngine._log.Debug((object) "Threshold calculations finished.");
                    int num2 = this.UpdateThresholds(connection, (ICollection<BaselineProcessingInfo>) baselineProcessingInfo, utcNow);
                    num1 += num2;
                  }
                  else
                    break;
                }
                this.UpdateRecalculationNeededFlag(connection, thresholdNameKvp.Key);
                ThresholdProcessingEngine._log.DebugFormat("Processing of threshold name '{0}' finished.", (object) thresholdNameKvp.Value);
              }
            }
          }
          stopwatch.Stop();
          ThresholdProcessingEngine._log.InfoFormat("Update thresholds processed {0} rows and finished in {1} ms.", (object) num1, (object) stopwatch.ElapsedMilliseconds);
        }
      }
      ThresholdProcessingEngine._log.Debug((object) "UpdateThresholds method exit.");
    }

    public StatisticalDataHistogram[] GetHistogramForStatisticalData(
      string thresholdName,
      int instanceId)
    {
      if (thresholdName == null)
        throw new ArgumentNullException(nameof (thresholdName));
      ThresholdDataProvider thresholdDataProvider = this.GetThresholdDataProvider(thresholdName, true);
      DateTime startDay;
      DateTime endDay;
      this.GetHistogramDateBorders(thresholdDataProvider.GetStatisticalTableMetadata(thresholdName), instanceId, out startDay, out endDay);
      return new HistogramCalculator().CreateHistogramWithScaledInterval(thresholdDataProvider.GetStatisticalData(thresholdName, instanceId, startDay, endDay), TimeFramesDAL.GetCoreTimeFrames((string) null).ToArray(), BusinessLayerSettings.Instance.ThresholdsHistogramChartIntervalsCount, thresholdDataProvider.GetThresholdMinMaxValues(thresholdName, instanceId).get_DataType());
    }

    public string GetThresholdInstanceName(string thresholdName, int instanceId)
    {
      if (string.IsNullOrEmpty(thresholdName))
        throw new ArgumentNullException(nameof (thresholdName));
      return this.GetThresholdDataProvider(thresholdName, true).GetThresholdInstanceName(thresholdName, instanceId);
    }

    public string GetStatisticalDataChartName(string thresholdName)
    {
      if (thresholdName == null)
        throw new ArgumentNullException(nameof (thresholdName));
      return this.GetThresholdDataProvider(thresholdName, true).GetStatisticalDataChartName(thresholdName);
    }

    private void GetHistogramDateBorders(
      StatisticalTableMetadata metadata,
      int instanceId,
      out DateTime startDay,
      out DateTime endDay)
    {
      startDay = DateTime.UtcNow;
      endDay = DateTime.UtcNow.AddDays((double) (-1 * this.BaselineCollectionDuration));
      string str = string.Format("SELECT {0},{1} \r\n                                         FROM {2} stats \r\n                                         LEFT JOIN TimeFrames t ON t.TimeFrameID = stats.TimeFrameID \r\n                                         WHERE stats.{3} = @instanceId\r\n                                         AND t.Name = @timeFrameName", (object) metadata.get_MinDateTime(), (object) metadata.get_MaxDateTime(), (object) metadata.get_TableName(), (object) metadata.get_InstanceIdColumnName());
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
        {
          textCommand.Parameters.AddWithValue("@instanceId", (object) instanceId);
          textCommand.Parameters.AddWithValue("@timeFrameName", (object) this.BaselineTimeFrame);
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
          {
            if (!dataReader.Read())
              return;
            startDay = DatabaseFunctions.GetDateTime(dataReader, metadata.get_MinDateTime(), DateTimeKind.Utc);
            endDay = DatabaseFunctions.GetDateTime(dataReader, metadata.get_MaxDateTime(), DateTimeKind.Utc);
          }
        }
      }
    }

    private void ComputeThresholds(
      IThresholdDataProcessor thresholdDataProcessor,
      ThresholdDataProvider provider,
      BaselineProcessingInfo processingInfo,
      DateTime applyDate)
    {
      Threshold threshold = processingInfo.Threshold;
      if (thresholdDataProcessor.IsBaselineValuesValid(processingInfo.BaselineValues))
      {
        try
        {
          int disabledValue = ThresholdsHelper.GetDisabledValue(threshold.get_ThresholdOperator());
          double warningThreshold = threshold.get_WarningEnabled() ? thresholdDataProcessor.ComputeThreshold(threshold.get_WarningFormula(), processingInfo.BaselineValues, (ThresholdLevel) 1, threshold.get_ThresholdOperator()) : (double) disabledValue;
          double criticalThreshold = threshold.get_CriticalEnabled() ? thresholdDataProcessor.ComputeThreshold(threshold.get_CriticalFormula(), processingInfo.BaselineValues, (ThresholdLevel) 2, threshold.get_ThresholdOperator()) : (double) disabledValue;
          ThresholdMinMaxValue thresholdMinMaxValues = provider.GetThresholdMinMaxValues(threshold.get_ThresholdName(), threshold.get_InstanceId());
          ThresholdComputationResult computationResult = this.ProcessThresholds(threshold.get_WarningEnabled(), warningThreshold, threshold.get_CriticalEnabled(), criticalThreshold, threshold.get_ThresholdOperator(), thresholdMinMaxValues);
          if (computationResult.get_IsSuccess())
          {
            threshold.set_Warning(new double?(computationResult.get_Warning()));
            threshold.set_Critical(new double?(computationResult.get_Critical()));
            threshold.set_BaselineApplyError((string) null);
          }
          else
          {
            threshold.set_Warning(new double?());
            threshold.set_Critical(new double?());
            threshold.set_BaselineApplyError(computationResult.get_Message());
          }
        }
        catch (Exception ex)
        {
          ThresholdProcessingEngine._log.ErrorFormat("Cannot compute thresholds for baseline values [{0}] and threshold [{1}]. Exception: {2}", (object) processingInfo.BaselineValues, (object) threshold, (object) ex);
        }
      }
      else
      {
        ThresholdProcessingEngine._log.VerboseFormat("Baseline values [{0}] are not valid for threshold [{1}]", new object[2]
        {
          (object) processingInfo.BaselineValues,
          (object) threshold
        });
        DateTime? nullable = processingInfo.BaselineValues.get_MinDateTime();
        if (!nullable.HasValue)
          processingInfo.BaselineValues.set_MinDateTime(new DateTime?(applyDate));
        nullable = processingInfo.BaselineValues.get_MaxDateTime();
        if (!nullable.HasValue)
          processingInfo.BaselineValues.set_MaxDateTime(new DateTime?(applyDate));
        threshold.set_BaselineApplyError(Resources.get_LIBCODE_PF0_6());
      }
    }

    public ThresholdComputationResult ProcessThresholds(
      double warningThreshold,
      double criticalThreshold,
      ThresholdOperatorEnum oper,
      ThresholdMinMaxValue minMaxValues)
    {
      return this.ProcessThresholds(true, warningThreshold, true, criticalThreshold, oper, minMaxValues);
    }

    public ThresholdComputationResult ProcessThresholds(
      bool warningEnabled,
      double warningThreshold,
      bool criticalEnabled,
      double criticalThreshold,
      ThresholdOperatorEnum oper,
      ThresholdMinMaxValue minMaxValues)
    {
      double warningRounded;
      double criticalRounded;
      ThresholdProcessingEngine.RoundThresholdsValues(minMaxValues.get_Min(), minMaxValues.get_Max(), warningThreshold, criticalThreshold, out warningRounded, out criticalRounded);
      int disabledValue = ThresholdsHelper.GetDisabledValue(oper);
      ThresholdComputationResult computationResult1 = new ThresholdComputationResult();
      computationResult1.set_Warning(warningEnabled ? warningRounded : (double) disabledValue);
      computationResult1.set_Critical(criticalEnabled ? criticalRounded : (double) disabledValue);
      computationResult1.set_IsSuccess(false);
      computationResult1.set_IsValid(false);
      computationResult1.set_Message((string) null);
      ThresholdComputationResult computationResult2 = computationResult1;
      bool flag = warningEnabled & criticalEnabled;
      if (flag && warningThreshold == criticalThreshold)
        computationResult2.set_Message(Resources.get_LIBCODE_PF0_1());
      else if (warningEnabled && warningThreshold < minMaxValues.get_Min() || criticalEnabled && criticalThreshold < minMaxValues.get_Min())
      {
        computationResult2.set_IsValid(true);
        computationResult2.set_Message(Resources.get_LIBCODE_PF0_2());
      }
      else if (warningEnabled && warningThreshold > minMaxValues.get_Max() || criticalEnabled && criticalThreshold > minMaxValues.get_Max())
      {
        computationResult2.set_IsValid(true);
        computationResult2.set_Message(Resources.get_LIBCODE_PF0_3());
      }
      else if (flag && warningThreshold > criticalThreshold && (oper.Equals((object) (ThresholdOperatorEnum) 0) || oper.Equals((object) (ThresholdOperatorEnum) 1)))
        computationResult2.set_Message(Resources.get_LIBCODE_PF0_4());
      else if (flag && warningThreshold < criticalThreshold && (oper.Equals((object) (ThresholdOperatorEnum) 4) || oper.Equals((object) (ThresholdOperatorEnum) 3)))
      {
        computationResult2.set_Message(Resources.get_LIBCODE_PF0_5());
      }
      else
      {
        computationResult2.set_IsSuccess(true);
        computationResult2.set_IsValid(true);
      }
      return computationResult2;
    }

    private static void RoundThresholdsValues(
      double min,
      double max,
      double warning,
      double critical,
      out double warningRounded,
      out double criticalRounded)
    {
      ThresholdsHelper.RoundThresholdsValues(min, max, warning, critical, ref warningRounded, ref criticalRounded);
    }

    private static Dictionary<int, string> GetThresholdsNamesForRecalculation(
      SqlConnection connection)
    {
      Dictionary<int, string> dictionary = new Dictionary<int, string>(100);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT Id, \r\n                         Name\r\n                    FROM dbo.ThresholdsNames\r\n                   WHERE RecalculationNeeded = 1"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
        {
          while (dataReader.Read())
            dictionary.Add((int) dataReader["Id"], dataReader["Name"].ToString());
        }
      }
      return dictionary;
    }

    private int GetDefaultTimeFrameId(SqlConnection connection, string timeFrameName)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT TimeFrameID\r\n                    FROM dbo.TimeFrames\r\n                   WHERE Name = @timeFrameName\r\n                     AND IsDisabled = 0"))
      {
        textCommand.Parameters.AddWithValue(nameof (timeFrameName), (object) timeFrameName);
        int? nullable = SqlHelper.ExecuteScalar(textCommand, connection) as int?;
        if (!nullable.HasValue)
          throw new InvalidOperationException(string.Format("Cannot find time frame with name '{0}'", (object) timeFrameName));
        return nullable.Value;
      }
    }

    private IList<BaselineProcessingInfo> GetBaselineProcessingInfo(
      SqlConnection connection,
      ThresholdDataProvider provider,
      KeyValuePair<int, string> thresholdNameKvp,
      int timeFrameId,
      DateTime currentApplyDate,
      DateTime minBaselineDate,
      int batchSize)
    {
      StatisticalTableMetadata statisticalTableMetadata = provider.GetStatisticalTableMetadata(thresholdNameKvp.Value);
      string baselineInfoQuey = ThresholdProcessingEngine.CreateBaselineInfoQuey(provider, statisticalTableMetadata, batchSize);
      List<BaselineProcessingInfo> baselineProcessingInfoList = new List<BaselineProcessingInfo>(batchSize);
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      ThresholdProcessingEngine._log.DebugFormat("GetBaselineProcessingInfo started for '{0}' with batch size {1}.", (object) thresholdNameKvp.Value, (object) batchSize);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(baselineInfoQuey))
      {
        textCommand.Parameters.AddWithValue(nameof (currentApplyDate), (object) currentApplyDate);
        textCommand.Parameters.AddWithValue("thresholdNameId", (object) thresholdNameKvp.Key);
        textCommand.Parameters.AddWithValue(nameof (timeFrameId), (object) timeFrameId);
        if (!string.IsNullOrEmpty(statisticalTableMetadata.get_MinDateTime()))
          textCommand.Parameters.AddWithValue("minDateTime", (object) minBaselineDate);
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand, connection))
        {
          while (dataReader.Read())
          {
            Threshold threshold1 = new Threshold();
            threshold1.set_Id((int) dataReader["Id"]);
            threshold1.set_InstanceId((int) dataReader["InstanceId"]);
            threshold1.set_ThresholdOperator((ThresholdOperatorEnum) dataReader["ThresholdOperator"]);
            threshold1.set_WarningFormula(dataReader["WarningFormula"].ToString());
            threshold1.set_CriticalFormula(dataReader["CriticalFormula"].ToString());
            threshold1.set_ThresholdName(thresholdNameKvp.Value);
            threshold1.set_WarningPolls(new int?((int) dataReader["WarningPolls"]));
            threshold1.set_WarningPollsInterval(new int?((int) dataReader["WarningPollsInterval"]));
            threshold1.set_CriticalPolls(new int?((int) dataReader["CriticalPolls"]));
            threshold1.set_CriticalPollsInterval(new int?((int) dataReader["CriticalPollsInterval"]));
            threshold1.set_WarningEnabled((bool) dataReader["WarningEnabled"]);
            threshold1.set_CriticalEnabled((bool) dataReader["CriticalEnabled"]);
            Threshold threshold2 = threshold1;
            BaselineValues valuesFromReader = provider.CreateBaselineValuesFromReader(dataReader);
            baselineProcessingInfoList.Add(new BaselineProcessingInfo(threshold2, valuesFromReader));
          }
        }
      }
      stopwatch.Stop();
      ThresholdProcessingEngine._log.DebugFormat("GetBaselineProcessingInfo finished in {0} ms. Number of selected rows {1}", (object) stopwatch.ElapsedMilliseconds, (object) baselineProcessingInfoList.Count);
      return (IList<BaselineProcessingInfo>) baselineProcessingInfoList;
    }

    private static string CreateBaselineInfoQuey(
      ThresholdDataProvider provider,
      StatisticalTableMetadata metadata,
      int batchSize)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("SELECT TOP {0}\r\n                         t.Id\r\n                        ,t.InstanceId\r\n                        ,t.ThresholdOperator\r\n                        ,t.WarningFormula\r\n                        ,t.CriticalFormula\r\n                        ,t.WarningPolls\r\n                        ,t.WarningPollsInterval\r\n                        ,t.CriticalPolls\r\n                        ,t.CriticalPollsInterval\r\n                        ,t.WarningEnabled\r\n                        ,t.CriticalEnabled", (object) batchSize);
      stringBuilder.AppendLine();
      stringBuilder.AppendLine(provider.CreateProjectionFromMetadata(metadata));
      stringBuilder.AppendFormat("FROM dbo.Thresholds t\r\n                  JOIN {0}\r\n                  WHERE ((t.ThresholdType = 1 AND t.Warning IS NULL)\r\n                          OR ((t.ThresholdType = 2) AND (t.BaselineApplied IS NULL OR t.BaselineApplied < @currentApplyDate)))\r\n                  AND t.ThresholdNameId = @thresholdNameId\r\n                  AND stats.TimeFrameID = @timeFrameId\r\n                  {1}", (object) string.Format("{0} stats ON stats.{1} = t.InstanceId", (object) metadata.get_TableName(), (object) metadata.get_InstanceIdColumnName()), !string.IsNullOrEmpty(metadata.get_MinDateTime()) ? (object) string.Format("AND stats.{0} <= @minDateTime", (object) metadata.get_MinDateTime()) : (object) string.Empty);
      return stringBuilder.ToString();
    }

    private int UpdateThresholds(
      SqlConnection connection,
      ICollection<BaselineProcessingInfo> thresholdsToUpdate,
      DateTime applyDate)
    {
      if (!thresholdsToUpdate.Any<BaselineProcessingInfo>())
        return 0;
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      ThresholdProcessingEngine._log.DebugFormat("Bulk update thresholds operation started with {0} thresholds to update.", (object) thresholdsToUpdate.Count);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("IF OBJECT_ID('tempdb..#ThresholdsForUpdate') IS NULL\r\n                                        BEGIN\r\n\t                                        CREATE TABLE #ThresholdsForUpdate\r\n\t                                        (\r\n\t\t                                        ThresholdId INT,\r\n\t\t                                        Warning FLOAT,\r\n\t\t                                        Critical FLOAT,\r\n\t\t                                        MinDateTime DATETIME,\r\n\t\t                                        MaxDateTime DATETIME,\r\n\t\t                                        BaselineApplyError NVARCHAR(MAX),\r\n                                                WarningPolls INT,\r\n                                                WarningPollsInterval INT,\r\n                                                CriticalPolls INT,\r\n                                                CriticalPollsInterval INT,\r\n                                                WarningEnabled BIT,\r\n                                                CriticalEnabled BIT\r\n\t                                        )    \r\n                                        END\r\n                                        ELSE\r\n                                        BEGIN\r\n                                            TRUNCATE TABLE #ThresholdsForUpdate\r\n                                        END"))
        SqlHelper.ExecuteNonQuery(textCommand, connection);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      using (IDataReader dataReader = (IDataReader) new EnumerableDataReader<BaselineProcessingInfo>((PropertyAccessorBase<BaselineProcessingInfo>) new SinglePropertyAccessor<BaselineProcessingInfo>().AddColumn("ThresholdId", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_0 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_0 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_0)))).AddColumn("Warning", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_1 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_1 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_1)))).AddColumn("Critical", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_2 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_2 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_2)))).AddColumn("MinDateTime", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_3 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_3 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_3)))).AddColumn("MaxDateTime", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_4 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_4 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_4)))).AddColumn("BaselineApplyError", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_5 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_5 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_5)))).AddColumn("WarningPolls", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_6 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_6 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_6)))).AddColumn("WarningPollsInterval", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_7 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_7 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_7)))).AddColumn("CriticalPolls", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_8 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_8 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_8)))).AddColumn("CriticalPollsInterval", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_9 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_9 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_9)))).AddColumn("WarningEnabled", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_10 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_10 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_10)))).AddColumn("CriticalEnabled", ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_11 ?? (ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9__41_11 = new SinglePropertyAccessor<BaselineProcessingInfo>.SinglePropertyAcccessorConvert((object) ThresholdProcessingEngine.\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003CUpdateThresholds\u003Eb__41_11)))), (IEnumerable<BaselineProcessingInfo>) thresholdsToUpdate))
        SqlHelper.ExecuteBulkCopy("#ThresholdsForUpdate", dataReader, connection, (SqlTransaction) null, SqlBulkCopyOptions.TableLock);
      ThresholdProcessingEngine._log.DebugFormat("Bulk insert finished in {0} ms.", (object) stopwatch.ElapsedMilliseconds);
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE Thresholds\r\n                SET Thresholds.Warning = #ThresholdsForUpdate.Warning,\r\n                    Thresholds.Critical = #ThresholdsForUpdate.Critical,\r\n                    Thresholds.BaselineFrom = #ThresholdsForUpdate.MinDateTime,\r\n                    Thresholds.BaselineTo = #ThresholdsForUpdate.MaxDateTime,\r\n                    Thresholds.BaselineApplied = @applyDate,\r\n                    Thresholds.BaselineApplyError = #ThresholdsForUpdate.BaselineApplyError,\r\n                    Thresholds.WarningPolls = #ThresholdsForUpdate.WarningPolls,\r\n                    Thresholds.WarningPollsInterval = #ThresholdsForUpdate.WarningPollsInterval,\r\n                    Thresholds.CriticalPolls = #ThresholdsForUpdate.CriticalPolls,\r\n                    Thresholds.CriticalPollsInterval = #ThresholdsForUpdate.CriticalPollsInterval,\r\n                    Thresholds.WarningEnabled = #ThresholdsForUpdate.WarningEnabled,\r\n                    Thresholds.CriticalEnabled = #ThresholdsForUpdate.CriticalEnabled\r\n                FROM Thresholds\r\n                JOIN #ThresholdsForUpdate ON Thresholds.Id = #ThresholdsForUpdate.ThresholdId"))
      {
        textCommand.Parameters.AddWithValue(nameof (applyDate), (object) applyDate);
        int num = SqlHelper.ExecuteNonQuery(textCommand, connection);
        stopwatch.Stop();
        ThresholdProcessingEngine._log.DebugFormat("Bulk update finished in {0} ms. Affected rows {1}.", (object) stopwatch.ElapsedMilliseconds, (object) num);
        return num;
      }
    }

    private void UpdateRecalculationNeededFlag(SqlConnection connection, int thresholdNameId)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE dbo.ThresholdsNames \r\n                     SET RecalculationNeeded = 0\r\n                   WHERE Id = @id"))
      {
        textCommand.Parameters.AddWithValue("@id", (object) thresholdNameId);
        int num = SqlHelper.ExecuteNonQuery(textCommand, connection);
        ThresholdProcessingEngine._log.DebugFormat("RecalculationNeeded flag updated for thresholdNameId {0}. Affected rows {1}.", (object) thresholdNameId, (object) num);
      }
    }

    private IThresholdDataProcessor GetThresholdDataProcessor(
      string thresholdName,
      bool throwsException)
    {
      IThresholdDataProcessor ithresholdDataProcessor;
      if (this._dataProcessorsDictionary.TryGetValue(thresholdName, out ithresholdDataProcessor))
        return ithresholdDataProcessor;
      string message = string.Format("Threshold processor for '{0}' was not found.", (object) thresholdName);
      ThresholdProcessingEngine._log.Error((object) message);
      if (throwsException)
        throw new InvalidOperationException(message);
      return (IThresholdDataProcessor) null;
    }

    private ThresholdDataProvider GetThresholdDataProvider(
      string thresholdName,
      bool throwsException)
    {
      ThresholdDataProvider thresholdDataProvider;
      if (this._dataProvidersDictionary.TryGetValue(thresholdName, out thresholdDataProvider))
        return thresholdDataProvider;
      string message = string.Format("Threshold data provider for '{0}' was not found.", (object) thresholdName);
      ThresholdProcessingEngine._log.Error((object) message);
      if (throwsException)
        throw new InvalidOperationException(message);
      return (ThresholdDataProvider) null;
    }
  }
}
