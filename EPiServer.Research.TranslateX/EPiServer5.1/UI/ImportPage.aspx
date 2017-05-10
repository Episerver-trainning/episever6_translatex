<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportPage.aspx.cs" Inherits="EPiServer.Research.Translation4.UI.ImportPage" %>
<%@ Register Assembly="EPiServer" Namespace="EPiServer.WebControls" TagPrefix="EPiServer" %>
<%@ Register TagPrefix='translation' Namespace='EPiServer.Research.Translation4.UI' Assembly='EPiServer.Research.Translation5' %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Import</title>
    <script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUtilBySettings("javascript/episerverscriptmanager.js") %>" type="text/javascript"></script>   
    <script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js") %>" type="text/javascript"></script>   
    
    <script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.aspx")%>" type="text/javascript"></script>   
    <link href="<%=EPiServer.Configuration.Settings.Instance.SiteUrl + "App_Themes/Default/Styles/system.css"%>" type="text/css" rel="stylesheet"/>
    <link href="<%=EPiServer.Configuration.Settings.Instance.SiteUrl + "EPiServer.Research.Translation4/Styles/Grid.css" %>" type="text/css" rel='Stylesheet' />
    <script language="javascript" type="text/javascript">
        function importaschanged(obj) {
            var divdestnode = document.getElementById('destnode');
            var divfilesuffix = document.getElementById('filesuffix');
            if (obj.selectedIndex == 1) {
                divdestnode.style.display = 'none';
                divfilesuffix.style.display = 'block';
            } else {
                divdestnode.style.display = 'block';
                divfilesuffix.style.display = 'none';            
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
         <div class='modalWindow wide'>                   
            <div class='head'><br /><h1 class='title'>Import Translated Pages</h1></div>            
            <asp:Wizard 
                ID="Wizard1" 
                runat="server" 
                ActiveStepIndex="0"             
                CellPadding="5" 
                Font-Names="Verdana"
                Font-Size="0.8em" 
                Width="480px"
                DisplaySideBar="false"            
                OnActiveStepChanged="StepChanged" 
                OnFinishButtonClick="WizardFinished" 
                OnNextButtonClick="NextStep"
                OnPreviousButtonClick="PrevStep" 
                >        
                    <WizardSteps> 
                      <translation:CustomerWizardStep ID='customerStep' runat="server">                            
                      </translation:CustomerWizardStep>                                     
                      <asp:WizardStep ID="WizardStep1" runat="server" Title="Import page as">
                       <h3>Import pages as</h3>
                       <select id='importas' name='importas' runat="server" onchange='importaschanged(this)'>                       
                            <option value='1'>New page</option>
                            <option value='0'>New version</option>
                        </select>
                            <div id='destnode'>
                           <h3>Create nodes at</h3> <episerver:inputpagereference runat=server ID='createAt'></episerver:inputpagereference> 
                           </div>
                           <div id='filesuffix' style='display:none'>
                           <h3>Append suffix to file</h3> <asp:TextBox runat=server ID='suffixtext'></asp:TextBox>
                           </div>
                      </asp:WizardStep>                  
                      <asp:WizardStep ID="WizardStep3" runat="server" Title="Select publish type">
                           Publish type:
                           <select id='publishtype' name='publishtype' runat="server">
                                <option value='1'>Check in</option>
                                <option value='0'>Publish</option>
                            </select>                       
                      </asp:WizardStep>                  
                   </WizardSteps>
            <HeaderStyle VerticalAlign="Top" HorizontalAlign="Left"></HeaderStyle>
            <StartNavigationTemplate>
                <asp:LinkButton class='button' ID="StartNextButton" runat="server" CommandName="MoveNext" ><span>Next</span></asp:LinkButton>                     
            </StartNavigationTemplate>
            <NavigationStyle VerticalAlign="Top" HorizontalAlign="Right"/>
            <SideBarStyle VerticalAlign="Top" HorizontalAlign="Left" />
            <StepNavigationTemplate>
                <asp:LinkButton ID="LinkButton3" class='button' runat="server" CommandName="MovePrevious" ><span>Previous</span></asp:LinkButton>                     
                <asp:LinkButton ID="LinkButton1" class='button' runat="server" CommandName="MoveNext" ><span>Next</span></asp:LinkButton>                     
            </StepNavigationTemplate>
            <FinishNavigationTemplate>
                <asp:LinkButton ID="LinkButton2" class='button' runat="server" CommandName="MoveComplete" ><span>Finish</span></asp:LinkButton>                     
            </FinishNavigationTemplate>                       
          </asp:Wizard>
          <asp:PlaceHolder runat='server' ID='closeWindowPanel' Visible='false'>
            <script>window.close();</script>
          </asp:PlaceHolder>

        </div>      
    </form>    
</body>
</html>
