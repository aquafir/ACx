# !!Deprecated!!

The [login](https://ubstaging.surge.sh/master/docs/tools/loginmanagercommand/), [xp](https://utilitybelt.gitlab.io/docs/tools/autoxp/), and [spellbar management](https://ubstaging.surge.sh/master/docs/tools/spellmanager/) functionality have been added (and in some cases fixed/improved) to UtilityBelt (beta, for now).  That will be the preferred plugin in the future.

# ACx

Previously posted under [SillAC](https://github.com/sillAC/SillAC-Tools) before taking a long break.  The plugin has since been reworked using [trevis' hot-reload template](https://gitlab.com/trevis/HotDecalPluginTemplate), with a few improvements and a few things added.

To use, add `ACx.dll` to Decal.  It will show up under `Network Filters`





## Configuration

Configuration is done through `Config.json` and can be opened while in-game using `/x ec` or `/x editconfig`.

Within it you can change the trigger from `/x` to something else, or adjust the speed messages are sent to the server.



The main use of the configuration is to select which profile will be used for a character using their name, account, or server:

```
     {
      "FriendlyName": "Default Profile",
      "Priority": 0,
      "Path": "Profiles\\Default.json"
     }
    ,{
      "FriendlyName": "Bow",
      "CharName": "Robin|.*bow.*",
      "Priority": 2,
      "Path": "Profiles\\Bow.json"
    }
    ,{
      "FriendlyName": "TestServer",
      "Priority": 999,
      "Server": "HomeEmulator",
      "Path": "Profiles\\Test.json"
    }
```



Rules are looked at in order of descending priority.  If they match the character the profile at `Path` is used (double "\\\\" needed but you can use "/" instead), otherwise it keeps looking.

The above example  has three rules that match characters to profiles:  

* The first is a low-priority "default"
* The second matches the name Robin or anything with "bow" in it with a higher priority
* The last is a very high priority based on the server



Both configuration and profiles:

* Reload upon changes
* Are attempted to be created if missing





### Profile

Profiles contain group-specific settings that are documented under the relevant section (e.g., the `Policy` of [AutoXP](#AutoXP)).







## Commands

* Some commands may be hidden because they're less relevant to players.
* Commands aren't case-sensitive
* Many commands have shortened aliases (e.g., `loginnext|ln`)



`/x` or `/x help` lists commands

`/x ep|editpolicy` opens your current [Profile](#Profile)

`/x ec|editconfig` opens your current [Config](#Configuration)

`/x log` opens the plugin's log





### Login Commands

In a [Profile](#Profile) you can add an array of files that are loaded using AC's `/loadfile [batch file]` and chat commands:

```
"Login Load Commands": ["Login/Start.txt"],
"Login Commands": ["/x ln 1", "/x level"],
```

Commands will be ran after a short delay (2s) after logging in.  



https://user-images.githubusercontent.com/83029060/134779778-5ab99f4f-00be-4930-b31b-6a9ba79f0a8d.mp4



### AutoXP

Spend available experience based on a ratio you define in the `Policy` of a [Profile](#Profile).  

The higher the number the more experience you'd spend on the skill (e.g., War - 10, Endurance - 1 would level War if it cost less than 10x Endurance).



Start / stop spending experience with:

`/x [l|level]` will spend xp in batches

`/x levelslow ` will spend xp one level at a time



View plan to spend experience:

`/x plan`

View loaded policy in game:

`/x policy`


https://user-images.githubusercontent.com/83029060/134779589-2d254104-dbbe-4ba6-a889-93b31ee605ad.mp4



*You can delete or comment out unused targets if you like.*

*Not able to account for xp already spent on a skill. Doesn't try to get the last level for that reason (and to save server operators some grief)*

*In testing there are some server error messages (e.g., player tried to raise skill beyond 350046134 experience) at the end of levels.  Doesn't cause any harm besides the redtext and stops after.  Haven't figured out why it's happening, based on the approach I used.*



### Spellbar Manager

Save all spells (default path `Spells.json`):

`/x [ss|SaveSpells] [path]`

Clear all spells:

`/x [cs|ClearSpells]`

Load spells (takes time to add to bar, default path `Spells.json`):

`/x [ls|LoadSpells] [path]`



https://user-images.githubusercontent.com/83029060/134779689-16b89caf-b7bd-4822-a7f2-1fccfa3895eb.mp4



*This only works up to the 7th spell bar as far as I know.*



### Location Manager

This looks for the first `.nav` and `.txt` file in the VTank folder that contains the first 4 hex digits of the landblock and loads it:

`/x [loadlocation|ll]` would either the first matching location or create a nav called `0x########` based on location.

`Starter - 8C04.txt` would be loaded automatically via `/loadfile` when you entered the Yaraq Training Academy.



Disable in the [Profile](#Profile) with `"Load Locations": false,`




https://user-images.githubusercontent.com/83029060/134779994-91d9859a-632b-42cc-8e53-3ea8607928cd.mp4




*Assumes you use the default VTank directory*

*If there's interest this may be expanded to include:*

* *Metas or command triggers (no effort for this)*  
* *Coord support instead of just landblock*
  * *Proximity checks (e.g., periodic checks that trigger when 50 units away from a location)* 
* *Customizing more based on profile* 
* *Scanning navs to find the one with the closest* 





### Login Helper (requires [Mag-Filter](https://github.com/Mag-nus/Mag-Plugins/wiki/Mag%E2%80%90Filter))

This lets you set the next character to log in with via Mag-Filter using either a part of the name or a relative change from the current one, alphabetically.



Some examples:

`/x ln mule` would log in the first character with "mule" in their name

`/x lnl -1` would log in with the previous character, looping to the end after the first name

`/x loginnext 5` would log in 5 spots after the current one, but it would halt and clear the login queue if out of bounds

`/x PrintLogins` will display sorted characters and their ID



https://user-images.githubusercontent.com/83029060/134779881-34930129-f8e1-4fcd-a832-03079363bb83.mp4





*This is unable to be used the first time a character is created, but the plugin informs you if that is the case.*

*Todo:  maybe add Mag-Filter defaults, or integrate this with that/vice-versa*





### Party Helper (use with [IBControlUnlimited](http://immortalbob.com/phpBB3/viewtopic.php?f=6&t=656))

This is a pretty basic helper for [Unlimited_IBControl](http://immortalbob.com/phpBB3/viewtopic.php?f=6&t=656) that lets you avoid messing with the meta.



Add all nearby characters to the list of possible controllers (might be used if you trust others and want to briefly let them control your account):

`/x [pn|partynearby]`

Load all characters from `Party.txt` and add them as possible controllers:

`/x [lp|loadparty]`

Save all characters on your account to `Party.txt`:

`/x [ap|addparty]`




