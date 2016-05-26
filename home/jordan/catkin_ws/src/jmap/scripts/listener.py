#!/usr/bin/env python
# Software License Agreement (BSD License)
#
# Copyright (c) 2008, Willow Garage, Inc.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions
# are met:
#
#  * Redistributions of source code must retain the above copyright
#    notice, this list of conditions and the following disclaimer.
#  * Redistributions in binary form must reproduce the above
#    copyright notice, this list of conditions and the following
#    disclaimer in the documentation and/or other materials provided
#    with the distribution.
#  * Neither the name of Willow Garage, Inc. nor the names of its
#    contributors may be used to endorse or promote products derived
#    from this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
# "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
# LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
# FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
# COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
# INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
# BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
# LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
# CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
# LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
# ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
# POSSIBILITY OF SUCH DAMAGE.
#
# Revision $Id$

#########
# ODOM DATA TYPE
# std_msgs/Header header
#	uint32 seq
#	time stamp
#	string frame_id
# string child_frame_id
# geometry_msgs/PoseWithCovariance pose
# 	geometry_msgs/Pose pose
#		geometry_msgs/Point position <----------**RELEVANT**
#			float64 x, y, z
#		geometry_msgs/Quaternion orientation <-----**RELEVANT**
#			float64 x, y, z, w
#	float64[36] covariance
# Geometry_msgs/TwistWithCovariance twist
#	geometry_msgs/Twist twist
#		geometry_msgs/Vector3 linear
#			float64 x,y,z
#		geometry_msgs/Vector3 angular
#			float64 x,y,z
#	float64[36] covariance
############

############
# BASE SCAN DATA TYPE
# std_msgs/Header header
#	uint32 seq
#	time stamp
#	string frame_id
# float32 angle_min
# float32 angle_max
# float32 angle_increment
# float32 time_increment
# float32 scan_time
# float32 range_min
# float32 range_max
# float32[] ranges
# float32[] intensities
#########################

#test comment for git updating!
#modified the file!


import rospy
import os
import math
from std_msgs.msg import String
from nav_msgs.msg import Odometry
from sensor_msgs.msg import LaserScan
from geometry_msgs.msg import Point, Quaternion
def odomCallback(data):
	f = open('/home/jordan/jMap/jMapUnity/robotinfile.txt','w') #this clears the file too!
	g = open('/home/jordan/jMap/jMapUnity/rotationinfile.txt','w') #also clears the filer
	#rospy.loginfo(data.pose.pose.position)
	f.write(str(data.pose.pose.position.x))
	f.write(" ")
	f.write(str(data.pose.pose.position.y))
	f.write(" ")
	f.write(str(data.pose.pose.position.z))
	f.write(" ")
	f.write(str(data.header.frame_id))
	f.write('\n')
	g.write(str(data.pose.pose.orientation.x))
	g.write(" ")
	g.write(str(data.pose.pose.orientation.y))
	g.write(" ")
	g.write(str(data.pose.pose.orientation.z))
	g.write(" ")
	g.write(str(data.pose.pose.orientation.w))
	g.write("\n")
	f.close()
	g.close()

def laserCallback(data):
	print("LASER CALLBACK:")
	h = open('/home/jordan/jMap/jMapUnity/basescaninfile.txt','w')
	instructor = open('/home/jordan/jMap/jMapUnity/infilerefresh.txt','w')
	instructor.write(str(data.header.seq))
	instructor.close()
	angle = data.angle_min
	for foundRange in data.ranges:
		#print("ANGLE:", angle, "RANGE:", foundRange)
		#print data.angle_max
		h.write(str(angle))
		h.write(" ")
		if(math.isnan(foundRange)):
			h.write("-1")
		else:
			h.write(str(foundRange))
		h.write('\n')
		angle += data.angle_increment
		#print intensity;
	h.close()


def listener():
    print("listener was activated!")
    #global dontPrintCount
    #dontPrintCount = 0
    # In ROS, nodes are uniquely named. If two nodes with the same
    # name are launched, the previous one is kicked off. The
    # anonymous=True flag means that rospy will choose a unique
    # name for our 'listener' node so that multiple listeners can
    # run simultaneously.

    f = open('/home/jordan/jMap/jMapUnity/robotinfile.txt','w')
    f.write("")
    f.close()

    g = open('/home/jordan/jMap/jMapUnity/rotationinfile.txt','w')
    g.write("")
    g.close()

    rospy.init_node('listener', anonymous=True)



    rospy.Subscriber('/odom', Odometry, odomCallback)
    rospy.Subscriber('/base_scan', LaserScan, laserCallback)

    # spin() simply keeps python from exiting until this node is stopped
    rospy.spin()
    

if __name__ == '__main__':
    listener()
