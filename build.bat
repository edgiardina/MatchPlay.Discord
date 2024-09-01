docker build --tag matchplay-discord-image .
docker save -o matchplay-discord-image.tar matchplay-discord-image
copy matchplay-discord-image.tar \\192.168.1.220\docker\matchplay-discord-image.tar