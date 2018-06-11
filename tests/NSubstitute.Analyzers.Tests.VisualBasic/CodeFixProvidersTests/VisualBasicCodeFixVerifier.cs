﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using NSubstitute.Analyzers.Tests.Shared;

namespace NSubstitute.Analyzers.Tests.VisualBasic.CodeFixProvidersTests
{
    public abstract class VisualBasicCodeFixVerifier : CodeFixVerifier
    {
        protected override string Language { get; } = LanguageNames.VisualBasic;

        protected override CompilationOptions GetCompilationOptions()
        {
            return new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionStrict: OptionStrict.On);
        }
    }
}