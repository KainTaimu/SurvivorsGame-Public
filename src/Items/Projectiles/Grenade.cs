using System.Collections.Generic;
using System.Linq;
using Game.Core.ECS;
using Game.Core.Settings;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public partial class Grenade : RigidBody2D
{
	[Export]
	private PackedScene _explosionScene = null!;

	public BaseOffensive OffensiveOrigin = null!;

	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	private EntityComponentStore ComponentStore => EntityComponentStore.Instance;

	private double _t;

	private float TimeToExplode => OffensiveOrigin.Stats.Additional.GetValueOrDefault("TimeToExplode").AsSingle();

	private float CameraRecoilScale =>
		OffensiveOrigin.Stats.Additional.GetValueOrDefault("CameraRecoilScale").AsSingle();

	public override void _Ready()
	{
		_t = TimeToExplode;
	}

	public override void _ExitTree()
	{
		var explosion = _explosionScene.Instantiate<GpuParticles2D>();
		explosion.Emitting = true;
		explosion.GlobalPosition = GlobalPosition;
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, explosion);
	}

	public override void _Process(double delta)
	{
		_t -= delta;

		if (Engine.GetProcessFrames() % 10 != 0)
			return;

		TargetQuery.TryGetTargetsInArea(Position, OffensiveOrigin.Stats.ProjectileRadius, out var ids);

		if (ids.Count() > 6 && _t < 0.1f)
		{
			foreach (var id in ids)
				OffensiveOrigin.HandleHit(id: id);
			ApplyCameraRecoil();
			QueueFree();
		}

		if (_t <= 0)
		{
			foreach (var id in ids)
				OffensiveOrigin.HandleHit(id: id);
			ApplyCameraRecoil();
			QueueFree();
		}
	}

	private void ApplyCameraRecoil()
	{
		if (!GameSettings.Instance.EnableCameraShake)
			return;
		if (CameraRecoilScale == 0)
			return;

		var camera = GetViewport().GetCamera2D();

		var origPos = camera.Position;
		var tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Spring);

		for (var i = 0; i < 6; i++)
		{
			static int Rand()
			{
				return GD.RandRange(-1, 1);
			}

			var shake = new Vector2(Rand(), Rand()) * GD.RandRange(4, 9) * CameraRecoilScale;

			tween.TweenProperty(camera, "offset", camera.Position + shake, 1 / 30f);
		}

		tween.TweenProperty(camera, "offset", origPos, 1 / 8f);
	}
}
