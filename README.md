# Bean-Battles-Custom-Maps-mod
A mod of the "Custom map making/loading mod" for Bean Battles. This is a modded version Flarfo's BeanBattlesMapMaker. Therefore, either Flarfo's version or this version should be in the Bepinx plugins folder. Not both.

Flarfo's installation guide: https://www.youtube.com/watch?v=LKqCqfUcV_A&list=PL8_A1XBIbdkKR9wXkWJudVez3ZD7q7WBU

This mod makes the map window minimizable and changes the way it scrolls.

# Changelog
### Feb 26, 2025
* Made scrolling non-elastic
* Modified map window to make it collapsable
* Added a minimize window button
* Changed instances of `transform.parent=` to `transform.SetParent(...)` to mitigate console warning spam

### July 18, 2025
* Updated to .NET 4.6
* Updated the network code to match the current Bean Battles Custom Map build
* Closing the window no longer moves the title bar
* The minimize button now changes direction based on the window being closed or open
* Cleaned up code implementation of minimizing the window
* Removed vestigal assemblies
* Fixed circular references