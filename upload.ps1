param($key)

function Upload($package) {
    dotnet nuget push $package --api-key $key --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) {
        return $false;
    }
    return $true;
}

$packages = Get-ChildItem ./src/bin/Debug/net8.0/packages

$counter = 0
foreach ($package in $packages) {
    $result = Upload -package $packages.FullName
    if ($result) {
        $counter++
    } else {
        break;
    }
}
Write-Host "Uploaded packages: $counter"
if ($counter -ne 1500) {
    exit 1
}
