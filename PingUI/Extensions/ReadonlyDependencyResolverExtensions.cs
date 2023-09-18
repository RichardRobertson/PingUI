using System;
using PingUI.I18N;
using Splat;

namespace PingUI.Extensions;

/// <summary>
/// Extensions to <see cref="IReadonlyDependencyResolver" />.
/// </summary>
public static class ReadonlyDependencyResolverExtensions
{
	/// <summary>
	/// Gets a service from <see cref="Locator" /> or throws an exception.
	/// </summary>
	/// <typeparam name="T">The type for the object we want to retrieve.</typeparam>
	/// <param name="this">The <see cref="IReadonlyDependencyResolver" /> to perform a service lookup on.</param>
	/// <returns>The requested object.</returns>
	/// <exception cref="InvalidOperationException">Service of type <typeparamref name="T" /> was not found.</exception>
	public static T GetRequiredService<T>(this IReadonlyDependencyResolver @this)
	{
		return @this.GetService<T>()
			?? throw new InvalidOperationException(
				string.Format(Strings.ReadonlyDependencyResolverExtensions_MissingRequiredServiceFormat, typeof(T).FullName));
	}
}
