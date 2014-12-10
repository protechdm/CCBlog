<%@ Page Language="C#" MasterPageFile="~/admin/admin.master" AutoEventWireup="true" EnableEventValidation="false" ValidateRequest="false" CodeFile="Admin.aspx.cs" Inherits="UserControlsFeaturedPostsAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphAdmin" Runat="Server">
  <div class="alert alert-info">
	<p>1. Register control in web.config</p>
	<pre>&lt;add assembly="FeaturedPostsRotatorRevisited" namespace="App_Code.Controls" tagPrefix="rotator" /&gt;</pre>
	<p>2. Add control to the theme master (site.master)</p>
	<pre>&lt;rotator:FeaturedPostsRotatorControl ID="Featured" runat="server" /&gt;</pre>
  </div>
  <asp:Label ID="lblUseJquery" runat="server" AssociatedControlID="chkUseJquery" Text="Add JQuery Framework to Header?" />
  <asp:CheckBox ID="chkUseJquery" runat="server" /><asp:Button ID="btnSaveSettings" runat="server" Text="Save Settings" />  
  <br />
  <asp:DropDownList ID="ddPosts" runat="server" AutoPostBack="false"></asp:DropDownList>
  <asp:FileUpload runat="server" ID="txtUploadImage" Width="310" size="50" /><br />
 <!-- <asp:RegularExpressionValidator runat="server" ErrorMessage="Upload Jpegs and Gifs only." ControlToValidate="txtUploadImage"></asp:RegularExpressionValidator>-->
  <!--id="FileUpLoadValidator" ValidationExpression="^(([a-zA-Z]:)|(\\{2}\w+)\$?)(\\(\w[\w].*))(.jpg|.JPG|.gif|.GIF)$"-->
  <asp:Button runat="server" ID="btnUploadImage" Text="Upload" />

  <asp:GridView ID="gridFeaturedPostImages"
    PageSize="20"
    BorderColor="Silver"
    BorderStyle="solid"
    BorderWidth="1px"
    CellPadding="2"
    runat="server"
    Width="100%"
    AutoGenerateColumns="False"
    AllowPaging="True"
    OnPageIndexChanging="gridView_PageIndexChanging"
    AllowSorting="True">
    <Columns>
      <asp:BoundField DataField="id" Visible="false" />
      <asp:BoundField DataField="PostTitle" HeaderText="Post Title" Visible="true" />
      <asp:BoundField DataField="ImageSize" HeaderText="Size" Visible="true" />
      <asp:TemplateField HeaderText="Images">
        <ItemTemplate>
          <%# Eval("Image") %>
        </ItemTemplate>
      </asp:TemplateField>

      <asp:TemplateField ShowHeader="False" ItemStyle-VerticalAlign="middle" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="25">
        <ItemTemplate>
          <asp:ImageButton ID="btnDelete" runat="server" ImageAlign="middle" CausesValidation="false" ImageUrl="~/Custom/Controls/FeaturedPostsRotator/images/del.png" OnClick="btnDelete_Click" CommandName="btnDelete" AlternateText="Delete" />
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
    <PagerSettings Mode="Numeric" Position="Bottom" PageButtonCount="10" />
  </asp:GridView>

</asp:Content>
