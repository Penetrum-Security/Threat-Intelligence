// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.SNMPManagerWrapper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Net.SNMP;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class SNMPManagerWrapper
  {
    private SNMPManager _manager = new SNMPManager();
    private Queue<SNMPRequest> _Query = new Queue<SNMPRequest>();
    private BackgroundWorker bgworker = new BackgroundWorker();
    private bool _doWork = true;
    private const int _maxCount = 5;

    public SNMPManagerWrapper()
    {
      this.bgworker.DoWork += new DoWorkEventHandler(this.bgworker_DoWork);
    }

    private void bgworker_DoWork(object sender, DoWorkEventArgs e)
    {
      while (this._doWork)
      {
        bool flag = false;
        lock (this._Query)
        {
          if (this._Query.Count > 0)
          {
            if (this._manager.get_OutstandingQueries() <= 5)
            {
              SNMPRequest snR = this._Query.Dequeue();
              int err = 0;
              string ErrDes = string.Empty;
              this.BeginQuery(snR, true, out err, out ErrDes);
            }
            else
              flag = true;
          }
          else
            flag = true;
        }
        if (flag)
          Thread.Sleep(100);
      }
    }

    public bool BeginQuery(SNMPRequest snR, bool used, out int err, out string ErrDes)
    {
      lock (this._Query)
      {
        if (this._manager.get_OutstandingQueries() <= 5)
          return this._manager.BeginQuery(snR, used, ref err, ref ErrDes);
        this._Query.Enqueue(snR);
        if (!this.bgworker.IsBusy)
          this.bgworker.RunWorkerAsync();
        err = 0;
        ErrDes = string.Empty;
        return true;
      }
    }

    public int OutstandingQueries
    {
      get
      {
        lock (this._Query)
          return this._manager.get_OutstandingQueries() + this._Query.Count;
      }
    }

    public SNMPRequest DefaultInfo
    {
      get
      {
        return this._manager.get_DefaultInfo();
      }
    }

    public SNMPResponse Query(SNMPRequest snR, bool usDI)
    {
      return this._manager.Query(snR, usDI);
    }

    public void Cancel()
    {
      this._manager.Cancel();
      this._doWork = false;
    }

    public void Dispose()
    {
      this._manager.Dispose();
      this._doWork = false;
    }
  }
}
