import os
import json

file = 'aliases.json'

def add(alias, userID):
	with open (file, 'r') as f:
		data = json.load(f)
		data[alias] = userID

	os.remove(file)
	with open(file, 'w') as f:
		json.dump(data, f, indent = 4)