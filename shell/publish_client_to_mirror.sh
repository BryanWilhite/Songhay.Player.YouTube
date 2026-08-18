#!/bin/bash

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

dotnet publish \
    ../Songhay.Player.YouTube.Client/Songhay.Player.YouTube.Client.fsproj \
    --configuration:Release \
    /property:GenerateFullPaths=true \
    /consoleloggerparameters:NoSummary \
    --runtime linux-x64

echo "Running rsync with local mirror..."
rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    ../Songhay.Player.YouTube.Client/bin/Release/net10.0/publish/wwwroot/ \
    ../../s3-buckets/wasabi/b-roll-players/video-yt/songhay/

echo "Script is finished. Make sure to check the base.href of the Blazor index.html file."
