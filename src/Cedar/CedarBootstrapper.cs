namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Serialization;
    using Cedar.Exceptions;
    using Cedar.Hosting;
    using Nancy.TinyIoc;

    public abstract class CedarBootstrapper
    {
        /// <summary>
        ///     Gets the assemblies to scan.
        /// </summary>
        /// <value>
        ///     The assemblies to scan. The default value is the assembly the bootstrapper is defined in.
        /// </value>
        protected virtual IEnumerable<Assembly> AssembliesToScan
        {
            get { return new[] { typeof(CedarBootstrapper).Assembly, GetType().Assembly }; }
        }

        /// <summary>
        ///     Gets the command handler types.
        /// </summary>
        /// <value>
        ///     The command handler types.
        /// </value>
        /// <remarks>
        ///     All types returned must implement <see cref="ICommandHandler{T}"/>
        /// </remarks>
        public virtual IEnumerable<Type> CommandHandlerTypes
        {
            get { return AssembliesToScan.SelectMany(assembly => assembly.GetImplementorsOfOpenGenericInterface(typeof(ICommandHandler<>))); }
        }

        public virtual void ConfigureApplicationContainer(TinyIoCContainer container) { }

        public abstract string VendorName { get; } 

        public virtual IExceptionToModelConverter ExceptionToModelConverter
        {
            get
            {
                return new ExceptionToModelConverter();
            }
        }
    }
}