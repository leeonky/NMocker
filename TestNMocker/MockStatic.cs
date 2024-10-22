using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestNMocker
{
    public abstract class LineVerification
    {
        public abstract string ToString(int verifyLineOffset, StackFrame stackFrame);
    }

    public class ExpectVerification : LineVerification
    {
        private string expect;
        private int line;

        public ExpectVerification(string expect, int line)
        {
            this.expect = expect;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{expect} from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + verifyLineOffset + line - 1}";
        }
    }

    public class InvocationVerification : LineVerification
    {
        private string method;
        private int line;

        public InvocationVerification(string method, int line)
        {
            this.method = method;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{method} called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + line}";
        }
    }

    public class HitInvocationVerification : InvocationVerification
    {
        private readonly string hit;
        private readonly int line;

        public HitInvocationVerification(string hit, int line, string method, int line2) : base(method, line2)
        {
            this.hit = hit;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{hit} from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + verifyLineOffset + line - 1} => " + base.ToString(verifyLineOffset, stackFrame);
        }
    }

    public class TestBase
    {
        public StackFrame stackFrame;
        public UnexpectedCallException exception;

        public void VerifyMessage(int verifyLineOffset, params LineVerification[] lines)
        {
            IEnumerable<string> calls = lines.Where(l => l is InvocationVerification).Select(line => line.ToString(verifyLineOffset, stackFrame)); ;
            int width = calls.Select(l =>
            {
                int index = l.IndexOf("=>");
                if (index < 0)
                    return 0;
                return index + 3;
            }).DefaultIfEmpty(0).Max();

            string invocations = string.Join(string.Empty, calls.Select(l =>
               {
                   if (l.Contains("=>"))
                   {
                       return l.Replace("=>", new String(' ', l.IndexOf("=>") + 3 - width) + "=>");
                   }
                   else
                   {
                       return new String(' ', width) + l;
                   }
               }).Select(l => "\n    " + l));

            Assert.AreEqual($@"Unsatisfied invocation:
{string.Join("\n", lines.Where(l => l is ExpectVerification).Select(line => "    " + line.ToString(verifyLineOffset, stackFrame)))}
All invocations:{invocations}", exception.Message);
        }

        public LineVerification Expected(string expect, int line)
        {
            return new ExpectVerification(expect, line);
        }

        public LineVerification Invocation(string hit, int line, string method, int line2)
        {
            return new HitInvocationVerification(hit, line, method, line2);
        }

        public LineVerification Invocation(string method, int line)
        {
            return new InvocationVerification(method, line);
        }

        public void ExecuteFailed(Action action)
        {
            exception = Assert.ThrowsException<UnexpectedCallException>(action);
        }
    }

    [TestClass]
    public class TestVerifyTimesAndHit : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.When(() => Target.method(Arg.Any<string>())).ThenDefault();
        }

        public class Target
        {
            public static int method(string s)
            {
                return 0;
            }
        }

        [TestMethod]
        public void no_call_and_any_expectation()
        {
            stackFrame = new StackTrace(true).GetFrame(0);

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Called(() => Target.method("a")).Verify();
            });

            VerifyMessage(4,
                Expected("Expected to call 1 times, but actually call 0 times", 1));

            Verifier.Times(0).Called(() => Target.method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(0).Called(() => Target.method("a")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            Verifier.Times(1).Called(() => Target.method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Called(() => Target.method("b")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Invocation("static Target::method(String<a>)", 1));

            Verifier.Times(0).Called(() => Target.method("b")).Verify();
        }

        [TestMethod]
        public void excess_group_and_hit_group_a_hit_b_and_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Called(() => Target.method("b"))
                .Times(0).Called(() => Target.method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 2, "static Target::method(String<a>)", 1));

            Verifier
                .Times(0).Called(() => Target.method("b"))
                .Times(1).Called(() => Target.method("a"))
                .Verify();
        }

        [TestMethod]
        public void hit_group_and_excess_group_a_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(0).Called(() => Target.method("a"))
                .Times(1).Called(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            Verifier
                .Times(1).Called(() => Target.method("a"))
                .Times(0).Called(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void miss_group_and_excess_group_a_hit_b_and_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Called(() => Target.method("b"))
                .Times(1).Called(() => Target.method("c"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("static Target::method(String<a>)", 1));

            Verifier
                .Times(0).Called(() => Target.method("b"))
                .Times(0).Called(() => Target.method("c"))
                .Verify();
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Called(() => Target.method("a"))
                .Called(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                .Called(() => Target.method("a"))
                .Called(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(18,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_x_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Called(() => Target.method("x"))
                .Called(() => Target.method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("static Target::method(String<a>)", 1));

            Verifier.Times(0)
                .Called(() => Target.method("x"))
                .Called(() => Target.method("a"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2));

            Verifier.Times(1)
                .Called(() => Target.method("a"))
                .Called(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(20,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2));
        }

        [TestMethod]
        public void single_group_balanced_a_c_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("c");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<c>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 3));

            Verifier.Times(1)
                .Called(() => Target.method("a"))
                .Called(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Called(() => Target.method("a"))
                    .Times(0).Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2));

            Verifier
                .Times(1).Called(() => Target.method("a"))
                .Times(1).Called(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_c_d_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Times(0)
                    .Called(() => Target.method("c"))
                    .Called(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(10,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Expected("Expected to call 0 times, but actually call 1 times", 5),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 3),
                Invocation("hit(1)", 5, "static Target::method(String<d>)", 4));

            Verifier
                .Times(1)
                .Called(() => Target.method("a"))
                .Called(() => Target.method("b"))
                .Times(1)
                .Called(() => Target.method("c"))
                .Called(() => Target.method("d"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_verification_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("c");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Times(0)
                    .Called(() => Target.method("c"))
                    .Called(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 2));

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(1)
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Times(1)
                    .Called(() => Target.method("c"))
                    .Called(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(26,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Expected("Expected to call 1 times, but actually call 0 times", 5),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 2));
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_call_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Called(() => Target.method("a"))
                    .Times(0).Called(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<c>)", 3),
                Invocation("static Target::method(String<d>)", 4));

            Verifier
                .Times(1).Called(() => Target.method("a"))
                .Times(1).Called(() => Target.method("c"))
                .Verify();
        }

        [TestMethod]
        public void hit_more_than_once_a_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Called(() => Target.method("a")).Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::method(String<a>)", 2));

            Verifier.Times(2).Called(() => Target.method("a")).Verify();
        }
    }

    [TestClass]
    public class MoreVerificationApi : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.Mock(() => Target.method(Arg.Any<string>()));
        }

        public class Target
        {
            public static int method(string s)
            {
                return 0;
            }

            public static void methodVoid(string s)
            {
            }
        }

        [TestMethod]
        public void at_least()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");
            Target.method("b");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .AtLeast(3).Called(() => Target.method("a"))
                    .AtLeast(3).Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call at least 3 times, but actually call 2 times", 1),
                Expected("Expected to call at least 3 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "static Target::method(String<b>)", 4));

            Verifier
                .AtLeast(2).Called(() => Target.method("a"))
                .AtLeast(2).Called(() => Target.method("b"))
                .Verify();

            Verifier
                .AtLeast(1).Called(() => Target.method("a"))
                .AtLeast(1).Called(() => Target.method("b"))
                .Verify();

        }

        [TestMethod]
        public void once()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");
            Target.method("b");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => Target.method("a"))
                    .Once().Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "static Target::method(String<b>)", 4)
                );
        }

        [TestMethod]
        public void at_most()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");
            Target.method("b");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .AtMost(1).Called(() => Target.method("a"))
                    .AtMost(1).Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call at most 1 times, but actually call 2 times", 1),
                Expected("Expected to call at most 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "static Target::method(String<b>)", 4)
                );

            Verifier
                .AtMost(2).Called(() => Target.method("a"))
                .AtMost(2).Called(() => Target.method("b"))
                .Verify();

            Verifier
                .AtMost(3).Called(() => Target.method("a"))
                .AtMost(3).Called(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void default_is_at_least_call_in_single_group()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                    .Called(() => Target.method("a"))
                    .Called(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call at least 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            Verifier
               .Called(() => Target.method("a"))
                .Verify();

            Verifier
                .AtLeast(1).Called(() => Target.method("a"))
                .Verify();

        }

        [TestMethod]
        public void void_method()
        {
            Mocker.Mock(() => Target.methodVoid(Arg.Any<string>()));
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.methodVoid("a");
            Target.methodVoid("a");
            Target.methodVoid("b");
            Target.methodVoid("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => Target.methodVoid("a"))
                    .Once().Called(() => Target.methodVoid("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "static Target::methodVoid(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::methodVoid(String<a>)", 2),
                Invocation("hit(1)", 2, "static Target::methodVoid(String<b>)", 3),
                Invocation("hit(2)", 2, "static Target::methodVoid(String<b>)", 4)
                );
        }
    }

    [TestClass]
    public class MockProperty : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.When(() => Target.Method).ThenDefault();
        }

        public class Target
        {
            public static int Method { get { return 0; } }
        }

        [TestMethod]
        public void verify()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            int i = Target.Method;
            int j = Target.Method;

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => Target.Method)
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Invocation("hit(1)", 1, "static Target::get_Method()", 1),
                Invocation("hit(2)", 1, "static Target::get_Method()", 2)
                );

            Verifier
                .Times(2).Called(() => Target.Method)
                .Verify();
        }
    }
}

