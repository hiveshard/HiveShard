#!/bin/bash
FOLDER=$1
PACKAGE=$FOLDER/$2/$2.csproj
VERSION=$3

dotnet nuget push ./nupkg/$2.$VERSION.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
