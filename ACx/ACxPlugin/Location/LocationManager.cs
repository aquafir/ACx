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
			//var actions = CoreManager.Current.Actions;
			if (e.Type == PortalEventType.ExitPortal)
			{
				if (TryFindLocationNav(out string nav))
					Utils.Command($"/vt nav load {nav}");

				//If you wanted to do this with metas...
				//if(TryFindLocationMeta(out string meta))
				//	Utils.Command($"/vt meta load {meta}");
			}

		}

		public static bool TryFindLocationNav(out string fileName)
		{
			fileName = null;
			var lbHex = $"{CoreManager.Current.Actions.Landcell:X}";
			var locMatches = Directory.GetFiles(VTANK_DIR, $"*{lbHex}*.nav");

			if(locMatches.Length > 0)
			{
				fileName = Path.GetFileNameWithoutExtension(locMatches[0]);
				return true;
			}

			return false;
		}

		public static bool TryFindLocationMeta(out string fileName)
		{
			fileName = null;
			var lbHex = $"{CoreManager.Current.Actions.Landcell:X}";
			var locMatches = Directory.GetFiles(VTANK_DIR, $"*{lbHex}*.met");

			if (locMatches.Length > 0)
			{
				fileName = Path.GetFileNameWithoutExtension(locMatches[0]);
				return true;
			}

			return false;
		}

		public static void CreateOrLoadNav()
		{
			//Use nav file if it can be found, otherwise load the hex of the landblock
			if (!TryFindLocationNav(out string nav))
				nav = $"0x{CoreManager.Current.Actions.Landcell:X}";

				Utils.Command($"/vt nav load {nav}");
		}

		public override void Startup(PluginLogic plugin)
		{
			base.Startup(plugin);

			//Add landblock detection?
			if(Plugin.Profile.LoadLocations)
				CoreManager.Current.CharacterFilter.ChangePortalMode += CharacterFilter_ChangePortalMode;
		}

		public override void Shutdown()
		{
			base.Shutdown();

			//Todo: decide if this should be moved to the event / handled with some internal record 
			if (Plugin.Profile.LoadLocations)
				CoreManager.Current.CharacterFilter.ChangePortalMode -= CharacterFilter_ChangePortalMode;
		}
	}
}
