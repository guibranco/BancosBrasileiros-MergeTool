using System.Runtime.CompilerServices;
using System.Text;

internal static class TestSetup
{
    [ModuleInitializer]
    public static void Initialize() =>
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}
