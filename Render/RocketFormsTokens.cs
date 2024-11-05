using DNNrocketAPI.Components;
using RazorEngine.Text;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketForms.Components
{
    public class RocketFormsTokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {

        // Define data classes, so we can use intellisense in inject templates
        public AppThemeLimpet appTheme;
        public AppThemeSystemLimpet appThemeSystem;
        public ModuleFormsLimpet moduleData;
        [Obsolete("Use moduleData instead")]
        public ModuleFormsLimpet moduleSettings;
        public PortalLimpet portalData;
        public List<string> enabledlanguages = DNNrocketUtils.GetCultureCodeList();
        public SessionParams sessionParams;
        public UserParams userParams;
        public SimplisityInfo info;
        public SimplisityInfo infoempty;
        public SystemLimpet systemData;
        public SystemLimpet systemDirectoryData;
        public SystemGlobalData globalSettings;
        public AppThemeDataList appThemeList;

        public string AssigDataModel(SimplisityRazor sModel)
        {
            AssignDataModel(sModel);
            return "";
        }

        public string AssignDataModel(SimplisityRazor sModel)
        {
            appTheme = (AppThemeLimpet)sModel.GetDataObject("apptheme");
            appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
            systemData = (SystemLimpet)sModel.GetDataObject("systemdata");
            portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
            moduleData = (ModuleFormsLimpet)sModel.GetDataObject("modulesettings");
            globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");
            appThemeList = (AppThemeDataList)sModel.GetDataObject("appthemedatalist");

            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            infoempty = new SimplisityInfo();

            AddProcessDataResx(appTheme, true);
            AddProcessData("resourcepath", systemData.SystemRelPath + "/App_LocalResources/");

            // legacy
            moduleSettings = moduleData;


            // use return of "string", so we don;t get error with converting void to object.
            return "";
        }


        /// <summary>
        /// Displays a "Post" button with delay.  This is to avoid unwanted robot posts.
        /// </summary>
        /// <param name="sModel">Model</param>
        /// <param name="spost">jquery selector of post element (element wrapper id)</param>
        /// <param name="millisec"> time of delay until display</param>
        /// <param name="template">razor template</param>
        /// <returns>html for delayed display button</returns>
        public IEncodedString DelayFormButton(SimplisityRazor sModel, string spost, int millisec = 1200, string template = "DelayFormButton.cshtml")
        {
            sModel.SetSetting("spost", spost);
            var appTheme = (AppThemeLimpet)sModel.GetDataObject("apptheme");
            var appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
            var razorTempl = appTheme.GetTemplate(template, sModel.SessionParamsData.ModuleRef);
            if (razorTempl == "") razorTempl = appThemeSystem.GetTemplate(template, sModel.SessionParamsData.ModuleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, sModel.DataObjects, sModel.Settings, sModel.SessionParamsData, true);
            if (pr.StatusCode != "00") return new RawString("DelayFormButton Razor Token.  : " + pr.ErrorMsg);
            return new RawString(pr.RenderedText);
        }
        public string ResourceKey(AppThemeLimpet appTheme, String resourceKey, String lang = "", String resourceExtension = "Text")
        {
            var strOut = "";
            if (Processdata.ContainsKey("resourcepath"))
            {
                var resxlist = new List<string>();
                resxlist.Add("RocketForms");
                foreach (var f in appTheme.GetTemplatesResx())
                {
                    var fKey = f.Key.Split('.')[0].ToLower();
                    if (!resxlist.Contains(fKey)) resxlist.Add(fKey);
                }

                var l = Processdata["resourcepath"];
                foreach (var r in l)
                {
                    foreach (var rf in resxlist)
                    {
                        var resourceFileKey = rf + "." + resourceKey;
                        strOut = DNNrocketUtils.GetResourceString(r, resourceFileKey, resourceExtension, lang);
                        if (strOut != "") break;
                    }
                    if (strOut != "") break;
                }
            }
            if (strOut == "") strOut = resourceKey;
            return strOut;
        }
        /// <summary>
        /// Standardized method and names to craete top,bottom,left,right padding on an element.
        /// Allows potion adjustment from module settings without change CSS files.
        /// field Id: leftpadding,rightpadding,toppadding,bottompadding
        /// </summary>
        /// <param name="sModel"></param>
        /// <returns>The padding CSS for an inline style on an element.</returns>
        public string StylePadding()
        {
            var strOut = "";
            if (moduleData.GetSettingInt("leftpadding") > 0)
            {
                strOut += "padding-left:" + moduleData.GetSettingInt("leftpadding") + "px;";
            }
            if (moduleData.GetSettingInt("rightpadding") > 0)
            {
                strOut += "padding-right:" + moduleData.GetSettingInt("rightpadding") + "px;";
            }
            if (moduleData.GetSettingInt("toppadding") > 0)
            {
                strOut += "padding-top:" + moduleData.GetSettingInt("toppadding") + "px;";
            }
            if (moduleData.GetSettingInt("bottompadding") > 0)
            {
                strOut += "padding-bottom:" + moduleData.GetSettingInt("bottompadding") + "px;";
            }
            return strOut;
        }


    }
}
