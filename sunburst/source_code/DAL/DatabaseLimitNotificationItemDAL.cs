// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.DatabaseLimitNotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Globalization;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal sealed class DatabaseLimitNotificationItemDAL : NotificationItemDAL
  {
    public static readonly Guid DatabaseLimitNotificationItemId = new Guid("71475071-459F-4844-B689-6F210B0D416F");
    public static readonly Guid warningNotificationTypeGuid = new Guid("{7E8C21EF-61B1-4B7C-9122-B9A7E807B272}");
    public static readonly Guid reachedNotificationTypeGuid = new Guid("{7EA47379-CB96-48A3-89C4-84C18559351B}");
    private static readonly string linkWrapper = "&nbsp;&nbsp;<span style='font-weight:normal'>&#0187;&nbsp;<a href='{1}'>{0}</a></span>";
    private string WarningTitle = string.Format(Resources.get_COREBUSINESSLAYERDAL_CODE_1(), (object) 80.0) + string.Format(DatabaseLimitNotificationItemDAL.linkWrapper, (object) Resources.get_COREBUSINESSLAYERDAL_CODE_3(), (object) string.Format("https://www.solarwinds.com/documentation/kbloader.aspx?lang={0}&kb=3545", (object) Resources.get_CurrentHelpLanguage()));
    private string LimitReachedTitle = string.Format(Resources.get_COREBUSINESSLAYERDAL_CODE_2(), (object) 90.0) + string.Format(DatabaseLimitNotificationItemDAL.linkWrapper, (object) Resources.get_COREBUSINESSLAYERDAL_CODE_3(), (object) string.Format("https://www.solarwinds.com/documentation/kbloader.aspx?lang={0}&kb=3545", (object) Resources.get_CurrentHelpLanguage()));
    public const double CriticalPercentLimit = 90.0;
    public const double WarningPercentLimit = 80.0;
    private readonly IDatabaseInfoDAL databaseInfoDAL;

    public static DatabaseLimitNotificationItemDAL GetItem()
    {
      return NotificationItemDAL.GetItemById<DatabaseLimitNotificationItemDAL>(DatabaseLimitNotificationItemDAL.DatabaseLimitNotificationItemId);
    }

    public void Show(double databaseSize, double percent)
    {
      string description = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}|{1}", (object) databaseSize, (object) percent);
      bool isWarning = percent < 90.0;
      Guid typeId = isWarning ? DatabaseLimitNotificationItemDAL.warningNotificationTypeGuid : DatabaseLimitNotificationItemDAL.reachedNotificationTypeGuid;
      DatabaseLimitNotificationItemDAL notificationItemDal = DatabaseLimitNotificationItemDAL.GetItem();
      if (notificationItemDal == null)
      {
        NotificationItemDAL.Insert(DatabaseLimitNotificationItemDAL.DatabaseLimitNotificationItemId, typeId, this.GetNotificationMessage(isWarning), description, false, (string) null, new DateTime?(), (string) null);
      }
      else
      {
        bool flag = double.Parse(notificationItemDal.Description.Split('|')[1], (IFormatProvider) CultureInfo.InvariantCulture) < 90.0;
        if (flag == isWarning)
          return;
        if (flag != isWarning)
          notificationItemDal.SetNotAcknowledged();
        notificationItemDal.TypeId = typeId;
        notificationItemDal.Title = this.GetNotificationMessage(isWarning);
        notificationItemDal.Description = description;
        notificationItemDal.Update();
      }
    }

    public void Hide()
    {
      NotificationItemDAL.Delete(DatabaseLimitNotificationItemDAL.DatabaseLimitNotificationItemId);
    }

    private string GetNotificationMessage(bool isWarning)
    {
      return !isWarning ? this.LimitReachedTitle : this.WarningTitle;
    }

    public DatabaseLimitNotificationItemDAL()
      : this((IDatabaseInfoDAL) new DatabaseInfoDAL())
    {
    }

    public DatabaseLimitNotificationItemDAL(IDatabaseInfoDAL databaseInfoDAL)
    {
      if (databaseInfoDAL == null)
        throw new ArgumentNullException(nameof (databaseInfoDAL));
      this.databaseInfoDAL = databaseInfoDAL;
    }

    public bool CheckNotificationState()
    {
      double limitInMegabytes = this.databaseInfoDAL.GetDatabaseLimitInMegabytes();
      if (limitInMegabytes < 0.0)
      {
        this.Hide();
        return false;
      }
      double databaseSizeInMegaBytes = this.databaseInfoDAL.GetDatabaseSizeInMegaBytes();
      double percent = databaseSizeInMegaBytes / limitInMegabytes * 100.0;
      NotificationItemDAL.log.DebugFormat("Database limit is {0} MB. Database size is {1} MB", (object) limitInMegabytes, (object) databaseSizeInMegaBytes);
      if (percent >= 80.0)
        this.Show(databaseSizeInMegaBytes, percent);
      else
        this.Hide();
      return true;
    }
  }
}
