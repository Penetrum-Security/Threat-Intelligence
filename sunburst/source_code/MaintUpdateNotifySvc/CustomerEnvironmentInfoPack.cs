// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc.CustomerEnvironmentInfoPack
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
  public class CustomerEnvironmentInfoPack : INotifyPropertyChanged
  {
    private ModuleInfo[] modulesField;
    private string oSVersionField;
    private string orionDBVersionField;
    private string sQLVersionField;
    private Guid customerUniqueIdField;
    private DateTime lastUpdateCheckField;

    [XmlArray(Order = 0)]
    [XmlArrayItem("Module")]
    public ModuleInfo[] Modules
    {
      get
      {
        return this.modulesField;
      }
      set
      {
        this.modulesField = value;
        this.RaisePropertyChanged(nameof (Modules));
      }
    }

    [XmlAttribute]
    public string OSVersion
    {
      get
      {
        return this.oSVersionField;
      }
      set
      {
        this.oSVersionField = value;
        this.RaisePropertyChanged(nameof (OSVersion));
      }
    }

    [XmlAttribute]
    public string OrionDBVersion
    {
      get
      {
        return this.orionDBVersionField;
      }
      set
      {
        this.orionDBVersionField = value;
        this.RaisePropertyChanged(nameof (OrionDBVersion));
      }
    }

    [XmlAttribute]
    public string SQLVersion
    {
      get
      {
        return this.sQLVersionField;
      }
      set
      {
        this.sQLVersionField = value;
        this.RaisePropertyChanged(nameof (SQLVersion));
      }
    }

    [XmlAttribute]
    public Guid CustomerUniqueId
    {
      get
      {
        return this.customerUniqueIdField;
      }
      set
      {
        this.customerUniqueIdField = value;
        this.RaisePropertyChanged(nameof (CustomerUniqueId));
      }
    }

    [XmlAttribute]
    public DateTime LastUpdateCheck
    {
      get
      {
        return this.lastUpdateCheckField;
      }
      set
      {
        this.lastUpdateCheckField = value;
        this.RaisePropertyChanged(nameof (LastUpdateCheck));
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
