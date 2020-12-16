// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.LicenseSaturationNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class LicenseSaturationNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid LicenseSaturationNotificationItemId = new Guid("{B138550D-824C-482d-9CBB-D82A6C95EC3B}");
    private static readonly string popupCallFunction = "javascript:SW.Core.SalesTrigger.ShowLicensePopupAsync();";

    private static string NotificationMessge
    {
      get
      {
        return string.Format(Resources.get_LIBCODE_PCC_24()) + " " + string.Format(Resources.get_LIBCODE_PCC_25());
      }
    }

    protected override Guid GetNotificationItemTypeId()
    {
      return (Guid) GenericNotificationItem.LicenseSaturationNotificationTypeGuid;
    }

    public static LicenseSaturationNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<LicenseSaturationNotificationItemDAL>(LicenseSaturationNotificationItemDAL.LicenseSaturationNotificationItemId);
    }

    public static void Show(IEnumerable<string> elementsOverLimit)
    {
      string description = string.Join(";", elementsOverLimit.ToArray<string>());
      LicenseSaturationNotificationItemDAL notificationItemDal = LicenseSaturationNotificationItemDAL.GetItem();
      if (notificationItemDal == null)
      {
        NotificationItemDAL.Insert<LicenseSaturationNotificationItemDAL>(LicenseSaturationNotificationItemDAL.LicenseSaturationNotificationItemId, LicenseSaturationNotificationItemDAL.NotificationMessge, description, false, LicenseSaturationNotificationItemDAL.popupCallFunction, new DateTime?(), (string) null);
      }
      else
      {
        if (description == notificationItemDal.Description)
          return;
        int num;
        if (!string.IsNullOrEmpty(notificationItemDal.Description))
          num = elementsOverLimit.Except<string>((IEnumerable<string>) notificationItemDal.Description.Split(';')).Count<string>() > 0 ? 1 : 0;
        else
          num = 1;
        if (num != 0)
          notificationItemDal.SetNotAcknowledged();
        notificationItemDal.Description = description;
        notificationItemDal.Update();
      }
    }

    public static void Hide()
    {
      NotificationItemDAL.Delete(LicenseSaturationNotificationItemDAL.LicenseSaturationNotificationItemId);
    }
  }
}
