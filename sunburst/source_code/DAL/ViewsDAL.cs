// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.ViewsDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class ViewsDAL
  {
    private static readonly Log log = new Log();

    public static Views GetSummaryDetailsViews()
    {
      // ISSUE: method pointer
      return (Views) Collection<int, WebView>.FillCollection<Views>(new Collection<int, WebView>.CreateElement((object) null, __methodptr(CreateView)), "SELECT * FROM Views WHERE (NOT(ViewType LIKE 'Volume%')) AND ((ViewType LIKE 'Summary') OR (ViewType LIKE '%Details'))", (SqlParameter[]) null);
    }

    private static WebView CreateView(IDataReader reader)
    {
      WebView webView = new WebView();
      webView.set_ViewType(reader["ViewType"].ToString().Trim());
      webView.set_ViewID(Convert.ToInt32(reader["ViewID"]));
      webView.set_ViewTitle(reader["ViewTitle"].ToString().Trim());
      webView.set_ViewGroupName(reader["ViewGroupName"].ToString().Trim());
      return webView;
    }
  }
}
