<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditProject.aspx.cs" Inherits="EPiServer.Research.Translation4.UI.EditProject" %>
<%@ Register TagPrefix='translation' Namespace='EPiServer.Research.Translation4.UI' Assembly='EPiServer.Research.Translation5' %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Project editor</title>
    <script type="text/javascript" language="javascript"  >
        function selectbox(selected)
        {
            var selectbox = document.forms[0].elements['files'];
            
            for (var i=0;i<selectbox.length;i++)
                selectbox[i].checked = selected;
        }
        function checkTargetLanguage(source, args) {
            var obj = document.getElementById('<%=targetLanguages.ClientID %>');
            var checkboxes = obj.getElementsByTagName('input');
            var checked = false;
         
            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].checked) {
                    checked = true;
                    break;
                }
            }
            args.IsValid = checked;
        }
    </script>
    <link href="<%=EPiServer.Configuration.Settings.Instance.SiteUrl + "App_Themes/Default/Styles/system.css"%>" type="text/css" rel="stylesheet"/>
    <link href="<%=EPiServer.Configuration.Settings.Instance.SiteUrl + "EPiServer.Research.Translation4/Styles/Grid.css" %>" type="text/css" rel='Stylesheet' />
</head>
<body>    
    <form id="form1" runat="server">
    <div class='modalWindow wide'>          
        <div class='head'><br /><h1 class='title'>New translation for "<%=CurrentPage.PageName%>"</h1></div>
        <div class='wizard'>
            <h2 class='black'>New Translation Project</h2>        
            <div class='seperator widecenter'></div>        
            <asp:Wizard 
                ID="Wizard1" 
                runat="server" 
                ActiveStepIndex="0"            
                CellPadding="0" 
                Font-Names="Verdana"
                Font-Size="0.8em" 
                Width="530px"
                DisplaySideBar="False"            
                OnActiveStepChanged="StepChanged" 
                OnFinishButtonClick="WizardFinished" >        
                    <WizardSteps>                
                      <asp:WizardStep ID="ProjectSetting" runat="server" Title="Select source language">
                        <div class='divrow'>
                            <div class='divcolumn1'><h3>Project</h3></div>
                            <div class='divcolumn'>
                                <h3>Project name</h3>&nbsp;
                                <asp:TextBox runat="server" ID='projectName' Width="200px"></asp:TextBox>
                            </div>
                        </div>
                        <div class='seperator widecenter'></div>
                        <div class='divrow'>
                            <div class='divcolumn1'><h3>Translation</h3></div>
                            <div class='divcolumn'>
                                <h3>Translate from</h3>&nbsp;
                                <asp:dropdownlist runat="server" ID='sourceLanguage' style="border: none 0px;display:none">
                                </asp:dropdownlist><%=EPiServer.DataAbstraction.LanguageBranch.Load(CurrentPage.LanguageBranch).Name%><br /><br />
                                <h3>Translate to</h3><br />
                                <p>Select the languages thant you want to translate the original page to</p>
                                <asp:CheckBoxList runat="server" ID='targetLanguages' RepeatDirection="Horizontal">
                                </asp:CheckBoxList>
                                <asp:CustomValidator runat="server"  EnableClientScript="true" ClientValidationFunction='checkTargetLanguage' ErrorMessage='* you must specify one language translate to <br/>'></asp:CustomValidator>
                                <h3>Translate subpages</h3><br />
                                <p>Select this option if you want to include translation of subpages to "<%=CurrentPage.PageName%>"</p>
                                <asp:CheckBox runat="server" ID='includeChildren' Text="Includes subpages" />
                            </div>
                        </div>
                        <div class='seperator widecenter'></div>
                       <%-- <div class='divrow'>
                            <div class='divcolumn1'><h3>Notification</h3></div>
                            <div class='divcolumn '>
                            <h3>Send notification to</h3>&nbsp;
                            <asp:TextBox runat="server" ID='notificationMail'></asp:TextBox><br />
                            </div>    
                        </div>--%>
                        <div class='divrow'>
                            <div class='divcolumn1'><h3>Service</h3></div>
                            <div class='divcolumn'>
                            <h3>Choose the service to use:</h3>&nbsp;
                            <asp:DropDownList runat='server' ID='ddlConnector'>                                
                            </asp:DropDownList>
                            </div>                        
                        </div>
                        <div class='seperator widecenter'></div>
                      </asp:WizardStep>
                      <asp:WizardStep ID="ProjectPages" runat="server" Title="Modify pages" >                    
                        <h3>Select subpages</h3>
                        <div class='grid wide500'>
                            <div class='top'></div>
                            <asp:repeater runat="server" id="pagesToTranslate">
                                <headerTemplate><div class='pagegrid'></headerTemplate>
                                <itemtemplate>
                                    <img runat="server" src="~/EPiServer.Research.Translation4/Images/blank.gif" width='<%#int.Parse(Eval("Indent")+"0")%>' height="1"/>
                                    <asp:linkbutton ID="Linkbutton1" runat="server" OnCommand='changePageStatus' CommandArgument='<%#Eval("Page_Link") %>'><img src='<%# EPiServer.Configuration.Settings.Instance.SiteUrl + "EPiServer.Research.Translation4/images/" + Eval("status") + ".gif"%>' border='0'/></asp:linkbutton>
                                    <%#Eval("Page_Name") %>
                                     <br />
                                </itemtemplate>
                                <footertemplate></div></footertemplate>
                            </asp:repeater>
                        </div>
                      </asp:WizardStep>
                      <asp:WizardStep ID='ProjectFiles' runat="server" Title='Choose files'>                    
                            <h3>Select files</h3>
                            <div class='grid wide500'>
                                <div class='top'></div>
                                <asp:repeater runat="server" id="filesToTranslate">
                                    <headerTemplate><div class='pagegrid'></headerTemplate>
                                    <itemtemplate>
                                        <img runat="server" src="~/EPiServer.Research.Translation4/Images/blank.gif" width='<%#int.Parse(Eval("Indent")+"0")%>' height="1"/>
                                        <%#Eval("Page_Name") %>
                                         <br />
                                         <asp:Repeater runat=server DataSource='<%#GetPageFiles(Eval("Page_Link") as string,(int)Eval("Indent")) %>'>
                                         <ItemTemplate>
                                            <img runat="server" src="~/EPiServer.Research.Translation4/Images/blank.gif" width='<%#int.Parse(Eval("Indent")+"0")+15%>' height="1"/>
                                            <asp:linkbutton runat="server" OnCommand='changeFileStatus' CommandArgument='<%#Eval("Filepath") %>'><img src='<%# EPiServer.Configuration.Settings.Instance.SiteUrl + "EPiServer.Research.Translation4/images/" + Eval("status") + ".gif"%>' border='0'/></asp:linkbutton>
                                            <%#Eval("Filename") %>
                                            <br />
                                        </ItemTemplate>
                                        </asp:Repeater>
                                    </itemtemplate>
                                    <footertemplate>
                                    </div>
                                    <div>
                                    <!--a href='#' onclick='selectbox(true)'>Select all</a>&nbsp;<a href='#' onclick='selectbox(false)'>select none</a-->
                                    </div>
                                    </footertemplate>
                                </asp:repeater> 
                        </div>                   
                      </asp:WizardStep>
                      <translation:CustomerWizardStep ID='customerStep' runat="server">
                            
                      </translation:CustomerWizardStep>
                      <asp:WizardStep ID='summarystep' runat='server'>
                        Pages to send: <asp:Label runat="server" ID='numberofpages'></asp:Label><br />
                        Files to send: <asp:Label runat="server" ID='numberoffiles'></asp:Label><br />
                      </asp:WizardStep>
                      <asp:WizardStep ID="WizardFinal" runat="server" Title="Finished" StepType="Complete">
                        
                        <asp:Label ID="Label1" runat="server"/>
                        Project is created.<br />
                        <a class="button" onclick='window.close()'><span>Close window</span></a> 
                      </asp:WizardStep>
                    </WizardSteps>
            <HeaderStyle VerticalAlign="Top" HorizontalAlign="Left"></HeaderStyle>
            <StartNavigationTemplate>
                <br />
                <asp:LinkButton class='button' ID="StartNextButton" runat="server" CommandName="MoveNext" ><span>Next</span></asp:LinkButton>
            </StartNavigationTemplate>
            <NavigationStyle VerticalAlign="Top" HorizontalAlign="Right"/>
            <SideBarStyle VerticalAlign="Top" HorizontalAlign="Left" />
            <StepNavigationTemplate>
                <br />
                <%--<asp:LinkButton class='button' runat="server" CommandName="MovePrevious" ><span>Previous</span></asp:LinkButton>--%>                     
                <asp:LinkButton class='button' runat="server" CommandName="MoveNext" ><span>Next</span></asp:LinkButton>                     
            </StepNavigationTemplate>
            <FinishNavigationTemplate>
                <br />
                <%--<asp:LinkButton class='button' runat="server" CommandName="MovePrevious" ><span>Previous</span></asp:LinkButton>--%>                     
                <asp:LinkButton class='button' runat="server" CommandName="MoveComplete" ><span>Finish</span></asp:LinkButton>
            </FinishNavigationTemplate>    
            
            </asp:Wizard>
            </div>
        </div>               
    </form>
</body>
</html>

