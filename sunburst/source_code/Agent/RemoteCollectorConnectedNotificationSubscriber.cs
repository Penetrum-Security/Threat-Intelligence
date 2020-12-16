// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Agent.RemoteCollectorConnectedNotificationSubscriber
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.AgentManagement.Contract;
using SolarWinds.InformationService.Contract2;
using SolarWinds.InformationService.Contract2.PubSub;
using SolarWinds.InformationService.Linq;
using SolarWinds.InformationService.Linq.Plugins;
using SolarWinds.InformationService.Linq.Plugins.Core.Orion;
using SolarWinds.Logging;
using SolarWinds.Orion.Core.BusinessLayer.Engines;
using SolarWinds.Orion.Core.Common.Swis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SolarWinds.Orion.Core.BusinessLayer.Agent
{
  internal sealed class RemoteCollectorConnectedNotificationSubscriber : INotificationSubscriber
  {
    private static readonly Log Log = new Log();
    internal static readonly string RemoteCollectorConnectedSubscriptionQuery = "SUBSCRIBE CHANGES TO Orion.AgentManagement.Agent INCLUDE AgentId, AgentStatus WHEN (ADDED OR DELETED OR AgentStatus CHANGED OR Type CHANGED)" + string.Format(" AND (Type = {0} OR PREVIOUS(Type) = {1})", (object) 2, (object) 2);
    private readonly ISwisContextFactory _swisContextFactory;
    private readonly Action<IEngineComponent> _onRemoteCollectorStatusChanged;
    private readonly int _masterEngineId;

    public RemoteCollectorConnectedNotificationSubscriber(
      ISwisContextFactory swisContextFactory,
      Action<IEngineComponent> onRemoteCollectorStatusChanged,
      int masterEngineId)
    {
      ISwisContextFactory iswisContextFactory = swisContextFactory;
      if (iswisContextFactory == null)
        throw new ArgumentNullException(nameof (swisContextFactory));
      this._swisContextFactory = iswisContextFactory;
      Action<IEngineComponent> action = onRemoteCollectorStatusChanged;
      if (action == null)
        throw new ArgumentNullException(nameof (onRemoteCollectorStatusChanged));
      this._onRemoteCollectorStatusChanged = action;
      this._masterEngineId = masterEngineId;
    }

    public void OnIndication(
      string subscriptionId,
      string indicationType,
      PropertyBag indicationProperties,
      PropertyBag sourceInstanceProperties)
    {
      try
      {
        // ISSUE: object of a compiler-generated type is created
        // ISSUE: variable of a compiler-generated type
        RemoteCollectorConnectedNotificationSubscriber.\u003C\u003Ec__DisplayClass7_0 cDisplayClass70 = new RemoteCollectorConnectedNotificationSubscriber.\u003C\u003Ec__DisplayClass7_0();
        // ISSUE: reference to a compiler-generated field
        cDisplayClass70.\u003C\u003E4__this = this;
        AgentStatus agentStatus;
        // ISSUE: reference to a compiler-generated field
        if (RemoteCollectorConnectedNotificationSubscriber.GetRequiredProperties(sourceInstanceProperties, out cDisplayClass70.agentId, out agentStatus))
        {
          // ISSUE: reference to a compiler-generated field
          RemoteCollectorConnectedNotificationSubscriber.Log.DebugFormat("Remote Collector on Agent #{0} has changed status", (object) cDisplayClass70.agentId);
          using (CoreSwisContext coreSwisContext = this._swisContextFactory.Create())
          {
            ParameterExpression parameterExpression1;
            ParameterExpression parameterExpression2;
            // ISSUE: method reference
            // ISSUE: method reference
            // ISSUE: field reference
            // ISSUE: method reference
            // ISSUE: method reference
            // ISSUE: field reference
            // ISSUE: method reference
            int? nullable = ((IQueryable<EngineProperties>) ((SwisContext) coreSwisContext).Entity<EngineProperties>()).Where<EngineProperties>(Expression.Lambda<Func<EngineProperties, bool>>((Expression) Expression.AndAlso((Expression) Expression.AndAlso((Expression) Expression.Equal((Expression) Expression.Property((Expression) parameterExpression1, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (EngineProperties.get_PropertyName))), (Expression) Expression.Constant((object) "AgentId", typeof (string))), (Expression) Expression.Equal((Expression) Expression.Property((Expression) parameterExpression1, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (EngineProperties.get_PropertyValue))), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass70, typeof (RemoteCollectorConnectedNotificationSubscriber.\u003C\u003Ec__DisplayClass7_0)), FieldInfo.GetFieldFromHandle(__fieldref (RemoteCollectorConnectedNotificationSubscriber.\u003C\u003Ec__DisplayClass7_0.agentId))))), (Expression) Expression.Equal((Expression) Expression.Property((Expression) Expression.Property((Expression) parameterExpression1, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (EngineProperties.get_Engine))), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SolarWinds.InformationService.Linq.Plugins.Core.Orion.Engines.get_MasterEngineID))), (Expression) Expression.Convert((Expression) Expression.Field((Expression) Expression.Constant((object) this, typeof (RemoteCollectorConnectedNotificationSubscriber)), FieldInfo.GetFieldFromHandle(__fieldref (RemoteCollectorConnectedNotificationSubscriber._masterEngineId))), typeof (int?)))), parameterExpression1)).Select<EngineProperties, int?>(Expression.Lambda<Func<EngineProperties, int?>>((Expression) Expression.Property((Expression) parameterExpression2, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (EngineProperties.get_EngineID))), parameterExpression2)).FirstOrDefault<int?>();
            if (nullable.HasValue)
            {
              // ISSUE: reference to a compiler-generated field
              RemoteCollectorConnectedNotificationSubscriber.Log.DebugFormat("Remote Collector engine #{0} on Agent #{1} changes status to {2}", (object) nullable, (object) cDisplayClass70.agentId, (object) agentStatus);
              this._onRemoteCollectorStatusChanged((IEngineComponent) new RemoteCollectorConnectedNotificationSubscriber.EngineComponent(nullable.Value, agentStatus));
            }
            else
            {
              // ISSUE: reference to a compiler-generated field
              RemoteCollectorConnectedNotificationSubscriber.Log.DebugFormat("Remote Collector on Agent #{0} in not connected to local master engine #{1}", (object) cDisplayClass70.agentId, (object) this._masterEngineId);
            }
          }
        }
        else
          RemoteCollectorConnectedNotificationSubscriber.Log.DebugFormat("Remote Collector Agent connection indication does not have all the required properties (AgentId and AgentStatus)", Array.Empty<object>());
      }
      catch (Exception ex)
      {
        RemoteCollectorConnectedNotificationSubscriber.Log.Error((object) "Unexpected error in processing Agent Remote Collector Agent connection indication", ex);
      }
    }

    private static bool GetRequiredProperties(
      PropertyBag sourceInstanceProperties,
      out string agentId,
      out AgentStatus agentStatus)
    {
      agentId = (string) null;
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref agentStatus = 0;
      object obj1;
      object obj2;
      if (sourceInstanceProperties == null || !((Dictionary<string, object>) sourceInstanceProperties).TryGetValue("AgentId", out obj1) || (obj1 == null || !((Dictionary<string, object>) sourceInstanceProperties).TryGetValue("AgentStatus", out obj2)) || obj2 == null)
        return false;
      agentId = obj1.ToString();
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref agentStatus = (int) (AgentStatus) obj2;
      return true;
    }

    private class EngineComponent : IEngineComponent
    {
      public EngineComponent(int engineId, AgentStatus agentStatus)
      {
        this.EngineId = engineId;
        this.AgentStatus = agentStatus;
      }

      private AgentStatus AgentStatus { get; }

      public int EngineId { get; }

      public EngineComponentStatus GetStatus()
      {
        return this.AgentStatus != 1 ? EngineComponentStatus.Down : EngineComponentStatus.Up;
      }
    }
  }
}
