// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.ReportJobInitializer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Actions.Utility;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Models.Actions;
using SolarWinds.Orion.Core.Models.Actions.Contexts;
using SolarWinds.Orion.Core.Models.MacroParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class ReportJobInitializer
  {
    private static readonly Log log = new Log();

    public static void AddActionsToScheduler(
      ReportJobConfiguration config,
      CoreBusinessLayerService service)
    {
      if (!config.get_Enabled())
        return;
      ReportingActionContext reportingActionContext = new ReportingActionContext();
      reportingActionContext.set_AccountID(config.get_AccountID());
      reportingActionContext.set_UrlsGroupedByLeftPart(ReportJobInitializer.GroupUrls(config));
      reportingActionContext.set_WebsiteID(config.get_WebsiteID());
      ReportingActionContext reportingContext = reportingActionContext;
      MacroContext macroContext = ((ActionContextBase) reportingContext).get_MacroContext();
      ReportingContext reportingContext1 = new ReportingContext();
      reportingContext1.set_AccountID(config.get_AccountID());
      reportingContext1.set_ScheduleName(config.get_Name());
      reportingContext1.set_ScheduleDescription(config.get_Description());
      reportingContext1.set_LastRun(config.get_LastRun());
      reportingContext1.set_WebsiteID(config.get_WebsiteID());
      macroContext.Add((ContextBase) reportingContext1);
      ((ActionContextBase) reportingContext).get_MacroContext().Add((ContextBase) new GenericContext());
      int num = 0;
      if (config.get_Schedules() == null)
        return;
      using (List<ReportSchedule>.Enumerator enumerator1 = config.get_Schedules().GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          ReportSchedule current = enumerator1.Current;
          DateTime dateTime = !current.get_EndTime().HasValue ? DateTime.MaxValue : current.get_EndTime().Value;
          Scheduler.get_Instance().Add(new ScheduledTask(string.Format("ReportJob-{0}_{1}", (object) config.get_ReportJobID(), (object) num), (TimerCallback) (o =>
          {
            ReportJobInitializer.log.Info((object) "Starting action execution");
            using (List<ActionDefinition>.Enumerator enumerator = config.get_Actions().GetEnumerator())
            {
              while (enumerator.MoveNext())
                service.ExecuteAction(enumerator.Current, (ActionContextBase) reportingContext);
            }
            config.set_LastRun(new DateTime?(DateTime.Now.ToUniversalTime()));
            ReportJobDAL.UpdateLastRun(config.get_ReportJobID(), config.get_LastRun());
          }), (object) null, current.get_CronExpression(), current.get_StartTime(), dateTime, config.get_LastRun(), current.get_CronExpressionTimeZoneInfo()), true);
          ++num;
        }
      }
    }

    public static Dictionary<string, List<string>> GroupUrls(
      ReportJobConfiguration config)
    {
      StringBuilder errors = new StringBuilder();
      StringComparer strcmp = StringComparer.OrdinalIgnoreCase;
      Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>((IEqualityComparer<string>) strcmp);
      if (config == null)
      {
        ReportJobInitializer.log.ErrorFormat("GroupUrls(ReportJobConfiguration) config is NULL {0}", (object) Environment.StackTrace);
        return dictionary;
      }
      try
      {
        List<string> list = ((IEnumerable<ReportTuple>) config.get_Reports()).Select<ReportTuple, string>((Func<ReportTuple, string>) (report => string.Format("{0}/Orion/Report.aspx?ReportID={1}", (object) WebsitesDAL.GetSiteAddress(config.get_WebsiteID()), (object) report.get_ID()))).Union<string>(config.get_Urls().Select<string, string>((Func<string, string>) (url => !url.Contains<char>('?') ? url + "?" : url))).ToList<string>();
        foreach (string uriString in list)
        {
          if (uriString.IndexOf("/Orion/", StringComparison.OrdinalIgnoreCase) < 0)
          {
            if (!dictionary.ContainsKey((string) OrionWebClient.UseDefaultWebsiteIdentifier))
              dictionary.Add((string) OrionWebClient.UseDefaultWebsiteIdentifier, new List<string>());
            dictionary[(string) OrionWebClient.UseDefaultWebsiteIdentifier].Add(uriString);
          }
          else
          {
            string uriLeftPart;
            try
            {
              Uri result;
              if (!Uri.TryCreate(uriString, UriKind.Absolute, out result))
              {
                errors.AppendFormat("Invalid URL {0} \r\n", (object) uriString);
                continue;
              }
              uriLeftPart = result.GetLeftPart(UriPartial.Authority);
            }
            catch (Exception ex)
            {
              errors.AppendFormat("Invalid URL {0}. {1}\r\n", (object) uriString, (object) ex);
              continue;
            }
            if (!dictionary.ContainsKey(uriLeftPart))
              dictionary.Add(uriLeftPart, list.Where<string>((Func<string, bool>) (u =>
              {
                try
                {
                  Uri result;
                  if (Uri.TryCreate(u, UriKind.Absolute, out result))
                    return strcmp.Equals(uriLeftPart, result.GetLeftPart(UriPartial.Authority));
                  errors.AppendFormat("Invalid URL {0} \r\n", (object) u);
                  return false;
                }
                catch (Exception ex)
                {
                  errors.AppendFormat("Invalid URL {0}. {1}\r\n", (object) u, (object) ex);
                  return false;
                }
              })).ToList<string>());
          }
        }
      }
      catch (Exception ex)
      {
        errors.AppendFormat("Unexpected exception {0}", (object) ex);
      }
      if (errors.Length > 0)
      {
        StringBuilder stringBuilder = new StringBuilder().AppendFormat("Errors in ReportJob-{0}({1}) @ Engine {2} & Website {3} \r\n", (object) config.get_ReportJobID(), (object) config.get_Name(), (object) config.get_EngineId(), (object) config.get_WebsiteID()).Append((object) errors);
        ReportJobInitializer.log.Error((object) stringBuilder);
      }
      return dictionary;
    }
  }
}
