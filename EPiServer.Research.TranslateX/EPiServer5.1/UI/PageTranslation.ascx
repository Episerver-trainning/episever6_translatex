<%@control Language="C#" AutoEventWireup="true" CodeBehind="PageTranslation.ascx.cs" Inherits="EPiServer.Research.Translation4.UI.PagePicker" %>
<%@ Register TagPrefix="EPiServer" Namespace="EPiServer.WebControls" Assembly="EPiServer" %>
<link href='<%=EPiServer.Configuration.Settings.Instance.SiteUrl+"EPiServer.Research.Translation4/Styles/Grid.css" %>' type="text/css" rel='Stylesheet' />
<div class='pluginarea'>
    <h1>Translation of page : <%=CurrentPage.PageName %></h1>
    <div class='seperator'></div>
    <h2>New translation project:</h2>
    <a class='button' id='btnNewProject' name='btnNewProject' onclick='newProject(0)'><span>New project</span></a>
    <div class='seperator'></div>
    <h2>Existing translation projects:</h2>

    <div class='grid'>
    <asp:Repeater runat="server" ID='existingProjects'>
        <HeaderTemplate><table>   
        <tr>
            <th>Name</th>
            <th>Translate from</th>
            <th>Translate to</th>
            <th>Local status</th>
            <th></th>        
        </tr>
        </HeaderTemplate>
        <FooterTemplate></table></FooterTemplate>
        <ItemTemplate>
            <tr class='alternating'>
            <td><a href='#' onclick='viewDetails(<%#Eval("pkid") %>)' title='view details'><%#Eval("projectname") %></a></td>
            <td><%#Eval("sourcelanguage") %></td>
            <td><%#GetTargetLanguages( ((int) Eval("pkid"))) %></td>
            <td><%#GetLocalStatus((int)Eval("projectstatus"))%></td>
            <td><asp:PlaceHolder ID="PlaceHolder1" runat=server Visible='<%# GetLocalStatus((int)Eval("projectstatus"))=="ReadyForImport"%>'><a href='javascript:importPages(<%#(int)Eval("pkid") %>)' class='button'><span>Import</span></a></asp:PlaceHolder></td>
            </tr>        
        </ItemTemplate>
        <AlternatingItemTemplate>
            <tr>
            <td><a href='#' onclick='viewDetails(<%#Eval("pkid") %>)' title='view details'><%#Eval("projectname") %></a></td>
            <td><%#Eval("sourcelanguage") %></td>
            <td><%#GetTargetLanguages( ((int) Eval("pkid"))) %></td>
            <td><%#GetLocalStatus((int)Eval("projectstatus"))%></td>
            <td><asp:PlaceHolder runat="server" Visible='<%# GetLocalStatus((int)Eval("projectstatus"))=="ReadyForImport"%>'><a href='javascript:importPages(<%#(int)Eval("pkid") %>)' class='button'><span>Import</span></a></asp:PlaceHolder></td>
            </tr>            
        </AlternatingItemTemplate>
    </asp:Repeater>
    </div>


    <script type="text/javascript">
    <!--
        var newwinoptions = 'width=550px,height=500px,resizable=1,location=no,scrollbars=0'
        var newwinoptions2 = 'width=520px,height=250px,resizable=1,location=no,scrollbars=0'
        var baseurl = '<%=EPiServer.Configuration.Settings.Instance.SiteUrl+"EPiServer.Research.Translation4/UI/" %>';
        var currentpageid = '<%=CurrentPage.PageLink.ToString() %>';
        function importPages(projectid)
        {
            window.open(baseurl+'importpage.aspx?custompageid='+currentpageid+'&projectid=' + projectid, '_blank', newwinoptions2);
        }
        function viewDetails(projectid)
        {
            window.open(baseurl + 'viewDetails.aspx?custompageid=' + currentpageid + '&projectid=' + projectid, '_blank', newwinoptions);
        }
        function newProject(projectid)
        {
            window.open(baseurl + 'EditProject.aspx?custompageid=' + currentpageid + '&projectid=' + projectid, '_blank', newwinoptions);    
        }
    //-->
    </script>
</div>