# Test script for invoice upload endpoint (PowerShell 5+ compatible)
# Usage: .\test-upload-v5.ps1 -FilePath "path\to\invoice.pdf"

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
    # Get file info
    $fileInfo = Get-Item -Path $FilePath
    $fileName = $fileInfo.Name
    $fileBytes = [System.IO.File]::ReadAllBytes($FilePath)

    # Create boundary for multipart/form-data
    $boundary = [System.Guid]::NewGuid().ToString()
    $LF = "`r`n"

    # Build multipart form data body
    $bodyLines = @(
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
        "Content-Type: application/octet-stream$LF",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
        "--$boundary--$LF"
    )

    $body = $bodyLines -join $LF

    Write-Host "Uploading invoice..." -ForegroundColor Yellow
    Write-Host "File size: $([math]::Round($fileBytes.Length / 1KB, 2)) KB" -ForegroundColor Gray

    # Send request
    $response = Invoke-WebRequest `
        -Uri $ApiUrl `
        -Method Post `
        -ContentType "multipart/form-data; boundary=$boundary" `
        -Body ([System.Text.Encoding]::GetEncoding("iso-8859-1").GetBytes($body))

    Write-Host "`nSuccess!" -ForegroundColor Green
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "`nResponse:" -ForegroundColor Cyan

    # Pretty print JSON response
    $json = $response.Content | ConvertFrom-Json

    Write-Host "`nInvoice Details:" -ForegroundColor Cyan
    Write-Host "  ID: $($json.id)" -ForegroundColor White
    Write-Host "  File: $($json.fileName)" -ForegroundColor White
    Write-Host "  Upload Date: $($json.uploadDate)" -ForegroundColor White

    if ($json.extractedData) {
        Write-Host "`nExtracted Data:" -ForegroundColor Cyan
        Write-Host "  Vendor: $($json.extractedData.vendor)" -ForegroundColor White
        Write-Host "  Invoice #: $($json.extractedData.invoiceNumber)" -ForegroundColor White
        Write-Host "  Date: $($json.extractedData.invoiceDate)" -ForegroundColor White
        Write-Host "  Amount: $($json.extractedData.totalAmount) $($json.extractedData.currency)" -ForegroundColor White
    }

    if ($json.classification) {
        Write-Host "`nClassification:" -ForegroundColor Cyan
        Write-Host "  Category: $($json.classification.category)" -ForegroundColor White
        Write-Host "  Confidence: $([math]::Round($json.classification.confidence * 100, 2))%" -ForegroundColor White
        if ($json.classification.reasoning) {
            Write-Host "  Reasoning: $($json.classification.reasoning)" -ForegroundColor Gray
        }
    }

    if ($json.processingMetadata) {
        Write-Host "`nProcessing:" -ForegroundColor Cyan
        Write-Host "  Time: $([math]::Round($json.processingMetadata.processingTime, 2)) seconds" -ForegroundColor White
        Write-Host "  Status: $($json.processingMetadata.status)" -ForegroundColor White
    }

    Write-Host "`nFull JSON Response:" -ForegroundColor Gray
    Write-Host ($json | ConvertTo-Json -Depth 10) -ForegroundColor DarkGray

} catch {
    Write-Host "`nError occurred!" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            Write-Host "`nError Details:" -ForegroundColor Yellow
            Write-Host $errorBody -ForegroundColor White
        } catch {
            Write-Host "Could not read error response" -ForegroundColor Gray
        }
    }
}
