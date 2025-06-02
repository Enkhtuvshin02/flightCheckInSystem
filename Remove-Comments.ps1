# PowerShell script to remove all comments from C# files
$rootDir = "c:\Users\user\source\repos\FlightCheckInSystem"
$files = Get-ChildItem -Path $rootDir -Include *.cs -Recurse | 
    Where-Object { $_.FullName -notlike '*\obj\*' -and $_.FullName -notlike '*\bin\*' }

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove single-line comments (//...)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "//.*?$\r?\n", [System.Text.RegularExpressions.MatchEvaluator]{
        param($match)
        if ($match.Value.Trim().StartsWith("///")) {
            return $match.Value  # Preserve XML documentation comments
        }
        return $match.Value.TrimEnd() + "`r`n"
    }, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    # Remove multi-line comments (/* ... */)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "/\*.*?\*/", "", 
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    # Save the file with the same encoding
    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
    Write-Host "Processed: $($file.FullName)"
}

Write-Host "Comment removal complete!"
