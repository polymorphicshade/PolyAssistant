#!/bin/bash

#
# TODO: remove this script when SD can be run in a container
#

# require sudo
if [ "$(id -u)" -ne 0 ]; then
    exec sudo "$0" "$@"
    exit 1
fi

# setup stable-diffusion-webui
# NOTE: the directory path CANNOT have a '.' in it (i.e. BAD -> /path/to/.some/folder/stable-diffusion-webui)
apt install software-properties-common
add-apt-repository --yes ppa:deadsnakes/ppa
apt-get update -y
apt-get install -y libgl1 git libgoogle-perftools4 libtcmalloc-minimal4 python3.10-venv
git clone https://github.com/AUTOMATIC1111/stable-diffusion-webui /home/user/stable-diffusion-webui
cp webui-user.sh /home/user/stable-diffusion-webui/webui-user.sh
cd /home/user/stable-diffusion-webui
sudo -u \#1000 python3.10 -m venv venv

# install some extensions
cd extensions
git clone https://github.com/papuSpartan/stable-diffusion-webui-auto-tls-https
git clone https://github.com/huchenlei/sd-webui-api-payload-display
git clone https://github.com/AUTOMATIC1111/stable-diffusion-webui-rembg
git clone https://github.com/pkuliyi2015/multidiffusion-upscaler-for-automatic1111
git clone https://github.com/continue-revolution/sd-webui-animatediff
git clone https://github.com/Bing-su/adetailer
git clone https://github.com/Gourieff/sd-webui-reactor-sfw
git clone https://github.com/glucauze/sd-webui-faceswaplab
git clone https://github.com/continue-revolution/sd-webui-segment-anything
git clone https://github.com/alemelis/sd-webui-ar
git clone https://github.com/Bing-su/adetailer
git clone https://github.com/hako-mikan/sd-webui-regional-prompter
git clone https://github.com/huchenlei/sd-webui-openpose-editor
# not working: git clone https://github.com/Iyashinouta/sd-model-downloader
cd ..

# install ControlNet models
mkdir -p models/ControlNet
cd models/ControlNet
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11e_sd15_ip2p.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11e_sd15_shuffle.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11f1e_sd15_tile.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11f1p_sd15_depth.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_canny.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_inpaint.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_lineart.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_mlsd.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_normalbae.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_openpose.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_scribble.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_seg.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15_softedge.pth
wget https://huggingface.co/lllyasviel/ControlNet-v1-1/resolve/main/control_v11p_sd15s2_lineart_anime.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_canny_sd15v2.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_color_sd14v1.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_depth_sd15v2.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_keypose_sd14v1.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_openpose_sd14v1.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_seg_sd14v1.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_sketch_sd15v2.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_style_sd14v1.pth
wget https://huggingface.co/TencentARC/T2I-Adapter/resolve/main/models/t2iadapter_zoedepth_sd15v1.pth

# give ownership to 1000 (matches the service permissions below)
sudo chown -R 1000:1000 /home/user/stable-diffusion-webui

# install stable-diffusion-webui service
# NOTE: must NOT run as root
echo "[Unit]
Description=Stable Diffusion WebUI
After=network.target

[Service]
ExecStart=/home/user/stable-diffusion-webui/webui.sh
User=1000
Group=1000

[Install]
WantedBy=default.target" > /etc/systemd/system/stable-diffusion-webui.service

# TODO: configure webui-user.sh
#
# ex) 'export COMMANDLINE_ARGS="--allow-code --listen --port 9999 --gradio-auth user:pass --api --administrator --disable-tls-verify --xformers --reinstall-xformers --theme=dark --no-prompt-history --enable-insecure-extension-access"' > ...?
# ex) copy over pre-configured file from repo
#