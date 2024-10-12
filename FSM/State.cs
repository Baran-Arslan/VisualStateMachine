using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("iCare.Core.Editor")]

namespace iCare.Core {
    [Icon("Assets/iCare/Core/Icons/unity.png")]
    public abstract class State : ScriptableObject {
        [OnValueChanged(nameof(UpdateObjectName))]
        [SerializeField] string title;

        [HideInInspector]
        [SerializeField] string uniqueID;

        [HideLabel]
        [TableList(AlwaysExpanded = true, DrawScrollView = false, ShowPaging = false, IsReadOnly = true)]
        [SerializeField] List<Transition> transitions = new();


        bool _started;
        protected StateMachineController Controller { get; private set; }

        readonly Dictionary<string, Transition> _transitionLookup = new();


        void Awake() {
            OnValidate();
        }

        void OnValidate() {
            _transitionLookup.Clear();

            foreach (var transition in transitions.Where(transition => transition != null))
                _transitionLookup[transition.GetTrueStateID()] = transition;

            title ??= GetType().Name;
        }

        internal void Init(StateMachineController controller) {
            Controller = controller;
            this.TryInjectFrom(controller.gameObject);
            var conditions = transitions.SelectMany(transition => transition.Conditions);
            foreach (var condition in conditions) condition.TryInjectFrom(controller.gameObject);
        }

        internal bool Started => _started;

        internal string UniqueID => uniqueID;

        internal void SetUniqueID(string id) {
            uniqueID = id;
        }

        internal string GetTitle() {
            return title;
        }

        internal void SetTitle(string targetTitle) {
            title = targetTitle;
        }


        internal IEnumerable<Transition> GetTransitions() {
            return transitions;
        }

        internal void Enter() {
            foreach (var transition in transitions) transition.OnStateEnter();
            _started = true;
            OnStateEnter();
        }

        internal void Tick() {
            CheckTransitions();
            OnStateTick();
        }

        internal void Exit() {
            foreach (var transition in transitions) transition.OnStateExit();
            OnStateExit();
            _started = false;
        }

        public virtual void OnStateEnter() { }
        public virtual void OnStateExit() { }
        public virtual void OnStateTick() { }

        Transition GetTransition(string trueStateID) {
            return _transitionLookup.GetValueOrDefault(trueStateID);
        }

        void CheckTransitions() {
            foreach (var trueStateID in from transition in transitions
                     let success = transition.Check()
                     where success
                     select transition.GetTrueStateID()) {
                Controller.SwitchState(trueStateID);
            }
        }

#if UNITY_EDITOR
        internal void SetPosition(Vector2 pos) {
            Undo.RecordObject(this, "State Moved");
            position = pos;
            EditorUtility.SetDirty(this);
        }

        internal void AddTransition(string trueStateID, string trueStateKey) {
            Undo.RecordObject(this, "Transition Added");
            transitions.Add(new Transition(uniqueID, trueStateID, trueStateKey));
            OnValidate();
            EditorUtility.SetDirty(this);
        }

        internal void RemoveTransition(string trueStateID) {
            Undo.RecordObject(this, "Transition Removed");
            transitions.Remove(GetTransition(trueStateID));
            OnValidate();
            EditorUtility.SetDirty(this);
        }

        void UpdateObjectName(string newName) {
            name = newName;
            EditorUtility.SetDirty(this);
        }

        internal Vector2 GetPosition() {
            return position;
        }

        //TODO - If cause bug in the future, remove from if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] Vector2 position;


#endif
    }
}