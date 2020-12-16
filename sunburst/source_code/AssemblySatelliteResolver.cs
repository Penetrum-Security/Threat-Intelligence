// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.AssemblySatelliteResolver
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common.i18n;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal static class AssemblySatelliteResolver
  {
    private static readonly Log log = new Log();
    private static readonly ConcurrentDictionary<string, AssemblySatelliteResolver.IntRef> ResolveCnt = new ConcurrentDictionary<string, AssemblySatelliteResolver.IntRef>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, AssemblySatelliteResolver.IntRef> EntryCount = new ConcurrentDictionary<string, AssemblySatelliteResolver.IntRef>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static readonly char[] PathWhiteSpace = new char[8]
    {
      '\t',
      '\n',
      '\v',
      '\f',
      '\r',
      ' ',
      '\x0085',
      ' '
    };

    internal static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
    {
      try
      {
        AssemblyName assemblyName = new AssemblyName(args.Name);
        if (!assemblyName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
          return (Assembly) null;
        bool isLog = AssemblySatelliteResolver.EntryCount.GetOrAdd(assemblyName.FullName, (Func<string, AssemblySatelliteResolver.IntRef>) (_ => new AssemblySatelliteResolver.IntRef(0))).Increment() > 2 && !AssemblySatelliteResolver.ResolveCnt.TryGetValue(assemblyName.FullName, out AssemblySatelliteResolver.IntRef _);
        if (isLog)
          AssemblySatelliteResolver.log.DebugFormat("Resolving satellite assembly \"{0}\"", (object) assemblyName);
        string[] strArray = assemblyName.Name.Split('.');
        string sourceName = string.Join(".", ((IEnumerable<string>) strArray).Take<string>(strArray.Length - 1));
        string resolveFileName = assemblyName.Name + ".dll";
        List<Assembly> assemblyList = new List<Assembly>((IEnumerable<Assembly>) AppDomain.CurrentDomain.GetAssemblies());
        Assembly requestingAssembly = args.RequestingAssembly;
        if (requestingAssembly != (Assembly) null)
        {
          assemblyList.Remove(requestingAssembly);
          assemblyList.Insert(0, requestingAssembly);
        }
        foreach (Assembly assembly in assemblyList)
        {
          AssemblyName name = assembly.GetName();
          if (AssemblySatelliteResolver.SatelliteMatchesDefinition(assemblyName, name))
          {
            bool flag = AssemblySatelliteResolver.ResolveCnt.GetOrAdd(assembly.FullName, (Func<string, AssemblySatelliteResolver.IntRef>) (_ => new AssemblySatelliteResolver.IntRef(0))).Increment() == 1;
            if (isLog | flag)
              AssemblySatelliteResolver.log.InfoFormat("Resolved \"{0}\" as \"{1}\" at \"{2}\"", (object) assemblyName, (object) name, (object) AssemblySatelliteResolver.GetSymbolicLocation(assembly));
            return assembly;
          }
        }
        Assembly resolvedAssembly;
        if (AssemblySatelliteResolver.ProbeViaLoadedAssemblies((ICollection<Assembly>) assemblyList, assemblyName, resolveFileName, requestingAssembly, sourceName, isLog, out resolvedAssembly))
          return resolvedAssembly;
      }
      catch (Exception ex)
      {
        Trace.TraceError("AssemblyResove failed. {0}", (object) ex);
        AssemblySatelliteResolver.log.FatalFormat("AssemblyResove failed. {0}", (object) ex);
        GC.KeepAlive((object) ex);
      }
      return (Assembly) null;
    }

    internal static void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
      if (!AssemblySatelliteResolver.log.get_IsErrorEnabled())
        return;
      try
      {
        Assembly loadedAssembly = args.LoadedAssembly;
        AssemblyName name = loadedAssembly.GetName();
        StringBuilder stringBuilder1 = new StringBuilder();
        int num = 1;
        bool flag1 = name.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase);
        bool flag2 = name.Name.IndexOf("Auditing", StringComparison.OrdinalIgnoreCase) > -1;
        bool flag3 = name.Name.IndexOf(".Strings", StringComparison.OrdinalIgnoreCase) > -1;
        if (flag1)
        {
          AssemblySatelliteResolver.IntRef intRef;
          if (!AssemblySatelliteResolver.EntryCount.TryGetValue(name.FullName, out intRef) && !AssemblySatelliteResolver.ResolveCnt.TryGetValue(name.FullName, out intRef))
            num = 2;
          string[] strArray = name.Name.Split('.');
          string sourceName = string.Join(".", ((IEnumerable<string>) strArray).Take<string>(strArray.Length - 1));
          Assembly assembly1 = ((IEnumerable<Assembly>) AppDomain.CurrentDomain.GetAssemblies()).FirstOrDefault<Assembly>((Func<Assembly, bool>) (a => a.GetName().Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase)));
          if (assembly1 != (Assembly) null)
          {
            Assembly assembly2 = (Assembly) null;
            try
            {
              assembly2 = assembly1.GetSatelliteAssembly(name.CultureInfo);
            }
            catch (FileNotFoundException ex)
            {
            }
            catch (FileLoadException ex)
            {
            }
            if (assembly2 == (Assembly) null || assembly2 != loadedAssembly)
            {
              stringBuilder1.AppendLine().AppendFormat("Unexpected satellite for \"{0}\" : \"{1}\"", (object) loadedAssembly, (object) name.CultureInfo.Name);
              num = 3;
            }
          }
        }
        if (!(flag1 | flag2 | flag3))
          return;
        StringBuilder stringBuilder2 = new StringBuilder().AppendFormat("Loaded \"{0}\" at \"{1}\".{2}", (object) name, (object) AssemblySatelliteResolver.GetSymbolicLocation(loadedAssembly), AssemblySatelliteResolver.GetDebugStackTrace()).Append((object) stringBuilder1);
        switch (num)
        {
          case 1:
            AssemblySatelliteResolver.log.Info((object) stringBuilder2);
            break;
          case 2:
            AssemblySatelliteResolver.log.Warn((object) stringBuilder2);
            break;
          case 3:
            AssemblySatelliteResolver.log.Error((object) stringBuilder2);
            break;
        }
      }
      catch (Exception ex)
      {
        Trace.TraceError("AssemblyLoad failed. {0}", (object) ex);
        AssemblySatelliteResolver.log.FatalFormat("AssemblyLoad failed. {0}", (object) ex);
        GC.KeepAlive((object) ex);
      }
    }

    internal static bool SatelliteMatchesDefinition(AssemblyName reference, AssemblyName comparee)
    {
      if (!AssemblyName.ReferenceMatchesDefinition(reference, comparee) || reference.CultureInfo == null != (comparee.CultureInfo == null))
        return false;
      if (reference.CultureInfo != null && comparee.CultureInfo != null && !reference.CultureInfo.Equals((object) comparee.CultureInfo))
      {
        bool isNeutralCulture1 = reference.CultureInfo.IsNeutralCulture;
        bool isNeutralCulture2 = comparee.CultureInfo.IsNeutralCulture;
        if (isNeutralCulture1 & isNeutralCulture2 || !(isNeutralCulture1 ? (object) reference.CultureInfo : (object) reference.CultureInfo.Parent).Equals(isNeutralCulture2 ? (object) comparee.CultureInfo : (object) comparee.CultureInfo.Parent))
          return false;
      }
      return true;
    }

    private static string NormalizePath(string path)
    {
      if (string.IsNullOrEmpty(path))
        return string.Empty;
      return path.Trim(AssemblySatelliteResolver.PathWhiteSpace).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar).ToUpperInvariant();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object GetDebugStackTrace()
    {
      return AssemblySatelliteResolver.log.get_IsDebugEnabled() ? (object) (Environment.NewLine + (object) new StackTrace(1, false)) : (object) string.Empty;
    }

    private static IEnumerable<Uri> GetAssemblyBaseUris(Assembly assembly)
    {
      List<Uri> source = new List<Uri>(2);
      try
      {
        if (assembly.IsDynamic || assembly.GlobalAssemblyCache)
          return (IEnumerable<Uri>) source;
        string codeBase = assembly.CodeBase;
        if (!string.IsNullOrEmpty(codeBase))
          source.Add(new Uri(codeBase, UriKind.Absolute));
      }
      catch (NotImplementedException ex)
      {
      }
      catch (Exception ex)
      {
        AssemblySatelliteResolver.log.DebugFormat("Cannot get CodeBase of \"{0}\". {1}", (object) assembly, (object) ex);
      }
      try
      {
        string location = assembly.Location;
        if (!string.IsNullOrEmpty(location))
        {
          Uri locUri = new Uri(location, UriKind.Absolute);
          if (!source.Any<Uri>((Func<Uri, bool>) (uri => uri.Equals((object) locUri))))
            source.Add(locUri);
        }
      }
      catch (NotImplementedException ex)
      {
      }
      catch (Exception ex)
      {
        AssemblySatelliteResolver.log.DebugFormat("Cannot get Location of \"{0}\". {1}", (object) assembly, (object) ex);
      }
      return (IEnumerable<Uri>) source;
    }

    private static Uri GetAssemblyLocation(Assembly assembly)
    {
      return AssemblySatelliteResolver.GetAssemblyBaseUris(assembly).LastOrDefault<Uri>();
    }

    private static string GetSymbolicLocation(Assembly assembly)
    {
      Uri assemblyLocation = AssemblySatelliteResolver.GetAssemblyLocation(assembly);
      if (assemblyLocation != (Uri) null)
        return assemblyLocation.ToString();
      if (assembly.IsDynamic)
        return "«dynamic»";
      return assembly.GlobalAssemblyCache ? "«GAC»" : "«unknown»";
    }

    private static IEnumerable<string> ExpandCulture(CultureInfo culture)
    {
      for (; culture != null && !CultureInfo.InvariantCulture.Equals((object) culture); culture = culture.Parent)
        yield return culture.Name;
      yield return string.Empty;
    }

    private static bool ProbeViaLoadedAssemblies(
      ICollection<Assembly> loadedAssemblies,
      AssemblyName resolve,
      string resolveFileName,
      Assembly requesting,
      string sourceName,
      bool isLog,
      out Assembly resolvedAssembly)
    {
      bool first = AssemblySatelliteResolver.EntryCount.GetOrAdd(resolve.Name, (Func<string, AssemblySatelliteResolver.IntRef>) (_ => new AssemblySatelliteResolver.IntRef(0))).Increment() == 1;
      List<Uri> uriList = new List<Uri>(loadedAssemblies.Count);
      foreach (Assembly loadedAssembly in (IEnumerable<Assembly>) loadedAssemblies)
      {
        AssemblyName name = loadedAssembly.GetName();
        if (!(requesting != (Assembly) null) || requesting.Equals((object) loadedAssembly) || name.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase))
          uriList.AddRange(AssemblySatelliteResolver.GetAssemblyBaseUris(loadedAssembly));
      }
      List<string> list = AssemblySatelliteResolver.ExpandCulture(resolve.CultureInfo).ToList<string>();
      Assembly resolvedAssembly1;
      if (AssemblySatelliteResolver.ProbeForAssemblySatellite((IEnumerable<Uri>) uriList, list, resolve, resolveFileName, isLog, first, out resolvedAssembly1))
      {
        resolvedAssembly = resolvedAssembly1;
        return true;
      }
      if (isLog)
      {
        string str = string.Format("Cannot resolve \"{0}\".{1}", (object) resolve, AssemblySatelliteResolver.GetDebugStackTrace());
        if (resolve.Name.StartsWith("SolarWinds", StringComparison.OrdinalIgnoreCase) && ((IEnumerable<string>) LocaleConfiguration.get_InstalledLocales()).Intersect<string>((IEnumerable<string>) list, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Any<string>())
          AssemblySatelliteResolver.log.Warn((object) str);
        else
          AssemblySatelliteResolver.log.Debug((object) str);
      }
      resolvedAssembly = (Assembly) null;
      return false;
    }

    private static bool ProbeForAssemblySatellite(
      IEnumerable<Uri> loadedUris,
      List<string> cultures,
      AssemblyName resolve,
      string resolveFileName,
      bool isLog,
      bool first,
      out Assembly resolvedAssembly)
    {
      HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.Ordinal);
      foreach (Uri loadedUri in loadedUris)
      {
        string localPath;
        if (loadedUri.IsAbsoluteUri && !string.IsNullOrEmpty(localPath = loadedUri.LocalPath))
        {
          string loadedPath = (string) null;
          try
          {
            loadedPath = AssemblySatelliteResolver.NormalizePath(Path.GetDirectoryName(localPath));
          }
          catch (Exception ex)
          {
            AssemblySatelliteResolver.log.DebugFormat("Cannot get directory path of \"{0}\". {1}", (object) localPath, (object) ex);
          }
          if (!string.IsNullOrEmpty(loadedPath) && stringSet.Add(loadedPath))
          {
            List<string> list = cultures.Select<string, string>((Func<string, string>) (culture => Path.Combine(loadedPath, culture, resolveFileName))).Select<string, string>(new Func<string, string>(AssemblySatelliteResolver.NormalizePath)).Where<string>(new Func<string, bool>(File.Exists)).ToList<string>();
            if (isLog && AssemblySatelliteResolver.log.get_IsDebugEnabled() && list.Count < 1)
              AssemblySatelliteResolver.log.DebugFormat("Resolving \"{0}\" cannot find ({2}) satellite for \"{1}\"", (object) resolve, (object) loadedPath, (object) string.Join(", ", (IEnumerable<string>) cultures));
            foreach (string fullPath in list)
            {
              try
              {
                if (AssemblySatelliteResolver.LoadSatelliteByPath(resolve, fullPath, isLog, first, out resolvedAssembly))
                  return true;
              }
              catch (Exception ex)
              {
                AssemblySatelliteResolver.log.ErrorFormat("Error resolving \"{0}\" as \"{1}\". {2}", (object) resolve, (object) fullPath, (object) ex);
              }
            }
          }
        }
      }
      resolvedAssembly = (Assembly) null;
      return false;
    }

    private static bool LoadSatelliteByPath(
      AssemblyName resolve,
      string fullPath,
      bool isLog,
      bool first,
      out Assembly resolvedAssembly)
    {
      AssemblyName assemblyName = AssemblyName.GetAssemblyName(fullPath);
      if (!AssemblySatelliteResolver.SatelliteMatchesDefinition(resolve, assemblyName))
      {
        if (isLog)
          AssemblySatelliteResolver.log.DebugFormat("Resolving \"{0}\" does not match \"{1}\"", (object) resolve, (object) assemblyName);
        resolvedAssembly = (Assembly) null;
        return false;
      }
      Exception exception = (Exception) null;
      Assembly assembly = (Assembly) null;
      if (first)
      {
        try
        {
          assembly = Assembly.Load(assemblyName);
        }
        catch (FileNotFoundException ex)
        {
        }
        catch (FileLoadException ex)
        {
        }
        catch (Exception ex)
        {
          exception = ex;
        }
      }
      if (assembly == (Assembly) null)
      {
        if (exception != null)
          AssemblySatelliteResolver.log.DebugFormat("Cannot load \"{0}\", falling back to load-from. {1}", (object) assemblyName, (object) exception);
        assembly = Assembly.LoadFrom(fullPath);
      }
      try
      {
        if (AssemblySatelliteResolver.ResolveCnt.GetOrAdd(assembly.FullName, (Func<string, AssemblySatelliteResolver.IntRef>) (_ => new AssemblySatelliteResolver.IntRef(0))).Increment() == 1 | isLog)
        {
          Uri assemblyLocation = AssemblySatelliteResolver.GetAssemblyLocation(assembly);
          string str = !(assemblyLocation != (Uri) null) || !assemblyLocation.IsAbsoluteUri ? string.Empty : AssemblySatelliteResolver.NormalizePath(assemblyLocation.LocalPath);
          if (AssemblyName.ReferenceMatchesDefinition(resolve, assembly.GetName()))
          {
            if (fullPath.Equals(str, StringComparison.Ordinal))
              goto label_19;
          }
          AssemblySatelliteResolver.log.ErrorFormat("Resolved \"{0}\" as \"{1}\" at \"{2}\"", (object) resolve, (object) assembly, (object) str);
        }
      }
      catch (Exception ex)
      {
        AssemblySatelliteResolver.log.ErrorFormat("Error reporting assembly-resolved. {0}", (object) ex);
      }
label_19:
      resolvedAssembly = assembly;
      return true;
    }

    private sealed class IntRef
    {
      private int Value;

      internal IntRef(int value)
      {
        this.Value = value;
      }

      internal int Increment()
      {
        return Interlocked.Increment(ref this.Value);
      }
    }
  }
}
