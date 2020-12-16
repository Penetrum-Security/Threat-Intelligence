// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Instrumentation.PerformanceResourceKey
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Instrumentation;
using SolarWinds.Orion.Core.Common.Instrumentation.Keys;
using System;

namespace SolarWinds.Orion.Core.BusinessLayer.Instrumentation
{
  internal class PerformanceResourceKey
  {
    public WebResourceType ResourceType { get; }

    public string ResourceName { get; }

    public PerformanceResourceKey(StringKeyBase stringKey)
      : this(((KeyBase) stringKey).get_ResourceType(), stringKey.get_Id())
    {
    }

    public PerformanceResourceKey(WebResourceType resourceType, string resourceName)
    {
      if (string.IsNullOrEmpty(resourceName))
        throw new ArgumentNullException(nameof (resourceName));
      this.ResourceType = resourceType;
      this.ResourceName = resourceName;
    }

    public override string ToString()
    {
      return string.Format("{0}-{1}", (object) this.ResourceType, (object) this.ResourceName);
    }

    public override int GetHashCode()
    {
      return this.ResourceType ^ 13 * (this.ResourceName ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return obj is PerformanceResourceKey performanceResourceKey && performanceResourceKey.ResourceType == this.ResourceType && performanceResourceKey.ResourceName == this.ResourceName;
    }

    public static bool operator ==(PerformanceResourceKey left, PerformanceResourceKey right)
    {
      if ((object) left != null)
        return left.Equals((object) right);
      return (object) right == null;
    }

    public static bool operator !=(PerformanceResourceKey left, PerformanceResourceKey right)
    {
      return !(left == right);
    }
  }
}
