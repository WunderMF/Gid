import fileman
from datetime import datetime

def get_time():
	return datetime.now()

def format_time(dt):
	dto = datetime.strptime(dt, '%Y-%m-%d %H:%M:%S.%f')
	date = dto.strftime('%a %d %b %Y')
	time = dto.strftime('%I:%M%p').lstrip('0')

	return date + ' at ' + time

def member_from_alias(alias, server):
	id = fileman.find(alias.lower(), 'aliases.json')

	if id:
		return server.get_member(id)
	else:
		return None

def bt(message):
	return '`' + message + '`'