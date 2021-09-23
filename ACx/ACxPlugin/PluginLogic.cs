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
		private FileSystemWatcher profileWatcher;
		private FileSystemWatcher configurationWatcher;

		[XmlIgnoreAttribute]
		private List<Module> modules;

		private void CharacterFilter_LoginComplete(object sender, EventArgs e)
		{
			try
			{
				Initialize();
			}
			catch (Exception ex) { Utils.LogError(ex); }
		}

		private void CharacterFilter_Logoff(object sender, Decal.Adapter.Wrappers.LogoffEventArgs e)
		{
			try
			{
				Shutdown();
			}
			catch (Exception ex) { Utils.LogError(ex); }
		}


		/// <summary>
		/// Called when plugin reloaded while being logged in or after logging in for the first time.
		/// </summary>
		private void Initialize()
		{
			try
			{
				//Config/Profiles
				//Utils.WriteToChat("Loading configuration...");
				Config = Configuration.LoadOrCreateConfiguration(this);
				//Utils.WriteToChat("Finding character profile...");
				Profile = Config.LoadOrCreateProfile();

				//Utils.WriteToChat($"Watching configuration for changes: {Path.GetDirectoryName(config.Path)}\t{Path.GetFileName(config.Path)})");
				configurationWatcher = new FileSystemWatcher()
				{
					Path = Path.GetDirectoryName(Config.Path),
					Filter = Path.GetFileName(Config.Path),
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.LastWrite,
					IncludeSubdirectories = true
				};
				configurationWatcher.Changed += RequestReload;
				//Utils.WriteToChat($"Watching profile for changes: {Path.GetDirectoryName(profile.Path)}\t{Path.GetFileName(profile.Path)}");
				profileWatcher = new FileSystemWatcher()
				{
					Path = Path.GetDirectoryName(Profile.Path),
					Filter = Path.GetFileName(Profile.Path),
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.LastWrite,
					IncludeSubdirectories = true
				};
				profileWatcher.Changed += RequestReload;

				//First time only activities
				//if (IsFirstLogin)
				//{
				//	IsFirstLogin = false;
				//	//Utils.WriteToChat("Running login commands...");
				//	Profile.ScheduleLoginCommands();
				//}

				//Tries to load settings if a file watcher indicates they've changed. Stop on success
				reloadTimer = new Timer() { AutoReset = true, Enabled = false, Interval = TIME_BETWEEN_RELOAD_ATTEMPTS };
				reloadTimer.Elapsed += TryReload;

				//Modules
				modules = new List<Module>();
				modules.Add(new CommandManager(this));
				modules.Add(new SpellTabManager(this));
				modules.Add(new ExperienceManager(this));
				foreach (var m in modules)
					m.Startup();

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
				Initialize();
				Utils.WriteToChat("Reloaded settings successfully.");
			}
			catch (Exception ex)
			{
				//File most likely in use
				Utils.LogError(ex);
			}
		}

		private void RequestReload(object sender, FileSystemEventArgs e)
		{
			//Utils.WriteToChat($"Requesting reload: {e.FullPath} \t {e.ChangeType}");
			reloadTimer.Enabled = true;
		}

		#region Startup / Shutdown
		// Called once when the plugin is loaded.  Loads modules.
		public void Startup(NetServiceHost host, CoreManager core, string pluginAssemblyDirectory, string accountName, string characterName, string serverName)
		{
			
			Utils.AssemblyDirectory = pluginAssemblyDirectory;

			//If the player is logged in and the plugin is reloaded reinitialize
			if (Utils.IsLoggedIn())
			{
				var timeLapsedLastLoad = DateTime.Now - lastLoad;
				if (timeLapsedLastLoad.TotalMilliseconds > TIME_BETWEEN_PLUGIN_RELOAD)
				{

					//Utils.WriteToChat($"Reloaded after {timeLapsedLastLoad.TotalSeconds} seconds since last load");
					lastLoad = DateTime.Now;

					Initialize();
				}
			}

			//Otherwise the plugin handles things on login
			CoreManager.Current.CharacterFilter.LoginComplete += this.CharacterFilter_LoginComplete;
			CoreManager.Current.CharacterFilter.Logoff += CharacterFilter_Logoff;

			var actions = CoreManager.Current.Actions;
			CoreManager.Current.CharacterFilter.ChangePortalMode += (send, e) =>
			{
				if (e.Type == PortalEventType.ExitPortal)
				{
					Utils.WriteToChat($"{actions.Landcell}\t{actions.LocationX},{actions.LocationY},{actions.LocationZ}");
					}
			};
		}

		/// <summary>
		/// Called when the plugin is shutting down.  Unregister from any events here and do any cleanup.
		/// </summary>
		public void Shutdown()
		{
			try
			{
				foreach (var m in modules)
					m.Shutdown();

				//Stop anything currently going on
				//Utils.WriteToChat("Shutting down command manager...");

				//Utils.WriteToChat("Removing timers...");
				if (reloadTimer != null)
					reloadTimer.Enabled = false;
				if (configurationWatcher != null)
					configurationWatcher.Dispose();
				if (profileWatcher != null)
					profileWatcher.Dispose();

				//Remove events
				//Utils.WriteToChat("Removing config watcher...");
				if (configurationWatcher != null)
					configurationWatcher.Changed -= RequestReload;
				//Utils.WriteToChat("Removing profile watcher...");
				if (profileWatcher != null)
					profileWatcher.Changed -= RequestReload;
				reloadTimer.Elapsed -= TryReload; 

				//Utils.WriteToChat("Removing login event...");
				CoreManager.Current.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
				//Utils.WriteToChat("Removing logoff event...");
				CoreManager.Current.CharacterFilter.Logoff -= CharacterFilter_Logoff;
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}
		#endregion
	}
}
