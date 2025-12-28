// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// BatchProcessor intentionally catches all exceptions to report them in results
#pragma warning disable CA1031 // Do not catch general exception types

namespace TagLibSharp2.Core;

/// <summary>
/// Provides batch processing capabilities for media file tag operations.
/// </summary>
/// <remarks>
/// <para>
/// This class enables efficient processing of multiple files with support for:
/// </para>
/// <list type="bullet">
/// <item>Parallel processing with configurable concurrency</item>
/// <item>Custom transformations applied to tags</item>
/// <item>Progress reporting during batch operations</item>
/// <item>Error handling with detailed results</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Process multiple FLAC files
/// var files = Directory.GetFiles(musicDir, "*.flac");
/// var results = await BatchProcessor.ProcessAsync(
///     files,
///     path => FlacFile.ReadFromFileAsync(path),
///     (file, path) => {
///         file.Artist = "Corrected Artist";
///         return file.SaveToFileAsync(path);
///     });
/// </code>
/// </example>
public static class BatchProcessor
{
	/// <summary>
	/// The default maximum degree of parallelism for batch operations.
	/// </summary>
	public static int DefaultMaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

	/// <summary>
	/// Processes multiple files asynchronously with the specified operation.
	/// </summary>
	/// <typeparam name="T">The type of the processed result for each file.</typeparam>
	/// <param name="paths">The file paths to process.</param>
	/// <param name="operation">The operation to perform on each file.</param>
	/// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. Defaults to processor count.</param>
	/// <param name="progress">Optional progress reporter.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A collection of results for each file.</returns>
	public static async Task<IReadOnlyList<BatchResult<T>>> ProcessAsync<T> (
		IEnumerable<string> paths,
		Func<string, CancellationToken, Task<T>> operation,
		int? maxDegreeOfParallelism = null,
		IProgress<BatchProgress>? progress = null,
		CancellationToken cancellationToken = default)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (paths is null)
			throw new ArgumentNullException (nameof (paths));
		if (operation is null)
			throw new ArgumentNullException (nameof (operation));
#else
		ArgumentNullException.ThrowIfNull (paths);
		ArgumentNullException.ThrowIfNull (operation);
#endif

		var pathList = paths.ToList ();
		var results = new BatchResult<T>[pathList.Count];
		var completed = 0;
		var parallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism;

		using var semaphore = new SemaphoreSlim (parallelism, parallelism);
		var tasks = new Task[pathList.Count];

		for (var i = 0; i < pathList.Count; i++) {
			var index = i;
			var path = pathList[i];

			tasks[i] = Task.Run (async () => {
				await semaphore.WaitAsync (cancellationToken).ConfigureAwait (false);
				try {
					var result = await operation (path, cancellationToken).ConfigureAwait (false);
					results[index] = BatchResult<T>.Success (path, result);
				} catch (OperationCanceledException) {
					results[index] = BatchResult<T>.Cancelled (path);
					throw;
				} catch (Exception ex) {
					results[index] = BatchResult<T>.Failure (path, ex);
				} finally {
					semaphore.Release ();
					var count = Interlocked.Increment (ref completed);
					progress?.Report (new BatchProgress (count, pathList.Count, path));
				}
			}, cancellationToken);
		}

		try {
			await Task.WhenAll (tasks).ConfigureAwait (false);
		} catch (OperationCanceledException) {
			// Some tasks may have been cancelled, results will reflect this
		}

		return results;
	}

	/// <summary>
	/// Processes multiple files synchronously with the specified operation.
	/// </summary>
	/// <typeparam name="T">The type of the processed result for each file.</typeparam>
	/// <param name="paths">The file paths to process.</param>
	/// <param name="operation">The operation to perform on each file.</param>
	/// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations. Defaults to processor count.</param>
	/// <param name="progress">Optional progress reporter.</param>
	/// <returns>A collection of results for each file.</returns>
	public static IReadOnlyList<BatchResult<T>> Process<T> (
		IEnumerable<string> paths,
		Func<string, T> operation,
		int? maxDegreeOfParallelism = null,
		IProgress<BatchProgress>? progress = null)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (paths is null)
			throw new ArgumentNullException (nameof (paths));
		if (operation is null)
			throw new ArgumentNullException (nameof (operation));
