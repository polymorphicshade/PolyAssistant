# 📃 Requirements
- Ubuntu Server (at least 24.04.01)
- A GPU with CUDA cores and at least 8 GB of VRAM (tested with an RTX3090)

# 🚀 Getting Started
1. Clone and enter the repository:
   ```
   git clone https://github.com/polymorphicshade/PolyAssistant && cd PolyAssistant
   ```
2. Run the setup script in the `PolyAssistant` repository:
    ```bash
    ./setup.sh
    ```
    > [!NOTE]  
    > Running `setup.sh` will also update/upgrade your Ubuntu Server OS.
3. Wait for the machine to reboot, then in the `PolyAssistant` repository, run:
    ```bash
    ./start.sh
    ```
    > [!WARNING]
    > Running `start.sh` for the first time might take several (>30) mintues because lots of things need to be downloaded and installed.

# 💻 Proxmox
If you are running an Ubuntu Server guest in a Proxmox, these commands might help:

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