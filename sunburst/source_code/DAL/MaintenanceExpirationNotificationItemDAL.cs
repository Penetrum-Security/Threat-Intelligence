// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MaintenanceExpirationNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class MaintenanceExpirationNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid MaintenanceExpirationNotificationItemId = new Guid("{561BE782-187F-4977-B5C4-B8666E73E582}");
    public static readonly Guid MaintenanceExpirationWarningNotificationTypeGuid = new Guid("{93465286-2E85-411D-8980-EFD32F04F0EE}");
    public static readonly Guid MaintenanceExpiredNotificationTypeGuid = new Guid("{ED77CD80-345D-4D51-B6A7-4AB3728F2200}");

    public static MaintenanceExpirationNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<MaintenanceExpirationNotificationItemDAL>(MaintenanceExpirationNotificationItemDAL.MaintenanceExpirationNotificationItemId);
    }

    public static void Show(Dictionary<string, int> moduleExpirations)
    {
      Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> moduleExpirations1 = new Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>();
      foreach (KeyValuePair<string, int> moduleExpiration in moduleExpirations)
        moduleExpirations1[moduleExpiration.Key] = new MaintenanceExpirationNotificationItemDAL.ExpirationInfo()
        {
          LicenseName = string.Empty,
          DaysToExpire = moduleExpiration.Value,
          LastRemindMeLaterDate = new DateTime?()
        };
      MaintenanceExpirationNotificationItemDAL.Show(moduleExpirations1);
    }

    internal static void Show(
      Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> moduleExpirations)
    {
      bool expired = moduleExpirations.Any<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, bool>) (m => m.Value.DaysToExpire <= 0));
      int daysToExpire = moduleExpirations.Min<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, int>) (x => x.Value.DaysToExpire));
      string url = "javascript:SW.Core.SalesTrigger.ShowMaintenancePopupAsync();";
      Guid typeId = expired ? MaintenanceExpirationNotificationItemDAL.MaintenanceExpiredNotificationTypeGuid : MaintenanceExpirationNotificationItemDAL.MaintenanceExpirationWarningNotificationTypeGuid;
      int expiredShowAgainAtDays = BusinessLayerSettings.Instance.MaintenanceExpiredShowAgainAtDays;
      MaintenanceExpirationNotificationItemDAL notificationItemDal = MaintenanceExpirationNotificationItemDAL.GetItem();
      if (notificationItemDal == null)
      {
        string description = MaintenanceExpirationNotificationItemDAL.Serialize(moduleExpirations);
        NotificationItemDAL.Insert(MaintenanceExpirationNotificationItemDAL.MaintenanceExpirationNotificationItemId, typeId, MaintenanceExpirationNotificationItemDAL.GetNotificationMessage(expired, daysToExpire), description, false, url, new DateTime?(), (string) null);
      }
      else
      {
        Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> previousExpirations = MaintenanceExpirationNotificationItemDAL.Deserialize(notificationItemDal.Description);
        foreach (KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> keyValuePair in previousExpirations.Where<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, bool>) (previousExpiration => moduleExpirations.ContainsKey(previousExpiration.Key))))
          moduleExpirations[keyValuePair.Key].LastRemindMeLaterDate = keyValuePair.Value.LastRemindMeLaterDate;
        DateTime utcNow1 = DateTime.UtcNow;
        ref DateTime local1 = ref utcNow1;
        DateTime? nullable = notificationItemDal.AcknowledgedAt;
        DateTime dateTime1 = nullable ?? DateTime.UtcNow;
        int totalDays = (int) local1.Subtract(dateTime1).TotalDays;
        DateTime? acknowledgedAt = notificationItemDal.AcknowledgedAt;
        foreach (KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> moduleExpiration in moduleExpirations)
        {
          if ((previousExpirations.ContainsKey(moduleExpiration.Key) || totalDays != expiredShowAgainAtDays) && (!previousExpirations.ContainsKey(moduleExpiration.Key) || moduleExpiration.Value.DaysToExpire <= 0 || totalDays != expiredShowAgainAtDays) && (!previousExpirations.ContainsKey(moduleExpiration.Key) || previousExpirations[moduleExpiration.Key].DaysToExpire <= 0 || moduleExpiration.Value.DaysToExpire > 0))
          {
            DateTime utcNow2 = DateTime.UtcNow;
            ref DateTime local2 = ref utcNow2;
            nullable = moduleExpiration.Value.LastRemindMeLaterDate;
            DateTime dateTime2 = nullable ?? DateTime.UtcNow;
            if ((int) local2.Subtract(dateTime2).TotalDays != expiredShowAgainAtDays)
              continue;
          }
          notificationItemDal.SetNotAcknowledged();
          break;
        }
        if (acknowledgedAt.HasValue)
        {
          foreach (KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> keyValuePair in moduleExpirations.Where<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, bool>) (m => m.Value.DaysToExpire <= 0 && (!m.Value.LastRemindMeLaterDate.HasValue && previousExpirations.ContainsKey(m.Key)) && previousExpirations[m.Key].DaysToExpire <= 0)))
            keyValuePair.Value.LastRemindMeLaterDate = acknowledgedAt;
        }
        notificationItemDal.TypeId = typeId;
        notificationItemDal.Description = MaintenanceExpirationNotificationItemDAL.Serialize(moduleExpirations);
        notificationItemDal.Url = url;
        notificationItemDal.Title = MaintenanceExpirationNotificationItemDAL.GetNotificationMessage(expired, daysToExpire);
        notificationItemDal.Update();
      }
    }

    public static void Hide()
    {
      NotificationItemDAL.Delete(MaintenanceExpirationNotificationItemDAL.MaintenanceExpirationNotificationItemId);
    }

    private static string GetNotificationMessage(bool expired, int daysToExpire)
    {
      return !expired ? string.Format(Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_4(), (object) daysToExpire, (object) "https://www.solarwinds.com/embedded_in_products/productLink.aspx?id=online_quote") : string.Format(Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_3(), (object) "https://www.solarwinds.com/embedded_in_products/productLink.aspx?id=online_quote");
    }

    private static string Serialize(
      Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> moduleExpirations)
    {
      return string.Join("|", moduleExpirations.Select<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, string>((Func<KeyValuePair<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>, string>) (m => string.Format("{0};{1};{2};{3};{4}", (object) m.Key, (object) m.Value.DaysToExpire, (object) m.Value.LicenseName, (object) m.Value.LastRemindMeLaterDate, (object) m.Value.ActivationKey))).ToArray<string>());
    }

    private static Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> Deserialize(
      string moduleExpirations)
    {
      Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo> dictionary = new Dictionary<string, MaintenanceExpirationNotificationItemDAL.ExpirationInfo>();
      if (!string.IsNullOrEmpty(moduleExpirations))
      {
        string str1 = moduleExpirations;
        char[] chArray = new char[1]{ '|' };
        foreach (string str2 in str1.Split(chArray))
        {
          try
          {
            string[] strArray = str2.Split(';');
            MaintenanceExpirationNotificationItemDAL.ExpirationInfo expirationInfo = new MaintenanceExpirationNotificationItemDAL.ExpirationInfo();
            expirationInfo.DaysToExpire = Convert.ToInt32(strArray[1]);
            if (strArray.Length > 2 && !string.IsNullOrWhiteSpace(strArray[2]))
              expirationInfo.LicenseName = strArray[2];
            if (strArray.Length > 3 && !string.IsNullOrWhiteSpace(strArray[3]))
              expirationInfo.LastRemindMeLaterDate = new DateTime?(DateTime.Parse(strArray[3]));
            if (strArray.Length > 4 && !string.IsNullOrWhiteSpace(strArray[4]))
              expirationInfo.ActivationKey = strArray[4];
            dictionary[strArray[0]] = expirationInfo;
          }
          catch (Exception ex)
          {
            NotificationItemDAL.log.Warn((object) "Unable to parse maintenance expiration notification panel data", ex);
          }
        }
      }
      return dictionary;
    }

    internal class ExpirationInfo
    {
      private string _moduleNameLegacy;

      [Obsolete("Use LicenseName instead", true)]
      public string ModuleName
      {
        get
        {
          return this._moduleNameLegacy ?? this.LicenseName;
        }
        set
        {
          this._moduleNameLegacy = value;
        }
      }

      public string LicenseName { get; set; }

      public int DaysToExpire { get; set; }

      public DateTime? LastRemindMeLaterDate { get; set; }

      public string ActivationKey { get; set; }
    }
  }
}
