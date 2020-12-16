// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MaintenanceRenewalFilter
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class MaintenanceRenewalFilter : NotificationItemFilter
  {
    public string ProductTag { get; set; }

    public MaintenanceRenewalFilter(
      bool includeAcknowledged,
      bool includeIgnored,
      string productTag)
      : base(includeAcknowledged, includeIgnored)
    {
      this.ProductTag = productTag;
    }

    public MaintenanceRenewalFilter()
      : this(false, false, (string) null)
    {
    }
  }
}
