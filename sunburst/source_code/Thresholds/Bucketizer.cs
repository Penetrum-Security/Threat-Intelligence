// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.Bucketizer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models.Thresholds;
using SolarWinds.Orion.Core.Common.Thresholds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class Bucketizer
  {
    public Bucket[] CreateBuckets(int bucketsCount, ThresholdMinMaxValue minMax)
    {
      if (bucketsCount == 0 || minMax.get_Max() < minMax.get_Min())
        return new Bucket[0];
      Bucket[] bucketArray = new Bucket[bucketsCount];
      double num = (minMax.get_Max() - minMax.get_Min()) / (double) bucketsCount;
      double maxValue = ThresholdsHelper.RoundThresholdsValue(minMax.get_Min());
      for (int index = 0; index < bucketsCount; ++index)
      {
        double minValue = maxValue;
        maxValue = ThresholdsHelper.RoundThresholdsValue(maxValue + num);
        bucketArray[index] = new Bucket(minValue, maxValue);
      }
      bucketArray[bucketArray.Length - 1].MaxValue = minMax.get_Max();
      double referalMinValue = ThresholdsHelper.RoundThresholdsValue(minMax.get_Min());
      if (!((IEnumerable<Bucket>) bucketArray).All<Bucket>((Func<Bucket, bool>) (bucket => bucket.MinValue.Equals(referalMinValue))))
        return bucketArray;
      return new Bucket[1]
      {
        bucketArray[bucketArray.Length - 1]
      };
    }
  }
}
