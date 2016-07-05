using System;
using JungleBus.Interfaces.IoC;
using StructureMap;

namespace JungleBus.IoC
{
    /// <summary>
    /// Implements the object builder interface backed by a StructureMap container
    /// </summary>
    public sealed class StructureMapObjectBuilder : IObjectBuilder
    {
        /// <summary>
        /// Instance of the container
        /// </summary>
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapObjectBuilder" /> class.
        /// </summary>
        public StructureMapObjectBuilder()
            : this(new Container())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapObjectBuilder" /> class.
        /// </summary>
        /// <param name="container">Configured instance of the container</param>
        public StructureMapObjectBuilder(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Gets an instance of type type
        /// </summary>
        /// <param name="type">Type to get</param>
        /// <returns>Instance of type</returns>
        public object GetValue(Type type)
        {
            return _container.TryGetInstance(type);
        }

        /// <summary>
        /// Gets an instance of type T
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <returns>Instance of type T</returns>
        public T GetValue<T>()
        {
            return (T)_container.TryGetInstance(typeof(T));
        }

        /// <summary>
        /// Registers an instance of type T with the builder
        /// </summary>
        /// <typeparam name="T">Type to register</typeparam>
        /// <param name="value">Instance of T to register</param>
        public void RegisterInstance<T>(T value)
            where T : class
        {
            _container.Inject<T>(value);
        }

        /// <summary>
        /// Register a concrete type to a given base type
        /// </summary>
        /// <param name="baseType">Base type</param>
        /// <param name="concreteType">Concrete Type</param>
        public void RegisterType(Type baseType, Type concreteType)
        {
            _container.Configure(x => x.For(baseType).Use(concreteType));
        }

        /// <summary>
        /// Creates a child builder
        /// </summary>
        /// <returns>Nested builder</returns>
        public IObjectBuilder GetNestedBuilder()
        {
            return new StructureMapObjectBuilder(_container.GetNestedContainer());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
