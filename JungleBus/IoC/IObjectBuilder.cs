using System;

namespace JungleBus.IoC
{
    /// <summary>
    /// Build objects for the bus
    /// </summary>
    public interface IObjectBuilder : IDisposable
    {
        /// <summary>
        /// Creates a child builder
        /// </summary>
        /// <returns>Nested builder</returns>
        IObjectBuilder GetNestedBuilder();

        /// <summary>
        /// Registers an instance of type T with the builder
        /// </summary>
        /// <typeparam name="T">Type to register</typeparam>
        /// <param name="value">Instance of T to register</param>
        void RegisterInstance<T>(T value) where T : class;

        /// <summary>
        /// Register a concrete type to a given base type
        /// </summary>
        /// <param name="baseType">Base type</param>
        /// <param name="concreteType">Concrete Type</param>
        void RegisterType(Type baseType, Type concreteType);

        /// <summary>
        /// Gets an instance of type T
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <returns>Instance of type T</returns>
        T GetValue<T>();

        /// <summary>
        /// Gets an instance of type type
        /// </summary>
        /// <param name="type">Type to get</param>
        /// <returns>Instance of type</returns>
        object GetValue(Type type);
    }
}
