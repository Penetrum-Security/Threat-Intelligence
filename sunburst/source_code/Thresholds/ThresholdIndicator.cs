// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.ThresholdIndicator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.InformationService.Contract2;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.Notification;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub;
using System;
using System.Collections.Generic;
using System.Data;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class ThresholdIndicator : IThresholdIndicator
  {
    private readonly IInformationServiceProxyFactory _swisFactory;
    private readonly IPublisherManager _publishManager;
    private DataTable _previousThresholdValues;

    public ThresholdIndicator()
      : this((IInformationServiceProxyFactory) new InformationServiceProxyFactory(), PublisherClient.get_Instance())
    {
    }

    public ThresholdIndicator(
      IInformationServiceProxyFactory swisFactory,
      IPublisherManager publishManager)
    {
      IInformationServiceProxyFactory serviceProxyFactory = swisFactory;
      if (serviceProxyFactory == null)
        throw new ArgumentNullException(nameof (swisFactory));
      this._swisFactory = serviceProxyFactory;
      IPublisherManager ipublisherManager = publishManager;
      if (ipublisherManager == null)
        throw new ArgumentNullException(nameof (publishManager));
      this._publishManager = ipublisherManager;
    }

    private ThresholdIndicator.InstanceInformation GetInstanceInformation(
      string entityType,
      int instanceId)
    {
      if (string.IsNullOrEmpty(entityType) || instanceId == 0)
        return (ThresholdIndicator.InstanceInformation) null;
      ThresholdIndicator.InstanceInformation instanceInformation = new ThresholdIndicator.InstanceInformation();
      using (IInformationServiceProxy2 connection = this._swisFactory.CreateConnection())
      {
        DataTable dataTable1 = ((IInformationServiceProxy) connection).Query("SELECT TOP 1 Prefix, KeyProperty, NameProperty FROM Orion.NetObjectTypes WHERE EntityType = @entityType", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            nameof (entityType),
            (object) entityType
          }
        });
        if (dataTable1 != null)
        {
          if (dataTable1.Rows.Count == 1)
          {
            string str = dataTable1.Rows[0]["Prefix"] as string;
            object obj1 = dataTable1.Rows[0]["KeyProperty"];
            object obj2 = dataTable1.Rows[0]["NameProperty"];
            instanceInformation.NetObject = string.Format("{0}:{1}", (object) str, (object) instanceId);
            if (obj1 != DBNull.Value && obj1 != DBNull.Value)
            {
              DataTable dataTable2 = ((IInformationServiceProxy) connection).Query(string.Format("SELECT {0} FROM {1} WHERE {2} = @InstanceId", obj2, (object) entityType, obj1), (IDictionary<string, object>) new Dictionary<string, object>()
              {
                {
                  "InstanceId",
                  (object) instanceId
                }
              });
              instanceInformation.InstanceName = dataTable2 == null || dataTable2.Rows.Count <= 0 ? instanceInformation.NetObject : dataTable2.Rows[0][obj2.ToString()].ToString();
            }
            else
              instanceInformation.InstanceName = instanceInformation.NetObject;
          }
        }
      }
      return instanceInformation;
    }

    public void LoadPreviousThresholdData(int instanceId, string thresholdName)
    {
      using (IInformationServiceProxy2 connection = this._swisFactory.CreateConnection())
        this._previousThresholdValues = ((IInformationServiceProxy) connection).Query("SELECT OT.ThresholdOperator,\r\n                    OT.Level1Value,\r\n                    OT.Level1Formula,\r\n                    OT.Level2Value,\r\n                    OT.Level2Formula,\r\n                    OT.WarningPolls,\r\n                    OT.WarningPollsInterval,\r\n                    OT.CriticalPolls,\r\n                    OT.CriticalPollsInterval,\r\n                    OT.WarningEnabled,\r\n                    OT.CriticalEnabled\r\n                    FROM Orion.Thresholds OT\r\n                    WHERE OT.InstanceId = @InstanceId AND OT.Name = @Name", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "InstanceId",
            (object) instanceId
          },
          {
            "Name",
            (object) thresholdName
          }
        });
    }

    private string GetThresholdEntityType(string thresholdName)
    {
      using (IInformationServiceProxy2 connection = this._swisFactory.CreateConnection())
      {
        DataTable dataTable = ((IInformationServiceProxy) connection).Query("SELECT EntityType FROM Orion.Thresholds WHERE Name = @Name", (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "Name",
            (object) thresholdName
          }
        });
        if (dataTable != null)
        {
          if (dataTable.Rows.Count > 0)
            return dataTable.Rows[0]["EntityType"].ToString();
        }
      }
      return (string) null;
    }

    public void ReportThresholdNotification(Threshold threshold)
    {
      if (threshold == null)
        throw new ArgumentNullException(nameof (threshold));
      ThresholdIndicator.InstanceInformation instanceInformation = this.GetInstanceInformation(this.GetThresholdEntityType(threshold.get_ThresholdName()), threshold.get_InstanceId());
      PropertyBag propertyBag1 = new PropertyBag();
      ((Dictionary<string, object>) propertyBag1).Add("InstanceType", (object) "Orion.Thresholds");
      ((Dictionary<string, object>) propertyBag1).Add("Name", (object) threshold.get_ThresholdName());
      ((Dictionary<string, object>) propertyBag1).Add("InstanceName", instanceInformation != null ? (object) instanceInformation.InstanceName : (object) threshold.get_InstanceId().ToString());
      ((Dictionary<string, object>) propertyBag1).Add("InstanceId", (object) threshold.get_InstanceId());
      ((Dictionary<string, object>) propertyBag1).Add("ThresholdType", (object) (int) threshold.get_ThresholdType());
      ((Dictionary<string, object>) propertyBag1).Add("ThresholdOperator", (object) (int) threshold.get_ThresholdOperator());
      ((Dictionary<string, object>) propertyBag1).Add("Level1Value", (object) threshold.get_Warning());
      ((Dictionary<string, object>) propertyBag1).Add("Level2Value", (object) threshold.get_Critical());
      ((Dictionary<string, object>) propertyBag1).Add("Level1Formula", (object) threshold.get_WarningFormula());
      ((Dictionary<string, object>) propertyBag1).Add("Level2Formula", (object) threshold.get_CriticalFormula());
      ((Dictionary<string, object>) propertyBag1).Add("WarningPolls", (object) threshold.get_WarningPolls());
      ((Dictionary<string, object>) propertyBag1).Add("WarningPollsInterval", (object) threshold.get_WarningPollsInterval());
      ((Dictionary<string, object>) propertyBag1).Add("CriticalPolls", (object) threshold.get_CriticalPolls());
      ((Dictionary<string, object>) propertyBag1).Add("CriticalPollsInterval", (object) threshold.get_CriticalPollsInterval());
      ((Dictionary<string, object>) propertyBag1).Add("WarningEnabled", (object) threshold.get_WarningEnabled());
      ((Dictionary<string, object>) propertyBag1).Add("CriticalEnabled", (object) threshold.get_CriticalEnabled());
      PropertyBag propertyBag2 = propertyBag1;
      if (instanceInformation != null && !string.IsNullOrEmpty(instanceInformation.NetObject))
        ((Dictionary<string, object>) propertyBag2).Add("NetObject", (object) instanceInformation.NetObject);
      if (this._previousThresholdValues != null && this._previousThresholdValues.Rows.Count > 0)
      {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        object obj1 = this._previousThresholdValues.Rows[0]["ThresholdOperator"];
        object obj2 = this._previousThresholdValues.Rows[0]["Level1Value"];
        object obj3 = this._previousThresholdValues.Rows[0]["Level2Value"];
        object obj4 = this._previousThresholdValues.Rows[0]["Level1Formula"];
        object obj5 = this._previousThresholdValues.Rows[0]["Level2Formula"];
        object obj6 = this._previousThresholdValues.Rows[0]["WarningPolls"];
        object obj7 = this._previousThresholdValues.Rows[0]["WarningPollsInterval"];
        object obj8 = this._previousThresholdValues.Rows[0]["CriticalPolls"];
        object obj9 = this._previousThresholdValues.Rows[0]["CriticalPollsInterval"];
        object obj10 = this._previousThresholdValues.Rows[0]["WarningEnabled"];
        object obj11 = this._previousThresholdValues.Rows[0]["CriticalEnabled"];
        dictionary.Add("ThresholdOperator", obj1 != DBNull.Value ? obj1 : (object) null);
        dictionary.Add("Level1Value", obj2 != DBNull.Value ? obj2 : (object) null);
        dictionary.Add("Level2Value", obj3 != DBNull.Value ? obj3 : (object) null);
        dictionary.Add("Level1Formula", obj4 != DBNull.Value ? obj4 : (object) null);
        dictionary.Add("Level2Formula", obj5 != DBNull.Value ? obj5 : (object) null);
        dictionary.Add("WarningPolls", obj6 != DBNull.Value ? obj6 : (object) null);
        dictionary.Add("WarningPollsInterval", obj7 != DBNull.Value ? obj7 : (object) null);
        dictionary.Add("CriticalPolls", obj8 != DBNull.Value ? obj8 : (object) null);
        dictionary.Add("CriticalPollsInterval", obj9 != DBNull.Value ? obj9 : (object) null);
        dictionary.Add("WarningEnabled", obj10 != DBNull.Value ? obj10 : (object) null);
        dictionary.Add("CriticalEnabled", obj11 != DBNull.Value ? obj11 : (object) null);
        if (dictionary.Count > 0)
          ((Dictionary<string, object>) propertyBag2).Add("PreviousProperties", (object) dictionary);
        this._previousThresholdValues.Clear();
      }
      this._publishManager.Publish((INotification) new ThresholdNotification(IndicationHelper.GetIndicationType((IndicationType) 2), (IDictionary<string, object>) propertyBag2));
    }

    public class InstanceInformation
    {
      public string NetObject { get; set; }

      public string InstanceName { get; set; }
    }
  }
}
