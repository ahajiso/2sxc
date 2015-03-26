﻿using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using ToSic.SexyContent.DataSources;
using ToSic.SexyContent.Engines;
using ToSic.SexyContent.WebApi;

namespace ToSic.SexyContent.ViewAPI
{
    [SupportedModules("2sxc,2sxc-app")]
    public class ModuleController : SxcApiController
    {

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public void AddItem([FromUri] int? sortOrder = null)
        {
			// ToDo: Fix this
			throw new NotImplementedException("ToDo: Fix this");
			//var contentGroupId = Sexy.GetContentGroupIdFromModule(ActiveModule.ModuleID);
			//var templateId = Sexy.GetTemplateForModule(ActiveModule.ModuleID).TemplateID;
			//SexyUncached.AddContentGroupItem(contentGroupId, UserInfo.UserID, templateId, null, sortOrder.HasValue ? sortOrder.Value + 1 : sortOrder, true, ContentGroupItemType.Content, false);
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public void SaveTemplateId([FromUri] int? templateId)
        {
			Sexy.ContentGroups.UpdateContentGroup(Sexy.GetContentGroupIdFromModule(ActiveModule.ModuleID), templateId);
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public void SetTemplateChooserState([FromUri]bool state)
        {
            new DotNetNuke.Entities.Modules.ModuleController().UpdateModuleSetting(ActiveModule.ModuleID, SexyContent.SettingsShowTemplateChooser, state.ToString());
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public IEnumerable<object> GetSelectableApps()
        {
            try
            {
                var zoneId = SexyContent.GetZoneID(ActiveModule.PortalID);
                return SexyContent.GetApps(zoneId.Value, false, new PortalSettings(ActiveModule.OwnerPortalID)).Select(a => new { a.Name, a.AppId });
            }
            catch (Exception e)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                throw e;
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public void SetAppId(int? appId)
        {
			// Reset template to nothing (prevents errors after changing app)
			Sexy.ContentGroups.UpdateContentGroup(Sexy.GetContentGroupIdFromModule(ActiveModule.ModuleID), null);

            SexyContent.SetAppIdForModule(ActiveModule, appId);

            // Change to 1. template if app has been set
            if (appId.HasValue)
            {
                var sexyForNewApp = new SexyContent(Sexy.App.ZoneId, appId.Value, false);
                var templates = sexyForNewApp.GetAvailableTemplatesForSelector(ActiveModule).ToList();
                if (templates.Any())
					Sexy.ContentGroups.UpdateContentGroup(Sexy.GetContentGroupIdFromModule(ActiveModule.ModuleID), templates.First().TemplateId);
                else
					Sexy.ContentGroups.UpdateContentGroup(Sexy.GetContentGroupIdFromModule(ActiveModule.ModuleID), null);
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public IEnumerable<object> GetSelectableContentTypes()
        {
            return Sexy.GetAvailableAttributeSetsForVisibleTemplates().Select(p => new { p.AttributeSetId, p.Name } );
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public IEnumerable<object> GetSelectableTemplates()
        {
            var availableTemplates = Sexy.GetAvailableTemplatesForSelector(ActiveModule);
            return availableTemplates.Select(t => new { t.TemplateId, t.Name, t.ContentTypeStaticName });
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RenderTemplate([FromUri]int templateId)
        {
            try
            {
                var template = Sexy.Templates.GetTemplate(templateId);

                var engine = EngineFactory.CreateEngine(template);
                var dataSource = (ViewDataSource) Sexy.GetViewDataSource(ActiveModule.ModuleID, SexyContent.HasEditPermission(ActiveModule), template);
                engine.Init(template, Sexy.App, ActiveModule, dataSource, InstancePurposes.WebView, Sexy);
                engine.CustomizeData();

				if (template.ContentTypeStaticName != "" && template.ContentDemoEntity == null && dataSource["Default"].List.Count == 0) { 
					var toolbar = "<ul class='sc-menu' data-toolbar='" + Newtonsoft.Json.JsonConvert.SerializeObject(new { sortOrder = 0, useModuleList = true, action = "edit" }) + "'></ul>";
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<div class='dnnFormMessage dnnFormInfo'>No demo item exists for the selected template. " + toolbar + "</div>") };
				}

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(engine.Render(), Encoding.UTF8, "text/plain");
                return response;
            }
            catch (Exception e)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                throw e;
            }
        }

    }
}