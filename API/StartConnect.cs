using DNNrocketAPI.Components;
using DNNrocketAPI.Interfaces;
using RocketContentAPI.Components;
using Simplisity;
using System.Collections.Generic;

namespace RocketForms.API
{
    public partial class StartConnect : IProcessCommand
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private SessionParams _sessionParams;
        private int _portalId;
        private int _moduleId;
        private int _tabId;
        private string _formref;
        private Components.DataObjectLimpet _dataObject;
        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketforms_publicpostform":
                    strOut = PostForm();
                    break;
                case "rocketforms_edit":
                    strOut = ListForm();
                    break;
                case "rocketforms_delete":
                    strOut = DeleteForm();
                    break;
                case "rocketforms_sendemail":
                    strOut = SendEmail();
                    break;
                case "rocketforms_deleteall":
                    strOut = DeleteFormAll();
                    break;



                case "rocketforms_settings":
                    strOut = DisplaySettings();
                    break;
                case "rocketforms_savesettings":
                    strOut = SaveSettings();
                    break;
                case "rocketforms_selectappthemeproject":
                    strOut = SelectAppThemeProject();
                    break;
                case "rocketforms_selectapptheme":
                    strOut = SelectAppTheme("");
                    break;
                case "rocketforms_selectappthemeview":
                    strOut = SelectAppTheme("view");
                    break;
                case "rocketforms_selectappthemeversion":
                    strOut = SelectAppThemeVersion("");
                    break;
                case "rocketforms_selectappthemeversionview":
                    strOut = SelectAppThemeVersion("view");
                    break;
                case "rocketforms_resetapptheme":
                    strOut = ResetAppTheme();
                    break;
                case "rocketforms_resetappthemeview":
                    strOut = ResetAppThemeView();
                    break;



                default:
                    strOut = "Invalid cmd: " + storeParamCmd;
                    break;
            }

            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);

            return rtnDic;

        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = PortalUtils.GetCurrentPortalId();
            if (portalid < 0 && systemInfo.PortalId >= 0) portalid = systemInfo.PortalId;

            var rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);

            _tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
            _moduleId = _postInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            _formref = _postInfo.GetXmlProperty("genxml/hidden/formref");
            if (_formref == "") _formref = _paramInfo.GetXmlProperty("genxml/hidden/formref");
            _sessionParams.ModuleId = _moduleId;
            _sessionParams.ModuleRef = portalid + "_ModuleID_" + _moduleId;
            _sessionParams.TabId = _tabId;

            // use a selectkey.  the selectkey is the same as the rowkey.
            // we can not duplicate ID on simplisity_click in the s-fields, when the id is on the form. 
            // The paramInfo field would contain the same as the form.  On load this may be empty.
            var selectkey = _paramInfo.GetXmlProperty("genxml/hidden/selectformref");
            if (selectkey != "") _formref = selectkey;

            // Assign Langauge
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            _dataObject = new Components.DataObjectLimpet(portalid, _sessionParams.ModuleRef, _formref, _sessionParams);

            if (!_dataObject.ModuleSettings.HasAppThemeAdmin) // Check if we have an AppTheme
            {
                if (!paramCmd.StartsWith("rocketforms_") && !paramCmd.StartsWith("rocketsystem_")) return "rocketforms_settings";
            }
            var securityData = new SecurityLimpet(_dataObject.PortalId, _dataObject.SystemKey, rocketInterface, _sessionParams.TabId, _sessionParams.ModuleId);
            return securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");
        }

    }

}
