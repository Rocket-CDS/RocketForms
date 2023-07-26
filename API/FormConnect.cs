using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using RocketForms.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketForms.API
{
    public partial class StartConnect
    {

        public string DeleteForm()
        {
            var fMapPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\DNNrocket\\" + _dataObject.SystemKey + "\\" + _dataObject.ModuleSettings.ModuleRef + "\\" + _formref;
            if (File.Exists(fMapPath)) File.Delete(fMapPath);
            return ListForm();
        }
        public string SendEmail()
        {
            var fMapPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\DNNrocket\\" + _dataObject.SystemKey + "\\" + _dataObject.ModuleSettings.ModuleRef + "\\" + _formref;
            if (File.Exists(fMapPath))
            {
                SendEmailForm(_portalId, fMapPath);
            }
            return ListForm();
        }
        public string DeleteFormAll()
        {
            var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\DNNrocket\\" + _dataObject.SystemKey + "\\" + _dataObject.ModuleSettings.ModuleRef;
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
                var portalid = PortalUtils.GetCurrentPortalId();

                // Get list of forms from Directory
                var formlist = new List<SimplisityRecord>();
                var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\DNNrocket\\" + _dataObject.SystemKey + "\\" + _dataObject.ModuleSettings.ModuleRef;
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var l = Directory.GetFiles(folderPath).OrderByDescending(i => i);
                foreach (var f in l)
                {
                    var sRec = ReadFileRecord(f);
                    formlist.Add(sRec);
                }
                _dataObject.SetDataObject("formlist", formlist);
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("AdminDetail.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _postInfo, _dataObject.DataObjects, null, _sessionParams, true);
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
            //Save To DISK.  Save all post to disk (NOT DB, to stop SQL inject).  Also encode XML as Base64.
            var folderPath = PortalUtils.HomeDirectoryMapPath(_portalId) + "\\DNNrocket\\" + _dataObject.SystemKey + "\\" + _dataObject.ModuleSettings.ModuleRef;
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            // remove any hack injection
            var dList = _postInfo.ToDictionary();
            var rtnRecord = new SimplisityRecord();
            rtnRecord.SetXmlProperty("genxml/data/createddate", DateTime.Now.ToString("O"), TypeCode.DateTime);

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
                var fileMapPath = folderPath + "\\" + filename;
                FileUtils.SaveFile(fileMapPath, dataSave);
                SendEmailForm(_portalId, fileMapPath);
                var portalContent = new PortalContentLimpet(_portalId, _sessionParams.CultureCodeEdit); // Portal 0 is admin, editing portal setup
                var razorTempl = _dataObject.AppTheme.GetTemplate("SentMessage.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _postInfo, _dataObject.DataObjects, null, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            else
            {
                LogUtils.LogSystem("Email Limit Exceeded: " + folderPath + "\\" + filename);
            }
            return "Invalid record";
        }
        public SimplisityRecord ReadFileRecord(string fileMapPath)
        {
            var dataRecord = FileUtils.ReadFile(fileMapPath);
            if (dataRecord == "") return null;
            var rtnRecord = new SimplisityRecord();
            rtnRecord.XMLData = dataRecord;

            var fData = FileUtils.ReadFile(fileMapPath);
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
                    try
                    {
                        strText = GeneralUtils.Base64Decode(strText);
                        strText = strText.Replace(Environment.NewLine, "<br/>");
                        strText = strText.Replace("\n", "<br/>");
                        strText = strText.Replace("\r", "<br/>");
                        sRecDecode.SetXmlProperty("genxml/" + d.Key, strText);
                    }
                    catch (Exception)
                    {
                        sRecDecode.SetXmlProperty("genxml/" + d.Key, "ERROR");
                    }
                }
            }
            sRec.XMLData = sRecDecode.XMLData;
            return sRec;
        }
        public void SendEmailForm(int portalid, string fileMapPath)
        {
            var rtnRecord = ReadFileRecord(fileMapPath);
            if (rtnRecord != null)
            {
                var razorTemplEmail = _dataObject.AppTheme.GetTemplate("EmailForm.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTemplEmail, new SimplisityInfo(rtnRecord), _dataObject.DataObjects, null, _sessionParams, true);
                if (pr.StatusCode != "00") LogUtils.LogSystem("ERROR - RocketForms Email: " + pr.ErrorMsg);
                if (_dataObject.ModuleSettings.GetSettingBool("debugmode"))
                {
                    var debugPath = PortalUtils.TempDirectoryMapPath() + "\\debug";
                    if (!Directory.Exists(debugPath)) Directory.CreateDirectory(debugPath);
                    var emailfilename = debugPath + "\\rocketforms_email.html";
                    if (pr.StatusCode != "00")
                        FileUtils.SaveFile(emailfilename, pr.ErrorMsg);
                    else
                        FileUtils.SaveFile(emailfilename, pr.RenderedText);
                }
                if (pr.StatusCode == "00" && _dataObject.ModuleSettings.GetSettingBool("emailon"))
                {
                    var eFunc = new EmailLimpet(portalid, _sessionParams.CultureCode);
                    var replyEmail = rtnRecord.GetXmlProperty("genxml/textbox/email");
                    if (replyEmail == "") replyEmail = rtnRecord.GetXmlProperty("genxml/textbox/replytoemail");
                    if (replyEmail == "") replyEmail = _dataObject.ModuleSettings.GetSetting("manageremail");
                    eFunc.SendEmail(pr.RenderedText, _dataObject.ModuleSettings.GetSetting("fromemail"), _dataObject.ModuleSettings.GetSetting("manageremail"), replyEmail, _dataObject.ModuleSettings.GetSetting("subject"));
                }
            }
        }

    }
}

