using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using MonoTorrent.Common;
using MonoTorrent.TorrentWatcher;
using MonoTorrent.Tracker;
using MonoTorrent.Tracker.Listeners;

namespace Gumshoe.Core.Trackers
{
	public class FolderWatcherTracker : MonoTorrentTrackerBase
	{
		public readonly TorrentFolderWatcher Watcher;
		public readonly ConcurrentDictionary<string, InfoHashTrackable> Mappings;
		public readonly HttpListener HttpListener;
		public readonly UdpListener UdpListener;

		public FolderWatcherTracker (string trackerBroadcastDomain, int trackerPort, string torrentsDirectoryPath, TimeSpan announceInterval, bool allowUnregisteredTorrents)
		{
			// Set defaults.
			AnnounceInterval = announceInterval;
			AllowUnregisteredTorrents = allowUnregisteredTorrents;

			// Make listeners.
			var httpEndpoint = string.Format ("http://{0}:{1}/announce", trackerBroadcastDomain, trackerPort).ToLower();
			var udpEndpoint = string.Format ("udp://{0}:{1}/announce", trackerBroadcastDomain, trackerPort).ToLower();
			HttpListener = new HttpListener (httpEndpoint + "/"); // Not sure why it needs trailing slash??!!
			UdpListener = new UdpListener(trackerPort);

			// Register listeners with the trackers.
			RegisterListener (HttpListener);
			RegisterListener (UdpListener);

			// Expose the endpoints.
			Endpoints = new [] { httpEndpoint, udpEndpoint };

			// Make mappings.
			Mappings = new ConcurrentDictionary<string, InfoHashTrackable> ();

			// Make watcher.
			Watcher = new TorrentFolderWatcher (torrentsDirectoryPath, "*.torrent");
			Watcher.TorrentFound += (sender, e) =>
			{
				try
				{
					// Wait for file to finish copying.
					System.Threading.Thread.Sleep (500);

					// Make InfoHashTrackable from torrent.
					var torrent = Torrent.Load (e.TorrentPath);
					var trackable = new InfoHashTrackable (torrent);

					// Add to tracker.
					lock (this)
						Add (trackable);

					// Save to mappings.
					Mappings[e.TorrentPath] = trackable;

					// Log.
					Console.WriteLine("Added {0}", e.TorrentPath);
				}
				catch (Exception exception)
				{
					Debug.WriteLine ("Error loading torrent from disk: {0}", exception.Message);
					Debug.WriteLine ("Stacktrace: {0}", exception.ToString ());
				}
			};
			Watcher.TorrentLost += (sender, e) =>
			{
				try
				{
					// Get from mappings.
					var trackable = Mappings[e.TorrentPath];

					// Remove from tracker.
					lock(this)
						Remove(trackable);

					// Remove from mappings.
					Mappings.TryRemove(e.TorrentPath, out trackable);

					// Log.
					Console.WriteLine("Removed {0}", e.TorrentPath);
				}
				catch(Exception exception)
				{
					Debug.WriteLine ("Error uploading torrent from disk: {0}", exception.Message);
					Debug.WriteLine ("Stacktrace: {0}", exception.ToString ());
				}
			};
		}

		public override void Start()
		{
			HttpListener.Start();
			UdpListener.Start ();
			Watcher.Start();
			Watcher.ForceScan();
		}
	}
}