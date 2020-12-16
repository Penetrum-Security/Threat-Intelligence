// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TechnologyFactory
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class TechnologyFactory
  {
    private static readonly Log log = new Log();
    private Dictionary<string, ITechnology> items;

    public TechnologyFactory(ComposablePartCatalog catalog)
    {
      this.items = this.InitializeMEF(catalog).ToDictionary<ITechnology, string>((Func<ITechnology, string>) (n => n.get_TechnologyID()));
      if (((IEnumerable<KeyValuePair<string, ITechnology>>) this.items).Any<KeyValuePair<string, ITechnology>>())
        TechnologyFactory.log.Info((object) ("Technology loader found technologies: " + string.Join(",", ((IEnumerable<ITechnology>) this.items.Values).Select<ITechnology, string>((Func<ITechnology, string>) (t => t.get_TechnologyID())).ToArray<string>())));
      else
        TechnologyFactory.log.Error((object) "Technology loader found 0 technologies");
    }

    protected IEnumerable<ITechnology> InitializeMEF(
      ComposablePartCatalog catalog)
    {
      using (CompositionContainer compositionContainer = new CompositionContainer(catalog, Array.Empty<ExportProvider>()))
        return (IEnumerable<ITechnology>) compositionContainer.GetExports<ITechnology>().Select<Lazy<ITechnology>, ITechnology>((Func<Lazy<ITechnology>, ITechnology>) (n => n.Value)).ToList<ITechnology>();
    }

    public IEnumerable<ITechnology> Items()
    {
      return (IEnumerable<ITechnology>) this.items.Values;
    }

    public ITechnology GetTechnology(string technologyID)
    {
      if (string.IsNullOrEmpty(technologyID))
        throw new ArgumentNullException(nameof (technologyID));
      ITechnology itechnology = (ITechnology) null;
      if (!this.items.TryGetValue(technologyID, out itechnology))
        throw new KeyNotFoundException(technologyID);
      return itechnology;
    }
  }
}
