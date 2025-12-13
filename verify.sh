#!/bin/bash
FOLDER=$1
PACKAGE=$FOLDER/$2/$2.csproj

dotnet build $PACKAGE -c Release -p:Version=0.0.1-demo
dotnet pack $PACKAGE -c Release -p:Version=0.0.1-demo -o ./nupkg-demos
