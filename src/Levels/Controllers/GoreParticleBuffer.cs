using System.Collections.Generic;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GoreParticleBuffer : Node2D
{
	[Export]
	private Texture2D _particleSprite = null!;

	[Export]
	private ShaderMaterial _goreShaderMaterial = null!;

	[ExportGroup("Internal")]
	[Export]
	public ulong RenderBudgetUs = 500;

	private static readonly StringName _gameTimeParam = "game_time";

	// Packed byte ranges, mirrored by unpack_w() in gore.gdshader.
	private const float PACKED_SCALE_MIN = 0.1f;
	private const float PACKED_SCALE_RANGE = 3.9f;
	private const float PACKED_SETTLE_MIN = 0.02f;
	private const float PACKED_SETTLE_RANGE = 1.73f;

	private MultiMeshInstance2D _mmiNode = null!;

	private GoreBurstParams _deathNormal = null!;
	private GoreBurstParams _deathExplosion = null!;
	private GoreBurstParams _spurt = null!;

	private CircularBuffer<GoreParticle> _particles = null!;
	private readonly Queue<GoreParticle> _burstQueue = [];
	private MultiMesh _multiMesh = null!;

	private float _gameTime;

	// burst means a collection of particles. a particle is an individual sprite/droplet
	private int _maxBurstCount;

	private int ParticleCount => _particles.Size;

	public void Initialize(
		int maxBurstCount,
		GoreBurstParams? deathNormal,
		GoreBurstParams? deathExplosion,
		GoreBurstParams? spurt
	)
	{
		_deathNormal = deathNormal ?? new GoreBurstParams();
		_deathExplosion = deathExplosion ?? new GoreBurstParams { SpeedMin = 10f, SpeedMax = 1200f };
		_spurt = spurt ?? new GoreBurstParams();

		_maxBurstCount = Math.Max(Math.Max(_deathNormal.Count, _deathExplosion.Count), _spurt.Count);

		_particles = new CircularBuffer<GoreParticle>(Mathf.Max(1, ToParticleCapacity(maxBurstCount)));
		_multiMesh = CreateMultiMesh(_particles.Capacity);
		_mmiNode = new MultiMeshInstance2D
		{
			Name = "GoreParticleMultiMeshInstance2D",
			Multimesh = _multiMesh,
			Texture = _particleSprite,
			Material = _goreShaderMaterial,
		};
		AddChild(_mmiNode);
	}

	public override void _PhysicsProcess(double delta)
	{
		_gameTime += (float)delta;
		_goreShaderMaterial.SetShaderParameter(_gameTimeParam, _gameTime);

		var start = Time.GetTicksUsec();
		while (_burstQueue.TryDequeue(out var particle))
		{
			if (Time.GetTicksUsec() - start > RenderBudgetUs)
				break;
			Write(in particle);
		}
	}

	public void SpawnDeathBurst(Vector2 position, DeathCauseEnum cause)
	{
		SpawnBurst(position, null, cause == DeathCauseEnum.Normal ? _deathNormal : _deathExplosion);
	}

	public void SpawnSpurtBurst(Vector2 position, float directionRadians)
	{
		SpawnBurst(position, directionRadians, _spurt);
	}

	private void SpawnBurst(Vector2 position, float? baseAngle, GoreBurstParams p)
	{
		var spreadRadians = Mathf.DegToRad(p.SpreadDegrees);
		for (var i = 0; i < p.Count; i++)
		{
			var angle = baseAngle is { } direction
				? direction + (float)GD.RandRange(-spreadRadians, spreadRadians)
				: (float)GD.RandRange(0f, Mathf.Tau);
			var velocity = Vector2.FromAngle(angle) * (float)GD.RandRange(p.SpeedMin, p.SpeedMax);
			var origin = position + p.EmitOffset + RandomPointInCircle(p.EmitRadius);
			var particles = new GoreParticle(
				origin,
				velocity,
				_gameTime,
				(float)GD.RandRange(p.ScaleMin, p.ScaleMax),
				(float)GD.RandRange(0f, Mathf.Tau),
				(float)GD.RandRange(p.SettleMin, p.SettleMax),
				RandomTint(p)
			);
			if (i < 50)
				Write(in particles);
			else
				_burstQueue.Enqueue(particles);
		}
	}

	public void SetCapacity(int targetCapacity)
	{
		var newCapacity = ToParticleCapacity(targetCapacity);

		if (newCapacity == _particles.Capacity)
			return;

		if (newCapacity <= 0)
		{
			_particles = new CircularBuffer<GoreParticle>(newCapacity);
			_multiMesh.InstanceCount = 0;
			return;
		}

		CircularBuffer<GoreParticle> newParticles;
		if (newCapacity < _particles.Capacity)
		{
			// this is so particles are kept when decreasing the max gore particles.
			// its also slow as balls when compared to passing in a slice of an array
			newParticles = new CircularBuffer<GoreParticle>(newCapacity);
			for (var i = 0; i < newCapacity; i++)
			{
				if (_particles.IsEmpty)
					break;
				newParticles.PushBack(_particles.Back());
				_particles.PopBack();
			}
		}
		else
			newParticles = new CircularBuffer<GoreParticle>(newCapacity, [.. _particles]);

		_particles = newParticles;

		// Reallocates the buffer, so everything must be re-uploaded.
		_multiMesh.InstanceCount = newCapacity;
		for (var i = 0; i < _particles.Size; i++)
		{
			var goreParticle = _particles[i];
			Upload(i, in goreParticle);
		}

		_multiMesh.VisibleInstanceCount = _particles.Size;
	}

	private void Write(in GoreParticle particle)
	{
		_particles.PushBack(particle);

		Upload(_particles.End, in particle);
		_multiMesh.VisibleInstanceCount = _particles.Size;
	}

	private void Upload(int idx, in GoreParticle particle)
	{
		_multiMesh.SetInstanceTransform2D(idx, new Transform2D(0f, particle.Origin));
		_multiMesh.SetInstanceCustomData(idx, PackCustomData(in particle));
		_multiMesh.SetInstanceColor(idx, particle.Tint);
	}

	// Packs scale/rotation/settle time as bytes in the w channel,
	// mirrored by unpack_w() in gore.gdshader.
	private static Color PackCustomData(in GoreParticle particle)
	{
		var scale = PackByte(particle.Scale, PACKED_SCALE_MIN, PACKED_SCALE_RANGE);
		var rotation = PackByte(particle.Rotation, 0f, Mathf.Tau);
		var settle = PackByte(particle.SettleTime, PACKED_SETTLE_MIN, PACKED_SETTLE_RANGE);

		var bits = scale | (rotation << 8) | (settle << 16);
		var packed = BitConverter.UInt32BitsToSingle(bits);
		return new Color(particle.Velocity.X, particle.Velocity.Y, particle.SpawnTime, packed);
	}

	private static uint PackByte(float value, float min, float range)
	{
		return (uint)Math.Clamp((value - min) / range * 255f, 0f, 255f);
	}

	private MultiMesh CreateMultiMesh(int capacity)
	{
		// Quad sized to the 3x2 small_chevron texture.
		return new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			UseCustomData = true,
			Mesh = new QuadMesh { Size = _particleSprite.GetSize() },
			InstanceCount = capacity,
			VisibleInstanceCount = 0,
		};
	}

	private int ToParticleCapacity(int maxBursts)
	{
		return Math.Max(1, maxBursts * Math.Max(1, _maxBurstCount));
	}

	private static Vector2 RandomPointInCircle(float radius)
	{
		var r = radius * Mathf.Sqrt((float)GD.RandRange(0f, 1f));
		return Vector2.FromAngle((float)GD.RandRange(0f, Mathf.Tau)) * r;
	}

	private static Color RandomTint(GoreBurstParams p)
	{
		var hue = Mathf.Wrap(p.BaseTint.H + (float)GD.RandRange(p.HueVariationMin, p.HueVariationMax), 0f, 1f);
		return Color.FromHsv(hue, p.BaseTint.S, p.BaseTint.V, p.BaseTint.A);
	}

	private readonly struct GoreParticle(
		Vector2 origin,
		Vector2 velocity,
		float spawnTime,
		float scale,
		float rotation,
		float settleTime,
		Color tint
	)
	{
		public readonly Vector2 Origin = origin;
		public readonly Vector2 Velocity = velocity;
		public readonly float SpawnTime = spawnTime;
		public readonly float Scale = scale;
		public readonly float Rotation = rotation;
		public readonly float SettleTime = settleTime;
		public readonly Color Tint = tint;
	}
}
