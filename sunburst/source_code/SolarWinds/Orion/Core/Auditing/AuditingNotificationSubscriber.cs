// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.Auditing.AuditingNotificationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Common.Utility;
using SolarWinds.InformationService.Contract2;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common;
using SolarWinds.Orion.Core.Common.i18n;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Common.Swis;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.Auditing
{
  internal class AuditingNotificationSubscriber : ISubscriber
  {
    private static readonly Log log = new Log(typeof (AuditingNotificationSubscriber));
    private readonly AuditingPluginManager auditingPlugins = new AuditingPluginManager();
    private readonly AuditingDAL auditingDAL = new AuditingDAL();
    private bool checkAuditingSetting = true;
    private readonly ConcurrentDictionary<string, IEnumerable<IAuditing2>> subscriptionIdToAuditingInstances = new ConcurrentDictionary<string, IEnumerable<IAuditing2>>();
    private const string AuditingIndications = "AuditingIndications";
    private IPublisherManager notificationPublisherManager;
    private readonly ISubscriptionManager subscriptionManager;

    public AuditingNotificationSubscriber()
      : this(SubscriptionManager.get_Instance())
    {
    }

    public AuditingNotificationSubscriber(ISubscriptionManager subscriptionManager)
    {
      ISubscriptionManager isubscriptionManager = subscriptionManager;
      if (isubscriptionManager == null)
        throw new ArgumentNullException(nameof (subscriptionManager));
      this.subscriptionManager = isubscriptionManager;
    }

    protected bool AuditingTrailsEnabled { get; private set; }

    private void PublishModificationOfAuditingEvents(
      AuditDatabaseDecoratedContainer auditDatabaseDecoratedContainer,
      int insertedId)
    {
      if (this.notificationPublisherManager == null)
        this.notificationPublisherManager = PublisherClient.get_Instance();
      this.notificationPublisherManager.Publish((INotification) new Notification("System.InstanceCreated", (IDictionary<string, object>) IndicationHelper.GetIndicationProperties(), (IDictionary<string, object>) new Dictionary<string, object>()
      {
        {
          "ActionType",
          (object) ((object) auditDatabaseDecoratedContainer.get_ActionType()).ToString()
        },
        {
          "AuditEventId",
          (object) insertedId
        },
        {
          "InstanceType",
          (object) "Orion.AuditingEvents"
        },
        {
          "OriginalAccountId",
          (object) auditDatabaseDecoratedContainer.AccountId
        }
      }));
    }

    private string FormatPropertyData(string prefix, string key, object value)
    {
      return prefix + key + ": " + (value ?? (object) "null") + Environment.NewLine;
    }

    public void Start()
    {
      try
      {
        this.AuditingTrailsEnabled = (bool) SettingsDAL.GetCurrent<bool>("SWNetPerfMon-AuditingTrails", (M0) 1);
      }
      catch (Exception ex)
      {
        AuditingNotificationSubscriber.log.FatalFormat("Auditing setting error - will be forciby enabled. {0}", (object) ex);
        this.AuditingTrailsEnabled = true;
      }
      this.checkAuditingSetting = true;
      this.auditingPlugins.Initialize();
      Scheduler.get_Instance().Add(new ScheduledTask("AuditingIndications", new TimerCallback(this.Subscribe), (object) null, TimeSpan.FromSeconds(1.0), TimeSpan.FromMinutes(1.0)), true);
    }

    public void Stop()
    {
      Scheduler.get_Instance().Remove("AuditingIndications");
    }

    private void Subscribe(object state)
    {
      AuditingNotificationSubscriber.log.Debug((object) "Subscribing auditing indications..");
      try
      {
        AuditingNotificationSubscriber.DeleteOldSubscriptions();
      }
      catch (Exception ex)
      {
        AuditingNotificationSubscriber.log.Warn((object) "Exception deleting old subscriptions:", ex);
      }
      HashSet<string> stringSet = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      using (IEnumerator<IAuditing2> enumerator = this.auditingPlugins.AuditingInstances.GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          IAuditing2 current = enumerator.Current;
          if (current is IAuditingMultiSubscription multiSubscription)
          {
            foreach (string subscriptionQuery in multiSubscription.GetSubscriptionQueries())
              stringSet.Add(subscriptionQuery);
          }
          else
            stringSet.Add(current.GetSubscriptionQuery());
        }
      }
      foreach (string query in stringSet)
      {
        try
        {
          SubscriptionId subscriptionId;
          ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (AuditingNotificationSubscriber).FullName + "." + this.GetHashFromQuery(query), (Scope) 0);
          SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
          subscriberConfiguration1.set_SubscriptionQuery(query);
          SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
          AuditingNotificationSubscriber.log.DebugFormat("Subscribing '{0}'", (object) query);
          SubscriptionId id = this.subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2).get_Id();
          string query1 = query;
          this.subscriptionIdToAuditingInstances.TryAdd(id.ToString(), ((IEnumerable<IAuditing2>) this.auditingPlugins.AuditingInstances).Where<IAuditing2>((Func<IAuditing2, bool>) (instance =>
          {
            try
            {
              return string.Compare(query1, instance.GetSubscriptionQuery(), StringComparison.OrdinalIgnoreCase) == 0;
            }
            catch (NotImplementedException ex)
            {
              return instance is IAuditingMultiSubscription multiSubscription && ((IEnumerable<string>) multiSubscription.GetSubscriptionQueries()).Contains<string>(query1);
            }
          })));
          AuditingNotificationSubscriber.log.DebugFormat("Subscribed '{0}' with {1} number of auditing instances.", (object) query, (object) this.subscriptionIdToAuditingInstances[id.ToString()].Count<IAuditing2>());
        }
        catch (Exception ex)
        {
          AuditingNotificationSubscriber.log.ErrorFormat("Unable to subscribe auditing instance with query '{0}'. {1}", (object) query, (object) ex);
        }
      }
      AuditingNotificationSubscriber.log.InfoFormat("Auditing pub/sub subscription succeeded.", Array.Empty<object>());
      Scheduler.get_Instance().Remove("AuditingIndications");
    }

    private static void DeleteOldSubscriptions()
    {
      using (IInformationServiceProxy2 iinformationServiceProxy2_1 = ((IInformationServiceProxyCreator) SwisConnectionProxyPool.GetSystemCreator()).Create())
      {
        string str1 = "SELECT Uri FROM System.Subscription WHERE description = @description";
        IInformationServiceProxy2 iinformationServiceProxy2_2 = iinformationServiceProxy2_1;
        string str2 = str1;
        foreach (DataRow dataRow in ((IInformationServiceProxy) iinformationServiceProxy2_2).Query(str2, (IDictionary<string, object>) new Dictionary<string, object>()
        {
          {
            "description",
            (object) "AuditingIndications"
          }
        }).Rows.Cast<DataRow>())
          ((IInformationServiceProxy) iinformationServiceProxy2_1).Delete(dataRow[0].ToString());
      }
    }

    private string GetHashFromQuery(string query)
    {
      using (SHA1 shA1 = SHA1.Create())
      {
        byte[] bytes = Encoding.ASCII.GetBytes(query);
        byte[] hash = shA1.ComputeHash(bytes);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte num in hash)
          stringBuilder.Append(num.ToString("x2"));
        return stringBuilder.ToString();
      }
    }

    public Task OnNotificationAsync(Notification notification)
    {
      if (AuditingNotificationSubscriber.log.get_IsDebugEnabled())
        AuditingNotificationSubscriber.log.DebugFormat("OnNotification type: {0} SubscriptionId: {1}", (object) notification.get_IndicationType(), (object) notification.get_SubscriptionId());
      PropertyBag propertyBag1 = new PropertyBag(notification.get_SourceInstanceProperties());
      PropertyBag propertyBag2 = new PropertyBag(notification.get_IndicationProperties());
      if (this.checkAuditingSetting)
      {
        try
        {
          object obj;
          if (IndicationHelper.GetIndicationType((IndicationType) 2) == notification.get_IndicationType() && propertyBag1 != null && ((string) propertyBag1.TryGet<string>("SettingsID") == "SWNetPerfMon-AuditingTrails" && (string) propertyBag1.TryGet<string>("InstanceType") == "Orion.Settings") && ((Dictionary<string, object>) propertyBag1).TryGetValue("CurrentValue", out obj))
            this.AuditingTrailsEnabled = Convert.ToBoolean(obj);
          else if (!this.AuditingTrailsEnabled)
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
          AuditingNotificationSubscriber.log.FatalFormat("Auditing check error - will be forciby enabled. {0}", (object) ex);
          this.AuditingTrailsEnabled = true;
          this.checkAuditingSetting = false;
        }
      }
      AuditNotificationContainer auditNotificationContainer = new AuditNotificationContainer(notification.get_IndicationType(), propertyBag2, propertyBag1);
      IEnumerable<IAuditing2> iauditing2s;
      if (this.subscriptionIdToAuditingInstances.TryGetValue(notification.get_SubscriptionId().ToString(), out iauditing2s))
      {
        using (IEnumerator<IAuditing2> enumerator1 = iauditing2s.GetEnumerator())
        {
          while (((IEnumerator) enumerator1).MoveNext())
          {
            IAuditing2 current1 = enumerator1.Current;
            try
            {
              if (AuditingNotificationSubscriber.log.get_IsTraceEnabled())
                AuditingNotificationSubscriber.log.TraceFormat("Trying plugin {0}", new object[1]
                {
                  (object) current1
                });
              IEnumerable<AuditDataContainer> source = ((IAuditing) current1).ComposeDataContainers(auditNotificationContainer);
              if (source != null)
              {
                if (AuditingNotificationSubscriber.log.get_IsTraceEnabled())
                  AuditingNotificationSubscriber.log.Trace((object) "Storing notification.");
                CultureInfo currentUiCulture = Thread.CurrentThread.CurrentUICulture;
                try
                {
                  Thread.CurrentThread.CurrentUICulture = LocaleConfiguration.GetNonNeutralLocale(LocaleConfiguration.get_PrimaryLocale());
                }
                catch (Exception ex)
                {
                  AuditingNotificationSubscriber.log.Warn((object) "Unable set CurrentUICulture to PrimaryLocale.", ex);
                }
                using (IEnumerator<AuditDataContainer> enumerator2 = source.Select<AuditDataContainer, AuditDataContainer>((Func<AuditDataContainer, AuditDataContainer>) (composedDataContainer => new AuditDataContainer(composedDataContainer, auditNotificationContainer.get_AccountId()))).GetEnumerator())
                {
                  while (((IEnumerator) enumerator2).MoveNext())
                  {
                    AuditDataContainer current2 = enumerator2.Current;
                    AuditDatabaseDecoratedContainer decoratedContainer = new AuditDatabaseDecoratedContainer(current2, auditNotificationContainer, ((IAuditing) current1).GetMessage(current2));
                    int insertedId = this.auditingDAL.StoreNotification(decoratedContainer);
                    this.PublishModificationOfAuditingEvents(decoratedContainer, insertedId);
                  }
                }
                try
                {
                  Thread.CurrentThread.CurrentUICulture = currentUiCulture;
                }
                catch (Exception ex)
                {
                  AuditingNotificationSubscriber.log.Warn((object) "Unable set CurrentUICulture back to original locale.", ex);
                }
              }
              else if (AuditingNotificationSubscriber.log.get_IsTraceEnabled())
                AuditingNotificationSubscriber.log.Trace((object) "ComposeDataContainers returned null.");
            }
            catch (Exception ex)
            {
              string seed = string.Empty;
              if (propertyBag2 != null)
                seed = ((IEnumerable<KeyValuePair<string, object>>) propertyBag2).Aggregate<KeyValuePair<string, object>, string>(Environment.NewLine, (Func<string, KeyValuePair<string, object>, string>) ((current, item) => current + this.FormatPropertyData("Indication Property: ", item.Key, item.Value)));
              if (propertyBag1 != null)
                seed = ((IEnumerable<KeyValuePair<string, object>>) propertyBag1).Aggregate<KeyValuePair<string, object>, string>(seed, (Func<string, KeyValuePair<string, object>, string>) ((current, item) => current + this.FormatPropertyData("SourceInstance Property: ", item.Key, item.Value)));
              AuditingNotificationSubscriber.log.ErrorFormat("Auditing translation failed. IndicationType: {0}, {1} PluginName: {2}, subscriptionId: {3} Exception: {4}", new object[5]
              {
                (object) notification.get_IndicationType(),
                (object) seed,
                (object) ((IAuditing) current1).get_PluginName(),
                (object) notification.get_SubscriptionId(),
                (object) ex
              });
            }
          }
        }
      }
      else if (AuditingNotificationSubscriber.log.get_IsDebugEnabled())
        AuditingNotificationSubscriber.log.DebugFormat("No auditing instances has been registered yet for subscriptionId '{0}'", (object) notification.get_SubscriptionId().ToString());
      return Task.CompletedTask;
    }
  }
}
