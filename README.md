# KH2ArchipelagoTracker

A Kingdom Hearts 2 item tracker for use with the Archipelago multiworld multi-game randomizer

![Screenshot](KH2Tracker.png)

## Options

* Save Progress
* Saves the current tracker state to a file. If hints are loaded they will be saved as well
* Load Progress
    * Loads a saved tracker file.
* Run Auto Tracker
    * Connects the tracker to the PC game and automatically tracks important checks as they are found

***

* Broadcast Window
    * Opens a window with a more typical tracker display for streaming. Everything in this window updates when tracking
      checks from the default window

***

* Reset Window
* Resets the main and broadcast windows to their default size
* Reset Tracker
    * Resets the tracker to its default state

## Toggles

* Loading Hints will automatically toggle the settings that the hint file was made with

* Promise Charm
    * Toggles on/off the promise charm as an important check
* Once More / Second Chance
    * Toggles on/off once more and second chance as important checks
* Torn Pages
    * Toggles on/off the torn pages as important checks
* Cure
    * Toggles on/off the cure spells as important checks
* Final Form
    * Toggles on/off final form as an important check

***

* Sora's Heart
    * Toggles on/off Levels as a place that important checks can be found
* Simulated Twilight Town
    * Toggles on/off Simulated Twilight Town as a place that important checks can be found
* 100 Acre Wood
    * Toggles on/off 100 Acre Wood as a place that important checks can be found
* Atlantica
    * Toggles on/off Atlantica as a place that important checks can be found

***

* Simple Icons
    * Simplistic important check icons for both the main window and broadcast window
* Orb Icons
    * Orb-like important check icons for both the main window and broadcast window
* Third Option
    * Orb-like important check icons for the main window with more detailed icons for the broadcast window

***

* World Icons

    * Toggles between simplistic icons and abbreviations for worlds

***

* Drag and Drop
    * Toggles between drag and dropping items + selecting a world and double clicking an item or just selecting a world
      and single clicking an item to track items

## How To Use

Drag an item to the location that you found it in. Alternatively highlight worlds by clicking on them and then double
click on items to mark them as collected in that world. Clicking on a marked item will return it to the item pool. (if
not using drag and drop controls then only a single click on an item is required)

The question marks connected to each world denote the number of important checks in a world. If hints are loaded these
will be set automatically as reports are tracked. If hints are not loaded they can be increased or decreased with the
scroll wheel or by selecting a world and using page up / page down

If a hint file is loaded into the tracker reports must be tracked correctly. Incorrectly tracking a report 3 times will
lock you out of tracking that report and receiving its hint. Hovering over already tracked reports will also display
their hint text.

## Auto Tracker

The auto tracker functionality works by reading pcsx2's memory. Trying to run the auto tracker when pcsx2 is closed will
not work and closing pcsx2 while the auto tracker is running will stop it.

In addition to automatically tracking the important checks, the auto tracker tracks your stats as well as the starting
weapon you chose. One thing to keep in mind for stats is that during cutscenes and some fights they values they show
will not be correct. If using the broadcast window the auto tracker will also track drive form levels and growth
abilities. Currently there isn't a way to track these yourself or see them in the main window.

Valor form is not being automatically tracked due to the flag for it being obtained being set anytime you open the
summon command menu. Final form will only be tracked automatically if it is found before being forced (both normally or
through light and dark).

If you are playing from a saved tracker file be sure to load it before turning on the auto tracker or everything will be
placed into the world you are currently in. Also if you are playing a seed across multiple sessions do not start a new
session from a save point in STT or the checks there will be tracked incorrectly for now.

When playing multiple seeds in a row be sure to fully close and reopen pcsx2 or wait to start the auto tracker again
after starting a new game to make sure the memory has been reset

## Troubleshooting

### On Windows 11, the cursor spins for a second as if its opening but then nothing opens
apparently this is a workaround:
- it seems for wahtever reason your PC isnt allowing the tracker to make the needed folder in appdata
- you can try running as admin
- or what fixed it for them was running it in compatiblity mode for some reason
- they were on windows 11
- can check by looking at appdata/local
- there should be a KhTracker folder
- then inside that a folder per version of tracker
- if youve only ever used 1 just a folder inside of KhTracker

## Thanks

* Dee-Ayy
    * Wrote and maintained the tracker, of which this one is just a modification to work with Archipelago.
* Tommadness
    * Created the broadcast window and the framework from which the auto tracker was built. Spent a ton of time helping
      figure out bugs and solutions to get the auto tracker working.
* Televo
    * Made all of the icons the tracker uses that weren't taken straight from the game (some modified very slightly by
      me)
* Sonicshadowsilver2
    * Made the GoA mod that the randomizer itself is built on and provided tons of useful information to help create the
      auto tracker
