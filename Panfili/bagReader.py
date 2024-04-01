import bagpy
from bagpy import bagreader
import pandas as pd

b = bagreader(r'Z:\20240208_140628.bag')

data = {'info':{},'topic':[]}
data['info']['type'] = b.reader.get_type_and_topic_info()[0]
data['info']['topic'] = b.reader.get_type_and_topic_info()[1]
data['topic'] = list(data['info']['topic'].keys())
# data['depth'] = pd.read_csv(b.message_by_topic(data['topic'][1]))
 

for topic, message, t in a:
    depth = message
    print(1)


# replace the topic name as per your need
LASER_MSG = b.message_by_topic('/vehicle/front_laser_points')
df_laser = pd.read_csv(LASER_MSG)