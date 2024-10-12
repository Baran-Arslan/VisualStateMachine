using UnityEngine;

namespace iCare.Core.StateTemplates {
    [StateCategory("Templates")]
    internal sealed class TeleportState : ActionState {
        [SerializeField] float delay = 1f;
        [SerializeField] ActionState nextState;
        float _timer;


        public override void OnStateEnter() {
            _timer = delay;
        }

        public override void OnStateTick() {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) {
                Controller.SwitchState(nextState);
            }
        }
    }
}