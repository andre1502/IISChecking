using Microsoft.Web.Administration;
using System.Reflection;

namespace IISChecking.Module {

  public class ServerManagerModule : Common {

    public ServerManagerModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public void ApplicationPoolList() {
      ServerManager serverManager = new ServerManager();
      ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
      ApplicationPoolProcessModel applicationPoolProcessModel = null;

      _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - Begin ApplicationPoolCollection", MethodInfo.GetCurrentMethod().Name);
      foreach (ApplicationPool itemApplicationPool in applicationPoolCollection) {
        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
        _logger.InfoFormat("[{0}] - itemApplicationPool.Name: {1}", MethodInfo.GetCurrentMethod().Name, itemApplicationPool.Name);
        _logger.InfoFormat("[{0}] - itemApplicationPool.AutoStart: {1}", MethodInfo.GetCurrentMethod().Name, itemApplicationPool.AutoStart);
        _logger.InfoFormat("[{0}] - itemApplicationPool.Enable32BitAppOnWin64: {1}", MethodInfo.GetCurrentMethod().Name, itemApplicationPool.Enable32BitAppOnWin64);
        _logger.InfoFormat("[{0}] - itemApplicationPool.ManagedRuntimeVersion: {1}", MethodInfo.GetCurrentMethod().Name, itemApplicationPool.ManagedRuntimeVersion);
        _logger.InfoFormat("[{0}] - itemApplicationPool.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)itemApplicationPool.State);
        _logger.InfoFormat("[{0}] - itemApplicationPool.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, itemApplicationPool.State);

        applicationPoolProcessModel = itemApplicationPool.ProcessModel;
        _logger.InfoFormat("[{0}] - applicationPoolProcessModel.IdentityType: {1}", MethodInfo.GetCurrentMethod().Name, (int)applicationPoolProcessModel.IdentityType);
        _logger.InfoFormat("[{0}] - applicationPoolProcessModel.IdentityType[string]: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.IdentityType);
        _logger.InfoFormat("[{0}] - applicationPoolProcessModel.UserName: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.UserName);
        _logger.InfoFormat("[{0}] - applicationPoolProcessModel.Password: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolProcessModel.Password);
        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      }
      _logger.InfoFormat("[{0}] - End ApplicationPoolCollection", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
    }

    public void ApplicationSiteList() {
      ServerManager serverManager = new ServerManager();
      SiteCollection siteCollection = serverManager.Sites;

      BindingCollection bindings = null;
      ApplicationCollection applications = null;

      _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - Begin SiteCollection", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      foreach (Site itemSite in siteCollection) {
        _logger.InfoFormat("[{0}] - itemSite.Id: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.Id);
        _logger.InfoFormat("[{0}] - itemSite.Name: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.Name);
        _logger.InfoFormat("[{0}] - itemSite.ServerAutoStart: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.ServerAutoStart);
        _logger.InfoFormat("[{0}] - itemSite.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)itemSite.State);
        _logger.InfoFormat("[{0}] - itemSite.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.State);

        bindings = itemSite.Bindings;
        _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
        foreach (Binding binding in bindings) {
          _logger.InfoFormat("[{0}] - binding.Protocol: {1}", MethodInfo.GetCurrentMethod().Name, binding.Protocol);
          _logger.InfoFormat("[{0}] - binding.BindingInformation: {1}", MethodInfo.GetCurrentMethod().Name, binding.BindingInformation);
          _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
        }
        _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);

        applications = itemSite.Applications;
        foreach (Application itemApplication in itemSite.Applications) {
          _logger.InfoFormat("[{0}] - itemApplication.ApplicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, itemApplication.ApplicationPoolName);
          _logger.InfoFormat("[{0}] - itemApplication.EnabledProtocols: {1}", MethodInfo.GetCurrentMethod().Name, itemApplication.EnabledProtocols);
          _logger.InfoFormat("[{0}] - itemApplication.Path: {1}", MethodInfo.GetCurrentMethod().Name, itemApplication.Path);

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
        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      }
      _logger.InfoFormat("[{0}] - End SiteCollection", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
    }
  }
}