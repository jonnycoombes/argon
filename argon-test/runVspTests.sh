#!/bin/zsh
dotnet test --logger="console;verbosity=detailed" --filter="Provider=VSP"
