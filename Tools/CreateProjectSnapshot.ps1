
# Warp Project Snapshot
# Read-only collection of project metadata and source files.
# Does not modify Unity assets or Git history.

$ErrorActionPreference = "Stop"

$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ProjectName = Split-Path $ProjectRoot -Leaf
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Store generated snapshots OUTSIDE the Git repository.
$OutputRoot = Join-Path (Split-Path $ProjectRoot -Parent) "Warp_Handoffs"
$SnapshotName = "${ProjectName}_Handoff_${Timestamp}"
$WorkDir = Join-Path $env:TEMP $SnapshotName
$ZipPath = Join-Path $OutputRoot "${SnapshotName}.zip"

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $Parent = Split-Path $Path -Parent
    if (-not (Test-Path $Parent)) {
        New-Item -ItemType Directory -Path $Parent -Force | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $Content -Encoding UTF8
}



function Run-Git {
    param([string[]]$GitArgs)

    $PreviousPreference = $ErrorActionPreference

    try {
        # Git may write warnings to stderr even when it succeeds.
        # Do not let PowerShell 5.1 stop on those warnings.
        $ErrorActionPreference = "Continue"

        $Output = & git -C $ProjectRoot @GitArgs 2>&1
        $ExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $PreviousPreference
    }

    $Lines = @(
        "Command: git $($GitArgs -join ' ')"
        "Exit code: $ExitCode"
    )

    foreach ($Item in $Output) {
        $Lines += "$Item"
    }

    return ($Lines -join "`r`n")
}


try {
    if (-not (Test-Path (Join-Path $ProjectRoot "Assets"))) {
        throw "Assets folder not found. Wrong project root: $ProjectRoot"
    }

    if (-not (Test-Path (Join-Path $ProjectRoot "ProjectSettings"))) {
        throw "ProjectSettings folder not found."
    }

    if (Test-Path $WorkDir) {
        Remove-Item $WorkDir -Recurse -Force
    }

    New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
    New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

    # Copy project handoff documents.
    $DocsDir = Join-Path $WorkDir "ProjectDocs"
    New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null

    foreach ($Name in @(
        "PROJECT_STATUS.md",
        "ARCHITECTURE.md",
        "CHANGELOG.md",
        "TEST_LOG.md",
        "README.md"
    )) {
        $Source = Join-Path $ProjectRoot $Name
        if (Test-Path $Source) {
            Copy-Item $Source $DocsDir
        }
    }

    # Copy all C# source files, preserving relative paths.
    $ScriptsDir = Join-Path $WorkDir "Scripts"
    New-Item -ItemType Directory -Path $ScriptsDir -Force | Out-Null

    $CsFiles = Get-ChildItem (Join-Path $ProjectRoot "Assets") `
        -Filter "*.cs" -File -Recurse

    foreach ($File in $CsFiles) {
        $Relative = $File.FullName.Substring(
            (Join-Path $ProjectRoot "Assets").Length
        ).TrimStart('\', '/')

        $Destination = Join-Path $ScriptsDir $Relative
        $DestinationDir = Split-Path $Destination -Parent

        New-Item -ItemType Directory -Path $DestinationDir `
            -Force | Out-Null

        Copy-Item $File.FullName $Destination
    }

    # Copy Unity version and package manifest.
    $ConfigDir = Join-Path $WorkDir "UnityConfig"
    New-Item -ItemType Directory -Path $ConfigDir -Force | Out-Null

    $ConfigFiles = @(
        "ProjectSettings/ProjectVersion.txt",
        "Packages/manifest.json",
        "Packages/packages-lock.json",
        ".gitignore",
        ".gitattributes"
    )

    foreach ($RelativePath in $ConfigFiles) {
        $Source = Join-Path $ProjectRoot $RelativePath
        if (Test-Path $Source) {
            $Destination = Join-Path $ConfigDir $RelativePath
            $DestinationDir = Split-Path $Destination -Parent

            New-Item -ItemType Directory -Path $DestinationDir `
                -Force | Out-Null

            Copy-Item $Source $Destination
        }
    }

    # Git state and recent history.
    $GitDir = Join-Path $WorkDir "Git"
    New-Item -ItemType Directory -Path $GitDir -Force | Out-Null

    $IsGitRepo = $false
    $GitRoot = & git -C $ProjectRoot rev-parse --show-toplevel 2>$null

    if ($LASTEXITCODE -eq 0 -and $GitRoot) {
        $IsGitRepo = $true
    }

    if ($IsGitRepo) {
        Write-TextFile (Join-Path $GitDir "status.txt") `
            (Run-Git @("status", "--short", "--branch", "--untracked-files=all"))

        Write-TextFile (Join-Path $GitDir "recent-commits.txt") `
            (Run-Git @("log", "-10", "--date=iso-strict", `
                "--pretty=format:%h %ad %s"))

        Write-TextFile (Join-Path $GitDir "diff-stat.txt") `
            (Run-Git @("diff", "--stat"))

        Write-TextFile (Join-Path $GitDir "staged-diff-stat.txt") `
            (Run-Git @("diff", "--cached", "--stat"))

            
