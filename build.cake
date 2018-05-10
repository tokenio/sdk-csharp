#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Done");
var configuration = Argument("configuration", "Debug");

Task("Build")
  .Does(() =>
{
  MSBuild("sdk/sdk.csproj");
});

Task("Build-Samples")
  .IsDependentOn("Build")
  .Does(() =>
{
  MSBuild("samples/samples.csproj");
});

Task("Build-Tests")
  .IsDependentOn("Build-Samples")
  .Does(() =>
{
  MSBuild("tests/tests.csproj");
});

Task("Run-Tests")
  .IsDependentOn("Build-Tests")
  .Does(() =>
{
  NUnit3("tests/bin/" + configuration + "/tests.dll",
  new NUnit3Settings {
    NoResults = true,
    Labels = NUnit3Labels.All
  });
});

Task("Done")
  .IsDependentOn("Run-Tests");

RunTarget(target);
