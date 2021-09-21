using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ACxPlugin
{
    public class IBControlManager
    {
        public IBControlManager(bool autoCraft)
        {
            AutoCraft = autoCraft;
        }

        /// <summary>
        /// Substring for the leader of the party.  Creating a party of nearby players is based on the first match of this.
        /// Could use a Regex instead.
        /// </summary>
        public string PartyLeader { get; set; } = "Silla.*";
        /// <summary>
        /// Max number of players that will be set to be in the party.
        /// </summary>
        public int PartySize { get; set; } = 4;

        #region IBControl Variables
        /// <summary>
        /// Variables set for party members
        /// </summary>
        private string[] metaVars = new string[] { "charone", "chartwo", "charthree", "charfour", "charfive", "charsix", "charseven", "chareight", "charnine" };

        public string Profile { get; set; } = "IB_Control_config.txt";
        public int Tab { get; set; } = 2;
        public string ChatCommand { get; set; } = "/a";
        //After setting, setvar[ch,false]
        public bool View { get; set; } = false;
        //senddeathmessage
        public bool SendDeathMessage { get; set; } = true;
        //permit
        public bool Permit { get; set; } = true;
        //platscarabcheck
        public bool PlatScarabCheck { get; set; } = true;
        //manascarabcheck
        public bool ManaScarabCheck { get; set; } = true;
        //lowpackspace
        public bool LowPackSpace { get; set; } = true;
        //fellowname
        public string FellowName { get; set; } = "MyFellow";
        //createfellow
        public bool CreateFellow { get; set; } = true;
        //AutoCraft
        public bool AutoCraft { get; set; } = true;
        //AutoReadContracts
        public bool AutoReadContracts { get; set; } = true;
        #endregion

        //public bool Permit { get; set; } = true;

        //Override nav with leader?


        public void SetPartyNearby()
        {
            var player = CoreManager.Current.WorldFilter.GetByNameSubstring(PartyLeader).First();
            //var player = CoreManager.Current.WorldFilter.GetByObjectClass(ObjectClass.Player).First();

            var foo = CoreManager.Current.WorldFilter.GetByObjectClass(ObjectClass.Player).OrderBy(o => o.Coordinates().DistanceToCoords(player.Coordinates())).ToArray();

            int memberCount = 0;


            Utils.WriteToChat("Num players: " + foo.Length);
            for (var i = 0; i < metaVars.Length; i++)
            {
                //Utils.WriteToChat($"Adding player {i+1}");
                //If there are nearby players, add them to the party

                if (i < foo.Length)
                {
                    var p = foo[i];
                    DecalProxy.DispatchChatToBoxWithPluginIntercept($"/vt mexec setvar[{metaVars[i]},{Regex.Escape(p.Name)}]");
                    continue;
                }

                DecalProxy.DispatchChatToBoxWithPluginIntercept($"/vt mexec setvar[{metaVars[i]},MISSING]");
                //Otherwise add MISSING but set the variable for IBControl

            }
            //foreach (var p in foo)
            //{
            //    Utils.WriteToChat("Name: " + p.Name);
            //}
            ////var playerCoords = CoreManager.Current.WorldFilter.
            ////CoreManager.Current.WorldFilter.GetByObjectClass(ObjectClass.Player).OrderBy(o => o.Coordinates().DistanceToCoords());


            //DecalProxy.DispatchChatToBoxWithPluginIntercept($"/vt dumpmetavars");
        }

        public void SetParty()
        {
            if (CoreManager.Current.CharacterFilter.Name.Length > 5)
            {
                var suffix = CoreManager.Current.CharacterFilter.Name.Substring(5);

                var metaVars = new string[] { "charone", "chartwo", "charthree", "charfour", "charfive", "charsix", "charseven", "chareight", "charnine" };
                for (var i = 0; i < metaVars.Length; i++)
                {
                    var charName = "Sill" + (char)('a' + i) + suffix;
                    DecalProxy.DispatchChatToBoxWithPluginIntercept($"/vt mexec setvar[{metaVars[i]},{charName}]");
                }
            }
        }



    }
}
