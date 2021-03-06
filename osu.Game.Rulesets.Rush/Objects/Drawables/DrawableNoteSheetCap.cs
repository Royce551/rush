// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Rush.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Rush.Objects.Drawables
{
    public class DrawableNoteSheetCap<TObject> : DrawableLanedHit<TObject>
        where TObject : LanedHit
    {
        private readonly DrawableNoteSheetCapStar capStar;
        protected readonly DrawableNoteSheet NoteSheet;

        [Resolved]
        private RushPlayfield playfield { get; set; }

        public override bool DisplayExplosion => true;

        public DrawableNoteSheetCap(DrawableNoteSheet noteSheet, TObject hitObject)
            : base(hitObject)
        {
            NoteSheet = noteSheet;
            Size = new Vector2(DrawableNoteSheet.NOTE_SHEET_SIZE * 1.1f);
            Origin = Anchor.Centre;

            Content.Child = capStar = new DrawableNoteSheetCapStar
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                AccentColour = { BindTarget = AccentColour },
            };
        }

        public override Drawable CreateHitExplosion() => new NoteSheetHitExplosion(this);

        protected override void UpdateInitialTransforms()
        {
            Scale = Vector2.One;
            Alpha = 1f;
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            if (state == ArmedState.Hit)
                Hide();
        }

        public bool TriggerResult() => UpdateResult(true);

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
        }

        public override bool OnPressed(RushAction action) => false; // Handled by the note sheet object itself.

        public override void OnReleased(RushAction action)
        {
        }

        private class NoteSheetHitExplosion : CompositeDrawable
        {
            private readonly DrawableNoteSheetCapStar explosionStar;
            private readonly Circle flashCircle;

            public NoteSheetHitExplosion(DrawableNoteSheetCap<TObject> drawableNoteSheet)
            {
                Anchor = drawableNoteSheet.LaneAnchor;
                Origin = Anchor.Centre;
                Size = drawableNoteSheet.Size;

                InternalChildren = new Drawable[]
                {
                    explosionStar = new DrawableNoteSheetCapStar
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        AccentColour = { Value = drawableNoteSheet.LaneAccentColour },
                    },
                    flashCircle = new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0.4f,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.5f),
                        Colour = drawableNoteSheet.LaneAccentColour.Lighten(0.5f)
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                explosionStar.ScaleTo(2f, RushPlayfield.HIT_EXPLOSION_DURATION)
                             .FadeOutFromOne(RushPlayfield.HIT_EXPLOSION_DURATION)
                             .Expire(true);

                flashCircle.ScaleTo(4f, RushPlayfield.HIT_EXPLOSION_DURATION / 2)
                           .Then()
                           .ScaleTo(0.5f, RushPlayfield.HIT_EXPLOSION_DURATION / 2)
                           .FadeOut(RushPlayfield.HIT_EXPLOSION_DURATION / 2)
                           .Expire(true);

                // TODO: very low priority for now, but this shouldn't stay as-is in every similar composite.
                this.Delay(InternalChildren.Max(d => d.LatestTransformEndTime)).Expire(true);
            }
        }
    }
}
