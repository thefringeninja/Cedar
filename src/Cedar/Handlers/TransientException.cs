namespace Cedar.Handlers
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an exception that when thrown by a handler indicates the fault is transient and retryable.
    /// </summary>
    public class TransientException
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientException"/> class.
        /// </summary>
        public TransientException()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TransientException(string message)
            : base(message)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TransientException(string message, Exception innerException)
            : base(message, innerException)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public TransientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}