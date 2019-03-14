using log4net;
using System;

namespace IISChecking.Module {

  public class Common {
    private readonly static string zoneID = "Taipei Standard Time";
    protected readonly string SERVER_NAME = "localhost";
    protected readonly int THREAD_SLEEP = 30000;
    protected readonly ILog _logger = null;

    public Common() {
      _logger = LogManager.GetLogger(GetType());
    }

    protected static DateTime GetCurrentDateTime() {
      DateTime utcTime = DateTime.UtcNow;
      TimeZoneInfo serverZone = TimeZoneInfo.FindSystemTimeZoneById(zoneID);
      DateTime serverDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, serverZone);

      return serverDateTime;
    }
  }
}