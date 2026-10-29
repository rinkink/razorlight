using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Integration
{
    public class CSharp14SyntaxTests
    {
        private static RazorLightEngine CreateEngine() => new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(Root))
            .UseMemoryCachingProvider()
            .Build();

        [Fact]
        public async Task Template_Can_Use_Null_Conditional_Assignment()
        {
            var engine = CreateEngine();
            string template = "@{ var d = new System.Collections.Generic.Dictionary<string, int>(); d?[\"a\"] = 5; }@d[\"a\"]";

            string result = await engine.CompileRenderStringAsync("cs14-null-conditional-assignment", template, new { });

            Assert.Equal("5", result);
        }

        [Fact]
        public async Task Template_Can_Use_Nameof_Unbound_Generic()
        {
            var engine = CreateEngine();
            string template = "@nameof(System.Collections.Generic.List<>)";

            string result = await engine.CompileRenderStringAsync("cs14-nameof-unbound", template, new { });

            Assert.Equal("List", result);
        }

        [Fact]
        public async Task Template_Can_Use_Escape_Character_Literal()
        {
            var engine = CreateEngine();
            string template = "@{ var esc = \"\\e\"; }@((int)esc[0])";

            string result = await engine.CompileRenderStringAsync("cs13-escape-literal", template, new { });

            Assert.Equal("27", result);
        }
    }
}
