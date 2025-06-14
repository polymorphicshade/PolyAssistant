#!/bin/bash

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

# disable needrestart prompt
sed -i "s/#\$nrconf{kernelhints} = -1;/\$nrconf{kernelhints} = -1;/g" /etc/needrestart/needrestart.conf

apt update -y

if [ "$1" == "vm" ]; then
    systemctl mask sleep.target suspend.target hibernate.target hybrid-sleep.target
    apt install -y qemu-guest-agent
    systemctl enable qemu-guest-agent
fi

# TODO: should we do this before installing Nvidia stuff?
# apt autoremove nvidia* --purge -y

# install Nvidia stuff
wget https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2404/x86_64/cuda-ubuntu2404.pin
sudo mv cuda-ubuntu2404.pin /etc/apt/preferences.d/cuda-repository-pin-600
wget https://developer.download.nvidia.com/compute/cuda/12.9.1/local_installers/cuda-repo-ubuntu2404-12-9-local_12.9.1-575.57.08-1_amd64.deb
sudo dpkg -i cuda-repo-ubuntu2404-12-9-local_12.9.1-575.57.08-1_amd64.deb
sudo cp /var/cuda-repo-ubuntu2404-12-9-local/cuda-*-keyring.gpg /usr/share/keyrings/
sudo apt-get update
sudo apt-get -y install cuda-toolkit-12-9
sudo apt-get install -y nvidia-open # OR cuda-drivers (proprietary)

# install Docker stuff
apt update -y
apt install -y ca-certificates curl
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
apt update -y
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose docker-compose-plugin

# configure Docker to work with Nvidia
curl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg
curl -s -L https://nvidia.github.io/libnvidia-container/stable/deb/nvidia-container-toolkit.list | sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | tee /etc/apt/sources.list.d/nvidia-container-toolkit.list
apt update -y
apt install -y nvidia-container-toolkit
nvidia-ctk runtime configure --runtime=docker
systemctl restart docker

# upgrade the system (suppress "restart service" prompt)
DEBIAN_FRONTEND=noninteractive apt upgrade -y

# re-enable needrestart prompt
sed -i "s/\$nrconf{kernelhints} = -1;/#\$nrconf{kernelhints} = -1;/g" /etc/needrestart/needrestart.conf

# generate certs for Nginx
mkdir config/nginx/certs
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout nginx/certs/key.key -out nginx/certs/cert.crt -subj "/C=US/ST=GenericState/L=GenericCity/O=GenericOrg/OU=GenericUnit/CN=generichost.com/emailAddress=generic@example.com"

# clone other repos
mkdir .repos
git clone https://github.com/polymorphicshade/PolyAssistant.Chatterbox.git .repos/PolyAssistant.Chatterbox
git clone https://github.com/polymorphicshade/PolyAssistant.FramePack-Studio.git .repos/PolyAssistant.FramePack-Studio
git clone https://github.com/polymorphicshade/PolyAssistant.StableDiffusionWebUi.git .repos/PolyAssistant.StableDiffusionWebUi
git clone https://github.com/polymorphicshade/PolyAssistant.Zonos.git .repos/PolyAssistant.Zonos

# make a directory for PolyAssistant data
mkdir .data

# a reboot is required
reboot