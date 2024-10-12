using System.Linq;

namespace iCare.Core {
    public sealed class EntryState : State {
        internal string GetEntryStateID() {
            return GetTransitions().ToList()[0].GetTrueStateID();
        }
    }
}