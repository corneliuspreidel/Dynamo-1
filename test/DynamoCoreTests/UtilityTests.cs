﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DynNodes = Dynamo.Nodes;
using System.Xml;
using System.IO;

namespace Dynamo.Tests
{
    internal class UtilityTests : DynamoUnitTest
    {
        [Test]
        public void PreprocessTypeName00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = null;
                DynNodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        public void PreprocessTypeName01()
        {
            // Empty fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = string.Empty;
                DynNodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        public void PreprocessTypeName02()
        {
            // "Dynamo.Elements." prefix should be replaced.
            string fqn = "Dynamo.Elements.MyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName03()
        {
            // "Dynamo.Nodes." prefix should never be replaced.
            string fqn = "Dynamo.Nodes.MyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName04()
        {
            // System type names should never be modified.
            string fqn = "System.Environment";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("System.Environment", result);
        }

        [Test]
        public void PreprocessTypeName05()
        {
            // "Dynamo.Elements.dyn" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName06()
        {
            // "Dynamo.Nodes.dyn" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName07()
        {
            // "Dynamo.Elements.dynXYZ" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyXYZClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyXyzClass", result);
        }

        [Test]
        public void PreprocessTypeName08()
        {
            // "Dynamo.Nodes.dynUV" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyUVClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyUvClass", result);
        }

        [Test]
        public void ResolveType00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string fqn = null;
                DynNodes.Utilities.ResolveType(fqn);
            });
        }

        [Test]
        public void ResolveType01()
        {
            // Empty fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string fqn = string.Empty;
                DynNodes.Utilities.ResolveType(fqn);
            });
        }

        [Test]
        public void ResolveType02()
        {
            // Unknown type returns a 'null'.
            string fqn = "Dynamo.Connectors.ConnectorModel";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreEqual(null, type);
        }

        [Test]
        public void ResolveType03()
        {
            // Known internal type.
            string fqn = "Dynamo.Nodes.Addition";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("Dynamo.Nodes.Addition", type.FullName);
        }

        [Test]
        public void ResolveType04()
        {
            // System type names should be discoverable.
            string fqn = "System.Environment";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("System.Environment", type.FullName);
        }

        [Test]
        public void ResolveType05()
        {
            // 'NumberRange' class makes use of this attribute.
            string fqn = "Dynamo.Nodes.dynBuildSeq";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("Dynamo.Nodes.NumberRange", type.FullName);
        }

