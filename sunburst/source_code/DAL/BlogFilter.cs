// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.BlogFilter
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class BlogFilter : NotificationItemFilter
  {
    public int MaxResults { get; set; }

    public BlogFilter(bool includeAcknowledged, bool includeIgnored, int maxResults)
      : base(includeAcknowledged, includeIgnored)
    {
      this.MaxResults = maxResults;
    }

    public BlogFilter()
      : this(false, false, -1)
    {
    }
  }
}
