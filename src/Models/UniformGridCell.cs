// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Game.Models;

public struct UniformGridCell<T>
	where T : struct
{
	public const int MAX_SIZE = 16;

	[InlineArray(MAX_SIZE)]
	public struct CellBuffer
	{
		private T _element0;
	}

	public CellBuffer Items;

	public int Count { get; private set; }

	// Drops obj silently by design
	public void Add(T obj)
	{
		if (Count >= MAX_SIZE)
			return;

		// Directly indexing into an InlineArray causes the compiler to turn this into Span via InlineArrayAsSpan for runtime bounds checking.
		// Very expensive.
		// Items[Count] = obj;

		// jump to Count index and set directly
		Unsafe.Add(ref Unsafe.As<CellBuffer, T>(ref Items), Count) = obj;
		Count++;
	}

	public void Clear()
	{
		Count = 0;
	}

	[UnscopedRef]
	public ReadOnlySpan<T> AsReadOnlySpan()
	{
		return ((ReadOnlySpan<T>)Items)[..Count];
	}

	public override readonly string ToString()
	{
		return $"{Count} objects";
	}
}
