// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory.PluginsFactory`1
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Catalogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.BackgroundInventory
{
  internal sealed class PluginsFactory<T>
  {
    private static readonly Log log = new Log();

    public PluginsFactory()
      : this(OrionConfiguration.get_InstallPath(), "*.Pollers.dll", "*.Collector.dll", "*.Plugin.dll")
    {
    }

    public PluginsFactory(string directory, params string[] filePatterns)
    {
      if (string.IsNullOrEmpty(directory))
        throw new ArgumentNullException(nameof (directory));
      this.Plugins = new List<T>();
      foreach (string filePattern in filePatterns)
      {
        if (Directory.Exists(directory))
        {
          this.Process(Directory.EnumerateFiles(directory, filePattern, SearchOption.TopDirectoryOnly));
          foreach (string enumerateDirectory in Directory.EnumerateDirectories(directory))
            this.Process(Directory.EnumerateFiles(enumerateDirectory, filePattern, SearchOption.TopDirectoryOnly));
        }
      }
    }

    public List<T> Plugins { get; private set; }

    private void Process(IEnumerable<string> files)
    {
      IEnumerable<PluginsFactory<T>.AssemblyVersionInfo> assemblyVersionInfos = files.Select<string, PluginsFactory<T>.AssemblyVersionInfo>(new Func<string, PluginsFactory<T>.AssemblyVersionInfo>(PluginsFactory<T>.AssemblyVersionInfo.Create)).OrderByDescending<PluginsFactory<T>.AssemblyVersionInfo, PluginsFactory<T>.AssemblyVersionInfo>((Func<PluginsFactory<T>.AssemblyVersionInfo, PluginsFactory<T>.AssemblyVersionInfo>) (name => name)).Distinct<PluginsFactory<T>.AssemblyVersionInfo>();
      string basePath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
      Dictionary<string, FileInfo> dictionary = new DirectoryXmlFilePluginsProvider(OrionConfiguration.get_InstallPath()).ReadAreas().Where<PluginsArea>((Func<PluginsArea, bool>) (n => n.get_Name().Equals("BackgroundInventory"))).SelectMany<PluginsArea, Plugin>((Func<PluginsArea, IEnumerable<Plugin>>) (area => (IEnumerable<Plugin>) area.get_Plugins())).Select<Plugin, FileInfo>((Func<Plugin, FileInfo>) (plugin => new FileInfo(Path.Combine(basePath, plugin.get_AssemblyPath())))).Where<FileInfo>((Func<FileInfo, bool>) (info => info.Exists)).ToDictionary<FileInfo, string>((Func<FileInfo, string>) (info => info.Name), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      List<PluginsFactory<T>.AssemblyVersionInfo> assemblyVersionInfoList = new List<PluginsFactory<T>.AssemblyVersionInfo>();
      foreach (PluginsFactory<T>.AssemblyVersionInfo assemblyVersionInfo1 in assemblyVersionInfos)
      {
        PluginsFactory<T>.AssemblyVersionInfo assemblyVersionInfo2 = assemblyVersionInfo1;
        try
        {
          FileInfo fileInfo;
          if (dictionary.TryGetValue(assemblyVersionInfo2.FileInfo.Name, out fileInfo))
            assemblyVersionInfo2 = new PluginsFactory<T>.AssemblyVersionInfo(fileInfo);
          assemblyVersionInfoList.Add(assemblyVersionInfo2);
        }
        catch (Exception ex)
        {
          PluginsFactory<T>.log.ErrorFormat("Failed to process {0} @ '{1}'. {2}", (object) assemblyVersionInfo2.AssemblyName, (object) assemblyVersionInfo2.FileInfo, (object) ex);
        }
      }
      ResolveEventHandler resolveEventHandler = new ResolveEventHandler(PluginsFactory<T>.CurrentDomain_ReflectionOnlyAssemblyResolve);
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;
      try
      {
        foreach (PluginsFactory<T>.AssemblyVersionInfo assemblyVersionInfo in assemblyVersionInfoList)
        {
          try
          {
            this.ProcessAssembly(assemblyVersionInfo.FileInfo.FullName);
          }
          catch (Exception ex)
          {
            PluginsFactory<T>.log.WarnFormat("Failed to initialize {0} @ '{1}'. {2}", (object) assemblyVersionInfo.AssemblyName, (object) assemblyVersionInfo.FileInfo, (object) ex);
          }
        }
      }
      finally
      {
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
      }
    }

    private static string FormatCodeBase(Assembly assembly)
    {
      string codeBase = PluginsFactory<T>.GetCodeBase(assembly);
      return !string.IsNullOrWhiteSpace(codeBase) ? " at '" + codeBase + (object) '\'' : codeBase;
    }

    private static string GetCodeBase(Assembly assembly)
    {
      if (assembly == (Assembly) null)
        return string.Empty;
      string codeBase = assembly.CodeBase;
      if (string.IsNullOrEmpty(codeBase))
        return string.Empty;
      try
      {
        Uri uri = new Uri(codeBase);
        if (uri.IsAbsoluteUri)
          return uri.LocalPath;
      }
      catch (Exception ex)
      {
        GC.KeepAlive((object) ex);
      }
      return codeBase;
    }

    private static string FormatRequest(Assembly requesting)
    {
      if (!(requesting != (Assembly) null))
        return string.Empty;
      return " requested by [" + (object) requesting + "]" + PluginsFactory<T>.FormatCodeBase(requesting);
    }

    private static void ReportReflectionLoad(
      AssemblyName assmName,
      Assembly assembly,
      Assembly requesting)
    {
      if (PluginsFactory<T>.log.get_IsWarnEnabled() && !StringComparer.Ordinal.Equals(assmName.FullName, assembly.FullName))
      {
        PluginsFactory<T>.log.WarnFormat("inspecting [{0}] as [{1}]{2}{3}", new object[4]
        {
          (object) assmName,
          (object) assembly,
          (object) PluginsFactory<T>.FormatCodeBase(assembly),
          (object) PluginsFactory<T>.FormatRequest(requesting)
        });
      }
      else
      {
        if (!PluginsFactory<T>.log.get_IsDebugEnabled())
          return;
        PluginsFactory<T>.log.DebugFormat("inspecting [{0}] as [{1}]{2}{3}", new object[4]
        {
          (object) assmName,
          (object) assembly,
          (object) PluginsFactory<T>.FormatCodeBase(assembly),
          (object) PluginsFactory<T>.FormatRequest(requesting)
        });
      }
    }

    private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(
      object sender,
      ResolveEventArgs args)
    {
      AssemblyName assemblyName1 = new AssemblyName(args.Name);
      Assembly requestingAssembly = args.RequestingAssembly;
      List<Exception> exceptionList = new List<Exception>();
      try
      {
        Assembly assembly = Assembly.ReflectionOnlyLoad(args.Name);
        PluginsFactory<T>.ReportReflectionLoad(assemblyName1, assembly, requestingAssembly);
        return assembly;
      }
      catch (Exception ex)
      {
        exceptionList.Add(ex);
      }
      foreach (Assembly assembly in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
      {
        if (AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName1))
        {
          PluginsFactory<T>.ReportReflectionLoad(assemblyName1, assembly, requestingAssembly);
          return assembly;
        }
      }
      if (requestingAssembly != (Assembly) null)
      {
        try
        {
          Uri uri = new Uri(requestingAssembly.CodeBase);
          if (uri.IsAbsoluteUri)
          {
            string str = Path.Combine(Path.GetDirectoryName(uri.LocalPath) ?? string.Empty, assemblyName1.Name + ".dll");
            if (File.Exists(str))
            {
              AssemblyName assemblyName2 = AssemblyName.GetAssemblyName(str);
              if (AssemblyName.ReferenceMatchesDefinition(assemblyName1, assemblyName2))
              {
                Assembly assembly = Assembly.ReflectionOnlyLoadFrom(str);
                PluginsFactory<T>.ReportReflectionLoad(assemblyName1, assembly, requestingAssembly);
                return assembly;
              }
            }
          }
        }
        catch (Exception ex)
        {
          exceptionList.Add(ex);
        }
      }
      try
      {
        foreach (Assembly assembly1 in AppDomain.CurrentDomain.GetAssemblies())
        {
          AssemblyName name = assembly1.GetName();
          if (AssemblyName.ReferenceMatchesDefinition(name, assemblyName1))
          {
            Uri uri = new Uri(name.CodeBase);
            if (uri.IsAbsoluteUri)
            {
              Assembly assembly2 = Assembly.ReflectionOnlyLoadFrom(uri.LocalPath);
              PluginsFactory<T>.ReportReflectionLoad(assemblyName1, assembly2, requestingAssembly);
              return assembly2;
            }
          }
        }
      }
      catch (Exception ex)
      {
        exceptionList.Add(ex);
      }
      if (!PluginsFactory<T>.log.get_IsErrorEnabled())
        return (Assembly) null;
      AggregateException aggregateException = new AggregateException(new StringBuilder("inspecting [").Append(args.Name).Append(']').Append(PluginsFactory<T>.FormatRequest(requestingAssembly)).Append('.').ToString(), (IEnumerable<Exception>) exceptionList);
      PluginsFactory<T>.log.WarnFormat("{0}", (object) aggregateException);
      return (Assembly) null;
    }

    private void ProcessAssembly(string fileName)
    {
      PluginsFactory<T>.log.DebugFormat("Loading plugins from {0}", (object) fileName);
      AssemblyName assemblyName = AssemblyName.GetAssemblyName(fileName);
      Assembly assembly1 = (Assembly) null;
      foreach (Assembly assembly2 in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
      {
        if (AssemblyName.ReferenceMatchesDefinition(assembly2.GetName(), assemblyName))
        {
          assembly1 = assembly2;
          try
          {
            string fullPath = Path.GetFullPath(new Uri(assembly1.CodeBase).LocalPath);
            if (!StringComparer.OrdinalIgnoreCase.Equals(fileName, fullPath))
            {
              PluginsFactory<T>.log.WarnFormat("inspecting [{0}] at '{1}' as [{2}] at '{3}'", new object[4]
              {
                (object) assemblyName,
                (object) fileName,
                (object) assembly1,
                (object) fullPath
              });
              break;
            }
            break;
          }
          catch (Exception ex)
          {
            PluginsFactory<T>.log.WarnFormat("inspecting [{0}] at '{1}' as [{2}]. {3}", new object[4]
            {
              (object) assemblyName,
              (object) fileName,
              (object) assembly1,
              (object) ex
            });
            break;
          }
        }
      }
      if (assembly1 == (Assembly) null)
        assembly1 = Assembly.ReflectionOnlyLoadFrom(fileName);
      List<Type> derivedTypes = PluginsFactory<T>.FindDerivedTypes<T>(assembly1);
      if (derivedTypes.Count != 0)
        derivedTypes = PluginsFactory<T>.FindDerivedTypes<T>(Assembly.LoadFrom(fileName));
      foreach (Type type in derivedTypes)
      {
        PluginsFactory<T>.log.DebugFormat("Creating plugin for {0}", (object) type);
        T instance = (T) Activator.CreateInstance(type);
        this.Plugins.Add(instance);
        string empty = string.Empty;
        PropertyInfo property = instance.GetType().GetProperty("FlagName");
        if (property != (PropertyInfo) null)
          empty = (string) property.GetValue((object) instance, (object[]) null);
        if (PluginsFactory<T>.log.get_IsInfoEnabled())
          PluginsFactory<T>.log.InfoFormat("Loaded plugin {0} for {1} from {2}", (object) type, (object) empty, (object) fileName);
      }
    }

    private static List<Type> FindDerivedTypes<K>(Assembly assembly)
    {
      Type[] types;
      try
      {
        types = assembly.GetTypes();
      }
      catch (ReflectionTypeLoadException ex)
      {
        types = ex.Types;
      }
      return ((IEnumerable<Type>) types).Where<Type>((Func<Type, bool>) (n => n != (Type) null && !n.IsAbstract && !n.IsInterface && n.GetInterface(typeof (K).Name) != (Type) null)).ToList<Type>();
    }

    [DebuggerDisplay("{ToString(),nq}")]
    private sealed class AssemblyVersionInfo : IComparable<PluginsFactory<T>.AssemblyVersionInfo>, IEquatable<PluginsFactory<T>.AssemblyVersionInfo>
    {
      internal AssemblyName AssemblyName { get; private set; }

      internal Version FileVersion { get; private set; }

      internal FileInfo FileInfo { get; private set; }

      internal static PluginsFactory<T>.AssemblyVersionInfo Create(string fileName)
      {
        return new PluginsFactory<T>.AssemblyVersionInfo(fileName);
      }

      internal AssemblyVersionInfo(FileInfo fileInfo)
      {
        this.FileInfo = fileInfo;
        this.AssemblyName = AssemblyName.GetAssemblyName(this.FileInfo.FullName);
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(this.FileInfo.FullName);
        this.FileVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
      }

      internal AssemblyVersionInfo(string fileName)
        : this(new FileInfo(Path.GetFullPath(fileName)))
      {
      }

      public int CompareTo(PluginsFactory<T>.AssemblyVersionInfo other)
      {
        int num = this.AssemblyName.Version.CompareTo(other.AssemblyName.Version);
        if (num == 0)
          num = this.FileVersion.CompareTo(other.FileVersion);
        return num;
      }

      public bool Equals(PluginsFactory<T>.AssemblyVersionInfo other)
      {
        return StringComparer.OrdinalIgnoreCase.Equals(this.AssemblyName.Name, other.AssemblyName.Name);
      }

      public override string ToString()
      {
        return this.AssemblyName.ToString() + " @ " + (object) this.FileInfo + (object) ':' + (object) this.FileVersion;
      }
    }
  }
}