#else
		ArgumentNullException.ThrowIfNull (paths);
		ArgumentNullException.ThrowIfNull (operation);
#endif

		var pathList = paths.ToList ();
		var results = new BatchResult<T>[pathList.Count];
		var completed = 0;
		var parallelism = maxDegreeOfParallelism ?? DefaultMaxDegreeOfParallelism;

		var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };

		Parallel.For (0, pathList.Count, options, i => {
			var path = pathList[i];
			try {
				var result = operation (path);
				results[i] = BatchResult<T>.Success (path, result);
			} catch (Exception ex) {
				results[i] = BatchResult<T>.Failure (path, ex);
			} finally {
				var count = Interlocked.Increment (ref completed);
				progress?.Report (new BatchProgress (count, pathList.Count, path));
			}
		});

		return results;
	}

	/// <summary>
	/// Applies a tag transformation to multiple files.
	/// </summary>
	/// <typeparam name="TFile">The media file type.</typeparam>
	/// <typeparam name="TTag">The tag type.</typeparam>
	/// <param name="paths">The file paths to process.</param>
	/// <param name="readFile">Function to read the file and extract its tag.</param>
	/// <param name="transform">The transformation to apply to each tag.</param>
	/// <param name="saveFile">Function to save the file after transformation.</param>
	/// <param name="maxDegreeOfParallelism">Maximum number of concurrent operations.</param>
	/// <param name="progress">Optional progress reporter.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A collection of results indicating success or failure for each file.</returns>
	public static async Task<IReadOnlyList<BatchResult<bool>>> TransformTagsAsync<TFile, TTag> (
		IEnumerable<string> paths,
		Func<string, CancellationToken, Task<(TFile file, TTag tag)?>> readFile,
		Action<TTag> transform,
		Func<TFile, string, CancellationToken, Task> saveFile,
		int? maxDegreeOfParallelism = null,
		IProgress<BatchProgress>? progress = null,
		CancellationToken cancellationToken = default)
		where TTag : Tag
	{
		return await ProcessAsync (paths, async (path, ct) => {
			var fileResult = await readFile (path, ct).ConfigureAwait (false);
			if (fileResult is null)
				throw new InvalidOperationException ($"Failed to read file: {path}");

			var (file, tag) = fileResult.Value;
			transform (tag);
			await saveFile (file, path, ct).ConfigureAwait (false);
			return true;
		}, maxDegreeOfParallelism, progress, cancellationToken).ConfigureAwait (false);
	}
}

