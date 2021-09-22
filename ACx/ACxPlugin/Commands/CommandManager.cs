﻿using Decal.Adapter;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Decal.Adapter.Wrappers;

namespace ACxPlugin
{
    public class CommandManager
    {
        private PluginLogic Plugin;
        private Regex commandParser;
        private string commandPattern = String.Join("|", Enum.GetNames(typeof(Command)).OrderBy(x=>x).ToArray()).ToLower();

        public CommandManager(PluginLogic plugin)
        {
            this.Plugin = plugin;
        }

        public void Core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                //Just the trigger dumps the list of commands
                var match = commandParser.Match(e.Text);

                //Seems like there's a weird time-sensistize need to set e.Eat
                if (match.Success)
                {
                    //Utils.WriteToChat($"Successful match: {e.Text}");
                    //for (var i = 0; i < match.Groups.Count; i++)
                    //    Utils.WriteToChat($"Group {i}: {match.Groups[i].Success}\t{match.Groups[i].Value}");

                    //Don't propagate if command was matched
                    e.Eat = true;

                    //Just the trigger
                    if (!match.Groups[1].Success)
                    {
                        //Utils.WriteToChat("Trigger hit: " + CommandTrigger);
                        ProcessCommand(Command.Help);
                        return;
                    }

                    var command = (Command)Enum.Parse(typeof(Command), match.Groups[1].Value, true);
                    //There aren't parameters but a command
                    if (!match.Groups[2].Success)
                    {
                        //Utils.WriteToChat("Command: " + match.Groups[1].Value);
                        ProcessCommand(command);
                        return;
                    }
                    //There are parameters
                    else if (match.Groups[2].Success)
                    {
                        //Utils.WriteToChat("Group 2?  " + match.Groups[2].Value);
                        //Utils.WriteToChat("Command: " + match.Groups[1].Value + "\t" + command);
                        //Utils.WriteToChat("Params: " + match.Groups[2].Value);
                        ProcessCommand(command, match.Groups[2].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                Utils.WriteToChat($"Error parsing command: {ex.Message}");
            }
        }

        private void ProcessCommand(Command command)
        {
            switch (command)
            {
                //Aliases should be set equal to full command in the Command enum
                case Command.AddParty:
                    PartyHelper.AddParty();
                    break;
                case Command.LoadParty:
                    PartyHelper.LoadParty();
                    break;
                case Command.PartyNearby:
                    PartyHelper.SetPartyNearby();
                    break;
                case Command.Policy:
                    Plugin.Profile.ExpPolicy.PrintPolicy();
                    break;
                case Command.Plan:
                    Plugin.Profile.ExpPolicy.PrintExperiencePlan();
                    break;
                case Command.LevelSlow:
                    Plugin.Profile.ExpPolicy.SpendExperience();
                    break;
                case Command.Level:
                    Plugin.Profile.ExpPolicy.SpendExperience(true);
                    break;
                case Command.EditConfig:
                    try { Process.Start(Plugin.Profile.Path); }
                    catch (Exception e) { Utils.LogError(e); }
                    break;
                case Command.EditPolicy:
                    try { Process.Start(Plugin.Profile.Path); }
                    catch (Exception e) { Utils.LogError(e); }
                    break;
                case Command.PrintLogins:
                    LoginHelper.PrintLogins();
                    break;
                case Command.Log:
                    try {
                        Process.Start(Utils.LogPath);
                    }
                    catch (Exception e) { Utils.LogError(e); }
                    break;
                case Command.SaveSpells:
                    Plugin.SpellManager.SaveAllSpells();
                    break;
                case Command.ClearSpells:
                    Plugin.SpellManager.ClearAllSpells();
                    break;
                case Command.LoadSpells:
                    Plugin.SpellManager.LoadSpells();
                    break;
                //case Command.Pickup:
                //    //var core = CoreManager.Current.Actions;
                //    if (core.CurrentSelection != 0)
                //        core.MoveItem(core.CurrentSelection, CoreManager.Current.CharacterFilter.Id);
                //    break;
                case Command.Help:
                default:
                    Utils.WriteToChat("Valid commands are: " + commandPattern);
                    break;
            }
        }

        //Could split this into a separate ParamCommand enum
        private void ProcessCommand(Command command, string parameters)
        {
            switch (command)
            {
                case Command.SaveSpells:
                    Plugin.SpellManager.SaveAllSpells(parameters);
                    break;
                case Command.LoadSpells:
                    Plugin.SpellManager.LoadSpells(parameters);
                    break;
                case Command.LoginNext:
                    LoginHelper.SetNextLogin(parameters, false);
                    break;
                case Command.LoginNextLoop:
                    LoginHelper.SetNextLogin(parameters, true);
                    break;
                default:
                    Utils.WriteToChat("Valid commands are: " + commandPattern);
                    break;
            }
        }

        public void SetupCommandParser()
        {
            var trigger = Regex.Escape(Plugin.Config.Trigger) ?? Utils.DEFAULT_TRIGGER;
            //Utils.WriteToChat("Trigger:" + trigger);
            string commandRegex =
                $"^(?:{trigger} (?<command>{commandPattern})) (?<params>.+)$|" +  //Command with params
                $"^(?:{trigger} (?<command>{commandPattern}))$|" +  //Command no params
                $"^(?:{trigger})$";                      //Just trigger-- could use this to match anything starting with the trigger but not matching a command
                                                         //Utils.WriteToChat(commandRegex);
            commandParser = new Regex(commandRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //Utils.WriteToChat($"Command regex: {commandRegex}");

            //Register event
            CoreManager.Current.CommandLineText += Core_CommandLineText;
        }

        internal void Shutdown()
        {
            //Utils.WriteToChat("Shutting down command manager...");
            CoreManager.Current.CommandLineText -= Core_CommandLineText;
        }
    }
}