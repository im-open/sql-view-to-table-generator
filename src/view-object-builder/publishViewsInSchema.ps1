Param(
    [string]$SchemaName,
    [string]$DbName,
    [string]$NugetFolder = "$PSScriptRoot\.nuget",
    [string]$DbServer = "localhost",
    [string]$Port = 1433,
    [switch]$Publish = $false,
    [switch]$Verbose = $false,
    [string]$DefaultBranchName = "main",
    [string]$NugetRetrievalUrl,
    [string]$NugetPublishUrl,
    [string]$PackageFolder = "$PSScriptRoot\.packages",
    [string]$NugetApiKey
)

$ErrorActionPreference = "Stop"
$originalLocation = Get-Location

Set-Location -Path $PSScriptRoot

#ExitCodes
$sqlCreationPassed = 0
$nuGetInstallFinished = 0
$equivalentScripts = 0
$differentScripts = 1

# Parse the current branch information
$currentBranch = git branch --show-current
$branchName = $currentBranch.replace("_", "-").replace("/", "-") # "_" and "/" are illegal characters for nuget versions and metadata
$shortSha = git rev-parse --short HEAD
$metadata = ""
if ($branchName -ne $DefaultBranchName) { $metadata = "-$branchName-$shortSha" }

$targetNugetExe = "$NugetFolder\nuget.exe"
$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$sqlOutputPath = $PackageFolder
$dbManager = "view-object-builder"
$dbManagerPath = "$PSScriptRoot\$dbManager.dll"

# Regex variables
$package = "package"
$packageNameVer = "packageNameVer"
$packageName = "packageName"
$packageVersion = "packageVersion"
$extension = "extensions"
$sqlFileRegex = "(?<$package>(?<$packageNameVer>(?<$packageName>\w+.\w+).(?<$packageVersion>\d.\d)))(?<$extension>.sql)"

if(![System.IO.File]::Exists($targetNugetExe)) {
    Write-Host "Downloading nuget.exe"
    New-Item -ItemType Directory -Path $NugetFolder
    Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}

#Create new sql scripts
Write-Host "Creating Sql scripts for $SchemaName."
$dbManagerArgList = @(
    "BuildSql",
    "-o", $sqlOutputPath,
    "-n", $DbServer,
    "-d", $DbName,
    "-s", $SchemaName,
    "-p", $Port,
    "-v", $Verbose
)
& dotnet $dbManagerPath $dbManagerArgList

if ($LASTEXITCODE -ne $sqlCreationPassed) {
    Set-Location -Path $originalLocation
    throw "$dbManager tool failed to create scripts."
}

# Compare sql scripts to their master versions
Write-Host "Compare new sql scripts to published master versions."
Get-ChildItem $sqlOutputPath -Filter *.sql |
ForEach-Object {
    $match = [regex]::Match($_.Name, $sqlFileRegex)
    $packageId = $match.groups[$packageName].value
    $version = $match.groups[$packageVersion].value

    & $targetNugetExe install $packageId -Source $NugetRetrievalUrl -OutputDirectory $NugetFolder -ExcludeVersion -Version $version

    if($LASTEXITCODE -eq $nuGetInstallFinished) {
        $dbManagerArgList = @(
            "CompareFiles",
            "-f", $_.FullName,
            "-f", "$NugetFolder\$packageId\$packageId.sql",
            "-v", $Verbose
        )

        & dotnet $dbManagerPath $dbManagerArgList

        # Delete if no change
        # Throw if scripts differ
        if ($LASTEXITCODE -eq $equivalentScripts)
        {
            Remove-Item -path $_.FullName
        }
        elseif ($LASTEXITCODE -eq $differentScripts)
        {
            Set-Location -Path $originalLocation
            throw "Versions of changed scripts have not been updated"
        }
    }
}

Write-Host "Creating nuget packages for scripts to publish."
$packagesCreated = 0
$packagesPublished = 0

Get-ChildItem $sqlOutputPath -Filter *.sql |
ForEach-Object {
    # Create nuget package for script
    $machineName = Hostname
    $fullPath = $_.FullName
    $directory = $_.DirectoryName
    $match = [regex]::Match($_.Name, $sqlFileRegex)
    $nameVer = $match.groups[$packageNameVer].value
    $packageId = $match.groups[$packageName].value
    $version = $match.groups[$packageVersion].value
    $fullPackageName = "$nameVer$metadata"
    $nuspecFile = "$directory\$fullPackageName.nuspec"
    $unversionedName = "$packageId.sql"

    # Remove version from .sql file name
    Rename-Item $fullPath -NewName "$directory\$unversionedName"

    $nuspec = '<?xml version="1.0"?><package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"><metadata>'
    $nuspec += "<id>$packageId</id>"
    $nuspec += "<version>$version$metadata</version>"
    $nuspec += "<authors>$machineName</authors>"
    $nuspec += "<description>Table version of the $packageId view.</description>"
    $nuspec += "</metadata>"
    $nuspec += "<files><file src=`"$directory\$unversionedName`" target=`"`" /></files>"
    $nuspec += "</package>"
    $nuspec >> $nuspecFile

    & $targetNugetExe pack $nuspecFile -OutputDirectory $directory
    Rename-Item -Path "$directory\$nameVer.0$metadata.nupkg" -NewName "$fullPackageName.nupkg"

    if($LASTEXITCODE -eq 0) {
        $packagesCreated++
    }

    # Publish script
    if ($Publish)
    {
        Write-Host "Publishing $fullPackageName package."
        $nupkgPath = "$directory\$fullPackageName.nupkg"
        $source = "$NugetPublishUrl/$SchemaName"

        & $targetNugetExe push $nupkgPath -Source $source -ApiKey $NugetApiKey
        if ($LASTEXITCODE -eq 0){
            $packagesPublished++
        }
    }
}

Write-Host "Packages created: $packagesCreated"
Write-Host "Packages published: $packagesPublished"

Set-Location -Path $originalLocation
