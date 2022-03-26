version=$(xidel ECMSBuildTasks.csproj -s -e '//PackageReference[@Include="Microsoft.Net.Compilers.Toolset"]/Version')
sed -i -e "s/CompilerVersion => ".*";/CompilerVersion => \"$version\";/" ECXModuleDLLTask.cs
