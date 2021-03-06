﻿// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.DAL.IDependencyDAL
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.Common.Models;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer.DAL
{
  public interface IDependencyDAL
  {
    IList<Dependency> GetAllDependencies();

    Dependency GetDependency(int id);

    void SaveDependency(Dependency depenedency);

    void DeleteDependency(Dependency dependency);
  }
}
