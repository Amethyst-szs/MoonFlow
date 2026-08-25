---
tags:
  - tools
  - starter_guide
---
# FTP Syncing
FTP *(File-Transfer Protocol)* allows you to wirelessly transfer files between devices over the internet! This can be done over long distances to a specific IP address, or just on your local router. Using FTP is extremely useful when developing Super Mario Odyssey mods on official hardware, as it lets you quickly transfer your project files onto the console for testing.

MoonFlow has integration as an FTP client, allowing easy-to-use file transfers! Though to get started with this feature, you'll need to setup your switch as an FTP server!

## Setting up your Switch as a Server
There are many options for setting up FTP on console. My personal recommendation will always be [sys-ftpd](https://github.com/cathery/sys-ftpd), which runs as a sysmodule in the background. This means the FTP server will be available without opening any apps on the switch, as long as the device is powered on!

Make sure to follow the installation instructions on your preferred Nintendo Switch FTP server's download page. There are various FTP clients you can use to test the connection, like [WinSCP](https://winscp.net/eng/index.php) and [FileZilla](https://filezilla-project.org/) if you're on windows.

## Connecting MoonFlow
In the settings page of MoonFlow is an **"FTP Syncing"** tab. Here you'll need to input your switch's IP address or domain name, port, username, and password. Your Nintendo Switch's IP address can be found in `System Settings -> Internet -> Connection Status -> IP Address.` The other details will be determined by your FTP server's config during setup.

Once you have all the login details entered, attempt to connect! If all goes to plan, you'll get a successful connection to your switch. Now all your project's files will be synced automatically.

Below the login info are additional options to configure how your files are synced, make sure to set these up to your preference and environment.

## Important Note for WSL
If MoonFlow does not detect files being modified in your project, this is an issue with CS's `FileSystemWatcher`. This is a common problem for people on windows developing through WSL. At the moment there are two main solutions for this problem:

- Move the RomFS of your project to a local storage device outside of WSL, if possible. This can be made easier using the "Clone Project into Another Project" option, allowing you to copy all RomFS files of this project on top of another MoonFlow project. *Note this will overwrite the data in the project you clone on top of!*

- Install the Linux release of MoonFlow onto WSL and run the app natively that way, however this is not a flawless solution. More info about running GUI apps from WSL [here](https://learn.microsoft.com/en-us/windows/wsl/tutorials/gui-apps)