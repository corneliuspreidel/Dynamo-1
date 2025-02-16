using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    public class SSATransformTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void UpdateMember01()
        {
            String code =
@"class C{    x;    constructor C()    {        x = 1;    }}p = C.C();a = p.x;p.x = 10;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        public void UpdateMember02()
        {
            String code =
@"class complex{	mx : var;	my : var;	constructor complex(px : int, py : int)	{		mx = px; 		my = py; 	}	def scale : int(s : int)	{		mx = mx * s; 		my = my * s; 		return = 0;	}}p = complex.complex(8,16);i = p.mx;j = p.my;k1 = p.scale(2); k2 = p.scale(10); k3 = p.scale(10); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("i");
            Assert.IsTrue((Int64)o.Payload == 1600);
            o = mirror.GetValue("j");
            Assert.IsTrue((Int64)o.Payload == 3200);
        }

        [Test]
        public void ArrayAssignmentNoCycle1()
        {
            String code =
@"// Script must not cyclea={0,1,2};x={10,11,12};a[0] = x[0];x[1] = a[1];y = x[1]; // 1";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("y");
            Assert.IsTrue((Int64)o.Payload == 1);
        }

        [Test]
        public void ArrayAssignmentNoCycle2()
        {
            String code =
@"// Script must not cyclea={0,1,2};x={10,11,12};i = 1;a[0] = x[0];x[i] = a[i];y = x[i]; // 1";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("y");
            Assert.IsTrue((Int64)o.Payload == 1);
        }

        [Test]
        public void UpdateMemberArray1()
        {
            String code =
@"class C{    x;    constructor C()    {        x = {1,2,3};    }}p = C.C();a = p.x[2];p.x = {10,20,30};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 30);
        }


        [Test]
        public void UpdateMemberArray2()
        {
            String code =
@"class C{    x;    constructor C()    {        x = {{1,2,3},{10,20,30}};    }}i = 0;j = 1;p = C.C();g = C.C();a = p.x[i][j] + g.x[j][2];g.x = {{1},{100,200,300,400}}; ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 302);
        }

        [Test]
        public void UpdateMemberArray3()
        {
            String code =
@"class A{	x : var[];		constructor A()	{		x = { B.B(), B.B(), B.B() };	}}class B{	y : var[]..[];		constructor B()	{		y = { { 1, 2 }, { 3, 4 }, { 5, 6 }};			}}a = { A.A(), A.A(), A.A() };g = 2;b = a[0].x[1].y[g][0]; a[0].x[1].y[g][0] = 100;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("b");
            Assert.IsTrue((Int64)o.Payload == 100);
        }

        [Test]
        public void TestReplicationGuide01()
        {
            String code =
@"a = {1,2};b = {1,2};c = a<1> + b<2>;x = c[0];y = c[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuide02()
        {
            String code =
@"a = {1,2};b = {1,2};a = a<1> + b<2>;x = a[0];y = a[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuideOnProperty01()
        {
            String code =
@"class A{    x : var[];    constructor A()    {        x = {1,2};    }}a = A.A();b = A.A();c = a.x<1> + b.x<2>;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        public void TestReplicationGuideOnProperty02()
        {
            String code =
@"class C{    def f(a : int)    {        return = 10;    }}p = {C.C(), C.C()};x = p<1>.f({1,2}<2>);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { new object[] { 10, 10 }, new object[] { 10, 10 } });
        }

        [Test]
        public void TestReplicationGuideOnFunction01()
        {
            String code =
@"def f(){    return = { 1, 2 };}def g(){    return = { 3, 4 };}x = f()<1> + g()<2>;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { new object[] { 4, 5 }, new object[] { 5, 6 } });
        }

        [Test]
        public void TestArrayIndexingFromFunction01()
        {
            String code =
@"class A{    a:int;    constructor A(i:int)    {        a = i;    }}def foo(){    return = {A.A(1)};}x = foo()[0].a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition01()
        {
            String code =
@"global = 0;def f(x : int){    loc = [Imperative]    {        if (x > 1)        {            return = f(x - 1) + x;        }        return = x;    }     global = global + 1;    return = loc;}y = f(10);a = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition02()
        {
            String code =
@"global = 0;def g(){    global = global + 1;    return = 0;}def f(x : int){    loc = [Imperative]    {        if (x > 1)        {            return = f(x - 1) + x;        }        return = x;    }     t = g();    return = loc;}y = f(10);a = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition03()
        {
            String code =
@"global = 0;def g(){    global = global + 1;    return = 0;}def f(x : int){    loc = [Imperative]    {        if (x > 1)        {            return = f(x - 1) + x;        }        return = x;    }     t = g();    global = global + 1;    return = loc;}y = f(10);a = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 20);
        }

        [Test]
        public void TestEmptyArray()
        {
            String code =
@"class C{    constructor C(i : int, j :int[]..[])    {    }}a = 1;p = C.C(a, {{}});a = 10;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }
    }
}
