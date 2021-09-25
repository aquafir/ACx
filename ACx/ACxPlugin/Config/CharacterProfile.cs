using ACxPlugin.AutoXP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace ACxPlugin
{
	public class CharacterProfile : Module
	{
		private const int DEFAULT_LOGIN_COMMAND_DELAY = 5000;
		private Timer loginCommandTimer;
		private FileSystemWatcher profileWatcher;

		[JsonIgnore]
		public string Path { get; set; }
		[JsonIgnore]
		public string Directory { get { return System.IO.Path.GetDirectoryName(Path); } }

		[JsonProperty("Load Locations")]
		public bool LoadLocations { get; set; } = true;
		[JsonProperty("Login Load Commands")]
		public string[] LoginLoad { get; set; } = { };
		[JsonProperty("Login Commands")]
		public string[] LoginCommands { get; set; } = { };
		[JsonProperty("Policy")]
		public ExperiencePolicy ExpPolicy { get; set; }

		private void DelayedLoginCommands(object sender, ElapsedEventArgs e)
		{
			try
			{
				//Execute each login command
				foreach (var loginCommand in LoginCommands)
				{
					Utils.WriteToChat($"Executing command: {loginCommand}");
					Utils.Command(loginCommand);
				}

				//Execute each /loadfile command if the file exists
				foreach (var loadFile in LoginLoad)
				{
					var path = System.IO.Path.IsPathRooted(loadFile) ? loadFile : System.IO.Path.Combine(Directory, loadFile);

					if (File.Exists(path))
					{
						Utils.WriteToChat($"Running: /loadfile {path}");
						DecalProxy.DispatchChatToBoxWithPluginIntercept($"/loadfile {path}");
					}
					else
					{
						Utils.WriteToChat($"Missing file at: /loadfile {path}");
					}
				}
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}

		public static CharacterProfile Default
		{
			get
			{
				return new CharacterProfile()
				{
					LoadLocations = true,
					ExpPolicy = ExperiencePolicy.Default
				};
			}
		}

		public override void Startup(PluginLogic plugin)
		{
			base.Startup(plugin);

			//Create a timer to execute login commands after a delay
			loginCommandTimer = new Timer { Interval = DEFAULT_LOGIN_COMMAND_DELAY, AutoReset = false, Enabled = true };
			loginCommandTimer.Elapsed += DelayedLoginCommands;

			//Watch Profile and request reload when changed
			profileWatcher = new FileSystemWatcher()
			{
				Path = System.IO.Path.GetDirectoryName(Path),
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite,
			};
			profileWatcher.Changed += Plugin.RequestReload;

			Utils.WriteToChat($"Selected Profile: {plugin.Config.SelectedProfile}");
		}

		public override void Shutdown()
		{
			base.Shutdown();
			loginCommandTimer.Enabled = false;
			profileWatcher.Changed -= Plugin.RequestReload;
			profileWatcher?.Dispose();
		}
	}
}
