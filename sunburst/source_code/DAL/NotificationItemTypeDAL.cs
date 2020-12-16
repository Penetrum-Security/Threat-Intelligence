// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.NotificationItemTypeDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Enums;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class NotificationItemTypeDAL
  {
    private static readonly Log log = new Log();
    private static List<NotificationItemTypeDAL> _cachedTypes;

    public Guid Id { get; set; }

    public string TypeName { get; set; }

    public string Module { get; set; }

    public string Caption { get; set; }

    public string DetailsUrl { get; set; }

    public string DetailsCaption { get; set; }

    public string Icon { get; set; }

    public string Description { get; set; }

    public List<NotificationItemType.Roles> RequiredRoles { get; private set; }

    public NotificationTypeDisplayAs DisplayAs { get; set; }

    public string CustomDismissButtonText { get; set; }

    public bool HideDismissButton { get; set; }

    private NotificationItemTypeDAL()
    {
      this.Id = Guid.Empty;
      this.TypeName = string.Empty;
      this.Module = string.Empty;
      this.Caption = string.Empty;
      this.DetailsUrl = string.Empty;
      this.DetailsCaption = string.Empty;
      this.Icon = string.Empty;
      this.Description = string.Empty;
      this.DisplayAs = (NotificationTypeDisplayAs) 0;
      this.RequiredRoles = new List<NotificationItemType.Roles>();
    }

    public static NotificationItemTypeDAL GetTypeById(Guid typeId)
    {
      try
      {
        return NotificationItemTypeDAL.GetTypes().FirstOrDefault<NotificationItemTypeDAL>((Func<NotificationItemTypeDAL, bool>) (x => x.Id == typeId));
      }
      catch (ResultCountException ex)
      {
        NotificationItemTypeDAL.log.DebugFormat("Can't find notification item type in database: ID={0}", (object) typeId);
        return (NotificationItemTypeDAL) null;
      }
    }

    public static ICollection<NotificationItemTypeDAL> GetTypes()
    {
      List<NotificationItemTypeDAL> cachedTypes = NotificationItemTypeDAL._cachedTypes;
      if (cachedTypes != null)
        return (ICollection<NotificationItemTypeDAL>) cachedTypes;
      NotificationItemTypeDAL._cachedTypes = NotificationItemTypeDAL.LoadAllTypes();
      return (ICollection<NotificationItemTypeDAL>) NotificationItemTypeDAL._cachedTypes;
    }

    private static List<NotificationItemTypeDAL> LoadAllTypes()
    {
      List<NotificationItemTypeDAL> notificationItemTypeDalList = NotificationItemTypeDAL.LoadCollectionFromDB();
      Dictionary<Guid, List<NotificationItemType.Roles>> dictionary = NotificationItemTypeDAL.LoadRolesFromDB();
      foreach (NotificationItemTypeDAL notificationItemTypeDal in notificationItemTypeDalList)
      {
        if (dictionary.ContainsKey(notificationItemTypeDal.Id))
          notificationItemTypeDal.RequiredRoles = dictionary[notificationItemTypeDal.Id];
      }
      return notificationItemTypeDalList;
    }

    private static List<NotificationItemTypeDAL> LoadCollectionFromDB()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM NotificationItemTypes"))
      {
        using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
        {
          List<NotificationItemTypeDAL> notificationItemTypeDalList = new List<NotificationItemTypeDAL>();
          while (reader.Read())
          {
            NotificationItemTypeDAL notificationItemTypeDal = new NotificationItemTypeDAL();
            notificationItemTypeDal.LoadFromReader(reader);
            notificationItemTypeDalList.Add(notificationItemTypeDal);
          }
          return notificationItemTypeDalList;
        }
      }
    }

    private void LoadFromReader(IDataReader reader)
    {
      this.Id = reader.GetGuid(reader.GetOrdinal("TypeID"));
      this.TypeName = reader.GetString(reader.GetOrdinal("TypeName"));
      this.Module = reader.GetString(reader.GetOrdinal("Module"));
      this.Caption = DatabaseFunctions.GetString(reader, "Caption");
      this.DetailsUrl = DatabaseFunctions.GetString(reader, "DetailsUrl");
      this.DetailsCaption = DatabaseFunctions.GetString(reader, "DetailsCaption");
      this.Icon = DatabaseFunctions.GetString(reader, "Icon");
      this.Description = DatabaseFunctions.GetString(reader, "Description");
      this.CustomDismissButtonText = DatabaseFunctions.GetString(reader, "CustomDismissButtonText");
      this.HideDismissButton = DatabaseFunctions.GetBoolean(reader, "HideDismissButton");
      this.DisplayAs = (NotificationTypeDisplayAs) SqlHelper.ParseEnum<NotificationTypeDisplayAs>(reader.GetString(reader.GetOrdinal("DisplayAs")));
    }

    private static Dictionary<Guid, List<NotificationItemType.Roles>> LoadRolesFromDB()
    {
      Dictionary<Guid, List<NotificationItemType.Roles>> dictionary = new Dictionary<Guid, List<NotificationItemType.Roles>>();
      using (SqlCommand sqlCommand = new SqlCommand("SELECT NotificationTypeID, RequiredRoleID FROM NotificationTypePermissions"))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(sqlCommand))
        {
          while (dataReader.Read())
          {
            Guid guid = DatabaseFunctions.GetGuid(dataReader, 0);
            int int32 = DatabaseFunctions.GetInt32(dataReader, 1);
            if (!dictionary.ContainsKey(guid))
              dictionary[guid] = new List<NotificationItemType.Roles>();
            dictionary[guid].Add((NotificationItemType.Roles) int32);
          }
          return dictionary;
        }
      }
    }
  }
}
