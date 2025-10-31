// See https://aka.ms/new-console-template for more information

using NuGet.Packaging;
using NuGet.Packaging.Licenses;

Console.WriteLine("Hello, World!");

var output = "./packages";
if (!Directory.Exists(output)) 
    Directory.CreateDirectory(output);

for (int i = 1; i <= 1500; i++)
{
    var packageBuilder = new PackageBuilder();
    packageBuilder.Title = packageBuilder.Id = $"LimitTester.Package{i}";
    packageBuilder.Authors.Add("Lo_Wang");
    packageBuilder.Version = new NuGet.Versioning.NuGetVersion(1, 0, 0);
    packageBuilder.Description = "Package description";
    packageBuilder.EmitRequireLicenseAcceptance = false;
    packageBuilder.LicenseMetadata = new LicenseMetadata(LicenseType.Expression, "MIT", NuGetLicenseExpression.Parse("MIT"), [], new Version(1, 0, 0));
    packageBuilder.Files.Add(new PhysicalPackageFile
    {
        SourcePath = "./nope.txt",
        TargetPath = "nope.txt"
    });
    var fileName = Path.Combine(output, $"{packageBuilder.Id}.{packageBuilder.Version.ToNormalizedString()}.nupkg");
    if (File.Exists(fileName))
        File.Delete(fileName);
    using var fs = new FileStream(fileName, FileMode.Create);
    packageBuilder.Save(fs);
}
