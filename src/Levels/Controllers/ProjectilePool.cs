using System.Collections.Generic;
using Arch.Core;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class ProjectilePool : Node
{
	[Signal]
	public delegate void OnNewProjectileCreatedEventHandler(BaseProjectile projectile);

	public PackedScene ProjectileScene
	{
		get;
		set
		{
			field = value;
			ResetPool();
		}
	} = null!;

	private readonly List<BaseProjectile> _activePool = [];
	private readonly Queue<BaseProjectile> _inactivePool = [];

	public override void _Ready()
	{
		Name = "ProjectilePool";
	}

	public void Initialize(Node owner, PackedScene projectileScene, Action<BaseProjectile> onProjectileCreated)
	{
		ProjectileScene = projectileScene;
		owner.AddChild(this);
		OnNewProjectileCreated += e => onProjectileCreated(e);
	}

	public void ReturnProjectile(BaseProjectile projectile)
	{
		_inactivePool.Enqueue(projectile);
		_activePool.Remove(projectile);
		projectile.ProcessMode = ProcessModeEnum.Disabled;
		projectile.Hide();
	}

	public BaseProjectile GetProjectile()
	{
		if (!_inactivePool.TryDequeue(out var projectile))
			projectile = NewProjectile();
		_activePool.Add(projectile);
		projectile.ProcessMode = ProcessModeEnum.Inherit;
		projectile.Show();
		return projectile;
	}

	private BaseProjectile NewProjectile()
	{
		var projectile = ProjectileScene.Instantiate<BaseProjectile>();
		if (projectile is not IPooledProjectile pooledProjectile)
		{
			projectile.QueueFree();
			throw new Exception($"{ProjectileScene.ResourceName} must be IPooledProjectile");
		}

		pooledProjectile.ProjectilePool = this;
		Callable.From(() => GetTree().Root.AddChild(projectile)).CallDeferred();
		projectile.Hide();

		projectile.ProcessMode = ProcessModeEnum.Disabled;
		projectile.Name = $"{projectile.GetType().Name}_{_activePool.Count}";
		EmitSignalOnNewProjectileCreated(projectile);
		return projectile;
	}

	private void ResetPool()
	{
		foreach (var projectile in _activePool)
			projectile.QueueFree();
		foreach (var projectile in _inactivePool)
			projectile.QueueFree();
		_activePool.Clear();
		_inactivePool.Clear();
	}

	public override string ToString()
	{
		return $"{_activePool.Count} active, {_inactivePool.Count} inactive";
	}
}
