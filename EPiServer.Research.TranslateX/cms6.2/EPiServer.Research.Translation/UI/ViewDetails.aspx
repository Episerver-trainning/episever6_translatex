<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="ViewDetails.aspx.cs" Inherits="EPiServer.Research.Translation.UI.ViewDetails" %>
<%@ Register Assembly="EPiServer" Namespace="EPiServer.Web.WebControls" TagPrefix="EPiServer" %>
<%@ Import Namespace="EPiServer.Research.Translation4.Common" %>
<%@ Register TagPrefix='translation' TagName='styleheader' Src='~/EPiServer.Research.Translation/UI/StyleHeader.ascx' %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Details</title>
    <translation:styleheader runat=server />
</head>
<body>
    <form id="form1" runat="server">
    <div class='modalWindow wide'>                   
       <div class='head'><br /><h1 class='title'>Project details</h1></div>   
       <div class='wizard'>
            <h2>Project information</h2>
            <div class='seperator'></div>
            
            <h3>Service :</h3> <%=CurrentProject.ConnectorName %><br />
            <h3>Local project id :</h3> <%=CurrentProject.LocalID %><br />
            <h3>Remote project id :</h3> <%=CurrentProject.RemoteID %><br />
            <h3>Local project status :</h3> <%=CurrentProject.Status %><br />
            <h3>Pages to translate :</h3><%=CurrentProject.Pages.Count %><br />
            <h3>Files to translate :</h3><%=CurrentProject.Files.Count %><br />
       </div> 
       <div class='head'><br /><h1 class='title'>Project details</h1></div>            
   </div>      
    <div style='display:none'>         
        Current local status:
        <asp:DropDownList runat="server" id='localstatus'></asp:DropDownList><asp:Button runat="server" Text='Set to new value' onclick='ok_click'/>
    </div>
    <div class='grid wide500' style='width:550px'>
        <div class='top'></div>
            <div  style="height:200px;overflow:auto;width:550px">
            <table>
            <tr>
            <th>Page name</th>
            <asp:repeater runat="server" DataSource='<%#CurrentProject.TargetLanguages %>' ID='targetLangs'>
                <itemtemplate>
                <th>local status for <%#(string)Container.DataItem %></th>
                <th>remote status for <%#(string)Container.DataItem %></th>            
                </itemtemplate>
            </asp:repeater>
            
            </tr>
            <asp:repeater runat="server" ID='dlPages'>
                <ItemTemplate>
                    <tr>
                    <td>
                    <img id="Img1" runat="server" src="~/EPiServer.Research.Translation/Images/blank.gif" width='<%#int.Parse(Eval("Indent")+"0")%>' height="1"/>                
                    <a href='<%#"GetReceivedData.aspx?pageid=" +((TranslationPage)(Container.DataItem)).ItemID%>' target="_blank"><%# ((TranslationPage)(Container.DataItem)).Properties["PageName"] %></a>
                    </td>
                    <asp:Repeater runat="server" DataSource='<%#GetPageLanguageStatus((TranslationPage)Container.DataItem )%>'>
                        <ItemTemplate>
                            <td><%#(string)(Container.DataItem) %></td>
                        </ItemTemplate>
                    </asp:Repeater>

                    </tr>               
                    <asp:Repeater runat="server" DataSource='<%# getFileList(((TranslationPage)(Container.DataItem))) %>'>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <img id="Img2" runat="server" src="~/EPiServer.Research.Translation/Images/blank.gif" width='<%#int.Parse(Eval("Indent")+"5")%>' height="1"/>
                                    <%# GetFilename(((TranslationFile)Container.DataItem).FilePath)%>
                                </td>
                                <asp:Repeater runat="server" DataSource='<%#GetFileLanguageStatus((TranslationFile)Container.DataItem )%>'>
                                    <ItemTemplate>
                                        <td><%#(string)(Container.DataItem) %></td>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </ItemTemplate>
            </asp:repeater>
            </table>
        </div>        
    </div>                            
    <div >
    </div>
    </form>
</body>
</html>
