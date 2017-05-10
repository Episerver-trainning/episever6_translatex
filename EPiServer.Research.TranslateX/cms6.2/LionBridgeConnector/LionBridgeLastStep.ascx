<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="LionBridgeLastStep.ascx.cs" Inherits="EPiServer.Research.Connector.Language.LionBridge.WizardLastStep" %>
<div class='divrow'>
    <div class='divcolumn1'><h3>Start date</h3></div>
    <div class='divcolumn '>
    <h3>Translate should start at:</h3><br />
    <asp:TextBox runat="server" ID='tbstartdate'></asp:TextBox><br />
    </div>    
</div>
<div class='divrow'>
    <div class='divcolumn1'><h3>Deliver date</h3></div>
    <div class='divcolumn '>
    <h3>Translate should deliver at:</h3><br />
    <asp:TextBox runat="server" ID='tbdeliverydate'></asp:TextBox><br />
    </div>    
</div>
<div class='divrow'>
    <div class='divcolumn1'><h3>Task ID</h3></div>
    <div class='divcolumn '>
    <h3>Task ID from Freeway service:</h3><br />    
    <asp:DropDownList runat="server" ID='ddltasks' AutoPostBack=true OnSelectedIndexChanged="taskchanged"></asp:DropDownList><br />
    </div>    
</div>
<div class='divrow'>
    <div class='divcolumn1'><h3>Subtask ID</h3></div>
    <div class='divcolumn '>
    <h3>Subtask ID from freeway service:</h3><br />    
    <asp:DropDownList runat="server" ID="ddlsubtasks"></asp:DropDownList>   <br />
    </div>    
</div>
<div class='divrow'>
    <div class='divcolumn1'><h3>UomID</h3></div>
    <div class='divcolumn '>
    <h3>UomID from freeway service:</h3><br />    
    <asp:DropDownList runat="server" ID="ddluoms"></asp:DropDownList><br />
    </div>    
</div>
<div class='divrow'>
    <div class='divcolumn1'><h3>Notification</h3></div>
    <div class='divcolumn '>
    <h3>Send notification to</h3><br />
    <asp:TextBox runat="server" ID='tbnotificationmail'></asp:TextBox><br />
    </div>    
</div>