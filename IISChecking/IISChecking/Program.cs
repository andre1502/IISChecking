using IISChecking.Module;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace IISChecking {

  public class Program : Common {

    private static void HowTo() {
      string s = @"
                IISChecking.exe /c checkapppool                       : start IIS ApplicationPool if ApplicationSite is running but ApplicationPool is not running
                IISChecking.exe /c createsite [ApplicationSiteConfig] : create IIS ApplicationSite based on config file (content must CASE-SENSITIVE)
                IISChecking.exe /c stopsite [ApplicationSiteName]     : stop IIS ApplicationSite based on site name (must CASE-SENSITIVE)
                IISChecking.exe /c detailsite [ApplicationSiteName]   : show IIS ApplicationSite properties based on site name (must CASE-SENSITIVE)";
      Console.WriteLine("command to proceed: {0}{1}", Environment.NewLine, s);
      Thread.Sleep(10000);
    }

    private static void Main(string[] args) {
      if (args.Length > 1 && (args[0].ToLower() == "-c" || args[0].ToLower() == "/c")) {
        ILog _logger = LogManager.GetLogger(typeof(Program));
        DateTime current = GetCurrentDateTime();

        switch (args[1].ToLower()) {
          case "checkapppool":
            GlobalContext.Properties["appname"] = "IISCheckingApplicationPool";
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\log4net.config"));

            _logger.InfoFormat("[{0}] - IIS MachineName: {1}", MethodInfo.GetCurrentMethod().Name, Environment.MachineName);

            new CheckingApplicationPoolModule().CheckApplicationSite();
            break;

          case "createsite":
            if (args.Length > 2) {
              GlobalContext.Properties["appname"] = "IISCreateApplicationSite";
              XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\log4net.config"));

              _logger.InfoFormat("[{0}] - IIS MachineName: {1}", MethodInfo.GetCurrentMethod().Name, Environment.MachineName);

              new CreateApplicationSiteModule().CreateApplicationSite(args[2]);
            } else {
              goto default;
            }
            break;

          case "stopsite":
            if (args.Length > 2) {
              GlobalContext.Properties["appname"] = "IISStopApplicationSite";
              XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\log4net.config"));

              _logger.InfoFormat("[{0}] - IIS MachineName: {1}", MethodInfo.GetCurrentMethod().Name, Environment.MachineName);

              new StopApplicationSiteModule().StopApplicationSite(args[2]);
            } else {
              goto default;
            }
            break;

          case "detailsite":
            if (args.Length > 2) {
              GlobalContext.Properties["appname"] = "IISApplicationSiteDetail";
              XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\log4net.config"));

              _logger.InfoFormat("[{0}] - IIS MachineName: {1}", MethodInfo.GetCurrentMethod().Name, Environment.MachineName);

              new ApplicationSiteDetailModule().GetApplicationSiteDetail(args[2]);
            } else {
              goto default;
            }
            break;

          default:
            Console.WriteLine("Fail: no function called.");
            HowTo();
            break;
        }
      } else {
        HowTo();
      }
    }
  }
}