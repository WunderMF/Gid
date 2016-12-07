# Gideon
A personal Discord bot written in C# using the Discord.NET library.

## Features
- Notifies when someone comes online or leaves the Discord channel
- Notifies when someone connects or disconnects from the voice channel

## Commands

### General
- `!help` Displays the link to this page.
- `!random` Randomly chooses between a list of things separated by spaces.

#### !Random examples
!random supports [x-y] ranges and multipliers for generating a list to choose from.
- `!random this that`: [this, that]
- `!random 1-5`: [1,2,3,4,5]
- `!random apple*2 banana`: [apple, apple, banana]
- `!random this 1-5 apple*2`: [this,1,2,3,4,5,apple,apple]

### Admin
- `!purge` Deletes the last x messages.
