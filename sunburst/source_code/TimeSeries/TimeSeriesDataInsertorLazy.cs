// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TimeSeries.TimeSeriesDataInsertorLazy
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Threading;
using SolarWinds.Database.TimeSeries;
using SolarWinds.Database.TimeSeries.Contracts;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.TimeSeries
{
  internal class TimeSeriesDataInsertorLazy : ITimeSeriesDataInsertor
  {
    private static readonly Log log = new Log();
    private readonly LazyWithoutExceptionCache<IDataInsertor> _dataInsertor = new LazyWithoutExceptionCache<IDataInsertor>(new Func<IDataInsertor>(TimeSeriesDataInsertorLazy.CreateDataInsertor));

    private static IDataInsertor CreateDataInsertor()
    {
      return new TimeSeriesDatabaseFactory(TimeSeriesSettings.get_Instance(), (IConnectionFactory) new TimeSeriesDataInsertorLazy.DatabaseConnectionFactory(), (ILoggerFactory) new TimeSeriesDataInsertorLazy.LoggerFactory()).InitializeDatabase().get_DataInsertor();
    }

    public void InsertData(
      string tableName,
      IReadOnlyList<string> columnNames,
      IEnumerable<IReadOnlyList<object>> data)
    {
      this._dataInsertor.get_Value().InsertData(tableName, columnNames, data);
    }

    private class LoggerFactory : ILoggerFactory
    {
      public ILogger<T> CreateLogger<T>()
      {
        return (ILogger<T>) new TimeSeriesDataInsertorLazy.LoggerFactory.Logger<T>();
      }

      private class Logger<T> : ILogger<T>, ILogger
      {
        public void LogDebug(string message)
        {
          TimeSeriesDataInsertorLazy.log.Debug((object) message);
        }

        public void LogInfo(string message)
        {
          TimeSeriesDataInsertorLazy.log.Info((object) message);
        }

        public void LogError(string message)
        {
          TimeSeriesDataInsertorLazy.log.Error((object) message);
        }
      }
    }

    private class DatabaseConnectionFactory : IConnectionFactory
    {
      public Task<IDbConnection> CreateAndOpenAsync(CancellationToken cancellation = default (CancellationToken))
      {
        return Task.FromResult<IDbConnection>((IDbConnection) DatabaseFunctions.CreateConnection());
      }

      public IDbConnection CreateAndOpen()
      {
        return (IDbConnection) DatabaseFunctions.CreateConnection();
      }

      public SqlConnection RetrieveSqlConnection(IDbConnection connection)
      {
        return (SqlConnection) connection;
      }
    }
  }
}
