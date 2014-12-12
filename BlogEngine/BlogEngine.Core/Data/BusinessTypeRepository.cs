using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace BlogEngine.Core.Data
{
    /// <summary>
    /// Category repository
    /// </summary>
    public class BusinessTypeRepository : IBusinessTypeRepository
    {
        /// <summary>
        /// Find items in collection
        /// </summary>
        /// <param name="take">Items per page, default 10, 0 to return all</param>
        /// <param name="skip">Items to skip</param>
        /// <param name="filter">Filter, for example filter=IsPublished,true,Author,Admin</param>
        /// <param name="order">Sort order, for example order=DateCreated,desc</param>
        /// <returns>List of items</returns>
        public IEnumerable<BusinessTypeItem> Find(int take = 10, int skip = 0, string filter = "", string order = "")
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ViewPublicPosts))
                throw new System.UnauthorizedAccessException();

            // get post categories with counts
            var items = new List<BusinessTypeItem>();
            //foreach (var p in BlogEngine.Core.Post.ApplicablePosts)
            //{
                
            //        var tmp = items.FirstOrDefault(cat => cat.Id == p.BusinessType);
            //        if (tmp == null)
            //            items.Add(new BusinessTypeItem { Id = p.BusinessType, BusinessType = p.BusinessType.BusinessTypeName , Count = 1 });
            //        else
            //            tmp.Count++;
                
            //}

            // add categories without posts
            foreach (var c in BusinessType.BusinessTypes)
            {
                var count = BlogEngine.Core.Post.ApplicablePosts.Count(p => p.BusinessType == c.Id);
                items.Add(new Data.Models.BusinessTypeItem { Id = c.Id, BusinessType = c.BusinessTypeName, Count = count });
            }

            // return only what requested
            var query = items.AsQueryable();

            if(!string.IsNullOrEmpty(filter))
                query = items.AsQueryable().Where(filter);

            if (take == 0) 
                take = items.Count;

            if (string.IsNullOrEmpty(order))
                order = "BusinessType";

            return query.OrderBy(order).Skip(skip).Take(take);
        }
        /// <summary>
        /// Get single item
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>Object</returns>
        public Data.Models.BusinessTypeItem FindById(Guid id)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.ViewPublicPosts))
                throw new System.UnauthorizedAccessException();

            // get post categories
            //var items = new List<Data.Models.BusinessTypeItem>();

            //foreach (var p in BlogEngine.Core.Post.ApplicablePosts)
            //{
                
            //        var tmp = items.FirstOrDefault(cat => cat.Id == p.BusinessType);
            //        if (tmp == null)
            //            items.Add(new Data.Models.BusinessTypeItem { Id = p.BusinessType, BusinessType = p.BusinessType.BusinessTypeName, Count = 1 });
            //        else
            //            tmp.Count++;
                
            //}
            // add categories without posts
            //foreach (var c in BusinessType.BusinessTypes)
            //{
            //    var x = items.Where(i => i.Id == c.Id).FirstOrDefault();
            //    if (x == null)
            //        items.Add(new Data.Models.BusinessTypeItem { Id = c.Id, BusinessType = c.BusinessTypeName, Count = 0 });
            //}
            //return items.Where(c => c.Id == id).FirstOrDefault();
            var bt = BusinessType.AllBlogBusinessTypes.FirstOrDefault(b => b.Id == id);
            var count = BlogEngine.Core.Post.ApplicablePosts.Count(p => p.BusinessType == id);
            return new Data.Models.BusinessTypeItem { Id = bt.Id, BusinessType = bt.BusinessTypeName, Count = count };
        }
        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="item">Post</param>
        /// <returns>Saved item with new ID</returns>
        public Data.Models.BusinessTypeItem Add(BusinessTypeItem item)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.CreateNewPosts))
                throw new System.UnauthorizedAccessException();

            var cat = (from c in BusinessType.BusinessTypes.ToList() where c.BusinessTypeName == item.BusinessType select c).FirstOrDefault();
            if (cat != null)
                throw new ApplicationException("Title must be unique");

            try
            {
                var newItem = new BusinessType();
                UpdateCoreFromJson(newItem, item);
                return GetJsonFromCore(newItem);
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("BusinessTypeRepository.Add: {0}", ex.Message));
                return null;
            }
        }
        /// <summary>
        /// Update item
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <returns>True on success</returns>
        public bool Update(BusinessTypeItem item)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnPosts))
                throw new System.UnauthorizedAccessException();

            var cat = (from c in BusinessType.BusinessTypes.ToList() where c.BusinessTypeName == item.BusinessType && c.Id != item.Id select c).FirstOrDefault();
            if (cat != null)
                throw new ApplicationException("Title must be unique");

            var core = (from c in BusinessType.BusinessTypes.ToList() where c.Id == item.Id select c).FirstOrDefault();

            if (core != null)
            {
                UpdateCoreFromJson(core, item);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Delete item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True on success</returns>
        public bool Remove(Guid id)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.DeleteOwnPosts))
                throw new System.UnauthorizedAccessException();
            try
            {
                var core = (from c in BusinessType.BusinessTypes.ToList() where c.Id == id select c).FirstOrDefault();
                if(core != null)
                {
                    foreach (Post post in Post.Posts)
                    {
                        if (post.BusinessType == core.Id)
                        {
                            post.BusinessType = Guid.Empty;
                        }
                    }
                    core.Delete();
                    core.Save();  
                 
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("BusinessTypeRepository.Remove: {0}", ex.Message));
                return false;
            }
        }

        #region Private methods

        static Data.Models.BusinessTypeItem GetJsonFromCore(BusinessType core)
        {
            return new Data.Models.BusinessTypeItem()
            {
                Id = core.Id,
                BusinessType = core.BusinessTypeName,
                
            };
        }

        static void UpdateCoreFromJson(BusinessType core, Data.Models.BusinessTypeItem json)
        {
            try
            {
                core.BusinessTypeName = json.BusinessType;

                core.Save();
            }
            catch (Exception ex)
            {
                Utils.Log("Error updating Business Type: ", ex);
            }
        }

        static SelectOption OptionById(Guid? id)
        {
            if (id == null || id == Guid.Empty)
                return null;

            var cat = (from c in Category.Categories.ToList() where c.Id == id select c).FirstOrDefault();
            return new SelectOption { IsSelected = false, OptionName = cat.Title, OptionValue = cat.Id.ToString() };
        }

        static string ParentId(Guid? id)
        {
            if (id == null || id == Guid.Empty)
                return string.Empty;

            var cat = (from c in Category.Categories.ToList() where c.Id == id select c).FirstOrDefault();
            return cat == null || cat.Parent == null || cat.Parent == Guid.Empty ? "" : cat.Parent.ToString();
        }

        #endregion
    }
}
