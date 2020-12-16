// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.DiscoveryAutoImportNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Globalization;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class DiscoveryAutoImportNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid DiscoveryAutoImportNotificationItemId = new Guid("{D52F46CF-99CA-4E93-9EA4-1FB9D8F27E46}");
    public static readonly Guid DiscoveryAutoImportNotificationTypeGuid = new Guid("{DD441A02-4789-4716-9A48-F0F7E3FC3EB4}");
    public static readonly string NetworkSonarDiscoveryURL = "/Orion/Discovery/Default.aspx";

    public static DiscoveryAutoImportNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<DiscoveryAutoImportNotificationItemDAL>(DiscoveryAutoImportNotificationItemDAL.DiscoveryAutoImportNotificationItemId);
    }

    public static void Show(DiscoveryResultBase result, StartImportStatus status)
    {
      DiscoveryAutoImportNotificationItemDAL notificationItemDal = DiscoveryAutoImportNotificationItemDAL.GetItem();
      string description = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "DiscoveryImportStatus:{0}", (object) status);
      string empty = string.Empty;
      string title;
      switch (status - 4)
      {
        case 0:
          title = Resources2.get_Notification_DiscoveryAutoImport_Failed();
          break;
        case 1:
          title = Resources2.get_Notification_DiscoveryAutoImport_LicenseExceeded();
          break;
        case 2:
          title = Resources2.get_Notification_DiscoveryAutoImport_Succeeded();
          break;
        default:
          return;
      }
      if (notificationItemDal == null)
      {
        NotificationItemDAL.Insert(DiscoveryAutoImportNotificationItemDAL.DiscoveryAutoImportNotificationItemId, DiscoveryAutoImportNotificationItemDAL.DiscoveryAutoImportNotificationTypeGuid, title, description, false, DiscoveryAutoImportNotificationItemDAL.NetworkSonarDiscoveryURL, new DateTime?(), (string) null);
      }
      else
      {
        notificationItemDal.SetNotAcknowledged();
        notificationItemDal.Title = title;
        notificationItemDal.Description = description;
        notificationItemDal.Update();
      }
    }

    public static void Hide()
    {
      NotificationItemDAL.Delete(DiscoveryAutoImportNotificationItemDAL.DiscoveryAutoImportNotificationItemId);
    }
  }
}
