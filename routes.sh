#!/bin/bash

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

NGINX_CONTAINER=$(docker ps --format "{{.ID}}\t{{.Image}}" | grep -E "nginx" | head -n 1 | awk '{print $1}')

if [ -z "$NGINX_CONTAINER" ]; then
    echo "No running Nginx container found."
else
    echo "Found Nginx container: $NGINX_CONTAINER"
    echo "--- Configured Nginx Routes ---"
    docker exec "$NGINX_CONTAINER" find /etc/nginx/ -name "*.conf" -exec grep -hE '^\s*location\s*(\S+)\s*\{' {} \; | sed -E 's/^\s*(location\s*.*)\s*\{/\1/g' | sort -u
    echo "-------------------------------"
fi