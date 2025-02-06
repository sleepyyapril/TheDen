using Content.Server._DEN.Components;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Roles.Jobs;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;


namespace Content.Server._DEN.EntitySystems;


/// <summary>
/// This handles...
/// </summary>
public sealed class AutomatedPowerSystem : EntitySystem
{
    [Dependency] private JobSystem _jobSystem = default!;
    [Dependency] private StationRecordsSystem _recordsSystem = default!;
    [Dependency] private StationSystem _stationSystem = default!;
    [Dependency] private ChatSystem _chatSystem = default!;
    [Dependency] private TransformSystem _transformSystem = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    private ProtoId<DepartmentPrototype> _engineeringDepartment = "Engineering";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AutomatedPowerComponent, PlayerSpawnCompleteEvent>(OnPlayerJoin);
    }

    private void OnPlayerJoin(Entity<AutomatedPowerComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        if (args.JobId == null)
            return;

        var station = _stationSystem.GetOwningStation(ent);

        if (station == null)
            return;

        var engineers = GetEngineersCount(station.Value);
        _jobSystem.TryGetDepartment(args.JobId, out var department);

        if (department == null || department.ID != _engineeringDepartment || engineers != 1)
            return;

        var title = Loc.GetString("automated-power-system-announcement-title");
        var localizedMessage = Loc.GetString("automated-power-system-engineer-spawned");
        _chatSystem.DispatchStationAnnouncement(station.Value, localizedMessage, title, colorOverride: Color.Lavender);
        TryQueueDel(ent);
    }

    private int GetEngineersCount(EntityUid station)
    {
        var iter = _recordsSystem.GetRecordsOfType<GeneralStationRecord>(station);
        var count = 0;

        foreach (var record in iter)
        {
            var generalRecord = record.Item2;
            _jobSystem.TryGetDepartment(generalRecord.JobPrototype, out var department);

            if (department == null || department.ID != _engineeringDepartment)
                continue;

            count++;
        }

        return count;
    }
}
