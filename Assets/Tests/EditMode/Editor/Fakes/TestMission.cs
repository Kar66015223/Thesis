namespace EditModeTests
{
    /// <summary>Minimal concrete <see cref="Mission"/> that exposes Complete() and counts the hooks.</summary>
    public class TestMission : Mission
    {
        public TestMission()
            : base("Test", "A mission used by unit tests.", MissionType.Sub, "TestScene")
        {
        }

        public int AcceptedCalls { get; private set; }
        public int CompletedCalls { get; private set; }
        public int SubmittedCalls { get; private set; }

        public void ForceComplete() => Complete();

        protected override void OnAccepted() => AcceptedCalls++;
        protected override void OnCompleted() => CompletedCalls++;
        protected override void OnSubmitted() => SubmittedCalls++;
    }
}
