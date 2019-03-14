using IISChecking.Model;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IISChecking.Module {

  public class CreateApplicationSiteModule : Common {

    public CreateApplicationSiteModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public void CreateApplicationSite(string applicationSiteFile) {
      if (File.Exists(applicationSiteFile)) {
        string content = File.ReadAllText(applicationSiteFile);
        ApplicationSiteModel applicationSite = JsonConvert.DeserializeObject<ApplicationSiteModel>(content);
        string applicationName = applicationSite.ApplicationName;

        ServerManager serverManager = new ServerManager();
        SiteCollection siteCollection = serverManager.Sites;
        ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
        Site site = null;
        ApplicationPool applicationPool = null;

        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
        _logger.InfoFormat("[{0}] - applicationName: {1}", MethodInfo.GetCurrentMethod().Name, applicationName);

        if ((siteCollection != null) && (siteCollection.Any())) {
          site = siteCollection.FirstOrDefault(p => p.Name == applicationName);

          if ((applicationPoolCollection != null) && (applicationPoolCollection.Any())) {
            applicationPool = applicationPoolCollection.FirstOrDefault(p => p.Name == applicationName);
          }

          if ((site != null) && (applicationPool != null)) {
            _logger.InfoFormat("[{0}] - ApplicationSite already exists: {1} please use different name", MethodInfo.GetCurrentMethod().Name, applicationName);

            return;
          }
        }

        applicationPool = applicationPoolCollection.Add(applicationName);

        if (applicationPool != null) {
          applicationPool.AutoStart = true;
          applicationPool.Enable32BitAppOnWin64 = false;
          applicationPool.ManagedRuntimeVersion = "v4.0";
          applicationPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
          applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.ApplicationPoolIdentity;
        }

        string physicalPath = applicationSite.PhysicalPath.Destination;

        site = siteCollection.Add(applicationName, physicalPath, 80);
        site.ServerAutoStart = true;
        site.ApplicationDefaults.ApplicationPoolName = applicationPool.Name;
        site.Applications[0].ApplicationPoolName = applicationPool.Name;

        string protocol = "http";
        string ipAddress = "*";
        string port = "80";
        string hostName = "*";
        string bindingInformation = string.Format("{0}:{1}:{2}", ipAddress, port, hostName);

        foreach (BindingInformationModel itemBindingInformation in applicationSite.BindingInformation) {
          protocol = itemBindingInformation.Protocol;
          ipAddress = itemBindingInformation.IPAddress;
          port = itemBindingInformation.Port;
          hostName = itemBindingInformation.HostName;
          bindingInformation = string.Format("{0}:{1}:{2}", ipAddress, port, hostName);

          site.Bindings.Add(bindingInformation, protocol);
        }

        serverManager.CommitChanges();
        applicationPool.Recycle();

        _logger.InfoFormat("[{0}] - ApplicationSite created", MethodInfo.GetCurrentMethod().Name);
        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      } else {
        _logger.InfoFormat("[{0}] - ApplicationSite file config not found, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
      }
    }
  }
}