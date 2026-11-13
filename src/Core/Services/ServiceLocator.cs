// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace Game.Core.Services;

// TODO: Use Nodes for exported services. Resources suck to edit in-editor.
public partial class ServiceLocator : Node
{
	[Export]
	private Array<Service> _exportedServices = null!;

	private static readonly List<Service> _services = [];

	public static ServiceLocator Instance { get; private set; } = null!;

	public override void _Ready()
	{
		if (_services.Count > 0)
		{
			CustomLogger.LogError(
				"_services is not empty when it shouldn't. ServiceLocator should only ever have one instance per launch. Clearing previous entries"
			);
			_services.Clear();
		}

		foreach (var service in _exportedServices)
			_services.Add(service);

		Instance = this;
	}

	public static T? GetService<T>()
		where T : Service
	{
		var service = _services.OfType<T>().FirstOrDefault();
		if (service is null)
		{
			CustomLogger.LogError($"Service \"{typeof(T).Name}\" not found!");
			return null;
		}

		return service;
	}
}
