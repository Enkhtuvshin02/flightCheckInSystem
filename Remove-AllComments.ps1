# PowerShell script to remove all comments from C# files, including those in string literals
$rootDir = "c:\Users\user\source\repos\FlightCheckInSystem"
$files = Get-ChildItem -Path $rootDir -Include *.cs -Recurse | 
    Where-Object { $_.FullName -notlike '*\obj\*' -and $_.FullName -notlike '*\bin\*' }

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Remove single-line comments (//...)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, 
        @"
        (?<!")           # Not preceded by a quote
        (                 # Start capture group
            (?:\\.|[^"\\])*?  # Any character except unescaped quote or backslash
            (?:""|'(?!'')*?  # Allow escaped quotes or single quotes that aren't part of a comment
        )
        |
        (//.*?$\r?\n)    # Match single-line comments
        "@, 
        {
            param($match)
            if ($match.Groups[1].Success) {
                return $match.Groups[1].Value  # Preserve string literals
            }
            return ""  # Remove comments
        },
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline -bor [System.Text.RegularExpressions.RegexOptions]::IgnorePatternWhitespace)
    
    # Remove multi-line comments (/* ... */)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, 
        @"
        (?<!")           # Not preceded by a quote
        (                 # Start capture group
            (?:\\.|[^"\\])*?  # Any character except unescaped quote or backslash
            (?:""|'(?!'')*?  # Allow escaped quotes or single quotes that aren't part of a comment
        )
        |
        /\*.*?\*/       # Match multi-line comments
        "@, 
        {
            param($match)
            if ($match.Groups[1].Success) {
                return $match.Groups[1].Value  # Preserve string literals
            }
            return ""  # Remove comments
        },
        [System.Text.RegularExpressions.RegexOptions]::Singleline -bor [System.Text.RegularExpressions.RegexOptions]::IgnorePatternWhitespace)
    
    # Save the file with the same encoding
    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
    Write-Host "Processed: $($file.FullName)"
}

Write-Host "Complete comment removal finished!"
