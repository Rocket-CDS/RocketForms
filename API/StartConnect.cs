using DNNrocketAPI.Components;
using RocketForms.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RocketForms.API
{
    public partial class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private SessionParams _sessionParams;
        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = "";
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _sessionParams = new SessionParams(_paramInfo);
            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketforms_postform":
                    strOut = PostForm();
                    rtnDic.Add("outputhtml", strOut);
                    break;
                case "remote_edit":
                    strOut = ListForm();
                    rtnDic.Add("outputhtml", strOut);
                    break;
                case "remote_delete":
                    strOut = DeleteForm();
                    rtnDic.Add("outputhtml", strOut);
                    break;
                case "remote_deleteall":
                    strOut = DeleteFormAll();
                    rtnDic.Add("outputhtml", strOut);
                    break;
                default:
                    paramCmd = paramCmd.Replace("rocketforms_", "rocketcontent_");
                    systemInfo.SetXmlProperty("genxml/systemkey", "rocketforms");
                    var contentStartConnect = new RocketContent.API.StartConnect();
                    rtnDic = contentStartConnect.ProcessCommand(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);
                    break;
            }

            return rtnDic;

        }
        public string DeleteForm()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
            var formref = _paramInfo.GetXmlProperty("genxml/hidden/formref");           
            var fMapPath = PortalUtils.HomeDirectoryMapPath() + "\\RocketForms\\" + moduleRef + "\\" + formref;
            if (File.Exists(fMapPath)) File.Delete(fMapPath);
            return ListForm();
        }
        public string DeleteFormAll()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
            var formref = _paramInfo.GetXmlProperty("genxml/hidden/formref");
            var folderPath = PortalUtils.HomeDirectoryMapPath() + "\\RocketForms\\" + moduleRef;
            var l = Directory.GetFiles(folderPath);
            foreach (var f in l)
            {
                if (File.Exists(f)) File.Delete(f);
            }
            return ListForm();
        }
        public string ListForm()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
            var portalid = PortalUtils.GetCurrentPortalId();
            var remoteModule = new RemoteModule(portalid, moduleRef);
            var appTheme = new AppThemeLimpet(portalid, remoteModule.AppThemeViewFolder, remoteModule.AppThemeViewVersion, remoteModule.Organisation);

            // Get list of forms from Directory
            var formlist = new List<SimplisityRecord>();
            var folderPath = PortalUtils.HomeDirectoryMapPath() + "\\RocketForms\\" + moduleRef;
            var l = Directory.GetFiles(folderPath);
            foreach (var f in l)
            {
                var fData = GeneralUtils.Base64Decode(FileUtils.ReadFile(f));
                var sRec = new SimplisityRecord();
                var sRecDecode = new SimplisityRecord();
                sRec.FromXmlItem(fData);
                var dList = sRec.ToDictionary();
                foreach (var d in dList)
                {
                    var strText = d.Value;
                    strText = strText.Replace(Environment.NewLine, "<br/>");
                    strText = strText.Replace("\n", "<br/>");
                    strText = strText.Replace("\r", "<br/>");
                    sRecDecode.SetXmlProperty("genxml/" + d.Key, strText);
                }
                sRec.XMLData = sRecDecode.XMLData;
                formlist.Add(sRec);
            }
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("remotemodule", remoteModule);
            dataObjects.Add("formlist", formlist);
            var razorTempl = appTheme.GetTemplate("AdminList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _postInfo, dataObjects, null, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public string PostForm()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            var portalid = PortalUtils.GetCurrentPortalId();
            var remoteModule = new RemoteModule(portalid, moduleRef);
            var appTheme = new AppThemeLimpet(portalid, remoteModule.AppThemeViewFolder, remoteModule.AppThemeViewVersion, remoteModule.Organisation);

            //Save To DISK.  Save all post to disk (NOT DB, to stop SQL inject).  Also encode XML as Base64.
            var folderPath = PortalUtils.HomeDirectoryMapPath() + "\\RocketForms\\" + moduleRef;
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            // remove any hack injection
            var dList = _postInfo.ToDictionary();
            var rtnRecord = new SimplisityRecord();
            rtnRecord.ModifiedDate = DateTime.Now;
            foreach (var d in dList)
            {
                var strText = d.Value;
                strText = System.Web.HttpUtility.HtmlEncode(strText);
                strText = strText.Replace("&gt;", "");
                strText = strText.Replace("&lt;", "");
                rtnRecord.SetXmlProperty("genxml/data/" + d.Key, strText); 
            }
            var filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").Replace(" ", "") + "_" + GeneralUtils.GetGuidKey();
            rtnRecord.GUIDKey = filename;
            var dataSave = GeneralUtils.Base64Encode(rtnRecord.ToXmlItem());
            FileUtils.SaveFile(folderPath + "\\" + filename, dataSave);

            //[TODO: SEND EMAIL] 



            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("remotemodule", remoteModule);
            var razorTempl = appTheme.GetTemplate("SentMessage.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _postInfo, dataObjects, null, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
    }

}
