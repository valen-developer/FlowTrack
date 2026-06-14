using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.Shared.Infrastructure.Persistence;
using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence;

[Service(Lifetime.Scoped)]
internal class WorkspaceDao(WorkManagementDbContext dbContext)
{
    public async Task Insert(WorkspaceEntity entity)
    {
        dbContext.Workspaces.Add(entity);
    }

    public async Task<List<WorkspaceEntity>> Matching(FilterCriteria criteria)
    {
        var query = EfFilterCriteriaConverter.Apply(dbContext.Workspaces.AsQueryable(), criteria);
        var entities = await query.ToListAsync();

        return entities;
    }
}
