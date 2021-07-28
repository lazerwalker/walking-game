# An AR Walking Game!

This is a Pokemon Go-style AR walking game built with [Unity](https://unity3d.com) and [PlayFab](https://playfab.com) being developed 100% in the open on [Twitch](https://twitch.tv/lazerwalker). If you're interested in following along live with progress, check out [my Twitch channel](https://twitch.tv/lazerwalker) for a more detailed streaming schedule, but in general work will always happen on Thursdays at 4pm ET as well as more ad-hoc sessions (usually announced on [my Twitter](https://twitter.com/lazerwalker). Specific planned upcoming streams:

* Friday, Feb 5, time TBD: Getting the app to compile for iOS (and maybe Android), adding Apple Game Center authentication, setting up cloud CI
* Thursday, Feb 11, 4pm ET: Implementing a single daily quest, from buying it in the store through to completing it with server-side verification

More info on the game is coming soon, as development continues! Check out this README for info as it happens, updates on streams, and links to past streams.
 
This is slightly deprecated -- I'm now focusing on a HTML5-based version of this.
 
# Dev Changelog

## 4 Feb 2021 ([Twitch VOD](https://www.twitch.tv/videos/901050244))

* First ever stream!
* Created new Unity project and added PlayFab
* Created a new PlayFab title for the game
* Added anonymous PlayFab login, generating and storing a GUID on launch
* Investigated adding persistent login with either Xbox Live, Twitch, or Google, but for various reasons decided to wait on persistent login until building out Apple Game Center or Google Play auth with native apps
* Researched how consumable inventory items worked, thinking through how daily quests will be modeled as consumable containers with both a single-use constraint and a 1-day expiration
* Created this git repo and added a gitignore
