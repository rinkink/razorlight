using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Integration
{
	public class CSharp12SyntaxTests
	{
		private static RazorLightEngine CreateEngine() => new RazorLightEngineBuilder()
			.UseEmbeddedResourcesProject(typeof(Root))
			.UseMemoryCachingProvider()
			.Build();

		[Fact]
		public async Task Template_Can_Use_Collection_Expression()
		{
			var engine = CreateEngine();
			string template = "@{ int[] items = [1, 2, 3]; int[] none = []; }@items.Length-@none.Length";

			string result = await engine.CompileRenderStringAsync("cs12-collection", template, new { });

			Assert.Equal("3-0", result);
		}

		[Fact]
		public async Task Template_Can_Use_Raw_String_Literal()
		{
			var engine = CreateEngine();
			string template = "@{ var s = \"\"\"raw \"quoted\" text\"\"\"; }@s";

			string result = await engine.CompileRenderStringAsync("cs11-raw", template, new { });

			Assert.Equal("raw &quot;quoted&quot; text", result);
		}
	}
}
