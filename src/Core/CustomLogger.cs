// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

public static class CustomLogger
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogInfo(params object[] s)
	{
		SendLog(GD.PrintRich, $"[color=white][Info  :  {GetCallerName()}] {ConstructString(s)}[/color]");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogDebug(params object[] s)
	{
		SendLog(GD.PrintRich, $"[color=darkgray][Debug  :  {GetCallerName()}] {ConstructString(s)}[/color]");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogWarning(params object[] s)
	{
		SendLog(GD.PrintRich, $"[color=yellow][Warning  :  {GetCallerName()}] {ConstructString(s)}[/color]");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogError(params object[] s)
	{
		GD.PrintErr($"[Error  :  {GetCallerName()}] {ConstructString(s)}");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SendLog(Action<string> method, string message)
	{
		if (OS.HasFeature("prod"))
			return;
		method(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string ConstructString(object[] s)
	{
		var builder = new StringBuilder();
		foreach (var x in s)
			builder.Append($"{x} ");
		return builder.ToString();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetCallerName()
	{
		var stackTrace = new StackTrace(1, false); //Captures 1 frame, false for not collecting information about the file
		var type = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;

		return type is null ? "" : type.Name;
	}
}
