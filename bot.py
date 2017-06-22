import discord
import config
import requests
import random
import json
import os
from collections import OrderedDict
from discord.ext import commands
from datetime import datetime
from discord import Status
from pprint import pprint

bot = commands.Bot(command_prefix = '.')
bot_files = ['seen.json', 'aliases.json']
bot_roles = ['voice']
game_roles = ['league', 'pubg']

#================================================================================
# Events
#================================================================================

@bot.event
async def on_ready():
	await init_bot()
	print ('Connected')

@bot.event
async def on_member_update(before, after):
	"""Log the users that connect and disconnect from the server."""
	if not after.bot:
		if (before.status is Status.offline) & (after.status is not Status.offline):
			await bot.send_message(bot.log, after.name + ' connected')
		elif (before.status is not Status.offline) & (after.status is Status.offline):
			await bot.send_message(bot.log, after.name + ' disconnected')
			update_seen(after, datetime.now())

@bot.event
async def on_voice_state_update(before, after):
	"""Log the users that join and leave a voice channel."""
	voice_role = discord.utils.get(bot.server.roles, name = 'voice')

	if (before.voice.voice_channel is not None) & (after.voice.voice_channel is None):
		await bot.send_message(bot.log, after.name + ' left ' + before.voice.voice_channel.name)	
		await bot.remove_roles(after, voice_role)

	elif (before.voice.voice_channel != after.voice.voice_channel):
		await bot.send_message(bot.log, after.name + ' joined ' + after.voice.voice_channel.name)
		await bot.add_roles(after, voice_role)

@bot.event
async def on_member_update(before, after):
	if (after.game):
		if (after.game.name == 'Google Chrome'):
			print (after.name + ' is playing League!')
			get_league_game(after)

#================================================================================
# User commands
#================================================================================

@bot.command()
async def info():
	"""Display the Github link."""
	await bot.say('https://github.com/adrianau/Gideon')

@bot.command()
async def choose(*choices : str):
	"""Random a choice from the given arguments."""
	if choices:
		answer = random.choice(choices)
		chance = '{0:.0f}%'.format(choices.count(answer) / len(choices) * 100)
		await bot.say(answer + ' (' + chance + ')')
	else:
		await bot.say('No input given')

@bot.command()
async def seen(user):
	"""Output when a user last connected to the server."""
	member = member_from_alias(user)

	if member:
		if (member.status is not Status.offline):
			await bot.say(member.name + ' is currently online')
			return

		seen_time = find(member.id, 'seen.json')
		if seen_time:
			await bot.say(member.name + ' was last seen on ' + format_time(seen_time))
		else:
			await bot.say(member.name + ' not seen yet')
	else:
		await bot.say('User does not exist')

@bot.command(pass_context = True)
async def addalias(context, alias):
	"""Add an alias/nickname associated to the caller."""
	author = context.message.author
	member_id = find(alias, 'aliases.json')

	if (member_id is not None):
		member = discord.utils.get(bot.server.members, id = member_id)
		await bot.say('Alias ' +  '\'' + alias + '\'' + ' already exists for ' + member.name)
		return

	update_file('aliases.json', alias.lower(), author.id)
	await bot.say('Alias ' + '\'' + alias + '\'' + ' has been set for ' + author.name)

@bot.command(pass_context = True)
async def addsummoner(context, summoner):
	"""Add a league summoner associated to the caller."""
	author = context.message.author

	obj = { summoner: get_summoner_id(summoner) }

	with open ('league.json', 'r') as f:
		data = json.load(f)

		if author.id in data:
			if obj not in data[author.id]:
				data[author.id].append(obj)
		else:
			data[author.id] = [obj]

	os.remove('league.json')
	with open ('league.json', 'w') as f:
		json.dump(data, f, indent = 4)

	await bot.say(author.name + ' has added summoner ' + summoner)

@bot.command()
async def countdown(num = '3'):
	"""Send a tts countdown defaulting to 3 and capped at 10."""
	if num.isdigit():
		num = int(num)

		if num > 10:
			await bot.say('Countdown capped to 10')
			return

		voice_text = discord.utils.get(bot.server.channels, name = 'voice')
		message = ''

		for i in range (num, 0, -1):
			message += str(i) + '. '

		message += 'Go.'
		await bot.send_message(voice_text, message, tts= True)

	else:
		await bot.say('Invalid input')

@bot.command(pass_context = True)
async def join(context, role_str):
	"""Enable caller to join a game role."""
	if role_str in game_roles:
		role = discord.utils.get(bot.server.roles, name = role_str.lower())
		author = context.message.author
		await bot.add_roles(author, role)
		await bot.say(author.name + ' has been added to the ' + role.name + ' group')
	else:
		await bot.say('Invalid role')

@bot.command(pass_context = True)
async def leave(context, role_str):
	"""Enable caller to leave a role."""
	role = discord.utils.get(bot.server.roles, name = role_str.lower())

	if not role:
		await bot.say('Invalid role')
		return

	author = context.message.author

	if role in author.roles:
		await bot.remove_roles(author, role)
		await bot.say(author.name + ' has left the ' + role.name + ' group')
	else:
		await bot.say('Invalid role')

@bot.command()
async def viewrole(role_str):
	"""Display all members in a given role."""
	role = discord.utils.get(bot.server.roles, name = role_str.lower())
	text = '`` \n'

	for member in bot.server.members:
		if role in member.roles:
			text += member.name + '\n'

	text += '``'
	await bot.say(text)

