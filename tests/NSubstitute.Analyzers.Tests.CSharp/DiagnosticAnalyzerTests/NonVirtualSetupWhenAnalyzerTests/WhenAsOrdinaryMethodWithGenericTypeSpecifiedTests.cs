﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NSubstitute.Analyzers.Shared;
using NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers;
using Xunit;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.NonVirtualSetupWhenAnalyzerTests
{
    public class WhenAsOrdinaryMethodWithGenericTypeSpecifiedTests : NonVirtualSetupWhenDiagnosticVerifier
    {
        [Theory]
        [InlineData("sub => sub.Bar()", 19, 63)]
        [InlineData("delegate(Foo sub) { sub.Bar(); }", 19, 76)]
        [InlineData("sub => { sub.Bar(); }", 19, 65)]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMethod(string whenAction, int expectedLine, int expectedColumn)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public int Bar()
        {{
            return 2;
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(expectedLine, expectedColumn)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Theory]
        [InlineData("sub => sub.Bar()")]
        [InlineData("delegate(Foo sub) { sub.Bar(); }")]
        [InlineData("sub => { sub.Bar(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMethod(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual int Bar()
        {{
            return 2;
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub.Bar()")]
        [InlineData("delegate(Foo2 sub) { sub.Bar(); }")]
        [InlineData("sub => { sub.Bar(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForNonSealedOverrideMethod(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual int Bar()
        {{
            return 2;
        }}
    }}

    public class Foo2 : Foo
    {{
        public override int Bar() => 1;
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo2>();
            SubstituteExtensions.When<Foo2>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub()")]
        [InlineData("delegate(Func<int> sub) { sub(); }")]
        [InlineData("sub => { sub(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForDelegate(string whenAction)
        {
            var source = $@"using NSubstitute;
using System;

namespace MyNamespace
{{
    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = Substitute.For<Func<int>>();
            SubstituteExtensions.When<Func<int>>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub.Bar()", 24, 64)]
        [InlineData("delegate(Foo2 sub) { sub.Bar(); }", 24,  78)]
        [InlineData("sub => { sub.Bar(); }", 24,  66)]
        public override async Task ReportsDiagnostics_WhenSettingValueForSealedOverrideMethod(string whenAction, int expectedLine, int expectedColumn)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual int Bar()
        {{
            return 2;
        }}
    }}

    public class Foo2 : Foo
    {{
        public sealed override int Bar() => 1;
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo2>();
            SubstituteExtensions.When<Foo2>(substitute, {whenAction}).Do(callInfo => i++);

        }}
    }}
}}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(expectedLine, expectedColumn)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Theory]
        [InlineData("sub => sub.Bar()")]
        [InlineData("delegate(Foo sub) { sub.Bar(); }")]
        [InlineData("sub => { sub.Bar(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForAbstractMethod(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public abstract class Foo
    {{
        public abstract int Bar();
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";

            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub.Bar()")]
        [InlineData("delegate(IFoo sub) { sub.Bar(); }")]
        [InlineData("sub => { sub.Bar(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceMethod(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public interface IFoo
    {{
        int Bar();
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<IFoo>();
            SubstituteExtensions.When<IFoo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("delegate(IFoo sub) { var x = sub.Bar; }")]
        [InlineData("sub => { int x; x = sub.Bar; }")]
        [InlineData("sub => { var x = sub.Bar; }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceProperty(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public interface IFoo
    {{
        int Bar {{ get; }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<IFoo>();
            SubstituteExtensions.When<IFoo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub.Bar<int>()")]
        [InlineData("delegate(IFoo<int> sub) { sub.Bar<int>(); }")]
        [InlineData("sub => { sub.Bar<int>(); }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForGenericInterfaceMethod(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
   public interface IFoo<T>
    {{
        int Bar<T>();
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<IFoo<int>>();
            SubstituteExtensions.When<IFoo<int>>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => { var x = sub.Bar; }")]
        [InlineData("sub => { int x; x = sub.Bar; }")]
        [InlineData("delegate(Foo sub) { var x = sub.Bar; }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForAbstractProperty(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public abstract class Foo
    {{
        public abstract int Bar {{ get; }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";

            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("delegate(IFoo sub) { var x = sub[1]; }")]
        [InlineData("sub => { var x = sub[1]; }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForInterfaceIndexer(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public interface IFoo
    {{
        int this[int i] {{ get; }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<IFoo>();
            SubstituteExtensions.When<IFoo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => sub.Bar()")]
        [InlineData("delegate(Foo sub) { sub.Bar(); }")]
        [InlineData("sub => { sub.Bar(); }")]
        public override async Task ReportsNoDiagnostics_WhenUsingUnfortunatelyNamedMethod(string whenAction)
        {
            var source = $@"

namespace NSubstitute
{{
    public class Foo
    {{
        public int Bar()
        {{
            return 1;
        }}
    }}

    public static class SubstituteExtensions
    {{
        public static T When<T>(this T substitute, System.Action<T> substituteCall, int x)
        {{
            return default(T);
        }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            Foo substitute = null;
            SubstituteExtensions.When<Foo>(substitute, {whenAction}, 1);
        }}
    }}
}}";
            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => { var x = sub.Bar; }", 16, 73)]
        [InlineData("sub => { int x; x = sub.Bar; }", 16, 76)]
        [InlineData("delegate(Foo sub) { var x = sub.Bar; }", 16, 84)]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualProperty(string whenAction, int expectedLine, int expectedColumn)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public int Bar {{get; set; }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(expectedLine, expectedColumn)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Theory]
        [InlineData("sub => { var x = sub.Bar; }")]
        [InlineData("sub => { int x; x = sub.Bar; }")]
        [InlineData("delegate(Foo sub) { var x = sub.Bar; }")]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualProperty(string whenAction)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public virtual int Bar {{get; set; }}
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";

            await VerifyDiagnostic(source);
        }

        [Theory]
        [InlineData("sub => { var x = sub[1]; }", 16, 73)]
        [InlineData("delegate(Foo sub) { var x = sub[1]; }", 16, 84)]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualIndexer(string whenAction, int expectedLine, int expectedColumn)
        {
            var source = $@"using NSubstitute;

namespace MyNamespace
{{
    public class Foo
    {{
        public int this[int x] => 1;
    }}

    public class FooTests
    {{
        public void Test()
        {{
            int i = 1;
            var substitute = NSubstitute.Substitute.For<Foo>();
            SubstituteExtensions.When<Foo>(substitute, {whenAction}).Do(callInfo => i++);
        }}
    }}
}}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member this[] can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(expectedLine, expectedColumn)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMember_InLocalFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            void SubstituteCall(Foo sub)
            {
                sub.Bar();
            }

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }
    }
}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(22, 17)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMember_InExpressionBodiedLocalFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            void SubstituteCall(Foo sub) => sub.Bar();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }
    }
}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(20, 45)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMember_InRegularFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }

        private void SubstituteCall(Foo sub)
        {
            var objBarr = sub.Bar();
        }
    }
}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(26, 27)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportsDiagnostics_WhenSettingValueForNonVirtualMember_InRegularExpressionBodiedFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }

        private void SubstituteCall(Foo sub) => sub.Bar();
    }
}";
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.NonVirtualWhenSetupSpecification,
                Severity = DiagnosticSeverity.Warning,
                Message = "Member Bar can not be intercepted. Only interface members and virtual, overriding, and abstract members can be intercepted.",
                Locations = new[]
                {
                    new DiagnosticResultLocation(24, 49)
                }
            };

            await VerifyDiagnostic(source, expectedDiagnostic);
        }

        [Fact]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMember_InLocalFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public virtual int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            void SubstituteCall(Foo sub)
            {
                sub.Bar();
            }

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }
    }
}";

            await VerifyDiagnostic(source);
        }

        [Fact]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMember_InExpressionBodiedLocalFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public virtual int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            void SubstituteCall(Foo sub) => sub.Bar();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }
    }
}";
            await VerifyDiagnostic(source);
        }

        [Fact]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMember_InRegularFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public virtual int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }

        private void SubstituteCall(Foo sub)
        {
            var objBarr = sub.Bar();
        }
    }
}";
            await VerifyDiagnostic(source);
        }

        [Fact]
        public override async Task ReportsNoDiagnostics_WhenSettingValueForVirtualMember_InRegularExpressionBodiedFunction()
        {
            var source = @"using NSubstitute;

namespace MyNamespace
{
    public class Foo
    {
        public virtual int Bar()
        {
            return 2;
        }
    }

    public class FooTests
    {
        public void Test()
        {
            int i = 0;
            var substitute = NSubstitute.Substitute.For<Foo>();

            SubstituteExtensions.When<Foo>(substitute, SubstituteCall).Do(callInfo => i++);
            substitute.Bar();
        }

        private void SubstituteCall(Foo sub) => sub.Bar();
    }
}";
            await VerifyDiagnostic(source);
        }
    }
}