# Gideon
A personal Discord bot written in C# using the Discord.NET library.

## Features
- Notifies when someone comes online or goes offline.
- Notifies when someone connects or disconnects from a voice channel.

## Commands

### General
- `!help` Displays the link to this page.
- `!random` Randomiser for choosing an item in a list of inputs.
- `!calculate` Will calculate the answer to simple math problems.

### Admin
- `!purge` Deletes the last x messages.

## Examples

### !Random
Supports numerical ranges [0 - 1 million] and multipliers to generate lists to random a value from:
- `!random a b` | [a, b]
- `!random 1-5` | [1, 2, 3, 4, 5]
- `!random a*2 b` | [a, a, b]
- `!random 1-5 a*2` | [1, 2, 3, 4, 5, a, a]

### !Purge
Purging offers only the basic functionality for deleting messages at the moment:
- `!purge 5` Downloads the last 5 messages and deletes them.
- `!purge 10 cat` Downloads the last 10 messages and deletes any with the word "cat".

### !Calculate
Calculate will follow "bidmas" and supports these operations:
- `!calculate !5` | 120 
- `!calculate 5-5` | 0
- `!calculate 5+5` | 10
- `!calculate 5/5` | 1
- `!calculate 5*5` | 25
- `!calculate 5^5` | 3125 