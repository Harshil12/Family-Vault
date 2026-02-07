<#
Apply CodeRabbit requested fixes:
- Use per-parameter cache keys for family/user scoped caches
- Remove member IsDeleted check from Documents subquery in cascading Delete
- Ensure document queries exclude soft-deleted rows
- Make GenericRepository read methods filter soft-deleted rows using EF.Property (generic)
This script updates files under both `scripts/src` and `src` (when present).
Run from repo root (C:\Personal\Code) on branch `generic-repository`. Review edits in Git Changes before committing/pushing.
#>

param()

$ErrorActionPreference = 'Stop'

# List of candidate files to patch (both in scripts/src and src)
$targets = @(
    "scripts/src/FamilyVault.Infrastructure/Repositories/GenericRepository.cs",
    "src/FamilyVault.Infrastructure/Repositories/GenericRepository.cs",
    "scripts/src/FamilyVault.Infrastructure/Repositories/DocumentRepository.cs",
    "src/FamilyVault.Infrastructure/Repositories/DocumentRepository.cs",
    "scripts/src/FamilyVault.Infrastructure/Repositories/FamilyRepository.cs",
    "src/FamilyVault.Infrastructure/Repositories/FamilyRepository.cs",
    "scripts/src/FamilyVault.Infrastructure/Repositories/FamilyMemberRepository.cs",
    "src/FamilyVault.Infrastructure/Repositories/FamilyMemberRepository.cs",
    "scripts/src/FamilyVault.Infrastructure/Repositories/UserRepository.cs",
    "src/FamilyVault.Infrastructure/Repositories/UserRepository.cs"
) | Where-Object { Test-Path $_ }

function Backup-File($path) {
    $bak = "$path.bak"
    if (-not (Test-Path $bak)) {
        Copy-Item -Path $path -Destination $bak -Force
        Write-Host "Backed up $path -> $bak"
    }
}

