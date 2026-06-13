using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal sealed record CreateDefaultWorkspaceCmd(string UserId) : ICommand { }
