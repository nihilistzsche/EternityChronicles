$version=..\Extern\xidel.exe ECMSBuildTasks.csproj -s -e '//PackageReference[@Include="Microsoft.Net.Compilers.Toolset"]/Version')
@"namespace ECMSBuildTasks {
  public partial class ECXModuleDLLTask
  {
      private string CompilerVersion => "$version";
  }
}
"@ | Out-File -FilePath ECXModuleDLLTask.Generated.cs
exit 0
