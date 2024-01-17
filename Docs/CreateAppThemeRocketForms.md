**Create a Folders**

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm
```

And create version sub-folder.

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0
```

Create a "Default" sub-folder

```plaintext
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/default
```
There are a number of razor templates required for an AppTheme.    
The AppTheme included both Admin templates (optional) and the view (website display) templates.  Standard names and structures are required.

NOTE: All admin templates use the w3.css framework, which is automatically added to the page by the rocketforms.system.  
[https://www.w3schools.com/w3css/](https://www.w3schools.com/w3css/)  


#### **Step 2 -  Default Razor Templates**

Create a file called "**View.cshtml**" with this content...

```
@inherits RocketContentAPI.Components.RocketContentAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appThemeView, true)
<!--inject-->

<div class="w3-row w3-padding">
    <label>@ResourceKey("DNNrocket.heading")</label>
    @TextBox(headerData, "genxml/header/headingtitle", " class='w3-input w3-border' autocomplete='off' ", "", false, 0)
</div>

[INJECT:appthemeadmin,AdminRow.cshtml]
```


#### Other templates

**AdminFirstHeader.cshtml**  
This template is injected into the admin page header before any other template.  

**AdminLastHeader.cshtml**  
This template is injected into the admin page header after any other template.

**ViewFirstHeader.cshtml**  
This template is injected into the view page header before any other template.  

**ViewLastHeader.cshtml**  
This template is injected into the view page header after any other template.  

**ThemeSettings.cshtml**  
This template is used to get user settings for the AppTheme.

#### Testing the AppTheme

Add a page on the RocketCDS website and add the RocketForms module.

Go into the settings and selected the "ContactForm" Apptheme.
