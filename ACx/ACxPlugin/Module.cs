using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
	public abstract class Module
	{
		protected PluginLogic Plugin { get; set; }

		public Module(PluginLogic plugin)
		{
			Plugin = plugin;
		}

		/// <summary>
		/// Called once when the main plugin is loaded
		/// </summary>
		public virtual void Startup() {
			if (Utils.DEBUG)
				Utils.WriteToChat($"Starting up {this.GetType().Name}...");
		}

		/// <summary>
		/// Called when the main plugin is shutting down.  Unregister from any events here and do any cleanup.
		/// </summary>
		public virtual void Shutdown() {
			if (Utils.DEBUG)
				Utils.WriteToChat($"Shutting down {this.GetType().Name}...");
		}
	}
}
