[![Build Status](https://github.com/CXuesong/TriesSharp/actions/workflows/TriesSharp.yml/badge.svg?branch=main)](https://github.com/CXuesong/OpenCCSharp/actions/workflows/OpenCCSharp.yml) | [![NuGet version (CXuesong.TriesSharp)](https://img.shields.io/nuget/vpre/CXuesong.TriesSharp.svg?style=flat-square) ![NuGet version (CXuesong.TriesSharp)](https://img.shields.io/nuget/dt/CXuesong.TriesSharp.svg?style=flat-square)](https://www.nuget.org/packages/CXuesong.TriesSharp)

# TriesSharp

Yet another [trie](https://en.wikipedia.org/wiki/Trie) implementation on .NET. This library targets .NET 6.0 as minimum supported platform.

This package is now available on NuGet. To install the package, use one of the following commands:

```powershell
#  Package Management Console
Install-Package CXuesong.TriesSharp -Prerelease
#  .NET CLI
dotnet add package CXuesong.TriesSharp --prerelease
```

The goal of this package is to provide a set of `IDictionary`-compliant API so that you can start using the package easily. This package provides an interface that exposes `IDictionary` API, plus

* `EnumEntriesFromPrefix`: queries for all the matching keys starting with the given prefix.
* `MatchLongestPrefix`: retrieves the longest key that the specified query string starts with.

Please refer to FuGet package gallery for the complete set of API we are providing: [`IReadOnlyTrie<TValue>`](https://www.fuget.org/packages/CXuesong.TriesSharp/*/lib/net6.0/TriesSharp.dll/TriesSharp.Collections/IReadOnlyTrie%601).

To decouple the interface from `string`, the trie interface provides API that can consume `string`, `ReadOnlyMemory<char>`, or `ReadOnlySpan<char>` where possible.

For now we only provide 1 implementation of the interface: `Trie<TValue>`.

The goal of this package is to support faster text conversion queries from [`CXuesong/OpenCCSharp`](https://github.com/CXuesong/OpenCCSharp). Currently we are only providing tries with char as key element type and for now I'd like to restrict the scope of this package to it. However, if you have new API request or any interesting idea, please raise it up on [Issues](https://github.com/CXuesong/TriesSharp/issues) section and I may take them into consideration some time😊

## See also

* [FuGet Gallery](https://www.fuget.org/packages/CXuesong.TriesSharp): Inspect public API
