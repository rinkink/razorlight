using System.Threading.Tasks;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
	public interface IRazorTemplateCompiler
	{
		ICompilationService CompilationService { get; }

		RazorLightProject Project { get; }

		Task<CompiledTemplateDescriptor> CompileAsync(string templateKey);
	}
}