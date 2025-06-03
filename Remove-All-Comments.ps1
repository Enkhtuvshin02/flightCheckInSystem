# PowerShell script to remove ALL comments from C# files
$rootDir = "c:\Users\user\source\repos\FlightCheckInSystem"
$files = Get-ChildItem -Path $rootDir -Include *.cs -Recurse | 
    Where-Object { $_.FullName -notlike '*\obj\*' -and $_.FullName -notlike '*\bin\*' }

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove all single-line comments (including ///)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "//.*?$\r?\n", [System.Text.RegularExpressions.MatchEvaluator]{
        return [String]::Empty
    }, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove all multi-line comments (/* ... */)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "/\*.*?\*/", "", 
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    # Remove any remaining comment markers at the end of lines
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "//.*$", "", 
        [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Save the file with the same encoding
    [System.IO.File]::WriteAllText($file.FullName, $content.Trim(), [System.Text.Encoding]::UTF8)
    Write-Host "Processed: $($file.Name)"
}

Write-Host "All comments have been removed from the project files."
