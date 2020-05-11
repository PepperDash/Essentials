# Uncomment these for local testing
# $Env:GITHUB_WORKSPACE = "C:\Working Directories\PD\essentials"
# $Env:SOLUTION_FILE = "PepperDashEssentials"
# $Env:VERSION = "0.0.0-buildType-test"

# Sets the root directory for the operation
$destination = "$($Env:GITHUB_HOME)\output"
New-Item -ItemType Directory -Force -Path ($destination)
Get-ChildItem ($destination)
$exclusions = @(git submodule foreach --quiet 'echo $name')
# Trying to get any .json schema files (not currently working)
# Gets any files with the listed extensions.
Get-ChildItem -recurse -Path "$($Env:GITHUB_WORKSPACE)" -include "*.clz", "*.cpz", "*.cplz", "*.dll" | ForEach-Object {
  $allowed = $true;
  # Exclude any files in submodules
  foreach ($exclude in $exclusions) {
    if ((Split-Path $_.FullName -Parent).contains("$($exclude)")) {
      $allowed = $false;
      break;
    }
  }
  if ($allowed) {
    Write-Host "allowing $($_)"
    $_;
  }
} | Copy-Item -Destination ($destination) -Force
Write-Host "Getting matching files..."
# Get any files from the output folder that match the following extensions
Get-ChildItem -Path $destination | Where-Object { ($_.Extension -eq ".clz") -or ($_.Extension -eq ".cpz" -or ($_.Extension -eq ".cplz")) } | ForEach-Object { 
  # Replace the extensions with dll and xml and create an array 
  # Removed dll file capture, as previous step should capture all of them. Add if needed-> $($_ -replace "cpz|clz|cplz", "dll"),
  $filenames = @($($_ -replace "cpz|clz|cplz", "xml"))
  Write-Host "Filenames:"
  Write-Host $filenames
  if ($filenames.length -gt 0) {
    # Attempt to get the files and return them to the output directory
    Get-ChildItem -Recurse -Path "$($Env:GITHUB_WORKSPACE)" -include $filenames | Copy-Item -Destination ($destination) -Force
  }
}

Get-ChildItem -Path $destination\*.cpz | Rename-Item -NewName { "$($_.BaseName)-$($Env:VERSION)$($_.Extension)" }
Compress-Archive -Path $destination -DestinationPath "$($Env:GITHUB_WORKSPACE)\$($Env:SOLUTION_FILE)-$($Env:VERSION).zip" -Force
Write-Host "Output Contents post Zip"
Get-ChildItem -Path $destination