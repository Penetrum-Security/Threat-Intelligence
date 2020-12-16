// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.TraceRoute.TraceRouteProviderSync
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.TraceRoute
{
  public class TraceRouteProviderSync : ITraceRouteProvider
  {
    public static int TTL_MAX = (int) byte.MaxValue;
    public static long TRACEROUTE_TIMEOUT_DEFAULT = 20000;
    public static long TRACEROUTE_TIMEOUT_PERPING_MIN = 2;
    public static int TRACEROUTE_SINGLE_MAX_FACTOR = 2;
    private static Log _log = new Log(nameof (TraceRouteProviderSync));

    public TracerouteResult TraceRoute(string destinationHostNameOrIpAddress)
    {
      TracerouteResult tracerouteResult = new TracerouteResult();
      int hopCount = -1;
      List<TraceRouteResultEntry> returnedResultList;
      string errorMessage;
      tracerouteResult.set_IsSuccess(this.TraceRoute(destinationHostNameOrIpAddress, out hopCount, out returnedResultList, out errorMessage));
      tracerouteResult.set_HopCount(hopCount);
      tracerouteResult.set_NodeList(returnedResultList);
      tracerouteResult.set_ErrorMessage(errorMessage);
      return tracerouteResult;
    }

    public TracerouteResult TraceRoute(
      string destinationHostNameOrIpAddress,
      long maxTimeoutInMilliseconds)
    {
      TracerouteResult tracerouteResult = new TracerouteResult();
      int hopCount = -1;
      List<TraceRouteResultEntry> returnedResultList;
      string errorMessage;
      tracerouteResult.set_IsSuccess(this.TraceRoute(destinationHostNameOrIpAddress, maxTimeoutInMilliseconds, out hopCount, out returnedResultList, out errorMessage));
      tracerouteResult.set_HopCount(hopCount);
      tracerouteResult.set_NodeList(returnedResultList);
      tracerouteResult.set_ErrorMessage(errorMessage);
      return tracerouteResult;
    }

    public bool TraceRoute(
      string destinationHostNameOrIpAddress,
      out int hopCount,
      out List<TraceRouteResultEntry> returnedResultList,
      out string errorMessage)
    {
      return this.TraceRoute(destinationHostNameOrIpAddress, TraceRouteProviderSync.TRACEROUTE_TIMEOUT_DEFAULT, out hopCount, out returnedResultList, out errorMessage);
    }

    public bool TraceRoute(
      string destinationHostNameOrIpAddress,
      long maxTimeoutInMilliseconds,
      out int hopCount,
      out List<TraceRouteResultEntry> returnedResultList,
      out string errorMessage)
    {
      Stopwatch stopWatchTotal = new Stopwatch();
      stopWatchTotal.Start();
      hopCount = -1;
      returnedResultList = (List<TraceRouteResultEntry>) null;
      errorMessage = (string) null;
      using (Ping pingSender = new Ping())
      {
        byte[] buffer = new byte[32];
        PingOptions pingOptions = new PingOptions(TraceRouteProviderSync.TTL_MAX, true);
        PingReply pingReply = pingSender.Send(destinationHostNameOrIpAddress, (int) maxTimeoutInMilliseconds, buffer, pingOptions);
        if (pingReply == null || pingReply.Status == IPStatus.TimedOut)
          return false;
        long timeoutInMilliseconds = pingReply.RoundtripTime * (long) TraceRouteProviderSync.TRACEROUTE_SINGLE_MAX_FACTOR;
        stopWatchTotal.Stop();
        if (stopWatchTotal.ElapsedMilliseconds > maxTimeoutInMilliseconds)
        {
          errorMessage = string.Format("Stop trace route due to timeout {0} (ms). Rount trip to destination {1} takes {2} (ms)", (object) maxTimeoutInMilliseconds, (object) destinationHostNameOrIpAddress, (object) pingReply.RoundtripTime);
          return false;
        }
        if (timeoutInMilliseconds < TraceRouteProviderSync.TRACEROUTE_TIMEOUT_PERPING_MIN)
          timeoutInMilliseconds = TraceRouteProviderSync.TRACEROUTE_TIMEOUT_PERPING_MIN;
        return this.TraceRouteForward(pingSender, destinationHostNameOrIpAddress, maxTimeoutInMilliseconds, timeoutInMilliseconds, buffer, pingOptions, stopWatchTotal, out hopCount, out returnedResultList, out errorMessage);
      }
    }

    public bool TraceRouteForward(
      Ping pingSender,
      string destinationHostNameOrIpAddress,
      long maxTimeoutInMilliseconds,
      long timeoutInMilliseconds,
      byte[] buffer,
      PingOptions pingOptions,
      Stopwatch stopWatchTotal,
      out int destinationTtl,
      out List<TraceRouteResultEntry> returnedResultList,
      out string errorMessage)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = 1;
      Stopwatch stopwatch = new Stopwatch();
      destinationTtl = -1;
      errorMessage = (string) null;
      returnedResultList = new List<TraceRouteResultEntry>();
      string hostName = Dns.GetHostName();
      stringBuilder.AppendLine(string.Format("Start tracing from Source: IP = {0}...", (object) hostName));
      TraceRouteResultEntry routeResultEntry1 = new TraceRouteResultEntry();
      routeResultEntry1.set_HostNameOrIpAddress(hostName);
      routeResultEntry1.set_RoundTripTimeInMilliseconds(0L);
      routeResultEntry1.set_Ttl(0);
      TraceRouteResultEntry routeResultEntry2 = routeResultEntry1;
      returnedResultList.Add(routeResultEntry2);
      bool flag = false;
      stopWatchTotal.Stop();
      while (num <= TraceRouteProviderSync.TTL_MAX && stopWatchTotal.ElapsedMilliseconds < maxTimeoutInMilliseconds)
      {
        pingOptions.Ttl = num;
        int timeout = Math.Min((int) (maxTimeoutInMilliseconds - stopWatchTotal.ElapsedMilliseconds), (int) timeoutInMilliseconds);
        if ((long) timeout < TraceRouteProviderSync.TRACEROUTE_TIMEOUT_PERPING_MIN)
          timeout = (int) TraceRouteProviderSync.TRACEROUTE_TIMEOUT_PERPING_MIN;
        TraceRouteProviderSync._log.DebugFormat("Trace Route Forward Start: destinationIP: {0}, timeOut: {1}, ttl: {2}", (object) destinationHostNameOrIpAddress, (object) timeout, (object) pingOptions.Ttl);
        stopWatchTotal.Start();
        stopwatch.Restart();
        PingReply pingReply = pingSender.Send(destinationHostNameOrIpAddress, timeout, buffer, pingOptions);
        stopwatch.Stop();
        if (pingReply.Status == IPStatus.Success)
        {
          flag = true;
          TraceRouteResultEntry routeResultEntry3 = new TraceRouteResultEntry();
          routeResultEntry3.set_HostNameOrIpAddress(pingReply.Address.ToString());
          routeResultEntry3.set_RoundTripTimeInMilliseconds(pingReply.RoundtripTime);
          routeResultEntry3.set_Ttl(num);
          TraceRouteResultEntry routeResultEntry4 = routeResultEntry3;
          if (routeResultEntry4.get_RoundTripTimeInMilliseconds() == 0L)
            routeResultEntry4.set_RoundTripTimeInMilliseconds(stopwatch.ElapsedMilliseconds);
          returnedResultList.Add(routeResultEntry4);
          stringBuilder.AppendLine(string.Format("Destination: IP = {0}, roundTrip = {1}, TTL = {2};", (object) routeResultEntry4.get_HostNameOrIpAddress(), (object) routeResultEntry4.get_RoundTripTimeInMilliseconds(), (object) num));
          TraceRouteProviderSync._log.DebugFormat("Trace Route Forward End Success: destinationIP: {0}, timeOut: {1}, ttl: {2}", (object) destinationHostNameOrIpAddress, (object) timeout, (object) pingOptions.Ttl);
          break;
        }
        if (pingReply.Status == IPStatus.TtlExpired || pingReply.Status == IPStatus.TimeExceeded)
        {
          TraceRouteResultEntry routeResultEntry3 = new TraceRouteResultEntry();
          routeResultEntry3.set_HostNameOrIpAddress(pingReply.Address.ToString());
          routeResultEntry3.set_RoundTripTimeInMilliseconds(pingReply.RoundtripTime);
          routeResultEntry3.set_Ttl(num);
          TraceRouteResultEntry routeResultEntry4 = routeResultEntry3;
          if (routeResultEntry4.get_RoundTripTimeInMilliseconds() == 0L)
            routeResultEntry4.set_RoundTripTimeInMilliseconds(stopwatch.ElapsedMilliseconds);
          returnedResultList.Add(routeResultEntry4);
          stringBuilder.AppendLine(string.Format("TTL exceeds: IP = {0}, roundTrip = {1}, TTL = {2};", (object) routeResultEntry4.get_HostNameOrIpAddress(), (object) routeResultEntry4.get_RoundTripTimeInMilliseconds(), (object) num));
          TraceRouteProviderSync._log.DebugFormat("Trace Route Forward End {0}: destinationIP: {1}, timeOut: {2}, ttl: {3}", new object[4]
          {
            (object) pingReply.Status,
            (object) destinationHostNameOrIpAddress,
            (object) timeout,
            (object) pingOptions.Ttl
          });
        }
        else if (pingReply.Status == IPStatus.TimedOut)
        {
          stringBuilder.AppendLine(string.Format("No response: IP = {0}, roundTrip = {1}, TTL = {2};", (object) "Unknown", (object) -1, (object) num));
          TraceRouteProviderSync._log.DebugFormat("Trace Route Forward End {0}: destinationIP: {1}, timeOut: {2}, ttl: {3}", new object[4]
          {
            (object) pingReply.Status,
            (object) destinationHostNameOrIpAddress,
            (object) timeout,
            (object) pingOptions.Ttl
          });
        }
        ++num;
        stopWatchTotal.Stop();
      }
      destinationTtl = num;
      if (!flag && stopWatchTotal.ElapsedMilliseconds > maxTimeoutInMilliseconds)
      {
        TraceRouteProviderSync._log.DebugFormat("Trace Route Forward Stop due to exceed max time: destinationIP: {0}, time spent: {1}, ttl: {2}", (object) destinationHostNameOrIpAddress, (object) stopWatchTotal.ElapsedMilliseconds, (object) pingOptions.Ttl);
        stringBuilder.AppendLine(string.Format("Stop trace route due to timeout {0} (ms). Rount trip to destination {1} takes {2} (ms)", (object) maxTimeoutInMilliseconds, (object) destinationHostNameOrIpAddress, (object) (timeoutInMilliseconds / (long) TraceRouteProviderSync.TRACEROUTE_SINGLE_MAX_FACTOR)));
        errorMessage = stringBuilder.ToString();
        return false;
      }
      errorMessage = stringBuilder.ToString();
      return true;
    }

    public int EstimateHopCount(int responseTtl)
    {
      if (responseTtl >= 128)
        return (int) byte.MaxValue - responseTtl + 1;
      if (responseTtl >= 64)
        return 128 - responseTtl + 1;
      if (responseTtl >= 60 || responseTtl >= 32)
        return 64 - responseTtl + 1;
      return 64 - responseTtl + 1;
    }
  }
}
