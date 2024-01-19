# Razor View Data Objects
The razor templates can use a defined set of data objects.  Some are auomatically add on every call, some are only injected if required.  

## Standard Razor View Data Objects

    appTheme = (AppThemeLimpet)sModel.GetDataObject("apptheme");
    appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
    systemData = (SystemLimpet)sModel.GetDataObject("systemdata");
    portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
    moduleData = (ModuleFormsLimpet)sModel.GetDataObject("modulesettings");
    globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
    sessionParams = sModel.SessionParamsData;
    userParams = (UserParams)sModel.GetDataObject("userparams");
    appThemeList = (AppThemeDataList)sModel.GetDataObject("appthemedatalist");

    infoempty = new SimplisityInfo();

## Data Objects (AdminList.cshtml)
    var formlist = (List<SimplisityRecord>)Model.GetDataObject("formlist");
## Data Objects (EmailForm.cshtml)
    var sRec = (SimplisityRecord)Model.GetDataObject("formdata");
