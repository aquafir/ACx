using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Newtonsoft.Json;
using System.Xml.Serialization;
using VirindiViewService;
using VirindiViewService.Controls;
using VirindiViewService.XMLParsers;
using ACxPlugin.Location;

namespace ACxPlugin
{
	/// <summary>
	/// This is where all your plugin logic should go.  Public fields are automatically serialized and deserialized
	/// between plugin sessions in this class.  Check out the main Plugin class to see how the serialization works.
	/// </summary>
	public class PluginLogic
	{
		private const int TIME_BETWEEN_PLUGIN_RELOAD = 5000;
		private const int TIME_BETWEEN_RELOAD_ATTEMPTS = 1000;
		[XmlIgnoreAttribute]
		//[XmlIgnoreAttribute]
		public bool IsFirstLogin { get; set; } = true;

		public DateTime lastLoad = DateTime.Now;

		[XmlIgnoreAttribute]
		public Configuration Config { get; set; }
		[XmlIgnoreAttribute]
		public CharacterProfile Profile { get; set; }

		//Todo: Possibly move reload logic to Config/Profiles and have them request the PluginLogic reloads
		private Timer reloadTimer;


		[XmlIgnoreAttribute]
		private List<Module> modules;

		private void CharacterFilter_LoginComplete(object sender, EventArgs e)
		{
			try
			{
				Initialize();
				CoreManager.Current.CharacterFilter.LoginComplete -= this.CharacterFilter_LoginComplete;
			}
			catch (Exception ex) { Utils.LogError(ex); }
		}

		//private void CharacterFilter_Logoff(object sender, Decal.Adapter.Wrappers.LogoffEventArgs e)
		//{
		//	try
		//	{
		//		Shutdown();
		//	}
		//	catch (Exception ex) { Utils.LogError(ex); }
		//}


		/// <summary>
		/// Called when plugin reloaded while being logged in or after logging in for the first time.
		/// </summary>
		private void Initialize()
		{
			try
			{
				//Config/profile
				Config = Configuration.LoadOrCreateConfiguration();
				Profile = Config.LoadOrCreateProfile();

				//Set up and start modules
				modules = new List<Module>();
				modules.Add(Config);
				modules.Add(Profile);
				modules.Add(new CommandManager());
				modules.Add(new SpellTabManager());
				modules.Add(new ExperienceManager());
				modules.Add(new LocationManager());
				
				//Start modules in order added
				for (var i = 0; i < modules.Count; i++)
					modules[i].Startup(this);

				//Try to reload settings if Config/Profile has changed. Stop on success
				reloadTimer = new Timer() { AutoReset = true, Enabled = false, Interval = TIME_BETWEEN_RELOAD_ATTEMPTS };
				reloadTimer.Elapsed += TryReload;

				Utils.WriteToChat("Successfully loaded!");
			}
			catch (Exception ex)
			{
				Utils.WriteToChat(ex.Message);
				Utils.LogError(ex);
			}
		}

		//TODO: Eventually split things into modules and held under config and reload only what's needed
		private void TryReload(object sender, ElapsedEventArgs e)
		{
			try
			{
				Shutdown();
			}
			catch (Exception ex)
			{
				//File most likely in use
				Utils.LogError(ex);
			}
			try
			{
				Initialize();
				Utils.WriteToChat("Reloaded settings successfully.");
			}
			catch (Exception ex)
			{
				//File most likely in use
				Utils.LogError(ex);
			}
		}

		public void RequestReload(object sender, FileSystemEventArgs e)
		{
			//Utils.WriteToChat($"Requesting reload: {e.FullPath} \t {e.ChangeType}");
			reloadTimer.Enabled = true;
		}

		#region Startup / Shutdown
		// Called once when the plugin is loaded.
		public void Startup(NetServiceHost host, CoreManager core, string pluginAssemblyDirectory, string accountName, string characterName, string serverName)
		{
			Utils.AssemblyDirectory = pluginAssemblyDirectory;

			//Initialize();

			//Gate the multiple reloads the hot-reload feature was doing..?
			var timeLapsedLastLoad = DateTime.Now - lastLoad;
			if (timeLapsedLastLoad.TotalMilliseconds > TIME_BETWEEN_PLUGIN_RELOAD)
			{
				Utils.WriteToChat($"Reloaded {timeLapsedLastLoad.TotalSeconds} seconds after last load");
				lastLoad = DateTime.Now;

				//If the player is logged in and the plugin is reloaded reinitialize
				if (Utils.IsLoggedIn())
					Initialize();
				//Otherwise initialize when logged in
				else
					CoreManager.Current.CharacterFilter.LoginComplete += this.CharacterFilter_LoginComplete;
			}


			//}

			//Otherwise the plugin handles things on login
			//CoreManager.Current.CharacterFilter.LoginComplete += this.CharacterFilter_LoginComplete;
			//CoreManager.Current.CharacterFilter.Logoff += CharacterFilter_Logoff;			
		}

		/// <summary>
		/// Called when the plugin is shutting down.  Unregister from any events here and do any cleanup.
		/// </summary>
		public void Shutdown()
		{
			try
			{
				//Shutdown modules in reverse order?
				for (var i = modules.Count - 1; i >= 0; i--)
					modules[i].Shutdown();
				modules.Clear();

				//Utils.WriteToChat("Removing timers...");
				if (reloadTimer != null)
					reloadTimer.Enabled = false;
				reloadTimer.Elapsed -= TryReload;

				//Remove login events
				//CoreManager.Current.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
				//CoreManager.Current.CharacterFilter.Logoff -= CharacterFilter_Logoff;
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}
		#endregion
	}
}