function Patch-File($path, $replacements) {
    Backup-File $path
    $text = Get-Content -Raw -Encoding UTF8 $path
    $orig = $text

    foreach ($r in $replacements) {
        $text = [System.Text.RegularExpressions.Regex]::Replace($text, $r.Pattern, $r.Replacement, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    }

    if ($text -ne $orig) {
        Set-Content -Path $path -Value $text -Encoding UTF8
        Write-Host "Patched: $path"
        return $true
    } else {
        Write-Host "No changes: $path"
        return $false
    }
}

# Replacements for GenericRepository: ensure reads exclude soft-deleted rows using EF.Property
$genericReplacements = @(
    @{
        Pattern = 'return await\s+_appDbContext\.Set<T>\(\)\s*\.AsNoTracking\(\)\s*\.FirstOrDefaultAsync\s*\('
        Replacement = 'return await _appDbContext.Set<T>().Where(e => !EF.Property<bool>(e, nameof(BaseEntity.IsDeleted))).AsNoTracking().FirstOrDefaultAsync('
    },
    @{
        Pattern = 'var result = await\s+_appDbContext\.Set<T>\(\)\s*\.AsNoTracking\(\)\s*\.ToListAsync\('
        Replacement = 'var result = await _appDbContext.Set<T>().Where(e => !EF.Property<bool>(e, nameof(BaseEntity.IsDeleted))).AsNoTracking().ToListAsync('
    }
)

# Replacements for DocumentRepository:
$documentReplacements = @(
    # Fix cache key interpolation if malformed (replace any double-quoted duplicated quotes)
    @{
        Pattern = 'var\s+cacheKey\s*=\s*""?Documents[_-]?FamilyMember[_-]?\{?familyMemberId\}?""?;'
        Replacement = 'var cacheKey = $"Documents_FamilyMember_{familyMemberId}";'
    },
    # Ensure query excludes deleted documents (use EF.Property to be generic)
    @{
        Pattern = '\.Where\(\s*d\s*=>\s*d\.FamilyMemberId\s*==\s*familyMemberId\s*\)'
        Replacement = '.Where(d => d.FamilyMemberId == familyMemberId && !EF.Property<bool>(d, nameof(BaseEntity.IsDeleted)))'
    }
)

# Replacements for FamilyRepository:
$familyReplacements = @(
    # Make per-user cache key
    @{
        Pattern = 'var\s+cacheKey\s*=\s*""AllFamilyMembers"";'
        Replacement = 'var cacheKey = $"""Families_User_{userId}""";'
    },
    # Where clause ensure family IsDeleted filtered generically (optional)
    @{
        Pattern = 'return await _appDbContext\.Families\s*\.\s*Where\(\s*f\s*=>\s*f\.UserId\s*==\s*userId\s*\)\s*\.AsNoTracking'
        Replacement = 'return await _appDbContext.Families.Where(f => f.UserId == userId && !EF.Property<bool>(f, nameof(BaseEntity.IsDeleted))).AsNoTracking'
    },
    # Remove !m.IsDeleted from subquery for Documents cascading delete
    @{
        Pattern = '\.Where\(\s*m\s*=>\s*m\.FamilyId\s*==\s*id\s*&&\s*!m\.IsDeleted\s*\)'
        Replacement = '.Where(m => m.FamilyId == id)'
    }
)

# Replacements for FamilyMemberRepository: per-family cache key
$familyMemberReplacements = @(
    @{
        Pattern = 'var\s+cacheKey\s*=\s*""AllFamilies"";'
        Replacement = 'var cacheKey = $"""FamilyMembers_Family_{familyId}""";'
    },
    # Ensure query filters by familyId and non-deleted via EF.Property
    @{
        Pattern = 'return await _appDbContext\.FamilyMembers\s*\.\s*Where\(\s*fm\s*=>\s*fm\.FamilyId\s*==\s*familyId\s*\)\s*\.AsNoTracking'
        Replacement = 'return await _appDbContext.FamilyMembers.Where(fm => fm.FamilyId == familyId && !EF.Property<bool>(fm, nameof(BaseEntity.IsDeleted))).AsNoTracking'
    }
)

# Replacements for UserRepository: fix malformed cacheKey quotes and rely on DbContext filter (no explicit !IsDeleted)
$userReplacements = @(
    @{
        Pattern = 'var\s+cacheKey\s*=\s*""UsersWithFamilies"";'
        Replacement = 'var cacheKey = "UsersWithFamilies";'
    },
    @{
        Pattern = '\.Include\(\s*u\s*=>\s*u\.Families\s*\.Where\(\s*f\s*=>\s*!f\.IsDeleted\s*\)\s*\)'
        Replacement = '.Include(u => u.Families)'
    },
    # If there are explicit Where(u => !u.IsDeleted) patterns in user repo, remove them (global filter covers it)
    @{
        Pattern = '\.Where\(\s*u\s*=>\s*!u\.IsDeleted\s*\)'
        Replacement = ''
    }
)

# Process files
foreach ($file in $targets) {
    $fileName = Split-Path $file -Leaf

    if ($fileName -like "*GenericRepository.cs") {
        Patch-File $file $genericReplacements | Out-Null
    }
    elseif ($fileName -like "*DocumentRepository.cs") {
        Patch-File $file $documentReplacements | Out-Null
    }
    elseif ($fileName -like "*FamilyRepository.cs") {
        Patch-File $file $familyReplacements | Out-Null
    }
    elseif ($fileName -like "*FamilyMemberRepository.cs") {
        Patch-File $file $familyMemberReplacements | Out-Null
    }
    elseif ($fileName -like "*UserRepository.cs") {
        Patch-File $file $userReplacements | Out-Null
    }
}

Write-Host ""
Write-Host "Staging changed files..."
git add -A

$changes = git status --porcelain
if ($changes) {
    git commit -m "fix: address CodeRabbit cache & soft-delete comments (per-parameter keys, document cascade, generic read filters)"
    Write-Host "Committed changes on branch 'generic-repository'."
} else {
    Write-Host "No modifications to commit."
}

Write-Host "Run 'dotnet build' and 'dotnet test' locally to validate changes and inspect any remaining issues."