// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using System.Text;
using Game.Utils;

namespace Game.Players.Controllers;

public partial class DebugClassInspector : CanvasLayer
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
	} = true;

	[Export]
	public BindingFlags BindingFlags = BindingFlags.Instance;

	[Export]
	public int RecursionDepth;

	[Export]
	private PanelContainer _panel = null!;

	[Export]
	private Label _label = null!;

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		var target = GetParentOrNull<Node>();
		if (target is null)
		{
			Hide();
			return;
		}

		if (!Visible)
			Show();

		var s = new StringBuilder();
		s.AppendLine(ClassInspector.GetClassFieldsString(target, BindingFlags, RecursionDepth));
		s.AppendLine(ClassInspector.GetClassPropertiesString(target, BindingFlags, RecursionDepth));
		_label.Text = s.ToString();
	}
}
