using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace iCare.Core.Editor {
    internal sealed class StateView : Node {
        readonly VisualElement _borderContainer;
        readonly State _state;
        readonly VisualElement _updateContainer;
        Port _inputPort;
        Port _outputPort;

        internal StateView(State state) : base(StateMachineEditor.Path + "StateView.uxml") {
            _state = state;

            if (state == null) 
                return;
            
            viewDataKey = state.UniqueID;
            style.left = state.GetPosition().x;
            style.top = state.GetPosition().y;

            _borderContainer = this.Q<VisualElement>("node-border");
            _updateContainer = this.Q<VisualElement>("state-update");

            CreatePorts();
            SetTitle();
            SetStyle();
            SetCapabilites();
        }

        public State GetState() {
            return _state;
        }

        internal TransitionEdge ConnectTo(StateView stateView) {
            return _outputPort.ConnectTo<TransitionEdge>(stateView._inputPort);
        }

        internal void UpdateView() {
            if (Application.isPlaying) {
                if (_state.Started)
                    _updateContainer.AddToClassList("runningState");
                else
                    _updateContainer.RemoveFromClassList("runningState");
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            _state.SetPosition(new Vector2(newPos.x, newPos.y));
        }

        public override void OnSelected() {
            base.OnSelected();

            if (_state is not EntryState) Selection.activeObject = _state;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            if (!Application.isPlaying) evt.menu.AppendAction("Add Transition", _ => DragTransitionEdge());
        }

        public override void CollectElements(HashSet<GraphElement> collectedElementSet,
            Func<GraphElement, bool> conditionFunc) {
            base.CollectElements(collectedElementSet, conditionFunc);

            if (_inputPort != null)
                foreach (var connection in _inputPort.connections)
                    collectedElementSet.Add(connection);

            if (_outputPort != null)
                foreach (var connection in _outputPort.connections)
                    collectedElementSet.Add(connection);
        }

        void CreatePorts() {
            if (_state is ActionState) {
                _inputPort = CreatePort(Direction.Input, Port.Capacity.Multi);
                _outputPort = CreatePort(Direction.Output, Port.Capacity.Multi);
            }

            if (_state is EntryState) _outputPort = CreatePort(Direction.Output, Port.Capacity.Single);

            if (_state is AnyState) _outputPort = CreatePort(Direction.Output, Port.Capacity.Multi);
        }

        Port CreatePort(Direction direction, Port.Capacity capacity) {
            var port = Port.Create<TransitionEdge>(Orientation.Vertical, direction, capacity, typeof(bool));
            Insert(0, port);
            return port;
        }

        void SetTitle() {
            if (_state is ActionState)
                BindTitle();
            else
                title = _state.GetTitle();
        }

        void BindTitle() {
            var titleLabel = this.Q<Label>("title-label");
            titleLabel.bindingPath = "title";
            titleLabel.Bind(new SerializedObject(_state));
        }

        void SetStyle() {
            if (_state is ActionState) _borderContainer.AddToClassList("actionState");

            if (_state is EntryState) _borderContainer.AddToClassList("entryState");

            if (_state is AnyState) _borderContainer.AddToClassList("anyState");
        }

        void SetCapabilites() {
            if (_state is EntryState || _state is AnyState) capabilities -= Capabilities.Deletable;
        }

        void DragTransitionEdge() {
            _outputPort.SendEvent(new DragEvent(_outputPort.GetGlobalCenter(), _outputPort));
        }

        sealed class DragEvent : MouseDownEvent {
            public DragEvent(Vector2 mousePosition, VisualElement target) {
                this.mousePosition = mousePosition;
                this.target = target;
            }
        }
    }
}