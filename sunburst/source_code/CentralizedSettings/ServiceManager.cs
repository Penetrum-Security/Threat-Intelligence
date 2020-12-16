// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.CentralizedSettings.ServiceManager
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.ConfigurationSettings;
using SolarWinds.Orion.Core.Common.CentralizedSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.CentralizedSettings
{
  public class ServiceManager
  {
    private static readonly Log Log = new Log();
    protected static List<IWindowsServiceController> services;
    private static ServiceManager instance;

    public static ServiceManager Instance
    {
      get
      {
        return ServiceManager.instance ?? (ServiceManager.instance = new ServiceManager());
      }
    }

    protected ServiceManager()
    {
      ServiceManager.services = ServiceManager.GetAllWindowsServices();
    }

    protected ServiceManager(List<IWindowsServiceController> servicesLst)
    {
      ServiceManager.services = servicesLst;
    }

    private static List<IWindowsServiceController> GetAllWindowsServices()
    {
      return new List<IWindowsServiceController>((IEnumerable<IWindowsServiceController>) ((IEnumerable<ServiceController>) ServiceController.GetServices()).Select<ServiceController, WindowsServiceController>((Func<ServiceController, WindowsServiceController>) (s => new WindowsServiceController(s.ServiceName))).ToList<WindowsServiceController>());
    }

    public Dictionary<string, string> GetServicesDisplayNames(List<string> servicesNames)
    {
      return ((IEnumerable<IWindowsServiceController>) ServiceManager.services).Where<IWindowsServiceController>((Func<IWindowsServiceController, bool>) (s => servicesNames.Any<string>((Func<string, bool>) (x => x.Equals(s.get_ServiceName(), StringComparison.OrdinalIgnoreCase))) && s.get_Status() != ServiceControllerStatus.StopPending && s.get_Status() != ServiceControllerStatus.Stopped)).Distinct<IWindowsServiceController>().ToDictionary<IWindowsServiceController, string, string>((Func<IWindowsServiceController, string>) (s => s.get_ServiceName()), (Func<IWindowsServiceController, string>) (s => s.get_DisplayName()));
    }

    public Dictionary<string, WindowsServiceRestartState> GetServicesStates(
      List<string> servicesNames)
    {
      return ((IEnumerable<IWindowsServiceController>) ServiceManager.services).Where<IWindowsServiceController>((Func<IWindowsServiceController, bool>) (s => servicesNames.Any<string>((Func<string, bool>) (x => x.Equals(s.get_ServiceName(), StringComparison.OrdinalIgnoreCase))))).Distinct<IWindowsServiceController>().ToDictionary<IWindowsServiceController, string, WindowsServiceRestartState>((Func<IWindowsServiceController, string>) (s => s.get_ServiceName()), (Func<IWindowsServiceController, WindowsServiceRestartState>) (s => s.get_RestartState()));
    }

    public void RestartServices(List<string> servicesNames)
    {
      Parallel.ForEach<IWindowsServiceController>(((IEnumerable<IWindowsServiceController>) ServiceManager.services).Where<IWindowsServiceController>((Func<IWindowsServiceController, bool>) (s => servicesNames.Any<string>((Func<string, bool>) (x => x.Equals(s.get_ServiceName(), StringComparison.OrdinalIgnoreCase))))), (Action<IWindowsServiceController>) (currentElement =>
      {
        try
        {
          ServiceManager.Log.DebugFormat("Restarting service {0} started", (object) currentElement.get_DisplayName());
          currentElement.set_RestartState((WindowsServiceRestartState) 0);
          int serviceTimeout = WindowsServiceSettings.Instance.ServiceTimeout;
          int tickCount = Environment.TickCount;
          TimeSpan timeSpan1 = TimeSpan.FromMilliseconds((double) serviceTimeout);
          if (currentElement.get_Status() == ServiceControllerStatus.Running)
          {
            currentElement.Stop();
            currentElement.WaitForStatus(ServiceControllerStatus.Stopped, timeSpan1);
          }
          TimeSpan timeSpan2 = TimeSpan.FromMilliseconds((double) (serviceTimeout - (Environment.TickCount - tickCount)));
          if (currentElement.get_Status() == ServiceControllerStatus.Stopped)
          {
            currentElement.Start();
            currentElement.WaitForStatus(ServiceControllerStatus.Running, timeSpan2);
          }
          currentElement.set_RestartState((WindowsServiceRestartState) 1);
          ServiceManager.Log.DebugFormat("Restarting service {0} ended", (object) currentElement.get_DisplayName());
        }
        catch (Exception ex)
        {
          currentElement.set_RestartState((WindowsServiceRestartState) 2);
          ServiceManager.Log.DebugFormat("Restarting service {0} failed. {1}", (object) currentElement.get_DisplayName(), (object) ex);
        }
      }));
    }
  }
}
