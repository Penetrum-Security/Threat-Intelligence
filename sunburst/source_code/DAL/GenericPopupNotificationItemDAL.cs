// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.GenericPopupNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using System;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class GenericPopupNotificationItemDAL : NotificationItemDAL
  {
    protected GenericPopupNotificationItemDAL()
    {
    }

    protected virtual Guid GetPopupNotificationItemId()
    {
      return Guid.Empty;
    }

    protected static TNotificationItem Create<TNotificationItem>(string title, string url) where TNotificationItem : GenericPopupNotificationItemDAL, new()
    {
      Guid notificationItemId = new TNotificationItem().GetPopupNotificationItemId();
      if (notificationItemId == Guid.Empty)
        throw new ArgumentException("Can't obtain Popup Notification Item GUID", nameof (TNotificationItem));
      TNotificationItem itemById = NotificationItemDAL.GetItemById<TNotificationItem>(notificationItemId);
      if ((object) itemById == null)
        return NotificationItemDAL.Insert<TNotificationItem>(notificationItemId, title, (string) null, false, url, new DateTime?(), (string) null);
      itemById.Title = title;
      itemById.Description = (string) null;
      itemById.Url = url;
      itemById.CreatedAt = DateTime.UtcNow;
      itemById.SetNotAcknowledged();
      itemById.Ignored = false;
      return !itemById.Update() ? default (TNotificationItem) : itemById;
    }
  }
}
