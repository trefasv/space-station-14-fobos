using Content.Shared._MK.Jukebox;

namespace Content.Client._MK.Jukebox;

public sealed class ClientJukeboxSongsSyncManager : JukeboxSongsSyncManager
{
    public override void OnSongUploaded(JukeboxSongUploadNetMessage message)
    {
        ContentRoot.AddOrUpdateFile(message.RelativePath!, message.Data);
    }
}
