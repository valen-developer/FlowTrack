namespace FlowTrack.Shared.Domain;

public class EnvVariableMissed(string key)
    : InternalException(
        $"Environment variable {key} is required",
        "exception.internal.env_variable_missed"
    ) { }
