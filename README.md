# 📃 Requirements
- Ubuntu Server (at least 24.04)
- At least any modern mid-range CPU
- At least 300 GB of free storage (1 TB recommended for scalability with new models and features)
- A GPU with CUDA cores and at least 8 GB of VRAM (tested with an RTX3090)

# 🚀 Getting Started
1. Clone and enter the repository:
   ```
   git clone https://github.com/polymorphicshade/PolyAssistant
   ```
2. Run the installation script (this will also update and reboot your system):
    ```bash
    ./setup.sh
    ```
3. Wait for the machine to reboot, then run the stack:
    ```bash
    ./start.sh
    ```
> [!NOTE]
> Running `start.sh` for the first time might take several (>30) mintues because lots of things need to be downloaded and installed.

# 💻 Proxmox
If you are using Proxmox to run an Ubuntu Server virtual machine, these might help:

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
- These scripts are very useful: https://community-scripts.github.io/ProxmoxVE/scripts
> [!TIP]
> If you are passing through your GPU, make sure you enable "All Functions" in settings.