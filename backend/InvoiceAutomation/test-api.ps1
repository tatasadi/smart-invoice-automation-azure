# Complete test script for all API endpoints

param(
    [string]$BaseUrl = "http://localhost:7071/api"
)

Write-Host "=== Invoice Automation API Test ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Get all invoices
Write-Host "1. Testing GET /api/invoices" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/invoices" -Method Get
    $invoices = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Success! Found $($invoices.count) invoices" -ForegroundColor Green

    if ($invoices.invoices -and $invoices.invoices.Count -gt 0) {
        Write-Host "   Recent invoices:" -ForegroundColor Yellow
        $invoices.invoices | Select-Object -First 3 | ForEach-Object {
            Write-Host "     - $($_.fileName) ($(if($_.extractedData){$_.extractedData.vendor}else{'N/A'}))" -ForegroundColor White
        }
    }
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: Upload invoice (if file path provided)
if ($args.Count -gt 0 -and (Test-Path $args[0])) {
    $filePath = $args[0]
    Write-Host "2. Testing POST /api/upload" -ForegroundColor Cyan
    Write-Host "   File: $filePath" -ForegroundColor Yellow

    try {
        $form = @{
            file = Get-Item -Path $filePath
        }

        $response = Invoke-WebRequest -Uri "$BaseUrl/upload" -Method Post -Form $form
        $result = $response.Content | ConvertFrom-Json

        Write-Host "   ✓ Upload successful!" -ForegroundColor Green
        Write-Host "   Invoice ID: $($result.id)" -ForegroundColor White
        Write-Host "   Vendor: $($result.extractedData.vendor)" -ForegroundColor White
        Write-Host "   Amount: $($result.extractedData.totalAmount) $($result.extractedData.currency)" -ForegroundColor White
        Write-Host "   Category: $($result.classification.category) (confidence: $($result.classification.confidence))" -ForegroundColor White
        Write-Host "   Processing Time: $($result.processingMetadata.processingTime) seconds" -ForegroundColor White

        # Save invoice ID for next test
        $script:testInvoiceId = $result.id

    } catch {
        Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
}

# Test 3: Get specific invoice (if we have an ID)
if ($script:testInvoiceId) {
    Write-Host "3. Testing GET /api/invoice/{id}" -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/invoice/$script:testInvoiceId" -Method Get
        $invoice = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Retrieved invoice successfully!" -ForegroundColor Green
        Write-Host "   File: $($invoice.fileName)" -ForegroundColor White
    } catch {
        Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan

# Usage instructions
Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  # Test all endpoints (without upload):" -ForegroundColor Gray
Write-Host "  .\test-api.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "  # Test all endpoints including upload:" -ForegroundColor Gray
Write-Host "  .\test-api.ps1 'path\to\invoice.pdf'" -ForegroundColor Gray
