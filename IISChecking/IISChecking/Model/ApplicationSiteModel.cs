using System.Collections.Generic;

namespace IISChecking.Model {

  public class PhysicalPathModel {
    public string Source { get; set; }
    public string SourcePackage { get; set; }
    public string Destination { get; set; }
  }

  public class BindingInformationModel {
    public string Protocol { get; set; }
    public string IPAddress { get; set; }
    public string Port { get; set; }
    public string HostName { get; set; }
  }

  public class ApplicationSiteModel {
    public string ApplicationName { get; set; }
    public PhysicalPathModel PhysicalPath { get; set; }
    public List<BindingInformationModel> BindingInformation { get; set; }
    public string Description { get; set; }
  }
}