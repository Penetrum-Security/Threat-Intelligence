// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.NodeStatus.EnhancedNodeStatusIndicator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Contract2;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Indications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace SolarWinds.Orion.Core.BusinessLayer.NodeStatus
{
  public class EnhancedNodeStatusIndicator
  {
    private static string SelectNodeStatusQuery = "SELECT TOP 1000 [ID], [Data]FROM [DatabaseIndicationQueue] WHERE [Owner] = 'Core.Status' ORDER BY ID";
    private static string DeleteNodeStatusQuery = "DELETE FROM [DatabaseIndicationQueue] WHERE ID=@id";
    private static readonly Log log = new Log();
    private readonly ISqlHelper sqlHelper;
    private readonly IIndicationReporterPublisher ip;

    public EnhancedNodeStatusIndicator(ISqlHelper sqlHelper, IIndicationReporterPublisher ip)
    {
      this.sqlHelper = sqlHelper;
      this.ip = ip;
    }

    public void Execute()
    {
      this.ProcessIndications((IEnumerable<EnhancedNodeStatusIndicator.IndicationInfo>) this.ReadFromDB());
    }

    private List<EnhancedNodeStatusIndicator.IndicationInfo> ReadFromDB()
    {
      List<EnhancedNodeStatusIndicator.IndicationInfo> indicationInfoList = new List<EnhancedNodeStatusIndicator.IndicationInfo>();
      using (SqlCommand textCommand = this.sqlHelper.GetTextCommand(EnhancedNodeStatusIndicator.SelectNodeStatusQuery))
      {
        using (IDataReader dataReader = this.sqlHelper.ExecuteReader(textCommand))
        {
          try
          {
            while (dataReader.Read())
            {
              int int32 = dataReader.GetInt32(0);
              string str = dataReader.GetString(1);
              indicationInfoList.Add(new EnhancedNodeStatusIndicator.IndicationInfo()
              {
                Id = int32,
                Data = str
              });
              EnhancedNodeStatusIndicator.log.Debug((object) string.Format("Reading new indication info from DB ({0},{1})", (object) int32, (object) str));
            }
          }
          catch (Exception ex)
          {
            EnhancedNodeStatusIndicator.log.Warn((object) "Reading indication data failed", ex);
          }
        }
      }
      return indicationInfoList;
    }

    private void DeleteIndicationFromDB(EnhancedNodeStatusIndicator.IndicationInfo record)
    {
      try
      {
        SqlCommand textCommand = this.sqlHelper.GetTextCommand(EnhancedNodeStatusIndicator.DeleteNodeStatusQuery);
        textCommand.Parameters.Add(new SqlParameter("id", (object) record.Id));
        this.sqlHelper.ExecuteNonQuery(textCommand);
      }
      catch (Exception ex)
      {
        EnhancedNodeStatusIndicator.log.Error((object) "Deleting from indication table failed", ex);
      }
    }

    internal void ProcessIndications(
      IEnumerable<EnhancedNodeStatusIndicator.IndicationInfo> indications)
    {
      foreach (EnhancedNodeStatusIndicator.IndicationInfo indication in indications)
      {
        try
        {
          EnhancedNodeStatusIndicator.NodeStatusIndication statusIndication = (EnhancedNodeStatusIndicator.NodeStatusIndication) OrionSerializationHelper.FromJSON(indication.Data, typeof (EnhancedNodeStatusIndicator.NodeStatusIndication));
          PropertyBag propertyBag1 = new PropertyBag();
          ((Dictionary<string, object>) propertyBag1)["PreviousStatus"] = (object) statusIndication.PreviousStatus;
          PropertyBag propertyBag2 = new PropertyBag();
          ((Dictionary<string, object>) propertyBag2)["NodeID"] = (object) statusIndication.NodeID;
          ((Dictionary<string, object>) propertyBag2)["Status"] = (object) statusIndication.Status;
          ((Dictionary<string, object>) propertyBag2)["InstanceType"] = (object) "Orion.Nodes";
          ((Dictionary<string, object>) propertyBag2)["PreviousProperties"] = (object) propertyBag1;
          this.ip.ReportIndication(IndicationHelper.GetIndicationType((IndicationType) 2), IndicationHelper.GetIndicationProperties(), propertyBag2);
          this.DeleteIndicationFromDB(indication);
          EnhancedNodeStatusIndicator.log.Debug((object) ("Enhanced node status indication processed " + string.Format("(N:{0} [{1}]->[{2}])", (object) statusIndication.NodeID, (object) statusIndication.PreviousStatus, (object) statusIndication.Status)));
        }
        catch (Exception ex)
        {
          EnhancedNodeStatusIndicator.log.Error((object) "Indication processing failed", ex);
        }
      }
    }

    internal class IndicationInfo
    {
      public int Id { get; set; }

      public string Data { get; set; }
    }

    [DataContract]
    internal class NodeStatusIndication
    {
      [DataMember]
      public int NodeID { get; set; }

      [DataMember]
      public int Status { get; set; }

      [DataMember]
      public int PreviousStatus { get; set; }
    }
  }
}
