# Test script for invoice upload endpoint
# Usage: .\test-upload.ps1 -FilePath "path\to\invoice.pdf"

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,

    [string]$ApiUrl = "http://localhost:7071/api/upload"
)

Write-Host "Testing Invoice Upload API" -ForegroundColor Cyan
Write-Host "File: $FilePath" -ForegroundColor Yellow
Write-Host "API: $ApiUrl" -ForegroundColor Yellow
Write-Host ""

# Check if file exists
if (-not (Test-Path $FilePath)) {
    Write-Host "Error: File not found!" -ForegroundColor Red
    exit 1
}

try {
    # Create form data
    $form = @{
        file = Get-Item -Path $FilePath
    }

    Write-Host "Uploading invoice..." -ForegroundColor Yellow

    # Send request
    $response = Invoke-WebRequest -Uri $ApiUrl -Method Post -Form $form

    Write-Host "`nSuccess!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "`nResponse:" -ForegroundColor Cyan

    # Pretty print JSON response
    $json = $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
    Write-Host $json -ForegroundColor White

} catch {
    Write-Host "`nError occurred!" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "`nError Details:" -ForegroundColor Yellow
        Write-Host $errorBody -ForegroundColor White
    }
}
