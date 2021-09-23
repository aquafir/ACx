# Sill's AC Tools  (WIP)

## Structure

* PluginLogic contains:
  * Modules and whether they're enabled or not
  * CommandManager routes command/
* Startup/Shutdown called when plugin hotloaded



## Config and Profiles

* **Config** located in `Config.json`
  * Contains plugin-wide values or values that wouldn't care which character is being used.
  * Maps to different *profiles* based on an optional regex of their *name*, *account*, and the *server name*.
  * Trigger can be customized, but by default it is `/xp`
  * Try to open the configuration with `/xp editconfig`
* **Profiles** contain more specific settings that vary per character
  * Matched in order of descending *priority*
* Defaults are attempted to be created if the path is  missing
* Reloads upon change



You can access the main profile and the character's profile with:

`/xp editconfig`

`/xp editpolicy`



## Modules



### AutoXP

[Demo video](https://streamable.com/yt0gwf)

Spend available experience based on a ratio you define.  The higher the number the more experience you'd spend on the skill (e.g., War - 10, Endurance - 1 would level War if it cost less than 10x Endurance).

View loaded policy in game:

`/xp policy`

View plan to spend experience:

`/xp plan`

Spend / stop spending experience:

`/xp level`

`/xp spend`



### Spellbar Manager

[Demo video](https://streamable.com/xu3ca5)

*This only works up to the 7th spell bar as far as I know.*



Save all spells (default path "Spells.json"):

`/xp ss [path]`

Clear all spells:

`/xp cs`

Load spells (takes time to add to bar, default path "Spells.json"):

`/xp ls [path]`



### Login Commands

The character profiles can include a file that will be loaded 5 seconds (for now) after logging in using AC's `/loadfile [batch file]`

There's other stuff out there that accomplishes the same, but this lets you do it based on the name/account/server regex, which gives a different (easier for me) way of grouping characters together.

[Login commands demo](https://imgur.com/mHcZg1n)

* Use either an absolute path for the file (replace `\` with `\\`) or a path relative to the *profile*.



### IBControl<sup>2</sup>

This externalizes the control of variables in IBControl.  When set up you shouldn't need to configure it for new characters. 

In ModdedIBControlmet:

* Removed initialization variables.  They are set through an external source.
* Changed `#action follow` to a command in UtilityBelt to follow the designated leader: 
  `chatbox[\/ub follow +getvar[charone]]`