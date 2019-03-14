using IISChecking.Model;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace IISChecking.Module {

  public class CheckingApplicationPoolModule : Common {

    public CheckingApplicationPoolModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public void CheckSpecificApplicationPool(string applicationName) {
      string path = string.Format("IIS://{0}/W3SVC/AppPools/{1}", SERVER_NAME, applicationName);
      DirectoryEntry appPool = new DirectoryEntry(path);

      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      _logger.InfoFormat("[{0}] - appPool: {1}", MethodInfo.GetCurrentMethod().Name, JsonConvert.SerializeObject(appPool, Formatting.Indented));
      _logger.InfoFormat("[{0}] - appPool.Name: {1}", MethodInfo.GetCurrentMethod().Name, appPool.Name);
      if (appPool.Properties.Contains("AppPoolState")) {
        int appPoolState = (int)appPool.InvokeGet("AppPoolState");

        EnumState enumState = EnumState.Unknown;
        EnumState enumAppPoolState = enumState;

        bool enumParsed = Enum.TryParse(appPoolState.ToString(), true, out enumState);

        if (enumParsed) {
          enumAppPoolState = enumState;
        }

        _logger.InfoFormat("[{0}] - appPoolState: {1}, enumAppPoolState: {2}", MethodInfo.GetCurrentMethod().Name, appPoolState, enumAppPoolState.ToString());

        if (enumAppPoolState != EnumState.Started) {
          bool retry = true;
          int ctr = 0;

          while (retry) {
            try {
              _logger.InfoFormat("[{0}] - ApplicationSite is running but AppPoolState is not running, try to invoke ApplicationPool", MethodInfo.GetCurrentMethod().Name);

              appPool.Invoke("Start", null);

              _logger.InfoFormat("[{0}] - AppPoolState is running", MethodInfo.GetCurrentMethod().Name);

              retry = false;
            } catch (Exception ex) {
              retry = true;

              _logger.Error(string.Format("[{0}] - Problem on invoke ApplicationPool. Message: {1}, retry {1} time(s)", MethodInfo.GetCurrentMethod().Name, ex.Message, ctr), ex);
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
        } else {
          _logger.InfoFormat("[{0}] - AppPoolState is running", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - AppPoolState property doesn't exists. Path: {1}", MethodInfo.GetCurrentMethod().Name, path);
      }
      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
    }

    public void CheckApplicationSiteOldMethod() {
      string path = string.Format("IIS://{0}/W3SVC", SERVER_NAME);
      DirectoryEntry appSite = new DirectoryEntry(path);

      foreach (DirectoryEntry itemAppSite in appSite.Children) {
        _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
        _logger.InfoFormat("[{0}] - itemAppSite: {1}", MethodInfo.GetCurrentMethod().Name, JsonConvert.SerializeObject(itemAppSite, Formatting.Indented));
        _logger.InfoFormat("[{0}] - itemAppSite.Name: {1}", MethodInfo.GetCurrentMethod().Name, itemAppSite.Name);

        string serverComment = null;
        string appPoolName = null;
        EnumState enumState = EnumState.Unknown;
        EnumState enumServerState = enumState;

        if (itemAppSite.Properties.Contains("ServerComment")) {
          serverComment = itemAppSite.InvokeGet("ServerComment").ToString();
          appPoolName = (serverComment == "Default Web Site") ? "DefaultAppPool" : serverComment;
          _logger.InfoFormat("[{0}] - ServerComment: {1}", MethodInfo.GetCurrentMethod().Name, serverComment);
        } else {
          _logger.InfoFormat("[{0}] - ServerComment property doesn't exists", MethodInfo.GetCurrentMethod().Name);
        }

        if (itemAppSite.Properties.Contains("ServerState")) {
          int serverState = (int)itemAppSite.InvokeGet("ServerState");

          bool enumParsed = Enum.TryParse(serverState.ToString(), true, out enumState);

          if (enumParsed) {
            enumServerState = enumState;
          }

          _logger.InfoFormat("[{0}] - ServerState: {1}, enumServerState: {2}", MethodInfo.GetCurrentMethod().Name, serverState, enumServerState.ToString());

          if (enumServerState == EnumState.Started) {
            try {
              _logger.InfoFormat("[{0}] - ApplicationSite is running, need to check AppPoolState status", MethodInfo.GetCurrentMethod().Name);

              CheckSpecificApplicationPool(appPoolName);
            } catch (Exception ex) {
              _logger.Error(string.Format("[{0}] - Problem on check AppPoolState status. serverComment: {1}, appPoolName: {2}, Message: {3}", MethodInfo.GetCurrentMethod().Name, serverComment,
                appPoolName, ex.Message), ex);
            }
          } else {
            _logger.InfoFormat("[{0}] - ApplicationSite is not running, no need to check AppPoolState status", MethodInfo.GetCurrentMethod().Name);
          }
        } else {
          _logger.InfoFormat("[{0}] - ServerState property doesn't exists", MethodInfo.GetCurrentMethod().Name);
        }

        _logger.InfoFormat("[{0}] - =======================================================================================", MethodInfo.GetCurrentMethod().Name);
      }
    }

    public bool CheckSpecificApplicationPool(ApplicationPoolCollection applicationPoolCollection, string applicationPoolName) {
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

          if (objectState != ObjectState.Started) {
            bool retry = true;
            int ctr = 0;

            while (retry) {
              try {
                _logger.InfoFormat("[{0}] - ApplicationSite is started but ApplicationPool is not started, try to start ApplicationPool", MethodInfo.GetCurrentMethod().Name);

                ObjectState returnState = applicationPool.Start();

                _logger.InfoFormat("[{0}] - returnState: {1}", MethodInfo.GetCurrentMethod().Name, (int)returnState);
                _logger.InfoFormat("[{0}] - returnState[string]: {1}", MethodInfo.GetCurrentMethod().Name, returnState);

                if (returnState == ObjectState.Started) {
                  _logger.InfoFormat("[{0}] - ApplicationPool is started", MethodInfo.GetCurrentMethod().Name);

                  applicationPool.AutoStart = true;
                  changes = true;

                  retry = false;
                } else {
                  _logger.InfoFormat("[{0}] - ApplicationPool failed to start", MethodInfo.GetCurrentMethod().Name);

                  retry = true;
                }
              } catch (Exception ex) {
                retry = true;

                _logger.Error(string.Format("[{0}] - Problem on start ApplicationPool. Message: {1}, retry {1} time(s)", MethodInfo.GetCurrentMethod().Name, ex.Message, ctr), ex);
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

    public void CheckApplicationSite() {
      ServerManager serverManager = new ServerManager();
      SiteCollection siteCollection = serverManager.Sites;
      bool changeSite = false;
      bool changePool = false;

      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      if ((siteCollection != null) && (siteCollection.Any())) {
        ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
        string previousApplicationPoolName = string.Empty;
        string currentApplicationPoolName = string.Empty;
        ObjectState objectState = ObjectState.Unknown;

        List<Site> siteList = siteCollection.Where(p => p.State == ObjectState.Started).ToList();
        if ((siteList != null) && (siteList.Any())) {
          _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
          foreach (Site itemSite in siteList) {
            objectState = itemSite.State;

            _logger.InfoFormat("[{0}] - itemSite.Id: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.Id);
            _logger.InfoFormat("[{0}] - itemSite.Name: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.Name);
            _logger.InfoFormat("[{0}] - itemSite.ServerAutoStart: {1}", MethodInfo.GetCurrentMethod().Name, itemSite.ServerAutoStart);
            _logger.InfoFormat("[{0}] - itemSite.State: {1}", MethodInfo.GetCurrentMethod().Name, (int)objectState);
            _logger.InfoFormat("[{0}] - itemSite.State[string]: {1}", MethodInfo.GetCurrentMethod().Name, objectState);

            if (objectState == ObjectState.Started) {
              _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
              _logger.InfoFormat("[{0}] - ApplicationSite is started, also need to check ApplicationPool state status", MethodInfo.GetCurrentMethod().Name);

              if (!itemSite.ServerAutoStart) {
                itemSite.ServerAutoStart = true;
                changeSite = true;
              }

              bool changeLocalPool = false;
              ApplicationCollection applications = itemSite.Applications;
              foreach (Application itemApplication in itemSite.Applications) {
                currentApplicationPoolName = itemApplication.ApplicationPoolName;

                _logger.InfoFormat("[{0}] - itemApplication.ApplicationPoolName: {1}", MethodInfo.GetCurrentMethod().Name, currentApplicationPoolName);

                if ((string.IsNullOrWhiteSpace(previousApplicationPoolName) || (currentApplicationPoolName != previousApplicationPoolName))) {
                  previousApplicationPoolName = currentApplicationPoolName;
                  changeLocalPool = CheckSpecificApplicationPool(applicationPoolCollection, currentApplicationPoolName);

                  if (changeLocalPool) {
                    changePool = true;
                  }
                } else {
                  _logger.InfoFormat("[{0}] - ApplicationPool already been processed", MethodInfo.GetCurrentMethod().Name);
                }

                _logger.InfoFormat("[{0}] - ===========", MethodInfo.GetCurrentMethod().Name);
              }
            } else {
              _logger.InfoFormat("[{0}] - ApplicationSite is not started, no need to check ApplicationPool state status", MethodInfo.GetCurrentMethod().Name);
            }
            _logger.InfoFormat("[{0}] - ======================", MethodInfo.GetCurrentMethod().Name);
          }

          if ((changeSite) || (changePool)) {
            serverManager.CommitChanges();
          }
        } else {
          _logger.InfoFormat("[{0}] - No started ApplicationSite at moment", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - Server doesn't have any ApplicationSite", MethodInfo.GetCurrentMethod().Name);
      }
      _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
    }
  }
}