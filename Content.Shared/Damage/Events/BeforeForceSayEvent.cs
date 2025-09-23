using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared.Damage.Events;

/// <summary>
///     Event for interrupting and changing the prefix for when an entity is being forced to say something
/// </summary>
[ByRefEvent]
public record struct BeforeForceSayEvent(ProtoId<LocalizedDatasetPrototype> PrefixDataset);
