#!/bin/bash

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

echo "Running rsync with default assets..."
rsync -r \
    --checksum \
    --links \
    --progress \
    --stats \
    ../../Songhay.Publications.SonghayStudio/src/b-roll/video-yt/_bundles/
    ../Songhay.Player.YouTube.Client/wwwroot/ \
