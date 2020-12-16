// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.GeolocationJobInitializer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Models.Actions;
using SolarWinds.Orion.Core.Models.Actions.Contexts;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class GeolocationJobInitializer
  {
    private static readonly Log log = new Log();
    public const string JobNamingPattern = "GeolocationJob-{0}";

    public static void AddActionsToScheduler(CoreBusinessLayerService service)
    {
      GeolocationActionContext geolocationContext = new GeolocationActionContext();
      string[] availableForGeolocation = WorldMapPointsDAL.GetEntitiesAvailableForGeolocation();
      int num = 1;
      foreach (string str1 in availableForGeolocation)
      {
        string currentEntity = str1;
        Scheduler.get_Instance().Add(new ScheduledTask(string.Format("GeolocationJob-{0}", (object) num), (TimerCallback) (o =>
        {
          string str;
          if (!Settings.IsAutomaticGeolocationEnabled || !WebSettingsDAL.TryGet(string.Format("{0}_GeolocationField", (object) currentEntity), ref str) || string.IsNullOrWhiteSpace(str))
            return;
          GeolocationJobInitializer.log.Info((object) "Starting action execution");
          CoreBusinessLayerService businessLayerService = service;
          ActionDefinition actionDefinition = new ActionDefinition();
          actionDefinition.set_ActionTypeID("Geolocation");
          actionDefinition.set_Enabled(true);
          ActionProperties actionProperties = new ActionProperties();
          actionProperties.Add("StreetAddress", str);
          actionProperties.Add("Entity", currentEntity);
          actionProperties.Add("MapQuestApiKey", WorldMapPointsDAL.GetMapQuestKey());
          actionDefinition.set_Properties(actionProperties);
          GeolocationActionContext geolocationActionContext = geolocationContext;
          businessLayerService.ExecuteAction(actionDefinition, (ActionContextBase) geolocationActionContext);
        }), (object) null, Settings.AutomaticGeolocationCheckInterval));
        ++num;
      }
    }
  }
}
