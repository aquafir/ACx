using Decal.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace ACxPlugin
{
    public class SpellTabManager : Module
    {
        //Might be wrong, but in testing only up to tab 7 was supported
        private static int MAX_SPELL_TAB = 7;
        private static string DEFAULT_SPELL_PATH = "Spells.json";

        private PluginLogic plugin;
        private Timer timer;
        private List<Spell> spellsToLoad = new List<Spell>();
        private int spellIndex { get; set; } = 0;
        private bool spellBarsEmpty = false;

        private void SpendExperienceTick(object sender, ElapsedEventArgs e)
        {
            //Check for cleared spell bars
            if (!spellBarsEmpty)
            {
                if (!IsSpellBarsEmpty())
                {
                    Utils.WriteToChat("Waiting for spellbar to be emptied.");
                    return;
                }
                spellBarsEmpty = true;
            }

            //If there's no more spells stop
            if (spellsToLoad.Count <= spellIndex)
            {
                //Plan finished.
                Utils.WriteToChat($"Finished loading {spellIndex} spells.");
                timer.Enabled = false;
            }
            else
            {
                var spell = spellsToLoad[spellIndex];
                CoreManager.Current.Actions.SpellTabAdd(spell.Tab, spell.Index, spell.SpellID);
                spellIndex++;
            }
        }

        private bool IsSpellBarsEmpty()
        {
            for (var i = 0; i < MAX_SPELL_TAB; i++)
            {
                if (CoreManager.Current.CharacterFilter.SpellBar(i).Count > 0)
                    return false;
            }
            return true;
        }

        public void SaveSpells()
        {
            SaveAllSpells(DEFAULT_SPELL_PATH);
        }
        public void SaveAllSpells(string path)
        {
            path = Path.Combine(Utils.AssemblyDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            SpellTab[] tabs = new SpellTab[MAX_SPELL_TAB];
            for (var i = 0; i < tabs.Length; i++)
                tabs[i] = GetSpellTab(i);

            string json = JsonConvert.SerializeObject(tabs);

            try
            {
                File.WriteAllText(path, json);
                Utils.WriteToChat($"Saved spells to {path}");
            }
            catch (Exception e)
            {
                Utils.WriteToChat($"Unable to save spells to: {Path.GetFullPath(path)}");
                Utils.LogError(e);
            }
        }

        public SpellTab GetSpellTab(int tabNumber)
        {
            if (tabNumber < 0 || tabNumber >= MAX_SPELL_TAB)
                throw new Exception("Tried to get an illegal spell tab: " + tabNumber);

            return new SpellTab() { Spells = CoreManager.Current.CharacterFilter.SpellBar(tabNumber).ToArray() };
        }

        public void ClearAllSpells()
        {
            //Utils.WriteToChat("Clearing all spell bars");
            for (var i = 0; i < MAX_SPELL_TAB; i++)
            {
                ClearTab(i);
            }
        }
        public void ClearTab(int tabNumber)
        {
            if (tabNumber < 0 || tabNumber >= MAX_SPELL_TAB)
                throw new Exception("Tried to clear an illegal spell tab: " + tabNumber);

            var tab = CoreManager.Current.CharacterFilter.SpellBar(tabNumber);
            //for (var i = tab.Count; i >= 0; i--)
            for (var i = 0; i < tab.Count; i++)
            {
                CoreManager.Current.Actions.SpellTabDelete(tabNumber, tab[i]);
            }
        }

        #region Load Spells Details
        // Summary:
        //     Adds a spell to the specified tab on the player's spell bar. The spell must be
        //     in the player's spell book. Each spell tab can contain only one copy of each
        //     spell. Putting a spell onto a tab that already contains that spell will just
        //     move the spell to the new index.
        //
        // Parameters:
        //   tab:
        //     The zero-based tab index to add the spell.
        //
        //   index:
        //     The zero-based slot on the tab to add the spell. If this index is greater than
        //     the number of spells on the tab, the spell will be added to the first unused
        //     slot.
        //
        //   spellId:
        //     The ID of the spell to be added.
        #endregion
        public void LoadSpells()
        {
            LoadSpells(DEFAULT_SPELL_PATH);
        }
        public void LoadSpells(string path)
        {
            path = Path.Combine(Utils.AssemblyDirectory, path);
            if (!File.Exists(path))
            {
                Utils.WriteToChat($"No file found at {path}");
                return;
            }

            //TODO: Wait until spells are all gone / implement something that only deletes/inserts what you need to
            ClearAllSpells();

            try
            {
                var json = File.ReadAllText(path);
                var tabs = JsonConvert.DeserializeObject<SpellTab[]>(json);

                //Populate list of spells to be leveled
                spellsToLoad = new List<Spell>();
                for (var i = 0; i < tabs.Length; i++)
                {
                    var tab = tabs[i];
                    for (var j = 0; j < tab.Spells.Length; j++)
                    {
                        var spell = new Spell
                        {
                            Tab = i,
                            Index = j,
                            SpellID = tab.Spells[j]
                        };
                        spellsToLoad.Add(spell);
                        //Utils.WriteToChat($"Planning to load ID {spell.SpellID} to tab {spell.Tab}, Slot {spell.Index}");
                    }
                }

                //Start leveling
                spellIndex = 0;
                timer.Enabled = true;

                Utils.WriteToChat($"Loading {spellsToLoad.Count} spells from {path}");
            }
            catch (Exception ex)
            {
                Utils.WriteToChat($"Failed to load spells from: {path}");
                Utils.LogError(ex);
            }
        }


        public static SpellTabManager Instance { get; set; }
        public override void Startup(PluginLogic plugin)
        {
            base.Startup(plugin);
            Instance = this;
            timer = new Timer() { AutoReset = true, Enabled = false, Interval = Plugin.Config.Interval };
            timer.Elapsed += SpendExperienceTick;
        }
        public override void Shutdown()
        {
            base.Shutdown();
            Instance = null;
            timer.Enabled = false;
            timer.Elapsed -= SpendExperienceTick;
        }
    }
}
