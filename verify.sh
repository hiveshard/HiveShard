#!/bin/bash
FOLDER=$1
PACKAGE=$FOLDER/$2/$2.csproj
VERSION=$3

dotnet build $PACKAGE -c Release -p:Version=$VERSION
dotnet pack $PACKAGE -c Release -p:Version=$VERSION -o ./nupkg
