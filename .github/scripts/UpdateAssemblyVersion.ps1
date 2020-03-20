function Update-SourceVersion {
    Param ([string]$Version)
    #$fullVersion = $Version
    $baseVersion = [regex]::Match($Version, "(\d+.\d+.\d+).*").captures.groups[1].value
    $NewAssemblyVersion = ‘AssemblyVersion("‘ + $baseVersion + ‘.*")’
    Write-Output "AssemblyVersion = $NewAssemblyVersion"
    $NewAssemblyInformationalVersion = ‘AssemblyInformationalVersion("‘ + $Version + ‘")’
    Write-Output "AssemblyInformationalVersion = $NewAssemblyInformationalVersion"

    foreach ($o in $input) {
        Write-output $o.FullName
        $TmpFile = $o.FullName + “.tmp”
        get-content $o.FullName |
        ForEach-Object {
            $_ -replace ‘AssemblyVersion\(".*"\)’, $NewAssemblyVersion } |
        ForEach-Object {
            $_ -replace ‘AssemblyInformationalVersion\(".*"\)’, $NewAssemblyInformationalVersion
        }  > $TmpFile 
        move-item $TmpFile $o.FullName -force
    }
}

function Update-AllAssemblyInfoFiles ( $version ) {
    foreach ($file in “AssemblyInfo.cs”, “AssemblyInfo.vb” ) {
        get-childitem -Path $Env:GITHUB_WORKSPACE -recurse | Where-Object { $_.Name -eq $file } | Update-SourceVersion $version ;
    }
}

# validate arguments
$r = [System.Text.RegularExpressions.Regex]::Match($args[0], "\d+\.\d+\.\d+.*");
if ($r.Success) {
    Write-Output "Updating Assembly Version to $args ...";
    Update-AllAssemblyInfoFiles $args[0];
}
else {
    Write-Output ” “;
    Write-Output “Error: Input version does not match x.y.z format!”
    Write-Output ” “;
    Write-Output "Unable to apply version to AssemblyInfo.cs files";
}
