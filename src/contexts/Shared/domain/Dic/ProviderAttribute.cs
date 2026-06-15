namespace FlowTrack.Shared.Domain.Dic
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ProviderAttribute(Type serviceType, Lifetime lifetime = Lifetime.Scoped)
        : Attribute
    {
        public Type ServiceType { get; } = serviceType;
        public Lifetime Lifetime { get; } = lifetime;
    }
}
