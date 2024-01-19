**Create a Folders**

```
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.example1/1.0
```

And create sub-folders.

```
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/css
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/default
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/dep
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/img
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/js
/DesktopModules/RocketThemes/AppThemes-W3-CSS/rocketforms.ContactForm/1.0/resx
```
*note: We do not use all the sub-folders in this example*

There are a number of razor templates required for an AppTheme.    
Standard names and structures are expected.

### **Step 2 -  Form Template**

Create a file called "**View.cshtml**" with this content...

```
@inherits RocketForms.Components.RocketFormsTokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketForms.Components;
@using Simplisity;
@using RazorEngine.Text;
@using Rocket.AppThemes.Components;
@using RocketPortal.Components;

@AssigDataModel(Model)
<!--inject-->

<div id="contactform@(moduleData.ModuleId)" class=" w3-row">
    <div class="w3-container">
        <label>@ResourceKey("RocketForms.name")</label>
        @TextBox(infoempty, "genxml/textbox/name"," class='w3-input w3-border'")
    </div>

    <div id="buttondiv@(moduleData.ModuleId)" class="w3-center w3-margin simplisity_panel" style="min-height:40px;">
        <div class="w3-button w3-green">@ResourceKey("RocketForms.send")</div>
    </div>
    <!-- we must pass moduleid to server -->
    @HiddenField(infoempty, "genxml/hidden/moduleid","",moduleData.ModuleId.ToString())
</div>

<div class="simplisity_loader" style="display:none;">
    <span class=" simplisity_loader_inner">
    </span>
</div>

<script>
    $( document ).ready(function() {

        setTimeout(function(){
            var buttontext = '<button id="sendemail@(moduleData.ModuleId)" type="button" class="w3-button w3-green simplisity_click" s-before="move@(moduleData.ModuleId)" s-return="#buttondiv@(moduleData.ModuleId)"  s-cmdurl="/Desktopmodules/dnnrocket/api/rocket/action" s-cmd="rocketforms_publicpostform"  s-post="#contactform@(moduleData.ModuleId)" >@ResourceKey("RocketForms.send")</button>'
            $('#buttondiv@(moduleData.ModuleId)').html(buttontext);

            $('#contactform@(moduleData.ModuleId)').activateSimplisityPanel();

        }, 1200);

    });

    function move@(moduleData.ModuleId)() {
        $('.simplisity_loader').show();
    }
</script>

```


### Other templates

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

### Admin Templates (optonal)


*note: All admin templates use the w3.css framework, which is automatically added to the page by the rocketforms.system.*  
[https://www.w3schools.com/w3css/](https://www.w3schools.com/w3css/)  


#### Testing the AppTheme

Add a page on the RocketCDS website and add the RocketForms module.

Go into the settings and selected the "ContactForm" Apptheme.


