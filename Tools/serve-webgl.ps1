param(
    [string]$Root = "D:\Unity Projects\QA - Final\Builds\WebGL",
    [int]$Port = 8850
)

Add-Type -AssemblyName System.Net.HttpListener -ErrorAction SilentlyContinue

$mimeTypes = @{
    ".html" = "text/html"
    ".js"   = "application/javascript"
    ".wasm" = "application/wasm"
    ".data" = "application/octet-stream"
    ".css"  = "text/css"
    ".json" = "application/json"
    ".png"  = "image/png"
    ".ico"  = "image/x-icon"
    ".gz"   = "application/gzip"
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Start()
Write-Host "Serving $Root at http://localhost:$Port/"

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response
        try {
            $path = $request.Url.LocalPath
            if ($path -eq "/") { $path = "/index.html" }
            $filePath = Join-Path $Root ($path -replace "^/", "")
            $filePath = [System.IO.Path]::GetFullPath($filePath)

            if (-not $filePath.StartsWith([System.IO.Path]::GetFullPath($Root))) {
                $response.StatusCode = 403
            } elseif (Test-Path $filePath -PathType Leaf) {
                $ext = [System.IO.Path]::GetExtension($filePath)
                $contentType = $mimeTypes[$ext]
                if (-not $contentType) { $contentType = "application/octet-stream" }
                $response.ContentType = $contentType
                $bytes = [System.IO.File]::ReadAllBytes($filePath)
                $response.ContentLength64 = $bytes.Length
                $response.OutputStream.Write($bytes, 0, $bytes.Length)
            } else {
                $response.StatusCode = 404
            }
        } catch {
            $response.StatusCode = 500
        } finally {
            $response.OutputStream.Close()
        }
    }
} finally {
    $listener.Stop()
}
