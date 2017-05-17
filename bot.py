import discord
import config
import aliases
import fileman
import util
import random
import league
from discord.ext import commands
from server import Server
from discord import Status
from util import bt

bot = commands.Bot(command_prefix = '.')
server = Server()

@bot.event
async def on_ready():
	fileman.init()
	server.server = list(bot.connection._servers.values())[0]
	server.log = discord.utils.get(server.server.channels, name = 'log')
	print ('Connected')

# # Logs user online and offline status
# @bot.event
# async def on_member_update(before, after):
# 	if after.bot is False:
# 		if (before.status is not Status.offline) & (after.status is Status.offline):
# 			await bot.send_message(server.log, bt(after.name + ' is now offline') )
# 			fileman.update_seen(after, util.get_time())

# 		elif (before.status is Status.offline) & (after.status is not Status.offline):
# 			await bot.send_message(server.log, bt(after.name + ' is now online') )

# # Logs joining and leaving voice channels
# @bot.event
# async def on_voice_state_update(before, after):
# 	voice_role = get('voice', server.server.roles)

# 	if (before.voice.voice_channel is not None) & (after.voice.voice_channel is None):
# 		await bot.send_message(server.log, bt(after.name + ' left ' + before.voice.voice_channel.name) )
# 		await bot.remove_roles(after, voice_role)

# 	elif (before.voice.voice_channel != after.voice.voice_channel):
# 		await bot.send_message(log, bt(after.name + ' joined ' + after.voice.voice_channel.name) )
# 		await bot.add_roles(after, voice_role)

# League Link
@bot.event
async def on_member_update(before, after):
	if (after.game):
		if (after.game.name == 'Preview'):
			print (after.name + ' is playing League!')
			server.lq.append(after)
			league.check()

# Randomly chooses from a list of arguments
@bot.command() 
async def choose(*choices : str):
	if choices:
		answer = random.choice(choices)
		chance = '{0:.0f}%'.format(choices.count(answer) / len(choices) * 100)
		await bot.say( bt(answer + ' (' + chance + ')') )
	else:
		await bot.say( bt('No inputs given') )

# Checks when a user was last seen online
@bot.command()
async def seen(user):
	member = util.member_from_alias(user, server.server)

	if member is None:
		await bot.say( bt('User not found') )
		return

	seen_time = fileman.find(member.id, 'seen.json')

	if seen_time is None:
		await bot.say( bt(member.name + ' not seen yet') )
	else:
		await bot.say( bt(member.name + ' was last seen on ' + util.format_time(seen_time)) )

@bot.command(pass_context = True)
async def addl(context, summoner, user):
	if context.message.author.name == '4dau':
		member = discord.utils.get(server.server.members, name = user)
		league.add(summoner.lower(), member.id)
		await bot.say(bt(member.name + ' added ' + summoner))
	else:
		await bot.say(bt('u cant'))
		return

# @bot.command(pass_context = True)
# async def setAlias(context, alias, user):
# 	if context.message.author.name == '4dau':
# 		member = discord.utils.get(server.server.members, name = user)
# 		aliases.add(alias.lower(), member.id)
# 		await bot.say(bt(member.name + ' added ' + alias))
# 	else:
# 		await bot.say(bt('u cant'))
# 		return

# @bot.command(pass_context = True)
# async def addAlias(context, alias):
# 	member = context.message.author
# 	aliases.add(member.name.lower(), member.id)
# 	aliases.add(alias.lower(), member.id)
# 	await bot.say(bt(member.name + ' added ' + alias))
	
bot.run(config.token)
