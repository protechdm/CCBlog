<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="BlogEngine.Core.Web.Controls.PostViewBase" %>

<%@ Import Namespace="BlogEngine.Core" %>

<article class="post" id="post<%=Index %>">
    <header class="post-header">
    </header>
    <section class="post-body text">
        <div class="clearfix">
            <div class="post-thumbnail-container xclearfix">
                <a href="<%=Post.RelativeOrAbsoluteLink %>">
                   <img name="thumbnail" class="" src="<%=Utils.AbsoluteWebRoot %>Custom/Themes/Standard/images/<%=Post.CustomFields != null ? Post.CustomFields["Thumbnail"].Value : "nully"%>" />
                </a>
            </div>
            <div class="post-details-container clearfix">
                    <h2 class="post-title">
                        <a href="<%=Post.RelativeOrAbsoluteLink %>"><%=Server.HtmlEncode(Post.Title) %></a>
                    </h2>
                <div class="clearfix  font-normal-14px-purple" style="position:relative;width:100%;">
                    <span class="post-date">By&nbsp;<%=Post.Author%>&nbsp;</span>
                    <span class="post-date">&nbsp;|&nbsp;&nbsp;<%=Post.DateCreated.ToString("MMMM dd, yyyy")%></span>
                </div>
            <div class="post-details-body-content clearfix font-normal-14px-black" style="clear:both;">
                <asp:PlaceHolder ID="BodyContent" runat="server" />
            </div>
            </div>
            <div class="social-likes-container">
                <!-- Go to www.addthis.com/dashboard to customize your tools -->
                <div class="addthis_native_toolbox"></div>
            </div>



    <% if (Location != BlogEngine.Core.ServingLocation.SinglePost)
       {%>
            <div id="ReadMoreContainer" class="read-more-container">
                <a href="<%=Post.RelativeOrAbsoluteLink %>">
                   <img name="ReadMore" class="read-more" src="<%=Utils.AbsoluteWebRoot %>Custom/Themes/Standard/images/1x1-transparent.png" />
                </a>
            </div>

    <%} %>





        </div>
    </section>

    <% if (Location == BlogEngine.Core.ServingLocation.SinglePost)
       {%>
<%--    <footer class="post-footer">
        <div class="post-tags">
            <%=Resources.labels.tags %> : <%=TagLinks(", ") %>
        </div>
        <div class="post-rating">
            <%=Rating %>
        </div>
     
    </footer>--%>

    <%} %>

       <%=AdminLinks %>
</article>
