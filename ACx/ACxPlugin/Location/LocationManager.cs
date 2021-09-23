using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACxPlugin.Location
{
	public class LocationManager : Module
	{
		private void CharacterFilter_ChangePortalMode(object sender, Decal.Adapter.Wrappers.ChangePortalModeEventArgs e)
		{
			var actions = CoreManager.Current.Actions;
			if (e.Type == PortalEventType.ExitPortal)
			{
				Utils.WriteToChat($"{actions.Landcell}\t{actions.LocationX},{actions.LocationY},{actions.LocationZ}");
			}

		}

		public override void Startup(PluginLogic plugin)
		{
			base.Startup(plugin);
			CoreManager.Current.CharacterFilter.ChangePortalMode += CharacterFilter_ChangePortalMode;
		}

		public override void Shutdown()
		{
			base.Shutdown();
			CoreManager.Current.CharacterFilter.ChangePortalMode -= CharacterFilter_ChangePortalMode;
		}
	}
}
