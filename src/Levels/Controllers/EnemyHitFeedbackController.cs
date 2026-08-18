using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.VFX;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyHitFeedbackController : Node
{
	[Export]
	private GoreManager _goreManager = null!;

	[Export]
	private AudioStreamPlayer _hitmarkerStreamPlayer = null!;

	public override void _Process(double delta)
	{
		ProcessHitsQuery(GameWorld.World, (float)delta);
	}

	[Query]
	[All<HitFeedbackComponent, PositionComponent, AnimatedSpriteComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
	private void ProcessHits(
		[Data] in float delta,
		ref HitFeedbackComponent hit,
		ref PositionComponent pos,
		ref AnimatedSpriteComponent spr
	)
	{
		var newHitTime = hit.HitTimeLeft - delta;
		hit.HitTimeLeft = newHitTime;

		if (hit.HitTimeLeft <= 0)
			return;

		var flash = 128 * (newHitTime / hit.HitTime);
		spr.Flash = (byte)flash;

		if (hit.Damage <= 0)
			return;

		DamageIndicatorPool.Instance?.GetIndicator(pos.Position, hit.Damage, hit.IsCrit);

		var spurtDirection = GameWorld.Instance.MainPlayer.GlobalPosition.AngleToPoint(pos.Position);
		_goreManager?.SpawnHitSpurtPaticles(pos.Position, spurtDirection);

		hit.Damage = -1;
		_hitmarkerStreamPlayer?.Play();
	}
}
