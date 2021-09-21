using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace ACxPlugin
{
	public class CharacterProfile
	{
		private const int DEFAULT_LOGIN_COMMAND_DELAY = 5000;
		private Timer loginCommandTimer;

		[JsonIgnore]
		public string Path { get; set; }
		[JsonIgnore]
		public string Directory { get { return System.IO.Path.GetDirectoryName(Path); } }

		[JsonProperty("Login Load Commands")]
		public string[] LoginLoad { get; set; } = { };
		[JsonProperty("Login Commands")]
		public string[] LoginCommands { get; set; } = { };
		[JsonProperty("Policy")]
		public ExperiencePolicy ExpPolicy { get; set; }

		public CharacterProfile()
		{
			loginCommandTimer = new Timer { Interval = DEFAULT_LOGIN_COMMAND_DELAY, AutoReset = false, Enabled = true };
			loginCommandTimer.Elapsed += DelayedLoginCommands;
		}

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
					ExpPolicy = ExperiencePolicy.Default
				};
			}
		}
	}
}
