#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [string]$ApiProject = "src/Game.Api/Game.Api.csproj",
    [string]$ClientProject = "src/Game.Client/Game.Client.csproj"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

function Start-DevServer {
    param(
        [string]$Name,
        [string[]]$Arguments
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    foreach ($arg in $Arguments) {
        $null = $psi.ArgumentList.Add($arg)
    }
    $psi.WorkingDirectory = $repoRoot
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $process.EnableRaisingEvents = $true

    if (-not $process.Start()) {
        throw "Failed to start $Name process."
    }

    $outputHandler = [System.Diagnostics.DataReceivedEventHandler]{
        param($sender, $eventArgs)
        if ($eventArgs.Data) {
            Write-Host ("[{0}] {1}" -f $Name, $eventArgs.Data)
        }
    }

    $errorHandler = [System.Diagnostics.DataReceivedEventHandler]{
        param($sender, $eventArgs)
        if ($eventArgs.Data) {
            Write-Host ("[{0}] {1}" -f $Name, $eventArgs.Data) -ForegroundColor Red
        }
    }

    $process.add_OutputDataReceived($outputHandler)
    $process.add_ErrorDataReceived($errorHandler)
    $process.BeginOutputReadLine()
    $process.BeginErrorReadLine()

    return [pscustomobject]@{
        Name = $Name
        Process = $process
        OutputHandler = $outputHandler
        ErrorHandler = $errorHandler
    }
}

$servers = @(
    Start-DevServer -Name "API " -Arguments @("watch", "--project", $ApiProject, "run"),
    Start-DevServer -Name "CLNT" -Arguments @("watch", "--project", $ClientProject, "run")
)

Write-Host "Launched API + Client watchers. Press Ctrl+C to stop both processes." -ForegroundColor Cyan

$stopRequested = $false
$cancelHandler = [System.ConsoleCancelEventHandler]{
    param($sender, $eventArgs)
    $script:stopRequested = $true
    $eventArgs.Cancel = $true
    Write-Host "`nStopping dev servers..." -ForegroundColor Yellow
}
[System.Console]::CancelKeyPress += $cancelHandler

try {
    while (-not $stopRequested) {
        if ($servers | Where-Object { $_.Process.HasExited }) {
            Write-Warning "One of the dev servers exited. Stopping the rest."
            break
        }

        Start-Sleep -Seconds 1
    }
}
finally {
    [System.Console]::CancelKeyPress -= $cancelHandler

    foreach ($server in $servers) {
        $proc = $server.Process
        if (-not $proc.HasExited) {
            try {
                $proc.Kill($true)
                $proc.WaitForExit()
            }
            catch {
                Write-Warning "Failed to stop $($server.Name) cleanly: $_"
            }
        }

        $proc.remove_OutputDataReceived($server.OutputHandler)
        $proc.remove_ErrorDataReceived($server.ErrorHandler)
        $proc.Dispose()
    }
}
