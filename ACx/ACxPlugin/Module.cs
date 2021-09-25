using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
	public abstract class Module
	{
		//[JsonIgnore]
		//protected int StartupOrder {get;set;}	//Maybe use if there are issues with ordering dependent plugins
		[JsonIgnore]
		protected PluginLogic Plugin { get; set; }

		/// <summary>
		/// Called once when the main plugin is loaded
		/// </summary>
		public virtual void Startup(PluginLogic plugin)
		{
			if (Utils.DEBUG) Utils.WriteToChat($"Starting up {this.GetType().Name}...");

			Plugin = plugin;
		}

		/// <summary>
		/// Called when the main plugin is shutting down.  Unregister from any events here and do any cleanup.
		/// </summary>
		public virtual void Shutdown()
		{
			if (Utils.DEBUG) Utils.WriteToChat($"Shutting down {this.GetType().Name}...");
		}
	}
}
