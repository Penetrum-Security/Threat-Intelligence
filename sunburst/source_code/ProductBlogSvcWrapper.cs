// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.ProductBlogSvcWrapper
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Orion.Core.BusinessLayer.DAL;
using SolarWinds.Orion.Core.Common.Models;
using System.Collections.Generic;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  public class ProductBlogSvcWrapper
  {
    public static BlogItemDAL GetBlogItem(RssBlogItem rssBlog)
    {
      BlogItemDAL blogItemDal = new BlogItemDAL();
      blogItemDal.Title = rssBlog.get_Title();
      blogItemDal.Description = rssBlog.get_Description();
      blogItemDal.Ignored = false;
      blogItemDal.Url = rssBlog.get_Link();
      blogItemDal.SetNotAcknowledged();
      blogItemDal.PostGuid = rssBlog.get_PostGuid();
      blogItemDal.PostId = rssBlog.get_PostId();
      blogItemDal.Owner = rssBlog.get_Creator();
      blogItemDal.PublicationDate = rssBlog.get_PubDate();
      blogItemDal.CommentsUrl = rssBlog.get_CommentsURL();
      blogItemDal.CommentsCount = rssBlog.get_CommentsNumber();
      return blogItemDal;
    }

    public static List<BlogItemDAL> GetBlogItems(RssBlogItems rssBlogs)
    {
      List<BlogItemDAL> blogItemDalList = new List<BlogItemDAL>();
      using (List<RssBlogItem>.Enumerator enumerator = ((List<RssBlogItem>) rssBlogs.ItemList).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          RssBlogItem current = enumerator.Current;
          blogItemDalList.Add(ProductBlogSvcWrapper.GetBlogItem(current));
        }
      }
      return blogItemDalList;
    }
  }
}
