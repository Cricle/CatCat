using System.Diagnostics.CodeAnalysis;

// Suppress OpenAPI warnings globally - OpenAPI uses reflection for documentation generation
// This is acceptable as OpenAPI is a development/runtime feature, not used in AOT compilation
[assembly: UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
    Justification = "OpenAPI metadata generation uses reflection at runtime, which is acceptable for dev/swagger",
    Scope = "namespaceanddescendants", Target = "~N:CatCat.API.Endpoints")]

[assembly: UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
    Justification = "OpenAPI metadata generation uses dynamic code at runtime, which is acceptable for dev/swagger",
    Scope = "namespaceanddescendants", Target = "~N:CatCat.API.Endpoints")]

[assembly: UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
    Justification = "Health check endpoint uses reflection for OpenAPI only",
    Scope = "member", Target = "~M:Program.<Main>$(System.String[])")]

[assembly: UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
    Justification = "Health check endpoint uses dynamic code for OpenAPI only",
    Scope = "member", Target = "~M:Program.<Main>$(System.String[])")]

// ✅ NATS 已使用 System.Text.Json 源生成，无需警告抑制

