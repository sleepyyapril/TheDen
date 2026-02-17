// SPDX-FileCopyrightText: 2025 Mnemotechnican
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Floof.Examine;
using Content.Shared.Verbs;
using Robust.Client.Player;
using Robust.Shared.Utility;


namespace Content.Client._Floof.Examine;


public sealed class CustomExamineSystem : SharedCustomExamineSystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private SharedCustomExamineSystem _sharedCustomExamineSystem = default!;
    private CustomExamineSettingsWindow? _window = null;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<CustomExamineComponent, AfterAutoHandleStateEvent>(OnStateUpdate);

        _sharedCustomExamineSystem = _entityManager.System<SharedCustomExamineSystem>();
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (_player.LocalEntity is null || !CanChangeExamine(_player.LocalEntity.Value, args.Target))
            return;

        var target = args.Target;
        args.Verbs.Add(new()
        {
            Act = () => OpenUi(target),
            Text = Loc.GetString("custom-examine-verb"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/edit.svg.png")),
            ClientExclusive = true,
            DoContactInteraction = false
        });
    }

    private void OnStateUpdate(Entity<CustomExamineComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _window?.SetData(ent.Comp.Data);
    }

    private void OpenUi(EntityUid target)
    {
        if (_player.LocalEntity != null
            && !_sharedCustomExamineSystem.CanChangeExamine(target, _player.LocalEntity.Value))
            return;

        if (_window == null)
            EnsureWindow(target);

        // This will create a local component if it didn't exist before, but after sending the data to server it will become shared.
        var comp = EnsureComp<CustomExamineComponent>(target);
        _window?.SetData(comp.Data);

        if (_window!.IsOpen)
            _window.Close();
        else
            _window.OpenCenteredLeft(); // mid-top-center
    }

    private void OnSave(EntityUid target, List<CustomExamineData> data)
    {
        var ev = new SetCustomExamineMessage
        {
            Data = data,
            Target = GetNetEntity(target)
        };
        RaiseNetworkEvent(ev);
    }

    private void EnsureWindow(EntityUid target)
    {
        _window = new();

        _window.OnClose += () => _window = null;
        _window.OnSave += data => OnSave(target, data);
    }
}
