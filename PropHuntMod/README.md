# Prop Hunt

## What is Prop Hunt?
Prop Hunt is very similar to hide and seek.
You hide as objects (props) around the map while seekers try to find and you.
Once you've been found, you become a seeker. The last person standing wins! Look at the [Gameplay](#Gameplay) section for more details


## Installation
This mod requires [SSMP](https://thunderstore.io/c/hollow-knight-silksong/p/SSMP/SSMP/) for multiplayer.

### Thunderstore
You should be able to install this with any compatible installer/mod manager.

### Manual Installation
1) Download and extract the mod
2) Navigate to your BepInEx plugins folder
3) Copy the extracted mod folder to the plugins folder


## Gameplay
Before and between rounds (see [commands](#Commands) section), players are free to switch between props and destroy other players' props.
> A list of each room's props can be found [here](https://docs.google.com/spreadsheets/d/1pxumrYVEBxFdBz4NKolcoCLyxjkwwWMmx7_CYVWZ0Qc).
You can use a mod like [QuickWarp](https://thunderstore.io/c/hollow-knight-silksong/p/hk_speedrunning/QuickWarp/) to go to specific rooms easily.

> It's worth noting that some rooms may not have any props, or may have very few.
If you encounter any, please create an issue on the mod's [GitHub](https://github.com/BobbyTheCatfish/PropHuntMod/issues).

Take this time before starting to establish a few rules:
- Will seekers wait until all hiders say they're ready, or just wait for a length of time? (how long?)
- Can hiders hide in other rooms? (Only reccomended for larger games)

Once a round is started [(/hunt)](#Commands), 1 player will be selected to be the seeker, and everyone else will be a hider. 

#### Hiders
As soon as you're chosen as a hider, you'll be hidden as a random prop. You can change that prop and move it around if desired. (See [keybinds](#Keybinds))

Hide somewhere in the room (or in nearby rooms if allowed), blending in with the environment around you.
If a seeker comes along and attacks your prop, you'll be revealed and will become a seeker.
If you're the last player found, you win!

> If you're playing with a lot of people, it may be a good idea to allow hiding in nearby rooms and sub-areas.

#### Seekers
As soon as you're chosen as a seeker, you'll need to go to the designated waiting area to wait.
Once the waiting period is over, you can go searching for the hiders. **Attacking a prop will reveal the player underneath.**
Once a hider is found, they become a seeker.

## Usage
> See the [SSMP usage documentation](https://thunderstore.io/c/hollow-knight-silksong/p/SSMP/SSMP/) for details on setting up or joining a server

### Keybinds
There are a few keybinds for this mod, which can be changed within BepInEx Configuration Manager.

`P` -> `Prop` (Hide as a prop)

`R` -> `Reset` (Unhide and unprop)


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
- Add configuration to limit number of prop swaps per round
- Add configuration to select number of seekers
- Add more props