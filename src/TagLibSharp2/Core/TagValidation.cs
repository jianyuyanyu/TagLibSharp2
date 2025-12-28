// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Represents the severity level of a validation issue.
/// </summary>
public enum ValidationSeverity
{
	/// <summary>
	/// Informational message, not an issue.
	/// </summary>
	Info,

	/// <summary>
	/// Warning that might affect some players or applications.
	/// </summary>
	Warning,

	/// <summary>
	/// Error that may cause problems with playback or metadata display.
	/// </summary>
	Error
}

/// <summary>
/// Represents a single validation issue found during tag validation.
/// </summary>
public sealed class ValidationIssue
{
	/// <summary>
	/// Gets the field or frame that has the issue.
	/// </summary>
	public string Field { get; }

	/// <summary>
	/// Gets the severity level of this issue.
	/// </summary>
	public ValidationSeverity Severity { get; }

	/// <summary>
	/// Gets a description of the issue.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Gets an optional suggested fix for the issue.
	/// </summary>
	public string? SuggestedFix { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationIssue"/> class.
	/// </summary>
	/// <param name="field">The field with the issue.</param>
	/// <param name="severity">The severity level.</param>
	/// <param name="message">The issue description.</param>
	/// <param name="suggestedFix">An optional suggested fix.</param>
	public ValidationIssue (string field, ValidationSeverity severity, string message, string? suggestedFix = null)
	{
		Field = field;
		Severity = severity;
		Message = message;
		SuggestedFix = suggestedFix;
	}

	/// <inheritdoc/>
	public override string ToString ()
	{
		var result = $"[{Severity}] {Field}: {Message}";
		if (SuggestedFix is not null)
			result += $" (Suggested: {SuggestedFix})";
		return result;
	}
}

/// <summary>
/// Represents the result of validating a tag.
/// </summary>
public sealed class ValidationResult
{
	readonly List<ValidationIssue> _issues;

	/// <summary>
	/// Gets the list of all validation issues found.
	/// </summary>
	public IReadOnlyList<ValidationIssue> AllIssues => _issues;

	/// <summary>
	/// Gets a value indicating whether the tag is valid (no errors or warnings).
	/// </summary>
	public bool IsValid => _issues.Count == 0;

	/// <summary>
	/// Gets a value indicating whether the tag has any errors.
	/// </summary>
	public bool HasErrors
	{
		get {
			for (var i = 0; i < _issues.Count; i++) {
				if (_issues[i].Severity == ValidationSeverity.Error)
					return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the tag has any warnings.
	/// </summary>
	public bool HasWarnings
	{
		get {
			for (var i = 0; i < _issues.Count; i++) {
				if (_issues[i].Severity == ValidationSeverity.Warning)
					return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets the number of errors.
	/// </summary>
	public int ErrorCount
	{
		get {
			var count = 0;
			for (var i = 0; i < _issues.Count; i++) {
				if (_issues[i].Severity == ValidationSeverity.Error)
					count++;
			}
			return count;
		}
	}

	/// <summary>
	/// Gets the number of warnings.
	/// </summary>
	public int WarningCount
	{
		get {
			var count = 0;
			for (var i = 0; i < _issues.Count; i++) {
				if (_issues[i].Severity == ValidationSeverity.Warning)
					count++;
			}
			return count;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationResult"/> class.
	/// </summary>
	public ValidationResult ()
	{
		_issues = new List<ValidationIssue> (4);
	}

	/// <summary>
	/// Adds an issue to the validation result.
	/// </summary>
	/// <param name="issue">The issue to add.</param>
	public void AddIssue (ValidationIssue issue)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (issue is null)
			throw new ArgumentNullException (nameof (issue));
#else
		ArgumentNullException.ThrowIfNull (issue);
#endif
		_issues.Add (issue);
	}

	/// <summary>
	/// Adds an error to the validation result.
	/// </summary>
	/// <param name="field">The field with the issue.</param>
	/// <param name="message">The error message.</param>
	/// <param name="suggestedFix">An optional suggested fix.</param>
	public void AddError (string field, string message, string? suggestedFix = null)
	{
		_issues.Add (new ValidationIssue (field, ValidationSeverity.Error, message, suggestedFix));
	}

	/// <summary>
	/// Adds a warning to the validation result.
	/// </summary>
	/// <param name="field">The field with the issue.</param>
	/// <param name="message">The warning message.</param>
	/// <param name="suggestedFix">An optional suggested fix.</param>
	public void AddWarning (string field, string message, string? suggestedFix = null)
	{
		_issues.Add (new ValidationIssue (field, ValidationSeverity.Warning, message, suggestedFix));
	}

	/// <summary>
	/// Adds an informational message to the validation result.
	/// </summary>
	/// <param name="field">The field with the issue.</param>
	/// <param name="message">The informational message.</param>
	public void AddInfo (string field, string message)
	{
		_issues.Add (new ValidationIssue (field, ValidationSeverity.Info, message));
	}

	/// <summary>
	/// Gets all issues with the specified severity.
	/// </summary>
	/// <param name="severity">The severity to filter by.</param>
	/// <returns>The matching issues.</returns>
	public IEnumerable<ValidationIssue> GetIssues (ValidationSeverity severity)
	{
		for (var i = 0; i < _issues.Count; i++) {
			if (_issues[i].Severity == severity)
				yield return _issues[i];
		}
	}
}
