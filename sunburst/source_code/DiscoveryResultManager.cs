// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DiscoveryResultManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using SolarWinds.Orion.Core.Models.Discovery;
using SolarWinds.Orion.Discovery.Contract.DiscoveryPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public static class DiscoveryResultManager
  {
    private static readonly Log log = new Log();

    public static DiscoveryResultBase GetDiscoveryResult(
      int profileId,
      IList<IDiscoveryPlugin> discoveryPlugins)
    {
      if (discoveryPlugins == null)
        throw new ArgumentNullException(nameof (discoveryPlugins));
      if (profileId <= 0)
        throw new ArgumentException(string.Format("Invalid profile ID [{0}]", (object) profileId));
      DiscoveryResultBase discoveryResultBase = new DiscoveryResultBase();
      try
      {
        DiscoveryProfileEntry profileById = DiscoveryProfileEntry.GetProfileByID(profileId);
        discoveryResultBase.set_EngineId(profileById.get_EngineID());
        discoveryResultBase.set_ProfileID(profileById.get_ProfileID());
      }
      catch (Exception ex)
      {
        string message = string.Format("Unable to load profile {0}", (object) profileId);
        DiscoveryResultManager.log.Error((object) message, ex);
        throw new Exception(message, ex);
      }
      if (((ICollection<IDiscoveryPlugin>) discoveryPlugins).Count == 0)
        return discoveryResultBase;
      int millisecondsTimeout = 300000;
      bool flag = Environment.StackTrace.Contains("ServiceModel");
      if (flag)
      {
        try
        {
          System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
          XmlDocument xmlDocument = new XmlDocument();
          xmlDocument.Load(configuration.FilePath);
          XmlNode xmlNode = xmlDocument.SelectSingleNode("/configuration/system.serviceModel/bindings/netTcpBinding/binding[@name=\"Core.NetTcpBinding\"]");
          if (xmlNode != null)
          {
            if (xmlNode.Attributes != null)
              millisecondsTimeout = (int) TimeSpan.Parse(xmlNode.Attributes["receiveTimeout"].Value).TotalMilliseconds;
          }
        }
        catch (Exception ex)
        {
          DiscoveryResultManager.log.Warn((object) "Unable to read WCF timeout from Config file.");
        }
      }
      Thread thread = new Thread(new ParameterizedThreadStart(DiscoveryResultManager.LoadResults));
      DiscoveryResultManager.LoadResultsArgs loadResultsArgs = new DiscoveryResultManager.LoadResultsArgs()
      {
        discoveryPlugins = discoveryPlugins,
        profileId = profileId,
        result = discoveryResultBase
      };
      thread.Start((object) loadResultsArgs);
      if (flag)
      {
        if (!thread.Join(millisecondsTimeout))
        {
          DiscoveryResultManager.log.Error((object) "Loading results takes more time than WCF timeout is set. Enable debug logging to see which plugin takes too long.");
          return discoveryResultBase;
        }
      }
      else
        thread.Join();
      DiscoveryResultBase result = loadResultsArgs.result;
      DiscoveryFilterResultByTechnology.FilterByPriority(result, TechnologyManager.Instance);
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<DiscoveryPluginResultBase> list = ((IEnumerable<DiscoveryPluginResultBase>) result.get_PluginResults()).ToList<DiscoveryPluginResultBase>();
      result.get_PluginResults().Clear();
      using (List<DiscoveryPluginResultBase>.Enumerator enumerator = list.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          DiscoveryPluginResultBase current = enumerator.Current;
          result.get_PluginResults().Add(current.GetFilteredPluginResult());
        }
      }
      DiscoveryResultManager.log.DebugFormat("Filtering results took {0} milliseconds.", (object) stopwatch.ElapsedMilliseconds);
      GC.Collect();
      return result;
    }

    private static void LoadResults(object args)
    {
      DiscoveryResultManager.LoadResultsArgs loadResultsArgs = (DiscoveryResultManager.LoadResultsArgs) args;
      Stopwatch stopwatch = new Stopwatch();
      using (IEnumerator<IDiscoveryPlugin> enumerator = ((IEnumerable<IDiscoveryPlugin>) loadResultsArgs.discoveryPlugins).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IDiscoveryPlugin current = enumerator.Current;
          stopwatch.Restart();
          DiscoveryResultManager.log.DebugFormat("Loading results from plugin {0}", (object) ((object) current).GetType());
          DiscoveryPluginResultBase pluginResultBase = current.LoadResults(loadResultsArgs.profileId);
          DiscoveryResultManager.log.DebugFormat("Loading results from plugin {0} took {1} milliseconds.", (object) ((object) current).GetType(), (object) stopwatch.ElapsedMilliseconds);
          if (pluginResultBase == null)
            throw new Exception(string.Format("unable to get valid result for plugin {0}", (object) ((object) current).GetType()));
          pluginResultBase.set_PluginTypeName(((object) current).GetType().FullName);
          loadResultsArgs.result.get_PluginResults().Add(pluginResultBase);
        }
      }
    }

    private static string GetFilename(Type type)
    {
      return string.Format("C:\\{1}{0}.dat", (object) Guid.NewGuid(), (object) type);
    }

    private static void XmlSerializer(DiscoveryResultBase data)
    {
      Type type = typeof (DiscoveryResultBase);
      using (FileStream fileStream = new FileStream(DiscoveryResultManager.GetFilename(type), FileMode.Create))
      {
        new DataContractSerializer(type).WriteObject(XmlDictionaryWriter.CreateTextWriter((Stream) fileStream), (object) data);
        fileStream.Flush();
        fileStream.Close();
      }
    }

    private static void BinarySerializer(DiscoveryResultBase data)
    {
      FileStream fileStream = new FileStream(DiscoveryResultManager.GetFilename(typeof (DiscoveryResultBase)), FileMode.Create);
      BinaryFormatter binaryFormatter = new BinaryFormatter();
      try
      {
        binaryFormatter.Serialize((Stream) fileStream, (object) data);
      }
      finally
      {
        fileStream.Close();
      }
    }

    private class LoadResultsArgs
    {
      public int profileId;
      public IList<IDiscoveryPlugin> discoveryPlugins;
      public DiscoveryResultBase result;
    }
  }
}
