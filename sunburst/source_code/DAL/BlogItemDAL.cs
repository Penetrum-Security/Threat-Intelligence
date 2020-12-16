// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.BlogItemDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class BlogItemDAL : NotificationItemDAL
  {
    private new static readonly Log log = new Log();

    public Guid PostGuid { get; set; }

    public long PostId { get; set; }

    public string Owner { get; set; }

    public DateTime PublicationDate { get; set; }

    public string CommentsUrl { get; set; }

    public int CommentsCount { get; set; }

    public BlogItemDAL()
    {
      this.PostGuid = Guid.Empty;
      this.PostId = 0L;
      this.Owner = string.Empty;
      this.PublicationDate = DateTime.MinValue;
      this.CommentsUrl = string.Empty;
      this.CommentsCount = 0;
    }

    protected override Guid GetNotificationItemTypeId()
    {
      return (Guid) BlogItem.BlogTypeGuid;
    }

    protected override SqlCommand ComposeSelectCollectionCommand(
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT{0} * FROM NotificationBlogs LEFT JOIN NotificationItems ON \r\n                                         NotificationBlogs.BlogID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        if (filter is BlogFilter blogFilter && blogFilter.MaxResults > 0)
          sqlCommand.CommandText = string.Format(sqlCommand.CommandText, (object) (" TOP " + (object) blogFilter.MaxResults));
        else
          sqlCommand.CommandText = string.Format(sqlCommand.CommandText, (object) string.Empty);
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY PublicationDate DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        BlogItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0} collection: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectItemCommand()
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT * FROM NotificationBlogs LEFT JOIN NotificationItems ON \r\n                                         NotificationBlogs.BlogID = NotificationItems.NotificationID \r\n                                       WHERE BlogID=@BlogID");
      try
      {
        sqlCommand.Parameters.AddWithValue("@BlogID", (object) this.Id);
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        BlogItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectLatestItemCommand(
      NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM NotificationBlogs LEFT JOIN NotificationItems ON \r\n                                         NotificationBlogs.BlogID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (!filter.IncludeAcknowledged)
          SqlHelper.AddCondition(stringBuilder, "AcknowledgedAt IS NULL", "AND");
        if (!filter.IncludeIgnored)
          SqlHelper.AddCondition(stringBuilder, "Ignored=0", "AND");
        sqlCommand.CommandText += stringBuilder.ToString();
        sqlCommand.CommandText += " ORDER BY PublicationDate DESC";
        return sqlCommand;
      }
      catch (Exception ex)
      {
        sqlCommand.Dispose();
        BlogItemDAL.log.Error((object) (string.Format("Error while composing SELECT SQL command for latest {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    protected override SqlCommand ComposeSelectCountCommand(NotificationItemFilter filter)
    {
      SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(BlogID) FROM NotificationBlogs LEFT JOIN NotificationItems ON \r\n                                         NotificationBlogs.BlogID = NotificationItems.NotificationID");
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
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
        BlogItemDAL.log.Error((object) (string.Format("Error while composing SELECT COUNT SQL command for {0}: ", (object) this.GetType().Name) + ex.ToString()));
        throw;
      }
    }

    public static BlogItemDAL GetItemById(Guid itemId)
    {
      return NotificationItemDAL.GetItemById<BlogItemDAL>(itemId);
    }

    public static BlogItemDAL GetLatestItem()
    {
      return NotificationItemDAL.GetLatestItem<BlogItemDAL>(new NotificationItemFilter(false, false));
    }

    public static ICollection<BlogItemDAL> GetItems(BlogFilter filter)
    {
      return NotificationItemDAL.GetItems<BlogItemDAL>((NotificationItemFilter) filter);
    }

    public static int GetNotificationsCount()
    {
      return NotificationItemDAL.GetNotificationsCount<BlogItemDAL>(new NotificationItemFilter());
    }

    public static BlogItemDAL GetBlogItemForPost(Guid postGuid, long postId)
    {
      return BlogItemDAL.GetBlogItemForPost(postGuid, postId, (SqlConnection) null);
    }

    private static BlogItemDAL GetBlogItemForPost(
      Guid postGuid,
      long postId,
      SqlConnection connection)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM NotificationBlogs LEFT JOIN NotificationItems ON NotificationBlogs.BlogID=NotificationItems.NotificationID \r\n                                 WHERE PostGUID=@PostGUID AND PostID=@PostID"))
      {
        textCommand.Parameters.AddWithValue("@PostGUID", (object) postGuid);
        textCommand.Parameters.AddWithValue("@PostID", (object) postId);
        using (IDataReader rd = SqlHelper.ExecuteReader(textCommand, connection))
        {
          if (!rd.Read())
            return (BlogItemDAL) null;
          BlogItemDAL blogItemDal = new BlogItemDAL();
          blogItemDal.LoadFromReader(rd);
          return blogItemDal;
        }
      }
    }

    public static void StoreBlogItems(List<BlogItemDAL> blogItems, int targetBlogsCount)
    {
      if (targetBlogsCount < 0)
        throw new ArgumentOutOfRangeException(nameof (targetBlogsCount), (object) targetBlogsCount, "Should be >= 0");
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        List<BlogItemDAL> blogItemDalList1 = new List<BlogItemDAL>();
        List<BlogItemDAL> blogItemDalList2 = new List<BlogItemDAL>();
        foreach (BlogItemDAL blogItem in blogItems)
        {
          BlogItemDAL blogItemForPost = BlogItemDAL.GetBlogItemForPost(blogItem.PostGuid, blogItem.PostId, connection);
          if (blogItemForPost != null)
          {
            blogItemForPost.Title = blogItem.Title;
            blogItemForPost.Description = blogItem.Description;
            blogItemForPost.Url = blogItem.Url;
            blogItemForPost.Owner = blogItem.Owner;
            blogItemForPost.PublicationDate = blogItem.PublicationDate;
            blogItemForPost.CommentsUrl = blogItem.CommentsUrl;
            blogItemForPost.CommentsCount = blogItem.CommentsCount;
            blogItemDalList1.Add(blogItemForPost);
          }
          else
            blogItemDalList2.Add(blogItem);
        }
        using (SqlTransaction tr = connection.BeginTransaction(IsolationLevel.Serializable))
        {
          try
          {
            foreach (NotificationItemDAL notificationItemDal in blogItemDalList1)
              notificationItemDal.Update(connection, tr);
            foreach (BlogItemDAL blogItemDal in blogItemDalList2)
              BlogItemDAL.Insert(connection, tr, Guid.NewGuid(), blogItemDal.Title, blogItemDal.Description, false, blogItemDal.Url, new DateTime?(), (string) null, blogItemDal.PostGuid, blogItemDal.PostId, blogItemDal.Owner, blogItemDal.PublicationDate, blogItemDal.CommentsUrl, blogItemDal.CommentsCount);
            using (SqlCommand sqlCommand = new SqlCommand(string.Format("DELETE FROM NotificationBlogs WHERE BlogID NOT IN (SELECT TOP {0} BlogID FROM NotificationBlogs ORDER BY PublicationDate DESC)", (object) targetBlogsCount)))
              SqlHelper.ExecuteNonQuery(sqlCommand, connection, tr);
            using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM NotificationItems WHERE NotificationTypeID=@TypeID AND NotificationID NOT IN (SELECT BlogID FROM NotificationBlogs)"))
            {
              sqlCommand.Parameters.AddWithValue("@TypeID", (object) (Guid) BlogItem.BlogTypeGuid);
              SqlHelper.ExecuteNonQuery(sqlCommand, connection, tr);
            }
            tr.Commit();
          }
          catch
          {
            tr.Rollback();
            throw;
          }
        }
      }
    }

    private static BlogItemDAL Insert(
      SqlConnection con,
      SqlTransaction tr,
      Guid blogId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy,
      Guid postGuid,
      long postId,
      string owner,
      DateTime publicationDate,
      string commentsUrl,
      int commentsCount)
    {
      if (tr == null)
        throw new ArgumentNullException(nameof (tr));
      if (postGuid == Guid.Empty)
        throw new ArgumentException("postGuid GUID can't be Guid.Empty", nameof (postGuid));
      if (publicationDate == DateTime.MinValue)
        throw new ArgumentNullException(nameof (publicationDate));
      BlogItemDAL blogItemDal = NotificationItemDAL.Insert<BlogItemDAL>(con, tr, blogId, title, description, ignored, url, acknowledgedAt, acknowledgedBy);
      if (blogItemDal == null)
        return (BlogItemDAL) null;
      using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO NotificationBlogs (BlogID, PostGUID, PostID, Owner, PublicationDate, CommentsUrl, CommentsCount)\r\n                                              VALUES (@BlogID, @PostGUID, @PostID, @Owner, @PublicationDate, @CommentsUrl, @CommentsCount)"))
      {
        sqlCommand.Parameters.AddWithValue("@BlogID", (object) blogId);
        sqlCommand.Parameters.AddWithValue("@PostGUID", (object) postGuid);
        sqlCommand.Parameters.AddWithValue("@PostID", (object) postId);
        sqlCommand.Parameters.AddWithValue("@Owner", string.IsNullOrEmpty(owner) ? (object) DBNull.Value : (object) owner);
        sqlCommand.Parameters.AddWithValue("@PublicationDate", (object) publicationDate);
        sqlCommand.Parameters.AddWithValue("@CommentsUrl", string.IsNullOrEmpty(commentsUrl) ? (object) DBNull.Value : (object) commentsUrl);
        sqlCommand.Parameters.AddWithValue("@CommentsCount", commentsCount < 0 ? (object) DBNull.Value : (object) commentsCount);
        if (SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) == 0)
          return (BlogItemDAL) null;
        blogItemDal.PostGuid = postGuid;
        blogItemDal.PostId = postId;
        blogItemDal.Owner = owner;
        blogItemDal.PublicationDate = publicationDate;
        blogItemDal.CommentsUrl = commentsUrl;
        blogItemDal.CommentsCount = commentsCount;
        return blogItemDal;
      }
    }

    public static BlogItemDAL Insert(
      Guid blogId,
      string title,
      string description,
      bool ignored,
      string url,
      DateTime? acknowledgedAt,
      string acknowledgedBy,
      Guid postGuid,
      long postId,
      string owner,
      DateTime publicationDate,
      string commentsUrl,
      int commentsCount)
    {
      using (SqlConnection connection = DatabaseFunctions.CreateConnection())
      {
        using (SqlTransaction tr = connection.BeginTransaction())
        {
          try
          {
            BlogItemDAL blogItemDal = BlogItemDAL.Insert(connection, tr, blogId, title, description, ignored, url, acknowledgedAt, acknowledgedBy, postGuid, postId, owner, publicationDate, commentsUrl, commentsCount);
            tr.Commit();
            return blogItemDal;
          }
          catch (Exception ex)
          {
            tr.Rollback();
            BlogItemDAL.log.Error((object) (string.Format("Can't INSERT blog item: ") + ex.ToString()));
            throw;
          }
        }
      }
    }

    protected override bool Update(SqlConnection con, SqlTransaction tr)
    {
      if (!base.Update(con, tr))
        return false;
      using (SqlCommand sqlCommand = new SqlCommand("UPDATE NotificationBlogs SET PostGUID=@PostGUID, PostID=@PostID, Owner=@Owner, PublicationDate=@PublicationDate, \r\n                                              CommentsUrl=@CommentsUrl, CommentsCount=@CommentsCount WHERE BlogID=@BlogID"))
      {
        sqlCommand.Parameters.AddWithValue("@BlogID", (object) this.Id);
        sqlCommand.Parameters.AddWithValue("@PostGUID", (object) this.PostGuid);
        sqlCommand.Parameters.AddWithValue("@PostID", (object) this.PostId);
        sqlCommand.Parameters.AddWithValue("@Owner", string.IsNullOrEmpty(this.Owner) ? (object) DBNull.Value : (object) this.Owner);
        sqlCommand.Parameters.AddWithValue("@PublicationDate", (object) this.PublicationDate);
        sqlCommand.Parameters.AddWithValue("@CommentsUrl", string.IsNullOrEmpty(this.CommentsUrl) ? (object) DBNull.Value : (object) this.CommentsUrl);
        sqlCommand.Parameters.AddWithValue("@CommentsCount", this.CommentsCount < 0 ? (object) DBNull.Value : (object) this.CommentsCount);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    protected override bool Delete(SqlConnection con, SqlTransaction tr)
    {
      if (!base.Delete(con, tr))
        return false;
      using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM NotificationBlogs WHERE BlogID=@BlogID"))
      {
        sqlCommand.Parameters.AddWithValue("@BlogID", (object) this.Id);
        return SqlHelper.ExecuteNonQuery(sqlCommand, con, tr) > 0;
      }
    }

    protected override void LoadFromReader(IDataReader rd)
    {
      base.LoadFromReader(rd);
      this.PostGuid = DatabaseFunctions.GetGuid(rd, "PostGUID");
      this.PostId = DatabaseFunctions.GetLong(rd, "PostID");
      this.Owner = DatabaseFunctions.GetString(rd, "Owner");
      this.PublicationDate = DatabaseFunctions.GetDateTime(rd, "PublicationDate");
      this.CommentsUrl = DatabaseFunctions.GetString(rd, "CommentsUrl");
      this.CommentsCount = DatabaseFunctions.GetInt32(rd, "CommentsCount");
    }
  }
}
