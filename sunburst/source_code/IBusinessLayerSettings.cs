// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.IBusinessLayerSettings
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using System;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public interface IBusinessLayerSettings
  {
    int DBConnectionRetryInterval { get; }

    int DBConnectionRetryIntervalOnFail { get; }

    int DBConnectionRetries { get; }

    string JobSchedulerEndpointNetPipe { get; }

    string JobSchedulerEndpointTcpPipe { get; }

    bool ProxyAvailable { get; }

    string UserName { get; }

    string Password { get; }

    string ProxyAddress { get; }

    int ProxyPort { get; }

    string OrionProductTeamBlogUrl { get; }

    int LicenseSaturationCheckInterval { get; }

    int MaintenanceExpirationWarningDays { get; }

    int MaintenanceExpiredShowAgainAtDays { get; }

    TimeSpan PollerLimitTimer { get; }

    TimeSpan CheckDatabaseLimitTimer { get; }

    TimeSpan CheckForOldLogsTimer { get; }

    TimeSpan UpdateEngineTimer { get; }

    TimeSpan RemoteCollectorStatusCacheExpiration { get; }

    bool MaintenanceModeEnabled { get; }

    TimeSpan SettingsToRegistryFrequency { get; }

    TimeSpan DiscoveryUpdateNetObjectStatusWaitForChangesDelay { get; }

    TimeSpan DiscoveryUpdateNetObjectStatusStartupDelay { get; }

    bool EnableLimitationReplacement { get; }

    bool EnableTechnologyPollingAssignmentsChangesAuditing { get; }

    int LimitationSqlExaggeration { get; }

    TimeSpan AgentDiscoveryPluginsDeploymentTimeLimit { get; }

    TimeSpan AgentDiscoveryJobTimeout { get; }

    TimeSpan SafeCertificateMaintenanceTrialPeriod { get; }

    TimeSpan CertificateMaintenanceTaskCheckInterval { get; }

    TimeSpan CertificateMaintenanceNotificationReappearPeriod { get; }

    TimeSpan CertificateMaintenanceAgentPollFrequency { get; }

    TimeSpan TestJobTimeout { get; }

    TimeSpan OrionFeatureRefreshTimer { get; }

    TimeSpan BackgroundInventoryCheckTimer { get; }

    int BackgroundInventoryParallelTasksCount { get; }

    int BackgroundInventoryRetriesCount { get; }

    int ThresholdsProcessingBatchSize { get; }

    string ThresholdsProcessingDefaultTimeFrame { get; }

    TimeSpan ThresholdsProcessingDefaultTimer { get; }

    bool ThresholdsProcessingEnabled { get; }

    string ThresholdsUseBaselineCalculationMacro { get; }

    string ThresholdsUseBaselineWarningCalculationMacro { get; }

    string ThresholdsUseBaselineCriticalCalculationMacro { get; }

    string ThresholdsDefaultWarningFormulaForGreater { get; }

    string ThresholdsDefaultWarningFormulaForLess { get; }

    string ThresholdsDefaultCriticalFormulaForGreater { get; }

    string ThresholdsDefaultCriticalFormulaForLess { get; }

    int ThresholdsHistogramChartIntervalsCount { get; }

    int EvaluationExpirationCheckIntervalHours { get; }

    int EvaluationExpirationNotificationDays { get; }

    int EvaluationExpirationShowAgainAtDays { get; }

    int CachedWebImageExpirationPeriodDays { get; }
  }
}
