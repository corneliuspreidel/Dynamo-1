﻿using System;
using System.Reflection;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Revit;


using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Base class for all auto-generated Revit API nodes.
    /// </summary>
    public abstract class ApiMethodNode : RevitTransactionNodeWithOneOutput
    {
        protected Type BaseType;
        protected Type ReturnType;
        protected MethodBase MethodBase;
        protected ParameterInfo[] ParametersInfo;

        /////<summary>
        /////Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        /////</summary>
        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    //foreach (var e in Elements)
        //    //{
        //    //    this.DeleteElement(e);
        //    //}

        //    Value result = dynRevitUtils.InvokeAPIMethod(this, args, BaseType, ParametersInfo, MethodBase, ReturnType);

        //    return result;
        //}

    }

    /// <summary>
    /// Base class for wrapped properties. Does not create a transaction.
    /// </summary>
    public abstract class ApiPropertyNode : NodeModel
    {
        protected Type BaseType;
        protected Type ReturnType;
        protected PropertyInfo PropertyInfo;

        /////<summary>
        /////Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        /////</summary>
        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    return dynRevitUtils.GetAPIPropertyValue(args, base_type, pi, return_type);
        //}
    }

    /// <summary>
    /// Revit Document node. Returns the active Revit Document.
    /// </summary>
    [NodeName("Revit Document")]
    [NodeSearchTags("document", "active")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Gets the active Revit document.")]
    public class RevitDocument : RevitTransactionNodeWithOneOutput
    {
        public RevitDocument()
        {
            OutPortData.Add(new PortData("doc", "The active Revit doc."));
            RegisterAllPorts();
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    return Value.NewContainer(DocumentManager.Instance.CurrentUIDocument.Document);
        //}

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Document.Current", "Document.Current");
        }
    }
}
