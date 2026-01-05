// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Polyfill for init-only properties in netstandard2.0/2.1
// Required for 'readonly record struct' primary constructors
#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
internal static class IsExternalInit { }

#endif
