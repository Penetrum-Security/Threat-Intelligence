// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OneTimeJobs.IOneTimeJobService
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.JobEngine;
using SolarWinds.JobEngine.Security;
using SolarWinds.Orion.Core.BusinessLayer.BL;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.OneTimeJobs
{
  public interface IOneTimeJobService : IDisposable
  {
    void Start(string listenerUri);

    OneTimeJobResult<T> ExecuteJobAndGetResult<T>(
      int engineId,
      JobDescription jobDescription,
      CredentialBase jobCredential,
      JobResultDataFormatType resultDataFormat,
      string jobType)
      where T : class, new();

    OneTimeJobResult<T> ExecuteJobAndGetResult<T>(
      string engineName,
      JobDescription jobDescription,
      CredentialBase jobCredential,
      JobResultDataFormatType resultDataFormat,
      string jobType)
      where T : class, new();
  }
}
