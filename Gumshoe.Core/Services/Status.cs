using System;
using Gumshoe.Core.Trackers;
using RazorEngine;
using RazorEngine.Templating;
using ServiceStack.ServiceHost;

namespace Gumshoe.Core.Services
{
	[Route("/status")]
	public class StatusRequest : IReturn<string>
	{
	}

	public class Status : ServiceStack.ServiceInterface.Service
	{
		private const string statusCacheKey = "status";
		protected readonly MonoTorrentTrackerBase Tracker;

		public Status (MonoTorrentTrackerBase tracker)
		{
			Tracker = tracker;
		}

		public object Any(StatusRequest request)
		{
			return RequestContext.ToOptimizedResultUsingCache (Cache, statusCacheKey, new TimeSpan (0, 0, 5), () => 
			{
				return Razor.Resolve("status.cshtml", new { Tracker = Tracker }).Run(new ExecuteContext());
			});
		}
	}
}