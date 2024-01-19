using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using RocketContentAPI.Components;
using System.Reflection;

namespace RocketForms.Components
{
    public class DataObjectLimpet
    {
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _passSettings;
        public DataObjectLimpet(int portalid, string moduleRef, string formref, SessionParams sessionParams, bool editMode = true)
        {
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, formref, cultureCode, sessionParams.ModuleId, sessionParams.TabId);
        }
        public DataObjectLimpet(int portalid, string moduleRef, string formref, string cultureCode, int moduleId, int tabId)
        {
            Populate(portalid, moduleRef, formref, cultureCode, moduleId, tabId);
        }
        public void Populate(int portalid, string moduleRef, string formref, string cultureCode, int moduleId, int tabId)
        {
            FormRef = formref;

            _passSettings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            var moduleSettings = new ModuleFormsLimpet(portalid, moduleRef, SystemKey, moduleId, tabId);

            SetDataObject("modulesettings", moduleSettings);
            SetDataObject("appthemesystem", AppThemeUtils.AppThemeSystem(portalid, SystemKey));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", SystemSingleton.Instance(SystemKey));
            SetDataObject("appthemeprojects", AppThemeUtils.AppThemeProjects());
            SetDataObject("userparams", new UserParams("ModuleID:" + moduleSettings.ModuleId, true));

        }
        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);

            if (key == "modulesettings") // load appTheme if we has settings in ModuleSettings
            {
                if (ModuleSettings.HasProject)
                {
                    SetDataObject("appthemedatalist", new AppThemeDataList(ModuleSettings.PortalId, ModuleSettings.ProjectName, SystemKey));
                    if (ModuleSettings.HasAppThemeAdmin)
                    {
                        SetDataObject("apptheme", new AppThemeLimpet(ModuleSettings.PortalId, ModuleSettings.AppThemeAdminFolder, ModuleSettings.AppThemeAdminVersion, ModuleSettings.ProjectName));
                    }
                }
            }
        }
        public object GetDataObject(String key)
        {
            if (_dataObjects != null && _dataObjects.ContainsKey(key)) return _dataObjects[key];
            return null;
        }
        public void SetSetting(string key, string value)
        {
            if (_passSettings.ContainsKey(key)) _passSettings.Remove(key);
            _passSettings.Add(key, value);
        }
        public string GetSetting(string key)
        {
            if (!_passSettings.ContainsKey(key)) return "";
            return _passSettings[key];
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public string FormRef { set; get; }
        public string SystemKey { get { return "rocketforms"; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public ModuleFormsLimpet ModuleSettings { get { return (ModuleFormsLimpet)GetDataObject("modulesettings"); } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public PortalContentLimpet PortalContent { get { return (PortalContentLimpet)GetDataObject("portalcontent"); } }
        public AppThemeLimpet AppTheme { get { return (AppThemeLimpet)GetDataObject("apptheme"); } set { SetDataObject("apptheme", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public Dictionary<string, string> Settings { get { return _passSettings; } }

    }
}
