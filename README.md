<img src="https://i.troplo.com/i/52c4f6a9f34d.png" width="300px">

# Subnautica: Below Zero Multiplayer (By BOT Benson) for Linux/NetBird Removed

This patch to the popular multiplayer mod for Subnautica BZ removes the following:
- VPN (Netbird) which does not support Linux, also poses security risks.
- Online requirement. You can now use Steam Offline Mode.
- Redirect calls from subnauticamultiplayer.com to custom endpoint (more on this later.)

Because the mod is proprietary (source available, however outdated.) Patches have been applied directly to the DLLs.
Are the patches good? Probably not.

## Original Author Acknowledgement
Thank you to BOT Benson for creating this mod for Subnautica BZ. I do not mean disrespect to the original mod author, however, I believe that forced dependencies
for something that should be optional isn't right. This should be a toggle or setting to disable the VPN, as it is not required for people who port forward.

Connecting ALL players of the mod together in one shared bucket, to me, seems like a privacy/security risk, as services running on said computer might be vulnerable 
and therefore will be exposed to anyone playing the mod where a regular NAT router wouldn't allow access unless explicitly port forwarded. However, please correct me if my understanding of how the VPN service works is flawed.

## Instructions
1. Download the latest release at <a href="https://github.com/Troplo/Subnautica-BZ-Multiplayer-Linux/releases">releases</a>.
2. Extract the ZIP file
3. Move all the folders/files in `Game Folder` to your `SubnauticaZero` install location. You can find this by right clicking Subnautica: Below Zero in Steam, selecting "Properties" -> "Installed Files" -> Browse.
4. Move `.botbenson` to `drive_c/users/steamuser/AppData/Roaming` in the Proton/WINE Prefix. For me, this was `.local/share/Steam/steamapps/compatdata/848450/pfx/drive_c/users/steamuser/AppData/Roaming/` (Please run Subnautica BZ at least once to create this folder)
5. Right click "Subnautica: Below Zero" in Steam, select "Properties," and in the "General" tab, update your "Launch Options" to the following:
  `-peerIp LOCAL_OR_REMOTE_IP_ADDRESS -peerId RANDOM_UNIQUE_PEER_ID -userId RANDOM_UNIQUE_ID -username YOUR_USERNAME` You will need to update these values.
- `-peerIp` If you are in a LAN (Local Area Network)/same network, please enter your Local IP address of the computer. If you are playing remotely through the internet, please enter your public IP address. (IPv4) **Example: `-peerIp 192.168.0.12`
- `-peerId` This is a random string, make sure it's unique between all players. **NO SPACES.** Example: `-peerId troplo:connectIP:127.0.0.1` (connectIP is a special literal so your game doesn't try to connect to your public IP if you're not port forwarding yet, or have NAT reflection issues. You can replace 127.0.0.1 with any IP address. This is used when creating/hosting a server.)
- `-userId` This is a random numerical ID, make sure it's unique between all players. Example: `-userId 1`
- `-username` This is your desired in-game username. Make sure it's unique between all players. **NO SPACES** Example: `-username Troplo`
6. Launch Subnautica BZ and Enjoy! **ALL PLAYERS NEED TO FOLLOW THIS**

## OPTIONAL: Setup fully offline API server
To play 100% offline, you need to setup an API server, as the mod by default connects to the mod author's server to provide the Invite Code service.
Because there's a check to make sure you're connected to the VPN. I had to host a custom reimplementation of this service at `subbz-api.troplo.com` to make sure it doesn't send an error when joining/hosting.
You can find the reimplementation source code <a href="https://github.com/Troplo/Subnautica-BZ-Multiplayer-API">here.</a>
- To make it used by the clients pass in `-apiEndpoint https://api.yourdomain.com/api/` (example: `-apiEndpoint https://subbz-api.troplo.com/api/`)
- You can use insecure HTTP by replacing https with http.
