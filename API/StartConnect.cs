using DNNrocketAPI.Components;
using RocketContent.Components;
using RocketForms.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RocketForms.API
{
    public partial class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private SessionParams _sessionParams;
        private int _portalId;
        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = "";
            _portalId = PortalUtils.GetPortalId();
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
            var fMapPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\RocketForms\\" + moduleRef + "\\" + formref;
            if (File.Exists(fMapPath)) File.Delete(fMapPath);
            return ListForm();
        }
        public string DeleteFormAll()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
            var formref = _paramInfo.GetXmlProperty("genxml/hidden/formref");
            var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\RocketForms\\" + moduleRef;
            var l = Directory.GetFiles(folderPath);
            foreach (var f in l)
            {
                if (File.Exists(f)) File.Delete(f);
            }
            return ListForm();
        }
        public string ListForm()
        {
            try
            {

                var moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
                var portalid = PortalUtils.GetCurrentPortalId();
                var remoteModule = new RemoteModule(portalid, moduleRef);
                var appTheme = new AppThemeLimpet(portalid, remoteModule.AppThemeViewFolder, remoteModule.AppThemeViewVersion, remoteModule.Organisation);

                // Get list of forms from Directory
                var formlist = new List<SimplisityRecord>();
                var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\RocketForms\\" + moduleRef;
                var l = Directory.GetFiles(folderPath).OrderByDescending(i => i);
                foreach (var f in l)
                {
                    var fData = FileUtils.ReadFile(f);
                    var sRec = new SimplisityRecord();
                    var sRecDecode = new SimplisityRecord();
                    sRec.FromXmlItem(fData);
                    var dList = sRec.ToDictionary();
                    foreach (var d in dList)
                    {
                        var strText = d.Value;

                        if (d.Key == "createddate")
                        {
                            sRecDecode.SetXmlProperty("genxml/" + d.Key, strText);
                        }
                        else if (d.Key.StartsWith("base64-"))
                        {
                            string[] b64split = strText.Split(new[] { '-', ',' });
                            if (b64split.Length == 2)
                            {
                                sRecDecode.SetXmlProperty("genxml/" + d.Key + "meta", b64split[0]);
                                sRecDecode.SetXmlProperty("genxml/" + d.Key + "value", b64split[1]);
                            }
                            sRecDecode.SetXmlProperty("genxml/" + d.Key, strText);
                        }
                        else
                        { 
                            strText = strText.Replace(Environment.NewLine, "<br/>");
                            strText = strText.Replace("\n", "<br/>");
                            strText = strText.Replace("\r", "<br/>");
                            sRecDecode.SetXmlProperty("genxml/" + d.Key, GeneralUtils.Base64Decode(strText));
                        }
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
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return ex.ToString();
            }
        }

        public string PostForm()
        {
            var moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            var portalid = PortalUtils.GetCurrentPortalId();
            var remoteModule = new RemoteModule(portalid, moduleRef);
            var appTheme = new AppThemeLimpet(portalid, remoteModule.AppThemeViewFolder, remoteModule.AppThemeViewVersion, remoteModule.Organisation);

            //Save To DISK.  Save all post to disk (NOT DB, to stop SQL inject).  Also encode XML as Base64.
            var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\RocketForms\\" + moduleRef;
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            // remove any hack injection
            var dList = _postInfo.ToDictionary();
            var rtnRecord = new SimplisityRecord();
            rtnRecord.SetXmlProperty("genxml/data/createddate", DateTime.Now.ToString("O"),TypeCode.DateTime);
            foreach (var d in dList)
            {
                if (d.Key.StartsWith("base64-"))
                {
                    rtnRecord.SetXmlProperty("genxml/data/" + d.Key, d.Value.Replace(" ", "+"));
                }
                else
                {
                    var strText = d.Value;
                    strText = System.Web.HttpUtility.HtmlEncode(strText);
                    strText = strText.Replace("&gt;", "-");
                    strText = strText.Replace("&lt;", "-");
                    rtnRecord.SetXmlProperty("genxml/data/" + d.Key, GeneralUtils.Base64Encode(strText));
                }
            }
            var filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").Replace(" ", "") + "_" + GeneralUtils.GetGuidKey();
            rtnRecord.GUIDKey = filename;
            var dataSave = rtnRecord.ToXmlItem();

            var recordByteSize = System.Text.Encoding.Unicode.GetByteCount(dataSave);

            if (recordByteSize < 50000000) // 50MB limit
            {

                FileUtils.SaveFile(folderPath + "\\" + filename, dataSave);

                var portalContent = new PortalContentLimpet(portalid, _sessionParams.CultureCodeEdit); // Portal 0 is admin, editing portal setup

                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("remotemodule", remoteModule);
                dataObjects.Add("portalcontent", portalContent);

                var razorTemplEmail = appTheme.GetTemplate("EmailForm.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTemplEmail, new SimplisityInfo(rtnRecord), dataObjects, null, _sessionParams, true);
                if (pr.StatusCode != "00") LogUtils.LogSystem("ERROR - RocketForms Email: " + pr.ErrorMsg);
                if (portalContent.DebugMode)
                {
                    var emailfilename = PortalUtils.TempDirectoryMapPath() + "\\rocketforms_email.html";
                    if (pr.StatusCode != "00")
                        FileUtils.SaveFile(emailfilename, pr.ErrorMsg);
                    else
                        FileUtils.SaveFile(emailfilename, pr.RenderedText);
                }
                if (portalContent.EmailOn && pr.StatusCode == "00" && remoteModule.Record.GetXmlPropertyBool("genxml/checkbox/emailon"))
                {
                    var eFunc = new EmailLimpet(portalid, _sessionParams.CultureCode);
                    eFunc.SendEmail(pr.RenderedText, remoteModule.Record.GetXmlProperty("genxml/textbox/fromemail"), rtnRecord.GetXmlProperty("genxml/data/email"), remoteModule.Record.GetXmlProperty("genxml/textbox/replytoemail"), remoteModule.Record.GetXmlProperty("genxml/textbox/subject"));
                }
                var razorTempl = appTheme.GetTemplate("SentMessage.cshtml");
                pr = RenderRazorUtils.RazorProcessData(razorTempl, _postInfo, dataObjects, null, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return "Invalid record";
        }
    }

}
