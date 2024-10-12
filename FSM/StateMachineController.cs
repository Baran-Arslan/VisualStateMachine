using UnityEngine;

namespace iCare.Core {
    public sealed class StateMachineController : MonoBehaviour {
        [SerializeField] StateMachine stateMachine;

        void Awake() {
            stateMachine = stateMachine.Clone();
        }

        void Start() {
            stateMachine.InitFrom(this);
            stateMachine.Enter();
        }

        void Update() {
            stateMachine.Tick();
        }

        internal StateMachine GetStateMachine() {
            return stateMachine;
        }

        internal void SwitchState(string newStateID) {
            stateMachine.SwitchState(newStateID);
        }
        
        internal void SwitchState(State state) => stateMachine.SwitchState(state.UniqueID);
    }
}