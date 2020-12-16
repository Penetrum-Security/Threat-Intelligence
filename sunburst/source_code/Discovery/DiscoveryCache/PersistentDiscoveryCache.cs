// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache.PersistentDiscoveryCache
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Discovery;
using SolarWinds.Orion.Core.Models.DiscoveredObjects;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Models.Enums;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache
{
  internal class PersistentDiscoveryCache : IPersistentDiscoveryCache
  {
    private static readonly Log _log = new Log();

    public DiscoveryResultItem GetResultForNode(int nodeId)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT CachedTime, CacheBlob FROM NodeListResourcesCache WHERE NodeId=@nodeId"))
      {
        textCommand.Parameters.AddWithValue("@nodeId", (object) nodeId);
        using (IDataReader result = SqlHelper.ExecuteReader(textCommand))
        {
          if (result.Read())
            return this.DeserializeResults(nodeId, result);
          PersistentDiscoveryCache._log.DebugFormat("Cache for Node {0} not found", (object) nodeId);
          return (DiscoveryResultItem) null;
        }
      }
    }

    private DiscoveryResultItem DeserializeResults(
      int nodeId,
      IDataReader result)
    {
      Guid guid = Guid.NewGuid();
      DateTime dateTime = (DateTime) result[0];
      string s = (string) result[1];
      PersistentDiscoveryCache._log.DebugFormat("Found data in cache for Node {0} from {1}", (object) nodeId, (object) dateTime);
      DiscoveryResultItem discoveryResultItem1 = new DiscoveryResultItem(guid, new int?(nodeId), dateTime);
      DataContractSerializer contractSerializer = new DataContractSerializer(typeof (DiscoveredObjectTree));
      try
      {
        using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s)))
        {
          XmlDictionaryReader textReader = XmlDictionaryReader.CreateTextReader((Stream) memoryStream, Encoding.UTF8, new XmlDictionaryReaderQuotas(), (OnXmlDictionaryReaderClose) null);
          DiscoveryResultItem discoveryResultItem2 = discoveryResultItem1;
          OrionDiscoveryJobProgressInfo discoveryJobProgressInfo = new OrionDiscoveryJobProgressInfo();
          discoveryJobProgressInfo.set_Status(new DiscoveryComplexStatus((DiscoveryStatus) 2, string.Empty));
          discoveryJobProgressInfo.set_JobId(guid);
          discoveryResultItem2.set_Progress(discoveryJobProgressInfo);
          discoveryResultItem1.set_ResultTree((DiscoveredObjectTree) contractSerializer.ReadObject(textReader));
        }
        return discoveryResultItem1;
      }
      catch (Exception ex)
      {
        PersistentDiscoveryCache._log.Error((object) "Error while deserializing result tree!", ex);
        return (DiscoveryResultItem) null;
      }
    }

    public void StoreResultForNode(int nodeId, DiscoveryResultItem result)
    {
      DateTime now = DateTime.Now;
      try
      {
        string str;
        using (MemoryStream memoryStream = new MemoryStream())
        {
          new DataContractSerializer(typeof (DiscoveredObjectTree)).WriteObject((Stream) memoryStream, (object) result.get_ResultTree());
          str = Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE NodeListResourcesCache SET \r\nCachedTime = @time, CacheBlob = @blob WHERE NodeId=@nodeId\r\nIF @@ROWCOUNT = 0\r\n    INSERT INTO NodeListResourcesCache (NodeId, CachedTime, CacheBlob) VALUES(\r\n     @nodeId, @time, @blob\r\n    )"))
        {
          textCommand.Parameters.AddWithValue("@nodeId", (object) nodeId);
          textCommand.Parameters.AddWithValue("@time", (object) now);
          textCommand.Parameters.AddWithValue("@blob", (object) str);
          SqlHelper.ExecuteNonQuery(textCommand);
        }
      }
      catch (Exception ex)
      {
        PersistentDiscoveryCache._log.Error((object) string.Format("Error occured storing cached results for node [{0}]", (object) nodeId), ex);
      }
    }
  }
}
