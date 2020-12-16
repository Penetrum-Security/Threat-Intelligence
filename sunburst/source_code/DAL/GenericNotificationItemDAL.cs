// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.GenericNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class GenericNotificationItemDAL : NotificationItemDAL
  {
    protected override Guid GetNotificationItemTypeId()
    {
      return (Guid) GenericNotificationItem.GenericNotificationTypeGuid;
    }

    public static GenericNotificationItemDAL GetItemById(Guid itemId)
    {
      return NotificationItemDAL.GetItemById<GenericNotificationItemDAL>(itemId);
    }

    public static GenericNotificationItemDAL GetLatestItem()
    {
      return NotificationItemDAL.GetLatestItem<GenericNotificationItemDAL>(new NotificationItemFilter(false, false));
    }

    public static ICollection<GenericNotificationItemDAL> GetItems(
      NotificationItemFilter filter)
    {
      return NotificationItemDAL.GetItems<GenericNotificationItemDAL>(filter);
    }

    public static int GetNotificationItemsCount()
    {
      return NotificationItemDAL.GetNotificationsCount<GenericNotificationItemDAL>(new NotificationItemFilter(false, false));
    }

    public static GenericNotificationItemDAL Insert(
      Guid notificationId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
    {
      return NotificationItemDAL.Insert<GenericNotificationItemDAL>(notificationId, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
    }
  }
}
