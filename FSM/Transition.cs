using System;
using System.Linq;
using iCare.Core.ConditionTemplates;
using Sirenix.OdinInspector;
using UnityEngine;

namespace iCare.Core {
    [Serializable] [HideLabel]
    public class Transition {
        [HideInInspector]
        [SerializeField] string rootStateID;

        [HideInInspector]
        [SerializeField] string trueStateID;

        [Title("TO STATE", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
        [SerializeField] [HideLabel] [DisplayAsString(false, 20, TextAlignment.Center, true)]
        // ReSharper disable once NotAccessedField.Local
        [VerticalGroup("State Transitions")] string toState;

        [Space(20)]
        [VerticalGroup("State Transitions", PaddingBottom = 20)]
        [LabelText("Conditions", SdfIconType.Calculator)] 
        [ListDrawerSettings(ShowFoldout = false, DefaultExpandedState = true, ElementColor = "red")]
        [SerializeReference] IStateCondition[] conditions = {
            new StateTimeCondition()
        };

        internal IStateCondition[] Conditions => conditions;

        public Transition(string rootStateID, string trueStateID, string toState) {
            this.rootStateID = rootStateID;
            this.trueStateID = trueStateID;
            this.toState = $"<b><color=#FF5555><i>{toState}</i></color></b>";
        }

        public string GetRootStateID() {
            return rootStateID;
        }

        public string GetTrueStateID() {
            return trueStateID;
        }

        public void OnStateEnter() {
            foreach (var condition in conditions) condition.OnStateEnter();
        }

        public void OnStateExit() {
            foreach (var condition in conditions) condition.OnStateExit();
        }

        public bool Check() {
            return conditions.All(condition => condition.IsMet());
        }
    }
}