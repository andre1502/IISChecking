using Microsoft.Web.Administration;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace IISChecking.Module {

  public class StopApplicationSiteModule : Common {

    public StopApplicationSiteModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public bool StopApplicationPool(ApplicationPoolCollection applicationPoolCollection, string applicationPoolName) {
      bool changes = false;
      ApplicationPool applicationPool = null;

      if ((applicationPoolCollection != null) && (applicationPoolCollection.Any())) {
        _logger.InfoFormat("[{0}] - applicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, applicationPoolName);

        applicationPool = applicationPoolCollection.FirstOrDefault(p => p.Name == applicationPoolName);
        if (applicationPool != null) {
          ObjectState objectState = applicationPool.State;

          _logger.InfoFormat("[{0}] - applicationPool.Name: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.Name);
          _logger.InfoFormat("[{0}] - applicationPool.AutoStart: {1}", MethodInfo.GetCurrentMethod().Name, applicationPool.AutoStart);
          _logger.InfoFormat("[{0}] - applicationPool.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)objectState);
          _logger.InfoFormat("[{0}] - applicationPool.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, objectState);

          if (objectState != ObjectState.Stopped) {
            bool retry = true;
            int ctr = 0;

            while (retry) {
              try {
                ObjectState returnState = applicationPool.Stop();

                _logger.InfoFormat("[{0}] - returnState: {1}", MethodInfo.GetCurrentMethod().Name, (int)returnState);
                _logger.InfoFormat("[{0}] - returnState[string]: {1}", MethodInfo.GetCurrentMethod().Name, returnState);

                if (returnState == ObjectState.Stopped) {
                  _logger.InfoFormat("[{0}] - ApplicationPool is stopped", MethodInfo.GetCurrentMethod().Name);

                  applicationPool.AutoStart = false;
                  changes = true;

                  retry = false;
                } else {
                  _logger.InfoFormat("[{0}] - Failed to stop ApplicationPool", MethodInfo.GetCurrentMethod().Name);

                  retry = true;
                }
              } catch (Exception ex) {
                retry = true;

                _logger.Error(string.Format("[{0}] - Problem on stop ApplicationPool. Message: {1}, retry {2} time(s)", MethodInfo.GetCurrentMethod().Name, ex.Message, ctr), ex);
              }

              _logger.InfoFormat("[{0}] - retry status: {1}", MethodInfo.GetCurrentMethod().Name, retry);

              if (ctr > 10) {
                retry = false;
              }

              ctr++;

              if (retry) {
                Thread.Sleep(THREAD_SLEEP);
              }
            }
          }
        } else {
          _logger.InfoFormat("[{0}] - ApplicationPool doesn't exists", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - Server doesn't have any ApplicationPool", MethodInfo.GetCurrentMethod().Name);
      }

      return changes;
    }

    public void StopApplicationSite(string applicationName) {
      if (applicationName == "Default Web Site") {
        _logger.InfoFormat("[{0}] - Need to stop this ApplicationSite: {1} manually because it can conflict with ApplicationPool default", MethodInfo.GetCurrentMethod().Name, applicationName);

        return;
      }

      ServerManager serverManager = new ServerManager();
      SiteCollection siteCollection = serverManager.Sites;
      bool changeSite = false;
      bool changePool = false;

      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      if ((siteCollection != null) && (siteCollection.Any())) {
        ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
        string previousApplicationPoolName = string.Empty;
        string currentApplicationPoolName = string.Empty;

        _logger.InfoFormat("[{0}] - applicationName: {1}", MethodInfo.GetCurrentMethod().Name, applicationName);

        Site site = siteCollection.FirstOrDefault(p => p.Name == applicationName);
        if (site != null) {
          ObjectState objectState = site.State;

          _logger.InfoFormat("[{0}] - site.Id: {1}", MethodInfo.GetCurrentMethod().Name, site.Id);
          _logger.InfoFormat("[{0}] - site.Name: {1}", MethodInfo.GetCurrentMethod().Name, site.Name);
          _logger.InfoFormat("[{0}] - site.ServerAutoStart: {1}", MethodInfo.GetCurrentMethod().Name, site.ServerAutoStart);
          _logger.InfoFormat("[{0}] - site.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)objectState);
          _logger.InfoFormat("[{0}] - site.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, objectState);

          if (objectState != ObjectState.Stopped) {
            _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
            _logger.InfoFormat("[{0}] - ApplicationSite need to be stopped", MethodInfo.GetCurrentMethod().Name);

            bool retry = true;
            int ctr = 0;

            while (retry) {
              try {
                ObjectState returnState = site.Stop();

                _logger.InfoFormat("[{0}] - returnState: {1}", MethodInfo.GetCurrentMethod().Name, (int)returnState);
                _logger.InfoFormat("[{0}] - returnState[string]: {1}", MethodInfo.GetCurrentMethod().Name, returnState);

                if (returnState == ObjectState.Stopped) {
                  _logger.InfoFormat("[{0}] - ApplicationSite is stopped", MethodInfo.GetCurrentMethod().Name);

                  site.ServerAutoStart = false;
                  changeSite = true;

                  retry = false;
                } else {
                  _logger.InfoFormat("[{0}] - Failed to stop ApplicationSite", MethodInfo.GetCurrentMethod().Name);

                  retry = true;
                }
              } catch (Exception ex) {
                retry = true;

                _logger.Error(string.Format("[{0}] - Problem on stop ApplicationPool. Message: {1}, retry {2} time(s)", MethodInfo.GetCurrentMethod().Name, ex.Message, ctr), ex);
              }

              _logger.InfoFormat("[{0}] - retry status: {1}", MethodInfo.GetCurrentMethod().Name, retry);

              if (ctr > 10) {
                retry = false;
              }

              ctr++;

              if (retry) {
                Thread.Sleep(THREAD_SLEEP);
              }
            }
          }

          bool changeLocalPool = false;
          ApplicationCollection applications = site.Applications;
          foreach (Application itemApplication in site.Applications) {
            currentApplicationPoolName = itemApplication.ApplicationPoolName;

            _logger.InfoFormat("[{0}] - itemApplication.ApplicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, currentApplicationPoolName);
            _logger.InfoFormat("[{0}] - Check if ApplicationPool need to be stopped", MethodInfo.GetCurrentMethod().Name);

            if ((string.IsNullOrWhiteSpace(previousApplicationPoolName) || (currentApplicationPoolName != previousApplicationPoolName))) {
              previousApplicationPoolName = currentApplicationPoolName;
              changeLocalPool = StopApplicationPool(applicationPoolCollection, currentApplicationPoolName);

              if (changeLocalPool) {
                changePool = true;
              }
            } else {
              _logger.InfoFormat("[{0}] - ApplicationPool already been processed", MethodInfo.GetCurrentMethod().Name);
            }

            _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
          }

          if ((changeSite) || (changePool)) {
            serverManager.CommitChanges();
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