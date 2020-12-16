// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.Auditing.AuditDatabaseDecoratedContainer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Indications;
using System;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.Auditing
{
  public class AuditDatabaseDecoratedContainer : AuditDataContainer
  {
    private string accountId;
    private DateTime indicationTime;
    private string message;

    public AuditDatabaseDecoratedContainer(
      AuditDataContainer adc,
      AuditNotificationContainer anc,
      string message)
    {
      this.\u002Ector(adc);
      if (anc == null)
        throw new ArgumentNullException(nameof (anc));
      if (string.IsNullOrEmpty(message))
        throw new ArgumentNullException(nameof (message));
      object obj;
      this.accountId = anc.get_IndicationProperties() == null || !((Dictionary<string, object>) anc.get_IndicationProperties()).TryGetValue((string) IndicationConstants.AccountId, out obj) ? "SYSTEM" : obj as string;
      this.indicationTime = (DateTime) anc.GetIndicationPropertyValue<DateTime>(nameof (IndicationTime));
      this.indicationTime = this.indicationTime.Kind != DateTimeKind.Unspecified ? this.indicationTime.ToUniversalTime() : DateTime.SpecifyKind(this.indicationTime, DateTimeKind.Utc);
      this.message = message;
    }

    public string AccountId
    {
      get
      {
        return this.accountId;
      }
    }

    public DateTime IndicationTime
    {
      get
      {
        return this.indicationTime;
      }
    }

    public string Message
    {
      get
      {
        return this.message;
      }
    }
  }
}
