function Update-SourceVersion
{
    Param ([string]$Version)
    $fullVersion = $Version
    $baseVersion = [regex]::Match($Version, "(\d+.\d+.\d+).*").captures.groups[1].value
    $NewAssemblyVersion = ‘AssemblyVersion("‘ + $baseVersion + ‘.*")’
    echo "AssemblyVersion = $NewAssemblyVersion"
    $NewAssemblyInformationalVersion = ‘AssemblyInformationalVersion("‘ + $Version + ‘")’
    echo "AssemblyInformationalVersion = $NewAssemblyInformationalVersion"

    foreach ($o in $input)
   {
        Write-output $o.FullName
        $TmpFile = $o.FullName + “.tmp”
        get-content $o.FullName |
        %{
           $_ -replace ‘AssemblyVersion\(".*"\)’, $NewAssemblyVersion} |
        %{
           $_ -replace ‘AssemblyInformationalVersion\(".*"\)’, $NewAssemblyInformationalVersion
         }  > $TmpFile 
        move-item $TmpFile $o.FullName -force
    }
}

function Update-AllAssemblyInfoFiles ( $version )
{
    foreach ($file in “AssemblyInfo.cs”, “AssemblyInfo.vb” )
    {
        get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceVersion $version ;
    }
}

# validate arguments
$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "\d+\.\d+\.\d+.*");
if ($r.Success)
{
    echo "Updating Assembly Version to $args ...";
    Update-AllAssemblyInfoFiles $args[0];
}
else
{
    echo ” “;
    echo “Error: Input version does not match x.y.z format!”
    echo ” “;
    echo "Unable to apply version to AssemblyInfo.cs files";
}
