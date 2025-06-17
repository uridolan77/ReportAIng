
# Close VSCode if running
Stop-Process -Name "Code" -Force -ErrorAction SilentlyContinue

# Set user paths
$userName = $env:USERNAME
$vscodeExtensions = "C:\Users\$userName\.vscode\extensions"
$vscodeGlobalStorage = "C:\Users\$userName\AppData\Roaming\Code\User\globalStorage"
$vscodeCache = "C:\Users\$userName\AppData\Roaming\Code\Cache"
$vscodeCachedData = "C:\Users\$userName\AppData\Roaming\Code\CachedData"
$vscodeWorkspaceStorage = "C:\Users\$userName\AppData\Roaming\Code\User\workspaceStorage"

Write-Output "Cleaning Augment extensions..."

# Delete Augment extensions
Get-ChildItem -Path $vscodeExtensions -Directory | Where-Object { $_.Name -like "augment-ai*" } | Remove-Item -Recurse -Force

# Delete Global Storage related to Augment
Get-ChildItem -Path $vscodeGlobalStorage -Directory | Where-Object { $_.Name -like "augment*" } | Remove-Item -Recurse -Force

Write-Output "Cleaning VSCode cache (optional cleanup)..."

# Optional deeper cleanup
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $vscodeCache
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $vscodeCachedData
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $vscodeWorkspaceStorage

Write-Output "✅ Cleanup completed successfully."
Write-Output "You can now reopen VSCode and reinstall Augment."
