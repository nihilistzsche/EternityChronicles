EternityChronicles is aiming to be a type of dynamic MUD written in C# and utilizing the DynamicLanguageRuntime feature of the .NET Framework.

EternityChronicles consists of the following subprojects:
  - CSLog - A simple logging library based on the OCLog library for Objective-C.
  - DragonMUD - The mud engine itself.
  - ECMSBuildTasks - A collection of custom MSBuild tasks used in building the solution.
  - ECX (ECX.Core and ECX.Core.Module) - A plugin framework for .NET previously known as nmodule, now living in this repo as the main technology behind it.
  - EternityChronicles (EternityChronicles, EternityChronicles.Core, EternityChronicles.Glue) - A MUD built on the other technologies that will be based on open game reference rules.
  - IronDragon (IronDragon, IDragon) - IronDragon is a dynamic language for the DLR with extensive bidirectional support for C#.
  - MDK.Master - Live MUD editing toolkit, database bridge.
  - XDL - ExtensibleDataLoader XML based data loading.
