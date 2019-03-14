using Microsoft.Web.Administration;
using System.Linq;
using System.Reflection;

namespace IISChecking.Module {

  public class ApplicationSiteDetailModule : Common {

    public ApplicationSiteDetailModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public void GetApplicationPoolDetail(ApplicationPoolCollection applicationPoolCollection, string applicationPoolName) {
      ApplicationPoolProcessModel applicationPoolProcessModel = null;
      ApplicationPool applicationPool = null;

      if ((applicationPoolCollection != null) && (applicationPoolCollection.Any())) {
        _logger.InfoFormat("[{0}] - applicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolName);

        applicationPool = applicationPoolCollection.FirstOrDefault(p => p.Name == applicationPoolName);
        if (applicationPool != null) {
          _logger.InfoFormat("[{0}] - applicationPool.Name: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.Name);
          _logger.InfoFormat("[{0}] - applicationPool.AutoStart: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.AutoStart);
          _logger.InfoFormat("[{0}] - applicationPool.Enable32BitAppOnWin64: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.Enable32BitAppOnWin64);
          _logger.InfoFormat("[{0}] - applicationPool.ManagedRuntimeVersion: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.ManagedRuntimeVersion);
          _logger.InfoFormat("[{0}] - applicationPool.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)applicationPool.State);
          _logger.InfoFormat("[{0}] - applicationPool.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.State);

          applicationPoolProcessModel = applicationPool.ProcessModel;
          _logger.InfoFormat("[{0}] - applicationPoolProcessModel.IdentityType: {1}", MethodInfo.GetCurrentMethod().Name, (int)applicationPoolProcessModel.IdentityType);
          _logger.InfoFormat("[{0}] - applicationPoolProcessModel.IdentityType[string]: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.IdentityType);
          _logger.InfoFormat("[{0}] - applicationPoolProcessModel.UserName: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.UserName);
          _logger.InfoFormat("[{0}] - applicationPoolProcessModel.Password: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.Password);
        } else {
          _logger.InfoFormat("[{0}] - ApplicationPool doesn't exists", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - Server doesn't have any ApplicationPool", MethodInfo.GetCurrentMethod().Name);
      }
    }

    public void GetApplicationSiteDetail(string applicationName) {
      ServerManager serverManager = new ServerManager();
      SiteCollection siteCollection = serverManager.Sites;

      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      if ((siteCollection != null) && (siteCollection.Any())) {
        ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
        string previousApplicationPoolName = string.Empty;
        string currentApplicationPoolName = string.Empty;

        _logger.InfoFormat("[{0}] - applicationName: {1}", MethodInfo.GetCurrentMethod().Name, applicationName);

        Site site = siteCollection.FirstOrDefault(p => p.Name == applicationName);
        if (site != null) {
          _logger.InfoFormat("[{0}] - site.Id: {1}", MethodInfo.GetCurrentMethod().Name, site.Id);
          _logger.InfoFormat("[{0}] - site.Name: {1}", MethodInfo.GetCurrentMethod().Name, site.Name);
          _logger.InfoFormat("[{0}] - site.ServerAutoStart: {1}", MethodInfo.GetCurrentMethod().Name, site.ServerAutoStart);
          _logger.InfoFormat("[{0}] - site.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)site.State);
          _logger.InfoFormat("[{0}] - site.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, site.State);

          BindingCollection bindings = site.Bindings;
          _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
          foreach (Binding binding in bindings) {
            _logger.InfoFormat("[{0}] - binding.Protocol: {1}", MethodInfo.GetCurrentMethod().Name, binding.Protocol);
            _logger.InfoFormat("[{0}] - binding.BindingInformation: {1}", MethodInfo.GetCurrentMethod().Name, binding.BindingInformation);
            _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
          }
          _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);

          ApplicationCollection applications = site.Applications;
          foreach (Application itemApplication in site.Applications) {
            currentApplicationPoolName = itemApplication.ApplicationPoolName;

            _logger.InfoFormat("[{0}] - itemApplication.ApplicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, currentApplicationPoolName);
            _logger.InfoFormat("[{0}] - itemApplication.EnabledProtocols: {1}", MethodInfo.GetCurrentMethod().Name, itemApplication.EnabledProtocols);
            _logger.InfoFormat("[{0}] - itemApplication.Path: {1}", MethodInfo.GetCurrentMethod().Name, itemApplication.Path);

            if ((string.IsNullOrWhiteSpace(previousApplicationPoolName) || (currentApplicationPoolName != previousApplicationPoolName))) {
              previousApplicationPoolName = currentApplicationPoolName;
              GetApplicationPoolDetail(applicationPoolCollection, currentApplicationPoolName);
            } else {
              _logger.InfoFormat("[{0}] - using same ApplicationPoolName", MethodInfo.GetCurrentMethod().Name);
            }

            _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
            foreach (VirtualDirectory itemVirtualDirectory in itemApplication.VirtualDirectories) {
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.LogonMethod: {1}", MethodInfo.GetCurrentMethod().Name, (int)itemVirtualDirectory.LogonMethod);
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.LogonMethod[string]: {1}", MethodInfo.GetCurrentMethod().Name, itemVirtualDirectory.LogonMethod);
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.UserName: {1}", MethodInfo.GetCurrentMethod().Name, itemVirtualDirectory.UserName);
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.Password: {1}", MethodInfo.GetCurrentMethod().Name, itemVirtualDirectory.Password);
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.Path: {1}", MethodInfo.GetCurrentMethod().Name, itemVirtualDirectory.Path);
              _logger.InfoFormat("[{0}] - itemVirtualDirectory.PhysicalPath: {1}", MethodInfo.GetCurrentMethod().Name, itemVirtualDirectory.PhysicalPath);
              _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
            }
            _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
          }
        } else {
          _logger.InfoFormat("[{0}] - ApplicationSite doesn't exists", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - Server doesn't have any ApplicationSite", MethodInfo.GetCurrentMethod().Name);
      }
      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
    }
  }
}