# Extra Functionality

## emailsubjectprefix
If there is a hidden field call "emailsubjectprefix" in the form, then the value of that fields will be taken as a prefix to the email subject.   
*Tempate Code*
```
@HiddenField(new SimplisityInfo(), "genxml/hidden/emailsubjectprefix")
```
*Server side code*
```
var emailsubjectprefix = rtnRecord.GetXmlProperty("genxml/hidden/emailsubjectprefix");
```

