using Robust.Shared.GameStates;

namespace Content.Shared.DV_.Salvage.Components;

/// <summary>
/// Adds points to <see cref="MiningPointsComponent"/> when making a recipe that has miningPoints set.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MiningPointsLatheComponent : Component;
