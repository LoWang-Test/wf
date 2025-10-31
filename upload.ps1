param($key)

function Upload($package) {
    (dotnet nuget push $package --api-key $key --source https://api.nuget.org/v3/index.json) | Tee-Object -Variable output | Out-Null
    $output = ""
    #(& bash ./dump.sh $package $key) | Tee-Object -Variable output | Out-Null
    Write-Host $output
    if ($LASTEXITCODE -ne 0) {
        return $false;
    }
    return $true;
}

$packages = Get-ChildItem ./packages

$counter = 0
foreach ($package in $packages) {
    $result = Upload -package $package.FullName
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