        [Test]
        public void SetDocumentXmlPath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                DynNodes.Utilities.SetDocumentXmlPath(null, null);
            });
        }

        [Test]
        public void SetDocumentXmlPath01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without any root element.
                XmlDocument document = new XmlDocument();
                DynNodes.Utilities.SetDocumentXmlPath(document, null);
            });
        }

        [Test]
        public void SetDocumentXmlPath02()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            DynNodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = DynNodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.
        }

        [Test]
        public void SetDocumentXmlPath03()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            DynNodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = DynNodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.

            // Test target file path removal through an empty string.
            DynNodes.Utilities.SetDocumentXmlPath(document, string.Empty);

            Assert.Throws<InvalidOperationException>(() =>
            {
                DynNodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        public void SetDocumentXmlPath04()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            DynNodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = DynNodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.

            // Test target file path removal through a null value.
            DynNodes.Utilities.SetDocumentXmlPath(document, null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                DynNodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        public void GetDocumentXmlPath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                DynNodes.Utilities.GetDocumentXmlPath(null);
            });
        }

        [Test]
        public void GetDocumentXmlPath01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without any root element.
                XmlDocument document = new XmlDocument();
                DynNodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        public void GetDocumentXmlPath02()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Test XmlDocument root element without path.
                XmlDocument document = new XmlDocument();
                document.AppendChild(document.CreateElement("RootElement"));
                DynNodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        public void SaveTraceDataToXmlDocument00()
        {
            XmlDocument document = new XmlDocument();
            var data = new Dictionary<Guid, List<string>>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test XmlDocument being null.
                DynNodes.Utilities.SaveTraceDataToXmlDocument(null, data);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test valid XmlDocument without document element.
                DynNodes.Utilities.SaveTraceDataToXmlDocument(document, data);
            });

            document.AppendChild(document.CreateElement("RootElement"));

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test Dictionary being null.
                DynNodes.Utilities.SaveTraceDataToXmlDocument(document, null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test valid Dictionary without any entry.
                DynNodes.Utilities.SaveTraceDataToXmlDocument(document, data);
            });
        }

        [Test]
        public void SaveTraceDataToXmlDocument01()
        {
            // Create a valid XmlDocument object.
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var nodeGuid0 = Guid.NewGuid();
            var nodeGuid1 = Guid.NewGuid();

            var nodeData0 = new List<string>()
            {
                "TraceData00", "TraceData01", "TraceData02"
            };

            var nodeData1 = new List<string>()
            {
                "TraceData10", "TraceData11", "TraceData12"
            };

            // Create sample data.
            var data = new Dictionary<Guid, List<string>>();
            data.Add(nodeGuid0, nodeData0);
            data.Add(nodeGuid1, nodeData1);

            IEnumerable<KeyValuePair<Guid, List<string>>> outputs = null;

            Assert.DoesNotThrow(() =>
            {
                DynNodes.Utilities.SaveTraceDataToXmlDocument(document, data);
                outputs = DynNodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });

            Assert.NotNull(outputs);
            Assert.AreEqual(2, outputs.Count());
            Assert.AreEqual(nodeGuid0, outputs.ElementAt(0).Key);
            Assert.AreEqual(nodeGuid1, outputs.ElementAt(1).Key);

            var outputData0 = outputs.ElementAt(0).Value;
            var outputData1 = outputs.ElementAt(1).Value;

            Assert.IsNotNull(outputData0);
            Assert.IsNotNull(outputData1);
            Assert.AreEqual(3, outputData0.Count);
            Assert.AreEqual(3, outputData1.Count);

            Assert.AreEqual("TraceData00", outputData0[0]);
            Assert.AreEqual("TraceData01", outputData0[1]);
            Assert.AreEqual("TraceData02", outputData0[2]);

            Assert.AreEqual("TraceData10", outputData1[0]);
            Assert.AreEqual("TraceData11", outputData1[1]);
            Assert.AreEqual("TraceData12", outputData1[2]);
        }

        [Test]
        public void LoadTraceDataFromXmlDocument00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                DynNodes.Utilities.LoadTraceDataFromXmlDocument(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without a document element.
                XmlDocument document = new XmlDocument();
                DynNodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });
        }

        [Test]
        public void LoadTraceDataFromXmlDocument01()
        {
            IEnumerable<KeyValuePair<Guid, List<string>>> outputs = null;

            Assert.DoesNotThrow(() =>
            {
                XmlDocument document = new XmlDocument();
                document.AppendChild(document.CreateElement("RootElement"));
                outputs = DynNodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });

            Assert.IsNotNull(outputs);
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        public void MakeRelativePath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid base path.
                DynNodes.Utilities.MakeRelativePath(null, Path.GetTempPath());
            });
        }

        [Test]
        public void MakeRelativePath01()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without an empty base path.
                DynNodes.Utilities.MakeRelativePath("", Path.GetTempPath());
            });
        }

        [Test]
        public void MakeRelativePath02()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = DynNodes.Utilities.MakeRelativePath(basePath, null);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void MakeRelativePath03()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = DynNodes.Utilities.MakeRelativePath(basePath, "");
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void MakeRelativePath04()
        {
            var justFileName = "JustSingleFileName.dll";
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = DynNodes.Utilities.MakeRelativePath(basePath, justFileName);
            Assert.AreEqual(justFileName, result);
        }

        [Test]
        public void MakeRelativePath05()
        {
            var tempPath = Path.GetTempPath();
            var basePath = Path.Combine(tempPath, "home.dyn");

            var filePath = Path.Combine(new string[]
            {
                tempPath, "This", "Is", "Sub", "Directory", "MyLibrary.dll"
            });

            var result = DynNodes.Utilities.MakeRelativePath(basePath, filePath);
            Assert.AreEqual(@"This\Is\Sub\Directory\MyLibrary.dll", result);
        }

        [Test]
        public void MakeAbsolutePath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                DynNodes.Utilities.MakeAbsolutePath(null, "Dummy");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                DynNodes.Utilities.MakeAbsolutePath(string.Empty, "Dummy");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                DynNodes.Utilities.MakeAbsolutePath("Dummy", null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                DynNodes.Utilities.MakeAbsolutePath("Dummy", string.Empty);
            });
        }

        [Test]
        public void MakeAbsolutePath01()
        {
            var validPath = Path.Combine(Path.GetTempPath(), "TempFile.dyn");

            Assert.Throws<UriFormatException>(() =>
            {
                DynNodes.Utilities.MakeAbsolutePath("Test", validPath);
            });

            Assert.DoesNotThrow(() =>
            {
                // "Test" is a completely valid relative path string.
                DynNodes.Utilities.MakeAbsolutePath(validPath, "Test");
            });
        }

        [Test]
        public void MakeAbsolutePath02()
        {
            var basePath = Path.GetTempPath();
            var relativePath = @"This\Is\Sub\Directory\MyLibrary.dll";
            var result = DynNodes.Utilities.MakeAbsolutePath(basePath, relativePath);

            var expected = Path.Combine(new string[]
            {
                basePath, "This", "Is", "Sub", "Directory", "MyLibrary.dll"
            });

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MakeAbsolutePath03()
        {
            var basePath = Path.GetTempPath();
            var relativePath = @"MyLibrary.dll";

            // "result" should be the same as "relativePath" because it is 
            // just a file name without directory information, therefore it 
            // will not be modified to prefix with a directory.
            // 
            var result = DynNodes.Utilities.MakeAbsolutePath(basePath, relativePath);
            Assert.AreEqual(relativePath, result);
        }

        [Test]
        public void MakeAbsolutePath04()
        {
            var basePath = @"C:\This\Is\Sub\Directory\Home.dyn";
            var relativePath = @"..\..\Another\Sub\Directory\MyLibrary.dll";
            var result = DynNodes.Utilities.MakeAbsolutePath(basePath, relativePath);

            var expected = Path.Combine(new string[]
            {
                "C:\\", "This", "Is", "Another", "Sub", "Directory", "MyLibrary.dll"
            });

            Assert.AreEqual(expected, result);
        }
    }
}
