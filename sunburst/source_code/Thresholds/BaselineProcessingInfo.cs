// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.BaselineProcessingInfo
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models.Thresholds;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class BaselineProcessingInfo
  {
    private readonly Threshold _threshold;
    private readonly BaselineValues _baselineValues;

    public BaselineProcessingInfo(Threshold threshold, BaselineValues baselineValues)
    {
      if (threshold == null)
        throw new ArgumentNullException(nameof (threshold));
      if (baselineValues == null)
        throw new ArgumentNullException(nameof (baselineValues));
      this._threshold = threshold;
      this._baselineValues = baselineValues;
    }

    public Threshold Threshold
    {
      get
      {
        return this._threshold;
      }
    }

    public BaselineValues BaselineValues
    {
      get
      {
        return this._baselineValues;
      }
    }
  }
}
