using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mobs.Events;

/// <summary>
///     Event for allowing the interrupting and change of the mob threshold severity alert
/// </summary>
[ByRefEvent]
public record struct BeforeAlertSeverityCheckEvent(
    ProtoId<AlertPrototype> CurrentAlert,
    short Severity,
    bool CancelUpdate = false);

