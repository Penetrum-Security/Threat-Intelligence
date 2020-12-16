// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionFeatureResolver
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Models.OrionFeature;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class OrionFeatureResolver
  {
    private static readonly Log log = new Log();
    private readonly IOrionFeaturesDAL dal;
    private readonly IOrionFearureProviderFactory providerFactory;

    public OrionFeatureResolver(IOrionFeaturesDAL dal, IOrionFearureProviderFactory providerFactory)
    {
      if (dal == null)
        throw new ArgumentNullException(nameof (dal));
      if (providerFactory == null)
        throw new ArgumentNullException(nameof (providerFactory));
      this.dal = dal;
      this.providerFactory = providerFactory;
    }

    public IEnumerable<IOrionFeatureProvider> GetProviders()
    {
      return this.providerFactory.GetProviders();
    }

    public void Resolve()
    {
      using (OrionFeatureResolver.log.Block())
        this.dal.Update(this.GetProviders().SelectMany<IOrionFeatureProvider, SolarWinds.Orion.Core.Models.OrionFeature.OrionFeature>((Func<IOrionFeatureProvider, IEnumerable<SolarWinds.Orion.Core.Models.OrionFeature.OrionFeature>>) (n => n.GetFeatures())));
    }

    internal void Resolve(string providerName)
    {
      if (string.IsNullOrEmpty(providerName))
        throw new ArgumentNullException(nameof (providerName));
      this.Resolve();
    }
  }
}
