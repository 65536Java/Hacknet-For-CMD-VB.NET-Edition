@echo off
echo pull...
git pull origin main --allow-unrelated-histories
git add .
echo commit...
git commit -m "Final project files update"
echo push....
git push -u origin main
echo Push complete
pause