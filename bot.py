import config
import discord
import json
import os
import random
import math
from datetime import datetime
from discord.ext import commands
from discord.member import Status

bot = commands.Bot(command_prefix = '!')

@bot.event
async def on_ready():
	init_data()
	init_flags()
	set_server('GGT')
	set_log('log')
	await update_voice_role()
	print ('Connected')

@bot.command() # displays github link
async def info():
	await bot.say('https://github.com/adrianau/Gideon/tree/python')

@bot.command() # randomly chooses from a given list
async def choose(*choices : str):
	if choices:
		answer = random.choice(choices)
		chance = '{0:.0f}%'.format(choices.count(answer) / len(choices) * 100)
		await bot.say( bt(answer + ' (' + chance + ')') )
	else:
		await bot.say( bt('No inputs given') )

@bot.event # logs online and offline status
async def on_member_update(before, after):
	if (before.bot == False):
		if (before.status != Status.offline) & (after.status == Status.offline):
			await bot.send_message(log, bt(after.name + ' is now offline') )
			update_seen(before)
		elif (before.status == Status.offline) & (after.status != Status.offline):
			await bot.send_message(log, bt(after.name + ' is now online') )

@bot.event # logs joining and leaving voice channels
async def on_voice_state_update(before, after):
	voice_role = get('voice', server.roles)

	if (before.voice.voice_channel is not None) & (after.voice.voice_channel is None):
		await bot.send_message(log, bt(after.name + ' left ' + before.voice.voice_channel.name) )
		await bot.remove_roles(after, voice_role)
	elif (before.voice.voice_channel != after.voice.voice_channel):
		await bot.send_message(log, bt(after.name + ' joined ' + after.voice.voice_channel.name) )
		await bot.add_roles(after, voice_role)

@bot.command(pass_context = True)
async def votemute(context, member: discord.Member = None):
	global vote_in_progress
	
	# preliminary checks
	if (member is None):
		await bot.say ( bt('User not found') )

	if (member.voice.voice_channel is None):
		await bot.say( bt('User not in a voice channel') )
		return

	if (vote_in_progress):
		await bot.say ( bt('There is a vote in progress') )
		return

	vote_in_progress = True

	if (member.voice.mute):
		vote_type = 'unmute'
	else:
		vote_type = 'mute'

	voice_clients = 0
	for m in server.members:
		if (m.voice.voice_channel):
			voice_clients += 1

	votes_needed = math.floor(voice_clients/2) + 1
	
	await bot.say( bt('Votes required to ' + vote_type + ' ' + member.name + ': ' + str(votes_needed + 1)) )
	result = await vote(context.message, votes_needed)

	if (result is None):
		await bot.say ( bt('Vote has ended') )
	elif (member.voice.mute):
		await bot.server_voice_state(member, mute = False)
		await bot.say( bt(member.name + ' has been unmuted') )
	elif (member.voice.mute is False):	
		await bot.server_voice_state(member, mute = True)
		await bot.say( bt(member.name + ' has been muted') )

	vote_in_progress = False

async def vote(message, votes_needed):
	upvotes = 0
	voted_users = []
	await bot.add_reaction(message, '👍' )

	def reaction_check(reaction, user):
		nonlocal upvotes
		nonlocal votes_needed

		if (user.bot == False) & (user not in voted_users):
			print ('received ' + reaction.emoji + ' from ' + user.name)

			if reaction.emoji == '👍':
				upvotes += 1
				voted_users.append(user)

			if (upvotes == votes_needed):
				return True

	return await bot.wait_for_reaction(timeout = 60, check = reaction_check)

@bot.command() # checks when member was last online
async def seen(message):
	member = server.get_member_named(message)

	if (member is None):
		await bot.say( bt('User not found') )
	else:
		with open(seen_data, 'r') as f:
			data = json.load(f)

			# check if member is in the data
			in_data = True if member.name in data else False
			if (in_data):
				await bot.say( bt(member.name + ' was last seen on ' + data[member.name]) )
			else:
				await bot.say( bt(member.name + ' not seen yet') )

# UTILITY FUNCTIONS #

# gets an object by name from an iterable list
def get(object_name, list):
	return discord.utils.get(list, name = object_name)

# sets the global server variable
def set_server(server_name):
	global server
	server = get(server_name, bot.servers)

# sets the channel to output the server logs
def set_log(channel_name):
	global log
	log = get(channel_name, server.channels)

# initialises data files
def init_data():
	global seen_data
	seen_data = 'seen.json'

	if (os.path.isfile(seen_data)) == False:
		with open(seen_data, 'w') as f:
			json.dump({}, f, indent = 4)

# initialises global flags
def init_flags():
	global vote_in_progress
	vote_in_progress = False

# gets current time (formatted)
def get_time():
	now = datetime.now()
	date = now.strftime('%a %d %b %Y')
	time = now.strftime('%I:%M%p').lstrip('0')
	return date + " at " + time

# updates seen data
def update_seen(member):

	# reads and updates json
	with open(seen_data, 'r') as f:
		data = json.load(f)
		data[member.name] = get_time()

	# remove the old json and write new file
	os.remove(seen_data)
	with open(seen_data, 'w') as f:
		json.dump(data, f, indent = 4)

# manually updates the members of the voice role when called
async def update_voice_role():
	voice_role = get('voice', server.roles)
	for member in server.members:
		if (member.voice.voice_channel):
			await bot.add_roles(member, voice_role)
		elif (voice_role in member.roles):
			await bot.remove_roles(member, voice_role)

# embeds a message in backticks
def bt(message):
	return '`' + message + '`'

# run bot
bot.run(config.token)
