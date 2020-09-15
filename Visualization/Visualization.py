import matplotlib.pyplot as plt
import csv
from itertools import cycle 


cycol = cycle('bgrcmk') 

repository = '../SystematicCapacity.AbstractCapacityModel/Solution'

data = list()
vehicle_color = dict()

f = open(repository + '/movement_result.csv')
for item in csv.DictReader(f):
	data_item = dict()
	data_item['ID'] = item['ID']
	data_item['FromLoc'] = int(item['FromLoc'])
	data_item['ToLoc'] = int(item['ToLoc'])
	data_item['FromTime'] = int(item['FromTime'])
	data_item['ToTime'] = int(item['ToTime'])
	data_item['VehicleID'] = item['VehicleID']

	if data_item['FromTime'] == data_item['ToTime']:
		data_item['FromTime'] -= 1
		data_item['ToTime'] += 1

	data.append(data_item)

	if item['VehicleID'] not in vehicle_color:
		vehicle_color[item['VehicleID']] = next(cycol)

for data_item in data:
	plt.plot([data_item['FromTime'], data_item['ToTime']], [data_item['FromLoc'], data_item['ToLoc']], c=vehicle_color[data_item['VehicleID']])

plt.show()

print(repository)



