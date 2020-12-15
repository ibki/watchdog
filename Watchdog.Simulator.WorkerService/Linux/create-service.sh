#!/bin/bash

# Move a service file
cp ./watchdog-simulator.service /etc/systemd/system/

# Create a service 
sudo chmod 0644 /etc/systemd/system/watchdog-simulator.service
sudo systemctl enable watchdog-simulator.service

# Start a service
sudo systemctl start watchdog-simulator.service
#sudo service watchdog-simulator start

# Determine a service's status
#sudo systemctl status watchdog-simulator.service