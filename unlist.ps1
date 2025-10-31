param($key)

for ($i = 1; $i -le 557; $i++) {
    dotnet nuget delete "LimitTester.Package$($i.ToString("0000")).1.0.0.nupkg" --api-key $key --source https://api.nuget.org/v3/index.json --non-interactive
}
