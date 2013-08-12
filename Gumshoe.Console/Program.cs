using System;
using System.IO;
using System.Threading;
using Gumshoe.Core;
using ManyConsole;

namespace Gumshoe.Console
{
	public class Program : ConsoleCommand
	{
		public int SitePort;
		public string TrackerBroadcastDomain;
		public int TrackerPort;
		public string TorrentsDirectory;
		public int AnnounceIntervalInSeconds = 60;
		public bool AllowUnregisteredTorrents;

		protected AppHost AppHost;

		public Program()
		{
			IsCommand("run", "Run the application.");
			HasRequiredOption ("p|port=", "Port that website is hosted on.", o => SitePort = int.Parse (o));
			HasRequiredOption ("td|trackerBroadcastDomain=", "Domain that will be broadcast to peers to connect to.", o => TrackerBroadcastDomain = o);
			HasRequiredOption ("tp|trackerPort=", "Port tracker is hosted on.", o => TrackerPort = int.Parse (o));
			HasRequiredOption ("t|torrentsDirectory=", "Directory holding the trackers torrents.", o => TorrentsDirectory = Path.GetFullPath (o));
			HasOption ("AnnounceInterval=", "Interval in seconds that tracker will callback to tracker.", o => AnnounceIntervalInSeconds = int.Parse (o));
			HasOption ("AllowUnregisteredTorrents", "Allows torrents not in the TorrentsDirectory to be tracked.", o => AllowUnregisteredTorrents = o != null);
			SkipsCommandSummaryBeforeRunning();
		}

		public override int Run (string[] remainingArguments)
		{
			// Make AppHost.
			AppHost = new AppHost (TrackerBroadcastDomain, TrackerPort, TorrentsDirectory, TimeSpan.FromSeconds(AnnounceIntervalInSeconds), AllowUnregisteredTorrents);
			AppHost.Init ();
			AppHost.Start (string.Format("http://*:{0}/", SitePort));

			// Never die.
			Thread.Sleep (Timeout.Infinite);
			return 0;
		}

		public static void Main (string[] args)
		{
			ConsoleCommandDispatcher.DispatchCommand (ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs (typeof(Program)), args, System.Console.Out);
		}
	}
}