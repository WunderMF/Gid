# Gideon
Discord bot 'Gideon' written in Python using the Discord.py API wrapper.

## Features
- Notifies when a user connects or disconnects from the server.
- Notifies when a user joins or leaves a voice channel.
- Logs all the notifications in a separate channel to avoid spam.
- Dynamic @Voice role to mention all users only in voice chat.
- Ability to add aliases to use in commands.

## Commands

### Info
- `!info` Displays this GitHub page.


### Seen
Check when a user was last online (by their username or nickname):
- `!seen Barry` | Barry was last seen at 8:00pm on Tue 7 Oct 2014


### Choose
Randomly chooses an item from a list of given inputs:
- `!choose Batman Superman Flash` | Flash (33%)


### AddAlias
Adds an alias associated to your user for use in commands:
- `!addAlias Batman` | Bruce adds 'Batman' as an alias
- `!seen Batman` now yields the same result as `!seen Bruce`


## Admin

### SetAlias
Sets an alias for a user.
- `!setAlias Batman Bruce` | Adds the alias 'Batman' for Bruce
- `!seen Batman` now yields the same result as `!seen Bruce`


## Upcoming

### VoteMute
- `!votemute Mxyzptlk` | Calls a vote to server mute/unmute a user in a voice channel


 - - - -
 ![Gideon - Built with Love](http://forthebadge.com/images/featured/featured-built-with-love.svg)
