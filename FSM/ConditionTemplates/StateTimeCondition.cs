using UnityEngine;

namespace iCare.Core.ConditionTemplates {
    [System.Serializable]
    internal sealed class StateTimeCondition : IStateCondition {
        [SerializeField] float duration = 1;
        float _startTime;

        public void OnStateEnter() {
            _startTime = Time.time;
        }

        public void OnStateExit() { }

        public bool IsMet() {
            var result = Time.time - _startTime >= duration;
            return result;
        }
    }
}