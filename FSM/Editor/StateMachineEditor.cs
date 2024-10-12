using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace iCare.Core.Editor {
    public sealed class StateMachineEditor : EditorWindow {
        internal const string Path = "Assets/iCare/Core/FSM/Editor/";
        StateMachineView _stateMachineView;

        void OnEnable() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void CreateGUI() {
            var root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path + "StateMachineEditor.uxml");
            visualTree.CloneTree(root);

            _stateMachineView = root.Q<StateMachineView>();

            OnSelectionChange();
        }

        void OnInspectorUpdate() {
            _stateMachineView?.UpdateStateViews();
        }

        void OnSelectionChange() {
            var stateMachine = Selection.activeObject as StateMachine;

            if (Selection.activeGameObject) {
                var controller = Selection.activeGameObject.GetComponent<StateMachineController>();

                if (controller != null) stateMachine = controller.GetStateMachine();
            }

            if (stateMachine != null) _stateMachineView.Refresh(stateMachine);
        }

        static void ShowWindow() {
            GetWindow(typeof(StateMachineEditor), false, "iCare FSM");
        }

        [OnOpenAsset]
        public static bool OnStateMachineOpened(int instanceID, int line) {
            var stateMachine = EditorUtility.InstanceIDToObject(instanceID) as StateMachine;

            if (stateMachine == null) return false;
            ShowWindow();
            return true;
        }

        void OnPlayModeStateChanged(PlayModeStateChange change) {
            if (_stateMachineView == null) return;
            if (change == PlayModeStateChange.EnteredEditMode) OnSelectionChange();
            if (change == PlayModeStateChange.EnteredPlayMode) OnSelectionChange();
        }
    }
}