#================================================================================
# Admin commands
#================================================================================

@bot.command(pass_context = True)
async def setalias(context, alias, user_str):
	"""Set an alias for a given user."""
	author = context.message.author
	if is_admin(author):
		member_id = find(alias, 'aliases.json')

		if (member_id is not None):
			member = discord.utils.get(bot.server.members, id = member_id)
			await bot.say('Alias ' +  '\'' + alias + '\'' + ' already exists for ' + member.name)
			return

		member = member_from_alias(user_str)

		update_file('aliases.json', alias.lower(), member.id)
		await bot.say('Alias ' + '\'' + alias + '\'' + ' has been set for ' + member.name)
	else:
		await bot.say('Insufficient privileges')

@bot.command(pass_context = True)
async def setsummoner(context, summoner, alias):
	"""Set a league summoner for a user."""
	author = context.message.author

	if is_admin(author):
		obj = { summoner: get_summoner_id(summoner)}
		member = member_from_alias(alias)

		with open ('league.json', 'r') as f:
			data = json.load(f)

			if member.id in data:
				if obj not in data[member.id]:
					data[member.id].append(obj)
			else:
				data[member.id] = [obj]

		os.remove('league.json')
		with open ('league.json', 'w') as f:
			json.dump(data, f, indent = 4)

		await bot.say('Added summoner ' + summoner + ' for ' + member.name)
	else:
		await bot.say('Insufficient privileges')

#================================================================================
# Utility functions
#================================================================================

def update_aliases():
	"""Update the aliases file with all the default member names and sort it."""
	for member in bot.server.members:
		update_file('aliases.json', (member.name).lower(), member.id)

	json_sort_value('aliases.json')

async def update_roles():
	"""Update all the member roles managed by the bot."""
	for role in bot_roles:
		role = discord.utils.get(bot.server.roles, name = role)

		if role.name == 'voice':
			for member in bot.server.members:
				if member.voice.voice_channel:
					await bot.add_roles(member, role)
				elif role in member.roles:
					await bot.remove_roles(member, role)

def update_seen(member, time):
	"""Update a member seen with the (current) time."""
	update_file('seen.json', member.id, str(time))

def format_time(dt):
	"""Format a datetime string like: 15 May 2017 at 9:01PM."""
	dto = datetime.strptime(dt, '%Y-%m-%d %H:%M:%S.%f')
	date = dto.strftime('%a %d %b %Y')
	time = dto.strftime('%I:%M%p').lstrip('0')

	return date + ' at ' + time

def member_from_alias(alias):
	"""Return a member object from a given alias."""
	id = find(alias.lower(), 'aliases.json')
	if id:
		return bot.server.get_member(id)
	else:
		return None

def is_admin(member):
	"""Return whether a member has admin privileges."""
	if member.server_permissions.administrator:
		return True
	else:
		return False

def json_sort_value(file):
	"""Sort and update a JSON by its values."""
	with open(file, 'r') as f:
		data = json.load(f)
		ordered_items = sorted(data.items(), key = lambda item: item[1])
		ordered_dict = OrderedDict(ordered_items)

		os.remove(file)
		with open (file, 'w') as f:
			json.dump(ordered_dict, f, indent = 4)

def get_summoner_id(summoner):
	"""Return summoner ID from summoner name."""
	if '+' in summoner:
		summoner = summoner.replace('+', '%20')

	data = requests.get('https://euw1.api.riotgames.com/lol/summoner/v3/summoners/by-name/' + summoner + '?api_key=' + config.lapi).json()
	return data['id']

def get_league_game(member):
	with open ('league.json', 'r') as f:
			data = json.load(f)

			if member.id in data:
				id_list = []

				for obj in data[member.id]:
					id_list.append(list(obj.values())[0])
				print(id_list)

				#for id in id_list:
				#	response = requests.get('https://euw1.api.riotgames.com/lol/spectator/v3/active-games/by-summoner/' + id + '?api_key=' + config.lapi)
				#	print(response.status_code)
				response = requests.get('https://euw1.api.riotgames.com/lol/spectator/v3/active-games/by-summoner/' + str(52204978) + '?api_key=' + config.lapi)
				print('should succeed:')
				print(response.status_code)
				response2 = requests.get('https://euw1.api.riotgames.com/lol/spectator/v3/active-games/by-summoner/' + str(36147400) + '?api_key=' + config.lapi)
				print('should fail:')
				print(response2.status_code)
			else:
				return
#================================================================================
# File management
#================================================================================

async def init_bot():
	"""Initialise files and necessary setup for the bot."""
	bot.server = list(bot.connection._servers.values())[0]
	bot.log = discord.utils.get(bot.server.channels, name = 'log')
	init_files()
	update_aliases()
	await update_roles()

def init_files():
	"""Creates necessary files if they don't exist"""
	for file in bot_files:
		if os.path.isfile(file):
			print(file + ' found!')
		else:
			print(file + ' not found')
			with open(file, 'w') as f:
				json.dump({}, f, indent = 4)

def update_file(file, key, value):
	"""Update a file with a new key/value pair."""
	with open(file, 'r') as f:
		data = json.load(f)
		data[key] = value

		os.remove(file)
		with open(file, 'w') as f:
			json.dump(data, f, indent = 4)

def find(key, file):
	"""Return the value of a key in a file."""
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
