namespace Cedar.Handlers
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a mechansim to resolve handlers for given message types.
    /// </summary>
    public interface IHandlerResolver
    {
        /// <summary>
        /// Resolves all handlers for the specified message type.
        /// </summary>
        /// <typeparam name="T">The message type for which handlers will be resolved for.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> of handlers for the specified message type.</returns>
        IEnumerable<IHandler<T>> ResolveAll<T>() where T : class;

        /// <summary>
        /// Resolves a single handler for the specified message type.
        /// </summary>
        /// <typeparam name="T">The message type for which a single handler will be resolved for.</typeparam>
        /// <returns>A <see cref="IHandler{TMessage}"/> for the specified message type.</returns>
        IHandler<T> ResolveSingle<T>() where T : class;
    }
}