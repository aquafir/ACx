﻿using Decal.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ACxPlugin
{
	/// <summary>
	/// System wide settings and a list of rules for finding character-specific profiles
	/// </summary>
	public class Configuration
	{
		public static string DefaultConfigurationPath = @"Config.json";
		public static string DefaultProfileFolder = @"Profiles";

		[JsonIgnore]
		public PluginLogic Plugin { get; set; }
		[JsonIgnore]
		public string Path { get { return System.IO.Path.Combine(Utils.AssemblyDirectory, DefaultConfigurationPath); } }
		//[JsonIgnore]
		//public string ProfilePath { get; set; }
		public string Trigger { get; set; } = Utils.DEFAULT_TRIGGER;
		public int Interval { get; set; } = 150;

		[JsonProperty("Profiles")]
		public List<ProfileSelector> Profiles { get; set; } = new List<ProfileSelector>();

		/// <summary>
		/// Try to load configuration from plugin directory.  Create if it doesn't exist.
		/// </summary>
		/// <param name="pluginDir"></param>
		/// <returns></returns>
		public static Configuration LoadOrCreateConfiguration(PluginLogic plugin)
		{
			Configuration config = new Configuration();

			//Utils.WriteToChat("Combined config path: " + Path.Combine(Utils.AssemblyDirectory, DefaultConfigurationPath));
			var configPath = System.IO.Path.Combine(Utils.AssemblyDirectory, DefaultConfigurationPath);
			try
			{
				//Create config if needed
				if (!File.Exists(configPath))
				{
					//Utils.WriteToChat($"No config found.  Creating default at: {ConfigurationPath}");
					config = new Configuration()
					{
						Plugin = plugin,    //Set plugin reference
						Profiles = new List<ProfileSelector>()
					{
						new ProfileSelector() { Account = ".*", Server = ".*", CharName = ".*", Priority = 1, FriendlyName = "Default Profile", Path = System.IO.Path.Combine(DefaultProfileFolder, "Default.json") },
						new ProfileSelector() { CharName = ".*War.*", Priority = 2, FriendlyName = "War Mage", Path = System.IO.Path.Combine(DefaultProfileFolder, "War.json") },
						new ProfileSelector() { CharName = ".*Void.*", Priority = 2, FriendlyName = "Void Mage", Path = System.IO.Path.Combine(DefaultProfileFolder, "Void.json") },
						new ProfileSelector() { CharName = ".*TH.*", Priority = 2, FriendlyName = "Two-Handed", Path = System.IO.Path.Combine(DefaultProfileFolder, "TH.json") },
						new ProfileSelector() { CharName = ".*Bow.*", Priority = 2, FriendlyName = "Missile", Path = System.IO.Path.Combine(DefaultProfileFolder, "Missile.json") },
						new ProfileSelector() { CharName = ".*Mule.*", Priority = 2, FriendlyName = "Stronk Mule", Path = System.IO.Path.Combine(DefaultProfileFolder, "Mule.json") }
					},
						Interval = 150,
						Trigger = Utils.DEFAULT_TRIGGER
					};

					try
					{
						string json = JsonConvert.SerializeObject(config, Formatting.Indented);
						File.WriteAllText(configPath, json);
					}
					//Unable to save
					catch (Exception e)
					{
						Utils.LogError(e);
					}

					return config;
				}
				else
				{
					//Utils.WriteToChat($"Loading config from: {ConfigurationPath}");
					try
					{
						config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configPath));
						config.Plugin = plugin;
					}
					//Unable to load
					catch (Exception e)
					{
						//Utils.WriteToChat($"Failed to load config from: {ConfigurationPath}");
						Utils.LogError(e);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.WriteToChat(ex.Message);
			}

			return config;
		}

		public CharacterProfile LoadOrCreateProfile()
		{
			var characterProfile = CharacterProfile.Default;

			//Utils.WriteToChat($"{CoreManager.Current.CharacterFilter.Name}\t{CoreManager.Current.CharacterFilter.AccountName}\t{CoreManager.Current.CharacterFilter.Server}");
			//Utils.WriteToChat("Policies: Profile,Char,Account,Server,Priority");
			//foreach (var p in Profiles.OrderByDescending(t => t.Priority))
			//{
			//    Utils.WriteToChat($"{p.FriendlyName,-20}{p.CharName,-20}");
			//    Utils.WriteToChat($"{p.Account,-15}{p.Server,-15}{p.Priority,-8}");
			//}

			foreach (var p in Profiles.OrderByDescending(t => t.Priority))
			{
				var namePattern = p.CharName ?? ".*";
				var accountPattern = p.Account ?? ".*";
				var serverPattern = p.Server ?? ".*";

				//var name = Regex.Escape(CoreManager.Current.CharacterFilter.Name);
				//var account = Regex.Escape(CoreManager.Current.CharacterFilter.AccountName);
				//var server = Regex.Escape(CoreManager.Current.CharacterFilter.Server);
				var name = CoreManager.Current.CharacterFilter.Name;
				var account = CoreManager.Current.CharacterFilter.AccountName;
				var server = CoreManager.Current.CharacterFilter.Server;


				//Utils.WriteToChat($"Looking at policy: {p.FriendlyName}:{name}\t{namePattern}\t\t{account}\t{accountPattern}\t\t{server}\t{serverPattern}");
				//Check for match.  Missing/null interpreted as always a match?
				if (!Regex.IsMatch(name, namePattern, RegexOptions.IgnoreCase))
					continue;
				if (!Regex.IsMatch(account, accountPattern, RegexOptions.IgnoreCase))
					continue;
				if (!Regex.IsMatch(server, serverPattern, RegexOptions.IgnoreCase))
					continue;

				//If the path is rooted use it, otherwise a path relative to the plugin directory
				var fullPath = System.IO.Path.IsPathRooted(p.Path) ? System.IO.Path.GetFullPath(p.Path) : System.IO.Path.Combine(Utils.AssemblyDirectory, p.Path);
				//Utils.WriteToChat("Path: " + fullPath);
				if (File.Exists(fullPath))
				{
					//Utils.WriteToChat($"Matched profile {p.FriendlyName} at: {fullPath}");
					try
					{
						characterProfile = JsonConvert.DeserializeObject<CharacterProfile>(File.ReadAllText(fullPath));
						characterProfile.Path = fullPath;
						return characterProfile;
					}
					catch (Exception e)
					{
						throw e;
						//Utils.WriteToChat($"Unable to load character profile at: {fullPath}");
						//Util.LogError(e);
					}
				}
				//Profile doesn't exist.  
				else
				{
					//Utils.WriteToChat($"Matched profile {p.FriendlyName} but missing at: {fullPath}");
					try
					{
						//Create default profile dir if it doesn't exist
						if (fullPath.Contains(DefaultProfileFolder) && !Directory.Exists(DefaultProfileFolder))
							Directory.CreateDirectory(DefaultProfileFolder);

						characterProfile = CharacterProfile.Default;
						characterProfile.Path = fullPath;
						var json = JsonConvert.SerializeObject(characterProfile, Formatting.Indented);
						File.WriteAllText(fullPath, json);
						Utils.WriteToChat($"Default profile created at: {fullPath}");
					}
					catch (Exception e)
					{
						Utils.WriteToChat($"Unable to write default profile: {fullPath}");
						Utils.LogError(e);
					}

					Utils.WriteToChat($"Loaded character " +
						$"profile {p.FriendlyName}");

					return characterProfile;
				}
			}

			return characterProfile;
		}
	}
}
