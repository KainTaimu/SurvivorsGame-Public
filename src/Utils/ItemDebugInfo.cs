// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using System.Text;
using Game.Players.Controllers;
using Node = Godot.Node;

namespace Game.Utils;

public partial class ItemDebugInfo : CanvasLayer
{
	[Export]
	public bool Enabled
	{
		get;
		set
		{
			field = value;
			Visible = field;
		}
	}

	[Export]
	public Node? Target;

	[Export]
	public AbstractPlayerWeaponController? PlayerWeaponController;

	[Export]
	public Label Label = null!;

	[Export]
	public int RecursionDepth;

	public override void _Ready()
	{
		Visible = Enabled;
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		var target = Target ?? PlayerWeaponController?.PrimaryAttack;
		if (target is null)
		{
			Hide();
			return;
		}

		if (!Visible)
			Show();

		var s = new StringBuilder();
		s.AppendLine(
			ClassInspector.GetClassFieldsString(target, BindingFlags.NonPublic | BindingFlags.Instance, RecursionDepth)
		);
		s.AppendLine(
			ClassInspector.GetClassPropertiesString(
				target,
				BindingFlags.NonPublic | BindingFlags.Instance,
				RecursionDepth
			)
		);
		Label.Text = s.ToString();
	}
}
