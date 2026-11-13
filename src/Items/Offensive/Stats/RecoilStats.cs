// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class RecoilStats : ItemStat
{
	[Export]
	public float HorizontalRecoilMin { get; set; } = 1f;

	[Export]
	public float HorizontalBaseRecoil { get; set; } = 3f;

	[Export]
	public float HorizontalRecoilRandom { get; set; } = 1f;

	[Export]
	public float VerticalRecoilMin { get; set; } = 2f;

	[Export]
	public float VerticalBaseRecoil { get; set; } = 3f;

	[Export]
	public float VerticalRecoilRandom { get; set; } = 0.1f;

	[Export]
	public float RecoilScale { get; set; } = 1f;

	[Export]
	public float RecoilAccumilationScale { get; set; } = 1f;

	[Export]
	public bool HorizontalRecoilPunish { get; set; } = true;

	[Export]
	public float CameraRecoilScale { get; set; } = 1f;
}
