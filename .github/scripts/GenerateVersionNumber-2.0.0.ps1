$latestVersion = [version]"2.0.0"

$newVersion = [version]$latestVersion
$phase = ""
$newVersionString = ""

switch -regex ($Env:GITHUB_REF) {
  '^refs\/pull\/*.' {
    $splitRef = $Env:GITHUB_REF -split "/"
    $phase = "pr$($splitRef[2])"
    $newVersionString = "{0}-{3}-{4}" -f $newVersion, $phase, $Env:GITHUB_RUN_NUMBER
  }
  '^refs\/heads\/feature-2.0.0\/*.' {
    $phase = 'alpha'
    $newVersionString = "{0}-{3}-{4}" -f $newVersion, $phase, $Env:GITHUB_RUN_NUMBER
  }
  '^refs\/heads\/development-2.0.0' {
    $phase = 'beta'
    $newVersionString = "{0}-{3}-{4}" -f $newVersion, $phase, $Env:GITHUB_RUN_NUMBER
  }  
}


Write-Output $newVersionString
