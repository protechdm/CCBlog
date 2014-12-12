namespace BlogEngine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using BlogEngine.Core.Providers;

    /// <summary>
    /// Categories are a way to organize posts. 
    ///     A post can be in multiple categories.
    /// </summary>
    [Serializable]
    public class BusinessType : BusinessBase<BusinessType, Guid>, IComparable<BusinessType>
    {
        #region Constants and Fields

        /// <summary>
        ///     The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        ///     The categories.
        /// </summary>
        private static Dictionary<Guid, List<BusinessType>> businesstypes = new Dictionary<Guid, List<BusinessType>>();

        /// <summary>
        ///     The description.
        /// </summary>
        private string businessTypeName;

        /// <summary>
        ///     The parent.
        /// </summary>
        private Guid? id;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Category"/> class. 
        /// </summary>
        static BusinessType()
        {
            Blog.Saved += (s, e) =>
            {
                if (e.Action == SaveAction.Delete)
                {
                    Blog blog = s as Blog;
                    if (blog != null)
                    {
                        // remove deleted blog from static 'categories'

                        if (businesstypes != null && businesstypes.ContainsKey(blog.Id))
                            businesstypes.Remove(blog.Id);
                    }
                }
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Category" /> class.
        /// </summary>
        public BusinessType()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        ///     The category.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public BusinessType(string name)
        {
            this.Id = Guid.NewGuid();
            this.businessTypeName = name;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a sorted list of all Categories.
        /// </summary>
        /// <value>The categories.</value>
        public static List<BusinessType> BusinessTypes
        {
            get
            {
                Blog blog = Blog.CurrentInstance;
                List<BusinessType> blogBusinessTypes;

                if (!businesstypes.TryGetValue(blog.BlogId, out blogBusinessTypes))
                {
                    lock (SyncRoot)
                    {
                        if (!businesstypes.TryGetValue(blog.BlogId, out blogBusinessTypes))
                        {
                            businesstypes[blog.Id] = blogBusinessTypes = BlogService.FillBusinessTypes();

                            if(blogBusinessTypes != null)
                                blogBusinessTypes.Sort();
                        }
                    }
                }

                return blogBusinessTypes;
            }
        }

        /// <summary>
        ///     Gets a sorted list of all Categories across all blog instances.
        /// </summary>
        /// <value>The categories.</value>
        public static List<BusinessType> AllBlogBusinessTypes
        {
            get
            {
                List<Blog> blogs = Blog.Blogs.Where(b => b.IsActive).ToList();
                Guid originalBlogInstanceIdOverride = Blog.InstanceIdOverride;
                List<BusinessType> businessTypesAcrossAllBlogs = new List<BusinessType>();

                // Categories are not loaded for a blog instance until that blog
                // instance is first accessed.  For blog instances where the
                // categories have not yet been loaded, using InstanceIdOverride to
                // temporarily switch the blog CurrentInstance blog so the Categories
                // for that blog instance can be loaded.
                //
                for (int i = 0; i < blogs.Count; i++)
                {
                    List<BusinessType> blogBusinessTypes;
                    if (!businesstypes.TryGetValue(blogs[i].Id, out blogBusinessTypes))
                    {
                        // temporarily override the Current BlogId to the
                        // blog Id we need categories to be loaded for.
                        Blog.InstanceIdOverride = blogs[i].Id;
                        blogBusinessTypes = BusinessTypes;
                        Blog.InstanceIdOverride = originalBlogInstanceIdOverride;
                    }

                    businessTypesAcrossAllBlogs.AddRange(blogBusinessTypes);
                }

                return businessTypesAcrossAllBlogs;
            }
        }

        /// <summary>
        ///     Gets a sorted list of all Categories, taking into account the
        ///     current blog instance's Site Aggregation status in determining if
        ///     categories from just the current instance or all instances should
        ///     be returned.
        /// </summary>
        /// <remarks>
        ///     This logic could be put into the normal 'Categories' property, however
        ///     there are times when a Site Aggregation blog instance may just need
        ///     its own categories.  So ApplicableCategories can be called when data
        ///     across all blog instances may be needed, and Categories can be called
        ///     when data for just the current blog instance is needed.
        /// </remarks>
        public static List<BusinessType> ApplicableBusinessTypes
        {
            get
            {
                if (Blog.CurrentInstance.IsSiteAggregation)
                    return AllBlogBusinessTypes;
                else
                    return BusinessTypes;
            }
        }

        /// <summary>
        ///     Gets the absolute link to the page displaying all posts for this category.
        /// </summary>
        /// <value>The absolute link.</value>
        public Uri AbsoluteLink
        {
            get
            {
                return new Uri(this.Blog.AbsoluteWebRootAuthority + this.RelativeLink);
            }
        }

        /// <summary>
        ///     Returns a relative link if possible if the hostname of this blog instance matches the
        ///     hostname of the site aggregation blog.  If the hostname is different, then the
        ///     absolute link is returned.
        /// </summary>
        public string RelativeOrAbsoluteLink
        {
            get
            {
                if (this.Blog.DoesHostnameDifferFromSiteAggregationBlog)
                    return this.AbsoluteLink.ToString();
                else
                    return this.RelativeLink;
            }
        }

        /// <summary>
        ///     Gets the absolute link to the feed for this category's posts.
        /// </summary>
        /// <value>The feed absolute link.</value>
        public Uri FeedAbsoluteLink
        {
            get
            {
                return Utils.ConvertToAbsolute(this.FeedRelativeLink);
            }
        }

        /// <summary>
        ///     Gets the relative link to the feed for this category's posts.
        /// </summary>
        /// <value>The feed relative link.</value>
        public string FeedRelativeLink
        {
            get
            {
                var root = Blog.CurrentInstance.IsSiteAggregation ? Utils.ApplicationRelativeWebRoot : Blog.RelativeWebRoot;
                return string.Format("{0}businesstype/feed/{1}{2}", root,
                    Utils.RemoveIllegalCharacters(this.businessTypeName),
                    BlogConfig.FileExtension);
            }
        }

        /// <summary>
        ///     Gets all posts in this category.
        /// </summary>
        /// <value>The posts.</value>
        public List<Post> Posts
        {
            get
            {
                return Post.GetPostsByBusinessType(this.Id);
            }
        }

        /// <summary>
        ///     Gets the relative link to the page displaying all posts for this category.
        /// </summary>
        /// <value>The relative link.</value>
        public string RelativeLink
        {
            get
            {
                var root = Blog.CurrentInstance.IsSiteAggregation ? Utils.ApplicationRelativeWebRoot : Blog.RelativeWebRoot;
                return root + "businesstypes/" + Utils.RemoveIllegalCharacters(this.businessTypeName) +
                       BlogConfig.FileExtension;
            }
        }

        /// <summary>
        ///     Gets or sets the Title or the object.
        /// </summary>
        /// <value>The title.</value>
        public string BusinessTypeName
        {
            get
            {
                return this.businessTypeName;
            }

            set
            {
                base.SetValue("businessTypeName", value, ref this.businessTypeName);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a category based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The category id.
        /// </param>
        /// <returns>
        /// The category.
        /// </returns>
  

        /// <summary>
        /// Returns a category based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The category id.
        /// </param>
        /// <param name="acrossAllBlogInstances">
        /// Whether to search across the categories of all blog instances.
        /// </param>
        /// <returns>
        /// The category.
        /// </returns>
        public static BusinessType GetBusinessType(Guid id)
        {
            return (BusinessTypes).FirstOrDefault(b => b.Id == id);
        }




        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.businessTypeName;
        }

        #endregion

        #region Implemented Interfaces

        #region IComparable<BusinessType>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. 
        ///     The return value has the following meanings: Value Meaning Less than zero This object is 
        ///     less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(BusinessType other)
        {
            return this.businessTypeName.CompareTo(other.businessTypeName);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the object from the data store.
        /// </summary>
        protected override void DataDelete()
        {
            if (this.Deleted)
            {
                foreach (var c in
                    BusinessTypes.ToArray().Where(
                        c => !c.Id.Equals(this.Id)))
                {
                    c.Save();
                }

                BlogService.DeleteBusinessType(this);
            }

            if (BusinessTypes.Contains(this))
            {
                BusinessTypes.Remove(this);
            }

            this.Dispose();
        }

        /// <summary>
        /// Inserts a new object to the data store.
        /// </summary>
        protected override void DataInsert()
        {
            if (this.New)
            {
                BlogService.InsertBusinessType(this);
            }
        }

        /// <summary>
        /// Retrieves the object from the data store and populates it.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the object.
        /// </param>
        /// <returns>
        /// True if the object exists and is being populated successfully
        /// </returns>
        protected override BusinessType DataSelect(Guid id)
        {
            return BlogService.SelectBusinessType(id);
        }

        /// <summary>
        /// Updates the object in its data store.
        /// </summary>
        protected override void DataUpdate()
        {
            if (this.IsChanged)
            {
                BlogService.UpdateBusinessType(this);
            }
        }

        protected override void ValidationRules()
        {
            this.AddRule("Title", "Title must be set", string.IsNullOrEmpty(this.BusinessTypeName));
        }
        #endregion
    }
}