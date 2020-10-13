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

## FAQ
Please check out the [FAQ](https://github.com/LeonBlade/BDTHPlugin/wiki/FAQ) page to see if your issue might be listed here.

## Final message
Thank you for using my tool, I'm very grateful to everyone who uses my tools and I enjoy seeing what people do with them. If you wish to support me, you can do so at any of the links below. You can also join at the Discord to ask questions or share your creations.

**Ko-Fi:** https://ko-fi.com/LeonBlade

**Patreon:** https://patreon.com/LeonBlade

**Discord:** https://discord.gg/EenZwsN
