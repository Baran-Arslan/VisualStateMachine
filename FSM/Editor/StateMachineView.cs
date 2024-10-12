using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace iCare.Core.Editor {
    public sealed class StateMachineView : GraphView {
        StateMachine _stateMachine;

        public StateMachineView() {
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(StateMachineEditor.Path + "StateMachineEditor.uss");
            styleSheets.Add(styleSheet);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        internal void Refresh(StateMachine stateMachine) {
            _stateMachine = stateMachine;

            graphViewChanged -= OnGraphViewChanged;

            DeleteElements(graphElements);

            graphViewChanged += OnGraphViewChanged;

            if (stateMachine != null) {
                foreach (var state in stateMachine.GetStates()) CreateStateView(state);

                foreach (var state in stateMachine.GetStates())
                foreach (var transition in state.GetTransitions())
                    CreateTransitionEdge(transition);
            }
        }

        internal void UpdateStateViews() {
            foreach (var node in nodes)
                if (node is StateView stateView)
                    stateView.UpdateView();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (Application.isPlaying) return;
            base.BuildContextualMenu(evt);
            Vector2 mousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            foreach (var stateType in GetStateTypes()) {
                var categoryName = stateType.GetCustomAttribute<StateCategory>() != null
                    ? stateType.GetCustomAttribute<StateCategory>().Category
                    : "States";
                evt.menu.AppendAction($"{categoryName} / {stateType.Name}", _ => CreateState(stateType, mousePosition));
            }
        }

        static IEnumerable<Type> GetStateTypes() {
            //Get all types that inherit from State and not AnyState or EntryState or Abstract
            //TODO - OPTIMIZE THIS
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ActionState)) && !type.IsAbstract);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.Where(endPort => endPort.direction != startPort.direction)
                .Where(endPort => !AreConnected(startPort, endPort)).ToList();
        }

        static bool AreConnected(Port startPort, Port endPort) {
            return startPort.connections.Any(connection => connection.input == endPort || connection.output == endPort);
        }

        void CreateStateView(State state) {
            StateView newStateView = new(state);
            AddElement(newStateView);
        }

        void CreateState(Type type, Vector2 mousePosition) {
            var newState = _stateMachine.CreateState(type, mousePosition);
            CreateStateView(newState);
        }

        void RemoveState(StateView stateView) {
            _stateMachine.RemoveState(stateView.GetState());
        }

        StateView GetStateView(string stateID) {
            return GetNodeByGuid(stateID) as StateView;
        }

        void CreateTransitionEdge(Transition transition) {
            var rootStateView = GetStateView(transition.GetRootStateID());
            var trueStateView = GetStateView(transition.GetTrueStateID());
            AddElement(rootStateView.ConnectTo(trueStateView));
        }

        void CreateTransition(TransitionEdge edge) {
            var rootState = _stateMachine.GetState(edge.output.node.viewDataKey);
            var key = edge.input.node.viewDataKey;
            rootState.AddTransition(key, GetStateView(key).GetState().GetTitle());
        }

        void RemoveTransition(TransitionEdge edge) {
            var rootState = _stateMachine.GetState(edge.output.node.viewDataKey);
            rootState.RemoveTransition(edge.input.node.viewDataKey);
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            var edgesToCreate = graphViewChange.edgesToCreate;

            if (edgesToCreate != null)
                foreach (var edge in edgesToCreate)
                    CreateTransition(edge as TransitionEdge);

            var elementsToRemove = graphViewChange.elementsToRemove;

            if (elementsToRemove == null) return graphViewChange;
            foreach (var element in elementsToRemove) {
                if (element is StateView stateView) RemoveState(stateView);

                if (element is TransitionEdge transitionEdge) RemoveTransition(transitionEdge);
            }

            return graphViewChange;
        }

        void OnUndoRedo() {
            Refresh(_stateMachine);
        }
#pragma warning disable CS0618 // Type or member is obsolete
        new sealed class UxmlFactory : UxmlFactory<StateMachineView, UxmlTraits> { }
#pragma warning restore CS0618 // Type or member is obsolete

    }
}