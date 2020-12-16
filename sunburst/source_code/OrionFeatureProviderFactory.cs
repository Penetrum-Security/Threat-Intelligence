// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionFeatureProviderFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Catalogs;
using SolarWinds.Orion.Core.Models.OrionFeature;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class OrionFeatureProviderFactory : IOrionFearureProviderFactory
  {
    [ImportMany(typeof (IOrionFeatureProvider))]
    private IEnumerable<IOrionFeatureProvider> _providers = Enumerable.Empty<IOrionFeatureProvider>();

    public static OrionFeatureProviderFactory CreateInstance()
    {
      using (ComposablePartCatalog catalogForArea = MEFPluginsLoader.get_Instance().GetCatalogForArea("OrionFeature"))
        return new OrionFeatureProviderFactory(catalogForArea);
    }

    public OrionFeatureProviderFactory(ComposablePartCatalog catalog)
    {
      if (catalog == null)
        throw new ArgumentNullException(nameof (catalog));
      using (CompositionContainer container = new CompositionContainer(catalog, Array.Empty<ExportProvider>()))
        container.ComposeParts((object) this);
    }

    public IEnumerable<IOrionFeatureProvider> GetProviders()
    {
      return this._providers;
    }
  }
}
