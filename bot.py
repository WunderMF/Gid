import discord
import config
import random
import time
import json
import os
from discord.ext import commands
from datetime import datetime
from discord import Status
from pprint import pprint

bot = commands.Bot(command_prefix = '.')
bot_files = ['seen.json', 'aliases.json']
bot_roles = ['voice']

#================================================================================
# Events
#================================================================================

@bot.event
async def on_ready():
	await init_bot()
	print ('Connected')

@bot.event
async def on_member_update(before, after):
	"""Logs when users connect and disconnect from the server"""
	if not after.bot:
		if (before.status is not Status.offline) & (after.status is Status.offline):
			await bot.send_message(bot.log, after.name + ' connected')
			update_seen(after, datetime.now())

		elif (before.status is Status.offline) & (after.status is not Status.offline):
			await bot.send_message(bot.log, after.name + ' disconnected')

@bot.event
async def on_voice_state_update(before, after):
	"""Logs when users join and leave a voice channel"""
	voice_role = discord.utils.get(bot.server.roles, name = 'voice')

	if (before.voice.voice_channel is not None) & (after.voice.voice_channel is None):
		await bot.send_message(bot.log, after.name + ' left ' + before.voice.voice_channel.name)	
		await bot.remove_roles(after, voice_role)

	elif (before.voice.voice_channel != after.voice.voice_channel):
		await bot.send_message(bot.log, after.name + ' joined ' + after.voice.voice_channel.name)
		await bot.add_roles(after, voice_role)

#================================================================================
# User commands
#================================================================================

@bot.command()
async def info():
	"""Displays Github link"""
	await bot.say('https://github.com/adrianau/Gideon')

@bot.command()
async def choose(*choices : str):
	"""Randomly chooses from given choices"""
	if choices:
		answer = random.choice(choices)
		chance = '{0:.0f}%'.format(choices.count(answer) / len(choices) * 100)
		await bot.say(answer + ' (' + chance + ')')
	else:
		await bot.say('No input given')

@bot.command()
async def seen(user):
	"""Outputs when the user last appeared online on the server"""
	member = member_from_alias(user, bot.server)
	if member is None:
		await bot.say('User not found')
		return

	seen_time = find(member.id, 'seen.json')
	if seen_time:
		await bot.say(member.name + ' was last seen on ' + format_time(seen_time))
	else:
		await bot.say(member.name + ' not seen yet')

#================================================================================
# Utility functions
#================================================================================

def update_seen(member, time):
	update_file('seen.json', member.id, str(time))

async def update_role(role):
	"""Updates roles with newest members"""
	if role.name == 'voice':
		for member in bot.server.members:
			if member.voice.voice_channel:
				await bot.add_roles(member, role)
			elif role in member.roles:
				await bot.remove_roles(member, role)

def format_time(dt):
	"""Formats a datetime string like: 15 May 2017 at 9:01PM"""
	dto = datetime.strptime(dt, '%Y-%m-%d %H:%M:%S.%f')
	date = dto.strftime('%a %d %b %Y')
	time = dto.strftime('%I:%M%p').lstrip('0')

	return date + ' at ' + time

def member_from_alias(alias, server):
	"""Retrieves a member object from a given alias"""
	id = find(alias.lower(), 'aliases.json')
	if id:
		return bot.server.get_member(id)
	else:
		return None

#================================================================================
# File management
#================================================================================

async def init_bot():
	"""Initialises files and necessary setup"""
	bot.server = list(bot.connection._servers.values())[0]
	bot.log = discord.utils.get(bot.server.channels, name = 'log')

	for file in bot_files:
		if os.path.isfile(file):
			print(file + ' found!')
		else:
			print(file + ' not found')
			with open(file, 'w') as f:
				json.dump({}, f, indent = 4)

	for role in bot_roles:
		role = discord.utils.get(bot.server.roles, name = role)
		await update_role(role)

def update_file(file, key, value):
	"""Updates a file with a new key/value pair"""
	with open(file, 'r') as f:
		data = json.load(f)
		data[key] = value

		os.remove(file)
		with open(file, 'w') as f:
			json.dump(data, f, indent = 4)

def find(key, file):
	"""Returns value of a key in a file"""
	with open(file, 'r') as f:
		data = json.load(f)
		found = True if key in data else False

		if found:
			return data[key]
		else:
			return None

#================================================================================
# Token
#================================================================================

bot.run(config.token)
