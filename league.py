import config
import json
import requests
import os
from server import Server
from bot import server

file = 'league.json'

def check():
	print(server.lq)

def get_id(summoner, userID):

	if '+' in summoner:
		summoner = summoner.replace('+', '%20')

	data = requests.get('https://euw1.api.riotgames.com/lol/summoner/v3/summoners/by-name/' + summoner + '?api_key=' + config.lapi).json()
	return data['id']

def add(summoner, userID):

	obj = { summoner: get_id(summoner, userID) }

	with open (file, 'r') as f:
		data = json.load(f)

		if userID in data:
			if obj not in data[userID]:
				data[userID].append(obj)
		else:
			data[userID] = [obj]

	os.remove(file)
	with open(file, 'w') as f:
		json.dump(data, f, indent = 4)

def ingame(summoner):
	id = get_id(summoner)
	r = requests.head('https://euw1.api.riotgames.com/lol/spectator/v3/active-games/by-summoner/' + id + '?api_key=' + config.lapi)