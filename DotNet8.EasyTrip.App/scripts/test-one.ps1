$f = 'd:\Easy-Trip System\DotNet8.EasyTrip.App\DotNet8.EasyTrip.App.Client\Pages\Transactions\Transactions.razor'
$content = Get-Content -LiteralPath $f -Raw -Encoding UTF8
Write-Host "Length: $($content.Length)"
Write-Host "Has @code: $($content.Contains('@code'))"
$idx = $content.IndexOf('@code')
Write-Host "Index: $idx"
