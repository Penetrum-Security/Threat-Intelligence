﻿// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache.IPersistentDiscoveryCache
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Discovery;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery.DiscoveryCache
{
  public interface IPersistentDiscoveryCache
  {
    DiscoveryResultItem GetResultForNode(int nodeId);

    void StoreResultForNode(int nodeId, DiscoveryResultItem result);
  }
}