# Full text diff of unstaged tracked changes.
# Binary assets are not exported as binary patches.
Write-TextFile (Join-Path $GitDir "unstaged-diff.txt") `
    (Run-Git @("diff", "--no-ext-diff", "--no-textconv"))

# Full text diff of staged changes.
Write-TextFile (Join-Path $GitDir "staged-diff.txt") `
    (Run-Git @("diff", "--cached", "--no-ext-diff", "--no-textconv"))

# List untracked files separately for easy review.
Write-TextFile (Join-Path $GitDir "untracked-files.txt") `
    (Run-Git @("ls-files", "--others", "--exclude-standard"))

# Record the last commit and its parent.
Write-TextFile (Join-Path $GitDir "last-commit.txt") `
    (Run-Git @("log", "-1", "--format=fuller"))



        Write-TextFile (Join-Path $GitDir "tracked-files.txt") `
            (Run-Git @("ls-files"))
    }
    else {
        Write-TextFile (Join-Path $GitDir "status.txt") `
            "No Git repository detected."
    }

    # File inventory, not the files themselves.
    $Inventory = Get-ChildItem (Join-Path $ProjectRoot "Assets") `
        -File -Recurse |
        ForEach-Object {
            $_.FullName.Substring($ProjectRoot.Length).TrimStart('\', '/')
        }

    Write-TextFile (Join-Path $WorkDir "Assets-file-inventory.txt") `
        ($Inventory -join "`r`n")

        
# Build an inventory of Unity assets without copying asset contents.
$AssetRoot = Join-Path $ProjectRoot "Assets"
$ResourceIndexPath = Join-Path $WorkDir "Unity-resource-index.csv"

