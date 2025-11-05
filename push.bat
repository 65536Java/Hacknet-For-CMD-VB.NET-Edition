@echo off
echo pulling...
git pull origin main --allow-unrelated-histories
git add .
git rm lib\.
echo commit...
set /p commit=Content: 
git commit -m "%commit%"
echo pushing....
git push -u origin main
echo Push complete
pause