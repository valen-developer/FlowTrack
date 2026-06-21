using FlowTrack.Shared.Domain.Bus.Query;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Application;

internal sealed record FindWorkspacesByOwnerQry(string OwnerId) : IQuery<List<Workspace>> { }
