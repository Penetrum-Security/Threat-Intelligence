// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.SwisUriParser
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class SwisUriParser : ISwisUriParser
  {
    public string GetEntityId(string uriStr)
    {
      SwisUri swisUri = SwisUri.Parse(uriStr);
      List<SwisUriFilter> swisUriFilterList1 = new List<SwisUriFilter>();
      swisUriFilterList1.Add(swisUri.get_Filter());
      List<SwisUriFilter> swisUriFilterList2 = swisUriFilterList1;
      for (SwisUriNavigation navigation = swisUri.get_Navigation(); navigation != null; navigation = navigation.get_Navigation())
        swisUriFilterList2.Add(navigation.get_Filter());
      if (((Dictionary<string, SwisUriFilterValue>) ((IEnumerable<SwisUriFilter>) swisUriFilterList2).Last<SwisUriFilter>()).Values.Count > 1)
        throw new InvalidOperationException("GetEntityId does not support multiple key entities");
      return ((IEnumerable<SwisUriFilter>) swisUriFilterList2).SelectMany<SwisUriFilter, SwisUriFilterValue>((Func<SwisUriFilter, IEnumerable<SwisUriFilterValue>>) (uriFilter => (IEnumerable<SwisUriFilterValue>) ((Dictionary<string, SwisUriFilterValue>) uriFilter).Values)).Last<SwisUriFilterValue>().get_Value();
    }
  }
}
