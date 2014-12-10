using BlogEngine.Core.Web.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BlogEngine.Core;
using System.Data;

public partial class UserControlsFeaturedPostsAdmin : System.Web.UI.Page
{
  static protected ExtensionSettings _settings = null;
  static protected ExtensionSettings _featuredPostsImages = null;
 // private static List<Post> _posts = new List<Post>();  
  
    private static List<Post> posts = new List<Post>(Post.Posts);

  #region Page_Load
  protected void Page_Load(object sender, EventArgs e)
  {
    _settings = ExtensionManager.GetSettings("FeaturedPostsRotator");
    _featuredPostsImages = ExtensionManager.GetSettings("FeaturedPostsRotator", "FeaturedPostsImages");

    if (!Page.IsPostBack && !Page.IsCallback)
    {
      BindPosts();
	
      BindSettingsForm();
    }

    BindGrid();

    btnSaveSettings.Click += new EventHandler(btnSaveSettings_Click);
    btnUploadImage.Click += new EventHandler(btnUploadImage_Click);
  }
  #endregion

  #region btnSaveSettings_Click
  void btnSaveSettings_Click(object sender, EventArgs e)
  {
    _settings.UpdateScalarValue("UseExtensionJQuery", chkUseJquery.Checked.ToString());
    ExtensionManager.SaveSettings("FeaturedPostsRotator", _settings);
    Response.Redirect(Request.RawUrl);
  }
  #endregion

  #region btnUploadImage_Click
  private void btnUploadImage_Click(object sender, EventArgs e)
  {
    string relativeFolder = DateTime.Now.Year.ToString() + Path.DirectorySeparatorChar + DateTime.Now.Month.ToString() + Path.DirectorySeparatorChar;
    string folder = BlogConfig.StorageLocation + "files" + Path.DirectorySeparatorChar;
    string fileName = txtUploadImage.FileName;
    Upload(folder + relativeFolder, txtUploadImage, fileName);

    string path = Utils.RelativeWebRoot.ToString();
    string img = string.Format("{0}image.axd?picture={1}", path, relativeFolder.Replace("\\", "/") + fileName);
    string size = SizeFormat(txtUploadImage.FileBytes.Length, "N");

    string id = Guid.NewGuid().ToString();
    //string postTitle = Post.GetPost(new Guid(ddPosts.SelectedValue)).Title;
    _featuredPostsImages.AddValue("id", id);
    _featuredPostsImages.AddValue("PostID", ddPosts.SelectedValue);
    _featuredPostsImages.AddValue("PostTitle", ddPosts.SelectedItem.Text);
    _featuredPostsImages.AddValue("Image", img);
    _featuredPostsImages.AddValue("ImageSize", size);

    ExtensionManager.SaveSettings("FeaturedPosts", _featuredPostsImages);
    Response.Redirect(Request.RawUrl);
  }
  #endregion

  #region btnDelete_Click
  protected void btnDelete_Click(object sender, EventArgs e)
  {
    ImageButton btn = (ImageButton)sender;
    GridViewRow grdRow = (GridViewRow)btn.Parent.Parent;

    foreach (ExtensionParameter par in _featuredPostsImages.Parameters)
    {
      par.DeleteValue(grdRow.RowIndex);
    }
    ExtensionManager.SaveSettings("FeaturedPosts", _featuredPostsImages);
    Response.Redirect(Request.RawUrl);
  }
  #endregion

  #region gridView_PageIndexChanging
  protected void gridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
  {
    gridFeaturedPostImages.PageIndex = e.NewPageIndex;
    gridFeaturedPostImages.DataBind();
  }
  #endregion
  
  #region BindGrid
  protected void BindGrid()
  {
    if(_featuredPostsImages != null)
	{
      gridFeaturedPostImages.DataKeyNames = new string[] { _featuredPostsImages.KeyField };
      gridFeaturedPostImages.DataSource = _featuredPostsImages.GetDataTable();
      gridFeaturedPostImages.DataBind();
	}
  }
  #endregion

  #region BindSettingsForm()
  protected void BindSettingsForm()
  {
    chkUseJquery.Checked = false;
    if (bool.Parse(_settings.GetSingleValue("UseExtensionJQuery")))
    {
      chkUseJquery.Checked = true;
    }
  }
  #endregion

  #region Upload
  private void Upload(string virtualFolder, FileUpload control, string fileName)  
   //Upload(folder + relativeFolder, txtUploadImage, fileName);  
  {
    string folder = Server.MapPath(virtualFolder);
    if (!Directory.Exists(folder))
      Directory.CreateDirectory(folder);

    control.PostedFile.SaveAs(folder + fileName);
  }
  #endregion

  #region SizeFormat
  private string SizeFormat(float size, string formatString)
  {
    if (size < 1024)
      return size.ToString(formatString) + " bytes";

    if (size < Math.Pow(1024, 2))
      return (size / 1024).ToString(formatString) + " kb";

    if (size < Math.Pow(1024, 3))
      return (size / Math.Pow(1024, 2)).ToString(formatString) + " mb";

    if (size < Math.Pow(1024, 4))
      return (size / Math.Pow(1024, 3)).ToString(formatString) + " gb";

    return size.ToString(formatString);
  }
  #endregion

  #region BindPosts
  private void BindPosts()
  {   
    posts.Sort(delegate(Post p1, Post p2) { return String.Compare(p1.Title, p2.Title); });	
	ddPosts.DataSource = posts;
    ddPosts.DataTextField = "Title";
    ddPosts.DataValueField = "Id";
    ddPosts.DataBind();		
  }
  #endregion 

}
