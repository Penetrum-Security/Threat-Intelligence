// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc.IMaintUpdateNotifySvc
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using System.CodeDom.Compiler;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc
{
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  [ServiceContract(ConfigurationName = "MaintUpdateNotifySvc.IMaintUpdateNotifySvc", Namespace = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09")]
  public interface IMaintUpdateNotifySvc
  {
    [OperationContract(Action = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetData", ReplyAction = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetDataResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    UpdateResponse GetData(UpdateRequest request);

    [OperationContract(Action = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetData", ReplyAction = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetDataResponse")]
    Task<UpdateResponse> GetDataAsync(UpdateRequest request);

    [OperationContract(Action = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetLocalizedData", ReplyAction = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetLocalizedDataResponse")]
    [XmlSerializerFormat(SupportFaults = true)]
    UpdateResponse GetLocalizedData(UpdateRequest request, string locale);

    [OperationContract(Action = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetLocalizedData", ReplyAction = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09/IMaintUpdateNotifySvc/GetLocalizedDataResponse")]
    Task<UpdateResponse> GetLocalizedDataAsync(
      UpdateRequest request,
      string locale);
  }
}
