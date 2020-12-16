// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.JobScheduler
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.JobEngine;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading;
using System.Xml;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class JobScheduler : IJobSchedulerHelper, IJobScheduler, IDisposable
  {
    private static readonly Log log = new Log();
    private readonly IJobEngineWcfCredentialProvider wcfCredentialProvider;
    private IJobScheduler schedulerChannel;

    public static IJobSchedulerHelper GetInstance()
    {
      return (IJobSchedulerHelper) new JobScheduler((IJobEngineWcfCredentialProvider) new JobEngineWcfCredentialProvider());
    }

    public JobScheduler(
      IJobEngineWcfCredentialProvider wcfCredentialProvider)
    {
      IJobEngineWcfCredentialProvider credentialProvider = wcfCredentialProvider;
      if (credentialProvider == null)
        throw new ArgumentNullException(nameof (wcfCredentialProvider));
      this.wcfCredentialProvider = credentialProvider;
    }

    private T ExecuteJobSchedulerOperation<T>(
      string operationName,
      JobScheduler.JobSchedulerOperation<T> operation)
    {
      return this.ExecuteJobSchedulerOperation<T>(operationName, operation, true);
    }

    private T ExecuteJobSchedulerOperation<T>(
      string operationName,
      JobScheduler.JobSchedulerOperation<T> operation,
      bool retryOnError)
    {
      int num = 0;
      for (int index = 0; index < JobScheduler.Settings.RetryCount; ++index)
      {
        ++num;
        try
        {
          this.Instantiate();
          return operation();
        }
        catch (TimeoutException ex)
        {
          JobScheduler.log.WarnFormat("{0}: JobScheduler channel threw TimeoutException. Retries: {1}", (object) operationName, (object) index);
          JobScheduler.log.Warn((object) "Job Scheduler Exception:", (Exception) ex);
          Thread.Sleep(JobScheduler.Settings.RetryInterval);
        }
        catch (ActionNotSupportedException ex)
        {
          JobScheduler.log.WarnFormat("{0}: JobScheduler channel threw ActionNotSupportedException", (object) operationName);
          JobScheduler.log.Warn((object) "Job Scheduler Exception:", (Exception) ex);
          throw;
        }
        catch (FaultException ex)
        {
          JobScheduler.log.WarnFormat("{0}: JobScheduler channel threw FaultException", (object) operationName);
          JobScheduler.log.Warn((object) "Job Scheduler Exception:", (Exception) ex);
          throw;
        }
        catch (CommunicationException ex)
        {
          JobScheduler.log.WarnFormat("{0}: JobScheduler channel threw CommunicationException. Retries: {1}", (object) operationName, (object) index);
          JobScheduler.log.Warn((object) "Job Scheduler Exception:", (Exception) ex);
          Thread.Sleep(JobScheduler.Settings.RetryInterval);
        }
        if (!retryOnError)
          break;
      }
      JobScheduler.log.ErrorFormat("{0}: Could not reach JobScheduler service at {1} after {2} retries", (object) operationName, (object) BusinessLayerSettings.Instance.JobSchedulerEndpointNetPipe, (object) num);
      throw new ApplicationException(string.Format("Could not reach JobScheduler service after {0} retries", (object) num));
    }

    public void CancelJob(Guid jobId)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (CancelJob), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.CancelJob(jobId);
        return 0;
      }));
    }

    public void Clear(string productNamespace)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (Clear), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.Clear(productNamespace);
        return 0;
      }));
    }

    public void AddExecutionEngine(Uri executionEngineUri, bool enabled)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (AddExecutionEngine), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.AddExecutionEngine(executionEngineUri, enabled);
        return 0;
      }));
    }

    public Guid AddJob(ScheduledJob jobConfiguration)
    {
      return this.ExecuteJobSchedulerOperation<Guid>(nameof (AddJob), (JobScheduler.JobSchedulerOperation<Guid>) (() => this.schedulerChannel.AddJob(jobConfiguration)));
    }

    public void AddPolicy(XmlElement policy)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (AddPolicy), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.AddPolicy(policy);
        return 0;
      }));
    }

    public void RemoveExecutionEngine(Uri executionEngineUri)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (RemoveExecutionEngine), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.RemoveExecutionEngine(executionEngineUri);
        return 0;
      }));
    }

    public void RemoveJob(Guid jobId)
    {
      try
      {
        this.ExecuteJobSchedulerOperation<int>(nameof (RemoveJob), (JobScheduler.JobSchedulerOperation<int>) (() =>
        {
          this.schedulerChannel.RemoveJob(jobId);
          return 0;
        }));
      }
      catch (Exception ex)
      {
        JobScheduler.log.WarnFormat("Exception while removing job {0}.  Exception: {1}", (object) jobId, (object) ex);
      }
    }

    public void RemoveJobs(Guid[] jobIds)
    {
      try
      {
        this.ExecuteJobSchedulerOperation<int>(nameof (RemoveJobs), (JobScheduler.JobSchedulerOperation<int>) (() =>
        {
          this.schedulerChannel.RemoveJobs(jobIds);
          return 0;
        }));
      }
      catch (Exception ex)
      {
        JobScheduler.log.WarnFormat("Exception while removing jobs {0}.  Exception: {1}", (object) jobIds, (object) ex);
      }
    }

    public void RemovePolicy(string policyId)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (RemovePolicy), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.RemovePolicy(policyId);
        return 0;
      }));
    }

    public bool ExecutionEngineExists(Uri executionEngineUri)
    {
      return this.ExecuteJobSchedulerOperation<bool>(nameof (ExecutionEngineExists), (JobScheduler.JobSchedulerOperation<bool>) (() => this.schedulerChannel.ExecutionEngineExists(executionEngineUri)), false);
    }

    public bool PolicyExists(string policyId)
    {
      return this.ExecuteJobSchedulerOperation<bool>(nameof (PolicyExists), (JobScheduler.JobSchedulerOperation<bool>) (() => this.schedulerChannel.PolicyExists(policyId)));
    }

    public void UpdateJob(Guid jobId, ScheduledJob job, bool executeImmediately)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (UpdateJob), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.UpdateJob(jobId, job, executeImmediately);
        return 0;
      }));
    }

    public string GetPublicKey()
    {
      return this.ExecuteJobSchedulerOperation<string>(nameof (GetPublicKey), (JobScheduler.JobSchedulerOperation<string>) (() => this.schedulerChannel.GetPublicKey()));
    }

    public IList<ExecutionEngineInfo> EnumerateExecutionEngines()
    {
      return this.ExecuteJobSchedulerOperation<IList<ExecutionEngineInfo>>(nameof (EnumerateExecutionEngines), (JobScheduler.JobSchedulerOperation<IList<ExecutionEngineInfo>>) (() => this.schedulerChannel.EnumerateExecutionEngines()));
    }

    public ScheduledJobInfo[] EnumerateScheduledJobs()
    {
      return this.ExecuteJobSchedulerOperation<ScheduledJobInfo[]>(nameof (EnumerateScheduledJobs), (JobScheduler.JobSchedulerOperation<ScheduledJobInfo[]>) (() => this.schedulerChannel.EnumerateScheduledJobs()));
    }

    public IList<XmlElement> EnumeratePolicies()
    {
      return this.ExecuteJobSchedulerOperation<IList<XmlElement>>(nameof (EnumeratePolicies), (JobScheduler.JobSchedulerOperation<IList<XmlElement>>) (() => this.schedulerChannel.EnumeratePolicies()));
    }

    public void ResumeExecutionEngine(Uri executionEngineUri)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (ResumeExecutionEngine), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.ResumeExecutionEngine(executionEngineUri);
        return 0;
      }));
    }

    public void SuspendExecutionEngine(Uri executionEngineUri)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (SuspendExecutionEngine), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.SuspendExecutionEngine(executionEngineUri);
        return 0;
      }));
    }

    public void UpdatePolicy(XmlElement policy)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (UpdatePolicy), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.UpdatePolicy(policy);
        return 0;
      }));
    }

    public Stream GetJobResultStream(Guid jobId, string streamName)
    {
      return this.ExecuteJobSchedulerOperation<Stream>(nameof (GetJobResultStream), (JobScheduler.JobSchedulerOperation<Stream>) (() => this.schedulerChannel.GetJobResultStream(jobId, streamName)));
    }

    public void DeleteJobResult(Guid jobId)
    {
      this.ExecuteJobSchedulerOperation<int>(nameof (DeleteJobResult), (JobScheduler.JobSchedulerOperation<int>) (() =>
      {
        this.schedulerChannel.DeleteJobResult(jobId);
        return 0;
      }));
    }

    ~JobScheduler()
    {
      this.Dispose(false);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (this.schedulerChannel == null)
        return;
      MessageUtilities.ShutdownCommunicationObject((ICommunicationObject) this.schedulerChannel);
    }

    private void Instantiate()
    {
      string host = WebSettingsDAL.Get("JobSchedulerHost");
      ChannelFactory<IJobScheduler> channelFactory = string.IsNullOrEmpty(host) || Environment.MachineName.Equals(host, StringComparison.OrdinalIgnoreCase) ? this.MakeNamedPipeChannelFactory() : this.MakeTcpChannelFactory(host);
      if (this.schedulerChannel != null)
        this.Dispose(false);
      this.schedulerChannel = channelFactory.CreateChannel();
    }

    private ChannelFactory<IJobScheduler> MakeTcpChannelFactory(
      string host)
    {
      NetTcpBinding netTcpBinding = new NetTcpBinding("Core.NetTcpBinding.ToJobScheduler");
      string uriString = string.Format(BusinessLayerSettings.Instance.JobSchedulerEndpointTcpPipe, (object) host);
      JobScheduler.log.DebugFormat("Channel created to {0}", (object) uriString);
      EndpointAddress remoteAddress = new EndpointAddress(new Uri(uriString), EndpointIdentity.CreateDnsIdentity("SolarWinds JobEngine Security"), Array.Empty<AddressHeader>());
      ChannelFactory<IJobScheduler> channelFactory = new ChannelFactory<IJobScheduler>((Binding) netTcpBinding, remoteAddress);
      channelFactory.Credentials.UserName.UserName = this.wcfCredentialProvider.get_UserName();
      channelFactory.Credentials.UserName.Password = this.wcfCredentialProvider.get_Password();
      X509ChainPolicy chainPolicy = new X509ChainPolicy();
      chainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid | X509VerificationFlags.AllowUnknownCertificateAuthority;
      channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
      channelFactory.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = X509CertificateValidator.CreateChainTrustValidator(true, chainPolicy);
      return channelFactory;
    }

    private ChannelFactory<IJobScheduler> MakeNamedPipeChannelFactory()
    {
      NetNamedPipeBinding namedPipeBinding = new NetNamedPipeBinding("Core.NamedPipeClientBinding.ToJobScheduler");
      string schedulerEndpointNetPipe = BusinessLayerSettings.Instance.JobSchedulerEndpointNetPipe;
      JobScheduler.log.DebugFormat("Channel created to {0}", (object) schedulerEndpointNetPipe);
      EndpointAddress remoteAddress = new EndpointAddress(new Uri(schedulerEndpointNetPipe), Array.Empty<AddressHeader>());
      return new ChannelFactory<IJobScheduler>((Binding) namedPipeBinding, remoteAddress);
    }

    private delegate T JobSchedulerOperation<T>();

    internal static class Settings
    {
      public static int RetryCount = 3;
      public static TimeSpan RetryInterval = TimeSpan.FromSeconds(1.0);
    }
  }
}
