﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    [DSNodeServices.RegisterForTrace]
    public class Topography : Element
    {
        #region internal properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        [SupressImportIntoVM]
        internal TopographySurface InternalTopographySurface
        {
            get;
            private set;
        }

        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalTopographySurface; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// The set of points from which this TopographySurface is constructed.
        /// </summary>
        public List<Point> Points
        {
            get
            {
                var pts = InternalTopographySurface.GetPoints();
                return pts.Select(x => x.ToPoint()).ToList();
            }
        }

        /// <summary>
        /// Get the underlying triangular Mesh from the Topography
        /// </summary>
        public Autodesk.DesignScript.Geometry.Mesh Mesh
        {
            get { return Geometry().OfType<Autodesk.DesignScript.Geometry.Mesh>().First(); }
        }

        #endregion

        #region private constructors

        private Topography(IList<XYZ> points)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldSurf =
                ElementBinder.GetElementFromTrace<TopographySurface>(Document);

            var document = DocumentManager.Instance.CurrentDBDocument;

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            var topo = TopographySurface.Create(document, points);
            InternalSetTopographySurface(topo);

            TransactionManager.Instance.TransactionTaskDone();

            if (oldSurf != null)
            {
                ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
            }
            else
            {
                ElementBinder.SetElementForTrace(this.InternalElement);  
            }

            // necessary so the points in the topography are valid
            DocumentManager.Regenerate();
        }

        [SupressImportIntoVM]
        private Topography(TopographySurface topoSurface)
        {
            InternalSetTopographySurface(topoSurface);
        }

        #endregion

        #region public constructors

        /// <summary>
        /// Create a topography surface from a list of points.
        /// </summary>
        /// <param name="points">The points which define the topography surface.</param>
        /// <returns>A topography surface through the specified points.</returns>
        public static Topography ByPoints(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            if (enumerable.Count() < 3)
            {
                throw new Exception("A minimum of three points is required to create a topography surface.");
            }

            return new Topography(enumerable.Select(x => x.ToXyz()).ToList());
        }

        #endregion

        #region private mutators

        [SupressImportIntoVM]
        private void InternalSetTopographySurface(TopographySurface topoSurface)
        {
            InternalTopographySurface = topoSurface;
            this.InternalElementId = InternalTopographySurface.Id;
            this.InternalUniqueId = InternalTopographySurface.UniqueId;
        }

        #endregion

        #region internal static constructors

        [SupressImportIntoVM]
        internal static Topography FromExisting(TopographySurface topoSurface, bool isRevitOwned)
        {
            return new Topography(topoSurface)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        public override string ToString()
        {
            return "Topography";
        }
    }
}
