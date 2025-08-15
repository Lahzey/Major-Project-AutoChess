# MP Auto Chess
This a the repository of an online video game of the auto chess genre.
## Local Setup
To run this locally, you must first download [Godot 4.3](https://godotengine.org/download/archive/4.3-stable/) (.NET Version).
You will also need a .NET SDK, which you can find [here](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks).
Now you may open the project in Godot.
It will likely complain that it cannot run a plugin. This is because it has not compiled yet. Simply click the build button on the top right (hammer icon), then go to project settings -> plugins and reenable the SceneSafeMutiplayer plugin.
It is possible that the project will work just fine without enabling the plugin though.
## Structure
The start of networking code can be found in ServerController.cs. Any commands (RPCs) sent by the player will be found in PlayerController.cs, while the RPCs from the server can be found all over the codebase.
The main entry point for game logic is GameSession, although almost all logic goes through the GameMode (with LiveMode.cs currently being the only implemented one).