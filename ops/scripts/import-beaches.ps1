param(
    [Parameter(Mandatory = $true)]
    [string]$Email,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [string]$ApiBaseUrl = "http://localhost:5143/api",

    [string]$FilePath = ".\\ops\\data\\beaches.template.json"
)

$ErrorActionPreference = "Stop"

function Get-AccessToken {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$LoginResponse
    )

    if ($LoginResponse.data -and $LoginResponse.data.accessToken) {
        return $LoginResponse.data.accessToken
    }

    if ($LoginResponse.accessToken) {
        return $LoginResponse.accessToken
    }

    if ($LoginResponse.data -and $LoginResponse.data.token) {
        return $LoginResponse.data.token
    }

    if ($LoginResponse.token) {
        return $LoginResponse.token
    }

    throw "Login response icinde access token bulunamadi."
}

$resolvedFilePath = Resolve-Path -LiteralPath $FilePath
$payload = Get-Content -LiteralPath $resolvedFilePath -Raw | ConvertFrom-Json

if (-not $payload -or $payload.Count -eq 0) {
    throw "Import dosyasi bos."
}

$loginBody = @{
    email = $Email
    password = $Password
} | ConvertTo-Json

Write-Host "Admin girisi yapiliyor: $ApiBaseUrl/Auth/login"
$loginResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$ApiBaseUrl/Auth/login" `
    -ContentType "application/json" `
    -Body $loginBody

$accessToken = Get-AccessToken -LoginResponse $loginResponse

Write-Host "Toplam kayit: $($payload.Count)"
$importBody = $payload | ConvertTo-Json -Depth 8

$importResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$ApiBaseUrl/Admin/beaches/import" `
    -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $accessToken" } `
    -Body $importBody

Write-Host ""
Write-Host "Import tamamlandi."
$importResponse | ConvertTo-Json -Depth 8
