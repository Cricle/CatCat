using System.Diagnostics.CodeAnalysis;

// Third-party library warnings - these are external dependencies that may not be fully AOT compatible
[assembly: UnconditionalSuppressMessage("AOT", "IL2104", Justification = "Third-party library not fully trim-safe")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3053", Justification = "Third-party library not fully AOT-compatible")]
[assembly: UnconditionalSuppressMessage("Trimming", "IL3000", Justification = "Third-party library using Assembly.Location")]
