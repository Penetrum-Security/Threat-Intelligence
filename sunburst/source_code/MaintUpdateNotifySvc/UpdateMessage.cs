// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc.UpdateMessage
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc
{
  [GeneratedCode("System.Xml", "4.8.3761.0")]
  [DebuggerStepThrough]
  [DesignerCategory("code")]
  [XmlType(Namespace = "http://www.solarwinds.com/contracts/IMaintUpdateNotifySvc/2009/09")]
  [Serializable]
  public class UpdateMessage : INotifyPropertyChanged
  {
    private DateTime publishDateField;
    private string maintenanceMessageField;

    [XmlElement(Order = 0)]
    public DateTime PublishDate
    {
      get
      {
        return this.publishDateField;
      }
      set
      {
        this.publishDateField = value;
        this.RaisePropertyChanged(nameof (PublishDate));
      }
    }

    [XmlElement(Order = 1)]
    public string MaintenanceMessage
    {
      get
      {
        return this.maintenanceMessageField;
      }
      set
      {
        this.maintenanceMessageField = value;
        this.RaisePropertyChanged(nameof (MaintenanceMessage));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
