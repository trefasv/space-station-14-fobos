// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.SignatureOnPaper.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Verbs;
using Content.Shared.Hands.Components;
using Robust.Shared.Utility;
using Content.Shared.Database;
using Content.Shared.Paper;
using Content.Shared.Examine;

namespace Content.Server.DeadSpace.SignatureOnPaper;

public sealed partial class SignaturePaperSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignaturePaperComponent, GetVerbsEvent<Verb>>(DoSetVerbs);
        SubscribeLocalEvent<SignaturePaperComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, SignaturePaperComponent component, ExaminedEvent args)
    {
        if (component.NumberSignatures > 0)
            args.PushMarkup(Loc.GetString("Документ подписан."));
    }

    private void DoSetVerbs(EntityUid uid, SignaturePaperComponent component, GetVerbsEvent<Verb> args)
    {
        if (!TryComp<PaperComponent>(uid, out var paperComp))
            return;

        if (!TryComp<HandsComponent>(args.User, out var handsComp))
            return;

        var item = handsComp.ActiveHandEntity;

        if (item == null)
            return;

        if (!EntityManager.TryGetComponent(args.User, typeof(MetaDataComponent), out var compObj))
            return;

        var name = ((MetaDataComponent)compObj).EntityName;

        if (paperComp.Signatures.Contains(name))
            return;

        if (TryComp<SignatureToolComponent>(item.Value, out var signatureToolComp) && component.NumberSignatures < component.MaximumSignatures)
        {
            args.Verbs.Add(new Verb()
            {
                Text = Loc.GetString("Расписаться"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/_DeadSpace/Interface/VerbIcons/pen.svg.192dpi.png")),
                Act = () => PushSignature((uid, paperComp), component, signatureToolComp, name),
                Impact = LogImpact.Low
            });
        }
    }

    private void PushSignature(Entity<PaperComponent> entity, SignaturePaperComponent component, SignatureToolComponent toolComponent, string name)
    {
        if (entity.Comp.Signatures.Contains(name) && component.NumberSignatures > component.MaximumSignatures)
            return;

        entity.Comp.Signatures.Add(name);
        component.NumberSignatures += 1;
        Dirty(entity);

        if (toolComponent.Sound != null)
            _audio.PlayPvs(toolComponent.Sound, entity);

        if (TryComp<AppearanceComponent>(entity, out var appearance))
            _appearance.SetData(entity, PaperComponent.PaperVisuals.Status, PaperComponent.PaperStatus.Written, appearance);
    }
}