$ResourceRows = @(
    Get-ChildItem $AssetRoot -File -Recurse |
    Where-Object { $_.Extension -ne ".meta" } |
    ForEach-Object {
        $File = $_
        $RelativePath = $File.FullName.Substring(
            $ProjectRoot.Length
        ).TrimStart('\', '/')

        $MetaPath = "$($File.FullName).meta"
        $Guid = ""
        $MetaExists = Test-Path $MetaPath

        if ($MetaExists) {
            $GuidLine = Select-String `
                -LiteralPath $MetaPath `
                -Pattern '^\s*guid:\s*([a-fA-F0-9]+)\s*$' |
                Select-Object -First 1

            if ($GuidLine) {
                $Guid = $GuidLine.Matches[0].Groups[1].Value
            }
        }

        $Category = switch ($File.Extension.ToLowerInvariant()) {
            ".unity"  { "Scene"; break }
            ".prefab" { "Prefab"; break }
            ".asset"  { "Asset or ScriptableObject"; break }
            ".cs"     { "CSharp"; break }
            ".mat"    { "Material"; break }
            ".fbx"    { "Model"; break }
            ".png"    { "Image"; break }
            ".jpg"    { "Image"; break }
            ".jpeg"   { "Image"; break }
            ".wav"    { "Audio"; break }
            ".mp3"    { "Audio"; break }
            ".mp4"    { "Video"; break }
            default   { "Other" }
        }

        [PSCustomObject]@{
            Path = $RelativePath
            Extension = $File.Extension
            Category = $Category
            SizeBytes = $File.Length
            MetaExists = $MetaExists
            GUID = $Guid
        }
    }
)

$ResourceRows |
    Export-Csv -LiteralPath $ResourceIndexPath `
        -NoTypeInformation -Encoding UTF8



# Build a simple GUID reference index from Unity text assets.
$ReferenceIndexPath = Join-Path $WorkDir "Unity-reference-index.csv"

$GuidToPaths = @{}
$GuidOccurrences = @()

# Map asset GUIDs to their paths using .meta files.
$MetaFiles = Get-ChildItem $AssetRoot -Filter "*.meta" -File -Recurse

foreach ($MetaFile in $MetaFiles) {
    $MetaMatch = Select-String -LiteralPath $MetaFile.FullName `
        -Pattern '^\s*guid:\s*([a-fA-F0-9]{32})\s*$' |
        Select-Object -First 1

    if ($MetaMatch) {
        $Guid = $MetaMatch.Matches[0].Groups[1].Value.ToLowerInvariant()

        
$AssetPath = $MetaFile.FullName.Substring(
    $AssetRoot.Length
).TrimStart('\', '/')

$AssetPath = $AssetPath -replace '\.meta$', ''
$AssetPath = "Assets/$($AssetPath -replace '\\', '/')"

        if (-not $GuidToPaths.ContainsKey($Guid)) {
            $GuidToPaths[$Guid] = $AssetPath
        }
        else {
            # Keep duplicate GUIDs visible for later manual review.
            $GuidToPaths[$Guid] = "DUPLICATE GUID: $($GuidToPaths[$Guid])"
        }
    }
}

# Only scan common Unity text-serialized asset formats.
$ReferenceExtensions = @(
    ".unity", ".prefab", ".asset", ".mat",
    ".controller", ".anim", ".overridecontroller",
    ".playable", ".lighting"
)

$ReferenceFiles = Get-ChildItem $AssetRoot -File -Recurse |
    Where-Object {
        $ReferenceExtensions -contains $_.Extension.ToLowerInvariant()
    }

foreach ($File in $ReferenceFiles) {
    $SourcePath = $File.FullName.Substring(
        $ProjectRoot.Length
    ).TrimStart('\', '/')

    $Matches = Select-String -LiteralPath $File.FullName `
        -Pattern '\bguid:\s*([a-fA-F0-9]{32})\b'

    foreach ($Line in $Matches) {
        foreach ($Match in $Line.Matches) {
            $ReferencedGuid = $Match.Groups[1].Value.ToLowerInvariant()

            
$TargetPath = ""
$Resolution = "UNRESOLVED"

if ($ReferencedGuid -match '^0+$') {
    $Resolution = "ZERO_OR_SPECIAL_GUID"
}
elseif ($GuidToPaths.ContainsKey($ReferencedGuid)) {
    $TargetPath = $GuidToPaths[$ReferencedGuid]

    if ($TargetPath.StartsWith("DUPLICATE GUID:")) {
        $Resolution = "DUPLICATE_GUID_REVIEW"
    }
    else {
        $Resolution = "MATCHED"
    }
}

            if ($GuidToPaths.ContainsKey($ReferencedGuid)) {
                $TargetPath = $GuidToPaths[$ReferencedGuid]

                if ($TargetPath.StartsWith("DUPLICATE GUID:")) {
                    $Resolution = "DUPLICATE GUID - REVIEW"
                }
                else {
                    $Resolution = "MATCHED"
                }
            }

            $GuidOccurrences += [PSCustomObject]@{
                SourcePath = $SourcePath
                LineNumber = $Line.LineNumber
                GUID = $ReferencedGuid
                TargetPath = $TargetPath
                Resolution = $Resolution
            }
        }
    }
}

$GuidOccurrences |
    Export-Csv -LiteralPath $ReferenceIndexPath `
        -NoTypeInformation -Encoding UTF8


    # Add a handoff note.
    $Note = @"
Project: $ProjectName
Snapshot created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")
Project root: $ProjectRoot

This snapshot contains selected source files, project documents,
Unity configuration and Git metadata.

It does NOT contain the complete Unity project, scenes, prefabs,
ScriptableObject assets, Library cache, or full Git history.

Verify the included files and remove sensitive information before
sharing this archive.
"@

    Write-TextFile (Join-Path $WorkDir "SNAPSHOT_README.txt") $Note

    if (Test-Path $ZipPath) {
        throw "Output ZIP already exists: $ZipPath"
    }

    Compress-Archive -Path (Join-Path $WorkDir "*") `
        -DestinationPath $ZipPath -CompressionLevel Optimal

    Write-Host ""
    Write-Host "Snapshot created successfully:" -ForegroundColor Green
    Write-Host $ZipPath
    Write-Host ""
    Write-Host "Review the ZIP before uploading it to any service."

}
catch {
    Write-Host "Snapshot failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    throw
}
finally {
    if (Test-Path $WorkDir) {
        Remove-Item $WorkDir -Recurse -Force
    }
}