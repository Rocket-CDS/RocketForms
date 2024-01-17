﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DNNrocketAPI.Components;
using DNNrocketAPI;

namespace RocketForms.Components
{
    public class ModuleFormsLimpet : ModuleBase
    {
        public ModuleFormsLimpet(int portalId, string moduleRef, string systemKey, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {
            base.SystemKey = systemKey;
        }
    }
}
