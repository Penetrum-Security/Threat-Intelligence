// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.ScheduledDiscoveryNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class ScheduledDiscoveryNotificationItemDAL : GenericPopupNotificationItemDAL
  {
    public static readonly Guid ScheduledDiscoveryNotificationItemId = new Guid("3D28249D-EFE1-462e-B1A7-C55273D09AE8");

    public ScheduledDiscoveryNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<ScheduledDiscoveryNotificationItemDAL>(ScheduledDiscoveryNotificationItemDAL.ScheduledDiscoveryNotificationItemId);
    }

    protected override Guid GetNotificationItemTypeId()
    {
      return (Guid) GenericNotificationItem.ScheduledDiscoveryNotificationTypeGuid;
    }

    protected override Guid GetPopupNotificationItemId()
    {
      return ScheduledDiscoveryNotificationItemDAL.ScheduledDiscoveryNotificationItemId;
    }

    public static ScheduledDiscoveryNotificationItemDAL Create(
      string title,
      string url)
    {
      return GenericPopupNotificationItemDAL.Create<ScheduledDiscoveryNotificationItemDAL>(title, url);
    }
  }
}