/// <summary>
/// Represents the result of a batch operation on a single file.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
#pragma warning disable CA1000 // Do not declare static members on generic types - factory methods are appropriate here
public readonly struct BatchResult<T> : IEquatable<BatchResult<T>>
{
	/// <summary>
	/// Gets the file path that was processed.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Gets a value indicating whether the operation was successful.
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// Gets a value indicating whether the operation was cancelled.
	/// </summary>
	public bool IsCancelled { get; }

	/// <summary>
	/// Gets the result value if successful.
	/// </summary>
	public T? Value { get; }

	/// <summary>
	/// Gets the exception if the operation failed.
	/// </summary>
	public Exception? Error { get; }

	BatchResult (string path, bool isSuccess, bool isCancelled, T? value, Exception? error)
	{
		Path = path;
		IsSuccess = isSuccess;
		IsCancelled = isCancelled;
		Value = value;
		Error = error;
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static BatchResult<T> Success (string path, T value) =>
		new (path, true, false, value, null);

	/// <summary>
	/// Creates a failed result.
	/// </summary>
	public static BatchResult<T> Failure (string path, Exception error) =>
		new (path, false, false, default, error);

	/// <summary>
	/// Creates a cancelled result.
	/// </summary>
	public static BatchResult<T> Cancelled (string path) =>
		new (path, false, true, default, null);

	/// <inheritdoc/>
	public bool Equals (BatchResult<T> other) =>
		Path == other.Path && IsSuccess == other.IsSuccess && IsCancelled == other.IsCancelled;

	/// <inheritdoc/>
	public override bool Equals (object? obj) => obj is BatchResult<T> other && Equals (other);

	/// <inheritdoc/>
#if NETSTANDARD2_0
	public override int GetHashCode ()
	{
		unchecked {
			var hash = 17;
			hash = hash * 31 + (Path?.GetHashCode () ?? 0);
			hash = hash * 31 + IsSuccess.GetHashCode ();
			hash = hash * 31 + IsCancelled.GetHashCode ();
			return hash;
		}
	}
#else
	public override int GetHashCode () => HashCode.Combine (Path, IsSuccess, IsCancelled);
#endif

	/// <summary>Equality operator.</summary>
	public static bool operator == (BatchResult<T> left, BatchResult<T> right) => left.Equals (right);

	/// <summary>Inequality operator.</summary>
	public static bool operator != (BatchResult<T> left, BatchResult<T> right) => !left.Equals (right);
}
#pragma warning restore CA1000

/// <summary>
/// Represents progress information during a batch operation.
/// </summary>
public readonly struct BatchProgress : IEquatable<BatchProgress>
{
	/// <summary>
	/// Gets the number of files processed so far.
	/// </summary>
	public int Completed { get; }

	/// <summary>
	/// Gets the total number of files to process.
	/// </summary>
	public int Total { get; }

	/// <summary>
	/// Gets the path of the most recently processed file.
	/// </summary>
	public string CurrentPath { get; }

	/// <summary>
	/// Gets the percentage of completion (0-100).
	/// </summary>
	public double PercentComplete => Total > 0 ? (double)Completed / Total * 100 : 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="BatchProgress"/> struct.
	/// </summary>
	public BatchProgress (int completed, int total, string currentPath)
	{
		Completed = completed;
		Total = total;
		CurrentPath = currentPath;
	}

	/// <inheritdoc/>
	public bool Equals (BatchProgress other) =>
		Completed == other.Completed && Total == other.Total && CurrentPath == other.CurrentPath;

	/// <inheritdoc/>
	public override bool Equals (object? obj) => obj is BatchProgress other && Equals (other);

	/// <inheritdoc/>
#if NETSTANDARD2_0
	public override int GetHashCode ()
	{
		unchecked {
			var hash = 17;
			hash = hash * 31 + Completed.GetHashCode ();
			hash = hash * 31 + Total.GetHashCode ();
			hash = hash * 31 + (CurrentPath?.GetHashCode () ?? 0);
			return hash;
		}
	}
#else
	public override int GetHashCode () => HashCode.Combine (Completed, Total, CurrentPath);
#endif

	/// <summary>Equality operator.</summary>
	public static bool operator == (BatchProgress left, BatchProgress right) => left.Equals (right);

	/// <summary>Inequality operator.</summary>
	public static bool operator != (BatchProgress left, BatchProgress right) => !left.Equals (right);
}

/// <summary>
/// Provides extension methods for batch result collections.
/// </summary>
public static class BatchResultExtensions
{
	/// <summary>
	/// Gets only the successful results from a batch operation.
	/// </summary>
	public static IEnumerable<BatchResult<T>> WhereSucceeded<T> (this IEnumerable<BatchResult<T>> results) =>
		results.Where (r => r.IsSuccess);

	/// <summary>
	/// Gets only the failed results from a batch operation.
	/// </summary>
	public static IEnumerable<BatchResult<T>> WhereFailed<T> (this IEnumerable<BatchResult<T>> results) =>
		results.Where (r => !r.IsSuccess && !r.IsCancelled);

	/// <summary>
	/// Gets the count of successful operations.
	/// </summary>
	public static int SuccessCount<T> (this IEnumerable<BatchResult<T>> results) =>
		results.Count (r => r.IsSuccess);

	/// <summary>
	/// Gets the count of failed operations.
	/// </summary>
	public static int FailureCount<T> (this IEnumerable<BatchResult<T>> results) =>
		results.Count (r => !r.IsSuccess && !r.IsCancelled);
}
