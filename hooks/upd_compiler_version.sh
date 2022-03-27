version=$(xidel ECMSBuildTasks.csproj -s -e '//PackageReference[@Include="Microsoft.Net.Compilers.Toolset"]/Version')
cat<<EOF > ECXModuleDLLTask.Generated.cs
namespace ECMSBuildTasks {
  public partial class ECXModuleDLLTask
  {
      private string CompilerVersion => "$version";
  }
}
EOF
