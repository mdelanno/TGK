using System.Runtime.CompilerServices;
using VerifyTests;

namespace TGK.Tests;

public static class VerifyModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.UseUtf8NoBom();
    }
}