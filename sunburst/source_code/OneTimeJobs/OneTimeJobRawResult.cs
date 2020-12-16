// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OneTimeJobs.OneTimeJobRawResult
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using System;
using System.IO;

namespace SolarWinds.Orion.Core.BusinessLayer.OneTimeJobs
{
  public struct OneTimeJobRawResult : IDisposable
  {
    public bool Success { get; set; }

    public string Error { get; set; }

    public Stream JobResultStream { get; set; }

    public Exception ExceptionFromJob { get; set; }

    public void Dispose()
    {
      this.JobResultStream?.Dispose();
    }
  }
}
