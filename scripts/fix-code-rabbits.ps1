param()

$ErrorActionPreference = 'Stop'

Write-Host "Ensure you are on branch 'generic-repository'..."
git fetch origin
git checkout generic-repository

function Backup-File($path) {
    if (Test-Path $path) {
        $bak = "$path.bak"
        if (-not (Test-Path $bak)) {
            Copy-Item -Path $path -Destination $bak -Force
            Write-Host "Backed up $path -> $bak"
        }
    }
}

function Replace-InFile($filePath, $pattern, $replacement) {
    if (-not (Test-Path $filePath)) { return $false }
    Backup-File $filePath
    $text = Get-Content -Raw -Encoding UTF8 $filePath
    $newText = [System.Text.RegularExpressions.Regex]::Replace($text, $pattern, $replacement, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if ($newText -ne $text) {
        Set-Content -Path $filePath -Value $newText -Encoding UTF8
        Write-Host "Patched: $filePath"
        return $true
    }
    return $false
}

Write-Host "1) Rename misspelled repository interface file (if present)..."
$oldRepoFile = "src/FamilyVault.Application/Interfaces/Repositories/IFamilymemeberRepository.cs"
$newRepoFile = "src/FamilyVault.Application/Interfaces/Repositories/IFamilyMemberRepository.cs"
if ((Test-Path $oldRepoFile) -and (-not (Test-Path $newRepoFile))) {
    Backup-File $oldRepoFile
    Rename-Item -Path $oldRepoFile -NewName (Split-Path $newRepoFile -Leaf)
    Write-Host "Renamed $oldRepoFile -> $newRepoFile"
}

Write-Host "2) Rename misspelled service interface file (if present)..."
$oldSvcFile = "src/FamilyVault.Application/Interfaces/Services/IFamilymemeberService.cs"
$newSvcFile = "src/FamilyVault.Application/Interfaces/Services/IFamilyMemberService.cs"
if ((Test-Path $oldSvcFile) -and (-not (Test-Path $newSvcFile))) {
    Backup-File $oldSvcFile
    Rename-Item -Path $oldSvcFile -NewName (Split-Path $newSvcFile -Leaf)
    Write-Host "Renamed $oldSvcFile -> $newSvcFile"
}


Write-Host "3) Replace symbol typos across the codebase (safe textual replacements)..."
$replacements = @(
    @{ pat = '\bIFamilymemeberRepository\b'; rep = 'IFamilyMemberRepository' },
    @{ pat = '\bIFamilymemeberService\b';    rep = 'IFamilyMemberService' },
    @{ pat = '\bIFamilymemeber\b';           rep = 'IFamilyMember' },
    @{ pat = '\bCreateFamilyMemember\b';     rep = 'CreateFamilyMember' },
    @{ pat = '\bUpdateFamilyMemember\b';     rep = 'UpdateFamilyMember' },

    @{ pat = '\bUserservice\b';              rep = 'UserService' },
    @{ pat = '\bUpdateuUerAsync\b';          rep = 'UpdateUserAsync' },

    # Ensure ILogger generic types are updated if still referencing old name
    @{ pat = 'ILogger<\s*Userservice\s*>';   rep = 'ILogger<UserService>' },

    # tests and call-sites
    @{ pat = '\bUserserviceTests\b';         rep = 'UserServiceTests' },
    @{ pat = '\bUpdateuUerAsync\(';          rep = 'UpdateUserAsync(' }
)

# Find all .cs files under src and scripts/src
$files = Get-ChildItem -Path . -Recurse -Include *.cs | Where-Object { $_.FullName -match "\\(src|scripts\\src)\\" -and $_.FullName -notmatch "\\bin\\|\\obj\\" }

foreach ($f in $files) {
    $filePath = $f.FullName
    foreach ($entry in $replacements) {
        $patched = Replace-InFile $filePath $entry.pat $entry.rep
    }
}

Write-Host "4) Fix IUserService interface method name if still wrong..."
$IUserServicePath = "src/FamilyVault.Application/Interfaces/Services/IUserService.cs"
if (Test-Path $IUserServicePath) {
    # replace UpdateuUerAsync signature with UpdateUserAsync
    Replace-InFile $IUserServicePath '\bUpdateuUerAsync\b' 'UpdateUserAsync'
}

Write-Host "5) Fix class declaration filename mismatches if any (simple check)..."
# If class file name contains Userservice class rename inside already handled above.

Write-Host "6) Stage and commit changes"
git add -A
$changes = git status --porcelain
if ($changes) {
    git commit -m "fix: address CodeRabbit review (rename typos: FamilyMember, UserService, UpdateUserAsync, tests)"
    Write-Host "Committed. Please review diffs in Visual Studio Git Changes."
} else {
    Write-Host "No changes to commit."
}

Write-Host "`n7) Run dotnet build to surface compile issues"
dotnet build

Write-Host "`nScript finished. Review the build output and the changes in your IDE."