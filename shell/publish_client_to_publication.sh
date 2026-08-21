#!/bin/bash

set -euo pipefail

# Ensure we are running inside a Git repository
if ! git rev-parse --is-inside-work-tree &>/dev/null; then
    echo "Error: this script must run in a Git repo. Exiting..." >&2
    exit 1
fi

# Check for any uncommitted changes (staged or unstaged)
if [ -n "$(git status --porcelain)" ]; then
    echo "Error: uncommitted repo changes found. Please commit or stash them first. Exiting..." >&2
    exit 1
fi

echo "Git repository is clean. Proceeding..."
SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

client_project_name="Songhay.Player.YouTube.Client"
publication_project_name="Songhay.Publications.KinteSpace"

base_href="/b-roll/video-yt/"

client_assets_dir="../$client_project_name/wwwroot/"
client_publish_dir="../$client_project_name/bin/Release/net10.0/publish/wwwroot/"
publication_assets_dir="../../$publication_project_name/src${base_href}wwwroot/"
publication_dir="../../$publication_project_name/app-staging$base_href"

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

rsync_from=$publication_assets_dir
rsync_to=$client_assets_dir
echo "running rsync from \`$rsync_from\` to \`$rsync_to\`..."

rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    "$rsync_from" "$rsync_to"

echo "deleting any existing files at publish target..."

rm -rf "../$client_project_name/bin/Release/net10.0/publish"

echo "publishing Blazor project to default location..."

dotnet publish "../$client_project_name/$client_project_name.fsproj" \
    --configuration:Release -p:CompressionEnabled=false \
    /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary \
    --runtime linux-x64

echo "running rsync from default Blazor publish location to local publication mirror..."

rsync_from=$client_publish_dir
rsync_to=$publication_dir

rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    --exclude .gitkeep \
    "$rsync_from" "$rsync_to"

echo "Rolling back any repo changes..."

git reset --hard HEAD && git clean -fd

echo "Script is finished. Make sure to double check the base.href of the Blazor index.html file."
