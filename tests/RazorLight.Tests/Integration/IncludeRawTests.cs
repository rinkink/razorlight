using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RazorLight.Tests.Utils;
using Xunit;

namespace RazorLight.Tests.Integration
{
	public class IncludeRawTests
	{
		private const string ExpectedCss =
			"body { color: #333; }\n@media only screen and (max-width: 479px) { body { font-size: 14px; } }\n";

		private static string Normalise(string s) => s.Replace("\r\n", "\n");

		[Fact]
		public async Task Raw_Include_From_FileSystem_Writes_File_Verbatim()
		{
			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(DirectoryUtils.RootDirectory, "Assets", "Files"))
				.UseMemoryCachingProvider()
				.Build();

			string result = await engine.CompileRenderStringAsync("raw-fs", "<style>@{ await IncludeRawAsync(\"site.css\"); }</style>", new { });

			Assert.Equal("<style>" + ExpectedCss + "</style>", Normalise(result));
		}

		[Fact]
		public async Task Raw_Include_From_EmbeddedResources_Writes_File_Verbatim()
		{
			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root).Assembly, "RazorLight.Tests.Assets.Embedded")
				.UseMemoryCachingProvider()
				.Build();

			string result = await engine.CompileRenderStringAsync("raw-embedded", "@{ await IncludeRawAsync(\"site.css\"); }", new { });

			Assert.Equal(ExpectedCss, Normalise(result));
		}

		[Fact]
		public async Task Raw_Include_Prefers_Dynamic_Templates()
		{
			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root))
				.UseMemoryCachingProvider()
				.AddDynamicTemplates(new Dictionary<string, string> { ["snippet.txt"] = "<b>@not razor</b>" })
				.Build();

			string result = await engine.CompileRenderStringAsync("raw-dynamic", "@{ await IncludeRawAsync(\"snippet.txt\"); }", new { });

			Assert.Equal("<b>@not razor</b>", result);
		}

		[Fact]
		public async Task Raw_Include_Of_Missing_Key_Throws_TemplateNotFound()
		{
			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(DirectoryUtils.RootDirectory, "Assets", "Files"))
				.UseMemoryCachingProvider()
				.Build();

			await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
				engine.CompileRenderStringAsync("raw-missing", "@{ await IncludeRawAsync(\"nope.css\"); }", new { }));
		}
	}
}
