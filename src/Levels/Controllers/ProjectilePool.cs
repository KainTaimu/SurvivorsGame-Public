// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
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
		Callable.From(() => GameWorld.Instance.AddChild(projectile)).CallDeferred();
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
