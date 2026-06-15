namespace FlowTrack.Shared.Domain.FilterCriterias
{
    public class Filters
    {
        private readonly List<List<Filter>> _groups;

        public IReadOnlyList<IReadOnlyList<Filter>> Groups => _groups;

        public Filters(params Filter[] filters)
        {
            _groups =
            [
                [.. filters],
            ];
        }

        public Filters Or(params Filter[] filters)
        {
            _groups.Add([.. filters]);
            return this;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Filters other || _groups.Count != other._groups.Count)
                return false;

            for (var i = 0; i < _groups.Count; i++)
            {
                foreach (var filter in _groups[i])
                {
                    var otherFilter = other._groups[i].FirstOrDefault(f => f.Equals(filter));
                    if (otherFilter is null)
                        return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var group in _groups)
            foreach (var filter in group)
                hash.Add(filter);
            return hash.ToHashCode();
        }
    }
}
