// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.DependencyDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Common;
using SolarWinds.Orion.Core.Common.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public class DependencyDAL
  {
    public static IList<Dependency> GetAllDependencies()
    {
      IList<Dependency> dependencyList = (IList<Dependency>) new List<Dependency>();
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM [dbo].[Dependencies]"))
      {
        using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
        {
          while (reader.Read())
            ((ICollection<Dependency>) dependencyList).Add(DependencyDAL.CreateDependency(reader));
        }
      }
      return dependencyList;
    }

    public static Dependency GetDependency(int id)
    {
      Dependency dependency = (Dependency) null;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("SELECT * FROM [dbo].[Dependencies] WHERE DependencyId = @id"))
      {
        textCommand.Parameters.AddWithValue("@id", (object) id);
        using (IDataReader reader = SqlHelper.ExecuteReader(textCommand))
        {
          if (reader.Read())
            dependency = DependencyDAL.CreateDependency(reader);
        }
      }
      return dependency;
    }

    public static void SaveDependency(Dependency dependency)
    {
      if (dependency == null)
        return;
      using (IDataReader dataReader = SqlHelper.ExecuteStoredProcReader("swsp_DependencyUpsert", new SqlParameter[7]
      {
        new SqlParameter("@DependencyId", (object) dependency.get_Id()),
        new SqlParameter("@Name", (object) dependency.get_Name()),
        new SqlParameter("@ParentUri", (object) dependency.get_ParentUri()),
        new SqlParameter("@ChildUri", (object) dependency.get_ChildUri()),
        new SqlParameter("@AutoManaged", (object) dependency.get_AutoManaged()),
        new SqlParameter("@EngineID", (object) dependency.get_EngineID()),
        new SqlParameter("@Category", (object) dependency.get_Category())
      }))
      {
        if (dataReader == null || dataReader.IsClosed || !dataReader.Read())
          return;
        dependency.set_Id(dataReader.GetInt32(0));
        dependency.set_LastUpdateUTC(dataReader.GetDateTime(1));
      }
    }

    public static void DeleteDependency(Dependency dependency)
    {
      if (dependency == null)
        return;
      using (SqlCommand textCommand = SqlHelper.GetTextCommand("\r\nSET NOCOUNT OFF;\r\nDelete FROM [dbo].[Dependencies]\r\n WHERE DependencyId = @id"))
      {
        textCommand.Parameters.AddWithValue("@id", (object) dependency.get_Id());
        SqlHelper.ExecuteNonQuery(textCommand);
      }
    }

    public static int DeleteDependencies(List<int> listIds)
    {
      if (listIds.Count == 0)
        return 0;
      string str1 = string.Empty;
      string str2 = string.Empty;
      foreach (int listId in listIds)
      {
        str1 = string.Format("{0}{1}'{2}'", (object) str1, (object) str2, (object) listId);
        str2 = ", ";
      }
      using (SqlCommand textCommand = SqlHelper.GetTextCommand(string.Format("\r\nSET NOCOUNT OFF;\r\nDelete FROM [dbo].[Dependencies]\r\n WHERE DependencyId in ({0})", (object) str1)))
        return SqlHelper.ExecuteNonQuery(textCommand);
    }

    private static Dependency CreateDependency(IDataReader reader)
    {
      Dependency dependency1 = (Dependency) null;
      if (reader != null)
      {
        Dependency dependency2 = new Dependency();
        dependency2.set_Id(reader.GetInt32(reader.GetOrdinal("DependencyId")));
        dependency2.set_Name(reader.GetString(reader.GetOrdinal("Name")));
        dependency2.set_ParentUri(reader.GetString(reader.GetOrdinal("ParentUri")));
        dependency2.set_ChildUri(reader.GetString(reader.GetOrdinal("ChildUri")));
        dependency2.set_LastUpdateUTC(reader.GetDateTime(reader.GetOrdinal("LastUpdateUTC")));
        dependency2.set_AutoManaged(reader.GetBoolean(reader.GetOrdinal("AutoManaged")));
        dependency2.set_EngineID(reader.GetInt32(reader.GetOrdinal("EngineID")));
        dependency2.set_Category(reader.GetInt32(reader.GetOrdinal("Category")));
        dependency1 = dependency2;
      }
      return dependency1;
    }
  }
}
