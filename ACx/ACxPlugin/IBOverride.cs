using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ACxPlugin
{
	/// <summary>
	/// Overrides values set in the configuration of IBControl
	/// </summary>
	class IBOverride
	{
		private static string PartyListPath { get { return Path.Combine(Utils.AssemblyDirectory, "Party.txt"); } }
		public static void SetPartyNearby()
		{
			//var playerCoords = new CoordsObject()
			var wf = CoreManager.Current.WorldFilter;
			//var partyLeader = "SomeName";
			var partyLeader = CoreManager.Current.CharacterFilter.Name;

			var player = CoreManager.Current.WorldFilter.GetByName(partyLeader).First();

			var neighbors = wf.GetByObjectClass(ObjectClass.Player).OrderBy(o => o.Coordinates().DistanceToCoords(player.Coordinates())).ToArray();

			//setvar[charlist, listcreate[]]
			//listadd[getvar[charlist], X]

			Utils.Mexec("setvar[charlist, listcreate[]]");
			Utils.WriteToChat("Nearby players: " + neighbors.Length);
			for (int i = 0; i < neighbors.Length; i++)
			{
				Utils.WriteToChat($"Adding {neighbors[i].Name} to party.");
				Utils.Mexec($"listadd[getvar[charlist], {neighbors[i].Name}]");
			}
		}

		//Todo: implement retry logic
		public static void AddParty()
		{
			if (!File.Exists(PartyListPath))
				File.Create(PartyListPath);

			var party = File.ReadAllLines(PartyListPath).ToList();

			//Add missing characters from this account?
			var chars = CoreManager.Current.CharacterFilter.Characters;
			var additions = new StringBuilder();
			for (var i = 0; i < chars.Count; i++)
			{
				var name = chars[i].Name;
				if (!party.Contains(name))
				{
					additions.AppendLine(name);
				}
			}
			File.AppendAllText(PartyListPath, additions.ToString());
		}

		public static void SetParty()
		{
			//senddeathmessage
			//permit
			//tapercheck
			//platscarabcheck
			//manascarabcheck
			//lowpackspace
			//fellowname
			//AutoCraft
			//AutoReadContracts
			//setvar[charlist, listcreat[]]
			//listadd[getvar[charlist], X]

			if (!File.Exists(PartyListPath))
			{
				Utils.WriteToChat("Creating party list: Party.txt");
				return;
			}

			var party = File.ReadAllLines(PartyListPath);

			Utils.Mexec("setvar[charlist, listcreate[]]");
			foreach (var member in party)
			{
				Utils.WriteToChat($"Adding {member} to party.");
				Utils.Mexec($"listadd[getvar[charlist], {member}]");
			}
		}
	}
}
