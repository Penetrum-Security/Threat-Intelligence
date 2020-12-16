// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.CoreHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Configuration;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Xml.Serialization;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class CoreHelper
  {
    private static readonly Log _log = new Log();

    private static void ProcessUpdateResponse(UpdateResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof (response));
      if (response.Success)
      {
        if (response.Manifest != null && response.Manifest.CurrentVersions != null && response.Manifest.CurrentVersions.Length != 0)
        {
          CoreHelper._log.Debug((object) string.Format("ProcessUpdateResponse: Following response has been received from SolarWind's Server: {0}", (object) Serializer.ToXmlString((object) response.Manifest.CurrentVersions)));
          foreach (VersionInfo currentVersion in response.Manifest.CurrentVersions)
          {
            switch (currentVersion.ModuleStatus)
            {
              case ModuleStatusType.Updated:
                if (CoreHelper.ShowUpdateProductNotification(currentVersion.ProductTag))
                {
                  MaintenanceRenewalItemDAL itemForProduct = MaintenanceRenewalItemDAL.GetItemForProduct(currentVersion.ProductTag);
                  if (itemForProduct == null)
                  {
                    CoreHelper._log.DebugFormat("Inserting new MaintenanceRenewalItem for product {0}.", (object) currentVersion.ProductTag);
                    MaintenanceRenewalItemDAL notificationItem = MaintUpdateNotifySvcWrapper.GetNotificationItem(currentVersion);
                    MaintenanceRenewalItemDAL.Insert(Guid.NewGuid(), notificationItem.Title, notificationItem.Description, notificationItem.Ignored, notificationItem.Url, notificationItem.AcknowledgedAt, notificationItem.AcknowledgedBy, notificationItem.ProductTag, notificationItem.DateReleased, notificationItem.NewVersion);
                    break;
                  }
                  CoreHelper._log.DebugFormat("Updating existing MaintenanceRenewalItem for product {0}.", (object) currentVersion.ProductTag);
                  MaintUpdateNotifySvcWrapper.UpdateNotificationItem(itemForProduct, currentVersion);
                  itemForProduct.Update();
                  break;
                }
                break;
              case ModuleStatusType.Current:
                MaintenanceRenewalItemDAL itemForProduct1 = MaintenanceRenewalItemDAL.GetItemForProduct(currentVersion.ProductTag);
                if (itemForProduct1 != null)
                {
                  itemForProduct1.Ignored = true;
                  itemForProduct1.Update();
                  break;
                }
                break;
            }
          }
        }
        else
          CoreHelper._log.Info((object) "No valid modules were submitted, nor found.");
      }
      else
        CoreHelper._log.Error((object) "Process update response failed.");
    }

    private static bool ShowUpdateProductNotification(string productTag)
    {
      ModuleInfo moduleInfo = ModulesCollector.GetModuleInfo(productTag);
      if (string.IsNullOrEmpty(moduleInfo.get_ValidateUpdateNotification()))
        return true;
      try
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetSystemCreator()).Create())
        {
          CoreHelper._log.DebugFormat("Calling SWQL query: {0}", (object) moduleInfo.get_ValidateUpdateNotification());
          DataTable dataTable = ((IInformationServiceProxy) iinformationServiceProxy2).Query(moduleInfo.get_ValidateUpdateNotification());
          if (dataTable.Columns.Count == 1 || dataTable.Rows.Count == 1)
            return dataTable.Rows[0][0] == null || Convert.ToBoolean(dataTable.Rows[0][0]);
          CoreHelper._log.WarnFormat("Invalid query: {0}", (object) moduleInfo.get_ValidateUpdateNotification());
          return true;
        }
      }
      catch (Exception ex)
      {
        CoreHelper._log.ErrorFormat("Execution of ValidateUpdateNotification '{0}' has failed. Exception: {1}", (object) moduleInfo.get_ValidateUpdateNotification(), (object) ex);
        return true;
      }
    }

    public static void CheckMaintenanceRenewals(bool forceCheck)
    {
      UpdateRequest request = new UpdateRequest();
      request.ContractVersion = "1";
      request.CustomerInfo = CustomerEnvironmentManager.GetEnvironmentInfoPack();
      MaintUpdateNotifySvcClient updateNotifySvcClient = new MaintUpdateNotifySvcClient("WSHttpBinding_IMaintUpdateNotifySvc");
      ((IHttpProxySettings) HttpProxySettings.Instance).SetBinding(new WSHttpBinding("WSHttpBinding_IMaintUpdateNotifySvc"));
      try
      {
        CoreHelper._log.Debug((object) string.Format("CheckMaintenanceRenewals: Send following Customer Info to SolarWind's Server: {0}", (object) Serializer.ToXmlString((object) request.CustomerInfo)));
        string primaryLanguage = new I18NHelper().get_PrimaryLanguage();
        CoreHelper.ProcessUpdateResponse(updateNotifySvcClient.GetLocalizedData(request, primaryLanguage));
      }
      catch (Exception ex)
      {
        CoreHelper._log.Error((object) ("CheckMaintenanceRenewals: Error connecting to MaintUpdateNotifySvcClient - " + ex.Message));
      }
      updateNotifySvcClient.Close();
      MaintenanceRenewalsCheckStatusDAL.SetLastUpdateCheck(Settings.CheckMaintenanceRenewalsTimer.TotalMinutes, forceCheck);
    }

    public static void CheckOrionProductTeamBlog()
    {
      HttpWebResponse httpWebResponse = WebRequestHelper.SendHttpWebRequest(BusinessLayerSettings.Instance.OrionProductTeamBlogUrl);
      if (httpWebResponse == null)
        return;
      using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
        BlogItemDAL.StoreBlogItems(ProductBlogSvcWrapper.GetBlogItems((RssBlogItems) new XmlSerializer(typeof (RssBlogItems)).Deserialize((TextReader) streamReader)), Convert.ToInt32(SettingsDAL.Get("ProductsBlog-StoredPostsCount")));
    }

    public static bool IsEngineVersionSameAsOnMain(int engineId)
    {
      string versionOfPrimaryEngine = CoreHelper.GetVersionOfPrimaryEngine();
      string versionOfEngine = CoreHelper.GetVersionOfEngine(engineId);
      int num = string.IsNullOrEmpty(versionOfEngine) || string.IsNullOrEmpty(versionOfPrimaryEngine) ? 0 : (versionOfEngine.Equals(versionOfPrimaryEngine, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0);
      if (num != 0)
        return num != 0;
      CoreHelper._log.Debug((object) string.Format("Engine ({0}) version [{1}] is different from the primary engine version [{2}]", (object) engineId, (object) versionOfEngine, (object) versionOfPrimaryEngine));
      return num != 0;
    }

    public static string GetVersionOfEngine(int engineId)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT EngineVersion FROM Engines WHERE EngineID = @engineID"))
      {
        try
        {
          textCommand.Parameters.AddWithValue("engineID", (object) engineId);
          return Convert.ToString(SqlHelper.ExecuteScalar(textCommand));
        }
        catch (Exception ex)
        {
          CoreHelper._log.Error((object) "Error while trying to get engine version of the engine.", ex);
          return (string) null;
        }
      }
    }

    public static string GetVersionOfPrimaryEngine()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT EngineVersion FROM Engines WHERE ServerType = 'Primary'"))
      {
        try
        {
          return Convert.ToString(SqlHelper.ExecuteScalar(textCommand));
        }
        catch (Exception ex)
        {
          CoreHelper._log.Error((object) "Error while trying to get engine version of the primary engine.", ex);
          return (string) null;
        }
      }
    }
  }
}
