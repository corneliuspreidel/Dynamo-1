﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class ScopedNodeTest : DSEvaluationUnitTest
    {
        private string TestFolder
        {
            get
            {
                return Path.Combine(GetTestDirectory(), "core", "scopednode"); 
                
            }
        }

        // test class
        private sealed class TwoScopedInputs: ScopedNodeModel
        {
            public TwoScopedInputs()
            {
                InPortData.Add(new PortData("port1", "Port1 block"));
                InPortData.Add(new PortData("port2", "Port2 block"));
                OutPortData.Add(new PortData("result", "Result"));

                RegisterAllPorts();
            }
        }

        [Test]
        public void TestScopedNode1()
        {
            //
            // n1 -> n2 --> n4 --> s1 --> n6 --> n7 --> s2
            //            /      /                            
            //          n3      n5                           
            // 
            // For s1, n1, n2, n3, n4 in its scope (for input port 0)
            //         n5 in its scope (for input port 1)
            // For s2, s1, n6, n7 in its scope for input port 0
            DynamoModel model = Controller.DynamoModel;
            Func<string, CodeBlockNodeModel> createCbn = 
                s => new CodeBlockNodeModel(s, Guid.NewGuid(), model.CurrentWorkspace, 0, 0);

            var cbn1 = createCbn("n1;");

            var cbn2 = createCbn("n2;");
            cbn2.ConnectInput(0, 0, cbn1);

            var cbn3 = createCbn("n3;");

            var cbn4 = createCbn("n4=x+y;");
            cbn4.ConnectInput(0, 0, cbn2);
            cbn4.ConnectInput(1, 0, cbn3);

            var cbn5 = createCbn("n5;");

            var s1 = new TwoScopedInputs();
            s1.ConnectInput(0, 0, cbn4);
            s1.ConnectInput(1, 0, cbn5);

            var scopedNodes = s1.GetInScopeNodesForInport(0).ToList();
            Assert.AreEqual(4, scopedNodes.Count());
            Assert.IsTrue(scopedNodes.Contains(cbn1));
            Assert.IsTrue(scopedNodes.Contains(cbn2));
            Assert.IsTrue(scopedNodes.Contains(cbn3));
            Assert.IsTrue(scopedNodes.Contains(cbn4));

            scopedNodes = s1.GetInScopeNodesForInport(1).ToList();
            Assert.AreEqual(1, scopedNodes.Count());
            Assert.IsTrue(scopedNodes.Contains(cbn5));

            var cbn6 = createCbn("n6;");
            cbn6.ConnectInput(0, 0, s1);

            var cbn7 = createCbn("n7;");
            cbn7.ConnectInput(0, 0, cbn6);

            var s2 = new TwoScopedInputs();
            s2.ConnectInput(0, 0, cbn7);

            scopedNodes = s2.GetInScopeNodesForInport(0).ToList();
            Assert.AreEqual(8, scopedNodes.Count());
            Assert.IsTrue(scopedNodes.Contains(cbn1));
            Assert.IsTrue(scopedNodes.Contains(cbn2));
            Assert.IsTrue(scopedNodes.Contains(cbn3));
            Assert.IsTrue(scopedNodes.Contains(cbn4));
            Assert.IsTrue(scopedNodes.Contains(cbn5));
            Assert.IsTrue(scopedNodes.Contains(cbn6));
            Assert.IsTrue(scopedNodes.Contains(cbn7));
            Assert.IsTrue(scopedNodes.Contains(s1));
        }

        [Test]
        public void TestScopedNodeModel02()
        {
            //                n5
            //               /
            // n1 -> n2 --> n4 --> s1
            //            /                                  
            //          n3                                 
            // 
            // For s1, none is in its scope 
            DynamoModel model = Controller.DynamoModel;
            Func<string, CodeBlockNodeModel> createCbn =
                s => new CodeBlockNodeModel(s, Guid.NewGuid(), model.CurrentWorkspace, 0, 0);

            var cbn1 = createCbn("n1;");

            var cbn2 = createCbn("n2;");
            cbn2.ConnectInput(0, 0, cbn1);

            var cbn3 = createCbn("n3;");

            var cbn4 = createCbn("n4=x+y;");
            cbn4.ConnectInput(0, 0, cbn2);
            cbn4.ConnectInput(1, 0, cbn3);

            var cbn5 = createCbn("n5;");
            cbn5.ConnectInput(0, 0, cbn4);

            var s1 = new TwoScopedInputs();
            s1.ConnectInput(0, 0, cbn4);

            var scopedNodes = s1.GetInScopeNodesForInport(0);
            Assert.AreEqual(0, scopedNodes.Count());
        }

        [Test]
        public void TestScopedNodeModel03()
        {
            //         n5       
            //        /       
            // n1 -> n2 --> n4 --> s1
            //            /                                  
            //          n3                                 
            // 
            // For s1, n3, n4 are in its scope
            DynamoModel model = Controller.DynamoModel;
            Func<string, CodeBlockNodeModel> createCbn =
                s => new CodeBlockNodeModel(s, Guid.NewGuid(), model.CurrentWorkspace, 0, 0);

            var cbn1 = createCbn("n1;");

            var cbn2 = createCbn("n2;");
            cbn2.ConnectInput(0, 0, cbn1);

            var cbn3 = createCbn("n3;");

            var cbn4 = createCbn("n4=x+y;");
            cbn4.ConnectInput(0, 0, cbn2);
            cbn4.ConnectInput(1, 0, cbn3);

            var cbn5 = createCbn("n5;");
            cbn5.ConnectInput(0, 0, cbn2);

            var s1 = new TwoScopedInputs();
            s1.ConnectInput(0, 0, cbn4);

            var scopedNodes = s1.GetInScopeNodesForInport(0).ToList();
            Assert.AreEqual(2, scopedNodes.Count());
            Assert.IsTrue(scopedNodes.Contains(cbn3));
            Assert.IsTrue(scopedNodes.Contains(cbn4));
        }
    }
}
