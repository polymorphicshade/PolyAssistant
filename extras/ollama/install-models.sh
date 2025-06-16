#!/bin/bash

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

OLLAMA_CONTAINER_ID=$(docker ps -q --filter "name=ollama")

if [ -z "$OLLAMA_CONTAINER_ID" ]; then
  echo "No running Docker container named 'ollama' found."
else
  docker exec -it "$OLLAMA_CONTAINER_ID" ollama pull llama3.2:latest
  docker exec -it "$OLLAMA_CONTAINER_ID" ollama pull mistral-small:24b
fi
