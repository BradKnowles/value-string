// Copyright © 2016 Şafak Gür. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyTitle("Dawn.ValueString")]
[assembly: AssemblyProduct("Dawn Utils")]
[assembly: AssemblyCompany("https://github.com/safakgur/value-string")]
[assembly: AssemblyCopyright("MIT")]
[assembly: AssemblyDescription("A struct that holds arbitrary data as string.")]

[assembly: CLSCompliant(true)]
[assembly: SecurityTransparent]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
