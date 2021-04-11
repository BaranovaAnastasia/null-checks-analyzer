using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NullAnalyzer.Test.CSharpCodeFixVerifier<
    NullAnalyzer.NullAnalyzerAnalyzer,
    NullAnalyzer.NullAnalyzerCodeFixProvider>;

namespace NullAnalyzer.Test
{
    [TestClass]
    public class NullAnalyzerUnitTest
    {

        [TestMethod]
        public async Task TestIsNoDiagnostics()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            if(obj is string)
            {
                Console.WriteLine(obj);
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestEqualsNoDiagnostics()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            if(obj == ""abc"")
            {
                Console.WriteLine(obj);
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestReferenceEqualsNoDiagnostics()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            string obj2 = ""abc"";

            if (Object.ReferenceEquals(obj, obj2))
            {
                Console.WriteLine(obj);
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestNotIsObjectNoDiagnostics()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            if (!(obj is string))
            {
                Console.WriteLine(obj);
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestConditionalExpressionNoDiagnostics()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            string obj2 = ""123"";

            string str1 = obj == ""123"" ? ""a"" : ""b"";
            string str2 = obj is string ? ""a"" : ""b"";
            string str3 = Object.ReferenceEquals(obj, obj2) ? ""a"" : ""b"";
            string str4 = Object.ReferenceEquals(obj2, obj) ? ""a"" : ""b"";
            string str5 = !(obj is string) ? ""a"" : ""b"";
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }




        [TestMethod]
        public async Task TestIsNull()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            [|if(obj is null)
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestEquals()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            [|if(obj == null)
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestReferenceEquals1()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            [|if (Object.ReferenceEquals(obj, null))
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestReferenceEquals2()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            
            [|if (Object.ReferenceEquals(null, obj))
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestNotIsObject1()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            
            [|if (!(obj is object))
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestNotIsObject2()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            
            [|if (!(obj is Object))
            {
                Console.WriteLine(obj);
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestCoalesceExpression()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            
            string obj2 = [|obj ?? ""abc""|];
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string obj2 = obj;
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestConditionalExpression()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = [|obj == null ? ""a"" : ""b""|];
            string str2 = [|obj is null ? ""a"" : ""b""|];
            string str3 = [|Object.ReferenceEquals(obj, null) ? ""a"" : ""b""|];
            string str4 = [|Object.ReferenceEquals(null, obj) ? ""a"" : ""b""|];
            string str5 = [|!(obj is object) ? ""a"" : ""b""|];
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = ""b"";
            string str2 = ""b"";
            string str3 = ""b"";
            string str4 = ""b"";
            string str5 = ""b"";
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }




        [TestMethod]
        public async Task TestCorrectFix1()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            [|if (obj is null)
            {
                Console.WriteLine(""abc"");
            }
            else
            {
                Console.WriteLine(""def"");
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            Console.WriteLine(""def"");
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestCorrectFix2()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            [|if (obj is null)
            {
                Console.WriteLine(""abc"");
            }
            else if (obj == ""1"")
            {
                Console.WriteLine(""def"");
            }
            else
            {
                Console.WriteLine(""ghi"");
            }|]
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;
            if (obj == ""1"")
            {
                Console.WriteLine(""def"");
            }
            else
            {
                Console.WriteLine(""ghi"");
            }
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestCorrectFix3()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = [|obj == null ? ""a"" : obj == ""abc"" ? ""q"" : ""w""|];
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = obj == ""abc"" ? ""q"" : ""w"";
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }

        [TestMethod]
        public async Task TestCorrectFix4()
        {
            var test = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = [|obj == null ? ""a"" : [|obj is null ? ""q"" : ""w""|]|];
        }
    }
}
";

            var fixTest = @"
using System;

namespace testCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string obj = null;

            string str1 = ""w"";
        }
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(test, fixTest);
        }
    }
}
