$f = 'd:\Easy-Trip System\DotNet8.EasyTrip.App\DotNet8.EasyTrip.App.Client\Pages\Transactions\Transactions.razor'
$content = Get-Content -LiteralPath $f -Raw -Encoding UTF8

function Extract-Code([string]$content) {
    $idx = $content.IndexOf('@code')
    if ($idx -lt 0) { return $content, $null }
    $start = $content.IndexOf('{', $idx)
    $depth = 0
    $end = $start
    for ($i = $start; $i -lt $content.Length; $i++) {
        if ($content[$i] -eq '{') { $depth++ }
        elseif ($content[$i] -eq '}') {
            $depth--
            if ($depth -eq 0) { $end = $i; break }
        }
    }
    $body = $content.Substring($start + 1, $end - $start - 1).Trim()
    $markup = ($content.Substring(0, $idx) + $content.Substring($end + 1)).Trim()
    return $markup, $body
}

$markup, $codeBody = Extract-Code $content
Write-Host "Code body length: $($codeBody.Length)"
