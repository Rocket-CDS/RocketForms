using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketCatalog.Components;
using RocketContent.Components;
using RocketPortal.Components;
using Simplisity;

namespace RocketForms.Components
{
    public class Scheduler : SchedulerInterface
    {
        /// <summary>
        /// This is called by DNNrocketAPI.Components.RocketScheduler
        /// </summary>
        /// <param name="systemData"></param>
        /// <param name="rocketInterface"></param>
        public override void DoWork()
        {
            var portalList = PortalUtils.GetPortals();
            foreach (var portalId in portalList)
            {
                var portalData = new PortalContentLimpet(portalId, "");
                if (portalData.Active)
                {
                    if ((portalData.Record.GetXmlPropertyDate("genxml/schedulertimedaterocketforms").Date < DateTime.Now.Date && DateTime.Now > DateTime.Now.Date.AddHours(3)) || portalData.DebugMode)
                    {
                        var dirList = Directory.GetDirectories(PortalUtils.HomeDirectoryMapPath(portalId) + "\\RocketForms");
                        foreach (var dir in dirList)
                        {
                            foreach (var f in Directory.GetFiles(dir))
                            {
                                if (File.Exists(f) && (File.GetCreationTime(f) < DateTime.Now.AddDays(-60)))
                                {
                                    File.Delete(f);
                                }
                            }
                        }
                        portalData.Record.SetXmlProperty("genxml/schedulertimedaterocketforms", DateTime.Now.ToString("O"), TypeCode.DateTime);
                        portalData.Update();
                        LogUtils.LogSystem("RocketForms - Scheduler");
                    }
                }
            }
        }
    }
}