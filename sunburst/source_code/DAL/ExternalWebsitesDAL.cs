// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.ExternalWebsitesDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal static class ExternalWebsitesDAL
  {
    private static readonly Log _log = new Log();
    private static readonly Lazy<ISwisConnectionProxyFactory> _swisConnectionProxyFactory = new Lazy<ISwisConnectionProxyFactory>((Func<ISwisConnectionProxyFactory>) (() => (ISwisConnectionProxyFactory) new SwisConnectionProxyFactory()), LazyThreadSafetyMode.PublicationOnly);

    public static ExternalWebsite Get(int id)
    {
      // ISSUE: method pointer
      return Collection<int, ExternalWebsite>.GetCollectionItem<ExternalWebsites>(new Collection<int, ExternalWebsite>.CreateElement((object) null, __methodptr(Create)), "SELECT * FROM ExternalWebsites WHERE ExternalWebsiteID=@site", new SqlParameter[1]
      {
        new SqlParameter("@site", (object) id)
      });
    }

    public static ExternalWebsites GetAll()
    {
      // ISSUE: method pointer
      return (ExternalWebsites) Collection<int, ExternalWebsite>.FillCollection<ExternalWebsites>(new Collection<int, ExternalWebsite>.CreateElement((object) null, __methodptr(Create)), "SELECT * FROM ExternalWebsites", Array.Empty<SqlParameter>());
    }

    public static void Delete(int id)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE FROM ExternalWebsites WHERE ExternalWebsiteID=@site"))
      {
        textCommand.Parameters.AddWithValue("site", (object) id);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
      ExternalWebsitesDAL.ClearMenuCache();
    }

    public static int Insert(ExternalWebsite site)
    {
      int int32;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT ExternalWebsites (ShortTitle, FullTitle, URL) VALUES (@short, @full, @url)\nSELECT scope_identity()"))
      {
        ExternalWebsitesDAL.AddParams(textCommand, site);
        int32 = Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
      }
      ExternalWebsitesDAL.ClearMenuCache();
      return int32;
    }

    public static void Update(ExternalWebsite site)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE ExternalWebsites SET ShortTitle=@short, FullTitle=@full, URL=@url WHERE ExternalWebsiteID=@site"))
      {
        ExternalWebsitesDAL.AddParams(textCommand, site);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
      ExternalWebsitesDAL.ClearMenuCache();
    }

    private static void AddParams(SqlCommand command, ExternalWebsite site)
    {
      command.Parameters.AddWithValue(nameof (site), (object) site.get_ID());
      command.Parameters.AddWithValue("short", (object) site.get_ShortTitle());
      command.Parameters.AddWithValue("full", (object) site.get_FullTitle());
      command.Parameters.AddWithValue("url", (object) site.get_URL());
    }

    private static ExternalWebsite Create(IDataReader reader)
    {
      ExternalWebsite externalWebsite = new ExternalWebsite();
      externalWebsite.set_ID((int) reader["ExternalWebsiteID"]);
      externalWebsite.set_ShortTitle((string) reader["ShortTitle"]);
      externalWebsite.set_FullTitle((string) reader["FullTitle"]);
      externalWebsite.set_URL((string) reader["URL"]);
      return externalWebsite;
    }

    private static void ClearMenuCache()
    {
      try
      {
        using (SwisConnectionProxy connection = ExternalWebsitesDAL._swisConnectionProxyFactory.Value.CreateConnection())
          connection.Invoke<object>("Orion.Web.Menu", "ClearCache", Array.Empty<object>());
      }
      catch (Exception ex)
      {
        ExternalWebsitesDAL._log.Warn((object) "Could not clear Orion.Web.Menu cache in $ExternalWebsitesDAL.", ex);
      }
    }

    private static class Fields
    {
      public const string ID = "ExternalWebsiteID";
      public const string ShortTitle = "ShortTitle";
      public const string FullTitle = "FullTitle";
      public const string URL = "URL";
    }
  }
}
