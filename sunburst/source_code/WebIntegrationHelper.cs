// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.WebIntegrationHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Models.WebIntegration;
using SolarWinds.Orion.Web.Integration.Common.Models;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal static class WebIntegrationHelper
  {
    public static SupportCase ToSupportCase(this WebSupportCase webSupportCase)
    {
      SupportCase supportCase = new SupportCase();
      supportCase.set_CaseNumber(webSupportCase.get_CaseNumber());
      supportCase.set_CaseURL(webSupportCase.get_CaseURL());
      supportCase.set_LastUpdated(webSupportCase.get_LastUpdated());
      supportCase.set_Status((CaseStatus) webSupportCase.get_Status());
      supportCase.set_Title(webSupportCase.get_Title());
      return supportCase;
    }

    public static MaintenanceStatus ToMaintenanceStatus(
      this WebMaintenanceStatus webMaintenanceStatus)
    {
      MaintenanceStatus maintenanceStatus = new MaintenanceStatus();
      maintenanceStatus.set_ExpirationDate(webMaintenanceStatus.get_ExpirationDate());
      maintenanceStatus.set_ProductName(webMaintenanceStatus.get_ProductName());
      maintenanceStatus.set_ShortName(webMaintenanceStatus.get_ShortName());
      return maintenanceStatus;
    }
  }
}
