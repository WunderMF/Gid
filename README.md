# Gideon
A personal Discord bot written in C# using the Discord.NET library.  
Maintained by Adrian & Matt.
## Features
- Notifies when someone comes online or goes offline.
- Notifies when someone connects or disconnects from a voice channel.

## Commands

### Random
Supports numerical ranges [0 - 1 million] and multipliers and generates a list to random a value from.
- `!random a b` | [a, b]
- `!random 1-5` | [1, 2, 3, 4, 5]
- `!random a*2 b` | [a, a, b]
- `!random 1-5 a*2` | [1, 2, 3, 4, 5, a, a]

### Purge
Purging offers only the basic functionality for deleting messages at the moment.
- `!purge 5` Downloads the last 5 messages and deletes them.
- `!purge 10 cat` Downloads the last 10 messages and deletes any with the word "cat".

### Calculate
Calculate will follow "bidmas" and supports these operations:
- `!calculate 5!` | 120 
- `!calculate 5-5` | 0
- `!calculate 5+5` | 10
- `!calculate 5/5` | 1
- `!calculate 5*5` | 25
- `!calculate 5^5` | 3125
- `!calculate 5!-5+5/5*5^5` | 3240

### LWiki (WIP)
Returns a page from the League of Legends Wikia:
- `!lwiki ashe` | http://leagueoflegends.wikia.com/wiki/Ashe
- `!lwiki health pot` | http://leagueoflegends.wikia.com/wiki/Health_pot

### Play
Gideon can join a channel number and play from a set of sounds:
- `!play [filename]` Plays audio in current channel.
- `!play [filename] [channel number]`  Plays audio in a specified channel.
- `!stop` Stops playing audio and disconnects from the channel.

##### Available Filenames:
- celebrate, home, jazz, start

 - - - -
 ![Gideon - Built with Love](http://forthebadge.com/images/featured/featured-built-with-love.svg)