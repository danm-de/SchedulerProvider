#!/bin/bash

export root=$(dirname $0)
export file_solution=$root/pcsc-sharp.sln
export dir_packages=$root/packages
export paket=$root/paket.sh

script_file_name=$(basename $0)
export file_buildlog=$root/${script_file_name%.*}.log

$paket restore group Build

if [ -f "$file_buildlog" ]; then
 rm "$file_buildlog" > /dev/null
fi

if [ -z "$1" ]; then
  "$dir_packages/build/FAKE/tools/Fake.exe" "$root/build.fsx" --logfile "$file_buildlog"
else
  "$dir_packages/build/FAKE/tools/Fake.exe" "$root/build.fsx" $1 --logfile "$file_buildlog"
fi
