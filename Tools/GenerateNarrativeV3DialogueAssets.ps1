$ErrorActionPreference = 'Stop'
$desktop = 'C:\Users\Usuario\OneDrive\' + ([char]0x00C1) + 'rea de Trabalho'
$doc = Join-Path $desktop 'MementoMori_PJ\Docs Base\Memento_Mori_Biblia_Narrativa_Roteiro_Jogavel_V3.docx'
if (-not (Test-Path -LiteralPath $doc)) { throw "Narrative V3 Bible DOCX was not found at $doc." }
$root = Join-Path $PSScriptRoot '..\Assets\Resources\Dialogue'
$catalog = Join-Path $PSScriptRoot '..\Assets\Scripts\Dialogue\NarrativeV3DialogueCatalog.cs'

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($doc)
$entry = $zip.GetEntry('word/document.xml')
$reader = New-Object IO.StreamReader($entry.Open())
$xml = $reader.ReadToEnd()
$reader.Close(); $zip.Dispose()
$text = $xml -replace '</w:p>', "`n" -replace '<w:tab[^>]*/>', "`t" -replace '<[^>]+>', ''
$lines = $text -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ }
$ids = @($lines | Where-Object { $_ -match '^DLG_[A-Z0-9_]+$' } | Select-Object -Unique)
if ($ids.Count -ne 82) { throw "Expected 82 V3 ids, found $($ids.Count)." }
$speakers = @('Melantha', 'Andrealphus', 'Poe', 'Narração', 'Voz de Melantha', 'Voz', 'Tela')

function Quote([string]$value) {
    $value = $value.Replace('\', '\\').Replace('"', '\"')
    return '"' + $value + '"'
}

foreach ($id in $ids) {
    $start = [Array]::IndexOf($lines, $id)
    $end = $lines.Count
    for ($i = $start + 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^DLG_[A-Z0-9_]+$') { $end = $i; break }
    }
    $pairs = New-Object System.Collections.Generic.List[object]
    $started = $false
    for ($i = $start + 1; $i -lt $end - 1; $i++) {
        if ($speakers -contains $lines[$i] -and $speakers -notcontains $lines[$i + 1]) {
            $pairs.Add([pscustomobject]@{ Speaker = $lines[$i]; Text = $lines[$i + 1] })
            $started = $true
            $i++
        } elseif ($started) {
            break
        }
    }
    if ($pairs.Count -eq 0) { throw "No dialogue lines found for $id." }
    $out = @('%YAML 1.1', '%TAG !u! tag:unity3d.com,2011:', '--- !u!114 &11400000', 'MonoBehaviour:', '  m_Script: {fileID: 11500000, guid: 8d1474fa99a3daa499fa16618548882f, type: 3}', "  m_Name: $id", '  m_EditorClassIdentifier: MementoMori.Runtime::MementoMori.Dialogue.DialogueData', "  sequenceId: $id", '  lines:')
    foreach ($pair in $pairs) { $out += "  - {Text: $(Quote $pair.Text), Speaker: $(Quote $pair.Speaker), CharactersPerSecond: 38, PauseBefore: 0, PauseAfter: 0, Thought: 0, Narration: 0, LockMovement: 1}" }
    $out += '  conditions: []'
    [IO.File]::WriteAllText((Join-Path $root "$id.asset"), ($out -join "`r`n") + "`r`n", (New-Object Text.UTF8Encoding($false)))
}

$array = ($ids | ForEach-Object { '            "' + $_ + '"' }) -join ",`r`n"
$source = @"
namespace MementoMori.Dialogue
{
    public static class NarrativeV3DialogueCatalog
    {
        public static readonly string[] Ids =
        {
$array
        };
    }
}
"@
[IO.File]::WriteAllText($catalog, $source.TrimStart(), (New-Object Text.UTF8Encoding($false)))
Write-Output "Generated $($ids.Count) dialogue assets and catalog."
