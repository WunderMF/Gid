# Gideon
Discord bot 'Gideon' written in Python using the Discord.py API wrapper.

## Features
- Notifies when a user connects or disconnects from the server.
- Notifies when a user joins or leaves a voice channel.
- Logs all the notifications in a separate channel to avoid spam.
- Dynamic @Voice role to mention all users only in voice chat.
- Ability to add custom aliases for use in commands.

## Commands

### info
- `!info` | Displays this GitHub page.


### seen
Check when a user was last online (by their username or nickname):
- `!seen Barry` | Barry was last seen at 8:00pm on Tue 7 Oct 2014


### countdown
Sends countdown messages, defaulting at 3 and capped at 5 (to prevent spam):
- `!countdown` | 3 2 1 Go
- `!countdown 5` | 5 4 3 2 1 Go 


### choose
Randomly chooses an item from a list of given inputs:
- `!choose Batman Superman Flash` | Flash (33%)


### addalias
Adds an alias associated to your user for use in commands:
- `!addalias Batman` | Bruce adds 'Batman' as an alias
- `!seen Batman` now yields the same result as `!seen Bruce`


### join [role]
Join available roles. Currently it is only limited to @league and @pubg:
- `!join pubg` | You have now joined the @pubg role
- You are now tagged when someone mentions @pubg


### leave [role]
Leaves any role you are a part of:
- `!leave pubg` | You have now left the @pubg role
- You are no longer tagged when someone mentions @pubg


### viewrole [role]
View all the members of a specified role:
- `!viewrole GGT` | Lists all members of the GGT role


## Admin

### setalias
Sets an alias for a user.
- `!setAlias Batman Bruce` | Adds the alias 'Batman' for Bruce
- `!seen Batman` now yields the same result as `!seen Bruce`


## Upcoming

### VoteMute
- `!votemute Mxyzptlk` | Calls a vote to server mute/unmute a user in a voice channel


 - - - -
 ![Gideon - Built with Love](http://forthebadge.com/images/featured/featured-built-with-love.svg)
