import os
import json
from datetime import datetime

def init():
	files = ['aliases.json', 'seen.json']

	for file in files:

		if os.path.exists(file) is False:

			print(file +  ' not found')

			with open(file, 'w') as f:
				json.dump({}, f, indent = 4)

			print(file + ' created')

def update_seen(member, now):
	update_file('seen.json', member.id, str(now))

def update_file(file, key, value):
	with open(file, 'r') as f:
		data = json.load(f)
		data[key] = value

		os.remove(file)
		with open(file, 'w') as f:
			json.dump(data, f, indent = 4)

def find(key, file):
	with open(file, 'r') as f:
		data = json.load(f)

		found = True if key in data else False
		if found:
			return data[key]
		else:
			return None