﻿<%--  
    
    Carefull
    
    This is a 1:1 copy of View.ascx. It's still in the distribution because we will first have to ensure that 
    DNN in new-install and in update mode will use this to show an app
    
    See also https://github.com/2sic/2sxc/issues/577
    
    We'll delete this file as soon as we updated the manifest with either xml or sql to ensure it works
    
--%>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="ToSic.SexyContent.View" Codebehind="View.ascx.cs" %>
<%@ Import Namespace="ToSic.SexyContent" %>
<asp:Placeholder runat="server" ID="pnlTemplateChooser" Visible="false" EnableViewState="False">
    
    <div sxc-app="2sxc.view" id="tselector<%= ModuleId %>" ng-controller="TemplateSelectorCtrl as vm" 
        data-moduleid="<%= ModuleId %>" class="sc-selector-wrapper" 
        ng-include="'template-selector/template-selector.html'"
        <%-- note that the importappdialog is only needed, till import-app works in angular-only --%>
        data-importAppDialog="<%= EditUrl("", "", SexyContent.ControlKeys.AppImport) %>"
        > 
    </div>
</asp:Placeholder>

<asp:Panel runat="server" Visible="False" class="dnnFormMessage dnnFormInfo" ID="pnlGetStarted"></asp:Panel>

<asp:Panel runat="server" ID="pnlZoneConfigurationMissing" Visible="false" CssClass="dnnFormMessage dnnFormInfo">
    <asp:Label runat="server" ID="lblMissingZoneConfiguration" ResourceKey="ZoneConfigurationMissing"></asp:Label>
    <asp:HyperLink runat="server" ID="hlkConfigureZone" 
        CssClass="dnnSecondaryAction" ResourceKey="hlkConfigureZone"></asp:HyperLink>
</asp:Panel>

<asp:Panel runat="server" ID="pnlError" CssClass="dnnFormMessage dnnFormWarning" Visible="false"></asp:Panel>
<asp:Panel runat="server" ID="pnlMessage" CssClass="dnnFormMessage dnnFormInfo" Visible="false"></asp:Panel>

<div class="sc-viewport">
    <asp:PlaceHolder runat="server" ID="phOutput"></asp:PlaceHolder>
</div>