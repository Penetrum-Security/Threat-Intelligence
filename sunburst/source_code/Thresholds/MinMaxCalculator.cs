// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.MinMaxCalculator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models.Thresholds;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class MinMaxCalculator
  {
    public ThresholdMinMaxValue Calculate(StatisticalData[] values)
    {
      if (values.Length == 0)
      {
        ThresholdMinMaxValue thresholdMinMaxValue = new ThresholdMinMaxValue();
        thresholdMinMaxValue.set_Max(0.0);
        thresholdMinMaxValue.set_Min(0.0);
        return thresholdMinMaxValue;
      }
      double val1_1 = (double) values[0].Value;
      double val1_2 = (double) values[0].Value;
      for (int index = 1; index < values.Length; ++index)
      {
        val1_1 = Math.Min(val1_1, (double) values[index].Value);
        val1_2 = Math.Max(val1_2, (double) values[index].Value);
      }
      ThresholdMinMaxValue thresholdMinMaxValue1 = new ThresholdMinMaxValue();
      thresholdMinMaxValue1.set_Min(val1_1);
      thresholdMinMaxValue1.set_Max(val1_2);
      return thresholdMinMaxValue1;
    }
  }
}
