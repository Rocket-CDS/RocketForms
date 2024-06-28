# Extra Functionality

## Hidden fields removed
If any fields on the form are hidden fields they will be removed.  *(With the exception of the "emailsubjectprefix" field.)*  
If hidden data needs to be passed back to the server via the form a textbox should be used and hidden with css.


## emailsubjectprefix
If there is a field call "emailsubjectprefix" in the form, then the value of that fields will be taken as a prefix to the email subject.    


*Example Tempate Code*
```
@HiddenField(new SimplisityInfo(), "genxml/hidden/emailsubjectprefix","","DefaultPREFIX")
```
```
@TextBox(new SimplisityInfo(), "genxml/textbox/emailsubjectprefix", " class='w3-input w3-border'")
```

The idea is that the value of the field can be altered based on selections the user makes in the form.

NOTE: Ensure the field is within the s-return element.