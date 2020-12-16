// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TechnologyManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.Catalogs;
using System;
using System.ComponentModel.Composition.Primitives;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class TechnologyManager
  {
    private static readonly Log log = new Log();
    public static readonly string TechonologyMEFPluginAreaID = "Technology";
    private static Lazy<TechnologyManager> cachedLazyInstance = new Lazy<TechnologyManager>(LazyThreadSafetyMode.ExecutionAndPublication);
    private TechnologyFactory techs;
    private TechnologyPollingFactory impls;

    internal TechnologyManager(ComposablePartCatalog catalog)
    {
      this.Initialize(catalog);
    }

    public TechnologyManager()
    {
      using (ComposablePartCatalog catalogForArea = MEFPluginsLoader.get_Instance().GetCatalogForArea(TechnologyManager.TechonologyMEFPluginAreaID))
        this.Initialize(catalogForArea);
    }

    private void Initialize(ComposablePartCatalog catalog)
    {
      this.techs = new TechnologyFactory(catalog);
      this.impls = new TechnologyPollingFactory(catalog);
    }

    public static TechnologyManager Instance
    {
      get
      {
        return TechnologyManager.cachedLazyInstance.Value;
      }
    }

    public TechnologyFactory TechnologyFactory
    {
      get
      {
        return this.techs;
      }
    }

    public TechnologyPollingFactory TechnologyPollingFactory
    {
      get
      {
        return this.impls;
      }
    }
  }
}
