#!/usr/bin/env bash
rm -rf ../SurvivorsGame-Public/
cp -r . ../SurvivorsGame-Public

cd ../SurvivorsGame-Public/
git filter-repo --path addons --path assets --path dev --path .ignore --path story/ --invert-paths --force

git remote add origin git@github.com:KainTaimu/SurvivorsGame-Public.git
git fetch origin
git branch --set-upstream-to=origin/master master