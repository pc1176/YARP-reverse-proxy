#!/bin/bash

# Start VPM instances
dotnet run --project VPM/VPM.csproj --launch-profile vpm1 &
dotnet run --project VPM/VPM.csproj --launch-profile vpm2 &
dotnet run --project VPM/VPM.csproj --launch-profile vpm3 &

# Wait for all instances
wait 