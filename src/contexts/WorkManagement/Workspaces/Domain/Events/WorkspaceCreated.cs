using FlowTrack.Shared.Domain.Bus.Event;

namespace FlowTrack.WorkManagement.Workspaces.Domain
{
    internal record WorkspaceCreated(string Id, string OwnerId, string Name)
        : DomainEvent,
            IDomainEvent
    {
        public static bool External => false;

        public static string Code => "flowtrack.workmanagement.1.event.workspace.created";

        public override string GetCode()
        {
            return Code;
        }

        public override bool IsExternal()
        {
            return External;
        }
    }
}
