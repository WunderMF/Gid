import config
import discord
import json
import os
from datetime import datetime
from discord.ext import commands

bot = commands.Bot(command_prefix = '!')

@bot.event
async def on_ready():
	init_data()
	set_server('GGT')
	set_log('log')
	print ('Connected')

@bot.command() # displays github link
async def info():
	await bot.say('https://github.com/adrianau/Gideon/tree/python')

@bot.event # logs online and offline status
async def on_member_update(before, after):
	if (before.status != discord.member.Status.offline) & (after.status == discord.member.Status.offline):
		await bot.send_message(log, bt(after.name + ' is now offline') )
		update_seen(before)
	elif (before.status == discord.member.Status.offline) & (after.status != discord.member.Status.offline):
		await bot.send_message(log, bt(after.name + ' is now online') )

@bot.event # logs joining and leaving voice channels
async def on_voice_state_update(before, after):
	if (before.voice.voice_channel is not None) & (after.voice.voice_channel is None):
		await bot.send_message(log, bt(after.name + ' left ' + before.voice.voice_channel.name) )
	elif (before.voice.voice_channel != after.voice.voice_channel):
		await bot.send_message(log, bt(after.name + ' joined ' + after.voice.voice_channel.name) )

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

# sets the global server variable
def set_server(server_name):
	global server
	server = discord.utils.get(bot.servers, name = server_name)

# sets the channel to output the server logs
def set_log(channel_name):
	global log
	log = discord.utils.get(server.channels, name = channel_name)

# initialises data files
def init_data():
	global seen_data
	seen_data = 'seen.json'

	if (os.path.isfile(seen_data)) == False:
		with open(seen_data, 'w') as f:
			json.dump({}, f, indent = 4)

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

# embeds a message in backticks
def bt(message):
	return '`' + message + '`'

# run bot
bot.run(config.token)
