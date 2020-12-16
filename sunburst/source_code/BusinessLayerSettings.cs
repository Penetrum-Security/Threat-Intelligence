// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.BusinessLayerSettings
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Configuration;
using SolarWinds.Orion.Core.SharedCredentials.Credentials;
using SolarWinds.Settings;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class BusinessLayerSettings : SettingsBase, IBusinessLayerSettings
  {
    private static readonly Lazy<IBusinessLayerSettings> instance = new Lazy<IBusinessLayerSettings>((Func<IBusinessLayerSettings>) (() => (IBusinessLayerSettings) new BusinessLayerSettings()));
    public static Func<IBusinessLayerSettings> Factory = (Func<IBusinessLayerSettings>) (() => BusinessLayerSettings.instance.Value);

    private BusinessLayerSettings()
    {
      base.\u002Ector();
    }

    public static IBusinessLayerSettings Instance
    {
      get
      {
        return BusinessLayerSettings.Factory();
      }
    }

    [Setting(AllowServerOverride = true, Default = 30, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int DBConnectionRetryInterval { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 300, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int DBConnectionRetryIntervalOnFail { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 10, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int DBConnectionRetries { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "net.pipe://localhost/solarwinds/jobengine/scheduler", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string JobSchedulerEndpointNetPipe { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "net.tcp://{0}:17777/solarwinds/jobengine/scheduler/ssl", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string JobSchedulerEndpointTcpPipe { get; internal set; }

    [Obsolete("use SolarWinds.Orion.Core.Common.Configuration.HttpProxySettings without using WCF")]
    public bool ProxyAvailable
    {
      get
      {
        return !((IHttpProxySettings) HttpProxySettings.Instance).get_IsDisabled();
      }
    }

    [Obsolete("use SolarWinds.Orion.Core.Common.Configuration.HttpProxySettings without using WCF")]
    public string UserName
    {
      get
      {
        IHttpProxySettings instance = (IHttpProxySettings) HttpProxySettings.Instance;
        if (!instance.get_IsValid())
          return (string) null;
        if (instance.get_UseSystemDefaultProxy())
          return string.Empty;
        return !(instance.get_Credential() is UsernamePasswordCredential credential) ? (string) null : credential.get_Username();
      }
    }

    [Obsolete("use SolarWinds.Orion.Core.Common.Configuration.HttpProxySettings without using WCF")]
    public string Password
    {
      get
      {
        IHttpProxySettings instance = (IHttpProxySettings) HttpProxySettings.Instance;
        if (!instance.get_IsValid())
          return (string) null;
        return !(instance.get_Credential() is UsernamePasswordCredential credential) ? (string) null : credential.get_Password();
      }
    }

    [Obsolete("use SolarWinds.Orion.Core.Common.Configuration.HttpProxySettings without using WCF")]
    public string ProxyAddress
    {
      get
      {
        IHttpProxySettings instance = (IHttpProxySettings) HttpProxySettings.Instance;
        return !instance.get_IsValid() ? string.Empty : new Uri(instance.get_Uri()).Host;
      }
    }

    [Obsolete("use SolarWinds.Orion.Core.Common.Configuration.HttpProxySettings without using WCF")]
    public int ProxyPort
    {
      get
      {
        IHttpProxySettings instance = (IHttpProxySettings) HttpProxySettings.Instance;
        return !instance.get_IsValid() ? 0 : new Uri(instance.get_Uri()).Port;
      }
    }

    [Setting(AllowServerOverride = true, Default = "http://thwackfeeds.solarwinds.com/blogs/orion-product-team-blog/rss.aspx", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string OrionProductTeamBlogUrl { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 60, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int LicenseSaturationCheckInterval { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 90, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int MaintenanceExpirationWarningDays { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 15, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int MaintenanceExpiredShowAgainAtDays { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:05:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan PollerLimitTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:30:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan CheckDatabaseLimitTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:05:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan CheckForOldLogsTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:00:30", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan UpdateEngineTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:02:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan RemoteCollectorStatusCacheExpiration { get; internal set; }

    [Setting(AllowServerOverride = true, Default = false, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public bool MaintenanceModeEnabled { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:01:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan SettingsToRegistryFrequency { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:00:10", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan DiscoveryUpdateNetObjectStatusWaitForChangesDelay { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:02:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan DiscoveryUpdateNetObjectStatusStartupDelay { get; internal set; }

    [Setting(AllowServerOverride = true, Default = true, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public bool EnableLimitationReplacement { get; internal set; }

    [Setting(AllowServerOverride = true, Default = true, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public bool EnableTechnologyPollingAssignmentsChangesAuditing { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 5, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int LimitationSqlExaggeration { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:10:00")]
    public TimeSpan AgentDiscoveryPluginsDeploymentTimeLimit { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:10:00")]
    public TimeSpan AgentDiscoveryJobTimeout { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "1.00:00:00", Deprecated = true, Description = "Time for which we try to perform safe certificate maintenance. If the certificate maintenance can't be done in a safe way - no damage to the system or data - for given time, we inform user and let him confirm maintenance with knowledge what will break.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan SafeCertificateMaintenanceTrialPeriod { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:05:00", Deprecated = true, Description = "Frequency with which certificate maintenance task result is checked.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan CertificateMaintenanceTaskCheckInterval { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:05:00", Deprecated = true, Description = "After how long a notification about certificate maintenance approval reappears if customer acknowledges it and does not approve certificate maintenance.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan CertificateMaintenanceNotificationReappearPeriod { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:01:00", Deprecated = true, Description = "How often Core polls AMS to get fresh status of agent certificate update for certificate maintenance.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan CertificateMaintenanceAgentPollFrequency { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:00:30", Description = "How long Core waits for results from test jobs.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan TestJobTimeout { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:10:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan BackgroundInventoryCheckTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 50, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int BackgroundInventoryParallelTasksCount { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 10, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int BackgroundInventoryRetriesCount { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 10000, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int ThresholdsProcessingBatchSize { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "Core_All", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsProcessingDefaultTimeFrame { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:05:00", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan ThresholdsProcessingDefaultTimer { get; internal set; }

    [Setting(AllowServerOverride = true, Default = true, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public bool ThresholdsProcessingEnabled { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${USE_BASELINE}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsUseBaselineCalculationMacro { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${USE_BASELINE_WARNING}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsUseBaselineWarningCalculationMacro { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${USE_BASELINE_CRITICAL}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsUseBaselineCriticalCalculationMacro { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${MEAN}+2*${STD_DEV}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsDefaultWarningFormulaForGreater { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${MEAN}-2*${STD_DEV}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsDefaultWarningFormulaForLess { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${MEAN}+3*${STD_DEV}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsDefaultCriticalFormulaForGreater { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "${MEAN}-3*${STD_DEV}", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public string ThresholdsDefaultCriticalFormulaForLess { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 50, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int ThresholdsHistogramChartIntervalsCount { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 12, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int EvaluationExpirationCheckIntervalHours { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 14, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int EvaluationExpirationNotificationDays { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 7, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int EvaluationExpirationShowAgainAtDays { get; internal set; }

    [Setting(AllowServerOverride = true, Default = 7, ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public int CachedWebImageExpirationPeriodDays { get; internal set; }

    [Setting(AllowServerOverride = true, Default = "00:10:00", Description = "How often we synchronize Orion.Features.", ServiceRestartDependencies = new string[] {"OrionModuleEngine"})]
    public TimeSpan OrionFeatureRefreshTimer { get; internal set; }
  }
}
