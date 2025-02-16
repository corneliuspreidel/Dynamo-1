﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using Dynamo.Core;

namespace Dynamo.ViewModels
{
    public delegate void PointEventHandler(object sender, EventArgs e);
    public delegate void NodeEventHandler(object sender, EventArgs e);
    public delegate void NoteEventHandler(object sender, EventArgs e);
    public delegate void ViewEventHandler(object sender, EventArgs e);
    public delegate void ZoomEventHandler(object sender, EventArgs e);
    public delegate void SelectionEventHandler(object sender, SelectionBoxUpdateArgs e);
    public delegate void ViewModelAdditionEventHandler(object sender, ViewModelEventArgs e);
    public delegate void WorkspacePropertyEditHandler(WorkspaceModel workspace);

    public partial class WorkspaceViewModel : ViewModelBase
    {
        #region Properties and Fields

        public WorkspaceModel _model;
        private bool _canFindNodesFromElements = false;
        public Dispatcher Dispatcher;

        public event PointEventHandler CurrentOffsetChanged;
        public event ZoomEventHandler ZoomChanged;
        public event ZoomEventHandler RequestZoomToViewportCenter;
        public event ZoomEventHandler RequestZoomToViewportPoint;
        public event ZoomEventHandler RequestZoomToFitView;

        public event NodeEventHandler RequestCenterViewOnElement;
        public event NodeEventHandler RequestNodeCentered;
        public event ViewEventHandler RequestAddViewToOuterCanvas;
        public event SelectionEventHandler RequestSelectionBoxUpdate;
        public event WorkspacePropertyEditHandler WorkspacePropertyEditRequested;

        /// <summary>
        /// Cursor Property Binding for WorkspaceView
        /// </summary>
        private Cursor currentCursor = null;
        public Cursor CurrentCursor
        {
            get { return currentCursor; }
            set { currentCursor = value; RaisePropertyChanged("CurrentCursor"); }
        }

        /// <summary>
        /// Force Cursor Property Binding for WorkspaceView
        /// </summary>
        private bool isCursorForced = false;
        public bool IsCursorForced
        {
            get { return isCursorForced; }
            set { isCursorForced = value; RaisePropertyChanged("IsCursorForced"); }
        }

        /// <summary>
        /// Convenience property
        /// </summary>
        public DynamoViewModel DynamoViewModel { get { return dynSettings.Controller.DynamoViewModel; } }

        /// <summary>
        /// Used during open and workspace changes to set the location of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
            {
                Debug.WriteLine(string.Format("Setting current offset to {0}", e.Point));
                CurrentOffsetChanged(this, e);
            }
        }

        /// <summary>
        /// Used during open and workspace changes to set the zoom of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnZoomChanged(object sender, ZoomEventArgs e)
        {
            if (ZoomChanged != null)
            {
                //Debug.WriteLine(string.Format("Setting zoom to {0}", e.Zoom));
                ZoomChanged(this, e);
            }
        }

