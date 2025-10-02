using System.Diagnostics.CodeAnalysis;

// Suppress AOT warnings in Program.cs - all endpoints use source-generated JSON serialization
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Using System.Text.Json source generation for AOT compatibility", Scope = "member", Target = "~M:Program.<Main>$(System.String[])")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Using System.Text.Json source generation for AOT compatibility", Scope = "member", Target = "~M:Program.<Main>$(System.String[])")]
