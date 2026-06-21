using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FlowTrack.Shared.Domain.FilterCriterias;

namespace FlowTrack.Shared.Infrastructure.Persistence
{
    public static class ElasticFilterCriteriaConverter
    {
        private static readonly JsonNamingPolicy NamingPolicy = JsonNamingPolicy.CamelCase;

        /// <summary>
        ///     Converts a <see cref="FilterCriteria" /> domain object into an
        ///     <see cref="Action{SearchRequestDescriptor{T}}" /> for use with the
        ///     Elastic.Clients.Elasticsearch library.
        /// </summary>
        /// <typeparam name="T">The document type (e.g., WorkspaceSearchDocument).</typeparam>
        /// <param name="criteria">The domain filter criteria.</param>
        /// <returns>
        ///     An action that configures a <see cref="SearchRequestDescriptor{T}" />
        ///     with the query, sort, and pagination derived from the criteria.
        /// </returns>
        public static Action<SearchRequestDescriptor<T>> Apply<T>(FilterCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(criteria);

            return descriptor =>
            {
                if (criteria.Filters is { Groups.Count: > 0 })
                {
                    descriptor.Query(q => BuildQuery(q, criteria.Filters));
                }

                if (
                    criteria.Order is { OrderBy.PropertyName.Length: > 0 }
                    && criteria.Order.OrderType.Type != OrderTypes.NONE
                )
                {
                    descriptor.Sort(s => ApplySorting(s, criteria.Order));
                }

                if (criteria.Offset.HasValue)
                    descriptor.From(criteria.Offset.Value);

                if (criteria.Limit.HasValue)
                    descriptor.Size(criteria.Limit.Value);
            };
        }

        /// <summary>
        ///     Builds an Elasticsearch query from the domain Filters model.
        ///     AND within a group, OR between groups.
        /// </summary>
        private static void BuildQuery<T>(QueryDescriptor<T> q, Filters filters)
        {
            var groups = filters.Groups;

            if (groups.Count == 1)
            {
                var group = groups[0];

                if (group.Count == 1)
                {
                    BuildFilterQuery(q, group[0]);
                }
                else
                {
                    q.Bool(b =>
                    {
                        foreach (var filter in group)
                        {
                            b.Must(m => BuildFilterQuery(m, filter));
                        }
                    });
                }
            }
            else
            {
                q.Bool(b =>
                {
                    b.MinimumShouldMatch(1);

                    foreach (var group in groups)
                    {
                        b.Should(s =>
                        {
                            if (group.Count == 1)
                            {
                                BuildFilterQuery(s, group[0]);
                            }
                            else
                            {
                                s.Bool(b2 =>
                                {
                                    foreach (var filter in group)
                                    {
                                        b2.Must(m => BuildFilterQuery(m, filter));
                                    }
                                });
                            }
                        });
                    }
                });
            }
        }

        /// <summary>
        ///     Converts a single domain Filter into an Elasticsearch query clause.
        /// </summary>
        private static void BuildFilterQuery<T>(QueryDescriptor<T> q, Filter filter)
        {
            var fieldName = ToElasticFieldName(filter.Field.FieldName);

            // Term-level queries (Term, Range, Wildcard, Prefix) on text fields search
            // the analyzed tokens, not the full value. We use the .keyword sub-field to
            // get exact match semantics consistent with the domain FilterCriteria model.
            var exactFieldName = $"{fieldName}.keyword";

            switch (filter.Operator.Type)
            {
                case FilterOperators.Equals:
                    q.Term(t => t.Field(exactFieldName).Value(filter.Value.FieldValue));
                    break;

                case FilterOperators.NotEquals:
                    q.Bool(b =>
                        b.MustNot(mn =>
                            mn.Term(t => t.Field(exactFieldName).Value(filter.Value.FieldValue))
                        )
                    );
                    break;

                case FilterOperators.GreaterThan:
                    q.Range(r => r.Term(t => t.Field(fieldName).Gt(filter.Value.FieldValue)));
                    break;

                case FilterOperators.GreaterThanOrEqual:
                    q.Range(r => r.Term(t => t.Field(fieldName).Gte(filter.Value.FieldValue)));
                    break;

                case FilterOperators.LessThan:
                    q.Range(r => r.Term(t => t.Field(fieldName).Lt(filter.Value.FieldValue)));
                    break;

                case FilterOperators.LessThanOrEqual:
                    q.Range(r => r.Term(t => t.Field(fieldName).Lte(filter.Value.FieldValue)));
                    break;

                case FilterOperators.Contains:
                    q.Wildcard(w => w.Field(fieldName).Value($"*{filter.Value.FieldValue}*"));
                    break;

                case FilterOperators.StartsWith:
                    q.Prefix(p => p.Field(fieldName).Value(filter.Value.FieldValue));
                    break;

                case FilterOperators.EndsWith:
                    q.Wildcard(w => w.Field(fieldName).Value($"*{filter.Value.FieldValue}"));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Filter operator '{filter.Operator.Type}' is not supported."
                    );
            }
        }

        /// <summary>
        ///     Applies sorting to the search descriptor.
        /// </summary>
        private static void ApplySorting<T>(SortOptionsDescriptor<T> s, Order order)
        {
            var fieldName = ToElasticFieldName(order.OrderBy.PropertyName);

            s.Field(
                fieldName,
                order.OrderType.Type == OrderTypes.ASC ? SortOrder.Asc : SortOrder.Desc
            );
        }

        /// <summary>
        ///     Converts a PascalCase property name (domain convention) to camelCase
        ///     (Elasticsearch field convention).
        /// </summary>
        private static string ToElasticFieldName(string propertyName)
        {
            return NamingPolicy.ConvertName(propertyName);
        }
    }
}
