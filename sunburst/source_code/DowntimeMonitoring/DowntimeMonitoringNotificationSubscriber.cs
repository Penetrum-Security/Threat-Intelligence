// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DowntimeMonitoring.DowntimeMonitoringNotificationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Models;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.DowntimeMonitoring
{
  public class DowntimeMonitoringNotificationSubscriber : ISubscriber
  {
    protected static readonly Log log = new Log();
    private static readonly Regex NodeIdRegex = new Regex("(N:\\d+)", RegexOptions.Compiled);
    private readonly List<SubscriptionId> subscriptionIds = new List<SubscriptionId>();
    private const string NetObjectDowntimeIndication = "NetObjectDowntimeIndication";
    private const string NetObjectDowntimeInitializator = "NetObjectDowntimeInitializator";
    private readonly ISubscriptionManager _subscriptionManager;
    private string _nodeNetObjectIdColumn;
    private readonly INetObjectDowntimeDAL _netObjectDowntimeDal;
    private readonly IInformationServiceProxyCreator _swisServiceProxyCreator;
    private readonly ISwisUriParser _swisUriParser;
    private Lazy<ILookup<string, NetObjectTypeEx>> _netObjectTypes;

    internal Lazy<ILookup<string, NetObjectTypeEx>> NetObjectTypes
    {
      set
      {
        this._netObjectTypes = value;
      }
    }

    private static bool IsEnabled
    {
      get
      {
        return (bool) SettingsDAL.GetCurrent<bool>("SWNetPerfMon-Settings-EnableDowntimeMonitoring", (M0) 1);
      }
    }

    public DowntimeMonitoringNotificationSubscriber(INetObjectDowntimeDAL netObjectDowntimeDal)
      : this(netObjectDowntimeDal, (IInformationServiceProxyCreator) SwisConnectionProxyPool.GetSystemCreator(), (ISwisUriParser) new SwisUriParser(), SubscriptionManager.get_Instance())
    {
    }

    internal DowntimeMonitoringNotificationSubscriber(
      INetObjectDowntimeDAL netObjectDowntimeDal,
      IInformationServiceProxyCreator serviceProxyCreator,
      ISwisUriParser swisUriParser,
      ISubscriptionManager subscriptionManager)
    {
      INetObjectDowntimeDAL objectDowntimeDal = netObjectDowntimeDal;
      if (objectDowntimeDal == null)
        throw new ArgumentNullException(nameof (netObjectDowntimeDal));
      this._netObjectDowntimeDal = objectDowntimeDal;
      IInformationServiceProxyCreator serviceProxyCreator1 = serviceProxyCreator;
      if (serviceProxyCreator1 == null)
        throw new ArgumentNullException(nameof (serviceProxyCreator));
      this._swisServiceProxyCreator = serviceProxyCreator1;
      ISwisUriParser swisUriParser1 = swisUriParser;
      if (swisUriParser1 == null)
        throw new ArgumentNullException(nameof (swisUriParser));
      this._swisUriParser = swisUriParser1;
      this._nodeNetObjectIdColumn = (string) null;
      this._netObjectTypes = new Lazy<ILookup<string, NetObjectTypeEx>>(new Func<ILookup<string, NetObjectTypeEx>>(this.LoadNetObjectTypesExtSwisInfo), LazyThreadSafetyMode.PublicationOnly);
      ISubscriptionManager isubscriptionManager = subscriptionManager;
      if (isubscriptionManager == null)
        throw new ArgumentNullException(nameof (subscriptionManager));
      this._subscriptionManager = isubscriptionManager;
    }

    internal int ExtractStatusID(object statusObject)
    {
      return Convert.ToInt32(statusObject);
    }

    public virtual void Start()
    {
      if (!DowntimeMonitoringNotificationSubscriber.IsEnabled)
      {
        DowntimeMonitoringNotificationSubscriber.log.Info((object) "Subscription of Downtime Monitoring cancelled (disabled by Settings option)");
        this.Stop();
      }
      else
      {
        Scheduler.get_Instance().Add(new ScheduledTask("NetObjectDowntimeInitializator", new TimerCallback(this.Initialize), (object) null, TimeSpan.FromSeconds(1.0), TimeSpan.FromMinutes(1.0)));
        Scheduler.get_Instance().Add(new ScheduledTask("NetObjectDowntimeIndication", new TimerCallback(this.Subscribe), (object) null, TimeSpan.FromSeconds(1.0), TimeSpan.FromMinutes(1.0)));
      }
    }

    private void Initialize(object state)
    {
      List<NetObjectDowntime> netObjectDowntimeList1 = new List<NetObjectDowntime>();
      try
      {
        using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisServiceProxyCreator.Create())
        {
          DateTime utcNow = DateTime.UtcNow;
          foreach (DataRow row in (InternalDataCollectionBase) ((IInformationServiceProxy) iinformationServiceProxy2).Query("SELECT Uri, Status, InstanceType, AncestorDetailsUrls\n                                            FROM System.ManagedEntity\n                                            WHERE UnManaged = false").Rows)
          {
            try
            {
              if (this.IsValid(row))
              {
                List<NetObjectDowntime> netObjectDowntimeList2 = netObjectDowntimeList1;
                NetObjectDowntime netObjectDowntime = new NetObjectDowntime();
                netObjectDowntime.set_DateTimeFrom(utcNow);
                netObjectDowntime.set_EntityID(this._swisUriParser.GetEntityId(row["Uri"].ToString()));
                netObjectDowntime.set_NodeID(this.ExtractStatusID((object) this.GetNodeIDFromUrl((string[]) row["AncestorDetailsUrls"])));
                netObjectDowntime.set_EntityType(row["InstanceType"].ToString());
                netObjectDowntime.set_StatusID((int) row["Status"]);
                netObjectDowntimeList2.Add(netObjectDowntime);
              }
            }
            catch (Exception ex)
            {
              DowntimeMonitoringNotificationSubscriber.log.Error((object) string.Format("Unable to create NetObjectDowntime instance from ManagedEntity with Uri '{0}', {1}", row["Uri"], (object) ex));
            }
          }
        }
      }
      catch (Exception ex)
      {
        DowntimeMonitoringNotificationSubscriber.log.ErrorFormat("Exception while initializing NetObjectDowntime table with ManagedEntities. {0}", (object) ex);
      }
      this._netObjectDowntimeDal.Insert((IEnumerable<NetObjectDowntime>) netObjectDowntimeList1);
      Scheduler.get_Instance().Remove("NetObjectDowntimeInitializator");
    }

    private bool IsValid(DataRow row)
    {
      try
      {
        this.GetNodeIDFromUrl((string[]) row["AncestorDetailsUrls"]);
        return !row.IsNull("Uri");
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public virtual void Stop()
    {
      this.Unsubscribe();
      Scheduler.get_Instance().Remove("NetObjectDowntimeIndication");
    }

    private void Unsubscribe()
    {
      if (this.subscriptionIds.Count == 0)
        return;
      try
      {
        using (List<SubscriptionId>.Enumerator enumerator = this.subscriptionIds.GetEnumerator())
        {
          while (enumerator.MoveNext())
            this._subscriptionManager.Unsubscribe(enumerator.Current);
        }
        this.subscriptionIds.Clear();
      }
      catch (Exception ex)
      {
        DowntimeMonitoringNotificationSubscriber.log.ErrorFormat("Unsubscribe failed: '{0}'", (object) ex);
        throw;
      }
    }

    private void Subscribe(object state)
    {
      DowntimeMonitoringNotificationSubscriber.log.Debug((object) "Subscribing Managed Entity changed indications..");
      try
      {
        try
        {
          this.DeleteOldSubscriptions();
        }
        catch (Exception ex)
        {
          DowntimeMonitoringNotificationSubscriber.log.Warn((object) "Exception deleting old subscriptions:", ex);
        }
        if (this.subscriptionIds.Count > 0)
          this.Unsubscribe();
        Tuple<string, string>[] tupleArray = new Tuple<string, string>[3]
        {
          new Tuple<string, string>("SUBSCRIBE System.InstanceDeleted WHEN InstanceType ISA 'System.ManagedEntity' OR SourceInstanceType ISA 'System.ManagedEntity'", "ManagedEntityDeleted"),
          new Tuple<string, string>("SUBSCRIBE System.InstanceCreated WHEN InstanceType ISA 'System.ManagedEntity' OR SourceInstanceType ISA 'System.ManagedEntity'", "ManagedEntityCreated"),
          new Tuple<string, string>("SUBSCRIBE CHANGES TO System.ManagedEntity WHEN Status CHANGES", "ManagedEntityStatusChanges")
        };
        foreach (Tuple<string, string> tuple in tupleArray)
        {
          SubscriptionId subscriptionId;
          ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (DowntimeMonitoringNotificationSubscriber).FullName + "." + tuple.Item2, (Scope) 0);
          SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
          subscriberConfiguration1.set_SubscriptionQuery(tuple.Item1);
          SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
          ISubscription isubscription = this._subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2);
          this.subscriptionIds.Add(isubscription.get_Id());
          DowntimeMonitoringNotificationSubscriber.log.InfoFormat("Pub/sub subscription succeeded. uri:'{0}'", (object) isubscription.get_Id());
        }
        Scheduler.get_Instance().Remove("NetObjectDowntimeIndication");
      }
      catch (Exception ex)
      {
        DowntimeMonitoringNotificationSubscriber.log.ErrorFormat("{0} in Subscribe: {1}\r\n{2}", (object) ex.GetType(), (object) ex.Message, (object) ex.StackTrace);
      }
    }

    private string DetailInfo(
      SubscriptionId subscriptionId,
      string indicationType,
      IDictionary<string, object> indicationProperties,
      IDictionary<string, object> sourceInstanceProperties)
    {
      return string.Format("Pub/Sub Notification: ID: {0}, Type: {1}, IndicationProperties: {2}, InstanceProperties: {3}", (object) subscriptionId, (object) indicationType, (object) string.Join(", ", indicationProperties.Select<KeyValuePair<string, object>, string>((Func<KeyValuePair<string, object>, string>) (kvp => string.Format("{0} = {1}", (object) kvp.Key, kvp.Value)))), sourceInstanceProperties.Count > 0 ? (object) string.Join(", ", sourceInstanceProperties.Select<KeyValuePair<string, object>, string>((Func<KeyValuePair<string, object>, string>) (kvp => string.Format("{0} = {1}", (object) kvp.Key, kvp.Value)))) : (object) string.Empty);
    }

    internal string GetNodeIDFromUrl(string[] urls)
    {
      foreach (string url in urls)
      {
        Match match = DowntimeMonitoringNotificationSubscriber.NodeIdRegex.Match(url);
        if (match.Success)
          return NetObjectHelper.GetObjectID(match.Value);
      }
      throw new ArgumentException(string.Format("Cannot parse NodeId from AncestorUrl. Urls: {0}.", (object) string.Join(",", urls)), nameof (urls));
    }

    internal string GetNetObjectIdColumnForSwisEntity(string instanceType)
    {
      string str = (string) null;
      if (this._netObjectTypes == null || this._netObjectTypes.Value == null)
        return (string) null;
      NetObjectTypeEx netObjectTypeEx = this._netObjectTypes.Value[instanceType].FirstOrDefault<NetObjectTypeEx>((Func<NetObjectTypeEx, bool>) (n => n.get_KeyPropertyIndex() == 0));
      if (netObjectTypeEx != null)
        str = netObjectTypeEx.get_KeyProperty();
      return str;
    }

    private ILookup<string, NetObjectTypeEx> LoadNetObjectTypesExtSwisInfo()
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2 = this._swisServiceProxyCreator.Create())
        return ((IInformationServiceProxy) iinformationServiceProxy2).Query("SELECT EntityType, Name, Prefix, KeyProperty, NameProperty, KeyPropertyIndex, CanConvert FROM Orion.NetObjectTypesExt").Rows.Cast<DataRow>().Select<DataRow, NetObjectTypeEx>((Func<DataRow, NetObjectTypeEx>) (row => new NetObjectTypeEx(row.Field<string>("EntityType"), row.Field<string>("Name"), row.Field<string>("Prefix"), row.Field<string>("KeyProperty"), row.Field<string>("NameProperty"), (int) row.Field<short>("CanConvert"), row.Field<int>("KeyPropertyIndex")))).ToLookup<NetObjectTypeEx, string>((Func<NetObjectTypeEx, string>) (k => k.get_EntityType()));
    }

    private void DeleteOldSubscriptions()
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2_1 = this._swisServiceProxyCreator.Create())
      {
        string str1 = "SELECT Uri FROM System.Subscription WHERE description = @description";
        IInformationServiceProxy2 iinformationServiceProxy2_2 = iinformationServiceProxy2_1;
        string str2 = str1;
        foreach (DataRow dataRow in ((IInformationServiceProxy) iinformationServiceProxy2_2).Query(str2, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "description",
            (object) "NetObjectDowntimeIndication"
          }
        }).Rows.Cast<DataRow>())
          ((IInformationServiceProxy) iinformationServiceProxy2_1).Delete(dataRow[0].ToString());
      }
    }

    public Task OnNotificationAsync(Notification notification)
    {
      Stopwatch stopwatch = new Stopwatch();
      try
      {
        stopwatch.Start();
        if (notification.get_SourceInstanceProperties() == null)
          throw new ArgumentNullException("sourceInstanceProperties");
        if (DowntimeMonitoringNotificationSubscriber.log.get_IsDebugEnabled())
          DowntimeMonitoringNotificationSubscriber.log.Debug((object) this.DetailInfo(notification.get_SubscriptionId(), notification.get_IndicationType(), notification.get_IndicationProperties(), notification.get_SourceInstanceProperties()));
        object obj1 = (object) null;
        notification.get_SourceInstanceProperties().TryGetValue("InstanceType", out obj1);
        if (obj1 == null)
          notification.get_SourceInstanceProperties().TryGetValue("SourceInstanceType", out obj1);
        if (!(obj1 is string instanceType))
        {
          DowntimeMonitoringNotificationSubscriber.log.Error((object) "Wrong PropertyBag data. InstanceType or SourceInstanceType are null");
          return Task.CompletedTask;
        }
        string columnForSwisEntity = this.GetNetObjectIdColumnForSwisEntity(instanceType);
        if (columnForSwisEntity == null)
        {
          DowntimeMonitoringNotificationSubscriber.log.DebugFormat("Not a supported instance type: {0}", (object) instanceType);
          return Task.CompletedTask;
        }
        object obj2;
        if (!notification.get_SourceInstanceProperties().TryGetValue(columnForSwisEntity, out obj2))
        {
          DowntimeMonitoringNotificationSubscriber.log.DebugFormat("Unable to get Entity ID. InstanceType : {0}, ID Field: {1}", (object) instanceType, (object) columnForSwisEntity);
          return Task.CompletedTask;
        }
        if (notification.get_IndicationType() == IndicationHelper.GetIndicationType((IndicationType) 2) || notification.get_IndicationType() == IndicationHelper.GetIndicationType((IndicationType) 0))
        {
          object statusObject1;
          notification.get_SourceInstanceProperties().TryGetValue("Status", out statusObject1);
          if (statusObject1 == null)
          {
            DowntimeMonitoringNotificationSubscriber.log.DebugFormat("No Status reported for InstanceType : {0}", (object) instanceType);
            return Task.CompletedTask;
          }
          if (this._nodeNetObjectIdColumn == null)
            this._nodeNetObjectIdColumn = this.GetNetObjectIdColumnForSwisEntity("Orion.Nodes");
          object statusObject2;
          notification.get_SourceInstanceProperties().TryGetValue(this._nodeNetObjectIdColumn, out statusObject2);
          if (statusObject2 == null)
          {
            DowntimeMonitoringNotificationSubscriber.log.DebugFormat("SourceBag must include NodeId. InstanceType : {0}", (object) instanceType);
            return Task.CompletedTask;
          }
          INetObjectDowntimeDAL objectDowntimeDal = this._netObjectDowntimeDal;
          NetObjectDowntime netObjectDowntime = new NetObjectDowntime();
          netObjectDowntime.set_EntityID(obj2.ToString());
          netObjectDowntime.set_NodeID(this.ExtractStatusID(statusObject2));
          netObjectDowntime.set_EntityType(instanceType);
          netObjectDowntime.set_DateTimeFrom((DateTime) notification.get_IndicationProperties()[(string) IndicationConstants.IndicationTime]);
          netObjectDowntime.set_StatusID(this.ExtractStatusID(statusObject1));
          objectDowntimeDal.Insert(netObjectDowntime);
        }
        else if (notification.get_IndicationType() == IndicationHelper.GetIndicationType((IndicationType) 1))
          this._netObjectDowntimeDal.DeleteDowntimeObjects(obj2.ToString(), instanceType);
      }
      catch (Exception ex)
      {
        DowntimeMonitoringNotificationSubscriber.log.Error((object) string.Format("Exception occured when processing incoming indication of type \"{0}\"", (object) notification.get_IndicationType()), ex);
      }
      finally
      {
        stopwatch.Stop();
        DowntimeMonitoringNotificationSubscriber.log.DebugFormat("Downtime notification has been processed in {0} miliseconds.", (object) stopwatch.ElapsedMilliseconds);
      }
      return Task.CompletedTask;
    }
  }
}
