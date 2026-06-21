using Elastic.Clients.Elasticsearch;
using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.Exception;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Shared.Domain;
using FlowTrack.WorkManagement.Workspaces.Domain;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure;

[Provider(typeof(IWorkspaceSearchEngine))]
internal class ElasticSearchWorkspaceSearchEngine : IWorkspaceSearchEngine
{
    private const string IndexName = "workspaces";
    private readonly ElasticsearchClient _client;

    public ElasticSearchWorkspaceSearchEngine(IEnvStore envStore)
    {
        var urlKey = WorkManagementEnvironmentKeysEnum.WORK_MANAGEMENT_ELASTICSEARCH_URL.ToString();

        var elasticSearchUrl = envStore.Get(urlKey) ?? throw new EnvVariableMissed(urlKey);
        _client = new(new ElasticsearchClientSettings(new Uri(elasticSearchUrl)));
    }

    public Task<List<Workspace>> Find(FilterCriteria criteria)
    {
        throw new NotImplementedException();
    }

    public async Task Index(Workspace workspace)
    {
        var document = WorkspaceSearchDocument.FromDomain(workspace);
        var response = await _client.IndexAsync(document, i => i.Index(IndexName));

        if (!response.IsValidResponse)
        {
            throw new Exception(
                $"Failed to index workspace {workspace.Id.Value}: {response.ElasticsearchServerError?.Error.Reason}"
            );
        }
    }
}
