using System;
using System.IO;
using System.Reflection;
using Gumshoe.Core.Trackers;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.WebHost.Endpoints;

namespace Gumshoe.Core
{
	public class AppHost : AppHostBase
	{
		public readonly MonoTorrentTrackerBase MonoTorrentTracker;

		public AppHost(string trackerBroadcastDomain, int trackerPort, string torrentsDirectoryPath, TimeSpan announceInterval, bool allowUnregisteredTorrents = false) : base(Assembly.GetExecutingAssembly().FullName, typeof(AppHost).Assembly)
		{
			MonoTorrentTracker = new FolderWatcherTracker (trackerBroadcastDomain, trackerPort, torrentsDirectoryPath, announceInterval, allowUnregisteredTorrents);
			MonoTorrentTracker.Start ();
		}

		public override void Configure(Funq.Container container)
		{
			// Register container items.
			container.Register<ICacheClient> (new MemoryCacheClient ());
			container.Register (MonoTorrentTracker);

			// Set default page.
			SetConfig (new EndpointHostConfig
			{ 
				DefaultRedirectPath = "/status"
			});

			// Setup razor.
			TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration ();
			templateConfig.Resolver = new DelegateTemplateResolver (name =>
			{
				string resourcePath = string.Format ("Gumshoe.Core.Views.{0}", name);
				if (!resourcePath.EndsWith (".cshtml"))
					resourcePath += ".cshtml";

				var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resourcePath);
				using (StreamReader reader = new StreamReader(stream))
					return reader.ReadToEnd ();
			});
			Razor.SetTemplateService (new TemplateService (templateConfig));
		}
	}
}