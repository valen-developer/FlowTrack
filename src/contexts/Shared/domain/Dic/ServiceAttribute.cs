namespace FlowTrack.Shared.Domain.Dic
{
    public enum Lifetime
    {
        Transient,
        Scoped,
        Singleton,
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ServiceAttribute(Lifetime lifetime = Lifetime.Scoped) : Attribute
    {
        public Lifetime Lifetime { get; } = lifetime;
    }
}
