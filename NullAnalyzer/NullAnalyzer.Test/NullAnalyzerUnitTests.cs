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
    }
}
