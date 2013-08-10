using System;
using System.Configuration;
using System.IO;
using System.Web;
using Gumshoe.Core;

namespace Gumshoe
{
	public class Application : HttpApplication
	{
		protected void Application_Start()
		{
			// Get settings from config.
			var trackerBroadcastDomain = ConfigurationManager.AppSettings ["TrackerBroadcastDomain"];
			var trackerPort = int.Parse (ConfigurationManager.AppSettings ["TrackerPort"]);
			var torrentsDirectory = ConfigurationManager.AppSettings ["TorrentsDirectory"];
			var announceIntervalInSeconds = TimeSpan.FromSeconds (double.Parse (ConfigurationManager.AppSettings ["AnnounceIntervalInSeconds"]));
			var allowUnregisteredTorrents = bool.Parse (ConfigurationManager.AppSettings ["AllowUnregisteredTorrents"]);

			// Create torrents directory.
			var torrentsDirectoryPath = HttpContext.Current.Server.MapPath (torrentsDirectory);
			if (!Directory.Exists (torrentsDirectoryPath))
				Directory.CreateDirectory (torrentsDirectoryPath);

			// Start app.
			new AppHost (trackerBroadcastDomain, trackerPort, torrentsDirectoryPath, announceIntervalInSeconds, allowUnregisteredTorrents).Init ();
		}
	}
}