version=$(xidel $1/ECMSBuildTasks.csproj -s -e '//PackageReference[@Include="Microsoft.Net.Compilers.Toolset"]/Version')
sed -i -e "s/CompilerVersion =&gt; .+;/CompilerVersion = \"$version\";/" $1/ECXModuleDLLTask.cs
