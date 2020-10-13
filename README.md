# BDTHPlugin

![BDTHPlugin UI](https://i.imgur.com/AdznUyJ.png)

## What is it?
BDTHPlugin is the plugin version of [Burning Down the House](https://github.com/LeonBlade/BurningDownTheHouse) which is a tool for FFXIV which gives you more control over placing housing items.

## How do I install it?
To use the plugin version of Burning Down the House, you must use the [FFXIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher) to run your game. The Quick Launcher allows for custom plugins to be installed to add new functionality to the game. To install, you first need to have the Quick Launcher setup for your game, instructions can be found on the [FFXIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher) page.

Once you've installed and configured the Quick Launcher properly, you can now download the latest [release](https://github.com/LeonBlade/BDTHPlugin/releases/latest) (you only need to download the `BDTHPlugin.zip`, not the Source code). Unzip the file, and place the containing BDTHPlugin folder in `%appdata%\XIVLauncher\installedPlugins`.

Due to the nature of how this plugin is developed, it should work without needing to update anything on new patches. However, you may find that plugins are not loaded when a new patch comes out. This is due to Quick Launcher itself needed to be updated. You can try again later after Quick Launcher as been updated for the patch. If you wish, you can also manually inject Dalamud into the game by running `%appdata%\XIVLauncher\addon\Hooks\Dalamud.Injector.exe`. However, due note that unexpected things can happen and your game may become unresponsive or crash. If this is something you're not comfortable with, please wait until the official update happens and you can use the plugin normally.

## How do I use it?
To actually use the plugin, you need to start the game via the Quick Launcher. Once you've started the game, you should be able to type in `/bdth` into the game's chat and press enter. This will then bring up the UI for the plugin.

The "Place Anywhere" checkbox can be enabled to allow you to place housing items more freely. Once you select a housing item, you'll notice that the coordinates will display in the UI. However you **MUST be in ROTATE mode** in order to modify the position. Simply change the coordinates however you like and then rotate the item slightly in order to lock in the changes.

The three sets of coordinates on the same line can be click and dragged in order to move the number positive and negative. The single sets of coordinates have the plus and minus buttons to increment and decrement. Both sets of these inputs can also be scrolled over with the mouse wheel in order to increase and decrease the numbers as well. The "drag" input can be modified to choose interval in which you will increase or decrease the coordinates.

## Frequent questions or issues
#### I tried to float my object, but it just snaps back to the floor!
The game itself limits how little you can move an item vertically based on its height. You need to roughly move it about as tall as the item itself in order to have your changes take effect. There is a [Google Sheets document](https://docs.google.com/spreadsheets/d/1UBuSixX3k0oDch25owaCSGfEW2ILM6l4P2z-vN8J39s/) which explains the traditional way of floating items and also contains information on each housing item to explain how far you need to raise or lower an item before it will take effect. Unfortunately, this is a limitation of the game itself, and there's nothing I can do to fix it.

#### My items were rotated but when I left and came back they reverted back!
The game only allows for rotation on the vertical axis, meaning you can only rotate items like you would normally rotate your character. This means, if you place a wall mounted item on the ceiling or floor, it will not stay that way as they need to be positioned up and down. This is a limitation of the game and there's nothing I can do to fix it.

#### Is this safe to use?
This plugin is as safe as any of my other tools which means that you take your own risks when using them as they are not supported nor do they abide by the terms of service. However, if you use it responsibly and do not talk about it within game, the risk of anything happening to you is next to none. This tool mimicks mostly what can be done in game via glitches. However, you have greater control over positioning objects that you normally would not have. I ask that you please be responsible with how you use this tool. Don't make eyesores on your lawn for other people to look at because you floated something 50 feet in the air. By treating this tool with respect, you allow it to exist without Square Enix intervening.

#### Is Quick Launcher safe to use?
I've been asked several times if the Quick Launcher is safe. Short answer is **YES**.

For those who want a bit more information on the Quick Launcher and why it's safe, please feel free to keep reading. Some people are worried about how you have to enter your credentials into a third party application and are worried that they may be jeopordizing their security. Rest assured, nothing happens with your account credentials with the launcher besides logging you into your account just like the vanilla launcher would do. The source code is available for you to look at if you know what you're looking for. Saved passwords are stored in Window's Credential Manager which is a safe place to store a password like this. You can opt out of saving your password however, so you can control your security.

## Final message
Thank you for using my tool, I'm very grateful to everyone who uses my tools and I enjoy seeing what people do with them. If you wish to support me, you can do so at any of the links below.

**Ko-Fi:** https://ko-fi.com/LeonBlade

**Patreon:** https://patreon.com/LeonBlade
