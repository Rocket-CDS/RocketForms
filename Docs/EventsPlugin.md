# RocketForms Events Plugin
The RocketForms System can be extended by adding an events plugin.

## Create Plugin
- Create project with : [RocketPluginProjectTemplate](https://github.com/Rocket-CDS/RocketPluginProjectTemplate)
- Convension Name of the new project is "RocketForms\<Plugin name>"
- Delete the API folder
- "App_Resources" Folder.  (If not using resx it can be deleted)
- In the Componants folder leave only the "Events.cs"
- "Render" Folder.  (If not using Razor templates it can be deleted)
- the "Themes" Folder.    (If not using Razor templates it can be deleted)

## system.rules
In system.rules file, just leave the event providerdata.
Example:
```
  <genxml>
    <systemkey>RocketForms</systemkey>
	<plugin>true</plugin>

    <groupsdata list="true">
    </groupsdata>

    <providerdata list="true">

		<genxml>
			<textbox>
				<interfacekey>events-rocketformstransoprtestimate</interfacekey>
				<namespaceclass>RocketFormsTransoprtEstimate.Components.Events</namespaceclass>
				<assembly>RocketFormsTransoprtEstimate</assembly>
				<interfaceicon></interfaceicon>
				<defaultcommand></defaultcommand>
				<relpath>/DesktopModules/DNNrocketModules/RocketFormsTransoprtEstimate</relpath>
			</textbox>
			<providertype>eventprovider</providertype>
			<dropdownlist>
				<group></group>
			</dropdownlist>
			<checkbox>
				<onmenu>false</onmenu>
				<active>true</active>
			</checkbox>
			<radio>
				<securityrolesadministrators>1</securityrolesadministrators>
				<securityrolesmanager>0</securityrolesmanager>
				<securityroleseditor>0</securityroleseditor>
				<securityrolesclienteditor>0</securityrolesclienteditor>
				<securityrolesregisteredusers>0</securityrolesregisteredusers>
				<securityrolessubscribers>0</securityrolessubscribers>
				<securityrolesall>0</securityrolesall>
			</radio>
		</genxml>

	</providerdata>

    <interfacedata list="true">
    </interfacedata>
  </genxml>

```

**NOTE: The plugin will need to be installed.  So the plugin is attached to the RocketForms system.**