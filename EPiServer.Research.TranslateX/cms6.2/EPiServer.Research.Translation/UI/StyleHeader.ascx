<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="StyleHeader.ascx.cs" Inherits="EPiServer.Research.Translation.UI.StyleHeader" %>
<%@ Import Namespace="EPiServer.Shell" %>

<script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUtilBySettings("javascript/episerverscriptmanager.js") %>" type="text/javascript"></script>   
<script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.js") %>" type="text/javascript"></script>   
<script language="javascript" src="<%=EPiServer.UriSupport.ResolveUrlFromUIBySettings("javascript/system.aspx")%>" type="text/javascript"></script>   
<link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/EPiServer.Research.Translation/Styles/Grid.css") %>">

<link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/App_Themes/Default/styles/system.css") %>">
<link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/App_Themes/Default/styles/ToolButton.css") %>">

<link rel="stylesheet" type="text/css" href="<%= Paths.ToShellResource("ClientResources/Shell.css")  %>"/>
<link rel="stylesheet" type="text/css" href="<%= Paths.ToShellResource("ClientResources/ShellCoreLightTheme.css")  %>"/>
<link rel="stylesheet" type="text/css" href="<%= EPiServer.UriSupport.ResolveUrlFromUIBySettings("ClientResources/Epi/Base/CMS.css")  %>"/>