using System.Data;
using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Configuration;

namespace BlogEngine.Core.Data
{
    /// <summary>
    /// Lookups repository
    /// </summary>
    public class LookupsRepository : ILookupsRepository
    {
        private Lookups lookups = new Lookups();

        /// <summary>
        /// List of available cultures
        /// </summary>
        /// <returns>List of cultures</returns>
        public Lookups GetLookups()
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.AccessAdminPages))
                throw new System.UnauthorizedAccessException();

            LoadCultures();

            LoadSelfRegisterRoles();

            LoadPages();

            LoadAuthors();

            LoadCategories();

            LoadBusinessType();

            LoadThemes();

            return lookups;
        }

        void LoadCultures()
        {
            var items = new List<SelectOption>();

            items.Add(new SelectOption { OptionName = "Auto", OptionValue = "Auto" });
            items.Add(new SelectOption { OptionName = "English", OptionValue = "en" });

            if (File.Exists(Path.Combine(HttpRuntime.AppDomainAppPath, "PrecompiledApp.config")))
            {
                var precompiledDir = HttpRuntime.BinDirectory;
                var translations = Directory.GetFiles(
                    precompiledDir, "App_GlobalResources.resources.dll", SearchOption.AllDirectories);
                foreach (var translation in translations)
                {
                    var path = Path.GetDirectoryName(translation);
                    if (path == null)
                        continue;

                    var resourceDir = path.Remove(0, precompiledDir.Length);
                    if (String.IsNullOrEmpty(resourceDir))
                        continue;

                    var info = CultureInfo.GetCultureInfoByIetfLanguageTag(resourceDir);
                    items.Add(new SelectOption { OptionName = info.NativeName, OptionValue = resourceDir, IsSelected = false });
                }
            }
            else
            {
                var path = HostingEnvironment.MapPath(string.Format("{0}App_GlobalResources/", Utils.ApplicationRelativeWebRoot));
                foreach (var file in Directory.GetFiles(path, "labels.*.resx"))
                {
                    var index = file.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    var filename = file.Substring(index);
                    filename = filename.Replace("labels.", string.Empty).Replace(".resx", string.Empty);
                    var info = CultureInfo.GetCultureInfoByIetfLanguageTag(filename);
                    items.Add(new SelectOption { OptionName = info.NativeName, OptionValue = filename, IsSelected = false });
                }
            }
            lookups.Cultures = items;
        }

        void LoadSelfRegisterRoles()
        {
            var roles = new List<SelectOption>();
            foreach (var role in Roles.GetAllRoles().Where(r => !r.Equals(BlogConfig.AnonymousRole, StringComparison.OrdinalIgnoreCase)))
            {
                roles.Add(new SelectOption { OptionName = role, OptionValue = role });
            }
            lookups.SelfRegisterRoles = roles;
        }

        void LoadAuthors()
        {
            var items = new List<SelectOption>();

            if (!Security.IsAuthorizedTo(Rights.EditOtherUsersPosts))
            {
                items.Add(new SelectOption { OptionName = Security.CurrentUser.Identity.Name, OptionValue = Security.CurrentUser.Identity.Name });
            }
            else
            {
                IEnumerable<MembershipUser> users = Membership.GetAllUsers()
                .Cast<MembershipUser>()
                .ToList()
                .OrderBy(a => a.UserName);

                foreach (MembershipUser user in users)
                {
                    items.Add(new SelectOption { OptionName = user.UserName, OptionValue = user.UserName });
                }
            }
            lookups.AuthorList = items;
        }

        void LoadPages()
        {
            var pages = new List<SelectOption>();
            foreach (var page in Page.Pages)
            {
                if (!page.IsDeleted)
                {
                    pages.Add(new SelectOption { OptionName = page.Title, OptionValue = page.Id.ToString() });
                }
            }
            lookups.PageList = pages;
        }

        void LoadCategories()
        {
            var cats = new List<SelectOption>();
            foreach (var cat in Category.Categories)
            {
                cats.Add(new SelectOption { OptionName = cat.Title, OptionValue = cat.Id.ToString() });
            }
            lookups.CategoryList = cats;
        }

        void LoadBusinessType()
        {
            var conn = new System.Data.SqlClient.SqlConnection();
            var command = new System.Data.SqlClient.SqlCommand();
            string connectionString = ConfigurationManager.ConnectionStrings["BlogEngine"].ConnectionString;

            conn.ConnectionString =
                "Data Source=94.126.43.196;Initial Catalog=BlogEngine;Integrated Security=False;Trusted_Connection=False;User Id=sa;Password=Her0sandwich=sql";
            conn.ConnectionString = connectionString;
            command.Connection = conn;
            command.CommandType = CommandType.Text;
            command.CommandText = "select * from be_BusinessTypes";
            conn.Open();
            var data = new System.Data.DataSet();
            var adapter = new System.Data.SqlClient.SqlDataAdapter();
            adapter.SelectCommand = command;
            adapter.Fill(data);
            conn.Close();

            var bt = (from DataRow row in data.Tables[0].Rows select new SelectOption {OptionName = row[1].ToString(), OptionValue = row[0].ToString()}).ToList();

            lookups.BusinessTypeList = bt;
        }

        void LoadThemes()
        {
            var items = new List<SelectOption>();
            var packages = Packaging.FileSystem.LoadThemes();

            foreach (var pkg in packages)
            {
                items.Add(new SelectOption { OptionName = pkg.Title, OptionValue = pkg.Id.ToString() });
            }
            lookups.InstalledThemes = items;
        }

    }
}
