﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Tests;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class IfTest : DSEvaluationUnitTest
    {
        string testFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "conditional"); } }

        [Test]
        public void TestIFBasic()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(testFolder, "testIfBasic.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("7d6e8c70-3abf-4fc4-864e-948f548e7ba2", 5.0);
            AssertPreviewValue("d5f5336d-3569-4a88-9a59-5538d6914037",
                               new object[] { 1, 1, 1, 1, 
                                              new object[] {-3, -2, -1, 0, 1, 2, 3},
                                              new object[] {-3, -2, -1, 0, 1, 2, 3},
                                              new object[] {-3, -2, -1, 0, 1, 2, 3}});
        }

        [Test]
        public void TestIfAsFunctionObject()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(testFolder, "testIFAsFunctionObject.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("82a84012-1c28-4fe1-a38e-4c751e5a2077", new object[] {43, 144, 144, 43});
            AssertPreviewValue("ab4e17e1-0065-441a-97f2-9210d968a9ee", new object[] {1, 2, 3, 4});
        }

        [Test]
        public void TestIfInCustomNode1()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(testFolder, "testIfInCustomNode1.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("bb5928fe-56ac-43e1-b3d4-96ea3ee4580f", new object[] { 1, 1, 1, 1, 1, 2, 3 });
        }


        [Test]
        public void TestIfInCustomNode2()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(testFolder, "testIFInCustomNode2.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("2569020c-9952-46c5-8962-02bbf8c9f027", 1);
            AssertPreviewValue("4cda88e3-b54e-46c1-9c80-2647de6e3468", 5);
        }


        [Test]
        public void TestScopeIfForFactorial()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(testFolder, "callFactorial.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("d70fab7e-7a2c-495e-a301-0b0797d86118", 720);
        }
    }
}
