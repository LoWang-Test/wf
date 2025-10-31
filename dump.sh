#!/usr/bin/env bash
echo "Publishing $1 with key $2"

if [[ "$1" == "/workspaces/wf/packages/LimitTester.Package0484.1.0.0.nupkg" ]]; then
    echo "Simulating failure for $1"
    exit 1
fi
