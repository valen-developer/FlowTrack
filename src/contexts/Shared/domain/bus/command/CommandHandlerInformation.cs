using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Shared.Domain;

public sealed class CommandHandlerInformation
{
    private readonly Dictionary<Type, Type> _handlers = new();

    public void Add(Type commandType, Type handlerType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
            throw new ArgumentException(
                $"Type '{commandType.FullName}' does not implement ICommand.",
                nameof(commandType)
            );

        if (!typeof(ICommandHandler<>).MakeGenericType(commandType).IsAssignableFrom(handlerType))
            throw new ArgumentException(
                $"Handler type '{handlerType.FullName}' does not implement ICommandHandler<{commandType.Name}>.",
                nameof(handlerType)
            );

        if (_handlers.ContainsKey(commandType))
            throw new InvalidOperationException(
                $"Command '{commandType.Name}' is already registered."
            );

        _handlers[commandType] = handlerType;
    }

    public Type Get<C>()
        where C : ICommand
    {
        var commandType = typeof(C);

        if (_handlers.TryGetValue(commandType, out var handlerType))
            return handlerType;

        throw new InvalidOperationException($"Command '{commandType.Name}' is not registered.");
    }
}
