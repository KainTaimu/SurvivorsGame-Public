// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class LogTimer
{
	public static IDisposable LogTimeMsec(string name)
	{
		var stopwatch = Stopwatch.StartNew();
		return new DelegateDisposable(() =>
		{
			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed.TotalMilliseconds;
			CustomLogger.LogDebug($"{name} took {elapsed:0.###} ms");
		});
	}

	public static IDisposable LogTimeMsec()
	{
		var name = GetCallerName();
		var stopwatch = Stopwatch.StartNew();
		return new DelegateDisposable(() =>
		{
			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed.TotalMilliseconds;
			CustomLogger.LogDebug($"{name} took {elapsed:0.###} ms");
		});
	}

	public static IDisposable LogTimeUsec(string name)
	{
		var stopwatch = Stopwatch.StartNew();
		return new DelegateDisposable(() =>
		{
			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed.TotalMicroseconds;
			CustomLogger.LogDebug($"{name} took {elapsed:0.###} us");
		});
	}

	public static IDisposable LogTimeUsec()
	{
		var name = GetCallerName();
		var stopwatch = Stopwatch.StartNew();
		return new DelegateDisposable(() =>
		{
			stopwatch.Stop();
			var elapsed = stopwatch.Elapsed.TotalMicroseconds;
			CustomLogger.LogDebug($"{name} took {elapsed:0.###} us");
		});
	}

	private sealed class DelegateDisposable(Action onDispose) : IDisposable
	{
		public void Dispose()
		{
			onDispose();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetCallerName()
	{
		var stackTrace = new StackTrace(1, false); //Captures 1 frame, false for not collecting information about the file
		var type = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;

		return type is null ? "" : type.Name;
	}
}
