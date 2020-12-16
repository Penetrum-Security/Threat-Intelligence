// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.MaintUpdateNotifySvc.VersionInfo
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
  public class VersionInfo : INotifyPropertyChanged
  {
    private string moduleNameField;
    private DateTime dateReleasedField;
    private string productTagField;
    private string linkField;
    private string releaseNotesField;
    private UpdateMessage messageField;
    private ModuleStatusType moduleStatusField;
    private string versionField;
    private string hotfixField;

    [XmlElement(Order = 0)]
    public string ModuleName
    {
      get
      {
        return this.moduleNameField;
      }
      set
      {
        this.moduleNameField = value;
        this.RaisePropertyChanged(nameof (ModuleName));
      }
    }

    [XmlElement(Order = 1)]
    public DateTime DateReleased
    {
      get
      {
        return this.dateReleasedField;
      }
      set
      {
        this.dateReleasedField = value;
        this.RaisePropertyChanged(nameof (DateReleased));
      }
    }

    [XmlElement(Order = 2)]
    public string ProductTag
    {
      get
      {
        return this.productTagField;
      }
      set
      {
        this.productTagField = value;
        this.RaisePropertyChanged(nameof (ProductTag));
      }
    }

    [XmlElement(Order = 3)]
    public string Link
    {
      get
      {
        return this.linkField;
      }
      set
      {
        this.linkField = value;
        this.RaisePropertyChanged(nameof (Link));
      }
    }

    [XmlElement(Order = 4)]
    public string ReleaseNotes
    {
      get
      {
        return this.releaseNotesField;
      }
      set
      {
        this.releaseNotesField = value;
        this.RaisePropertyChanged(nameof (ReleaseNotes));
      }
    }

    [XmlElement(Order = 5)]
    public UpdateMessage Message
    {
      get
      {
        return this.messageField;
      }
      set
      {
        this.messageField = value;
        this.RaisePropertyChanged(nameof (Message));
      }
    }

    [XmlElement(Order = 6)]
    public ModuleStatusType ModuleStatus
    {
      get
      {
        return this.moduleStatusField;
      }
      set
      {
        this.moduleStatusField = value;
        this.RaisePropertyChanged(nameof (ModuleStatus));
      }
    }

    [XmlElement(Order = 7)]
    public string Version
    {
      get
      {
        return this.versionField;
      }
      set
      {
        this.versionField = value;
        this.RaisePropertyChanged(nameof (Version));
      }
    }

    [XmlElement(Order = 8)]
    public string Hotfix
    {
      get
      {
        return this.hotfixField;
      }
      set
      {
        this.hotfixField = value;
        this.RaisePropertyChanged(nameof (Hotfix));
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
