# Prop Hunt

## What is Prop Hunt?
Prop Hunt is very similar to hide and seek.
You hide as objects (props) around the map while seekers try to find and you.
Once you've been found, you become a seeker. The last person standing wins! Look at the [Gameplay](#gameplay) section for more details.

```
Note: As part of this mod, enemies can't attack or target you by default. This is to make hiding easier.
If you want to disable this, you can change it within the BepInEx Config Manager
```

## Installation
This mod requires [SSMP](https://thunderstore.io/c/hollow-knight-silksong/p/SSMP/SSMP/) for multiplayer.

### Thunderstore
You should be able to install this with any compatible installer/mod manager.

### Manual Installation
1) Download and extract the mod
2) Navigate to your BepInEx plugins folder
3) Copy the extracted mod folder to the plugins folder (Example: `Plugins/PropHunt/PropHuntMod.dll`)


## Gameplay
Before and between rounds (see [commands](#commands) section), players are free to switch between props and destroy other players' props. Each room has a different set of props you can hide as.
> A list of each room's props can be found [here](https://docs.google.com/spreadsheets/d/1pxumrYVEBxFdBz4NKolcoCLyxjkwwWMmx7_CYVWZ0Qc).
You can use a mod like [QuickWarp](https://thunderstore.io/c/hollow-knight-silksong/p/hk_speedrunning/QuickWarp/) to go to specific rooms easily.

> It's worth noting that some rooms may not have any props, or may have very few.
If you encounter any, please create an issue on the mod's [GitHub](https://github.com/BobbyTheCatfish/PropHuntMod/issues).

Take this time before starting to establish a few rules:
- Will seekers wait until all hiders say they're ready, or just wait for a length of time? (30 seconds by default)
- Can hiders hide in other rooms? (Only reccomended for larger games)

Once a round is started with [/hunt](#commands), 1 player will be selected to be the seeker, and everyone else will be a hider. 

#### Hiders
As soon as you're chosen as a hider, you'll be hidden as a random prop. You can change that prop and move it around if desired. (See [keybinds](#keybinds))

Hide somewhere in the room (or in nearby rooms if allowed), blending in with the environment around you.
**If a seeker comes along and attacks your prop, you'll be revealed and will become a seeker.**
If you're the last player found, you win!

> If you're playing with a lot of people, it may be a good idea to allow hiding in nearby rooms and sub-areas.

#### Seekers
As soon as you're chosen as a seeker, your screen will turn black for 30 seconds, and you'll be unable to move. The hiders will use this time to hide.
Once the waiting period is over, you can go searching for the hiders. **Attacking a prop will reveal the player underneath.**
Once a hider is found, they become a seeker.

## Usage
> See the [SSMP usage documentation](https://thunderstore.io/c/hollow-knight-silksong/p/SSMP/SSMP/) for details on setting up or joining a server

### Keybinds
There are a few keybinds for this mod, which can be changed within BepInEx Configuration Manager.

`P` -> `Prop` (Change your prop)

`R` -> `Reset` (Reveal yourself and remove your prop)


The numpad can be used to move and rotate your prop within certain limits.
Unfortunately these aren't configurable yet, but will be in a future update.
```
7 8 9      forward         up          back
4 5 6       left      center/reset     right
1 2 3     rot. left       down      rot. right
```

### Commands
These commands can be used by pressing `Y` to open the SSMP chat
- `/hunt` (starts a round of prop hunt)
- `/stop` (stops the current round of prop hunt)
- `/hitboxes` (enables viewing prop hitboxes, hiders only and meant for debug)
- `/sync` (syncs the states of other players props)



## Roadmap
These are just some things that I'd like to add in the future
- Use a mode toggle system for prop movement
- Add game configuration
	- select number of seekers
	- seeker wait time
	- limit the number of prop swaps per player per round
- Add more props

## Reporting Issues
This is still a little beta at the moment. I haven't had many chances to test the multiplayer features, so please check your logs often.
Please report any errors you see (either in the logs or just things you noticed) to the mod's [GitHub repository](https://github.com/BobbyTheCatfish/PropHuntMod/issues).
I'd really appreciate it! I want this mod to be fun for everyone to play.