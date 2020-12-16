// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.WebMenubarDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal static class WebMenubarDAL
  {
    public static int InsertItem(WebMenuItem item)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT MenuItems (Title, Link, System, NewWindow, Description)\r\nVALUES (@title, @link, 'N', @newwindow, @description)\r\nSELECT scope_identity()"))
      {
        textCommand.Parameters.AddWithValue("title", (object) item.get_Title());
        textCommand.Parameters.AddWithValue("link", (object) item.get_Link());
        textCommand.Parameters.AddWithValue("newwindow", item.get_NewWindow() ? (object) "Y" : (object) "N");
        textCommand.Parameters.AddWithValue("description", (object) item.get_Description());
        return Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
      }
    }

    public static void AppendItemToMenu(string menuName, int itemId)
    {
      int int32;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT MAX(Position) FROM MenuBars WHERE MenuName=@menu"))
      {
        textCommand.Parameters.AddWithValue("menu", (object) menuName);
        int32 = Convert.ToInt32(SqlHelper.ExecuteScalar(textCommand));
      }
      int num = int32 + 1;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT MenuBars (MenuName, MenuItemID, Position)\r\nVALUES (@menu, @item, @position)"))
      {
        textCommand.Parameters.AddWithValue("menu", (object) menuName);
        textCommand.Parameters.AddWithValue("item", (object) itemId);
        textCommand.Parameters.AddWithValue("position", (object) num);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static bool MenuItemExists(string link)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT TOP 1 1 FROM [dbo].[MenuItems] WHERE Link = @link"))
      {
        textCommand.Parameters.AddWithValue("@link", (object) link);
        object obj = SqlHelper.ExecuteScalar(textCommand);
        return obj != DBNull.Value && Convert.ToBoolean(obj);
      }
    }

    private static WebMenuItem Create(IDataReader reader)
    {
      WebMenuItem webMenuItem = new WebMenuItem();
      webMenuItem.set_ID((int) reader["MenuItemID"]);
      webMenuItem.set_Title((string) reader["Title"]);
      webMenuItem.set_Link((string) reader["Link"]);
      webMenuItem.set_NewWindow((string) reader["NewWindow"] == "Y");
      webMenuItem.set_Description((string) reader["Description"]);
      return webMenuItem;
    }

    internal static void DeleteItemByLink(string link)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("DELETE MenuBars FROM MenuBars, MenuItems\r\nWHERE MenuItems.Link=@link\r\nAND MenuBars.MenuItemID=MenuItems.MenuItemID\r\n\r\nDELETE MenuItems FROM MenuItems WHERE MenuItems.Link=@link"))
      {
        textCommand.Parameters.AddWithValue(nameof (link), (object) link);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    internal static void RenameItemByLink(
      string newName,
      string newDescription,
      string newMenuBar,
      string link)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(" declare @position int\r\n\t\t\t\t\t\t\tdeclare @oldMenuBar varchar(200)\r\n\t\t\t\t\t\t\tdeclare @menuItemID int\r\n\r\n\t\t\t\t\t\t\tSELECT TOP 1 @oldMenuBar = MenuBars.MenuName, @menuItemID = MenuItems.MenuItemID FROM MenuBars\r\n\t\t\t\t\t\t\tINNER JOIN MenuItems ON MenuItems.MenuItemID = MenuBars.MenuItemID\r\n\t\t\t\t\t\t\tWHERE MenuItems.Link=@link\r\n\r\n\t\t\t\t\t\t\tIF @oldMenuBar = @menuName\r\n\t\t\t\t\t\t\t\tBEGIN\r\n\t\t\t\t\t\t\t\t\tUPDATE MenuItems SET Title=@title, Description=@description WHERE Link=@link\r\n\t\t\t\t\t\t\t\tEND\r\n\t\t\t\t\t\t\tELSE\r\n\t\t\t\t\t\t\t\tBEGIN\r\n\t\t\t\t\t\t\t\t\tSELECT @position = (SELECT MAX(Position) FROM MenuBars WHERE MenuName LIKE @menuName) + 1\r\n\r\n\t\t\t\t\t\t\t\t\tUPDATE MenuItems SET Title=@title, Description=@description WHERE Link=@link\r\n\t\t\t\t\t\t\t\t\tUPDATE MenuBars SET MenuName=@menuName, Position=@position WHERE MenuItemID=@menuItemID\r\n\t\t\t\t\t\t\t\tEND"))
      {
        textCommand.Parameters.AddWithValue("title", (object) newName);
        textCommand.Parameters.AddWithValue("description", (object) newDescription);
        textCommand.Parameters.AddWithValue("menuName", (object) newMenuBar);
        textCommand.Parameters.AddWithValue(nameof (link), (object) link);
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    private static class Fields
    {
      public const string ID = "MenuItemID";
      public const string Title = "Title";
      public const string Link = "Link";
      public const string NewWindow = "NewWindow";
      public const string Description = "Description";
    }
  }
}
