$ErrorActionPreference = 'Stop'
$client = (Resolve-Path (Join-Path $PSScriptRoot '..\DotNet8.EasyTrip.App.Client')).Path
$skip = @('_Imports.razor', 'Routes.razor')

$usings = @'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

'@

function Get-Namespace([string]$dirPath) {
    if ([string]::IsNullOrWhiteSpace($dirPath)) { return 'DotNet8.EasyTrip.App.Client.Pages' }
    $parts = $dirPath -split '\\'
    if ($parts[0] -eq 'Pages' -and $parts.Count -eq 1) { return 'DotNet8.EasyTrip.App.Client.Pages' }
    if ($parts[0] -eq 'Pages') { return 'DotNet8.EasyTrip.App.Client.Pages.' + ($parts[1..($parts.Count-1)] -join '.') }
    if ($parts[0] -eq 'Layout') { return 'DotNet8.EasyTrip.App.Client.Layout' }
    if ($parts[0] -eq 'Components') { return 'DotNet8.EasyTrip.App.Client.Components' }
    return 'DotNet8.EasyTrip.App.Client'
}

function Extract-Style([string]$content) {
    $css = [regex]::Matches($content, '(?s)<style>\s*(.*?)\s*</style>') | ForEach-Object { $_.Groups[1].Value.Trim() }
    $markup = [regex]::Replace($content, '(?s)<style>\s*.*?\s*</style>', '').Trim()
    $cssText = ($css -join "`n`n").Replace('@@', '@')
    return $markup, $cssText
}

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

function Is-PlaceholderCs([string]$text) {
    if ([string]::IsNullOrWhiteSpace($text)) { return $true }
    if ($text -match 'all logic is contained within') { return $true }
    return $false
}

function Build-Cs($ns, $class, $body) {
    $indented = ($body -split "`n" | ForEach-Object { if ($_.Trim()) { '        ' + $_ } else { $_ } }) -join "`n"
    return $usings + "namespace $ns`n{`n    public partial class $class`n    {`n$indented`n    }`n}`n"
}

function Merge-Cs([string]$existing, [string]$body) {
    $marker = "`n    }`n}`n"
    $idx = $existing.LastIndexOf($marker)
    if ($idx -lt 0) { return $existing }
    $indented = ($body -split "`n" | ForEach-Object { if ($_.Trim()) { '        ' + $_ } else { $_ } }) -join "`n"
    return $existing.Substring(0, $idx) + "`n" + $indented + $existing.Substring($idx)
}

function Ensure-Inherits([string]$markup) {
    if ($markup -match '@inherits ComponentBase') { return $markup }
    $lines = $markup -split "`n"
    $insertAt = 0
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim().StartsWith('@') -and -not $lines[$i].Trim().StartsWith('@using')) { $insertAt = $i + 1 }
        elseif ($insertAt -gt 0 -and -not $lines[$i].Trim().StartsWith('@')) { break }
    }
    if ($insertAt -eq 0) { return "@inherits ComponentBase`n`n$markup" }
    $before = $lines[0..($insertAt-1)]
    $after = $lines[$insertAt..($lines.Count-1)]
    return (($before + '@inherits ComponentBase' + $after) -join "`n")
}

Write-Host "Client root: $client"
$razors = Get-ChildItem -Path $client -Recurse -Filter '*.razor'
Write-Host "Found $($razors.Count) razor files"
$count = 0
$razors | ForEach-Object {
    if ($skip -contains $_.Name) { return }
    $rel = $_.FullName.Substring($client.Length + 1)
    $top = ($rel -split '\\')[0]
    if ($top -notin @('Pages','Layout','Components')) { return }

    $content = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
    $markup, $cssText = Extract-Style $content
    $markup, $codeBody = Extract-Code $markup
    if (-not $codeBody -and -not $cssText) { return }

    $dirPath = Split-Path $rel -Parent
    $ns = Get-Namespace $dirPath
    $class = $_.BaseName
    $csPath = $_.FullName + '.cs'
    $cssPath = $_.FullName + '.css'

    if ($codeBody) {
        if (Test-Path $csPath) {
            $existing = Get-Content $csPath -Raw
            if (Is-PlaceholderCs $existing) {
                Set-Content -Path $csPath -Value (Build-Cs $ns $class $codeBody) -Encoding utf8NoBOM
            } else {
                Set-Content -Path $csPath -Value (Merge-Cs $existing $codeBody) -Encoding utf8NoBOM
            }
        } else {
            Set-Content -Path $csPath -Value (Build-Cs $ns $class $codeBody) -Encoding utf8NoBOM
        }
        Write-Host "CS: $rel"
    }

    if ($cssText) {
        Set-Content -Path $cssPath -Value ($cssText + "`n") -Encoding utf8NoBOM
        Write-Host "CSS: $rel"
    }

    $markup = Ensure-Inherits $markup
    # Move @inject to code-behind pattern: keep in razor OR move to cs - existing Bus uses [Inject] in cs
    # Convert @inject lines to remove from markup (optional) - script leaves them for now
    Set-Content -Path $_.FullName -Value ($markup + "`n") -Encoding utf8NoBOM
    Write-Host "RAZOR: $rel"
    $count++
}

Write-Host "Done. Updated $count files."
