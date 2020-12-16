// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.WebRequestHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.Configuration;
using System;
using System.Net;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class WebRequestHelper
  {
    private static readonly Log _log = new Log();

    internal static HttpWebResponse SendHttpWebRequest(string query)
    {
      HttpWebRequest httpWebRequest = WebRequest.Create(query) as HttpWebRequest;
      httpWebRequest.Proxy = ((IHttpProxySettings) HttpProxySettings.Instance).AsWebProxy();
      httpWebRequest.Method = "GET";
      try
      {
        HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
        if (response.StatusCode == HttpStatusCode.OK)
          return response;
      }
      catch (Exception ex)
      {
        WebRequestHelper._log.ErrorFormat("Caught exception while trying to make http-request: {0}", (object) ex);
      }
      return (HttpWebResponse) null;
    }
  }
}
