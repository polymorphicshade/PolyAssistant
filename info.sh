#!/bin/bash

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

host_ip=$(hostname -I | awk '{print $1}')

nginx_container_id=$(docker ps -f "ancestor=nginx" --format "{{.ID}}" | head -n 1)

echo
echo "HTTP:"
echo

docker ps --format "{{.Names}}" | sort | while read container_name; do
    exposed_port=$(docker port "$container_name" | cut -d ':' -f 2)

    if [[ -z "$exposed_port" ]]; then
        continue
    fi

    echo "http://$host_ip:$exposed_port ($container_name)"
done

if [ -n "$nginx_container_id" ]; then
  echo
  echo "HTTPS:"
  echo
  docker exec "$nginx_container_id" nginx -T 2>&1 | grep -oP 'location\s+\K\/[^\{]+' | sed "s/^/https:\/\/$host_ip/"
fi

echo