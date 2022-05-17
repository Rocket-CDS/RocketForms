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
        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            if (paramCmd == "rocketforms_postform")
            {
                var strOut = "";
                var moduleRef = paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                var portalid = PortalUtils.GetCurrentPortalId();
                var remoteModule = new RemoteModule(portalid, moduleRef);
                var appTheme = new AppThemeLimpet(portalid, remoteModule.AppThemeViewFolder, remoteModule.AppThemeViewVersion, remoteModule.Organisation);
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("remotemodule", remoteModule);
                var razorTempl = appTheme.GetTemplate("SentMessage.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, postInfo, dataObjects, null, new SessionParams(paramInfo), true);
                if (pr.StatusCode != "00") 
                    strOut = pr.ErrorMsg;
                else 
                    strOut = pr.RenderedText;

                //Save To DISK.  Save all post to disk (NOT DB, to help stop SQL inject).  Also encode XML as Base64.
                var folderPath = PortalUtils.HomeDirectoryMapPath() + "\\RocketForms\\" + moduleRef;
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var dataSave = GeneralUtils.Base64Encode(postInfo.ToXmlItem());
                var filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").Replace(" ", "") + "_" + GeneralUtils.GetGuidKey();
                FileUtils.SaveFile(folderPath + "\\" + filename, dataSave);

                //[TODO: SEND EMAIL] 


                var rtnDic = new Dictionary<string, object>();
                rtnDic.Add("outputhtml", strOut);
                return rtnDic;
            }
            else
            {
                paramCmd = paramCmd.Replace("rocketforms_", "rocketcontent_");
                systemInfo.SetXmlProperty("genxml/systemkey", "rocketforms");
                var contentStartConnect = new RocketContent.API.StartConnect();
                return contentStartConnect.ProcessCommand(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);
            }
        }
    }

}
