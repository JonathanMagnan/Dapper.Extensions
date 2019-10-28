#addin Cake.Git

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var output=Argument<string>("output", "Output");
var version=Argument<string>("version", "2.3.0-rc");
var target = Argument<string>("target", "Default");
var release = Argument<bool>("release", true);
var nugetApiKey = Argument<string>("nugetApiKey", null);
var currentBranch = Argument<string>("currentBranch", GitBranchCurrent("./").FriendlyName);
var configuration=release?"Release":"Debug";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("UpdateVersion").DoesForEach(GetFiles("**/Dapper.Extensions*.csproj"),(file)=>{
   Information("Update Version:"+file);
   XmlPoke(file,"/Project/PropertyGroup/Version",version);
   XmlPoke(file,"/Project/PropertyGroup/GeneratePackageOnBuild","false");
   XmlPoke(file,"/Project/PropertyGroup/Description","A dapper extension library. Support MySql,SQL Server,MSSQL,PostgreSql,SQLite and ODBC, Support cache.");
   XmlPoke(file,"/Project/PropertyGroup/PackageProjectUrl","https://github.com/1100100/Dapper.Extensions");
   XmlPoke(file,"/Project/PropertyGroup/PackageTags","DapperExtensions,Dapper Extensions,DapperExtensions,Dapper,Dapper.Extensions.NetCore,Dapper.Extensions,Dapper,Extensions,DataBase,MsSql,Sql Server,MSSQL,MySql,PostgreSql,SQLite,ODBC,Cahce,Caching,Redis,Memory,Redis Caching,Memory caching");
   XmlPoke(file,"/Project/PropertyGroup/PackageIconUrl","https://raw.githubusercontent.com/1100100/Dapper.Extensions/master/icon.jpg");
   XmlPoke(file,"/Project/PropertyGroup/Authors","Owen");
   XmlPoke(file,"/Project/PropertyGroup/PackageLicenseExpression","MIT");
});

Task("Restore").Does(()=>{
   DotNetCoreRestore();
});

Task("Build").Does(()=>{
   DotNetCoreBuild("Dapper.Extensions.sln",new DotNetCoreBuildSettings{
      Configuration=configuration
   });
});

Task("CleanPackage").Does(()=>{
   if(DirectoryExists(output))
   {
      DeleteDirectory(output,true);
   }
});

Task("Pack")
.IsDependentOn("CleanPackage")
.IsDependentOn("UpdateVersion")
.DoesForEach(GetFiles("**/Dapper.Extensions*.csproj"),(file)=>{
   DotNetCorePack(file.ToString(),new DotNetCorePackSettings{
      OutputDirectory=output,
      Configuration=configuration
   });
});

Task("Push")
.IsDependentOn("Pack")
.Does(()=>{
   var nuGetPushSettings= new NuGetPushSettings {
      Source="https://www.nuget.org/api/v2/package",
      ApiKey=nugetApiKey
   };
   if(currentBranch=="master")
   {
      foreach (var package in GetFiles("Output/*.nupkg"))
      {
         NuGetPush(package,nuGetPushSettings);
      }
   }
   else
   {
      Information("Non-master build. Not publishing to NuGet. Current branch: " + currentBranch);
   }
});

Task("Default")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() => {

});



RunTarget(target);