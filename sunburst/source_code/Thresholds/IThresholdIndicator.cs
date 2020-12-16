// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.IThresholdIndicator
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models.Thresholds;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  public interface IThresholdIndicator
  {
    void LoadPreviousThresholdData(int instanceId, string thresholdName);

    void ReportThresholdNotification(Threshold threshold);
  }
}
