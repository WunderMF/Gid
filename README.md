# Gideon
A personal Discord bot written in C# using the Discord.NET 0.9 library.  
Maintained by Adrian and Matt.

## Features
- Notifies when a user comes online or goes offline.
- Notifies when a user connects or disconnects from a voice channel.
- Keeps track of when a user was last online.

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
- `!cal 5!` | 120 
- `!cal 5-5` | 0
- `!cal 5+5` | 10
- `!cal 5/5` | 1
- `!cal 5*5` | 25
- `!cal 5^5` | 3125
- `!cal 5!-5+5/5*5^5` | 3240
- `!cal (5+5)*5` | 50

### Rank
Gideon can find and return the rank of a league summoner:
- `!rank [summoner]` Returns the summoners rank

### Tilt
Gideon we calculate how tilted a league summoner is:
- `!tilt [summoner]` Returns a value predicting a summoners tilt (higher = more tilted)

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
- start

### Seen
Check when a user was last online (by their username or nickname):
- `!seen Barry` | Barry was last seen at 8:00pm on Tue 7 Oct 2014

### Define
Calls upon Oxford Dictionary's API to define a word and provides an example (if available).
- `!define camera`
```
camera
-
(Noun) a device for recording visual images in the form of photographs, film, or video signals:
Example: "she faced the cameras"
```

 - - - -
 ![Gideon - Built with Love](http://forthebadge.com/images/featured/featured-built-with-love.svg)