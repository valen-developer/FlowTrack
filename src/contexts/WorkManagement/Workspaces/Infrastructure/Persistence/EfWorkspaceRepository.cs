using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence
{
    [Provider(typeof(IWorkspaceRepository), Lifetime.Scoped)]
    internal class EfWorkspaceRepository(WorkspaceDao workspaceDao) : IWorkspaceRepository
    {
        public async Task<List<Workspace>> Matching(FilterCriteria criteria)
        {
            var entities = await workspaceDao.Matching(criteria);
            var workspaces = entities.Select(e => e.ToDomain()).ToList();
            return workspaces;
        }

        public async Task<Workspace?> MatchingOne(FilterCriteria criteria)
        {
            var entities = await workspaceDao.Matching(criteria);
            var workspace = entities.Select(e => e.ToDomain()).FirstOrDefault();

            return workspace;
        }

        public async Task Save(Workspace workspace)
        {
            var entity = WorkspaceEntity.FromDomain(workspace);
            await workspaceDao.Insert(entity);
        }
    }
}
