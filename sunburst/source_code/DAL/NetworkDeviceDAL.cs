// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.NetworkDeviceDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Threading;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.Catalogs;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  internal class NetworkDeviceDAL
  {
    private static readonly LazyWithoutExceptionCache<NetworkDeviceDAL> instance = new LazyWithoutExceptionCache<NetworkDeviceDAL>((Func<NetworkDeviceDAL>) (() =>
    {
      using (ComposablePartCatalog catalogForArea = MEFPluginsLoader.get_Instance().GetCatalogForArea("NetworkDevice"))
        return new NetworkDeviceDAL(catalogForArea);
    }));
    [Import("Syslog", typeof (INetworkDeviceDal), AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
    private INetworkDeviceDal syslogDal;
    [Import("Trap", typeof (INetworkDeviceDal), AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
    private INetworkDeviceDal trapDal;

    internal NetworkDeviceDAL(ComposablePartCatalog catalog)
    {
      this.ComposeParts(catalog);
    }

    private void ComposeParts(ComposablePartCatalog catalog)
    {
      using (CompositionContainer container = new CompositionContainer(catalog, Array.Empty<ExportProvider>()))
        container.ComposeParts((object) this);
    }

    public static NetworkDeviceDAL Instance
    {
      get
      {
        NetworkDeviceDAL networkDeviceDal = NetworkDeviceDAL.instance.get_Value();
        if (networkDeviceDal != null)
          return networkDeviceDal;
        throw new InvalidOperationException("Unable to instantiate NetworkDeviceDAL. That was most likely caused by failed try of MEF trying to resolve parts.");
      }
    }

    public List<SolarWinds.Orion.Core.Common.Models.Node> GetNetworkDevices(
      CorePageType pageType,
      List<int> limitationIDs)
    {
      switch (pageType - 1)
      {
        case 0:
          return AlertDAL.GetAlertNetObjects(limitationIDs);
        case 1:
          return this.syslogDal?.GetNetObjects(limitationIDs) ?? new List<SolarWinds.Orion.Core.Common.Models.Node>(0);
        case 2:
          return this.trapDal?.GetNetObjects(limitationIDs) ?? new List<SolarWinds.Orion.Core.Common.Models.Node>(0);
        default:
          throw new NotImplementedException("Unsupported page type");
      }
    }

    public Dictionary<int, string> GetNetworkDeviceNamesForPage(
      CorePageType pageType,
      List<int> limitationIDs,
      bool includeBasic)
    {
      switch (pageType - 1)
      {
        case 0:
          return AlertDAL.GetNodeData(limitationIDs, includeBasic);
        case 1:
          return this.syslogDal?.GetNodeData(limitationIDs) ?? new Dictionary<int, string>(0);
        case 2:
          return this.trapDal?.GetNodeData(limitationIDs) ?? new Dictionary<int, string>(0);
        case 3:
          return EventsDAL.GetNodeData(limitationIDs);
        default:
          throw new NotImplementedException("Unsupported page type");
      }
    }

    public Dictionary<int, string> GetNetworkDeviceNamesForPage(
      CorePageType pageType,
      List<int> limitationIDs)
    {
      return this.GetNetworkDeviceNamesForPage(pageType, limitationIDs, true);
    }

    public Dictionary<string, string> GetNetworkDeviceTypes(List<int> limitationIDs)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(Limitation.LimitSQL(" SELECT Vendor + '%' AS Value, '' AS Name \r\n FROM Nodes \r\n WHERE (Vendor <> '')  \r\n GROUP BY Vendor HAVING (Count(Vendor) > 1) \r\n UNION\r\n SELECT MachineType AS Value, MachineType AS Name \r\n FROM Nodes\r\n WHERE MachineType <> '' \r\n GROUP BY MachineType ", (IEnumerable<int>) limitationIDs)))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          int ordinal1 = dataReader.GetOrdinal("Value");
          int ordinal2 = dataReader.GetOrdinal("Name");
          string libcodeVb061;
          using (LocaleThreadState.EnsurePrimaryLocale())
            libcodeVb061 = Resources.get_LIBCODE_VB0_61();
          while (dataReader.Read())
          {
            string str1 = dataReader.GetString(ordinal2);
            string key = dataReader.GetString(ordinal1);
            string str2 = !string.IsNullOrEmpty(str1) ? str1 : string.Format(libcodeVb061, (object) key.Substring(0, key.Length - 1));
            dictionary.Add(key, str2);
          }
        }
      }
      return dictionary;
    }

    public List<string> GetAllVendors(List<int> limitationIDs)
    {
      List<string> stringList = new List<string>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(Limitation.LimitSQL("SELECT DISTINCT Vendor \r\n From Nodes WHERE (Vendor <> '')", (IEnumerable<int>) limitationIDs)))
      {
        using (IDataReader dataReader = SqlHelper.ExecuteReader(textCommand))
        {
          while (dataReader.Read())
            stringList.Add(DatabaseFunctions.GetString(dataReader, "Vendor"));
        }
      }
      return stringList;
    }
  }
}
