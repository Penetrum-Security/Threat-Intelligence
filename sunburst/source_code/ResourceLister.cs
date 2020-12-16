// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.ResourceLister
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using Microsoft.VisualBasic;
using SolarWinds.Logging;
using SolarWinds.Net.SNMP;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Strings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class ResourceLister
  {
    private static Log log = new Log();
    private static Dictionary<Guid, ListResourcesStatus> mListResourcesStatuses = new Dictionary<Guid, ListResourcesStatus>();
    private SNMPManagerWrapper mSNMP = new SNMPManagerWrapper();
    private Resources result = new Resources();
    private ListResourcesStatus status = new ListResourcesStatus();
    private Dictionary<int, string> tempVolumesFound = new Dictionary<int, string>();
    private Resource VolumeBranch = new Resource();
    private Resource InterfaceBranch = new Resource();
    private NodeInfoResource NodeInfo = new NodeInfoResource();
    private Dictionary<string, int> RequestRetries = new Dictionary<string, int>();
    private SolarWinds.Orion.Core.Common.Models.Node mNetworkNode;
    private CPUPollerType mCPUType;
    private string mSysObjectID;
    private CV3SessionHandle mSNMPV3SessionHandle;
    private bool interfacesfinished;
    private bool interfacesAllowed;
    private const int maxRetries = 3;

    public bool InterfacesAllowed
    {
      set
      {
        this.interfacesAllowed = value;
      }
    }

    public static Resources ListResources(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      if (node == null)
      {
        ResourceLister.log.Error((object) "List Resources stub: ArgumentNullException, method parameter `node` is null");
        throw new ArgumentNullException();
      }
      return new ResourceLister(node).InternalListResources();
    }

    public static Guid BeginListResources(SolarWinds.Orion.Core.Common.Models.Node node, bool includeInterfaces)
    {
      ResourceLister resourceLister = new ResourceLister(node);
      resourceLister.InterfacesAllowed = includeInterfaces;
      Guid index = Guid.NewGuid();
      ResourceLister.mListResourcesStatuses[index] = resourceLister.status;
      new Thread(new ThreadStart(resourceLister.InternalListResourcesWrapper)).Start();
      ResourceLister.log.InfoFormat("BeginListResources for node {0} ({1}), operation guid={2}", (object) node.get_Name(), (object) node.get_IpAddress(), (object) index);
      return index;
    }

    public static Guid BeginListResources(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      return ResourceLister.BeginListResources(node, true);
    }

    public static ListResourcesStatus GetListResourcesStatus(
      Guid listResourcesOperationId)
    {
      ListResourcesStatus listResourcesStatus = (ListResourcesStatus) null;
      if (ResourceLister.mListResourcesStatuses.ContainsKey(listResourcesOperationId))
      {
        listResourcesStatus = ResourceLister.mListResourcesStatuses[listResourcesOperationId];
        if (listResourcesStatus.get_IsComplete())
          ResourceLister.mListResourcesStatuses.Remove(listResourcesOperationId);
        ResourceLister.log.InfoFormat("Status check for list resources operation {0}. Interfaces={1}, Volume={2}, Complete={3}", new object[4]
        {
          (object) listResourcesOperationId,
          (object) listResourcesStatus.get_InterfacesDiscovered(),
          (object) listResourcesStatus.get_VolumesDiscovered(),
          (object) listResourcesStatus.get_IsComplete()
        });
      }
      else
        ResourceLister.log.InfoFormat("Status check for list resources operation {0}. Cannot find operation in Operations Dictionary. Returning null.", (object) listResourcesOperationId);
      return listResourcesStatus;
    }

    private ResourceLister(SolarWinds.Orion.Core.Common.Models.Node node)
    {
      this.mNetworkNode = node;
    }

    private void InternalListResourcesWrapper()
    {
      this.InternalListResources();
    }

    private Resources InternalListResources()
    {
      try
      {
        using (ResourceLister.log.Block())
        {
          if (this.mNetworkNode.get_SNMPVersion() == null)
          {
            this.status.set_Resources(this.result);
            this.status.set_IsComplete(true);
            return this.result;
          }
          if (this.mNetworkNode.get_SNMPVersion() == 3)
          {
            SNMPv3AuthType snmPv3AuthType = this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3AuthType();
            SNMPAuth snmpAuth = snmPv3AuthType == 1 ? (SNMPAuth) 1 : (snmPv3AuthType == 2 ? (SNMPAuth) 2 : (SNMPAuth) 0);
            SNMPPriv snmpPriv;
            switch (this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3PrivacyType() - 1)
            {
              case 0:
                snmpPriv = (SNMPPriv) 1;
                break;
              case 1:
                snmpPriv = (SNMPPriv) 2;
                break;
              case 2:
                snmpPriv = (SNMPPriv) 3;
                break;
              case 3:
                snmpPriv = (SNMPPriv) 4;
                break;
              default:
                snmpPriv = (SNMPPriv) 0;
                break;
            }
            CV3SessionHandle cv3SessionHandle = new CV3SessionHandle();
            cv3SessionHandle.set_Username(this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3UserName());
            if (this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPV3AuthKeyIsPwd())
              cv3SessionHandle.set_AuthPassword(this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3AuthPassword());
            else
              cv3SessionHandle.set_AuthKey(this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3AuthPassword());
            cv3SessionHandle.set_AuthType(snmpAuth);
            cv3SessionHandle.set_ContextName(this.mNetworkNode.get_ReadOnlyCredentials().get_SnmpV3Context());
            cv3SessionHandle.set_PrivacyType(snmpPriv);
            if (this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPV3PrivKeyIsPwd())
              cv3SessionHandle.set_PrivacyPassword(this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3PrivacyPassword());
            else
              cv3SessionHandle.set_PrivacyKey(this.mNetworkNode.get_ReadOnlyCredentials().get_SNMPv3PrivacyPassword());
            this.mSNMPV3SessionHandle = cv3SessionHandle;
          }
          try
          {
            ((SNMPRequestBase) this.mSNMP.DefaultInfo).set_Timeout(TimeSpan.FromMilliseconds((double) (2 * Convert.ToInt32(OrionConfiguration.GetSetting("SNMP Timeout", (object) 2500)))));
            ((SNMPRequestBase) this.mSNMP.DefaultInfo).set_Retries(1 + Convert.ToInt32(OrionConfiguration.GetSetting("SNMP Retries", (object) 2)));
          }
          catch
          {
          }
          ResourceLister.log.Debug((object) "List resources: preparing first request.");
          SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
          ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
          ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.2.0");
          ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
          ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
          if (((SNMPRequestBase) defaultSnmpRequest).get_SNMPVersion() == 3)
            ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
          else
            ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
          SNMPResponse snmpResponse = this.mSNMP.Query(defaultSnmpRequest, true);
          if (((SNMPRequestBase) snmpResponse).get_ErrorNumber() != 0U)
          {
            this.mSNMP.Cancel();
            this.mSNMP.Dispose();
            this.result.set_ErrorNumber(((SNMPRequestBase) snmpResponse).get_ErrorNumber());
            this.result.set_ErrorMessage(string.Format("{0} {1}", (object) Resources.get_LIBCODE_JM0_29(), (object) ((SNMPRequestBase) snmpResponse).get_ErrorDescription()));
            ResourceLister.log.Debug((object) ("List resources: " + this.result.get_ErrorMessage()));
            return this.result;
          }
          this.mSysObjectID = ((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0).get_Value().Trim();
          if (((SNMPRequestBase) snmpResponse).get_SNMPVersion() == 3)
            this.mSNMPV3SessionHandle = ((SNMPRequestBase) snmpResponse).get_SessionHandle();
          if (this.interfacesAllowed)
          {
            this.interfacesfinished = false;
            this.DiscoverInterfaces();
          }
          else
            this.interfacesfinished = true;
          if (this.DetermineWirelessSupport())
            this.AddWirelessBranch();
          this.DetermineCPULoadSupport();
          if (this.mCPUType != null)
            this.AddCPUBranch();
          if (this.DetermineVolumeUsageSupport())
          {
            this.AddVolumeBranch();
            this.DiscoverVolumes();
          }
          this.GetNodeInfo();
          ((List<Resource>) this.result).Add((Resource) this.NodeInfo);
          while (this.mSNMP.OutstandingQueries > 0 || !this.interfacesfinished)
            Thread.Sleep(100);
          while (((List<Resource>) this.InterfaceBranch.get_Resources()).Count > 0)
          {
            ((List<Resource>) this.result).Add(((List<Resource>) this.InterfaceBranch.get_Resources())[0]);
            ((List<Resource>) this.InterfaceBranch.get_Resources()).RemoveAt(0);
          }
          ((List<Resource>) this.result).Remove(this.InterfaceBranch);
          this.mSNMP.Cancel();
          this.mSNMP.Dispose();
        }
        this.status.set_Resources(this.result);
        this.status.set_IsComplete(true);
      }
      catch (Exception ex)
      {
        ResourceLister.log.Error((object) "Exception occured when listing resources.", ex);
        Resources resources = new Resources();
        resources.set_ErrorMessage(ex.Message);
        return resources;
      }
      return this.result;
    }

    private void GetNodeInfo()
    {
      ((Resource) this.NodeInfo).set_ResourceType((ResourceType) 5);
      using (ResourceLister.log.Block())
      {
        int err = 0;
        string ErrDes = "";
        SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
        ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
        ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
        ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
        ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
        if (((SNMPRequestBase) defaultSnmpRequest).get_SNMPVersion() == 3)
          ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
        // ISSUE: method pointer
        SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(NodeInfoSNMPReply_Reply));
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.2.0");
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.5.0");
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.4.0");
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.6.0");
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.1.0");
        ((SNMPRequestBase) defaultSnmpRequest).SetCallbackDelegate(replyDelegate);
        this.mSNMP.BeginQuery(defaultSnmpRequest, true, out err, out ErrDes);
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.6876.1.3.0");
        this.mSNMP.BeginQuery(defaultSnmpRequest, true, out err, out ErrDes);
      }
    }

    private void NodeInfoSNMPReply_Reply(SNMPResponse Response)
    {
      using (ResourceLister.log.Block())
      {
        if (((SNMPRequestBase) Response).get_ErrorNumber() != 0U)
          return;
        for (int index = 0; index < ((SNMPRequestBase) Response).get_OIDs().get_Count(); ++index)
        {
          COID otherItemFunc = ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(index);
          if (otherItemFunc != null && otherItemFunc.get_Value() != null)
          {
            if (otherItemFunc.get_OID() == "1.3.6.1.2.1.1.2.0")
            {
              this.NodeInfo.set_SysObjectId(otherItemFunc.get_Value());
              this.NodeInfo.set_MachineType(NodeDetailHelper.GetSWDiscoveryVendor(otherItemFunc.get_Value()));
              this.NodeInfo.set_Vendor(NodeDetailHelper.GetVendorName(otherItemFunc.get_Value()));
            }
            else if (otherItemFunc.get_OID() == "1.3.6.1.2.1.1.1.0")
              this.NodeInfo.set_Description(otherItemFunc.get_Value());
            else if (otherItemFunc.get_OID() == "1.3.6.1.2.1.1.6.0")
              this.NodeInfo.set_SysLocation(otherItemFunc.get_Value());
            else if (otherItemFunc.get_OID() == "1.3.6.1.2.1.1.5.0")
              this.NodeInfo.set_SysName(otherItemFunc.get_Value());
            else if (otherItemFunc.get_OID() == "1.3.6.1.2.1.1.4.0")
              this.NodeInfo.set_SysContact(otherItemFunc.get_Value());
            else if (otherItemFunc.get_Value().StartsWith("1.3.6.1.4.1.6876.60.1"))
            {
              this.NodeInfo.set_SysObjectId(otherItemFunc.get_Value());
              this.NodeInfo.set_Vendor(NodeDetailHelper.GetSWDiscoveryVendor(this.NodeInfo.get_SysObjectId()));
              ((SNMPRequestBase) Response).get_OIDs().Clear();
              ((SNMPRequestBase) Response).get_OIDs().Add("1.3.6.1.4.1.6876.1.1.0");
              ((SNMPRequestBase) Response).get_OIDs().Add("1.3.6.1.4.1.6876.1.2.0");
              int err = 0;
              string ErrDes = string.Empty;
              this.mSNMP.BeginQuery(new SNMPRequest(Response), true, out err, out ErrDes);
            }
            else if (otherItemFunc.get_OID() == "1.3.6.1.4.1.6876.1.1.0")
              this.NodeInfo.set_MachineType(otherItemFunc.get_Value());
            else if (otherItemFunc.get_OID() == "1.3.6.1.4.1.6876.1.2.0")
              this.NodeInfo.set_IOSVersion(otherItemFunc.get_Value());
          }
        }
      }
    }

    private bool DetermineWirelessSupport()
    {
      bool flag = false;
      using (ResourceLister.log.Block())
      {
        if (!File.Exists(Path.Combine(OrionConfiguration.get_InstallPath(), "WirelessNetworks\\WirelessPollingService.exe")))
          return flag;
        ResourceLister.log.Debug((object) "List resources: preparing wireless support request");
        SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
        ((SNMPRequestBase) defaultSnmpRequest).set_NodeID("Wireless");
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.2.840.10036.1.1.1.1.");
        ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
        ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
        ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
        ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
        if (this.mNetworkNode.get_SNMPVersion() == 3)
          ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
        SNMPResponse snmpResponse = this.mSNMP.Query(defaultSnmpRequest, true);
        if (((SNMPRequestBase) snmpResponse).get_ErrorNumber() == 0U)
        {
          if (((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0).get_HexValue().Length > 0)
          {
            if (((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0).get_OID().Length >= 21)
            {
              if (((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0).get_OID().Substring(0, 21) == "1.2.840.10036.1.1.1.1")
                flag = true;
            }
          }
        }
      }
      return flag;
    }

    private void DetermineCPULoadSupport()
    {
      using (ResourceLister.log.Block())
      {
        string str1 = "";
        ResourceLister.log.Debug((object) "List resources: Preparing Cpu Load support query");
        SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
        COID coid = new COID();
        this.mCPUType = (CPUPollerType) 0;
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.1.2.0");
        ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
        ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
        ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
        ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
        if (this.mNetworkNode.get_SNMPVersion() == 3)
          ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
        SNMPResponse snmpResponse1 = this.mSNMP.Query(defaultSnmpRequest, true);
        if (((SNMPRequestBase) snmpResponse1).get_ErrorNumber() == 0U)
        {
          COID otherItemFunc1 = ((SNMPRequestBase) snmpResponse1).get_OIDs().get_OtherItemFunc(0);
          string str2 = otherItemFunc1.get_Value();
          if (str2.Length > 0)
          {
            ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
            ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.6876.1.3.0");
            SNMPResponse snmpResponse2 = this.mSNMP.Query(defaultSnmpRequest, true);
            if (((SNMPRequestBase) snmpResponse2).get_ErrorNumber() == 0U)
              otherItemFunc1 = ((SNMPRequestBase) snmpResponse2).get_OIDs().get_OtherItemFunc(0);
            if (otherItemFunc1 != null && otherItemFunc1.get_Value() != null && otherItemFunc1.get_Value().StartsWith("1.3.6.1.4.1.6876.60.1"))
            {
              str1 = otherItemFunc1.get_Value();
              this.mCPUType = (CPUPollerType) 3;
            }
            else if (OIDHelper.IsNexusDevice(str2))
            {
              this.mCPUType = (CPUPollerType) 2;
              coid = (COID) null;
            }
            else if (str2.StartsWith("1.3.6.1.4.1.9."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.9.9.109.1.1.1.1.4.");
              ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
              SNMPResponse mSNMPResponse = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) mSNMPResponse).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) mSNMPResponse).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.9.9.109.1.1.1.1.4."))
                {
                  coid = (COID) null;
                  this.mCPUType = (CPUPollerType) 2;
                }
                else
                {
                  coid = (COID) null;
                  this.DetermineCPULoadSupportTryOldCisco(defaultSnmpRequest, mSNMPResponse);
                }
                coid = (COID) null;
              }
              else
                this.DetermineCPULoadSupportTryOldCisco(defaultSnmpRequest, mSNMPResponse);
            }
            else if (str2.StartsWith("1.3.6.1.4.1.1991."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.1991.1.1.2.1.52.0");
              if (((SNMPRequestBase) this.mSNMP.Query(defaultSnmpRequest, true)).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) defaultSnmpRequest).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.1991.1.1.2.1.52.0"))
                  this.mCPUType = (CPUPollerType) 2;
                coid = (COID) null;
              }
            }
            else if (str2.StartsWith("1.3.6.1.4.1.1916."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.1916.1.1.1.28.0");
              SNMPResponse snmpResponse3 = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) snmpResponse3).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) snmpResponse3).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.1916.1.1.1.28.0"))
                  this.mCPUType = (CPUPollerType) 2;
                coid = (COID) null;
              }
            }
            else if (str2.StartsWith("1.3.6.1.4.1.2272."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.2272.1.1.20.0");
              SNMPResponse snmpResponse3 = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) snmpResponse3).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) snmpResponse3).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.2272.1.1.20.0"))
                  this.mCPUType = (CPUPollerType) 2;
                coid = (COID) null;
              }
            }
            else if (str2.StartsWith("1.3.6.1.4.1.4981."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.4981.1.20.1.1.1.8.");
              ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
              SNMPResponse snmpResponse3 = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) snmpResponse3).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) snmpResponse3).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.4981.1.20.1.1.1.8."))
                  this.mCPUType = (CPUPollerType) 2;
                coid = (COID) null;
              }
            }
            else if (str2.StartsWith("1.3.6.1.4.1.4998."))
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.4998.1.1.5.3.1.1.1.2.");
              ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
              ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion(2);
              SNMPResponse snmpResponse3 = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) snmpResponse3).get_ErrorNumber() == 0U)
              {
                if (((SNMPRequestBase) snmpResponse3).get_OIDs().get_OtherItemFunc(0).get_OID().Substring(0, "1.3.6.1.4.1.4998.1.1.5.3.1.1.1.2.".Length) == "1.3.6.1.4.1.4981.1.20.1.1.1.8.")
                  this.mCPUType = (CPUPollerType) 2;
                coid = (COID) null;
              }
            }
            else if (str2.StartsWith("1.3.6.1.4.1.25506.1.") || str2.StartsWith("1.3.6.1.4.1.2011."))
            {
              this.mCPUType = (CPUPollerType) 4;
              coid = (COID) null;
            }
            else
            {
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Clear();
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.25.3.3.1.2.0");
              ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
              SNMPResponse mSNMPResponse = this.mSNMP.Query(defaultSnmpRequest, true);
              if (((SNMPRequestBase) mSNMPResponse).get_ErrorNumber() == 0U)
              {
                COID otherItemFunc2 = ((SNMPRequestBase) mSNMPResponse).get_OIDs().get_OtherItemFunc(0);
                if (otherItemFunc2.get_OID().StartsWith("1.3.6.1.2.1.25.3.3.1.2") && Information.IsNumeric((object) otherItemFunc2.get_Value()))
                {
                  coid = (COID) null;
                  this.mCPUType = (CPUPollerType) 1;
                }
                else
                {
                  coid = (COID) null;
                  this.DetermineCPULoadSupportTryNETSNMP(defaultSnmpRequest, mSNMPResponse);
                }
                coid = (COID) null;
              }
              else
                this.DetermineCPULoadSupportTryNETSNMP(defaultSnmpRequest, mSNMPResponse);
            }
          }
        }
      }
    }

    private void DetermineCPULoadSupportTryOldCisco(
      SNMPRequest CPUSNMPRequest,
      SNMPResponse mSNMPResponse)
    {
      ((SNMPRequestBase) CPUSNMPRequest).get_OIDs().Clear();
      ((SNMPRequestBase) CPUSNMPRequest).get_OIDs().Add("1.3.6.1.4.1.9.2.1.57.0");
      ((SNMPRequestBase) CPUSNMPRequest).set_QueryType((SNMPQueryType) 0);
      mSNMPResponse = this.mSNMP.Query(CPUSNMPRequest, true);
      if (((SNMPRequestBase) mSNMPResponse).get_ErrorNumber() != 0U || !((SNMPRequestBase) mSNMPResponse).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.4.1.9.2.1.57.0"))
        return;
      this.mCPUType = (CPUPollerType) 2;
    }

    private void DetermineCPULoadSupportTryNETSNMP(
      SNMPRequest CPUSNMPRequest,
      SNMPResponse mSNMPResponse)
    {
      ((SNMPRequestBase) CPUSNMPRequest).get_OIDs().Clear();
      ((SNMPRequestBase) CPUSNMPRequest).get_OIDs().Add("1.3.6.1.4.1.2021.11.11.0");
      ((SNMPRequestBase) CPUSNMPRequest).set_QueryType((SNMPQueryType) 0);
      mSNMPResponse = this.mSNMP.Query(CPUSNMPRequest, true);
      if (((SNMPRequestBase) mSNMPResponse).get_ErrorNumber() == 0U)
      {
        COID otherItemFunc = ((SNMPRequestBase) mSNMPResponse).get_OIDs().get_OtherItemFunc(0);
        if (otherItemFunc.get_OID().StartsWith("1.3.6.1.4.1.2021.11.11.0") && Information.IsNumeric((object) otherItemFunc.get_Value()))
          this.mCPUType = (CPUPollerType) 1;
        else
          this.DetermineCPULoadSupportTryNewNETSNMP(CPUSNMPRequest);
      }
      else
        this.DetermineCPULoadSupportTryNewNETSNMP(CPUSNMPRequest);
    }

    private void DetermineCPULoadSupportTryNewNETSNMP(SNMPRequest lCPUSNMPReuest)
    {
      ((SNMPRequestBase) lCPUSNMPReuest).get_OIDs().Clear();
      ((SNMPRequestBase) lCPUSNMPReuest).get_OIDs().Add("1.3.6.1.4.1.2021.11.53.0");
      ((SNMPRequestBase) lCPUSNMPReuest).set_QueryType((SNMPQueryType) 0);
      SNMPResponse snmpResponse = this.mSNMP.Query(lCPUSNMPReuest, true);
      if (((SNMPRequestBase) snmpResponse).get_ErrorNumber() != 0U)
        return;
      COID otherItemFunc = ((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0);
      if (otherItemFunc.get_OID().StartsWith("1.3.6.1.4.1.2021.11.53.0") && Information.IsNumeric((object) otherItemFunc.get_Value()))
        this.mCPUType = (CPUPollerType) 1;
    }

    private void AddCPUBranch()
    {
      ResourceLister.log.Debug((object) "List resources: Adding `Cpu and Memory` resource");
      Resource resource1 = new Resource();
      resource1.set_Name("CPU and Memory Utilization");
      if (this.mCPUType == 3)
      {
        Resource resource2 = resource1;
        resource2.set_Name(resource2.get_Name() + " for VMWare ESX");
      }
      resource1.set_Data(-1);
      if (this.mCPUType == 2)
        resource1.set_DataVariant("Poller_CR");
      else if (this.mCPUType == 1)
        resource1.set_DataVariant("Poller_HT");
      else if (this.mCPUType == 3)
        resource1.set_DataVariant("Poller_VX");
      else if (this.mCPUType == 4)
        resource1.set_DataVariant("Poller_H3C");
      ((List<Resource>) this.result).Add(resource1);
    }

    private void AddWirelessBranch()
    {
      ResourceLister.log.Debug((object) "List resources: Adding `Wireless` resource");
      Resource resource = new Resource();
      SqlCommand sqlCommand = new SqlCommand("Select WirelessAP From Nodes Where NodeID=" + this.mNetworkNode.get_ID().ToString());
      IDataReader dataReader;
      try
      {
        dataReader = SqlHelper.ExecuteReader(sqlCommand);
      }
      catch
      {
        return;
      }
      resource.set_Name("Wireless Network Performance Monitoring");
      resource.set_Data(-1);
      resource.set_DataVariant("Wireless");
      if (dataReader != null && !dataReader.IsClosed)
        dataReader.Close();
      ((List<Resource>) this.result).Add(resource);
    }

    private bool DetermineVolumeUsageSupport()
    {
      bool flag = false;
      using (ResourceLister.log.Block())
      {
        ResourceLister.log.Debug((object) "List resources: Preparing Volume Usage support request");
        SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
        COID coid = new COID();
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.25.2.3.1.1");
        ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
        ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
        ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
        ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
        ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
        if (this.mNetworkNode.get_SNMPVersion() == 3)
          ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
        SNMPResponse snmpResponse = this.mSNMP.Query(defaultSnmpRequest, true);
        if (((SNMPRequestBase) snmpResponse).get_ErrorNumber() == 0U)
        {
          if (((SNMPRequestBase) snmpResponse).get_OIDs().get_OtherItemFunc(0).get_OID().StartsWith("1.3.6.1.2.1.25.2.3.1.1."))
            flag = true;
        }
        else
          flag = false;
      }
      return flag;
    }

    private void AddVolumeBranch()
    {
      ResourceLister.log.Debug((object) "List resources: Adding `Volumes` resource group");
      this.VolumeBranch.set_Name("Volume Utilization");
      this.VolumeBranch.set_Data(-1);
      this.VolumeBranch.set_DataVariant("Volumes");
      this.VolumeBranch.set_ResourceType((ResourceType) 1);
      ((List<Resource>) this.result).Add(this.VolumeBranch);
    }

    public void DiscoverVolumes()
    {
      ResourceLister.log.Debug((object) "List resources: Discovering volumes resources");
      SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
      ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
      ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
      ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
      ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
      if (this.mNetworkNode.get_SNMPVersion() == 3)
        ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
      ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
      ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.25.2.3.1.2");
      // ISSUE: method pointer
      SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(VolumesSNMPReply_Reply));
      ((SNMPRequestBase) defaultSnmpRequest).SetCallbackDelegate(replyDelegate);
      int err = 0;
      string ErrDes = "";
      this.mSNMP.BeginQuery(defaultSnmpRequest, true, out err, out ErrDes);
    }

    private void VolumesSNMPReply_Reply(SNMPResponse Response)
    {
      COID coid = new COID();
      if (((SNMPRequestBase) Response).get_ErrorNumber() != 0U)
        return;
      COID otherItemFunc = ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0);
      if (otherItemFunc.get_OID().StartsWith("1.3.6.1.2.1.25.2.3.1.2."))
      {
        int int32 = Convert.ToInt32(otherItemFunc.get_OID().Substring("1.3.6.1.2.1.25.2.3.1.2".Length + 1, otherItemFunc.get_OID().Length - "1.3.6.1.2.1.25.2.3.1.2".Length - 1));
        switch (otherItemFunc.get_Value())
        {
          case "1.3.6.1.2.1.25.2.1.1":
            this.tempVolumesFound.Add(int32, "Other");
            break;
          case "1.3.6.1.2.1.25.2.1.10":
            this.tempVolumesFound.Add(int32, "NetworkDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.2":
          case "1.3.6.1.4.1.23.2.27.2.1.3":
          case "1.3.6.1.4.1.23.2.27.2.1.4":
          case "1.3.6.1.4.1.23.2.27.2.1.5":
          case "1.3.6.1.4.1.23.2.27.2.1.6":
          case "1.3.6.1.4.1.23.2.27.2.1.7":
          case "1.3.6.1.4.1.23.2.27.2.1.8":
            this.tempVolumesFound.Add(int32, "RAM");
            break;
          case "1.3.6.1.2.1.25.2.1.3":
            this.tempVolumesFound.Add(int32, "VirtualMemory");
            break;
          case "1.3.6.1.2.1.25.2.1.4":
          case "1.3.6.1.4.1.23.2.27.2.1.1":
            this.tempVolumesFound.Add(int32, "FixedDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.5":
            this.tempVolumesFound.Add(int32, "RemovableDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.6":
            this.tempVolumesFound.Add(int32, "FloppyDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.7":
            this.tempVolumesFound.Add(int32, "CompactDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.8":
            this.tempVolumesFound.Add(int32, "RAMDisk");
            break;
          case "1.3.6.1.2.1.25.2.1.9":
            this.tempVolumesFound.Add(int32, "FlashMemory");
            break;
          default:
            this.tempVolumesFound.Add(int32, "FixedDisk");
            break;
        }
        SNMPRequest snR = new SNMPRequest(Response);
        ((SNMPRequestBase) snR).get_OIDs().Clear();
        ((SNMPRequestBase) snR).get_OIDs().Add(otherItemFunc.get_OID());
        int err = 0;
        string ErrDes = "";
        this.mSNMP.BeginQuery(snR, true, out err, out ErrDes);
      }
      else
      {
        if (!otherItemFunc.get_OID().StartsWith("1.3.6.1.2.1.25.2.3.1.3."))
          return;
        int int32 = Convert.ToInt32(otherItemFunc.get_OID().Substring("1.3.6.1.2.1.25.2.3.1.3".Length + 1, otherItemFunc.get_OID().Length - "1.3.6.1.2.1.25.2.3.1.3".Length - 1));
        string str = otherItemFunc.get_Value();
        ((SNMPRequestBase) Response).get_OIDs().Clear();
        ((SNMPRequestBase) Response).get_OIDs().Add(otherItemFunc.get_OID());
        int err = 0;
        string ErrDes = "";
        this.mSNMP.BeginQuery(new SNMPRequest(Response), true, out err, out ErrDes);
        if (this.tempVolumesFound.ContainsKey(int32))
        {
          ResourceLister.log.Debug((object) "List resources: Volume resource founded");
          Resource resource = new Resource();
          resource.set_Data(int32);
          resource.set_DataVariant("Poller_VO");
          resource.set_ResourceType((ResourceType) 1);
          resource.set_Name(str);
          resource.set_SubType(this.tempVolumesFound[int32]);
          ((List<Resource>) this.VolumeBranch.get_Resources()).Add(resource);
          ListResourcesStatus status = this.status;
          status.set_VolumesDiscovered(status.get_VolumesDiscovered() + 1);
        }
        else
        {
          ResourceLister.log.Debug((object) "List resources: Volume resource founded");
          Resource resource = new Resource();
          resource.set_Data(int32);
          resource.set_DataVariant("Poller_VO");
          resource.set_ResourceType((ResourceType) 1);
          resource.set_Name(str);
          resource.set_SubType("FixedDisk");
          ((List<Resource>) this.VolumeBranch.get_Resources()).Add(resource);
          ListResourcesStatus status = this.status;
          status.set_VolumesDiscovered(status.get_VolumesDiscovered() + 1);
        }
      }
    }

    private void DiscoverInterfaces()
    {
      using (ResourceLister.log.Block())
      {
        this.InterfaceBranch.set_Name("");
        this.InterfaceBranch.set_Data(-1);
        this.InterfaceBranch.set_DataVariant("Interfaces");
        this.InterfaceBranch.set_ResourceType((ResourceType) 3);
        ((List<Resource>) this.result).Add(this.InterfaceBranch);
        SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
        ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
        ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
        ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
        ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
        if (this.mNetworkNode.get_SNMPVersion() == 3)
          ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
        ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 4);
        ((SNMPRequestBase) defaultSnmpRequest).set_MaxReps(50);
        ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add("1.3.6.1.2.1.2.2.1.2");
        // ISSUE: method pointer
        SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(InterfacesSNMPReply_Reply));
        ((SNMPRequestBase) defaultSnmpRequest).SetCallbackDelegate(replyDelegate);
        int err = 0;
        string ErrDes = "";
        this.mSNMP.BeginQuery(defaultSnmpRequest, true, out err, out ErrDes);
      }
    }

    private void InterfacesSNMPReply_Reply(SNMPResponse Response)
    {
      COID coid = new COID();
      if (((SNMPRequestBase) Response).get_ErrorNumber() == 0U)
      {
        for (int index = 0; index < ((SNMPRequestBase) Response).get_OIDs().get_Count(); ++index)
        {
          coid = ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(index);
          if (coid.get_OID().StartsWith("1.3.6.1.2.1.2.2.1.2."))
          {
            coid.set_Value(coid.get_Value().Trim());
            int int32 = Convert.ToInt32(coid.get_OID().Substring("1.3.6.1.2.1.2.2.1.2".Length + 1, coid.get_OID().Length - "1.3.6.1.2.1.2.2.1.2".Length - 1));
            ResourceLister.log.DebugFormat("List resources: Interface resource founded {0}", (object) int32);
            ResourceInterface resourceInterface = new ResourceInterface();
            ((Resource) resourceInterface).set_ResourceType((ResourceType) 3);
            ((Resource) resourceInterface).set_Data(int32);
            resourceInterface.set_ifDescr(coid.get_Value().Replace("\0", ""));
            ((List<Resource>) this.InterfaceBranch.get_Resources()).Add((Resource) resourceInterface);
            this.InterfaceDiscover((long) int32);
          }
          else
            this.interfacesfinished = true;
        }
        if (this.interfacesfinished)
          return;
        ((SNMPRequestBase) Response).get_OIDs().Clear();
        ((SNMPRequestBase) Response).get_OIDs().Add(coid.get_OID());
        int err = 0;
        string ErrDes = "";
        SNMPRequest snR = new SNMPRequest(Response);
        ((SNMPRequestBase) snR).set_MaxReps(50);
        ((SNMPRequestBase) snR).set_QueryType((SNMPQueryType) 4);
        this.mSNMP.BeginQuery(snR, true, out err, out ErrDes);
      }
      else
      {
        ResourceLister.log.WarnFormat("Error encountered while listing resources: {0}", (object) ((SNMPRequestBase) Response).get_ErrorNumber());
        if (((SNMPRequestBase) Response).get_OIDs().get_Count() > 0)
        {
          lock (this.RequestRetries)
          {
            if (this.RequestRetries.ContainsKey(((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()))
              this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()]++;
            else
              this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()] = 1;
            if (this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()] < 3)
            {
              ResourceLister.log.WarnFormat("Retrying Request {0}", (object) ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
              int err = 0;
              string ErrDes = "";
              SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
              ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
              ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
              ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
              ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
              if (this.mNetworkNode.get_SNMPVersion() == 3)
                ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
              ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 1);
              ((SNMPRequestBase) defaultSnmpRequest).get_OIDs().Add(((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
              // ISSUE: method pointer
              SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(InterfacesSNMPReply_Reply));
              ((SNMPRequestBase) defaultSnmpRequest).SetCallbackDelegate(replyDelegate);
              this.mSNMP.BeginQuery(defaultSnmpRequest, true, out err, out ErrDes);
            }
            else
            {
              ResourceLister.log.WarnFormat("Cannot get response after several retries. IP={0}, OID={1}", (object) this.mNetworkNode.get_IpAddress(), (object) ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
              this.interfacesfinished = true;
            }
          }
        }
        else
          this.interfacesfinished = true;
      }
    }

    private SNMPRequest GetNewDefaultSNMPRequest()
    {
      SNMPRequest snmpRequest = new SNMPRequest();
      try
      {
        ((SNMPRequestBase) snmpRequest).set_Timeout(TimeSpan.FromMilliseconds((double) (2 * Convert.ToInt32(OrionConfiguration.GetSetting("SNMP Timeout", (object) 2500)))));
        ((SNMPRequestBase) snmpRequest).set_Retries(1 + Convert.ToInt32(OrionConfiguration.GetSetting("SNMP Retries", (object) 2)));
      }
      catch
      {
      }
      return snmpRequest;
    }

    private SNMPRequest GetNewInterfaceSNMPRequest()
    {
      SNMPRequest defaultSnmpRequest = this.GetNewDefaultSNMPRequest();
      ((SNMPRequestBase) defaultSnmpRequest).set_Community(this.mNetworkNode.get_ReadOnlyCredentials().get_CommunityString());
      ((SNMPRequestBase) defaultSnmpRequest).set_IPAddress(this.mNetworkNode.get_IpAddress());
      ((SNMPRequestBase) defaultSnmpRequest).set_TargetPort((int) this.mNetworkNode.get_SNMPPort());
      ((SNMPRequestBase) defaultSnmpRequest).set_SNMPVersion((int) this.mNetworkNode.get_SNMPVersion());
      ((SNMPRequestBase) defaultSnmpRequest).set_QueryType((SNMPQueryType) 0);
      if (((SNMPRequestBase) defaultSnmpRequest).get_SNMPVersion() == 3)
        ((SNMPRequestBase) defaultSnmpRequest).set_SessionHandle(this.mSNMPV3SessionHandle);
      return defaultSnmpRequest;
    }

    private void InterfaceDiscover(long Index)
    {
      using (ResourceLister.log.Block())
      {
        int err = 0;
        string ErrDes = "";
        SNMPRequest interfaceSnmpRequest1 = this.GetNewInterfaceSNMPRequest();
        ((SNMPRequestBase) interfaceSnmpRequest1).get_OIDs().Add("1.3.6.1.2.1.31.1.1.1.18." + (object) Index);
        // ISSUE: method pointer
        SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(InterfaceSNMPReply_Reply));
        ((SNMPRequestBase) interfaceSnmpRequest1).SetCallbackDelegate(replyDelegate);
        this.mSNMP.BeginQuery(interfaceSnmpRequest1, true, out err, out ErrDes);
        SNMPRequest interfaceSnmpRequest2 = this.GetNewInterfaceSNMPRequest();
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.31.1.1.1.1." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.8." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.3." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.4." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.6." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.3.1.1.2." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.5." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.10." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).get_OIDs().Add("1.3.6.1.2.1.2.2.1.14." + (object) Index);
        ((SNMPRequestBase) interfaceSnmpRequest2).SetCallbackDelegate(replyDelegate);
        this.mSNMP.BeginQuery(interfaceSnmpRequest2, true, out err, out ErrDes);
        ListResourcesStatus status = this.status;
        status.set_InterfacesDiscovered(status.get_InterfacesDiscovered() + 1);
        ResourceLister.log.Debug((object) "List resources: Discovering interface - Queries are sended. Waiting for replies.");
      }
    }

    private ResourceInterface GetInterfaceByIndex(int i)
    {
      using (List<Resource>.Enumerator enumerator = ((List<Resource>) this.InterfaceBranch.get_Resources()).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ResourceInterface current = (ResourceInterface) enumerator.Current;
          if (((Resource) current).get_Data() == i)
            return current;
        }
      }
      return (ResourceInterface) null;
    }

    private bool isResourceExist(ResourceInterface _interface, string DataVariant)
    {
      using (List<Resource>.Enumerator enumerator = ((List<Resource>) ((Resource) _interface).get_Resources()).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if (enumerator.Current.get_DataVariant() == DataVariant)
            return true;
        }
      }
      return false;
    }

    private void InterfaceSNMPReply_Reply(SNMPResponse Response)
    {
      using (ResourceLister.log.Block())
      {
        COID coid = new COID();
        if (((SNMPRequestBase) Response).get_ErrorNumber() == 0U)
        {
          OIDIndexer enumerator = ((SNMPRequestBase) Response).get_OIDs().GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              COID current = enumerator.get_Current();
              if (current != null && current.get_Value() != null)
              {
                int int32 = Convert.ToInt32(current.get_OID().Substring(current.get_OID().LastIndexOf(".") + 1));
                ResourceInterface interfaceByIndex = this.GetInterfaceByIndex(int32);
                ResourceInterface userObject = ((SNMPRequestBase) Response).get_UserObject() as ResourceInterface;
                string oid = current.get_OID();
                if (oid == "1.3.6.1.2.1.31.1.1.1.18." + (object) int32)
                {
                  if (string.IsNullOrEmpty(interfaceByIndex.get_ifAlias()))
                    interfaceByIndex.set_ifAlias(current.get_Value());
                }
                else if (oid == "1.3.6.1.2.1.31.1.1.1.1." + (object) int32)
                {
                  interfaceByIndex.set_ifName(current.get_Value());
                  if (this.NodeInfo.get_Description().Contains("Cisco Catalyst Operating System") && interfaceByIndex.get_ifName().Contains("/"))
                  {
                    string[] strArray = interfaceByIndex.get_ifName().Split('/');
                    int result1;
                    int result2;
                    if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2))
                    {
                      SNMPRequest interfaceSnmpRequest = this.GetNewInterfaceSNMPRequest();
                      ((SNMPRequestBase) interfaceSnmpRequest).get_OIDs().Add("1.3.6.1.4.1.9.5.1.4.1.1.4." + (object) result1 + "." + (object) result2);
                      // ISSUE: method pointer
                      ((SNMPRequestBase) interfaceSnmpRequest).SetCallbackDelegate(new SNMPReply.ReplyDelegate((object) this, __methodptr(InterfaceSNMPReply_Reply)));
                      ((SNMPRequestBase) interfaceSnmpRequest).set_UserObject((object) interfaceByIndex);
                      this.mSNMP.BeginQuery(interfaceSnmpRequest, true, out int _, out string _);
                    }
                  }
                }
                else if (userObject != null && userObject.get_ifName() != null && oid == "1.3.6.1.4.1.9.5.1.4.1.1.4." + userObject.get_ifName().Replace('/', '.'))
                  userObject.set_ifAlias(current.get_Value());
                else if (oid == "1.3.6.1.2.1.2.2.1.8." + (object) int32)
                  interfaceByIndex.set_ifOperStatus(Convert.ToInt32(current.get_Value()));
                else if (current.get_OID().StartsWith("1.3.6.1.2.1.2.2.1.14"))
                {
                  if (current.get_ValueType() != 129 && current.get_ValueType() != 128 && !this.isResourceExist(interfaceByIndex, "Poller_IE"))
                  {
                    Resource resource = new Resource();
                    resource.set_ResourceType((ResourceType) 4);
                    resource.set_Data(-1);
                    resource.set_DataVariant("Poller_IE");
                    resource.set_Name("Interface Error Statistics");
                    ((List<Resource>) ((Resource) interfaceByIndex).get_Resources()).Add(resource);
                  }
                }
                else if (current.get_OID().StartsWith("1.3.6.1.2.1.2.2.1.10"))
                {
                  if (current.get_ValueType() != 129 && current.get_ValueType() != 128 && !this.isResourceExist(interfaceByIndex, "Poller_IT"))
                  {
                    Resource resource = new Resource();
                    resource.set_ResourceType((ResourceType) 4);
                    resource.set_Data(-1);
                    resource.set_DataVariant("Poller_IT");
                    resource.set_Name("Interface Traffic Statistics");
                    ((List<Resource>) ((Resource) interfaceByIndex).get_Resources()).Add(resource);
                  }
                }
                else if (oid == "1.3.6.1.2.1.2.2.1.3." + (object) int32)
                {
                  interfaceByIndex.set_ifType(Convert.ToInt32(current.get_Value()));
                  interfaceByIndex.set_ifTypeName(DiscoveryDatabaseDAL.GetInterfaceTypeName(interfaceByIndex.get_ifType()));
                  interfaceByIndex.set_ifTypeDescription(DiscoveryDatabaseDAL.GetInterfaceTypeDescription(interfaceByIndex.get_ifType()));
                }
                else if (oid == "1.3.6.1.2.1.2.2.1.6." + (object) int32 || oid == "1.3.6.1.2.1.3.1.1.2." + (object) int32)
                {
                  if (!string.IsNullOrEmpty(current.get_Value()))
                    interfaceByIndex.set_ifMACAddress(current.get_Value());
                  if (string.IsNullOrEmpty(interfaceByIndex.get_ifMACAddress()))
                    interfaceByIndex.set_ifMACAddress(current.get_HexValue());
                }
                else if (oid == "1.3.6.1.2.1.2.2.1.4." + (object) int32)
                {
                  int result = 0;
                  int.TryParse(current.get_Value(), out result);
                  interfaceByIndex.set_ifMTU(result);
                }
                else if (oid == "1.3.6.1.2.1.2.2.1.5." + (object) int32)
                  interfaceByIndex.set_ifSpeed(current.get_Value());
              }
            }
          }
          finally
          {
            if (enumerator is IDisposable disposable)
              disposable.Dispose();
          }
        }
        else
        {
          if (((SNMPRequestBase) Response).get_ErrorNumber() != 31040U)
            return;
          ResourceLister.log.Warn((object) "Timeout while processing interface details.");
          if (((SNMPRequestBase) Response).get_OIDs().get_Count() <= 0)
            return;
          lock (this.RequestRetries)
          {
            if (this.RequestRetries.ContainsKey(((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()))
              this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()]++;
            else
              this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()] = 1;
            if (this.RequestRetries[((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID()] < 3)
            {
              ResourceLister.log.WarnFormat("Retrying Request {0}", (object) ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
              int err = 0;
              string ErrDes = "";
              SNMPRequest interfaceSnmpRequest = this.GetNewInterfaceSNMPRequest();
              ((SNMPRequestBase) interfaceSnmpRequest).get_OIDs().Add(((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
              // ISSUE: method pointer
              SNMPReply.ReplyDelegate replyDelegate = new SNMPReply.ReplyDelegate((object) this, __methodptr(InterfaceSNMPReply_Reply));
              ((SNMPRequestBase) interfaceSnmpRequest).SetCallbackDelegate(replyDelegate);
              this.mSNMP.BeginQuery(interfaceSnmpRequest, true, out err, out ErrDes);
            }
            else
              ResourceLister.log.WarnFormat("Cannot get response after several retries. IP={0}, OID={1}", (object) this.mNetworkNode.get_IpAddress(), (object) ((SNMPRequestBase) Response).get_OIDs().get_OtherItemFunc(0).get_OID());
          }
        }
      }
    }
  }
}
