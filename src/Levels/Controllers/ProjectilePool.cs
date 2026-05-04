using System.Collections.Generic;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class ProjectilePool : Node
{
	public int PoolCount;

	[Export]
	public PackedScene ProjectileScene = null!;

	private readonly List<BaseProjectile> _activePool = [];
	private readonly Queue<BaseProjectile> _inactivePool = [];

	public override void _Ready()
	{
		Name = "ProjectilePool";
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
		{
			projectile = NewProjectile();
		}
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
			throw new Exception(
				$"{ProjectileScene.ResourceName} must be IPooledProjectile"
			);
		}
		pooledProjectile.ProjectilePool = this;
		Callable.From(() => GetTree().Root.AddChild(projectile)).CallDeferred();
		projectile.Hide();

		projectile.ProcessMode = ProcessModeEnum.Disabled;
		return projectile;
	}
}
