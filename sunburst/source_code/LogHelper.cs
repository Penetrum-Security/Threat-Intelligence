// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.LogHelper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class LogHelper
  {
    private static readonly Log log = new Log();

    public static void DeleteOldLogs(object state)
    {
      LogHelper.log.InfoFormat("Deleting old log files", Array.Empty<object>());
      try
      {
        foreach (RemoveOldOnetimeJobResultsInfo onetimeJobResultsInfo in (RemoveOldOnetimeJobResultsInfo[]) state)
        {
          try
          {
            using (LogHelper.log.Block())
            {
              if (onetimeJobResultsInfo.get_LogFilesPath() == null)
              {
                LogHelper.log.InfoFormat("No directory specified.", Array.Empty<object>());
              }
              else
              {
                foreach (string path in onetimeJobResultsInfo.get_LogFilesPath())
                {
                  if (string.IsNullOrEmpty(path))
                  {
                    LogHelper.log.Info((object) "No log file path found");
                  }
                  else
                  {
                    string directoryName = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directoryName))
                    {
                      LogHelper.log.InfoFormat("Directory {0} not found", (object) directoryName);
                    }
                    else
                    {
                      foreach (string pattern in onetimeJobResultsInfo.get_LogFileNamePattern())
                        LogHelper.DeleteOldFiles(directoryName, pattern, onetimeJobResultsInfo.get_MaxLogFileAge());
                    }
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            LogHelper.log.Error((object) "Unable to delete old log files", ex);
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.log.Error((object) "Unable to delete old log files", ex);
      }
    }

    private static void TryDeleteFile(string fileName)
    {
      LogHelper.log.DebugFormat("Deleting log file {0}", (object) fileName);
      try
      {
        File.Delete(fileName);
      }
      catch (IOException ex)
      {
        LogHelper.log.WarnFormat("Error deleting file {0} - ({1}).", (object) fileName, (object) ex.Message);
      }
    }

    public static void DeleteOldFiles(string path, string pattern, TimeSpan age)
    {
      using (LogHelper.log.Block())
      {
        if (path == null)
          throw new ArgumentNullException(nameof (path));
        if (pattern == null)
          throw new ArgumentNullException(nameof (pattern));
        if (LogHelper.log.get_IsDebugEnabled())
          LogHelper.log.DebugFormat("path = {0}, pattern = {1}, age = {2}", (object) path, (object) pattern, (object) age);
        DateTime thresholdDate = DateTime.Now - age.Duration();
        foreach (FileSystemInfo fileSystemInfo in ((IEnumerable<FileInfo>) new DirectoryInfo(path).GetFiles(pattern)).Where<FileInfo>((Func<FileInfo, bool>) (x => x.LastWriteTime < thresholdDate)))
          LogHelper.TryDeleteFile(fileSystemInfo.FullName);
      }
    }
  }
}
