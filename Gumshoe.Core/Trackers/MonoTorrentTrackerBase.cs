using System.Collections.Generic;
using MonoTorrent.Tracker;

namespace Gumshoe.Core.Trackers
{
	public abstract class MonoTorrentTrackerBase : Tracker
	{
		public IEnumerable<string> Endpoints{ get; protected set; }
		public abstract void Start ();
	}
}