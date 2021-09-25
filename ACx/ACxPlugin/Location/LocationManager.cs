using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACxPlugin.Location
{
	public class LocationManager : Module
	{
		private const string VTANK_DIR = @"C:\Games\VirindiPlugins\VirindiTank";

		private void CharacterFilter_ChangePortalMode(object sender, Decal.Adapter.Wrappers.ChangePortalModeEventArgs e)
		{
			if (e.Type == PortalEventType.ExitPortal)
			{
				LoadLocation();
			}
		}

		public static void LoadLocation(bool createMissingNav = false, bool createMissingTxt = false)
		{
			if (TryFindByLocation(out string nav, "nav"))
				Utils.Command($"/vt nav load {nav}");
			else if (createMissingNav)
			{
				nav = $"0x{CoreManager.Current.Actions.Landcell:X}";
				Utils.Command($"/vt nav load {nav}");
			}

			if (TryFindByLocation(out string load, "txt"))
				Utils.Command($"/loadfile {VTANK_DIR}\\{load}.txt");
			else if (createMissingTxt)
			{
				try
				{
					load = $"{VTANK_DIR}\\0x{CoreManager.Current.Actions.Landcell:X}.txt";
					File.Create(load);
				}
				catch (Exception ex)
				{
					Utils.LogError(ex);
				}
			}
		}

		public static bool TryFindByLocation(out string fileName, string extension)
		{
			fileName = null;

			//Require VTank
			if (!Directory.Exists(VTANK_DIR))
				return false;

			//Use first 4 hex of the landblock for compatibility with acpedia
			var lbHex = $"{CoreManager.Current.Actions.Landcell:X}".Substring(0, 4);
			var locMatches = Directory.GetFiles(VTANK_DIR, $"*{lbHex}*.{extension}");

			if (locMatches.Length > 0)
			{
				fileName = Path.GetFileNameWithoutExtension(locMatches[0]);
				Utils.WriteToChat($"Loading {extension} {fileName} from {locMatches.Length} matches of {lbHex}");

				return true;
			}

			return false;
		}

		private void CharacterFilter_LoginComplete(object sender, EventArgs e)
		{
			LoadLocation();
		}

		public override void Startup(PluginLogic plugin)
		{
			base.Startup(plugin);

			// landblock detection?
			if (Plugin.Profile.LoadLocations)
			{
				CoreManager.Current.CharacterFilter.ChangePortalMode += CharacterFilter_ChangePortalMode;

				//Load on login
				CoreManager.Current.CharacterFilter.LoginComplete += CharacterFilter_LoginComplete;
			}
		}

		public override void Shutdown()
		{
			base.Shutdown();

			//Todo: decide if this should be moved to the event / handled with some internal record 
			if (Plugin.Profile.LoadLocations)
			{
				CoreManager.Current.CharacterFilter.ChangePortalMode -= CharacterFilter_ChangePortalMode;
				CoreManager.Current.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
			}
		}
	}
}
