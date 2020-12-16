// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Instrumentation.PerformanceResourceIndex
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Instrumentation;
using SolarWinds.Orion.Core.Common.Instrumentation.Keys;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer.Instrumentation
{
  internal class PerformanceResourceIndex : IPerformanceResourceIndex
  {
    private static readonly Log log = new Log();
    private readonly Lazy<ConcurrentDictionary<PerformanceResourceKey, int>> cache = new Lazy<ConcurrentDictionary<PerformanceResourceKey, int>>(new Func<ConcurrentDictionary<PerformanceResourceKey, int>>(PerformanceResourceIndex.InitFromDatabase), LazyThreadSafetyMode.ExecutionAndPublication);

    public int GetResourceId(StringKeyBase resourceId)
    {
      if (resourceId == null)
        throw new ArgumentNullException(nameof (resourceId));
      PerformanceResourceIndex.log.Debug((object) string.Format("Identifying {0} - {1}", (object) ((KeyBase) resourceId).get_ResourceType(), (object) resourceId.get_Id()));
      return this.cache.Value.GetOrAdd(new PerformanceResourceKey(resourceId), new Func<PerformanceResourceKey, int>(this.AddToDatabase));
    }

    private static ConcurrentDictionary<PerformanceResourceKey, int> InitFromDatabase()
    {
      PerformanceResourceIndex.log.Debug((object) "Loading PerformanceResourceIndex from database");
      try
      {
        ConcurrentDictionary<PerformanceResourceKey, int> concurrentDictionary = new ConcurrentDictionary<PerformanceResourceKey, int>();
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT [ResourceTypeId], [ResourceName], [ResourceId] FROM [dbo].[WebsitePerformanceResource]"))
        {
          using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
          {
            while (dataReader.Read())
            {
              PerformanceResourceKey index = new PerformanceResourceKey((WebResourceType) dataReader.GetInt32(0), dataReader.GetString(1));
              concurrentDictionary[index] = dataReader.GetInt32(2);
            }
          }
        }
        PerformanceResourceIndex.log.Debug((object) string.Format("Loaded {0} items into {1} from database", (object) concurrentDictionary.Count, (object) nameof (PerformanceResourceIndex)));
        return concurrentDictionary;
      }
      catch (Exception ex)
      {
        PerformanceResourceIndex.log.Error((object) "Exception occurred when loading PerformanceResourceIndex from database", ex);
        throw;
      }
    }

    private int AddToDatabase(PerformanceResourceKey resourceId)
    {
      PerformanceResourceIndex.log.Debug((object) string.Format("Inserting performance resource key {0} into database", (object) resourceId));
      try
      {
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT INTO [dbo].[WebsitePerformanceResource] \r\n         ([ResourceTypeId],[ResourceName]) \r\n         VALUES(@resourceTypeId, @resourceName)\r\n\r\nSELECT @@IDENTITY"))
        {
          textCommand.Parameters.AddWithValue("resourceTypeId", (object) resourceId.ResourceType);
          textCommand.Parameters.AddWithValue("resourceName", (object) resourceId.ResourceName);
          return Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
        }
      }
      catch (Exception ex)
      {
        PerformanceResourceIndex.log.Error((object) string.Format("Exception occurred when inserting performance resource key {0} to database", (object) resourceId), ex);
        throw;
      }
    }
  }
}
