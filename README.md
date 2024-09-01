# MatchPlay.Discord

## Install link
https://discord.com/oauth2/authorize?client_id=1279230557917679678&scope=bot&permissions=3072

## How to use
`/matchplay subscribe tournamentid 123456`
Subscribe to tournament to the current discord channel

`/matchplay unsubscribe tournamentid 123456`
Unsubscribe a tournament to the current discord channel

`/matchplay unsubscribe`	
Unsubscribe all tournaments to the current discord channel

## Docker Build Instructions
```
docker build --tag matchplay-discord-image .
docker save -o matchplay-discord-image.tar matchplay-discord-image
```