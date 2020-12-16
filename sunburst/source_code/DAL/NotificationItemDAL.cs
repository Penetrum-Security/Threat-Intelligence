// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.NotificationItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class NotificationItemDAL
  {
    protected static readonly Log log = new Log();

    public Guid Id { get; private set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; protected set; }

    public bool Ignored { get; set; }

    public Guid TypeId { get; protected set; }

    public string Url { get; set; }

    public DateTime? AcknowledgedAt { get; private set; }

    public string AcknowledgedBy { get; private set; }

    public bool IsAcknowledged
    {
      get
      {
        return this.AcknowledgedAt.HasValue;
      }
    }

    public bool IsVisible
    {
      get
      {
        return !this.IsAcknowledged && !this.Ignored;
      }
    }

    public void SetAcknowledged(DateTime at, string by)
    {
      if (at == DateTime.MinValue)
        throw new ArgumentNullException(nameof (at));
      if (string.IsNullOrEmpty(by))
        throw new ArgumentNullException(nameof (by));
      this.AcknowledgedAt = new DateTime?(at);
      this.AcknowledgedBy = by;
    }

    public void SetNotAcknowledged()
    {
      this.AcknowledgedAt = new DateTime?();
      this.AcknowledgedBy = (string) null;
    }

    public NotificationItemDAL()
    {
      this.Id = Guid.Empty;
      this.Title = string.Empty;
      this.Description = string.Empty;
      this.CreatedAt = DateTime.MinValue;
      this.Ignored = false;
      this.TypeId = Guid.Empty;
      this.Url = (string) null;
      this.AcknowledgedAt = new DateTime?();
      this.AcknowledgedBy = (string) null;
    }

    protected virtual Guid GetNotificationItemTypeId()
    {
      return Guid.Empty;
    }

    public static TNotificationItem GetItemById<TNotificationItem>(Guid itemId) where TNotificationItem : NotificationItemDAL, new()
    {
      try
      {
        TNotificationItem notificationItem = new TNotificationItem();
        notificationItem.Id = itemId;
        notificationItem.LoadFromDB();
        return notificationItem;
      }
      catch (ResultCountException ex)
      {
        NotificationItemDAL.log.DebugFormat("Can't find notification item in database: ID={0}, Type={1}", (object) itemId, (object) typeof (TNotificationItem).Name);
        return default (TNotificationItem);
      }
    }

    public static ICollection<TNotificationItem> GetItems<TNotificationItem>(
      NotificationItemFilter filter)
      where TNotificationItem : NotificationItemDAL, new()
    {
      try
      {
        return NotificationItemDAL.LoadCollectionFromDB<TNotificationItem>(filter);
      }
      catch (ResultCountException ex)
      {
        NotificationItemDAL.log.DebugFormat("Can't get notification item collection from database: Type={0}", (object) typeof (TNotificationItem).Name);
        return (ICollection<TNotificationItem>) null;
      }
    }

    public static ICollection<NotificationItemDAL> GetItemsByTypeId(
      Guid typeId,
      NotificationItemFilter filter)
    {
      if (filter == null)
        throw new ArgumentNullException(nameof (filter));
      if (typeId == Guid.Empty)
        throw new ArgumentException("Value can't be empty GUID", nameof (typeId));
      try
      {
        using (SqlCommand sqlCommand = new NotificationItemDAL().ComposeSelectCollectionCommand(typeId, filter))
        {
          using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
          {
            List<NotificationItemDAL> notificationItemDalList = new List<NotificationItemDAL>();
            while (rd.Read())
            {
              NotificationItemDAL notificationItemDal = new NotificationItemDAL();
              notificationItemDal.LoadFromReader(rd);
              notificationItemDalList.Add(notificationItemDal);
            }
            return (ICollection<NotificationItemDAL>) notificationItemDalList;
          }
        }
      }
      catch (ResultCountException ex)
      {
        NotificationItemDAL.log.DebugFormat("Can't get notification item collection from database: TypeID={0}", (object) typeId);
        return (ICollection<NotificationItemDAL>) null;
      }
    }

    public static TNotificationItem GetLatestItem<TNotificationItem>(NotificationItemFilter filter) where TNotificationItem : NotificationItemDAL, new()
    {
      TNotificationItem notificationItem = new TNotificationItem();
      using (SqlCommand sqlCommand = notificationItem.ComposeSelectLatestItemCommand(filter))
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          if (!rd.Read())
            return default (TNotificationItem);
          notificationItem.LoadFromReader(rd);
          return notificationItem;
        }
      }
    }

    public static NotificationItemDAL GetLatestItemByType(
      Guid typeId,
      NotificationItemFilter filter)
    {
      NotificationItemDAL notificationItemDal = new NotificationItemDAL();
      using (SqlCommand sqlCommand = notificationItemDal.ComposeSelectLatestItemCommand(typeId, filter))
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          if (!rd.Read())
            return (NotificationItemDAL) null;
          notificationItemDal.LoadFromReader(rd);
          return notificationItemDal;
        }
      }
    }

    public static void GetLatestItemsWithCount(
      NotificationItemFilter filter,
      Action<NotificationItemDAL, int> readerDelegate)
    {
      using (SqlCommand sqlCommand = new NotificationItemDAL().ComposeSelectLatestItemsWithCountCommand(filter))
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          while (rd.Read())
          {
            NotificationItemDAL notificationItemDal = new NotificationItemDAL();
            notificationItemDal.LoadFromReader(rd);
            readerDelegate(notificationItemDal, DatabaseFunctions.GetInt32(rd, "NotificationCount"));
          }
        }
      }
    }

    protected virtual SqlCommand ComposeSelectCollectionCommand(
      NotificationItemFilter filter)
    {
      return this.ComposeSelectCollectionCommand(this.GetNotificationItemTypeId(), filter);
    }

    private SqlCommand ComposeSelectCollectionCommand(
      Guid typeId,
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationItems");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (typeId != Guid.Empty)
        {
          SqlHelper.AddCondition(stringBuilder, "NotificationTypeID=@NotificationTypeID", "AND");
          sqlCommand.Parameters.AddWithValue("@NotificationTypeID", (object) typeId);
        }
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY CreatedAt DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0} collection: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected virtual SqlCommand ComposeSelectItemCommand()
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationItems WHERE NotificationID=@NotificationID");
      try
      {
        sqlCommand.Parameters.AddWithValue("@NotificationID", (object) this.Id);
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected virtual SqlCommand ComposeSelectLatestItemCommand(
      NotificationItemFilter filter)
    {
      return this.ComposeSelectLatestItemCommand(this.GetNotificationItemTypeId(), filter);
    }

    protected virtual SqlCommand ComposeSelectLatestItemCommand(
      Guid typeId,
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM NotificationItems");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (typeId != Guid.Empty)
        {
          SqlHelper.AddCondition(stringBuilder, "NotificationTypeID=@NotificationTypeID", "AND");
          sqlCommand.Parameters.AddWithValue("@NotificationTypeID", (object) typeId);
        }
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY CreatedAt DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for latest {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected virtual SqlCommand ComposeSelectLatestItemsWithCountCommand(
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT (SELECT COUNT(NotificationID) FROM NotificationItems {0}) as NotificationCount, i1.*\r\n     FROM NotificationItems i1 LEFT OUTER JOIN \r\n     NotificationItems i2 ON (i1.NotificationTypeID = i2.NotificationTypeID AND i1.CreatedAt < i2.CreatedAt)");
      try
      {
        StringBuilder stringBuilder1 = new StringBuilder();
        SqlHelper.AddCondition(stringBuilder1, "NotificationTypeID = i1.NotificationTypeID", "AND");
        StringBuilder stringBuilder2 = new StringBuilder();
        SqlHelper.AddCondition(stringBuilder2, "i2.NotificationID IS NULL", "AND");
        if (!filter.IncludeAcknowledged)
        {
          SqlHelper.AddCondition(stringBuilder1, "AcknowledgedAt IS NULL", "AND");
          SqlHelper.AddCondition(stringBuilder2, "i1.AcknowledgedAt IS NULL", "AND");
        }
        if (!filter.IncludeIgnored)
        {
          SqlHelper.AddCondition(stringBuilder1, "Ignored=0", "AND");
          SqlHelper.AddCondition(stringBuilder2, "i1.Ignored=0", "AND");
        }
        sqlCommand.CommandText = string.Format((IFormatProvider) CultureInfo.InvariantCulture, sqlCommand.CommandText, (object) stringBuilder1);
        sqlCommand.CommandText += stringBuilder2.ToString();
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        NotificationItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for latest {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected virtual SqlCommand ComposeSelectCountCommand(NotificationItemFilter filter)
    {
      return this.ComposeSelectCountCommand(this.GetNotificationItemTypeId(), filter);
    }

    private SqlCommand ComposeSelectCountCommand(
      Guid typeId,
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(NotificationID) FROM NotificationItems");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (typeId != Guid.Empty)
        {
          SqlHelper.AddCondition(stringBuilder, "NotificationTypeID=@NotificationTypeID", "AND");
          sqlCommand.Parameters.AddWithValue("@NotificationTypeID", (object) typeId);
        }
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
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

    protected virtual void LoadFromReader(IDataReader rd)
    {
      if (rd == null)
        throw new ArgumentNullException(nameof (rd));
      this.Id = DatabaseFunctions.GetGuid(rd, "NotificationID");
      this.Title = DatabaseFunctions.GetString(rd, "Title");
      this.Description = DatabaseFunctions.GetString(rd, "Description");
      this.CreatedAt = DatabaseFunctions.GetDateTime(rd, "CreatedAt");
      this.Ignored = DatabaseFunctions.GetBoolean(rd, "Ignored");
      this.TypeId = DatabaseFunctions.GetGuid(rd, "NotificationTypeID");
      this.Url = DatabaseFunctions.GetString(rd, "Url");
      DateTime dateTime = DatabaseFunctions.GetDateTime(rd, "AcknowledgedAt");
      this.AcknowledgedAt = !(dateTime == DateTime.MinValue) ? new DateTime?(dateTime) : new DateTime?();
      this.AcknowledgedBy = DatabaseFunctions.GetString(rd, "AcknowledgedBy");
    }

    protected void LoadFromDB()
    {
      using (SqlCommand sqlCommand = this.ComposeSelectItemCommand())
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          if (!rd.Read())
            throw new ResultCountException(1, 0);
          this.LoadFromReader(rd);
        }
      }
    }

    private static ICollection<TNotificationItem> LoadCollectionFromDB<TNotificationItem>(
      NotificationItemFilter filter)
      where TNotificationItem : NotificationItemDAL, new()
    {
      if (filter == null)
        throw new ArgumentNullException(nameof (filter));
      using (SqlCommand sqlCommand = new TNotificationItem().ComposeSelectCollectionCommand(filter))
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(sqlCommand))
        {
          List<TNotificationItem> notificationItemList = new List<TNotificationItem>();
          while (rd.Read())
          {
            TNotificationItem notificationItem = new TNotificationItem();
            notificationItem.LoadFromReader(rd);
            notificationItemList.Add(notificationItem);
          }
          return (ICollection<TNotificationItem>) notificationItemList;
        }
      }
    }

    protected virtual bool Update(SqlConnection con, SqlTransaction tr)
    {
      if (con == null)
        throw new ArgumentNullException(nameof (con));
      using (SqlCommand sqlCommand = new SqlCommand("UPDATE NotificationItems SET Title=@Title, Description=@Description, CreatedAt=@CreatedAt, NotificationTypeID=@NotificationTypeID, \r\n                                               Url=@Url, Ignored=@Ignored, AcknowledgedAt=@AcknowledgedAt, AcknowledgedBy=@AcknowledgedBy\r\n                                               WHERE NotificationID=@NotificationID"))
      {
        sqlCommand.Parameters.AddWithValue("@NotificationID", (object) this.Id);
        sqlCommand.Parameters.AddWithValue("@Title", (object) this.Title);
        sqlCommand.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(this.Description) ? (object) DBNull.Value : (object) this.Description);
        sqlCommand.Parameters.AddWithValue("@CreatedAt", (object) this.CreatedAt);
        sqlCommand.Parameters.AddWithValue("@Ignored", (object) this.Ignored);
        sqlCommand.Parameters.AddWithValue("@NotificationTypeID", (object) this.TypeId);
        sqlCommand.Parameters.AddWithValue("@Url", string.IsNullOrEmpty(this.Url) ? (object) DBNull.Value : (object) this.Url);
        SqlParameterCollection parameters = sqlCommand.Parameters;
        DateTime? acknowledgedAt = this.AcknowledgedAt;
        object obj;
        if (!acknowledgedAt.HasValue)
        {
          obj = (object) DBNull.Value;
        }
        else
        {
          acknowledgedAt = this.AcknowledgedAt;
          obj = (object) acknowledgedAt.Value;
        }
        parameters.AddWithValue("@AcknowledgedAt", obj);
        sqlCommand.Parameters.AddWithValue("@AcknowledgedBy", string.IsNullOrEmpty(this.AcknowledgedBy) ? (object) DBNull.Value : (object) this.AcknowledgedBy);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    public bool Update()
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            int num = this.Update(connection, tr) ? 1 : 0;
            tr.Commit();
            return num != 0;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't UPDATE item of type {0}", (object) this.GetType().Name) + ex.ToString()));
            throw;
          }
        }
      }
    }

    public static void Update(
      Guid notificationId,
      Guid typeId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime createdAt,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
    {
      new NotificationItemDAL()
      {
        Id = notificationId,
        TypeId = typeId,
        Title = title,
        Description = description,
        Ignored = ignored,
        Url = url,
        CreatedAt = createdAt,
        AcknowledgedAt = acknowledgedAt,
        AcknowledgedBy = acknowledgedBy
      }.Update();
    }

    private static TNotificationItem Insert<TNotificationItem>(
      SqlConnection con,
      SqlTransaction tr,
      Guid notificationId,
      Guid typeId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
      where TNotificationItem : NotificationItemDAL, new()
    {
      TNotificationItem notificationItem = new TNotificationItem();
      if (con == null)
        throw new ArgumentNullException(nameof (con));
      if (notificationId == Guid.Empty)
        throw new ArgumentException("notificationId GUID can't be Guid.Empty", nameof (notificationId));
      if (string.IsNullOrEmpty(title))
        throw new ArgumentNullException(nameof (title));
      if (typeId == Guid.Empty)
      {
        typeId = notificationItem.GetNotificationItemTypeId();
        if (typeId == Guid.Empty)
          throw new ArgumentException("Can't obtain Type GUID", nameof (TNotificationItem));
      }
      DateTime utcNow = DateTime.UtcNow;
      using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO NotificationItems (NotificationID, Title, Description, CreatedAt, Ignored, NotificationTypeID, Url, AcknowledgedAt, AcknowledgedBy)\r\n                                                VALUES (@NotificationID, @Title, @Description, @CreatedAt, @Ignored, @NotificationTypeID, @Url, @AcknowledgedAt, @AcknowledgedBy)"))
      {
        sqlCommand.Parameters.AddWithValue("@NotificationID", (object) notificationId);
        sqlCommand.Parameters.AddWithValue("@Title", (object) title);
        sqlCommand.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object) DBNull.Value : (object) description);
        sqlCommand.Parameters.AddWithValue("@CreatedAt", (object) utcNow);
        sqlCommand.Parameters.AddWithValue("@Ignored", (object) ignored);
        sqlCommand.Parameters.AddWithValue("@NotificationTypeID", (object) typeId);
        sqlCommand.Parameters.AddWithValue("@Url", string.IsNullOrEmpty(url) ? (object) DBNull.Value : (object) url);
        sqlCommand.Parameters.AddWithValue("@AcknowledgedAt", acknowledgedAt.HasValue ? (object) acknowledgedAt.Value : (object) DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@AcknowledgedBy", string.IsNullOrEmpty(acknowledgedBy) ? (object) DBNull.Value : (object) acknowledgedBy);
        if (SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) == 0)
          return default (TNotificationItem);
        notificationItem.Id = notificationId;
        notificationItem.Title = title;
        notificationItem.Description = description;
        notificationItem.CreatedAt = utcNow;
        notificationItem.Ignored = ignored;
        notificationItem.TypeId = notificationItem.GetNotificationItemTypeId();
        notificationItem.Url = url;
        notificationItem.AcknowledgedAt = acknowledgedAt;
        notificationItem.AcknowledgedBy = acknowledgedBy;
        return notificationItem;
      }
    }

    protected static TNotificationItem Insert<TNotificationItem>(
      SqlConnection con,
      SqlTransaction tr,
      Guid notificationId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
      where TNotificationItem : NotificationItemDAL, new()
    {
      return NotificationItemDAL.Insert<TNotificationItem>(con, tr, notificationId, Guid.Empty, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
    }

    protected static TNotificationItem Insert<TNotificationItem>(
      Guid notificationId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
      where TNotificationItem : NotificationItemDAL, new()
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            TNotificationItem notificationItem = NotificationItemDAL.Insert<TNotificationItem>(connection, tr, notificationId, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
            tr.Commit();
            return notificationItem;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't INSERT item of type {0}", (object) typeof (TNotificationItem).Name) + ex.ToString()));
            throw;
          }
        }
      }
    }

    public static NotificationItemDAL Insert(
      Guid notificationId,
      Guid typeId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy)
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            NotificationItemDAL notificationItemDal = NotificationItemDAL.Insert<NotificationItemDAL>(connection, tr, notificationId, typeId, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
            tr.Commit();
            return notificationItemDal;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't INSERT item with ID {0}, typeId {1} ", (object) notificationId, (object) typeId) + ex.ToString()));
            throw;
          }
        }
      }
    }

    private static bool Delete(SqlConnection con, SqlTransaction tr, Guid notificationId)
    {
      if (con == null)
        throw new ArgumentNullException(nameof (con));
      if (notificationId == Guid.Empty)
        throw new ArgumentException("notificationId GUID can't be Guid.Empty", nameof (notificationId));
      using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM NotificationItems WHERE NotificationID=@NotificationID"))
      {
        sqlCommand.Parameters.AddWithValue("@NotificationID", (object) notificationId);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    protected virtual bool Delete(SqlConnection con, SqlTransaction tr)
    {
      return NotificationItemDAL.Delete(con, tr, this.Id);
    }

    public bool Delete()
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            int num = this.Delete(connection, tr) ? 1 : 0;
            tr.Commit();
            return num != 0;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't DELETE item of type {0}", (object) this.GetType().Name) + ex.ToString()));
            throw;
          }
        }
      }
    }

    public static bool Delete(Guid notificationId)
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            int num = NotificationItemDAL.Delete(connection, tr, notificationId) ? 1 : 0;
            tr.Commit();
            return num != 0;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            NotificationItemDAL.log.Error((object) (string.Format("Can't DELETE item with ID {0}", (object) notificationId) + ex.ToString()));
            throw;
          }
        }
      }
    }

    public static int GetNotificationsCount<TNotificationItem>(NotificationItemFilter filter) where TNotificationItem : NotificationItemDAL, new()
    {
      using (SqlCommand sqlCommand = new TNotificationItem().ComposeSelectCountCommand(filter))
      {
        object obj = SqlHelper.ExecuteScalar(sqlCommand);
        return obj == null || obj == DBNull.Value ? 0 : (int) obj;
      }
    }

    public static int GetNotificationsCountByType(Guid typeId, NotificationItemFilter filter)
    {
      using (SqlCommand sqlCommand = new NotificationItemDAL().ComposeSelectCountCommand(typeId, filter))
      {
        object obj = SqlHelper.ExecuteScalar(sqlCommand);
        return obj == null || obj == DBNull.Value ? 0 : (int) obj;
      }
    }

    public static Dictionary<Guid, int> GetNotificationsCounts()
    {
      Dictionary<Guid, int> dictionary = new Dictionary<Guid, int>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT NotificationTypeID, COUNT(NotificationID) as TheCount FROM NotificationItems WHERE AcknowledgedAt IS NULL AND Ignored=0 GROUP BY NotificationTypeID"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
          {
            Guid guid = DatabaseFunctions.GetGuid(dataReader, 0);
            int int32 = DatabaseFunctions.GetInt32(dataReader, 1);
            dictionary[guid] = int32;
          }
        }
      }
      return dictionary;
    }

    private static bool AcknowledgeItems(
      Guid? notificationId,
      Guid? typeId,
      string accountId,
      DateTime acknowledgedAt,
      DateTime createdBefore)
    {
      if (string.IsNullOrEmpty(accountId))
        throw new ArgumentNullException("accountID");
      if (acknowledgedAt == DateTime.MinValue)
        throw new ArgumentException("Value has to be specified", nameof (acknowledgedAt));
      if (createdBefore == DateTime.MinValue)
        throw new ArgumentException("Value has to be specified", nameof (createdBefore));
      StringBuilder stringBuilder = new StringBuilder("UPDATE NotificationItems SET AcknowledgedBy=@AccountID, AcknowledgedAt=@AcknowledgedAt \r\n                                                WHERE AcknowledgedAt IS NULL AND CreatedAt <= @CreatedBefore");
      using (SqlCommand sqlCommand = new SqlCommand())
      {
        sqlCommand.Parameters.AddWithValue("@AccountID", (object) accountId);
        sqlCommand.Parameters.AddWithValue("@AcknowledgedAt", (object) acknowledgedAt);
        sqlCommand.Parameters.AddWithValue("@CreatedBefore", (object) createdBefore);
        if (notificationId.HasValue)
        {
          stringBuilder.Append(" AND NotificationID=@NotificationID");
          sqlCommand.Parameters.AddWithValue("@NotificationID", (object) notificationId.Value);
        }
        if (typeId.HasValue)
        {
          stringBuilder.Append(" AND NotificationTypeID=@TypeID");
          sqlCommand.Parameters.AddWithValue("@TypeID", (object) typeId.Value);
        }
        sqlCommand.CommandText = stringBuilder.ToString();
        return SqlHelper.ExecuteNonQuery(sqlCommand) > 0;
      }
    }

    public static bool AcknowledgeItemsByType(
      Guid typeId,
      string accountId,
      DateTime createdBefore)
    {
      return NotificationItemDAL.AcknowledgeItems(new Guid?(), new Guid?(typeId), accountId, DateTime.UtcNow, createdBefore);
    }

    public static bool AcknowledgeItem(
      Guid notificationId,
      string accountId,
      DateTime acknowledgedAt,
      DateTime createdBefore)
    {
      if (notificationId == Guid.Empty)
        throw new ArgumentException("notificationId GUID can't be Guid.Empty", nameof (notificationId));
      return NotificationItemDAL.AcknowledgeItems(new Guid?(notificationId), new Guid?(), accountId, acknowledgedAt, createdBefore);
    }

    public static bool AcknowledgeItem(
      Guid notificationId,
      string accountId,
      DateTime createdBefore)
    {
      if (notificationId == Guid.Empty)
        throw new ArgumentException("notificationId GUID can't be Guid.Empty", nameof (notificationId));
      return NotificationItemDAL.AcknowledgeItems(new Guid?(notificationId), new Guid?(), accountId, DateTime.UtcNow, createdBefore);
    }

    public static bool AcknowledgeItems<TNotificationItem>(
      string accountId,
      DateTime acknowledgedAt,
      DateTime createdBefore)
      where TNotificationItem : NotificationItemDAL, new()
    {
      Guid notificationItemTypeId = new TNotificationItem().GetNotificationItemTypeId();
      if (notificationItemTypeId == Guid.Empty)
        throw new ArgumentException("Can't obtain Type GUID", nameof (TNotificationItem));
      return NotificationItemDAL.AcknowledgeItems(new Guid?(), new Guid?(notificationItemTypeId), accountId, acknowledgedAt, createdBefore);
    }

    public static bool AcknowledgeItems<TNotificationItem>(string accountId, DateTime createdBefore) where TNotificationItem : NotificationItemDAL, new()
    {
      return NotificationItemDAL.AcknowledgeItems<TNotificationItem>(accountId, DateTime.UtcNow, createdBefore);
    }

    public static bool AcknowledgeAllItems(
      string accountId,
      DateTime acknowledgedAt,
      DateTime createdBefore)
    {
      return NotificationItemDAL.AcknowledgeItems(new Guid?(), new Guid?(), accountId, acknowledgedAt, createdBefore);
    }

    public static bool AcknowledgeAllItems(string accountId, DateTime createdBefore)
    {
      return NotificationItemDAL.AcknowledgeItems(new Guid?(), new Guid?(), accountId, DateTime.UtcNow, createdBefore);
    }

    public static bool IgnoreItem(Guid notificationId)
    {
      if (notificationId == Guid.Empty)
        throw new ArgumentException("notificationId GUID can't be Guid.Empty", nameof (notificationId));
      using (SqlCommand sqlCommand = new SqlCommand("UPDATE NotificationItems SET Ignored=1 WHERE NotificationID=@NotificationID"))
      {
        sqlCommand.Parameters.AddWithValue("@NotificationID", (object) notificationId);
        return SqlHelper.ExecuteNonQuery(sqlCommand) > 0;
      }
    }

    public static bool IgnoreItems(ICollection<Guid> notificationIds)
    {
      bool flag = true;
      if (notificationIds != null && notificationIds.Count > 0)
      {
        foreach (Guid notificationId in (IEnumerable<Guid>) notificationIds)
        {
          if (!NotificationItemDAL.IgnoreItem(notificationId))
            flag = false;
        }
      }
      return flag;
    }
  }
}
