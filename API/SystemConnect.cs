using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RocketForms.API
{
    public partial class StartConnect
    {

        private string RenderSystemTemplate(string templateName)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate(templateName);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                _dataObject.PortalContent.Save(_postInfo);
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey + "setup", "True");
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey, "True");
                _dataObject.PortalData.Update();
                return RocketSystem();
            }
            return "Invalid PortalId";
        }
        private String RocketSystem()
        {
            return RenderSystemTemplate("RocketSystem.cshtml");
        }
        private String RocketSystemInit()
        {
            var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
            if (newportalId > 0)
            {
                var portalContent = new PortalContentLimpet(newportalId, _sessionParams.CultureCodeEdit);
                portalContent.Validate();
                portalContent.Active = true;
                portalContent.Update();
                _dataObject.SetDataObject("portalcontent", portalContent);
            }
            return "";
        }
        private String RocketSystemDelete()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalId > 0)
            {
                _dataObject.PortalContent.Delete();
            }
            return "";
        }
        private string ReloadPage()
        {
            // user does not have access, logoff
            UserUtils.SignOut();

            var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string MessageDisplay(string msgKey)
        {
            _dataObject.SetSetting("msgkey", msgKey);
            return RenderSystemTemplate("MessageDisplay.cshtml");
        }
        private string ExportData()
        {
            // check the scheduler initiated the call.
            var rtn = "";
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"));
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {

                rtn = "<export>";

                rtn += "<systemkey>" + _dataObject.SystemKey + "</systemkey>";
                rtn += "<databasetable></databasetable>";

                rtn += "<modulesettings>";
                rtn += _dataObject.ModuleSettings.Record.ToXmlItem();
                rtn += "</modulesettings>";

                rtn += "<appthemes>";
                rtn += "<admin>";

                var zipMapPath = _dataObject.AppTheme.ExportZipFile(_dataObject.ModuleSettings.ModuleRef);
                var systemByte = File.ReadAllBytes(zipMapPath);
                var systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                rtn += "<systembase64 filetype='zip'><![CDATA[";
                rtn += systemBase64;
                rtn += "]]></systembase64>";
                File.Delete(zipMapPath);

                zipMapPath = _dataObject.AppTheme.ExportPortalZipFile(_dataObject.ModuleSettings.ModuleRef);
                systemByte = File.ReadAllBytes(zipMapPath);
                systemBase64 = Convert.ToBase64String(systemByte, Base64FormattingOptions.None);
                rtn += "<portalbase64 filetype='zip'><![CDATA[";
                rtn += systemBase64;
                rtn += "]]></portalbase64>";
                File.Delete(zipMapPath);

                rtn += "</admin>";

                rtn += "</appthemes>";

                rtn += "</export>";
            }

            return rtn;
        }
        private void ImportAppTheme(AppThemeLimpet appTheme, XmlNode appThemeNod, string prefix)
        {
            if (appTheme != null && appThemeNod != null)
            {
                var base64String = appThemeNod.InnerText;
                if (base64String != "")
                {
                    var importZipMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + prefix + ".zip";
                    File.WriteAllBytes(importZipMapPath, Convert.FromBase64String(base64String));
                    appTheme.ImportZipFile(importZipMapPath);
                    if (File.Exists(importZipMapPath)) File.Delete(importZipMapPath);
                }
            }
        }
        private void ImportPortalAppTheme(AppThemeLimpet appTheme, XmlNode appThemeNod, string prefix, string oldModuleRef, string newModuleRef)
        {
            if (appTheme != null && appThemeNod != null)
            {
                var base64String = appThemeNod.InnerText;
                if (base64String != "")
                {
                    var importZipMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + prefix + ".zip";
                    File.WriteAllBytes(importZipMapPath, Convert.FromBase64String(base64String));
                    appTheme.ImportPortalZipFile(importZipMapPath, oldModuleRef, newModuleRef);
                    if (File.Exists(importZipMapPath)) File.Delete(importZipMapPath);
                }
            }
        }
        private void ImportData()
        {
            // check the scheduler initiated the call.
            var securityKey = DNNrocketUtils.GetTempStorage(_paramInfo.GetXmlProperty("genxml/hidden/securitykey"));
            if (securityKey != null) // if it exists in the temp table, it was created by the scheduler.
            {

                var moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
                var tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
                var systemKey = _paramInfo.GetXmlProperty("genxml/hidden/systemkey");
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                var databasetable = _paramInfo.GetXmlProperty("genxml/hidden/databasetable");
                var moduleRef = portalId + "_ModuleID_" + moduleId;

                var objCtrl = new DNNrocketController();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(_postInfo.XMLData);

                //import Settings (Saved in DNNrocket table)
                var settingsNod = xmlDoc.SelectSingleNode("export/modulesettings");
                if (settingsNod != null)
                {
                    var ms = new SimplisityRecord();
                    ms.FromXmlItem(settingsNod.InnerXml);
                    var rec = objCtrl.GetRecordByGuidKey(portalId, moduleId, "MODSETTINGS", moduleRef, "");
                    if (rec != null)
                    {
                        var storeId = rec.ItemID;
                        ms = rec;
                        ms.FromXmlItem(settingsNod.InnerXml);
                        ms.ItemID = storeId;
                    }
                    else
                        ms.ItemID = -1;

                    var legacymoduleref = ms.GUIDKey;
                    ms.SetXmlProperty("genxml/legacymoduleref", legacymoduleref); // used to link DataRef on Satellite modules.
                    var legacymoduleid = ms.ModuleId.ToString();
                    ms.SetXmlProperty("genxml/legacymoduleid", legacymoduleid);
                    ms.SetXmlProperty("genxml/settings/name", ms.GetXmlProperty("genxml/settings/name").Replace(legacymoduleid, moduleId.ToString()));
                    ms.PortalId = portalId;
                    ms.ModuleId = moduleId;
                    ms.GUIDKey = moduleRef;

                    objCtrl.Update(ms);

                    var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, systemKey, moduleId, tabId);

                    if (moduleSettings.HasProject)
                    {
                        AppThemeLimpet appThemeAdmin = null;
                        if (moduleSettings.HasAppThemeAdmin)
                        {
                            appThemeAdmin = new AppThemeLimpet(moduleSettings.PortalId, moduleSettings.AppThemeAdminFolder, moduleSettings.AppThemeAdminVersion, moduleSettings.ProjectName);
                        }

                        // Import AppTheme
                        ImportAppTheme(appThemeAdmin, xmlDoc.SelectSingleNode("export/appthemes/admin/systembase64"), moduleRef + "adminsystem");
                        ImportPortalAppTheme(appThemeAdmin, xmlDoc.SelectSingleNode("export/appthemes/admin/portalbase64"), moduleRef + "adminportal", legacymoduleref, moduleRef);
                    }
                }

            }

        }

    }
}

