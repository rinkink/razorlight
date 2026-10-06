using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Integration
{
	// Upstream issue #240: an include at the end of a page was reported to suppress the Layout and its sections.
	public class IncludeWithLayoutTests
	{
		private static RazorLightEngine CreateEngine() => new RazorLightEngineBuilder()
			.UseEmbeddedResourcesProject(typeof(Root))
			.UseMemoryCachingProvider()
			.AddDynamicTemplates(new Dictionary<string, string>
			{
				["layout.cshtml"] = "[L:@RenderSection(\"HEADER\")|@RenderBody()|@RenderSection(\"FOOTER\")]",
				["header.cshtml"] = "H",
				["footer.cshtml"] = "F",
				["extra.cshtml"] = "X",
			})
			.Build();

		private const string PageWithoutTrailingInclude = @"
@{
	Layout = ""layout.cshtml"";
}
@section HEADER {
	@{ await IncludeAsync(""header.cshtml""); }
}
@section FOOTER {
	@{ await IncludeAsync(""footer.cshtml""); }
}
<p>body</p>
";

		private const string PageWithTrailingInclude = PageWithoutTrailingInclude + @"
@{ await IncludeAsync(""extra.cshtml""); }
";

		private static string StripWhitespace(string s) => Regex.Replace(s, @"\s+", "");

		[Fact]
		public async Task Layout_And_Sections_Render_Without_Trailing_Include()
		{
			var engine = CreateEngine();

			string result = await engine.CompileRenderStringAsync("page-no-include", PageWithoutTrailingInclude, new { });

			Assert.Equal("[L:H|<p>body</p>|F]", StripWhitespace(result));
		}

		[Fact]
		public async Task Layout_And_Sections_Still_Render_With_Trailing_Include()
		{
			var engine = CreateEngine();

			string result = await engine.CompileRenderStringAsync("page-with-include", PageWithTrailingInclude, new { });

			Assert.Equal("[L:H|<p>body</p>X|F]", StripWhitespace(result));
		}
	}
}
