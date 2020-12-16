// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.PollerDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.DALs;
using SolarWinds.Orion.Core.Common.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  [Obsolete("Please start using SolarWinds.Orion.Core.Common.DALs.PollersDAL instead")]
  public class PollerDAL
  {
    private static PollersDAL pollersDAL = new PollersDAL();

    public static PollerAssignment GetPoller(int pollerID)
    {
      try
      {
        PollerAssignment assignment = PollerDAL.pollersDAL.GetAssignment(pollerID);
        if (assignment == null)
          throw new NullReferenceException();
        return assignment;
      }
      catch (Exception ex)
      {
        throw new ArgumentOutOfRangeException("PollerID", string.Format("Poller with Id {0} does not exist", (object) pollerID));
      }
    }

    public static int InsertPoller(PollerAssignment poller)
    {
      int num;
      PollerDAL.pollersDAL.Insert(poller, ref num);
      return num;
    }

    public static void DeletePoller(int pollerID)
    {
      PollerDAL.pollersDAL.DeletePollerByID(pollerID);
    }

    public static PollerAssignments GetPollersForNode(int nodeId)
    {
      PollerAssignments pollerAssignments = new PollerAssignments();
      pollerAssignments.Add(PollerDAL.pollersDAL.GetNetObjectPollers("N", nodeId, Array.Empty<string>()));
      return pollerAssignments;
    }

    public static PollerAssignments GetAllPollersForNode(
      int nodeId,
      bool includeInterfacePollers)
    {
      PollerAssignments pollerAssignments = new PollerAssignments();
      string str = "SELECT PollerID, PollerType, NetObjectType, NetObjectID, Enabled FROM Pollers WHERE NetObject = @NetObject ";
      if (includeInterfacePollers)
        str += "OR NetObject IN\r\n                        (\r\n                            SELECT 'I:' + RTRIM(LTRIM(STR(InterfaceID))) FROM Interfaces WHERE NodeID=@NodeID\r\n                        )";
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(str))
      {
        if (includeInterfacePollers)
          textCommand.Parameters.AddWithValue("@NodeID", (object) nodeId);
        textCommand.Parameters.Add("@NetObject", SqlDbType.VarChar, 50).Value = (object) string.Format("N:{0}", (object) nodeId);
        using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
        {
          while (reader.Read())
          {
            PollerAssignment poller = PollerDAL.CreatePoller(reader);
            ((Collection<int, PollerAssignment>) pollerAssignments).Add(poller.get_PollerID(), poller);
          }
        }
      }
      return pollerAssignments;
    }

    public static PollerAssignments GetPollersForVolume(int volumeId)
    {
      PollerAssignments pollerAssignments = new PollerAssignments();
      pollerAssignments.Add(PollerDAL.pollersDAL.GetNetObjectPollers("V", volumeId, Array.Empty<string>()));
      return pollerAssignments;
    }

    private static PollerAssignment CreatePoller(IDataReader reader)
    {
      PollerAssignment pollerAssignment = new PollerAssignment();
      for (int i = 0; i < reader.FieldCount; ++i)
      {
        string name = reader.GetName(i);
        if (!(name == "PollerType"))
        {
          if (!(name == "NetObjectType"))
          {
            if (!(name == "NetObjectID"))
            {
              if (!(name == "PollerID"))
              {
                if (!(name == "Enabled"))
                  throw new ApplicationException("Couldn't create poller - unknown field.");
                pollerAssignment.set_Enabled(DatabaseFunctions.GetBoolean(reader, i));
              }
              else
                pollerAssignment.set_PollerID(DatabaseFunctions.GetInt32(reader, i));
            }
            else
              pollerAssignment.set_NetObjectID(DatabaseFunctions.GetInt32(reader, name));
          }
          else
            pollerAssignment.set_NetObjectType(DatabaseFunctions.GetString(reader, i));
        }
        else
          pollerAssignment.set_PollerType(DatabaseFunctions.GetString(reader, i));
      }
      return pollerAssignment;
    }
  }
}
