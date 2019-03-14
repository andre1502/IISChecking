# IISChecking

IISChecking is a command tools which can help to do automation in IIS

# Features!
  - ApplicationPool monitoring
  - Create ApplicationSite [onbuild, not tested yet]
  - Stop ApplicationSite
  - Show ApplicationSite detail

Monitoring need to be set up manually through Task Scheduler with command parameter to run the software

### Tech
IISChecking uses a number of package (library) and target on specific IIS versions:
* [Microsoft.Web.Administration] - Management API for the web server that enables editing configuration through complete manipulation of the XML configuration files which need IIS versions 7.0 and above.
* [log4net] - Tool to help the programmer output log statements to a variety of output targets. In case of problems with an application, it is helpful to enable logging so that the problem can be located.
* [Newtonsoft] - Popular high-performance JSON framework for .NET.

### Development
IISChecking development started with [VisualStudio 2017] with following library or package which we define on [Tech](#tech) section

### Command
Please follow below script when try to run the command

#### ApplicationPool monitoring:
For monitoring which ApplicationSite is running but their ApplicationPool suddenly stopped.
```cmd
IISChecking.exe /c checkapppool
```

#### Create ApplicationSite:
To create ApplicationSite, config file need to be created based on [json file example] which provided inside the code.
```cmd
IISChecking.exe /c createsite [ApplicationSiteConfig]
```
Example of json content:
```json
{
  "ApplicationName": "001-TestApplication",
  "PhysicalPath": {
    "Source": "C:\\tempApp\\001-TestApplication",
    "Destination": "C:\\www\\001-TestApplication"
  },
  "BindingInformation": [
    {
      "Protocol": "http",
      "IPAddress": "*",
      "Port": "80",
      "HostName": "abc.com"
    },
    {
      "Protocol": "http",
      "IPAddress": "*",
      "Port": "80",
      "HostName": "www.abc.com"
    }
  ],
  "Description": "One ApplicationSite, one json file"
}
```
This json file will also include ApplicationName (which also used as ApplicationPool name), Directory location, BindingInformation that need to be defined or mapped to IPAddress or Domain. Description only used for defining the ApplicationSite you want to create. One ApplicationSite only can have one json config file.

#### Stop ApplicationSite:
Stop specific ApplicationSite that supplied in the parameter (ApplicationSiteName are name of instance when you set in IIS). This command also make sure to stop their respective ApplicationPool with assumption their ApplicationPool aren't DefaultAppPool or not used by multiple ApplicationSite.
```cmd
IISChecking.exe /c stopsite [ApplicationSiteName]
```

#### Show ApplicationSite detail:
Show specific ApplicationSite information that supplied in the parameter (ApplicationSiteName are name of instance when you set in IIS).
```cmd
IISChecking.exe /c detailsite [ApplicationSiteName]
```

### Todos

 - Improve and test on [Create ApplicationSite] functions

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

[Microsoft.Web.Administration]: <https://docs.microsoft.com/en-us/iis/manage/scripting/how-to-use-microsoftwebadministration>
[log4net]: <https://logging.apache.org/log4net/>
[Newtonsoft]: <https://www.newtonsoft.com/json>
[VisualStudio 2017]: <https://visualstudio.microsoft.com/>
[json file example]: <IISChecking/IISChecking/configurations/examplesite.json>