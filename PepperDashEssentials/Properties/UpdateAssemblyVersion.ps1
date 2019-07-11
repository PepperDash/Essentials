#Set-ExecutionPolicy RemoteSigned
function Usage
{
echo "This is ";
echo “Usage: “;
echo ” from cmd.exe: “;
echo ” powershell.exe SetVersion.ps1 2.8.3.0″;
echo ” “;
echo ” from powershell.exe prompt: “;
echo ” .\SetVersion.ps1 2.8.3.0″;
echo ” “;
}

function Update-SourceVersion
{
    Param ([string]$Version)
    $NewVersion = ‘AssemblyVersion("‘ + $Version + ‘.*")’;
    foreach ($o in $input)
    {
        Write-output $o.FullName
        $TmpFile = $o.FullName + “.tmp”
        get-content $o.FullName |
        %{$_ -replace ‘AssemblyVersion\("(\d+\.\d+\.\d+)\.\*"\)’, $NewVersion  }  > $TmpFile
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
$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "^\d+\.\d+\.\d+$");
    if ($r.Success)
    {
        Update-AllAssemblyInfoFiles $args[0];
    }
    else
    {
    echo ” “;
    echo “Bad Input!”
    echo ” “;
    Usage ;
}
