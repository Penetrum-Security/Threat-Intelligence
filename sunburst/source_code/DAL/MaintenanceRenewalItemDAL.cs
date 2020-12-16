// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MaintenanceRenewalItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class MaintenanceRenewalItemDAL : NotificationItemDAL
  {
    public string ProductTag { get; set; }

    public DateTime DateReleased { get; set; }

    public string NewVersion { get; set; }

    public MaintenanceRenewalItemDAL()
    {
      this.ProductTag = string.Empty;
      this.DateReleased = DateTime.MinValue;
      this.NewVersion = string.Empty;
    }

    protected override Guid GetNotificationItemTypeId()
    {
      return (Guid) MaintenanceRenewalItem.MaintenanceRenewalsTypeGuid;
    }

    protected override SqlCommand ComposeSelectCollectionCommand(
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationMaintenanceRenewals LEFT JOIN NotificationItems ON \r\n                                         NotificationMaintenanceRenewals.RenewalID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        if (filter is MaintenanceRenewalFilter maintenanceRenewalFilter && !string.IsNullOrEmpty(maintenanceRenewalFilter.ProductTag))
        {
          SqlHelper.AddCondition(stringBuilder, "ProductTag=@ProductTag", "AND");
          sqlCommand.Parameters.AddWithValue("@ProductTag", (object) maintenanceRenewalFilter.ProductTag);
        }
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY DateReleased DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0} collection: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectItemCommand()
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationMaintenanceRenewals LEFT JOIN NotificationItems ON \r\n                                         NotificationMaintenanceRenewals.RenewalID = NotificationItems.NotificationID \r\n                                       WHERE RenewalID=@RenewalID");
      try
      {
        sqlCommand.Parameters.AddWithValue("@RenewalID", (object) this.Id);
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectLatestItemCommand(
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM NotificationMaintenanceRenewals LEFT JOIN NotificationItems ON \r\n                                         NotificationMaintenanceRenewals.RenewalID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        if (filter is MaintenanceRenewalFilter maintenanceRenewalFilter && !string.IsNullOrEmpty(maintenanceRenewalFilter.ProductTag))
        {
          SqlHelper.AddCondition(stringBuilder, "ProductTag=@ProductTag", "AND");
          sqlCommand.Parameters.AddWithValue("@ProductTag", (object) maintenanceRenewalFilter.ProductTag);
        }
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY DateReleased DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for latest {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectCountCommand(NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(RenewalID) FROM NotificationMaintenanceRenewals LEFT JOIN NotificationItems ON \r\n                                         NotificationMaintenanceRenewals.RenewalID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        if (filter is MaintenanceRenewalFilter maintenanceRenewalFilter && !string.IsNullOrEmpty(maintenanceRenewalFilter.ProductTag))
        {
          SqlHelper.AddCondition(stringBuilder, "ProductTag=@ProductTag", "AND");
          sqlCommand.Parameters.AddWithValue("@ProductTag", (object) maintenanceRenewalFilter.ProductTag);
        }
        sqlCommand.CommandText += stringBuilder.ToString();
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT COUNT SQL command for {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    public static MaintenanceRenewalItemDAL GetItemById(Guid itemId)
    {
      return NotificationItemDAL.GetItemById<MaintenanceRenewalItemDAL>(itemId);
    }

    public static MaintenanceRenewalItemDAL GetLatestItem(
      NotificationItemFilter filter)
    {
      return NotificationItemDAL.GetLatestItem<MaintenanceRenewalItemDAL>(filter);
    }

    public static ICollection<MaintenanceRenewalItemDAL> GetItems(
      MaintenanceRenewalFilter filter)
    {
      return NotificationItemDAL.GetItems<MaintenanceRenewalItemDAL>((NotificationItemFilter) filter);
    }

    public static int GetNotificationsCount()
    {
      return NotificationItemDAL.GetNotificationsCount<MaintenanceRenewalItemDAL>(new NotificationItemFilter());
    }

    public static MaintenanceRenewalItemDAL GetItemForProduct(
      string productTag)
    {
      if (string.IsNullOrEmpty(productTag))
        throw new ArgumentNullException(nameof (productTag));
      using (SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationMaintenanceRenewals LEFT JOIN NotificationItems ON \r\n                                                NotificationMaintenanceRenewals.RenewalID = NotificationItems.NotificationID\r\n                                              WHERE ProductTag=@ProductTag"))
      {
        sqlCommand.Parameters.AddWithValue("@ProductTag", (object) productTag);
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          if (!rd.Read())
            return (MaintenanceRenewalItemDAL) null;
          MaintenanceRenewalItemDAL maintenanceRenewalItemDal = new MaintenanceRenewalItemDAL();
          maintenanceRenewalItemDal.LoadFromReader(rd);
          return maintenanceRenewalItemDal;
        }
      }
    }

    private static MaintenanceRenewalItemDAL Insert(
      SqlConnection con,
      SqlTransaction tr,
      Guid renewalId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy,
      string productTag,
      DateTime dateReleased,
      string newVersion)
    {
      if (tr == null)
        throw new ArgumentNullException(nameof (tr));
      if (string.IsNullOrEmpty(productTag))
        throw new ArgumentNullException(nameof (productTag));
      if (dateReleased == DateTime.MinValue)
        throw new ArgumentNullException(nameof (dateReleased));
      if (string.IsNullOrEmpty(newVersion))
        throw new ArgumentNullException(nameof (newVersion));
      MaintenanceRenewalItemDAL maintenanceRenewalItemDal = NotificationItemDAL.Insert<MaintenanceRenewalItemDAL>(con, tr, renewalId, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
      if (maintenanceRenewalItemDal == null)
        return (MaintenanceRenewalItemDAL) null;
      using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO NotificationMaintenanceRenewals (RenewalID, ProductTag, DateReleased, Version) VALUES (@RenewalID, @ProductTag, @DateReleased, @NewVersion)"))
      {
        sqlCommand.Parameters.AddWithValue("@RenewalID", (object) renewalId);
        sqlCommand.Parameters.AddWithValue("@ProductTag", (object) productTag);
        sqlCommand.Parameters.AddWithValue("@DateReleased", (object) dateReleased);
        sqlCommand.Parameters.AddWithValue("@NewVersion", (object) newVersion);
        if (SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) == 0)
        {
          maintenanceRenewalItemDal = (MaintenanceRenewalItemDAL) null;
        }
        else
        {
          maintenanceRenewalItemDal.ProductTag = productTag;
          maintenanceRenewalItemDal.DateReleased = dateReleased;
          maintenanceRenewalItemDal.NewVersion = newVersion;
        }
      }
      return maintenanceRenewalItemDal;
    }

    public static MaintenanceRenewalItemDAL Insert(
      Guid renewalId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy,
      string productTag,
      DateTime dateReleased,
      string newVersion)
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            MaintenanceRenewalItemDAL maintenanceRenewalItemDal = MaintenanceRenewalItemDAL.Insert(connection, tr, renewalId, title, description, ignored, url, acknowledgedAt, acknowledgedBy, productTag, dateReleased, newVersion);
            tr.Commit();
            return maintenanceRenewalItemDal;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't INSERT maintenance renewal item: ") + ex.ToString()));
            throw;
          }
        }
      }
    }

    protected override bool Update(SqlConnection con, SqlTransaction tr)
    {
      if (!base.Update(con, tr))
        return false;
      using (SqlCommand sqlCommand = new SqlCommand("UPDATE NotificationMaintenanceRenewals SET ProductTag=@ProductTag, DateReleased=@DateReleased, Version=@NewVersion WHERE RenewalID=@RenewalID"))
      {
        sqlCommand.Parameters.AddWithValue("@RenewalID", (object) this.Id);
        sqlCommand.Parameters.AddWithValue("@ProductTag", (object) this.ProductTag);
        sqlCommand.Parameters.AddWithValue("@DateReleased", (object) this.DateReleased);
        sqlCommand.Parameters.AddWithValue("@NewVersion", (object) this.NewVersion);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    protected override bool Delete(SqlConnection con, SqlTransaction tr)
    {
      if (!base.Delete(con, tr))
        return false;
      using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM NotificationMaintenanceRenewals WHERE RenewalID=@RenewalID"))
      {
        sqlCommand.Parameters.AddWithValue("@RenewalID", (object) this.Id);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    protected override void LoadFromReader(IDataReader rd)
    {
      base.LoadFromReader(rd);
      this.ProductTag = DatabaseFunctions.GetString(rd, "ProductTag");
      this.DateReleased = DatabaseFunctions.GetDateTime(rd, "DateReleased");
      this.NewVersion = DatabaseFunctions.GetString(rd, "Version");
    }
  }
}
