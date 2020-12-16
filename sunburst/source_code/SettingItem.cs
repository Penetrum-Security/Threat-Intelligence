// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.SettingItem
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class SettingItem : SynchronizeItem
  {
    public SettingItem.ColumnType Column;
    public string SettingID;

    public SettingItem(string settingID)
      : this(settingID, SettingItem.ColumnType.CurrentValue)
    {
    }

    public SettingItem(string settingID, SettingItem.ColumnType columnType)
    {
      this.SettingID = settingID;
      this.Column = columnType;
    }

    public override object GetDatabaseValue()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("SELECT {0} FROM Settings WHERE SettingID=@SettingID", (object) this.Column.ToString())))
      {
        textCommand.Parameters.AddWithValue("SettingID", (object) this.SettingID);
        return SqlHelper.ExecuteScalar(textCommand);
      }
    }

    public override string ToString()
    {
      return string.Format("{0}/{1}", (object) this.SettingID, (object) this.Column);
    }

    internal enum ColumnType
    {
      CurrentValue,
      Description,
    }
  }
}
