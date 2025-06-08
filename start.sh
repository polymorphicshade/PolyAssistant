#!/bin/bash

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

# run docker (will also run at start-up)
docker compose pull
docker compose "$@" up -d --no-deps --build

# print the containers and their access points
./info.sh