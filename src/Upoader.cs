using System.Net;
using System.Text.RegularExpressions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace PackageGenerator;

public class Uploader
{
    private static readonly Regex packageRegex = new(@"(.+)\.(\d+\.\d+\.\d+)", RegexOptions.Compiled);
    private readonly NuGetLogger nugetLogger = new();
    private readonly Login _login = new();
    private string? key;

    public async Task UploadAsync(IReadOnlyList<string> packageFiles, CancellationToken cancellationToken)
    {
        packageFiles = packageFiles.OrderBy(p => p).ToArray();
        Console.WriteLine("Starting upload of {0} packages...", packageFiles.Count);
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var searchResource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        var uploadResource = await repository.GetResourceAsync<PackageUpdateResource>(cancellationToken);
        var cache = new SourceCacheContext();
        key = await _login.GetTokenAsync(cancellationToken);
        foreach (var packageFile in packageFiles)
        {
            var match = packageRegex.Match(Path.GetFileNameWithoutExtension(packageFile));
            if (!match.Success)
            {
                Console.WriteLine("Filename {0} is not recognized as package", packageFile);
                continue;
            }
            var packageId = match.Groups[1].Value;
            var version = match.Groups[2].Value;
            if (!await searchResource.DoesPackageExistAsync(packageId, NuGetVersion.Parse(version), cache, nugetLogger, cancellationToken))
            {
                await UploadPackageAsync(packageFile, uploadResource, cancellationToken);
            }
        }
    }

    private async Task UploadPackageAsync(string packageFile, PackageUpdateResource uploadResource, CancellationToken cancellationToken)
    {
        try
        {
            await uploadResource.PushAsync([
                    packageFile
                ],
                null,
                300,
                false,
                _ => key,
                _ => null,
                false,
                true,
                false,
                true,
                nugetLogger
            );
            Console.WriteLine("Uploaded package: {0}", Path.GetFileName(packageFile));
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("Rate limit exceeded. Waiting for 65 minutes before retrying...");
                Console.WriteLine(ex);
                await Task.Delay(TimeSpan.FromMinutes(65), cancellationToken);
                Console.WriteLine("Retrying upload of package: {0}", Path.GetFileName(packageFile));
                key = await _login.GetTokenAsync(cancellationToken);
                await UploadPackageAsync(packageFile, uploadResource, cancellationToken);
            }
            else
            {
                Console.WriteLine("Error uploading package: {0}", Path.GetFileNameWithoutExtension(packageFile));
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.HttpRequestError);
            }
        }
    }
}