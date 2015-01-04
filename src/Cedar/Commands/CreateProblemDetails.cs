namespace Cedar.Commands
{
    using System;

    /// <summary>
    ///     A delegate to create a <see cref="HttpProblemDetails"/> from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>An <see cref="HttpProblemDetails"/>if it can be created, otherwise null.</returns>
    public delegate HttpProblemDetails CreateProblemDetails(Exception exception);
}