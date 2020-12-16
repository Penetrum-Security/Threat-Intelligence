// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.EvaluationExpirationNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal sealed class EvaluationExpirationNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid EvaluationExpirationNotificationItemId = new Guid("{AFA69A0B-2313-48C6-A8EA-BF6A0A256A1C}");
    public static readonly Guid EvaluationExpirationNotificationTypeGuid = new Guid("{6EE3D05F-7555-4E3E-9338-AA338834FE36}");

    public static EvaluationExpirationNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<EvaluationExpirationNotificationItemDAL>(EvaluationExpirationNotificationItemDAL.EvaluationExpirationNotificationItemId);
    }

    public void Show(IEnumerable<ModuleLicenseInfo> expiringModules)
    {
      if (expiringModules == null)
        throw new ArgumentNullException(nameof (expiringModules));
      Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> dictionary = new Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>();
      using (IEnumerator<ModuleLicenseInfo> enumerator = expiringModules.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          ModuleLicenseInfo current = enumerator.Current;
          dictionary[current.get_ModuleName()] = new EvaluationExpirationNotificationItemDAL.ExpirationInfo()
          {
            ModuleName = current.get_ModuleName(),
            LastRemindMeLater = new DateTime?(),
            DaysToExpire = current.get_DaysRemaining()
          };
        }
      }
      this.Show((IDictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>) dictionary);
    }

    private void Show(
      IDictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> expirations)
    {
      EvaluationExpirationNotificationItemDAL notificationItemDal = EvaluationExpirationNotificationItemDAL.GetItem();
      if (notificationItemDal == null)
      {
        string description = EvaluationExpirationNotificationItemDAL.Serialize(expirations);
        NotificationItemDAL.Insert(EvaluationExpirationNotificationItemDAL.EvaluationExpirationNotificationItemId, EvaluationExpirationNotificationItemDAL.EvaluationExpirationNotificationTypeGuid, Resources.get_LIBCODE_LC0_1(), description, false, "javascript:SW.Core.SalesTrigger.ShowEvalPopupAsync();", new DateTime?(), (string) null);
      }
      else
      {
        Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> previousExpirations = EvaluationExpirationNotificationItemDAL.Deserialize(notificationItemDal.Description);
        int showExpiredAgainAt = BusinessLayerSettings.Instance.EvaluationExpirationShowAgainAtDays;
        foreach (KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> keyValuePair in previousExpirations.Where<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>, bool>) (previousExpiration => expirations.ContainsKey(previousExpiration.Key))))
          expirations[keyValuePair.Key].LastRemindMeLater = keyValuePair.Value.LastRemindMeLater;
        int daysFromLastRemindMeLater = (int) DateTime.UtcNow.Subtract(notificationItemDal.AcknowledgedAt ?? DateTime.UtcNow).TotalDays;
        DateTime? acknowledgedAt = notificationItemDal.AcknowledgedAt;
        if (expirations.Any<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>, bool>) (module => !previousExpirations.ContainsKey(module.Key) || previousExpirations.ContainsKey(module.Key) && module.Value.DaysToExpire > 0 && daysFromLastRemindMeLater == showExpiredAgainAt || previousExpirations.ContainsKey(module.Key) && previousExpirations[module.Key].DaysToExpire > 0 && module.Value.DaysToExpire <= 0 || (int) DateTime.UtcNow.Subtract(module.Value.LastRemindMeLater ?? DateTime.UtcNow).TotalDays == showExpiredAgainAt)))
          notificationItemDal.SetNotAcknowledged();
        if (acknowledgedAt.HasValue)
        {
          foreach (KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> keyValuePair in expirations.Where<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>>((Func<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>, bool>) (m => m.Value.DaysToExpire <= 0 && (!m.Value.LastRemindMeLater.HasValue && previousExpirations.ContainsKey(m.Key)) && previousExpirations[m.Key].DaysToExpire <= 0)))
            keyValuePair.Value.LastRemindMeLater = acknowledgedAt;
        }
        notificationItemDal.TypeId = EvaluationExpirationNotificationItemDAL.EvaluationExpirationNotificationTypeGuid;
        notificationItemDal.Description = EvaluationExpirationNotificationItemDAL.Serialize(expirations);
        notificationItemDal.Url = "javascript:SW.Core.SalesTrigger.ShowEvalPopupAsync();";
        notificationItemDal.Title = Resources.get_LIBCODE_LC0_1();
        notificationItemDal.Update();
      }
    }

    public void Hide()
    {
      NotificationItemDAL.Delete(EvaluationExpirationNotificationItemDAL.EvaluationExpirationNotificationItemId);
    }

    public void CheckEvaluationExpiration()
    {
      ILicensingDAL licensing = (ILicensingDAL) new LicensingDAL();
      List<LicenseInfoModel> list = ((IEnumerable<LicenseInfoModel>) licensing.GetLicenses()).Where<LicenseInfoModel>((Func<LicenseInfoModel, bool>) (license => license.get_IsEvaluation() && (license.get_IsExpired() || license.get_DaysRemainingCount() <= BusinessLayerSettings.Instance.EvaluationExpirationNotificationDays) && !string.Equals("DPI", license.get_ProductName(), StringComparison.OrdinalIgnoreCase))).ToList<LicenseInfoModel>();
      licensing.FilterHiddenEvalLicenses(list);
      if (((IEnumerable<LicenseInfoModel>) list).All<LicenseInfoModel>((Func<LicenseInfoModel, bool>) (lic => licensing.get_DefaultLicenseFilter().Any<string>((Func<string, bool>) (module => string.Equals(module, lic.get_ProductName(), StringComparison.OrdinalIgnoreCase))))))
      {
        this.Hide();
      }
      else
      {
        Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> dictionary = new Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>();
        using (List<LicenseInfoModel>.Enumerator enumerator = list.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            LicenseInfoModel current = enumerator.Current;
            dictionary[current.get_LicenseName()] = new EvaluationExpirationNotificationItemDAL.ExpirationInfo()
            {
              ModuleName = current.get_LicenseName(),
              LastRemindMeLater = new DateTime?(),
              DaysToExpire = current.get_DaysRemainingCount()
            };
          }
        }
        this.Show((IDictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>) dictionary);
      }
    }

    private static string Serialize(
      IDictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> moduleExpirations)
    {
      return string.Join("|", moduleExpirations.Select<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>, string>((Func<KeyValuePair<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>, string>) (m => string.Format("{0};{1};{2};{3}", (object) m.Key, (object) m.Value.DaysToExpire, (object) m.Value.ModuleName, (object) m.Value.LastRemindMeLater))).ToArray<string>());
    }

    private static Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> Deserialize(
      string moduleExpirations)
    {
      Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo> dictionary = new Dictionary<string, EvaluationExpirationNotificationItemDAL.ExpirationInfo>();
      if (!string.IsNullOrEmpty(moduleExpirations))
      {
        string str1 = moduleExpirations;
        char[] chArray = new char[1]{ '|' };
        foreach (string str2 in str1.Split(chArray))
        {
          try
          {
            string[] strArray = str2.Split(';');
            EvaluationExpirationNotificationItemDAL.ExpirationInfo expirationInfo = new EvaluationExpirationNotificationItemDAL.ExpirationInfo();
            expirationInfo.DaysToExpire = Convert.ToInt32(strArray[1]);
            if (strArray.Length > 2 && !string.IsNullOrWhiteSpace(strArray[2]))
              expirationInfo.ModuleName = strArray[2];
            if (strArray.Length > 3 && !string.IsNullOrWhiteSpace(strArray[3]))
              expirationInfo.LastRemindMeLater = new DateTime?(DateTime.Parse(strArray[3]));
            dictionary[strArray[0]] = expirationInfo;
          }
          catch (Exception ex)
          {
            NotificationItemDAL.log.Warn((object) "Unable to parse evaluation expiration notification panel data", ex);
          }
        }
      }
      return dictionary;
    }

    private class ExpirationInfo
    {
      public string ModuleName { get; set; }

      public DateTime? LastRemindMeLater { get; set; }

      public int DaysToExpire { get; set; }
    }
  }
}
