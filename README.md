# RazorLight

> **This is a fork.** `Rinkink.RazorLight` is [toddams/RazorLight](https://github.com/toddams/RazorLight) taken at v2.3.1, retargeted to .NET 8 with the Razor compiler pinned to 6.0.36. Upstream has been inactive since 2023. The Precompile tool, samples and legacy .NET Framework support are removed. See [CHANGELOG.md](CHANGELOG.md) for details.
>
> Original work by [toddams](https://github.com/toddams), Apache-2.0, see [LICENSE](LICENSE).

---

Render Razor templates from strings, files or embedded resources outside of ASP.NET MVC.

[![Build](https://github.com/rinkink/razorlight/actions/workflows/ci.yml/badge.svg)](https://github.com/rinkink/razorlight/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Rinkink.RazorLight.svg)](https://www.nuget.org/packages/Rinkink.RazorLight/)
[![Downloads](https://img.shields.io/nuget/dt/Rinkink.RazorLight.svg)](https://www.nuget.org/packages/Rinkink.RazorLight/)

# Solidarity with Ukraine
> 🇺🇦 The original author, Ivan, lives in Ukraine. Please read
> [his message](https://github.com/toddams/RazorLight#solidarity-with-ukraine)
> and consider supporting [Come Back Alive](https://savelife.in.ua/en/donate-en/). Slava Ukraini!

# Table of contents
- [Quickstart](#quickstart)
- [Template sources](#template-sources)
  * [Files](#file-source)
  * [Embedded resources](#embeddedresource-source)
  * [Database (custom)](#custom-source)
- [Includes (aka Partial)](#includes-aka-partial-views)
- [Encoding](#encoding)
- [Additional metadata references](#additional-metadata-references)
- [Enable Intellisense support](#enable-intellisense-support)
- [FAQ](#faq)

# Quickstart
Install the NuGet package:

```shell
dotnet add package Rinkink.RazorLight
```

The simplest scenario is to create a template from string. Each template must have a `templateKey` that is associated with it, so you can render the same template next time without recompilation.

```csharp
var engine = new RazorLightEngineBuilder()
    // required to have a default RazorLightProject type,
    // but not required to create a template from string.
    .UseEmbeddedResourcesProject(typeof(ViewModel))
    .SetOperatingAssembly(typeof(ViewModel).Assembly)
    .UseMemoryCachingProvider()
    .Build();

string template = "Hello, @Model.Name. Welcome to RazorLight repository";
ViewModel model = new ViewModel {Name = "John Doe"};

string result = await engine.CompileRenderStringAsync("templateKey", template, model);
```

To render a compiled template:

```csharp
var cacheResult = engine.Handler.Cache.RetrieveTemplate("templateKey");
if(cacheResult.Success)
{
    var templatePage = cacheResult.Template.TemplatePageFactory();
    string result = await engine.RenderTemplateAsync(templatePage, model);
}
```

# Template sources

RazorLight can resolve templates from any source, but there are a built-in providers that resolve template source from filesystem and embedded resources.

## File source

When resolving a template from filesystem, templateKey - is a relative path to the root folder, that you pass to RazorLightEngineBuilder.

```csharp
var engine = new RazorLightEngineBuilder()
    .UseFileSystemProject("C:/RootFolder/With/YourTemplates")
    .UseMemoryCachingProvider()
    .Build();

var model = new {Name = "John Doe"};
string result = await engine.CompileRenderAsync("Subfolder/View.cshtml", model);
```

## EmbeddedResource source

For embedded resource, the key is the namespace of the project where the template exists combined with the template's file name.

The following examples are using this project structure:
```text
Project/
  Model.cs
  Program.cs
  Project.csproj
Project.Core/
  EmailTemplates/
    Body.cshtml
  Project.Core.csproj
  SomeService.cs
```

```csharp
var engine = new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(SomeService).Assembly)
    .UseMemoryCachingProvider()
    .Build();

var model = new Model();
string html = await engine.CompileRenderAsync("EmailTemplates.Body", model);
```

Setting the root namespace allows you to leave that piece off when providing the template name as the key:

```csharp
var engine = new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(SomeService).Assembly, "Project.Core.EmailTemplates")
    .UseMemoryCachingProvider()
    .Build();

var model = new Model();
string html = await engine.CompileRenderAsync("Body", model);
```

## Custom source

If you store your templates in database - it is recommended to create custom RazorLightProject that is responsible for getting templates source from it. The class will be used to get template source and ViewImports. RazorLight will use it to resolve Layouts, when you specify it inside the template.

```csharp
var project = new EntityFrameworkRazorProject(new AppDbContext());
var engine = new RazorLightEngineBuilder()
              .UseProject(project)
              .UseMemoryCachingProvider()
              .Build();

// For key as a GUID
string result = await engine.CompileRenderAsync("6cc277d5-253e-48e0-8a9a-8fe3cae17e5b", new { Name = "John Doe" });

// Or integer
int templateKey = 322;
string result = await engine.CompileRenderAsync(templateKey.ToString(), new { Name = "John Doe" });
```

You can find a full sample [in the upstream samples](https://github.com/toddams/RazorLight/tree/master/samples/RazorLight.Samples)

# Includes (aka Partial views)

Include feature is useful when you have reusable parts of your templates you want to share between different views. Includes are an effective way of breaking up large templates into smaller components. They can reduce duplication of template content and allow elements to be reused. *This feature requires you to use the RazorLight Project system, otherwise there is no way to locate the partial.*

```csharp
@model MyProject.TestViewModel
<div>
    Hello @Model.Title
</div>

@{ await IncludeAsync("SomeView.cshtml", Model); }
```
First argument takes a key of the template to resolve, second argument is a model of the view (can be null)

# Encoding
By the default RazorLight encodes Model values as HTML, but sometimes you want to output them as is. You can disable encoding for specific value using @Raw() function

```csharp
/* With encoding (default) */

string template = "Render @Model.Tag";
string result = await engine.CompileRenderAsync("templateKey", template, new { Tag = "<html>&" });

Console.WriteLine(result); // Output: &lt;html&gt;&amp

/* Without encoding */

string template = "Render @Raw(Model.Tag)";
string result = await engine.CompileRenderAsync("templateKey", template, new { Tag = "<html>&" });

Console.WriteLine(result); // Output: <html>&
```
In order to disable encoding for the entire document - just set `"DisableEncoding"` variable to true
```html
@model TestViewModel
@{
    DisableEncoding = true;
}

<html>
    Hello @Model.Tag
</html>
```

# Enable Intellisense support
Visual Studio tooling knows nothing about RazorLight and assumes, that the view you are using - is a typical ASP.NET MVC template. In order to enable Intellisense for RazorLight templates, you should give Visual Studio a little hint about the base template class, that all your templates inherit implicitly

```csharp
@using RazorLight
@inherits TemplatePage<MyModel>

<html>
    Your awesome template goes here, @Model.Name
</html>
```

# FAQ

## Coding Challenges (FAQ)

### How to use templates from memory without setting a project?

The short answer is, you have to set a project to use the memory caching provider.  The project doesn't have to do anything.  This is by design, as without a project system, RazorLight cannot locate partial views.

❌
You used to be able to write:

```csharp
var razorEngine = new RazorLightEngineBuilder()
.UseMemoryCachingProvider()
.Build();
```
... but this now throws an exception, saying, "`_razorLightProject cannot be null`".

✅
```csharp
var razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(AnyTypeInYourSolution)) // exception without this (or another project type)
                .UseMemoryCachingProvider()
                .Build();
```
Affects: RazorLight-2.0.0-beta1 and later.

Original Issue: https://github.com/toddams/RazorLight/issues/250

### How to embed an image in an email?

This isn't a RazorLight question, but please see [this StackOverflow answer](https://stackoverflow.com/a/32767496/1040437).

### How to embed css in an email?

This isn't a RazorLight question, but please look into PreMailer.Net.

## Compilation and Deployment Issues (FAQ)

Most problems with RazorLight deal with deploying it on a new machine, in a docker container, etc.  If it works fine in your development environment, read this list of problems to see if it matches yours.

### Additional metadata references
When RazorLight compiles your template - it loads all the assemblies from your entry assembly and creates MetadataReference from it. This is a default strategy and it works in 99% of the time. But sometimes compilation crashes with an exception message like "Can not find assembly My.Super.Assembly2000". In order to solve this problem you can pass additional metadata references to RazorLight.

```csharp
var metadataReference = MetadataReference.CreateFromFile("path-to-your-assembly");

var engine = new RazorLightEngineBuilder()
  .UseMemoryCachingProvider()
  .AddMetadataReferences(metadataReference)
  .Build();
```

### I'm getting "Cannot find compilation library" when I deploy this library on another server

Add these property groups to your **entry point csproj**.
It has to be the entry point project.  For example: ASP.NET Core web project, .NET Core Console project, etc.

```xml
  <PropertyGroup>
    <!-- This group contains project properties for RazorLight on .NET Core -->
    <PreserveCompilationContext>true</PreserveCompilationContext>
    <MvcRazorCompileOnPublish>false</MvcRazorCompileOnPublish>
    <MvcRazorExcludeRefAssembliesFromPublish>false</MvcRazorExcludeRefAssembliesFromPublish>
  </PropertyGroup>
```

### I'm getting "Can't load metadata reference from the entry assembly" exception

Set PreserveCompilationContext to true in your *.csproj file's PropertyGroup tag.

```xml
<PropertyGroup>
    ...
    <PreserveCompilationContext>true</PreserveCompilationContext>
</PropertyGroup>
```

Additionally, RazorLight allows you to specifically locate any `MetadataReference` you can't find, which could happen if you're running in SCD [(Self-Contained Deployment) mode](https://docs.microsoft.com/en-us/dotnet/core/deploying/), as the C# Compiler used by RazorLight [needs to be able to locate `mscorlib.dll`](https://github.com/toddams/RazorLight/issues/188#issuecomment-523418738).  This might be a useful trick if future versions of the .NET SDK tools ship with bad MSBuild targets that somehow don't "preserve compilation context" and you need an immediate fix while waiting for Microsoft support.

### I'm getting "Cannot find reference assembly 'Microsoft.AspNetCore.Antiforgery.dll'" exception

By default the SDK does not copy reference assemblies to the build output.
Set `PreserveCompilationReferences` and `PreserveCompilationContext` to true in your *.csproj file's PropertyGroup tag.

```xml
<PropertyGroup>
    <PreserveCompilationReferences>true</PreserveCompilationReferences>
    <PreserveCompilationContext>true</PreserveCompilationContext>
</PropertyGroup>
```

For more information, see https://github.com/aspnet/AspNetCore/issues/14418#issuecomment-535107767

## Unsupported Scenarios

### Serverless (AWS Lambda, Azure Functions)

Not tested by this fork. Upstream added an Azure Functions isolated-worker sample after 2.3.1,
see [toddams/RazorLight#306](https://github.com/toddams/RazorLight/issues/306) for history.

### RazorLight does not work with ASP.NET Core Integration Testing

RazorLight is not currently designed to support such integration tests.  If you need to test your RazorLight tests, current recommendation is to simply create a project called `<YourCompanyName>.<YourProjectName>.Templating` and write your template rendering layer as a Domain Service, and write tests against that service.  Then, you can mock in your integration tests any dependencies on RazorLight.

If you happen to get this working, please let us know what you did.
