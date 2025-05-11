using Content.Shared.GameTicking;

namespace Content.Server._MK.Jukebox;

public sealed class ServerJukeboxSongsSyncSystem : EntitySystem
{
    [Dependency] private readonly ServerJukeboxSongsSyncManager _jukeboxManager = default!;

    public event Action? PostRoundCleanUp;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _jukeboxManager?.CleanUp());
    }
}
