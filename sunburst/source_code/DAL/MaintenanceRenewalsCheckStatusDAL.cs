// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.MaintenanceRenewalsCheckStatusDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Discovery.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public sealed class MaintenanceRenewalsCheckStatusDAL
  {
    private static readonly Log log = new Log();

    public DateTime? LastUpdateCheck { get; set; }

    public DateTime? NextUpdateCheck { get; set; }

    public MaintenanceRenewalsCheckStatusDAL()
    {
      this.LastUpdateCheck = new DateTime?();
      this.NextUpdateCheck = new DateTime?();
    }

    public static MaintenanceRenewalsCheckStatusDAL GetCheckStatus()
    {
      try
      {
        MaintenanceRenewalsCheckStatusDAL renewalsCheckStatusDal = new MaintenanceRenewalsCheckStatusDAL();
        renewalsCheckStatusDal.LoadFromDB();
        return renewalsCheckStatusDal;
      }
      catch (ResultCountException ex)
      {
        MaintenanceRenewalsCheckStatusDAL.log.DebugFormat("Can't find maintenance renewals check status record in DB.", Array.Empty<object>());
        return (MaintenanceRenewalsCheckStatusDAL) null;
      }
    }

    public static MaintenanceRenewalsCheckStatusDAL Insert(
      DateTime? lastCheck,
      DateTime? nextCheck)
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("INSERT INTO MaintenanceRenewalsCheckStatus (LastUpdateCheck, NextUpdateCheck)\r\n                                                        VALUES (@LastUpdateCheck, @NextUpdateCheck)"))
      {
        textCommand.Parameters.AddWithValue("@LastUpdateCheck", lastCheck.HasValue ? (object) lastCheck : (object) DBNull.Value);
        textCommand.Parameters.AddWithValue("@NextUpdateCheck", nextCheck.HasValue ? (object) nextCheck : (object) DBNull.Value);
        if (SqlHelper.ExecuteNonQuery(textCommand) == 0)
          return (MaintenanceRenewalsCheckStatusDAL) null;
        return new MaintenanceRenewalsCheckStatusDAL()
        {
          LastUpdateCheck = lastCheck,
          NextUpdateCheck = nextCheck
        };
      }
    }

    public bool Update()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("UPDATE MaintenanceRenewalsCheckStatus SET LastUpdateCheck=@LastUpdateCheck, NextUpdateCheck=@NextUpdateCheck"))
      {
        textCommand.Parameters.AddWithValue("@LastUpdateCheck", this.LastUpdateCheck.HasValue ? (object) this.LastUpdateCheck : (object) DBNull.Value);
        textCommand.Parameters.AddWithValue("@NextUpdateCheck", this.NextUpdateCheck.HasValue ? (object) this.NextUpdateCheck : (object) DBNull.Value);
        return SqlHelper.ExecuteNonQuery(textCommand) > 0;
      }
    }

    public static void SetLastUpdateCheck(double timeout, bool forceCheck)
    {
      DateTime utcNow = DateTime.UtcNow;
      MaintenanceRenewalsCheckStatusDAL checkStatus = MaintenanceRenewalsCheckStatusDAL.GetCheckStatus();
      if (checkStatus == null)
      {
        MaintenanceRenewalsCheckStatusDAL.Insert(new DateTime?(utcNow), new DateTime?(utcNow.AddMinutes(timeout)));
      }
      else
      {
        checkStatus.LastUpdateCheck = new DateTime?(utcNow);
        if (forceCheck)
          checkStatus.NextUpdateCheck = new DateTime?(utcNow.AddMinutes(timeout));
        checkStatus.Update();
      }
    }

    private void LoadFromReader(IDataReader rd)
    {
      if (rd == null)
        throw new ArgumentNullException(nameof (rd));
      this.LastUpdateCheck = new DateTime?(DatabaseFunctions.GetDateTime(rd, "LastUpdateCheck"));
      this.NextUpdateCheck = new DateTime?(DatabaseFunctions.GetDateTime(rd, "NextUpdateCheck"));
    }

    private void LoadFromDB()
    {
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM MaintenanceRenewalsCheckStatus"))
      {
        using (IDataReader rd = SqlHelper.ExecuteReader(textCommand))
        {
          if (!rd.Read())
            throw new ResultCountException(1, 0);
          this.LoadFromReader(rd);
        }
      }
    }
  }
}
