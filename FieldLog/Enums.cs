﻿// FieldLog – .NET logging solution
// © Yves Goergen, Made in Germany
// Website: http://unclassified.software/source/fieldlog
//
// This library is free software: you can redistribute it and/or modify it under the terms of
// the GNU Lesser General Public License as published by the Free Software Foundation, version 3.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this
// library. If not, see <http://www.gnu.org/licenses/>.

namespace Unclassified.FieldLog
{
	/// <summary>
	/// Defines priority values for log items. Higher priority values indicate a more severe event.
	/// </summary>
	public enum FieldLogPriority
	{
		/// <summary>Detailed information for debugging purposes only. This is the default priority if unspecified.</summary>
		/// <example>User chose to load a file. / 5 files found to chose from. / Most other data items.</example>
		Trace,
		/// <summary>Information for debugging purposes only.</summary>
		/// <example>Scanning optional directories completed. / First part of the file was processed.</example>
		Checkpoint,
		/// <summary>Normal operation informational message.</summary>
		/// <example>The file was loaded normally.</example>
		Info,
		/// <summary>Noticeable informational message.</summary>
		/// <example>The file is in an old format but should have been long upgraded.</example>
		Notice,
		/// <summary>Warning condition.</summary>
		/// <example>Unknown entries in the file were ignored, but they might have an important meaning.</example>
		Warning,
		/// <summary>Error condition that can normally be handled.</summary>
		/// <example>File cannot be saved to disk, the user was informed and has alternative options.</example>
		Error,
		/// <summary>Critical error condition that prevents the application or an essential part of it from working (properly). This is the default priority for exceptions if unspecified.</summary>
		/// <example>Out of memory or missing assembly file, the algorithm cannot complete.</example>
		Critical
	}

	/// <summary>
	/// Defines scope item types.
	/// </summary>
	public enum FieldLogScopeType
	{
		/// <summary>A function or other scope was entered.</summary>
		Enter,
		/// <summary>A function or other scope was left.</summary>
		Leave,
		/// <summary>A thread has started. This occurs only once in a thread's lifetime.</summary>
		ThreadStart,
		/// <summary>A thread has ended. This occurs only once in a thread's lifetime.</summary>
		ThreadEnd,
		/// <summary>The logging has started. This occurs only once in an application's lifetime.</summary>
		LogStart,
		/// <summary>The logging was shut down. This occurs only once in an application's lifetime.</summary>
		LogShutdown,
		/// <summary>A web request has started.</summary>
		WebRequestStart,
		/// <summary>A web request has ended.</summary>
		WebRequestEnd
	}

	/// <summary>
	/// Defines item types in the log file. Can only use 4 bits, i. e. 16 values.
	/// </summary>
	public enum FieldLogItemType
	{
		/// <summary>Invalid value.</summary>
		None,
		/// <summary>The file record contains a shared string value.</summary>
		StringData,
		/// <summary>The file record contains a FieldLogTextItem.</summary>
		Text,
		/// <summary>The file record contains a FieldLogDataItem.</summary>
		Data,
		/// <summary>The file record contains a FieldLogExceptionItem.</summary>
		Exception,
		/// <summary>The file record contains a FieldLogScopeItem.</summary>
		Scope,
		/// <summary>The file record contains a repeated FieldLogScopeItem.</summary>
		RepeatedScope
	}
}