        /// <summary>
        /// For requesting registered workspace to zoom in center
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToViewportCenter(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportCenter != null)
            {
                RequestZoomToViewportCenter(this, e);
            }
        }

        /// <summary>
        /// For requesting registered workspace to zoom in out from a point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToViewportPoint(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportPoint != null)
            {
                RequestZoomToViewportPoint(this, e);
            }
        }

        /// <summary>
        /// For requesting registered workspace to zoom in or out to fitview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToFitView(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToFitView != null)
            {
                RequestZoomToFitView(this, e);
            }
        }

        public virtual void OnRequestCenterViewOnElement(object sender, ModelEventArgs e)
        {
            if (RequestCenterViewOnElement != null)
                RequestCenterViewOnElement(this, e);
        }

        public virtual void OnRequestNodeCentered(object sender, ModelEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }

        public virtual void OnRequestAddViewToOuterCanvas(object sender, ViewEventArgs e)
        {
            if (RequestAddViewToOuterCanvas != null)
                RequestAddViewToOuterCanvas(this, e);
        }

        public virtual void OnRequestSelectionBoxUpdate(object sender, SelectionBoxUpdateArgs e)
        {
            if (RequestSelectionBoxUpdate != null)
                RequestSelectionBoxUpdate(this, e);
        }

        public virtual void OnWorkspacePropertyEditRequested()
        {
            // extend this for all workspaces
            if (WorkspacePropertyEditRequested != null)
                WorkspacePropertyEditRequested(Model);
        }

        private CompositeCollection _workspaceElements = new CompositeCollection();
        public CompositeCollection WorkspaceElements { get { return _workspaceElements; } }

        ObservableCollection<ConnectorViewModel> _connectors = new ObservableCollection<ConnectorViewModel>();
        private ObservableCollection<Watch3DFullscreenViewModel> _watches = new ObservableCollection<Watch3DFullscreenViewModel>();
        ObservableCollection<NodeViewModel> _nodes = new ObservableCollection<NodeViewModel>();
        ObservableCollection<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();
        ObservableCollection<InfoBubbleViewModel> _errors = new ObservableCollection<InfoBubbleViewModel>();

        public ObservableCollection<ConnectorViewModel> Connectors
        {
            get { return _connectors; }
            set
            {
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<NodeViewModel> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<NoteViewModel> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }
        public ObservableCollection<InfoBubbleViewModel> Errors
        {
            get { return _errors; }
            set { _errors = value; RaisePropertyChanged("Errors"); }
        }

        public string Name
        {
            get
            {
                if (_model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return _model.Name;
            }
        }

        public string FileName
        {
            get { return _model.FileName; }
        }

        public bool CanEditName
        {
            get { return _model != dynSettings.Controller.DynamoViewModel.Model.HomeSpace; }
        }

        public bool IsCurrentSpace
        {
            get { return _model.IsCurrentSpace; }
        }

        public bool IsHomeSpace
        {
            get { return _model == dynSettings.Controller.DynamoModel.HomeSpace; }
        }

        public bool HasUnsavedChanges
        {
            get { return _model.HasUnsavedChanges; }
        }

        public WorkspaceModel Model
        {
            get { return _model; }
        }

        public ObservableCollection<Watch3DFullscreenViewModel> Watch3DViewModels
        {
            get { return _watches; }
            set
            {
                _watches = value;
                RaisePropertyChanged("Watch3DViewModels");
            }
        }

        public double Zoom
        {
            get { return _model.Zoom; }
        }

        public bool CanZoomIn
        {
            get { return CanZoom(Configurations.ZoomIncrement); }
        }

        public bool CanZoomOut
        {
            get { return CanZoom(-Configurations.ZoomIncrement); }
        }

        internal void ZoomInInternal()
        {
            var args = new ZoomEventArgs(Configurations.ZoomIncrement);
            OnRequestZoomToViewportCenter(this, args);
            ResetFitViewToggle(null);
        }

        internal void ZoomOutInternal()
        {
            var args = new ZoomEventArgs(-Configurations.ZoomIncrement);
            OnRequestZoomToViewportCenter(this, args);
            ResetFitViewToggle(null);
        }

        public bool CanFindNodesFromElements
        {
            get { return _canFindNodesFromElements; }
            set
            {
                _canFindNodesFromElements = value;
                RaisePropertyChanged("CanFindNodesFromElements");
            }
        }

        public bool CanShowInfoBubble
        {
            get { return stateMachine.IsInIdleState; }
        }

        public Action FindNodesFromElements { get; set; }

        #endregion

        public WorkspaceViewModel(WorkspaceModel model, DynamoViewModel vm)
        {
            _model = model;
            stateMachine = new StateMachine(this);

            var nodesColl = new CollectionContainer { Collection = Nodes };
            _workspaceElements.Add(nodesColl);

            var connColl = new CollectionContainer { Collection = Connectors };
            _workspaceElements.Add(connColl);

            var notesColl = new CollectionContainer { Collection = Notes };
            _workspaceElements.Add(notesColl);

            var errorsColl = new CollectionContainer { Collection = Errors };
            _workspaceElements.Add(errorsColl);

            // Add EndlessGrid
            var endlessGrid = new EndlessGridViewModel(this);
            _workspaceElements.Add(endlessGrid);

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _model.Nodes.CollectionChanged += Nodes_CollectionChanged;
            _model.Notes.CollectionChanged += Notes_CollectionChanged;
            _model.Connectors.CollectionChanged += Connectors_CollectionChanged;
            _model.PropertyChanged += ModelPropertyChanged;

            DynamoSelection.Instance.Selection.CollectionChanged += this.AlignSelectionCanExecuteChanged;

            // sync collections
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Nodes));
            Connectors_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Connectors));
            Notes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Notes));
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        void DynamoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShouldBeHitTestVisible")
            {
                RaisePropertyChanged("ShouldBeHitTestVisible");
            }
        }

        void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var viewModel = new ConnectorViewModel(item as ConnectorModel);
                        _connectors.Add(viewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _connectors.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _connectors.Remove(_connectors.First(x => x.ConnectorModel == item));
                    }
                    break;
            }
        }

        void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        //add a corresponding note
                        var viewModel = new NoteViewModel(item as NoteModel);
                        _notes.Add(viewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _notes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _notes.Remove(_notes.First(x => x.Model == item));
                    }
                    break;
            }
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        if (item != null && item is NodeModel)
                        {
                            var node = item as NodeModel;

                            var nodeViewModel = new NodeViewModel(node);
                            _nodes.Add(nodeViewModel);
                            Errors.Add(nodeViewModel.ErrorBubble);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _nodes.Clear();
                    Errors.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var node = item as NodeModel;
                        NodeViewModel nodeViewModel = _nodes.First(x => x.NodeLogic == item);
                        Errors.Remove(nodeViewModel.ErrorBubble);
                        _nodes.Remove(nodeViewModel);

                    }
                    break;
            }
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "X":
                    break;
                case "Y":
                    break;
                case "Zoom":
                    OnZoomChanged(this, new ZoomEventArgs(_model.Zoom));
                    RaisePropertyChanged("Zoom");
                    break;
                case "IsCurrentSpace":
                    RaisePropertyChanged("IsCurrentSpace");
                    RaisePropertyChanged("IsHomeSpace");
                    break;
                case "HasUnsavedChanges":
                    RaisePropertyChanged("HasUnsavedChanges");
                    break;
                case "FileName":
                    RaisePropertyChanged("FileName");
                    break;
            }
        }

        internal void SelectAll(object parameter)
        {
            DynamoSelection.Instance.ClearSelection();
            Nodes.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele.NodeModel));
        }

        internal bool CanSelectAll(object parameter)
        {
            return true;
        }

        internal void SelectInRegion(Rect region, bool isCrossSelect)
        {
            bool fullyEnclosed = !isCrossSelect;

            foreach (NodeModel n in Model.Nodes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                if (IsInRegion(region, n, fullyEnclosed))
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
                else
                {
                    if (n.IsSelected)
                        DynamoSelection.Instance.Selection.Remove(n);
                }
            }

            foreach (var n in Model.Notes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                if (IsInRegion(region, n, fullyEnclosed))
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
                else
                {
                    if (n.IsSelected)
                        DynamoSelection.Instance.Selection.Remove(n);
                }
            }
        }

        private static bool IsInRegion(Rect region, ILocatable locatable, bool fullyEnclosed)
        {
            double x0 = locatable.X;
            double y0 = locatable.Y;

            if (false == fullyEnclosed) // Cross selection.
            {
                var test = new Rect(x0, y0, locatable.Width, locatable.Height);
                return region.IntersectsWith(test);
            }

            double x1 = x0 + locatable.Width;
            double y1 = y0 + locatable.Height;
            return (region.Contains(x0, y0) && region.Contains(x1, y1));
        }

        public double GetSelectionAverageX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterX)
                           .Average();
        }

        public double GetSelectionAverageY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterY)
                           .Average();
        }

        public double GetSelectionMinX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Min();
        }

        public double GetSelectionMinY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Min();
        }

        public double GetSelectionMaxX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X + x.Width)
                           .Max();
        }

        public double GetSelectionMaxLeftX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Max();
        }

        public double GetSelectionMaxY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y + x.Height)
                           .Max();
        }

        public double GetSelectionMaxTopY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Max();
        }

        public void AlignSelected(object parameter)
        {
            string alignType = parameter.ToString();

            if (DynamoSelection.Instance.Selection.Count <= 1) return;

            // All the models in the selection will be modified, 
            // record their current states before anything gets changed.
            SmartCollection<ISelectable> selection = DynamoSelection.Instance.Selection;
            IEnumerable<ModelBase> models = selection.OfType<ModelBase>();
            _model.RecordModelsForModification(models.ToList());

            var toAlign = DynamoSelection.Instance.Selection.OfType<ILocatable>().ToList();

            switch (alignType)
            {
                case "HorizontalCenter":
                {
                    var xAll = GetSelectionAverageX();
                    toAlign.ForEach((x) => { x.CenterX = xAll; });
                }
                    break;
                case "HorizontalLeft":
                {
                    var xAll = GetSelectionMinX();
                    toAlign.ForEach((x) => { x.X = xAll; });
                }
                    break;
                case "HorizontalRight":
                {
                    var xAll = GetSelectionMaxX();
                    toAlign.ForEach((x) => { x.X = xAll - x.Width; });
                }
                    break;
                case "VerticalCenter":
                {
                    var yAll = GetSelectionAverageY();
                    toAlign.ForEach((x) => { x.CenterY = yAll; });
                }
                    break;
                case "VerticalTop":
                {
                    var yAll = GetSelectionMinY();
                    toAlign.ForEach((x) => { x.Y = yAll; });
                }
                    break;
                case "VerticalBottom":
                {
                    var yAll = GetSelectionMaxY();
                    toAlign.ForEach((x) => { x.Y = yAll - x.Height; });
                }
                    break;
                case "VerticalDistribute":
                {
                    if (DynamoSelection.Instance.Selection.Count <= 2) return;

                    var yMin = GetSelectionMinY();
                    var yMax = GetSelectionMaxY();

                    var spacing = 0.0;
                    var span = yMax - yMin;

                    var nodeHeightSum =
                        DynamoSelection.Instance.Selection.Where(y => y is ILocatable)
                            .Cast<ILocatable>()
                            .Sum((y) => y.Height);

                    if (span > nodeHeightSum)
                    {
                        spacing = (span - nodeHeightSum)
                            /(DynamoSelection.Instance.Selection.Count - 1);
                    }

                    var cursor = yMin;
                    foreach (var node in toAlign.OrderBy(y => y.Y))
                    {
                        node.Y = cursor;
                        cursor += node.Height + spacing;
                    }
                }
                    break;
                case "HorizontalDistribute":
                {
                    if (DynamoSelection.Instance.Selection.Count <= 2) return;

                    var xMin = GetSelectionMinX();
                    var xMax = GetSelectionMaxX();

                    var spacing = 0.0;
                    var span = xMax - xMin;
                    var nodeWidthSum =
                        DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                            .Cast<ILocatable>()
                            .Sum((x) => x.Width);

                    // If there is more span than total node width,
                    // distribute the nodes with a gap. If not, leave
                    // the spacing at 0 and the nodes will distribute
                    // up against each other.
                    if (span > nodeWidthSum)
                    {
                        spacing = (span - nodeWidthSum)
                            /(DynamoSelection.Instance.Selection.Count - 1);
                    }

                    var cursor = xMin;
                    foreach (var node in toAlign.OrderBy(x => x.X))
                    {
                        node.X = cursor;
                        cursor += node.Width + spacing;
                    }
                }
                    break;
            }

            toAlign.ForEach(x => x.ReportPosition());
        }

        private static bool CanAlignSelected(object parameter)
        {
            return DynamoSelection.Instance.Selection.Count > 1;
        }

        private void Hide(object parameters)
        {
            // Closing of custom workspaces will simply close those workspaces,
            // but closing Home workspace has a different meaning. First off, 
            // Home workspace cannot be closed or hidden, it can only be cleared.
            // As of this revision, pressing the "X" button on Home workspace 
            // tab simply clears the Home workspace, and bring up the Start Page
            // if there are no other custom workspace that is opened.
            // 
            var dvm = dynSettings.Controller.DynamoViewModel;

            if (this.IsHomeSpace)
            {
                if (dvm.CloseHomeWorkspaceCommand.CanExecute(null))
                    dvm.CloseHomeWorkspaceCommand.Execute(null);
            }
            else
            {
                if (!Model.HasUnsavedChanges || dvm.AskUserToSaveWorkspaceOrCancel(Model))
                    dvm.Model.HideWorkspace(_model);
            }
        }

        private static bool CanHide(object parameters)
        {
            // Workspaces other than HOME can be hidden (i.e. closed), but we 
            // are enabling it also for the HOME workspace. When clicked, the 
            // HOME workspace is cleared (i.e. equivalent of pressing the New 
            // button), and if there is no other workspaces opened, then the 
            // Start Page is displayed.
            // 
            return true;
        }

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            if (_model.X != p.X && _model.Y != p.Y)
            {
                _model.X = p.X;
                _model.Y = p.Y;
            }
        }

        private static bool CanSetCurrentOffset(object parameter)
        {
            return true;
        }

        private void CreateNodeFromSelection(object parameter)
        {
            CollapseNodes(
                DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
                    .Select(x => (x as NodeModel)));
        }

        //private void NodeFromSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    NodeFromSelectionCommand.RaiseCanExecuteChanged();
        //}

        private void AlignSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AlignSelectedCommand.RaiseCanExecuteChanged();
        }

        private static bool CanCreateNodeFromSelection(object parameter)
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>().Any();
        }

        private bool CanZoom(double zoom)
        {
            return (!(zoom < 0) || !(_model.Zoom <= WorkspaceModel.ZOOM_MINIMUM)) && (!(zoom > 0) || !(_model.Zoom >= WorkspaceModel.ZOOM_MAXIMUM));
        }

        private void SetZoom(object zoom)
        {
            _model.Zoom = Convert.ToDouble(zoom);
        }

        private static bool CanSetZoom(object zoom)
        {
            double setZoom = Convert.ToDouble(zoom);
            return setZoom >= WorkspaceModel.ZOOM_MINIMUM && setZoom <= WorkspaceModel.ZOOM_MAXIMUM;
        }

        private bool _fitViewActualZoomToggle = false;

        internal void FitViewInternal()
        {
            // Get the offset and focus width & height (zoom if 100%)
            double minX, maxX, minY, maxY;

            // Get the width and height of area to fit
            if (DynamoSelection.Instance.Selection.Count > 0)
            {   // has selection
                minX = GetSelectionMinX();
                maxX = GetSelectionMaxX();
                minY = GetSelectionMinY();
                maxY = GetSelectionMaxY();
            }
            else
            {   // no selection, fitview all nodes
                if (!_nodes.Any()) return;

                List<NodeModel> nodes = _nodes.Select(x => x.NodeModel).Where(x => x != null).ToList();
                minX = nodes.Select(x => x.X).Min();
                maxX = nodes.Select(x => x.X + x.Width).Max();
                minY = nodes.Select(y => y.Y).Min();
                maxY = nodes.Select(y => y.Y + y.Height).Max();
            }

            var offset = new Point(minX, minY);
            double focusWidth = maxX - minX;
            double focusHeight = maxY - minY;

            _fitViewActualZoomToggle = !_fitViewActualZoomToggle;
            ZoomEventArgs zoomArgs = _fitViewActualZoomToggle
                ? new ZoomEventArgs(offset, focusWidth, focusHeight)
                : new ZoomEventArgs(offset, focusWidth, focusHeight, 1.0);

            OnRequestZoomToFitView(this, zoomArgs);
        }

        private void ResetFitViewToggle(object o)
        {
            _fitViewActualZoomToggle = false;
        }

        private static bool CanResetFitViewToggle(object o)
        {
            return true;
        }

        private static void FindById(object id)
        {
            try
            {
                var node =
                    dynSettings.Controller.DynamoModel.Nodes.First(
                        x => x.GUID.ToString() == id.ToString());

                if (node != null)
                {
                    //select the element
                    DynamoSelection.Instance.ClearSelection();
                    DynamoSelection.Instance.Selection.Add(node);

                    //focus on the element
                    dynSettings.Controller.DynamoViewModel.ShowElement(node);

                    return;
                }
            }
            catch
            {
                dynSettings.DynamoLogger.Log("No node could be found with that Id.");
            }

            try
            {
                var function =
                    (Function)
                        dynSettings.Controller.DynamoModel.Nodes.First(
                            x =>
                                x is Function
                                    && ((Function)x).Definition.FunctionId.ToString()
                                        == id.ToString());

                if (function == null) return;

                //select the element
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(function);

                //focus on the element
                dynSettings.Controller.DynamoViewModel.ShowElement(function);
            }
            catch
            {
                dynSettings.DynamoLogger.Log("No node could be found with that Id.");
            }
        }

        private static bool CanFindById(object id)
        {
            return !string.IsNullOrEmpty(id.ToString());
        }

        private void FindNodesFromSelection(object parameter)
        {
            FindNodesFromElements();
        }

        private bool CanFindNodesFromSelection(object parameter)
        {
            return FindNodesFromElements != null;
        }

        private void DoGraphAutoLayout(object o)
        {
            if (_model.Nodes.Count == 0)
                return;

            var graph = new GraphLayout.Graph();
            var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
            
            foreach (NodeModel x in _model.Nodes)
            {
                graph.AddNode(x.GUID, x.Width, x.Height, x.Y);
                models.Add(x, UndoRedoRecorder.UserAction.Modification);
            }

            foreach (ConnectorModel x in _model.Connectors)
            {
                graph.AddEdge(x.Start.Owner.GUID, x.End.Owner.GUID, x.Start.Center.Y, x.End.Center.Y);
                models.Add(x, UndoRedoRecorder.UserAction.Modification);
            }

            _model.RecordModelsForModification(new List<ModelBase>(_model.Nodes));
            
            // Sugiyama algorithm steps
            graph.RemoveCycles();
            graph.AssignLayers();
            graph.OrderNodes();
            
            // Assign coordinates to node models
            graph.NormalizeGraphPosition();
            foreach (var x in _model.Nodes)
            {
                var id = x.GUID;
                x.X = graph.FindNode(id).X;
                x.Y = graph.FindNode(id).Y;
                x.ReportPosition();
            }

            // Fit view to the new graph layout
            DynamoSelection.Instance.ClearSelection();
            ResetFitViewToggle(null);
            FitViewInternal();
        }

        private static bool CanDoGraphAutoLayout(object o)
        {
            return true;
        }

        /// <summary>
        ///     Collapse a set of nodes in the current workspace.  Has the side effects of prompting the user
        ///     first in order to obtain the name and category for the new node, 
        ///     writes the function to a dyf file, adds it to the FunctionDict, adds it to search, and compiles and 
        ///     places the newly created symbol (defining a lambda) in the Controller's FScheme Environment.  
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void CollapseNodes(IEnumerable<NodeModel> selectedNodes)
        {
            NodeCollapser.Collapse(selectedNodes, dynSettings.Controller.DynamoViewModel.CurrentSpace);
        }

        internal void Loaded()
        {
            RaisePropertyChanged("IsHomeSpace");

            // New workspace or swapped workspace to follow it offset and zoom
            OnCurrentOffsetChanged(this, new PointEventArgs(new Point(Model.X, Model.Y)));
            OnZoomChanged(this, new ZoomEventArgs(Model.Zoom));
        }

        private static void PauseVisualizationManagerUpdates(object parameter)
        {
            dynSettings.Controller.VisualizationManager.Pause();
        }

        private static bool CanPauseVisualizationManagerUpdates(object parameter)
        {
            return true;
        }

        private static void UnPauseVisualizationManagerUpdates(object parameter)
        {
            dynSettings.Controller.VisualizationManager.UnPause();
        }

        private static bool CanUnPauseVisualizationManagerUpdates(object parameter)
        {
            return true;
        }
    }

    public class ViewModelEventArgs : EventArgs
    {
        public NodeViewModel ViewModel { get; set; }
        public ViewModelEventArgs(NodeViewModel vm)
        {
            ViewModel = vm;
        }
    }
}
