# 1. 强制合并两条独立的历史线
git pull origin main --allow-unrelated-histories

# 2. (可选) 提交您之前修改但未提交的文件
git add .
git commit -m "Final project files update"

# 3. 推送所有内容到 GitHub，并建立跟踪关系
git push -u origin main