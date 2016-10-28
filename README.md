# PlayFab Power Tools CLI
playfab power tools CLI  is a command line interface that can be used to perform various admin actions on a title and do title migrations.

***NOTE:*** This tool has been completely re-factored and is no longer the "Script" tool that it was.  It is much more useful now and new features can be added to it more easily.

##Supported Platforms
**Currently for Windows Only**

##Installation
Installation of PlayFab Power Tools CLI is simple.  

1. Extract the folder to any directory of your choosing.
2. Modify your environment path to point to that folder.
	1. **start** --> type: **environment** --> **Click Edit the system environment variables**  and modify your "path" to include your PFPT CLI directory.

##Modes
There are currently two modes for the PFPT CLI,  there is a CLI mode and a Console mode.

1. Console Mode: To enter into console mode just open up "cmd" and type PlayFabPowerTools
2. CLI Mode: Any command can be executed directly from CLI mode,  open "cmd" and type PlayFabPowerTools [command] [args]   

**CLI Example:**
1. Windows Key + R
2. type: cmd 
3. type: PlayFabPowerTools login marco+demo@playfab.com mypassword

in this example, you can login using your credentials and it will log you in and pull all the data related to your login.


##Commands
There are currently only a few commands that the tool can do right now.

1. Login - This allows you to login with a username & password
2. SetStores - This allows you to specify which stores you want to import when using migrate.
3. Migrate - This allows you to migrate from one title to another title. 
4. Help - Display a help screen
5. Exit - Exit Console Mode

