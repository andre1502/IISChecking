using IISChecking.Model;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using RoboSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace IISChecking.Module {

  public class CreateApplicationSiteModule : Common {
    private string applicationSiteFilePath = null;

    public CreateApplicationSiteModule() {
      _logger.InfoFormat("[{0}] - StartDateTime: {1}", MethodInfo.GetCurrentMethod().Name, GetCurrentDateTime());
    }

    public void CopyApplicationSite(string source, string destination) {
      RoboCommand roboCommand = new RoboCommand();

      //roboCommand.OnFileProcessed += OnFileProcessed;
      roboCommand.OnCommandCompleted += OnCommandCompleted;
      roboCommand.CopyOptions.Source = source;
      roboCommand.CopyOptions.Destination = destination;
      roboCommand.CopyOptions.Mirror = true;
      roboCommand.CopyOptions.CopySubdirectories = true;
      roboCommand.LoggingOptions.NoFileSizes = true;
      roboCommand.LoggingOptions.NoFileList = true;
      roboCommand.LoggingOptions.NoDirectoryList = true;
      roboCommand.LoggingOptions.NoProgress = true;

      var cmd = roboCommand.Start();
      cmd.Wait();

      ConfigureApplicationSite();
    }

    private void OnFileProcessed(object sender, FileProcessedEventArgs e) {
      //_logger.InfoFormat("[{0}] - Copy ApplicationSite InProgress: {1}, {2}, {3}.", MethodInfo.GetCurrentMethod().Name, e.ProcessedFile.FileClass, e.ProcessedFile.Name, e.ProcessedFile.Size);
    }

    private void OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e) {
      _logger.InfoFormat("[{0}] - Copy ApplicationSite Complete!", MethodInfo.GetCurrentMethod().Name);
    }

    public ApplicationSiteModel ApplicationSiteParseConfig(string applicationSiteFile) {
      ApplicationSiteModel applicationSite = null;

      try {
        if (File.Exists(applicationSiteFile)) {
          string content = File.ReadAllText(applicationSiteFile);
          applicationSite = JsonConvert.DeserializeObject<ApplicationSiteModel>(content);
        }
      } catch (Exception ex) {
        _logger.Error(string.Format("[{0}] - Problem on parse ApplicationSite config. Message: {1}", MethodInfo.GetCurrentMethod().Name, ex.Message), ex);
      }

      return applicationSite;
    }

    public void ConfigureApplicationSite(ApplicationSiteModel applicationSite = null) {
      if (applicationSite == null) {
        applicationSite = ApplicationSiteParseConfig(applicationSiteFilePath);
      }

      if (applicationSite != null) {
        try {
          if (!Directory.Exists(applicationSite.PhysicalPath.Destination)) {
            _logger.InfoFormat("[{0}] - ApplicationSite PhysicalPath Destination doesn't exists, failed to copy, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
            _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);

            return;
          }

          string applicationName = applicationSite.ApplicationName;

          ServerManager serverManager = new ServerManager();
          SiteCollection siteCollection = serverManager.Sites;
          ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;

          ApplicationPool applicationPool = applicationPoolCollection.Add(applicationName);

          if (applicationPool != null) {
            applicationPool.AutoStart = true;
            applicationPool.Enable32BitAppOnWin64 = false;
            applicationPool.ManagedRuntimeVersion = "v4.0";
            applicationPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.ApplicationPoolIdentity;
          }

          string physicalPath = applicationSite.PhysicalPath.Destination;

          Site site = siteCollection.Add(applicationName, physicalPath, 80);
          site.ServerAutoStart = true;
          site.ApplicationDefaults.ApplicationPoolName = applicationPool.Name;
          site.Applications[0].ApplicationPoolName = applicationPool.Name;

          string protocol = "http";
          string ipAddress = "*";
          string port = "80";
          string hostName = "*";
          string bindingInformation = string.Format("{0}:{1}:{2}", ipAddress, port, hostName);

          site.Bindings.Clear();
          foreach (BindingInformationModel itemBindingInformation in applicationSite.BindingInformation) {
            protocol = itemBindingInformation.Protocol;
            ipAddress = itemBindingInformation.IPAddress;
            port = itemBindingInformation.Port;
            hostName = itemBindingInformation.HostName;
            bindingInformation = string.Format("{0}:{1}:{2}", ipAddress, port, hostName);

            site.Bindings.Add(bindingInformation, protocol);
          }

          serverManager.CommitChanges();

          _logger.InfoFormat("[{0}] - ApplicationSite created", MethodInfo.GetCurrentMethod().Name);
          _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
        } catch (Exception ex) {
          _logger.Error(string.Format("[{0}] - Problem on parse ApplicationSite config. Message: {1}", MethodInfo.GetCurrentMethod().Name, ex.Message), ex);
          _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
        }
      } else {
        _logger.InfoFormat("[{0}] - ApplicationSite file config not found, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
        _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
      }
    }

    public void CreateApplicationSite(string applicationSiteFile) {
      applicationSiteFilePath = applicationSiteFile;
      ApplicationSiteModel applicationSite = ApplicationSiteParseConfig(applicationSiteFilePath);

      if (applicationSite != null) {
        try {
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

            if ((site != null) || (applicationPool != null)) {
              _logger.InfoFormat("[{0}] - ApplicationSite already exists: {1} please use different name", MethodInfo.GetCurrentMethod().Name, applicationName);

              return;
            }
          }

          if ((!Directory.Exists(applicationSite.PhysicalPath.Source)) && (!File.Exists(applicationSite.PhysicalPath.SourcePackage)) && (!Directory.Exists(applicationSite.PhysicalPath.Destination))) {
            _logger.InfoFormat("[{0}] - ApplicationSite PhysicalPath doesn't exists, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
            _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);

            return;
          } else if (Directory.Exists(applicationSite.PhysicalPath.Destination)) {
            _logger.InfoFormat("[{0}] - ApplicationSite PhysicalPath Destination already exists, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
            _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);

            return;
          } else if ((Directory.Exists(applicationSite.PhysicalPath.Source)) && (!Directory.Exists(applicationSite.PhysicalPath.Destination))) {
            try {
              Directory.CreateDirectory(applicationSite.PhysicalPath.Destination);

              if (Directory.Exists(applicationSite.PhysicalPath.Destination)) {
                CopyApplicationSite(applicationSite.PhysicalPath.Source, applicationSite.PhysicalPath.Destination);
              } else {
                _logger.InfoFormat("[{0}] - Failed to create directory {1}, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name, applicationSite.PhysicalPath.Destination);
                _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
              }
            } catch (Exception ex) {
              _logger.Error(string.Format("[{0}] - Problem on create Directory: {1}. Message: {2}", MethodInfo.GetCurrentMethod().Name, applicationSite.PhysicalPath.Destination, ex.Message), ex);
              _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
            }
          } else if ((File.Exists(applicationSite.PhysicalPath.SourcePackage)) && (!Directory.Exists(applicationSite.PhysicalPath.Destination))) {
            try {
              ZipFile.ExtractToDirectory(applicationSite.PhysicalPath.SourcePackage, applicationSite.PhysicalPath.Destination);

              if (Directory.Exists(applicationSite.PhysicalPath.Destination)) {
                _logger.InfoFormat("[{0}] - Package extracted into {1}, continue to configure ApplicationSite", MethodInfo.GetCurrentMethod().Name, applicationSite.PhysicalPath.Destination);

                ConfigureApplicationSite(applicationSite);
              } else {
                _logger.InfoFormat("[{0}] - Failed to extract package into {1}, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name, applicationSite.PhysicalPath.Destination);
                _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
              }
            } catch (Exception ex) {
              _logger.Error(string.Format("[{0}] - Problem on extract package: {1} to destination: {2}. Message: {3}", MethodInfo.GetCurrentMethod().Name, applicationSite.PhysicalPath.SourcePackage,
                applicationSite.PhysicalPath.Destination, ex.Message), ex);
              _logger.InfoFormat("[{0}] - =============================================", MethodInfo.GetCurrentMethod().Name);
            }
          }
        } catch (Exception ex) {
          _logger.Error(string.Format("[{0}] - Message: {1}", MethodInfo.GetCurrentMethod().Name, ex.Message), ex);
        }
      } else {
        _logger.InfoFormat("[{0}] - ApplicationSite file config not found, can't create ApplicationSite", MethodInfo.GetCurrentMethod().Name);
      }
    }
  }
}