using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace ACxPlugin
{
	public enum Macros
	{
		BlastNearest,
		BlastSelected
	}

	class MacroManager
	{
		private static Timer timer = new Timer();

		private static double MAX_DISTANCE = 20;
		private static int runCount = 0;
		private static Macros Macro;
		public static void StartMacro(Macros macro, int interval = 200, int distance = 20)
		{
			MAX_DISTANCE = distance;

			//Toggle macro if running
			if (timer.Enabled)
			{
				Utils.WriteToChat("Stopping macro.");
				timer.Enabled = false;
				return;
			}

			//Otherwise create the macro
			MacroManager.timer = new Timer()
			{
				Interval = interval,
				AutoReset = true,
				Enabled = true
			};
			runCount = 0;
			Macro = macro;

			timer.Elapsed += MacroTick;
		}

		private static void MacroTick(object sender, ElapsedEventArgs e)
		{
			try
			{
				//Check if finished
				if (runCount > 100000000)
				{
					Utils.WriteToChat($"Stopping macro.");
					timer.Enabled = false;
				}
				//Otherwise do something
				else
				{
					//Utils.WriteToChat($"{runCount++}");
					switch (Macro)
					{
						case Macros.BlastNearest:
							BlastNearest();
							break;
						case Macros.BlastSelected:
							BlastSelected();
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}

		private static void BlastNearest()
		{
			//Use wings on nearest if under 40?
			//var caster = "Wings of Rakhil";
			//Utils.Mexec($@"chatbox[iif[coordinatedistancewithz[getplayercoordinates[], wobjectgetphysicscoordinates[wobjectfindnearestmonster[]]] < {MAX_DISTANCE}, `/mt usep {caster} on ` +wobjectgetname[wobjectfindnearestmonster[]], ``]]");
			//Utils.Command($"/mt use {caster} on Stomper");
			//Utils.Command($"/mt click 1222 926");

			//try
			//{
			//	//Just for testing, TODO: remove
			//	var core = CoreManager.Current.Actions;
			//         var player = CoreManager.Current.CharacterFilter;
			//         var world = CoreManager.Current.WorldFilter;

			////Get the nearest monster
			////var neighbors = world.GetByObjectClass(ObjectClass.Monster).OrderBy(o => o.Coordinates().DistanceToCoords(world[player.Id].Coordinates())).ToArray();
			////foreach (var n in neighbors)
			////    Utils.WriteToChat($"{n.Name}\t{n.Coordinates().DistanceToCoords(world[player.Id].Coordinates())}");

			//	var target = world.GetByObjectClass(ObjectClass.Monster).OrderBy(o => o.Coordinates().DistanceToCoords(world[player.Id].Coordinates())).FirstOrDefault();

			//	//         if(target == null)
			//	//{
			//	//             Utils.WriteToChat("No monster found.  Halting macro.");
			//	//             timer.Enabled = false;
			//	//             return;
			//	//}
			//	if (target.Coordinates().DistanceToCoords(world[player.Id].Coordinates()) > MAX_DISTANCE)
			//	{
			//		//Utils.WriteToChat("Nearest monster out of range.  Halting macro.");
			//		//timer.Enabled = false;
			//		return;
			//	}

			//	//Select and blast nearest.  
			//	//TODO: check if it actually requires being selected
			//	core.CurrentSelection = target.Id;
			//	BlastTarget(target.Id);
			//}
			//catch (Exception ex)
			//{
			//	Utils.LogError(ex);
			//}
		}

		private static void BlastSelected()
		{
			try
			{
				//Blast if something is selected
				if (CoreManager.Current.Actions.CurrentSelection != 0)
					BlastTarget(CoreManager.Current.Actions.CurrentSelection);
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}

		private static void BlastTarget(int target)
		{
			try
			{
				//Just for testing, TODO: remove
				var core = CoreManager.Current.Actions;
				var player = CoreManager.Current.CharacterFilter;
				var world = CoreManager.Current.WorldFilter;

				//Check combat mode correct
				if (core.CombatMode != CombatState.Magic)
					return;

				if (!CheckTargetMonster(target))
					return;

				//Get the players wand.
				//TODO: make sure it is equipped
				int? wandId = GetWandId();

				if (wandId == null)
					return;

				//Use wand on monster to cast spell
				//Double check combat mode correct...
				if (core.CombatMode == CombatState.Magic)
					core.UseItem(wandId.Value, 1, target);
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
		}

		private static int? GetWandId()
		{
			WorldObject wand = null;
			try
			{
				var core = CoreManager.Current.Actions;
				var player = CoreManager.Current.CharacterFilter;
				var world = CoreManager.Current.WorldFilter;

				var items = world.GetByContainer(player.Id);
				foreach (var item in items)
				{
					if (item.ObjectClass == ObjectClass.WandStaffOrb && Regex.IsMatch(item.Name, "Wings|Heart of"))
					{
						wand = item;
						//Utils.WriteToChat($"{item.Name}\t{item.ObjectClass}\t{item.Type}");
					}
				}

				if (wand == null)
				{
					Utils.WriteToChat("Unable to find appropriate wand. Halting macro.");
					timer.Enabled = false;
					return null;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return wand.Id;
		}

		private static bool CheckTargetMonster(int target)
		{
			try
			{
				if (CoreManager.Current.WorldFilter[target].ObjectClass == ObjectClass.Monster)
					return true;
			}
			catch (Exception ex)
			{
				Utils.LogError(ex);
			}
			return false;
		}
	}
}
