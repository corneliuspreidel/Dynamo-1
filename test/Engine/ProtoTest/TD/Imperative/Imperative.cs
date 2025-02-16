using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    public class Imperative
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\Imperative\\";
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        [Category("Imperative")]
        public void T002_ClassConstructorNestedScope_InlineCondition()
        {
            String code =
             @"class A{    a : int;    constructor A ( x )     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    a = x > 1 ? 1 : 0;                }            }        }    }}t1 = A.A(2).a;             ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1);
        }

        [Test]
        [Category("Imperative")]
        public void T003_ClassConstructorNestedScope_RangeExpr()
        {
            String code =
             @"class A{    a : int[];    constructor A ( x:int[])     {        [Imperative]        {            c = 0;            [Associative]            {                [Imperative]                {                    for (i in x )                    {                        a[c] = Count ( 0..i..2 ) ;                        c = c + 1;                    }                }            }        }    }}t1 = A.A(0..4).a;             ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("t1", new Object[] { 1, 1, 2, 2, 3 });
        }

        [Test]
        [Category("Imperative")]
        public void T004_ClassConstructorNestedScope_TypeConversion()
        {
            String code =
             @"class A{    a : int[];    constructor A ( x:int)     {        [Imperative]        {            c = 0;            [Associative]            {                [Imperative]                {                    a[0] = x;                    a[1] = ""dummy"";                                  }            }        }    }}t1 = A.A(0).a;             ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("t1", new Object[] { 0, null });
        }

        [Test]
        [Category("Imperative")]
        public void T005_ClassConstructorNestedScope_TypeConversion()
        {
            String code =
             @"class test{    t : double;    constructor test()    {        [Imperative]        {            t = true;        }    }}a = test.test().t;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Imperative")]
        public void T006_ClassConstructorNestedScope_FunctionCall()
        {
            String code =
             @"def foo (x){    return = x*x;}class B{    static def foo ()    {        return = 1;    }    def foo2 ()    {        return = 1;    }}class test{    t : int;    constructor test ()     {        [Imperative]        {            c = 0;            [Associative]            {                [Imperative]                {                    b1 = B.B();                    while (c <= 2 )                    {                        t = t + b1.foo2() + B.foo() + foo(c);                        c = c + 1;                    }                                 }            }        }    }}a = test.test().t;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 11);
        }

        [Test]
        [Category("Imperative")]
        public void T007_ClassConstructorNestedScope_ArrayCreation()
        {
            String code =
             @"def foo (x){    return = x*x;}class B{    static def foo ()    {        return = 1;    }    def foo2 ()    {        return = 1;    }}class test{    t1 = {};    t2 : int[];    t3 = {0,0,0};    t4 = 0..2;    constructor test ()     {        [Imperative]        {            c = 0;            [Associative]            {                [Imperative]                {                    b1 = B.B();                    while (c <= 2 )                    {                        t1[c] = b1.foo2() + B.foo() + foo(c);                        t2[c] = b1.foo2() + B.foo() + foo(c);                        t3[c] = b1.foo2() + B.foo() + foo(c);                        t4[c] = b1.foo2() + B.foo() + foo(c);                        c = c + 1;                    }                                 }            }        }    }}a1 = test.test().t1;a2 = test.test().t2;a3 = test.test().t3;a4 = test.test().t4;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x = new Object[] { 2, 3, 6 };
            thisTest.Verify("a1", x);
            thisTest.Verify("a2", x);
            thisTest.Verify("a3", x);
            thisTest.Verify("a4", x);
        }

        [Test]
        [Category("Imperative")]
        public void T008_ClassConstructorNestedScope_ArrayIndexing()
        {
            String code =
             @"def foo (x){    return = 0..x;}class B{    static def foo (x)    {        return = 0..x;    }    def foo2 (x)    {        return = 0..x;    }}class test{    t1 = {};    t2 : int[];        constructor test ()     {        [Imperative]        {            c = 0;            [Associative]            {                [Imperative]                {                    b1 = B.B();                    while (c <= 2 )                    {                        t1[c] = b1.foo2(c)[0] + Sum(B.foo(c)) + foo(c)[0];                        c = c + 1;                    }                     t2 = t1[0..1];                                }            }        }    }}a = test.test();a1 = a.t1;a2 = a.t2;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", new Object[] { 0, 1, 3 });
            thisTest.Verify("a2", new Object[] { 0, 1 });

        }

        [Test]
        [Category("Imperative")]
        public void T009_ClassConstructorNestedScope_LogicalOperators()
        {
            String code =
             @"def foo (){    return = false;}class B{    static def foo ()    {        return = true;    }   }class test{    c1 ;    c2 ;       constructor test (a1, b1)     {        [Imperative]        {            c = 0..3;            [Associative]            {                [Imperative]                {                    b1 = B.B();                    if( !a1 )                      {                        c1 = a1 && b1 && !B.foo() && !foo();                        c2 = !a1 || !b1;                                        }                                                     }            }        }    }}a = test.test(false,true);a1 = a.c1;a2 = a.c2;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", false);
            thisTest.Verify("a2", true);
        }

        [Test]
        [Category("Imperative")]
        public void T010_ClassConstructorNestedScope_RelationalOperators()
        {
            String code =
             @"def foo (){    return = 1;}class B{    static def foo ()    {        return = 0;    }   }class test{    c1 ;    constructor test (x)     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    if( x > 1 )                      {                        c1 = B.foo() > x ? B.foo() : foo();                                                             }                    else if ( x < 1 )                    {                        c1 = B.foo() != foo() ? foo() : B.foo();                                            }                      else                     {                        c1 = B.foo() == foo() ? foo() : B.foo();                                            }                            }            }        }    }}a1 = test.test(1).c1;a2 = test.test(0).c1;a3 = test.test(2).c1;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 0);
            thisTest.Verify("a2", 1);
            thisTest.Verify("a3", 1);
        }

        [Test]
        [Category("Imperative")]
        [Category("Failing")]
        public void T011_ClassConstructorNestedScope_MathematicalOperators()
        {
            String code =
             @"def foo (){    return = 1;}class B{    static def foo ()    {        return = 0;    }   }class test{    c1 ;    constructor test ()     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    c1 = B.foo() + foo() / 5 * 5 %2;                 }            }        }    }}a1 = test.test().c1;";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4082
            string errmsg = "MAGN-4082: Using the 'mod' operator on double value yields null in imperative scope and an unexpected warning message in associative scope";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1.0);
        }

        [Test]
        [Category("Imperative")]
        public void T012_ClassConstructorNestedScope_ImplicitTypeConversion()
        {
            String code =
             @"def foo (x:int, y:int){    return = x+y;}def foo2(x : double){    return = x;}class test{    c1 ;    constructor test ()     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    c1 = foo(foo2(1.0),2);                }            }        }    }}a1 = test.test().c1;a2 = foo(foo2(1.0),2);";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", 3);
        }

        [Test]
        [Category("Imperative")]
        public void T013_ClassConstructorNestedScope_UseThisKeyWord()
        {
            String code =
             @"def foo (x:int, y:int){    return = x+y;}class B{    static def foo ( )    {        return = 1;    }}class test{    c1 ;    c2;    constructor test ()     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    c1 = foo(B.foo(),2);                    c2 = this.c1 + foo(B.foo(),this.c1);                }            }        }    }}a = test.test();a1 = a.c1;a2 = a.c2;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", 7);
        }

        [Test]
        [Category("Imperative")]
        public void T014_ClassConstructorNestedScope_CompareClassesUsingThis()
        {
            String code =
             @"class test{    c1 = true;       constructor test2 ( x)    {        c1 = x;    }    constructor test (x : test)     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    if(Equals(this,x ))                    {                        c1 = true;                    }                    else                    {                        c1 = false;                    }                }            }        }    }}a = test.test2(false);b = test.test(a);c = b.c1;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", false);
        }

        [Test]
        [Category("Imperative")]
        public void T015_ClassConstructorNestedScope_GlobalVariableInCode()
        {
            String code =
             @"y = 1;class test{    c1;       constructor test ()     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    if(y == 1)                     {                        c1 = 1;                    }                    else                    {                        c1 = 0;                    }                }            }        }    }}a = test.test();b = a.c1;y = 2;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("Imperative")]
        public void T016_ClassConstructorNestedScope_GlobalVariableInArgument()
        {
            String code =
             @"y = 1;class test{    c1;       constructor test (x)     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    if(x == 1)                     {                        c1 = 1;                    }                    else                    {                        c1 = 0;                    }                }            }        }    }}a = test.test(y);b = a.c1;y = 2;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("Imperative")]
        public void T017_ClassConstructorNestedScope_UpdateInSameScope()
        {
            String code =
             @"class test{    c1;       constructor test (y)     {        [Imperative]        {            [Associative]            {                [Imperative]                {                    x = y + 1;                    if(x == 1)                     {                        c1 = 1;                    }                    else                    {                        c1 = 0;                    }                    x = 1;                }            }        }    }}a = test.test(1);b = a.c1;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("Imperative")]
        [Category("Failing")]
        public void T018_ClassConstructorNestedScope_UpdateInInnerScope()
        {
            String code =
             @"class test{    x1;       x2;    constructor test (y)     {        x = y + 1;        [Imperative]        {            x1 = x + 1;            [Associative]            {                x2 = x + 1;                [Imperative]                {                    if(x > 1)                     {                        x = x - 1;                    }                    else                    {                        x = x + 1;                    }                                    }            }        }    }}a = test.test(1);b = a.x1;c = a.x2;";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1527
            string errmsg = "MAGN-1527: Cross Language Update Issue : Inner Associative block should trigger update of outer associative block variable";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("Imperative")]
        public void T018_ClassConstructorNestedScope_UpdateInOuterScope()
        {
            String code =
             @"class test{    x1;       constructor test (y)     {        x = y + 1;        [Imperative]        {            [Associative]            {                [Imperative]                {                    if(x > 1)                     {                        x1 = x - 1;                    }                    else                    {                        x1 = x + 1;                    }                                    }                x = x - 2;            }            x = x + 1;        }    }}a = test.test(1);b = a.x1;";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);

        }
    }
}
