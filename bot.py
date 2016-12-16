import config
import discord
import asyncio
from discord.ext import commands

import time

# aliases
bot = commands.Bot(command_prefix = '!')
status = discord.member.Status

@bot.event # on ready
async def on_ready():
	print('Connected')
	print('------')


@bot.event # online / offline message
async def on_member_update(before, after):
	c = before.server.default_channel

	if (before.status != status.offline) & (after.status == status.offline):
		await bot.send_message(c, '`' + after.name + ' is now offline' + '`')
	elif (before.status == status.offline) & (after.status != status.offline):
		await bot.send_message(c, '`' + after.name + ' is now online' + '`')


@bot.event # join / leave channel message
async def on_voice_state_update(before, after):
	c = before.server.default_channel
	bc = before.voice.voice_channel
	ac = after.voice.voice_channel

	if (bc is not None) & (ac is None):
		await bot.send_message(c, '`' + after.name + ' left ' + bc.name +'`')
	elif bc != ac:
		await bot.send_message(c, '`' + after.name + ' joined ' + ac.name +'`')


@bot.command() # shows github link
async def info():
	await bot.say('https://github.com/adrianau/Gideon')


@bot.command() # adds two numbers
async def add(left : int, right : int):
	if int(left) & int(right):
		bot.say(left + right)
	else: 
		bot.say('both need to be ints')
	

@bot.command(pass_context = True) # testing
async def test(context):
	member = context.message.author
	current_time = time.localtime()
	formatted = time.strftime('%a, %d %b %Y %H:%M:%S GMT', current_time)
	await bot.say(formatted)


# run bot
bot.run(config.token)