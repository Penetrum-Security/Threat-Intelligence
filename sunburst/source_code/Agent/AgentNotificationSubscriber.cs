// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.AgentNotificationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.PubSub;
using SolarWinds.Orion.Swis.PubSub.InformationService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal class AgentNotificationSubscriber : ISubscriber
  {
    private static readonly Log log = new Log();
    private List<SubscriptionId> subscriptionIds = new List<SubscriptionId>();
    private readonly Tuple<string, string>[] _subscriptionQueriesDescriptionPairs = new Tuple<string, string>[4]
    {
      new Tuple<string, string>("SUBSCRIBE CHANGES TO Orion.AgentManagement.Agent INCLUDE AgentId WHEN AgentStatus CHANGES OR ConnectionStatus CHANGES", "AgentStatusOrConnectionChange"),
      new Tuple<string, string>("SUBSCRIBE CHANGES TO Orion.AgentManagement.Agent INCLUDE AgentId WHEN ADDED", "AgentAdded"),
      new Tuple<string, string>("SUBSCRIBE CHANGES TO Orion.AgentManagement.AgentPlugin INCLUDE AgentId WHEN Status CHANGES", "AgentPluginStatusChanged"),
      new Tuple<string, string>("SUBSCRIBE CHANGES TO Orion.AgentManagement.AgentPlugin INCLUDE AgentId WHEN ADDED", "AgentPluginAdded")
    };
    private ISubscriptionManager _subscriptionManager;
    private Action<int> onIndication;

    public AgentNotificationSubscriber(Action<int> onIndication)
      : this(onIndication, SubscriptionManager.get_Instance())
    {
    }

    public AgentNotificationSubscriber(
      Action<int> onIndication,
      ISubscriptionManager subscriptionManager)
    {
      this.onIndication = onIndication;
      this._subscriptionManager = subscriptionManager;
    }

    public void Subscribe()
    {
      this.Unsubscribe();
      foreach (Tuple<string, string> queriesDescriptionPair in this._subscriptionQueriesDescriptionPairs)
      {
        SubscriptionId subscriptionId;
        ((SubscriptionId) ref subscriptionId).\u002Ector("Core", typeof (AgentNotificationSubscriber).FullName + "." + queriesDescriptionPair.Item2, (Scope) 0);
        SubscriberConfiguration subscriberConfiguration1 = new SubscriberConfiguration();
        subscriberConfiguration1.set_SubscriptionQuery(queriesDescriptionPair.Item1);
        SubscriberConfiguration subscriberConfiguration2 = subscriberConfiguration1;
        this.subscriptionIds.Add(this._subscriptionManager.Subscribe(subscriptionId, (ISubscriber) this, subscriberConfiguration2).get_Id());
      }
    }

    public void Unsubscribe()
    {
      while (this.subscriptionIds.Count > 0)
      {
        SubscriptionId subscriptionId = this.subscriptionIds[0];
        try
        {
          this._subscriptionManager.Unsubscribe(subscriptionId);
        }
        catch (Exception ex)
        {
          AgentNotificationSubscriber.log.ErrorFormat("Error unsubscribing 'agent change' subscription '{0}'. {1}", (object) subscriptionId, (object) ex);
        }
        this.subscriptionIds.RemoveAt(0);
      }
    }

    public bool IsSubscribed()
    {
      return this.subscriptionIds.Count > 0;
    }

    public Task OnNotificationAsync(Notification notification)
    {
      if (!this.subscriptionIds.Contains(notification.get_SubscriptionId()))
        return Task.CompletedTask;
      try
      {
        int int32 = Convert.ToInt32(notification.get_SourceInstanceProperties()["AgentId"]);
        if (this.onIndication != null)
        {
          if (int32 != 0)
            this.onIndication(int32);
        }
      }
      catch (Exception ex)
      {
        AgentNotificationSubscriber.log.ErrorFormat("Error processing agent notification. {0}", (object) ex);
        throw;
      }
      return Task.CompletedTask;
    }
  }
}
