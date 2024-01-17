using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketContentAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketForms.Components
{
    public static class RocketFormsUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketForms";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketForms/App_LocalResources";
        public static List<SimplisityRecord> DependanciesList(int portalId, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new List<SimplisityRecord>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam, false);
            if (dataObject.AppTheme != null && dataObject.AppTheme.Exists)
            {
                foreach (var depfile in dataObject.AppTheme.GetTemplatesDep())
                {
                    var dep = dataObject.AppTheme.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("deps"))
                    {
                        var urlstr = r.GetXmlProperty("genxml/url");
                        if (urlstr.Contains("{"))
                        {
                            if (dataObject.PortalData != null) urlstr = urlstr.Replace("{domainurl}", dataObject.PortalData.EngineUrlWithProtocol);
                            if (dataObject.AppTheme != null) urlstr = urlstr.Replace("{appthemefolder}", dataObject.AppTheme.AppThemeVersionFolderRel);
                            if (dataObject.AppThemeSystem != null) urlstr = urlstr.Replace("{appthemesystemfolder}", dataObject.AppThemeSystem.AppThemeVersionFolderRel);
                        }
                        r.SetXmlProperty("genxml/id", CacheUtils.Md5HashCalc(urlstr));
                        r.SetXmlProperty("genxml/url", urlstr);
                        rtn.Add(r);
                    }
                }
            }
            return rtn;
        }
        public static string DisplayView(int portalId, string systemKey, string moduleRef, string rowKey, SessionParams sessionParam, string template = "view.cshtml", string noAppThemeReturn= "")
        {
            var cacheKey = moduleRef + template + "_" + sessionParam.CultureCode;
            var moduleSettings = new ModuleFormsLimpet(portalId, moduleRef, systemKey, sessionParam.ModuleId, sessionParam.TabId);
            var pr = (RazorProcessResult)CacheUtils.GetCache(cacheKey, moduleRef);
            if (moduleSettings.DisableCache || pr == null)
            {
                var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam, false);
                if (!dataObject.ModuleSettings.HasAppThemeAdmin) return noAppThemeReturn; // test on Admin Theme.
                var razorTempl = dataObject.AppTheme.GetTemplate(template, moduleRef);
                pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                CacheUtils.SetCache(cacheKey, pr, moduleRef);
            }
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplaySystemView(int portalId, string moduleRef, SessionParams sessionParam, string template, bool editMode = true)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam, editMode);
            if (dataObject.AppThemeSystem == null) return "No System View";

            var razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString(ResourcePath, resourceKey, resourceExt, cultureCode);
        }        
    }

}
