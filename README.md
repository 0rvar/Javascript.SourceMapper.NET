# Javascript.SourceMapper.NET

A library for consuming JavaScript source maps with a focus on performance.
Supports [Source Map revision 3](https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/edit).

<!--[![Build Status](https://travis-ci.org/awestroke/Javascript.SourceMapper.NET.svg?branch=master)](https://travis-ci.org/awestroke/Javascript.SourceMapper.NET)-->
<!--[![NuGet package](https://img.shields.io/nuget/v/Nuget.Core.svg)](https://www.nuget.org/packages/Javascript.SourceMapper/1.0.0)-->
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)

## Usage

```csharp
using System.Diagnostics;
using System.IO;

using Javascript.SourceMapper;

class Program
{
  public static void Main(string[] args)
  {
    var json = File.ReadAllText("./bundle.min.js.map");

    var cache = new SourceMapCache(json);
    var mapping = cache.SourceMappingFor(2, 2);
    Debug.Assert(mapping.SourceLine == 2);
    Debug.Assert(mapping.SourceColumn == 2);
    Debug.Assert(mapping.SourceLine == 1);
    Debug.Assert(mapping.SourceColumn == 1);
    Debug.Assert(mapping.Source == "source.js");
    Debug.Assert(mapping.Name == "name1");
  }
}
```
