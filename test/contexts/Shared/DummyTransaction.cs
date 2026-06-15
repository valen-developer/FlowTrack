namespace FlowTrack.Shared.Test
{
    public sealed class DummyTransaction : Transaction
    {
        protected override async Task Commit()
        {
            // Do nothing
        }

        protected override async Task Initialize()
        {
            // Do nothing
        }

        protected override async Task Release()
        {
            // Do nothing
        }

        protected override async Task Rollback()
        {
            // Do nothing
        }
    }
}
