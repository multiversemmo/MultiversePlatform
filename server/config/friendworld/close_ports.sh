#!/bin/sh

# requires NOPASSWD sudo access
# get ports from multiverse.properties
# be less invasive; user-defined chain and don't flush (-F) all of INPUT

LOGIN_PORT=$1
PROXY_PORT=$2
VOICE_PORT=$3

PORT_LIST=$1,$2,$3

sudo /sbin/iptables -F INPUT

sudo /sbin/iptables -A INPUT -p tcp -s 127.0.0.1/32 -m multiport --dports $PORT_LIST -j ACCEPT
sudo /sbin/iptables -A INPUT -p udp -s 127.0.0.1/32 -m multiport --dports $PORT_LIST -j ACCEPT
sudo /sbin/iptables -A INPUT -p tcp -s 192.168.10.0/24 -m multiport --dports $PORT_LIST -j ACCEPT
sudo /sbin/iptables -A INPUT -p udp -s 192.168.10.0/24 -m multiport --dports $PORT_LIST -j ACCEPT
sudo /sbin/iptables -A INPUT -p tcp -s 192.168.20.0/24 -m multiport --dports $PORT_LIST -j ACCEPT
sudo /sbin/iptables -A INPUT -p udp -s 192.168.20.0/24 -m multiport --dports $PORT_LIST -j ACCEPT

sudo /sbin/iptables -A INPUT -p tcp -m multiport --dports $PORT_LIST -j REJECT
sudo /sbin/iptables -A INPUT -p udp -m multiport --dports $PORT_LIST -j REJECT

sudo /sbin/iptables -n -L
