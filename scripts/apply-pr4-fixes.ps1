# Applies quick fixes for PR#4: persist DataProtection keys, guard VerifyPassword,
# and remove redundant per-repo IsDeleted filters (rely on AppDbContext global filters).
# Run from repo root (C:\Personal\Code). Review changes in Git Changes before pushing.
param()

$ErrorActionPreference = 'Stop'

Write-Host "Switching to branch 'generic-repository'..."
git fetch origin
git checkout generic-repository

# Backup helper
function Backup-File($path) {
    $bak = "$path.bak"
    if (-not (Test-Path $bak)) {
        Copy-Item -Path $path -Destination $bak -Force
        Write-Host "Backed up $path -> $bak"
    }
}

# 1) CryptoService: wrap VerifyPassword with try/catch and accept SuccessRehashNeeded
$cryptoPath = "src/FamilyVault.Application/Services/CryptoService.cs"
if (Test-Path $cryptoPath) {
    Backup-File $cryptoPath
    $text = Get-Content -Raw -Encoding UTF8 $cryptoPath

    $pattern = 'public\s+bool\s+VerifyPassword\s*\(\s*string\s+hashPassword\s*,\s*string\s+password\s*\)\s*\{[\s\S]*?\}'
    $replacement = @'
public bool VerifyPassword(string hashPassword, string password)
{
    try
    {
        var result = _passwordHasher.VerifyHashedPassword(
            null,
            hashPassword,
            password);

        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
    catch (FormatException)
    {
        // Malformed stored hash -> treat as non-matching
        return false;
    }
}
'@

    if ($text -match $pattern) {
        $newText = [System.Text.RegularExpressions.Regex]::Replace($text, $pattern, [System.Text.RegularExpressions.Regex]::Escape($replacement) -replace '\\r\\n','`n', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        # The above approach escapes replacement; instead do direct replace using singleline regex with evaluator
        $newText = [System.Text.RegularExpressions.Regex]::Replace($text, $pattern, $replacement, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        Set-Content -Path $cryptoPath -Value $newText -Encoding UTF8
        Write-Host "Updated: $cryptoPath (VerifyPassword try/catch + rehash acceptance)"
    } else {
        Write-Host "Pattern not found in $cryptoPath; skipping."
    }
} else {
    Write-Host "File not found: $cryptoPath"
}

# 2) Program.cs: persist DataProtection keys to filesystem (uses ContentRootPath/DataProtection-Keys)
$programPath = "src/FamilyVault.API/Program.cs"
if (Test-Path $programPath) {
    Backup-File $programPath
    $text = Get-Content -Raw -Encoding UTF8 $programPath

    # Ensure using System.IO; exists
    if ($text -notmatch "using\s+System\.IO\s*;") {
        $text = "using System.IO;`r`n" + $text
        Write-Host "Inserted 'using System.IO;' to $programPath"
    }

    # Replace builder.Services.AddDataProtection();
    $old = 'builder.Services.AddDataProtection();'
    $new = 'builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("FamilyVault");'

    if ($text -match [regex]::Escape($old)) {
        $text = $text -replace [regex]::Escape($old), [System.Text.RegularExpressions.Regex]::Escape($new) -replace '\\r\\n','`n'
        # simpler direct replace:
        $text = $text -replace [regex]::Escape($old), $new
        Set-Content -Path $programPath -Value $text -Encoding UTF8
        Write-Host "Updated: $programPath (persist DataProtection keys)"
    } else {
        Write-Host "No direct call 'builder.Services.AddDataProtection();' found in $programPath; check manually."
    }
} else {
    Write-Host "File not found: $programPath"
}

# 3) Remove redundant IsDeleted filters across src (not Migrations, not scripts)
Write-Host "Removing redundant soft-delete checks in src (excluding Migrations and scripts)..."
Get-ChildItem -Path "src" -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch "\\Migrations\\" -and $_.FullName -notmatch "\\scripts\\" } |
    ForEach-Object {
        $file = $_.FullName
        $orig = Get-Content -Raw -Encoding UTF8 $file

        $modified = $orig

        # 3a) remove standalone .Where(x => !x.IsDeleted)
        $modified = [System.Text.RegularExpressions.Regex]::Replace($modified,
            '\.Where\(\s*[^\)]*?\=\>\s*!([A-Za-z0-9_]+)\.IsDeleted\s*\)\s*',
            '',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        # 3b) remove " && !x.IsDeleted" inside predicates
        $modified = [System.Text.RegularExpressions.Regex]::Replace($modified,
            '\s+&&\s*!([A-Za-z0-9_]+)\.IsDeleted',
            '',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        # 3c) remove ", !x.IsDeleted" fragments
        $modified = [System.Text.RegularExpressions.Regex]::Replace($modified,
            ',\s*!([A-Za-z0-9_]+)\.IsDeleted',
            '',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        # 3d) convert .Include(...Where(...IsDeleted)) to .Include(x => x.Navigation)
        # pattern: .Include( <identifier> .Where( ... .IsDeleted ... ) )
        $modified = [System.Text.RegularExpressions.Regex]::Replace($modified,
            '\.Include\(\s*([^\)\.]+?)\s*=>\s*([^\)]+?)\.Where\([^\)]*?\.IsDeleted[^\)]*?\)\s*\)',
            '.Include($2)',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)

        if ($modified -ne $orig) {
            Backup-File $file
            Set-Content -Path $file -Value $modified -Encoding UTF8
            Write-Host "Patched: $file"
        }
    }

# 4) Stage & commit changes
Write-Host "Staging changes..."
git add -A

$changes = git status --porcelain
if (-not $changes) {
    Write-Host "No changes detected to commit."
} else {
    git commit -m "chore(PR#4): persist data-protection keys, guard VerifyPassword, remove redundant IsDeleted filters"
    Write-Host "Committed changes on branch 'generic-repository'. Review in Git Changes."
}

# 5) Optionally build
Write-Host "Running dotnet build (showing output)..."
dotnet build

Write-Host "Script finished. Review and run tests as needed."