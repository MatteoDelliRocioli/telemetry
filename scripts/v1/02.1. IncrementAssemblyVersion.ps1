[CmdletBinding()]
param (
    [string] $masterProjectFile,
    [string] $projectsFiles
)

Get-Module | Remove-Module
$keys = @('PSBoundParameters','PWD','*Preference') + $PSBoundParameters.Keys 
Get-Variable -Exclude $keys | Remove-Variable -EA 0

$scriptFolder = $($PSScriptRoot.TrimEnd('\'));
Import-Module "$scriptFolder\Common.ps1" 

Set-Location $scriptFolder
$scriptName = $MyInvocation.MyCommand.Name
Start-Transcript -Path "\Logs\$scriptName.log" -Append

$buildSourcesDirectory = $env:BUILD_SOURCESDIRECTORY
if ([string]::IsNullOrEmpty($buildSourcesDirectory)) { $buildSourcesDirectory = ".." }
Set-Location "$buildSourcesDirectory" 

if ([string]::IsNullOrEmpty($masterProjectFile)) { $masterProjectFile = "SampleAPI.csproj" } #SampleAPI
if ([string]::IsNullOrEmpty($projectsFiles)) { $projectsFiles = "SampleAPI.Client.csproj|SampleAPI.Client.Publish.csproj" }

$masterProjectFileFullPath = Get-ChildItem $masterProjectFile -Recurse
if ([string]::IsNullOrEmpty($masterProjectFileFullPath)) { Write-Host "##vso[task.logissue type=warning]Error: no file found for masterProjectFile: '$masterProjectFile'." }
if ($masterProjectFileFullPath -is [array]) { Write-Host "##vso[task.logissue type=warning]Error: $($masterProjectFileFullPath.Count) files were found for masterProjectFile: '$masterProjectFile'." }
Write-Host "masterProjectFileFullPath: $masterProjectFileFullPath"

$masterProjectFileFolder = Split-Path -Path $masterProjectFileFullPath

$assemblyInfoFile = "$masterProjectFileFolder\Properties\AssemblyInfo.cs";
Write-Host "assemblyInfoFile: $assemblyInfoFile"

$version = IncrementVersionAttribute -filePath $assemblyInfoFile -versionAttribute "AssemblyVersion"
SetVersionAttribute -filePath $assemblyInfoFile -versionAttribute "AssemblyFileVersion" -version $version

$projectsFilesArray = $projectsFiles.Split('|')
foreach($projectFile in $projectsFilesArray) 
{
    $projectFileFullPath = Get-ChildItem $projectFile -Recurse
    $projectFileFolder = Split-Path -Path $projectFileFullPath

    $assemblyInfoFile = "$projectFileFolder\Properties\AssemblyInfo.cs";
    Write-Host "assemblyInfoFile: $assemblyInfoFile"
    SetVersionAttribute -filePath $assemblyInfoFile -versionAttribute "AssemblyVersion" -version $version
    SetVersionAttribute -filePath $assemblyInfoFile -versionAttribute "AssemblyFileVersion" -version $version
}

Write-Host "version: $version"
Write-Host "##vso[task.setvariable variable=version;isOutput=true]$version"


# git --version
# git config user.email buildagent@microsoft.com
# git config user.name "Build Agent" 
# Write-Host "before git commit"
# git commit -a -m "Build version update"
# Write-Host "after git commit"
# try {
#   Write-Host "before git push origin HEAD:$($env:BUILD_SOURCEBRANCHNAME)"
#   git push origin HEAD:$($env:BUILD_SOURCEBRANCHNAME) 
#   Write-Host "after git push"
# } catch {
#   Write-Host "##vso[task.logissue type=warning]failure on git push command."
# }


Stop-Transcript

return 0;