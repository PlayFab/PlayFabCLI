# PlayFab CLI
PlayFab CLI is a command line interface that can be used to perform various admin actions on a title and do title migrations.

## Community Support

This is a community supported tool. 

For new and existing users, you can use the current version as it is. The team at Microsoft is no longer providing support or making updates to this tool. You can continue to get community support and publish your own updates at [PlayFab forums](https://community.playfab.com/index.html).

What you have to do: 
* Fork this repo
* Push your updates
* Make sure you follow the Apache License, Version 2.0 guidelines for reproduction and modification, and document that Microsoft PlayFab is the original creator
* Go to [PlayFab forums](https://community.playfab.com/index.html)
* Write a post with a link to your forked repo so everyone knows about it

We're excited to hear from you. Thank you for your support and happy coding.
## Supported Platforms
**Currently for Windows Only**

## Installation
Installation of PlayFab CLI is simple.  

1. Extract the folder to any directory of your choosing.
2. Modify your environment path to point to that folder.
	1. **start** --> type: **environment** --> **Click Edit the system environment variables**  and modify your "path" to include your PFPT CLI directory.

## Modes
There are currently two modes for the PF CLI,  there is a CLI mode and a Console mode.

1. Console Mode: To enter into console mode just open up "cmd" and type PlayFabPowerTools
2. CLI Mode: Any command can be executed directly from CLI mode,  open "cmd" and type PlayFabPowerTools [command] [args]   

**CLI Example:**
1. Windows Key + R
2. type: cmd 
3. type: PlayFabPowerTools login marco+demo@playfab.com mypassword

in this example, you can login using your credentials and it will log you in and pull all the data related to your login.


## Commands
There are currently only a few commands that the tool can do right now.

1. Login - This allows you to login with a username & password.  Login will download all Titles that you have access to, you only need to do this once or any time you add titles to your PlayFab Account.
2. SetStores - This allows you to specify which stores you want to import when using migrate.
3. Migrate - This allows you to migrate from one title to another title. 
4. Help - Display a help screen
5. Exit - Exit Console Mode

