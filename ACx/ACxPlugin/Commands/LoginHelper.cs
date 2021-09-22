using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACxPlugin
{
    /// <summary>
    /// Helper for using Mag-Filter: https://github.com/Mag-nus/Mag-Plugins/wiki/Mag%E2%80%90Filter
    /// </summary>
    public static class LoginHelper
    {
        /// <summary>
        /// Set next login by name or relative index.  TODO: Possibly let you set part of name
        /// </summary>
        /// <param name="name"></param>
        public static void SetNextLogin(string login, bool loop)
        {
            //Get a sorted list of characters
            var chars = CoreManager.Current.CharacterFilter.Characters;
            var currentName = CoreManager.Current.CharacterFilter.Name;

            var sortedChars = new List<string>();
            for (var i = 0; i < chars.Count; i++)
            {
                //Utils.WriteToChat($"{i,-15}{(chars[i].Name == currentName ? $"***{currentName}***" : chars[i].Name),-30}{chars[i].Id,-20}");
                sortedChars.Add(chars[i].Name);
            }
            sortedChars.Sort();


            //Find by substring.  Null if no character matches
            string nextLogin = sortedChars.FirstOrDefault(x => x.ToUpperInvariant().Contains(login.ToUpperInvariant()));

            //Test if logging in by relative index
            int offset;
            if(int.TryParse(login, out offset))
			{
                var currentIndex = sortedChars.IndexOf(currentName);
                if(currentIndex < 0)
				{
                    Utils.WriteToChat("First time logging in as this character.  Unable to login relative to them until relogging.");
                    Utils.Command($"/mf lnc clear");
                    return;
                }


                int nextIndex;
                if(loop)
				{
                    //Get positive mod index
                    int n = sortedChars.Count;
                    nextIndex = ((currentIndex + offset) % n + n) % n;
                }
                else
				{
                    nextIndex = currentIndex + offset;
				}
                if(nextIndex < 0 || nextIndex >= sortedChars.Count)
				{
                    Utils.WriteToChat($"Selected login index is out of bounds.  Use the LoginNextLoop / LNL command to loop around.");
                    Utils.Command($"/mf lnc clear");
                    return;
                }
                nextLogin = sortedChars[nextIndex];
                Utils.WriteToChat($"Next index:{nextIndex}");
            }

            if(nextLogin == null)
			{
                Utils.WriteToChat($"Unable to parse {login}.  Either use part of a name or the relative slot (ex: '-2'   or  '5' or 'war')");
                Utils.Command($"/mf lnc clear");
                return;
            }

            Utils.WriteToChat($"Next login: {nextLogin}");
            Utils.Command($"/mf lnc set {nextLogin}");
        }


        public static void PrintLogins()
		{
            var chars = CoreManager.Current.CharacterFilter.Characters;
            var currentName = CoreManager.Current.CharacterFilter.Name;

            Utils.WriteToChat($"{"Index",-15}{"Name",-30}{"Id",-20}");
            for (var i = 0; i < chars.Count; i++)
			{
                Utils.WriteToChat($"{i,-15}{(chars[i].Name == currentName ? $"***{currentName}***" : chars[i].Name),-30}{chars[i].Id,-20}");
            }
		}
    }
}
