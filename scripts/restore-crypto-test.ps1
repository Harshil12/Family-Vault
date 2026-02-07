param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path .
$source = "scripts/src/FamilyVault.Test/FamilyValut.Tests/Application/Services/Others/CryptoServiceTest.cs.bak"
$destination = "src/FamilyVault.Test/FamilyValut.Tests/Application/Services/Others/CryptoServiceTests.cs"

Write-Host "Restoring test file:"
Write-Host "  source:      $source"
Write-Host "  destination: $destination"

if (-not (Test-Path $source)) {
    Write-Host "Source file not found: $source"
    exit 1
}

if (Test-Path $destination) {
    Write-Host "Destination already exists: $destination"
    Write-Host "Aborting to avoid overwrite. If you want to replace it, delete or move the existing file first."
    exit 1
}

# Ensure destination directory exists
$destDir = Split-Path $destination -Parent
if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Write-Host "Created directory: $destDir"
}

# Use git mv so rename is tracked
Write-Host "Renaming (git mv) the file..."
git mv -- "$source" "$destination"

Write-Host "Staging and committing..."
git add -- "$destination"
git commit -m "test: restore CryptoServiceTests (rename .bak -> CryptoServiceTests.cs)"

Write-Host "Running tests (dotnet test)..."
dotnet test

Write-Host "Done. Review changes in Visual Studio Git Changes."