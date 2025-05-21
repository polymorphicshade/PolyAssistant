# 📃 Requirements
- Ubuntu Server (at least 24.04.01)
- A GPU with CUDA cores and at least 8 GB of VRAM (tested with an RTX3090)

# ⚡ Automatic Setup
1. Run the setup script:
    ```bash
    ./setup.sh
    ```
2. Wait for the machine to reboot, then run:
    ```bash
    ./start.sh
    ```
> [!NOTE]  
> This script will also update/upgrade your Ubuntu Server OS.

# 🛠️ Manual Setup
2. Install Nvidia stuff:
    ```bash
    sudo apt update -y && \
    sudo apt install -y ubuntu-drivers-common && \
    sudo ubuntu-drivers autoinstall && \
    sudo reboot
    ```
3. Install Docker stuff:
    ```bash
    sudo apt update && \
    sudo apt install -y ca-certificates curl && \
    sudo install -m 0755 -d /etc/apt/keyrings && \
    sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc && \
    sudo chmod a+r /etc/apt/keyrings/docker.asc && \
    sudo echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    sudo apt update && \
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose docker-compose-plugin
    ```
4. Configure Docker with Nvidia:
    ```bash
    curl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg && \
    curl -s -L https://nvidia.github.io/libnvidia-container/stable/deb/nvidia-container-toolkit.list | \
    sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | \
    sudo tee /etc/apt/sources.list.d/nvidia-container-toolkit.list && \
    sudo apt update -y && \
    sudo apt install -y nvidia-container-toolkit && \
    sudo nvidia-ctk runtime configure --runtime=docker && \
    sudo systemctl restart docker
    ```
5. Verify Docker and Nvidia work together:
    ```bash
    sudo docker run --rm --runtime=nvidia --gpus all ubuntu nvidia-smi
    ```
    If everything is working, you should see your GPU in the `nvidia-smi` output.

# 💻 Proxmox
If you are running an Ubuntu Server guest in a Proxmox host, these commands might help:

- Update and install the QEMU guest agent:
    ```bash
    sudo apt update -y && \
    sudo apt install -y qemu-guest-agent && \
    sudo systemctl start qemu-guest-agent && \
    sudo reboot
    ```
- Prevent sleep/suspend:
    ```bash
    sudo systemctl mask sleep.target suspend.target hibernate.target hybrid-sleep.target
    ```
> [!TIP]
> If you are passing through your GPU, make sure you enable "All Functions" in settings.
