// *********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// *********************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("StreamInsight.Samples.Adapters.AsyncCsv")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("StreamInsight.Samples.Adapters.AsyncCsv")]
[assembly: AssemblyCopyright("Copyright © Microsoft Corporation 2010")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "StreamInsight.Samples.Adapters.OutputTracer")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Assembly should be signed used with StreamInsight service")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "CtiFrequency", Justification = "Cti is acceptable terminology")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:", Scope = "CtiFrequency", Justification = "Cti is acceptable terminology", Target = "CtiFrequency")]