// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.NodeChildStatusParticipationDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.PackageManager;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class NodeChildStatusParticipationDAL
  {
    private static readonly Log log = new Log();

    public static void ResyncAfterStartup()
    {
      try
      {
        bool needsreflow;
        NodeChildStatusParticipationDAL.UpdateParticipationFromInstalledProducts(out needsreflow);
        if (!needsreflow)
          return;
        NodeChildStatusParticipationDAL.ReflowAllNodeChildStatus();
      }
      catch (Exception ex)
      {
        NodeChildStatusParticipationDAL.log.Error((object) "Unhandled exception when reinitailizing node child status", ex);
      }
    }

    public static void UpdateParticipationFromInstalledProducts(out bool needsreflow)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(new StringBuilder("UPDATE dbo.[NodeChildStatusParticipation] set Installed=0 Where ModuleName not in (").Append(string.Join(",", ((IPackageManager) SolarWinds.Orion.Core.Common.PackageManager.PackageManager.Instance).GetInstalledPackages().Select<PackageInfo, string>((Func<PackageInfo, string>) (package => package.get_PackageId())).Concat<string>(((IEnumerable<ModuleInfo>) ModulesCollector.GetInstalledModules()).Select<ModuleInfo, string>((Func<ModuleInfo, string>) (module => module.get_ProductShortName()))).Select<string, string>((Func<string, string>) (name => '\''.ToString() + name.Replace("'", "''") + (object) '\'')))).Append(')').ToString()))
      {
        int num = SqlHelper.ExecuteNonQuery(textCommand);
        needsreflow = num > 0;
      }
    }

    private static SqlCommand MakeParticipationChangeQuery(
      Dictionary<string, bool> changes,
      bool value)
    {
      StringBuilder stringBuilder = new StringBuilder();
      SqlCommand sqlCommand = new SqlCommand();
      int num = 0;
      foreach (string str in changes.Where<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>) (x => x.Value == value)).Select<KeyValuePair<string, bool>, string>((Func<KeyValuePair<string, bool>, string>) (x => x.Key)))
      {
        if (num == 0)
          stringBuilder.AppendFormat("UPDATE dbo.NodeChildStatusParticipation set Enabled={0} WHERE Excluded=0 AND EntityType in (", value ? (object) "1" : (object) "0");
        stringBuilder.AppendFormat("{0}@e{1}", num == 0 ? (object) "" : (object) ",", (object) num);
        sqlCommand.Parameters.AddWithValue("@e" + num.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) str);
        ++num;
      }
      if (num != 0)
      {
        stringBuilder.Append(")");
        sqlCommand.CommandText = stringBuilder.ToString();
      }
      return sqlCommand;
    }

    public static void ReflowAllNodeChildStatus()
    {
      SqlHelper.ExecuteStoredProc("swsp_ReflowAllNodeChildStatus", Array.Empty<SqlParameter>());
    }
  }
}
