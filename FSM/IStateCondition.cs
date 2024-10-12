namespace iCare.Core {
    public interface IStateCondition {
        void OnStateEnter();
        void OnStateExit();
        bool IsMet();
    }
}