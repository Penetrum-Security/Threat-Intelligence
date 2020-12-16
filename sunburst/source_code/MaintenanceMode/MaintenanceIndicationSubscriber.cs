// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintenanceMode.MaintenanceIndicationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common.Indications;
using SolarWinds.Orion.Core.Common.InformationService;
using SolarWinds.Orion.Core.Models.MaintenanceMode;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.MaintenanceMode
{
  internal class MaintenanceIndicationSubscriber : ISubscriber, IDisposable
  {
    private static readonly Log log = new Log();
    private readonly IMaintenanceManager manager;
    private readonly ISubscriptionManager _subscriptionManager;
    private ISubscription _subscriptionId;
    private const string SubscriptionQuery = "SUBSCRIBE CHANGES TO Orion.MaintenancePlanAssignment";

    public MaintenanceIndicationSubscriber()
      : this((IMaintenanceManager) new MaintenanceManager(InformationServiceProxyPoolCreatorFactory.GetSystemCreator(), (IMaintenanceModePlanDAL) new MaintenanceModePlanDAL()), SubscriptionManager.get_Instance())
    {
    }

    public MaintenanceIndicationSubscriber(
      IMaintenanceManager manager,
      ISubscriptionManager subscriptionManager)
    {
      this.manager = manager;
      this._subscriptionManager = subscriptionManager;
    }

    public void Start()
    {
      try
      {
        this._subscriptionId = this.Subscribe();
      }
      catch (Exception ex)
      {
        MaintenanceIndicationSubscriber.log.ErrorFormat("Unable to start maintenance mode service. Unmanage functionality may be affected. {0}", (object) ex);
        throw;
      }
    }

    internal MaintenancePlanAssignment CreateAssignment(
      IDictionary<string, object> sourceInstanceProperties)
    {
      if (sourceInstanceProperties == null)
        throw new ArgumentNullException(nameof (sourceInstanceProperties));
      if (!sourceInstanceProperties.Keys.Any<string>())
        throw new ArgumentException(nameof (sourceInstanceProperties));
      MaintenancePlanAssignment maintenancePlanAssignment = new MaintenancePlanAssignment();
      maintenancePlanAssignment.set_ID(Convert.ToInt32(sourceInstanceProperties["ID"]));
      maintenancePlanAssignment.set_EntityType(Convert.ToString(sourceInstanceProperties["EntityType"]));
      maintenancePlanAssignment.set_EntityID(Convert.ToInt32(sourceInstanceProperties["EntityID"]));
      maintenancePlanAssignment.set_MaintenancePlanID(Convert.ToInt32(sourceInstanceProperties["MaintenancePlanID"]));
      return maintenancePlanAssignment;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (this._subscriptionId == null)
        return;
      try
      {
        this.Unsubscribe(this._subscriptionId);
        this._subscriptionId = (ISubscription) null;
      }
      catch (Exception ex)
      {
        MaintenanceIndicationSubscriber.log.ErrorFormat("Error unsubscribing subscription '{0}'. {1}", (object) this._subscriptionId, (object) ex);
      }
    }

    private ISubscription Subscribe()
    {
      SubscriptionId subscriptionId;
      ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (MaintenanceIndicationSubscriber).FullName, (Scope) 0);
      SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
      subscriberConfiguration1.set_SubscriptionQuery("SUBSCRIBE CHANGES TO Orion.MaintenancePlanAssignment");
      SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
      return this._subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2);
    }

    private void Unsubscribe(ISubscription subscriptionId)
    {
      this._subscriptionManager.Unsubscribe(subscriptionId.get_Id());
    }

    ~MaintenanceIndicationSubscriber()
    {
      this.Dispose(false);
    }

    public Task OnNotificationAsync(Notification notification)
    {
      if (!this._subscriptionId.get_Id().Equals((object) notification.get_SubscriptionId()))
        return Task.CompletedTask;
      MaintenanceIndicationSubscriber.log.DebugFormat("Received maintenance mode indication '{0}'.", (object) notification.get_IndicationType());
      MaintenanceIndicationSubscriber.log.DebugFormat("Indication Properties: {0}", (object) notification.get_IndicationProperties());
      MaintenanceIndicationSubscriber.log.DebugFormat("Source Instance Properties: {0}", (object) notification.get_SourceInstanceProperties());
      try
      {
        MaintenancePlanAssignment assignment = this.CreateAssignment(notification.get_SourceInstanceProperties());
        if (IndicationHelper.GetIndicationType((IndicationType) 0).Equals(notification.get_IndicationType()))
          this.manager.Unmanage(assignment);
        else if (IndicationHelper.GetIndicationType((IndicationType) 1).Equals(notification.get_IndicationType()))
          this.manager.Remanage(assignment);
        else
          IndicationHelper.GetIndicationType((IndicationType) 2).Equals(notification.get_IndicationType());
      }
      catch (Exception ex)
      {
        MaintenanceIndicationSubscriber.log.ErrorFormat("Unable to process maintenance mode indication. {0}", (object) ex);
        throw;
      }
      return Task.CompletedTask;
    }
  }
}
