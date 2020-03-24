$latestVersions = $(git tag --merged origin/master)
$latestVersion = [version]"0.0.0"
Foreach ($version in $latestVersions) {
  Write-Host $version
  try {
    if (([version]$version) -ge $latestVersion) {
      $latestVersion = $version
      Write-Host "Setting latest version to: $latestVersion"
    }
  }
  catch {
    Write-Host "Unable to convert $($version). Skipping"
    continue;
  }
}

$newVersion = [version]$latestVersion
$phase = ""
$newVersionString = ""
switch -regex ($Env:GITHUB_REF) {
  '^refs\/heads\/master*.' {
    $newVersionString = "{0}.{1}.{2}" -f $newVersion.Major, $newVersion.Minor, $newVersion.Build
  }
  '^refs\/heads\/feature\/*.' {
    $phase = 'alpha'
    $newVersionString = "{0}.{1}.{2}-{3}-{4}" -f $newVersion.Major, $newVersion.Minor, ($newVersion.Build + 1), $phase, $Env:GITHUB_RUN_NUMBER
  }
  '^refs\/heads\/release\/*.' {
    $splitRef = $Env:GITHUB_REF -split "/"
    $version = [version]($splitRef[-1] -replace "v", "")
    $phase = 'rc'
    $newVersionString = "{0}.{1}.{2}-{3}-{4}" -f $version.Major, $version.Minor, $version.Build, $phase, $Env:GITHUB_RUN_NUMBER
  }
  '^refs\/heads\/development*.' {
    $phase = 'beta'
    $newVersionString = "{0}.{1}.{2}-{3}-{4}" -f $newVersion.Major, $newVersion.Minor, ($newVersion.Build + 1), $phase, $Env:GITHUB_RUN_NUMBER
  }
  '^refs\/heads\/hotfix\/*.' {
    $phase = 'hotfix'
    $newVersionString = "{0}.{1}.{2}-{3}-{4}" -f $newVersion.Major, $newVersion.Minor, ($newVersion.Build + 1), $phase, $Env:GITHUB_RUN_NUMBER
  }
}


Write-Output $newVersionString
