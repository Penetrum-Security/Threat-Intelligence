// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Thresholds.ThresholdProcessingManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Threading;
using SolarWinds.Orion.Core.Common.Catalogs;
using SolarWinds.Orion.Core.Common.Settings;
using SolarWinds.Orion.Core.Common.Thresholds;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer.Thresholds
{
  internal class ThresholdProcessingManager
  {
    private static readonly LazyWithoutExceptionCache<ThresholdProcessingManager> _instance = new LazyWithoutExceptionCache<ThresholdProcessingManager>((Func<ThresholdProcessingManager>) (() =>
    {
      using (ComposablePartCatalog catalogForArea = MEFPluginsLoader.get_Instance().GetCatalogForArea("Thresholds"))
        return new ThresholdProcessingManager(catalogForArea, CollectorSettings.get_Instance());
    }));
    [ImportMany(typeof (IThresholdDataProcessor))]
    private IEnumerable<IThresholdDataProcessor> _thresholdProcessors = Enumerable.Empty<IThresholdDataProcessor>();
    [ImportMany(typeof (ThresholdDataProvider))]
    private IEnumerable<ThresholdDataProvider> _thresholdDataProviders = Enumerable.Empty<ThresholdDataProvider>();
    private readonly ThresholdProcessingEngine _engine;

    internal ThresholdProcessingManager(ComposablePartCatalog catalog, ICollectorSettings settings)
    {
      this.ComposeParts(catalog);
      this._engine = new ThresholdProcessingEngine(this._thresholdProcessors, this._thresholdDataProviders, (IThresholdIndicator) new ThresholdIndicator(), settings)
      {
        BatchSize = BusinessLayerSettings.Instance.ThresholdsProcessingBatchSize,
        BaselineTimeFrame = BusinessLayerSettings.Instance.ThresholdsProcessingDefaultTimeFrame
      };
    }

    private void ComposeParts(ComposablePartCatalog catalog)
    {
      using (CompositionContainer container = new CompositionContainer(catalog, Array.Empty<ExportProvider>()))
        container.ComposeParts((object) this);
    }

    public static ThresholdProcessingManager Instance
    {
      get
      {
        return ThresholdProcessingManager._instance.get_Value();
      }
    }

    internal static CompositionContainer CompositionContainer { get; set; }

    public ThresholdProcessingEngine Engine
    {
      get
      {
        return this._engine;
      }
    }
  }
}
