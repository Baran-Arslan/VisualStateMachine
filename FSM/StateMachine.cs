using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace iCare.Core {
    [CreateAssetMenu(menuName = "iCare/Core/FSM", fileName = "New State Machine", order = -1000)]
    public sealed class StateMachine : ScriptableObject, ISerializationCallbackReceiver {
        [HideInInspector]
        [SerializeField] EntryState entryState;

        [HideInInspector]
        [SerializeField] AnyState anyState;

        [HideInInspector]
        [SerializeField] List<State> states = new();

        readonly Vector2 _anyStateOffset = new(250, 50);
        readonly Vector2 _entryStateOffset = new(250, 0);
        readonly Dictionary<string, State> _stateLookup = new();
        State _currentState;

        void Awake() {
            OnValidate();
        }

        void OnEnable() {
            _currentState = null;
        }

        void OnValidate() {
            _stateLookup.Clear();

            foreach (var state in states.Where(state => state != null)) _stateLookup[state.UniqueID] = state;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
#if UNITY_EDITOR
            if (AssetDatabase.GetAssetPath(this) == "") return;

            foreach (var state in states.Where(state => AssetDatabase.GetAssetPath(state) == ""))
                AssetDatabase.AddObjectToAsset(state, this);

            if (entryState == null) {
                entryState = MakeState(typeof(EntryState), _entryStateOffset) as EntryState;
                if (entryState == null) return;
                entryState.SetTitle("Entry");
                AddState(entryState);
                
            }

            if (anyState != null) return;

            anyState = MakeState(typeof(AnyState), _anyStateOffset) as AnyState;
            anyState!.SetTitle("Any");
            AddState(anyState);
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        internal void InitFrom(StateMachineController controller) {
            foreach (var state in states) {
                state.Init(controller);
            }
        }

        internal StateMachine Clone() {
            var clone = Instantiate(this);

            clone.states.Clear();
            clone._stateLookup.Clear();

            foreach (var state in states) clone.AddState(Instantiate(state));

            clone.entryState = clone.GetState(entryState.UniqueID) as EntryState;
            clone.anyState = clone.GetState(anyState.UniqueID) as AnyState;

            return clone;
        }

        internal State GetState(string stateID) {
            if (_stateLookup.TryGetValue(stateID, out var state)) return state;
            Debug.LogError($"State with ID {stateID} not found");
            return null;
        }

        internal IEnumerable<State> GetStates() {
            return states;
        }

        internal void Enter() {
            SwitchState(entryState.GetEntryStateID());
        }

        internal void Tick() {
            _currentState.Tick();
            anyState.Tick();
        }

        internal void SwitchState(string newStateID) {
            _currentState?.Exit();
            _currentState = GetState(newStateID);
            _currentState.Enter();
        }

        void AddState(State newState) {
            states.Add(newState);
            OnValidate();
        }

#if UNITY_EDITOR
        internal State CreateState(Type type, Vector2 position) {
            var newState = MakeState(type, position);
            Undo.RegisterCreatedObjectUndo(newState, "State Created");
            Undo.RecordObject(this, "State Added");
            AddState(newState);
            return newState;
        }

        internal void RemoveState(State stateToRemove) {
            Undo.RecordObject(this, "State Removed");
            states.Remove(stateToRemove);
            OnValidate();
            Undo.DestroyObjectImmediate(stateToRemove);
        }

        static State MakeState(Type type, Vector2 position) {
            var newState = CreateInstance(type) as State;
            newState!.name = type.Name;
            newState.SetUniqueID(Guid.NewGuid().ToString());
            newState.SetPosition(position);
            return newState;
        }
#endif
    }
}