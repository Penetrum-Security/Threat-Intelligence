// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.PollerLimitNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class PollerLimitNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid PollerStatusNotificationItemId = new Guid("{C7070869-B2B8-42ED-8472-7F24056435D9}");
    public static readonly Guid PollerLimitWarningNotificationTypeGuid = new Guid("{68DF81BD-4025-4D7B-9296-C62C397AAC88}");
    public static readonly Guid PollerLimitReachedNotificationTypeGuid = new Guid("{25130585-7C09-4052-AF01-C706CC032940}");
    public static readonly string WarningTitle = Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_1();
    public static readonly string LimitReachedTitle = Resources.get_COREBUSINESSLAYERDAL_CODE_YK0_2();

    public static PollerLimitNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<PollerLimitNotificationItemDAL>(PollerLimitNotificationItemDAL.PollerStatusNotificationItemId);
    }

    public static void Show(
      Dictionary<string, int> warningEngines,
      Dictionary<string, int> reachedEngines)
    {
      if (warningEngines == null)
        throw new ArgumentNullException(nameof (warningEngines));
      if (reachedEngines == null)
        throw new ArgumentNullException(nameof (reachedEngines));
      bool isWarning = reachedEngines.Count == 0;
      Guid typeId = reachedEngines.Count > 0 ? PollerLimitNotificationItemDAL.PollerLimitReachedNotificationTypeGuid : PollerLimitNotificationItemDAL.PollerLimitWarningNotificationTypeGuid;
      PollerLimitNotificationItemDAL.EnginesStatus enginesStatus1 = new PollerLimitNotificationItemDAL.EnginesStatus(warningEngines, reachedEngines);
      string description = enginesStatus1.Serialize();
      PollerLimitNotificationItemDAL notificationItemDal = PollerLimitNotificationItemDAL.GetItem();
      string url = "javascript:SW.Core.SalesTrigger.ShowPollerLimitPopupAsync();";
      if (notificationItemDal == null)
      {
        NotificationItemDAL.Insert(PollerLimitNotificationItemDAL.PollerStatusNotificationItemId, typeId, PollerLimitNotificationItemDAL.GetNotificationMessage(isWarning), description, false, url, new DateTime?(), (string) null);
      }
      else
      {
        PollerLimitNotificationItemDAL.EnginesStatus enginesStatus2 = new PollerLimitNotificationItemDAL.EnginesStatus(notificationItemDal.Description);
        if (enginesStatus1.Extends(enginesStatus2))
          notificationItemDal.SetNotAcknowledged();
        notificationItemDal.TypeId = typeId;
        notificationItemDal.Description = description;
        notificationItemDal.Url = url;
        notificationItemDal.Title = PollerLimitNotificationItemDAL.GetNotificationMessage(isWarning);
        notificationItemDal.Update();
      }
    }

    public static void Hide()
    {
      NotificationItemDAL.Delete(PollerLimitNotificationItemDAL.PollerStatusNotificationItemId);
    }

    private static string GetNotificationMessage(bool isWarning)
    {
      return !isWarning ? PollerLimitNotificationItemDAL.LimitReachedTitle : PollerLimitNotificationItemDAL.WarningTitle;
    }

    private sealed class EnginesStatus
    {
      private Dictionary<string, int> warningEngines;
      private Dictionary<string, int> limitReachedEngines;

      private Dictionary<string, int> WarningEngines
      {
        get
        {
          return this.warningEngines;
        }
      }

      private Dictionary<string, int> LimitReachedEngines
      {
        get
        {
          return this.limitReachedEngines;
        }
      }

      public EnginesStatus(Dictionary<string, int> warn, Dictionary<string, int> reached)
      {
        this.warningEngines = warn;
        this.limitReachedEngines = reached;
      }

      public EnginesStatus(string enginesStatusString)
      {
        Dictionary<string, Dictionary<string, int>> dictionary = new Dictionary<string, Dictionary<string, int>>();
        if (string.IsNullOrEmpty(enginesStatusString))
          return;
        List<string> list = ((IEnumerable<string>) enginesStatusString.Split(new string[1]
        {
          "||"
        }, StringSplitOptions.None)).ToList<string>();
        if (list.Count < 2)
          throw new ArgumentException(nameof (enginesStatusString));
        this.warningEngines = new Dictionary<string, int>();
        ((IEnumerable<string>) list[0].Split(new char[1]
        {
          '|'
        }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (g =>
        {
          string[] strArray = g.Split(';');
          this.warningEngines[strArray[0]] = Convert.ToInt32(strArray[1]);
        }));
        this.limitReachedEngines = new Dictionary<string, int>();
        ((IEnumerable<string>) list[1].Split(new char[1]
        {
          '|'
        }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>().ForEach((Action<string>) (g =>
        {
          string[] strArray = g.Split(';');
          this.limitReachedEngines[strArray[0]] = Convert.ToInt32(strArray[1]);
        }));
      }

      public string Serialize()
      {
        return string.Join("||", string.Join("|", this.warningEngines.Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (e => e.Key + ";" + (object) e.Value)).ToArray<string>()), string.Join("|", this.limitReachedEngines.Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (e => e.Key + ";" + (object) e.Value)).ToArray<string>()));
      }

      public bool Extends(PollerLimitNotificationItemDAL.EnginesStatus value)
      {
        return value == null || this.WarningEngines.Any<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (engine => !value.WarningEngines.ContainsKey(engine.Key))) || this.LimitReachedEngines.Any<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (engine => !value.LimitReachedEngines.ContainsKey(engine.Key)));
      }
    }
  }
}
