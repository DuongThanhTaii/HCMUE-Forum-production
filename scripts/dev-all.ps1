# Chạy Forum BE/FE + UEBot sync-api/web-app trong MỘT terminal (log có prefix màu).
# Yêu cầu: Docker Postgres (port 5433), đã cài .env cho sync-api & web-app UEBot.
#
# Usage (từ thư mục gốc repo):
#   .\scripts\dev-all.ps1
# Hoặc sau khi npm install ở root:
#   npm run dev:all

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $Root

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  Write-Error "dotnet CLI không có trong PATH."
}
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
  Write-Error "npm không có trong PATH."
}

if (-not (Test-Path "node_modules\concurrently")) {
  Write-Host "Cài concurrently (lần đầu)..." -ForegroundColor Cyan
  npm install --no-fund --no-audit
}

Write-Host @"

  Forum API     -> http://localhost:5034
  Forum UI      -> http://localhost:5173
  UEBot API     -> http://localhost:4010  (sync-api)
  UEBot UI      -> http://localhost:1420  (web-app)

  Ctrl+C để dừng tất cả.

"@ -ForegroundColor DarkGray

npm run dev:all
