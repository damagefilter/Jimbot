using System;
using System.Linq;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;

namespace Jimbot.Di {
    public class DiContainer : IDisposable {
        private IKernel resolver;

        public DiContainer(IKernel kernel) {
            resolver = kernel;
        }

        public T Get<T>() {
            return this.resolver.Get<T>();
        }

        /// <summary>
        /// Add a new Binding to the dependency containerBuilder.
        /// Binds a service type to a target type.
        /// </summary>
        /// <param name="service">The interface</param>
        /// <param name="target">The concrete implementation </param>
        /// <param name="singleInstance"></param>
        public void AddBinding(Type service, Type target, bool singleInstance) {
            var registration = resolver.Bind(service).To(target);
            if (singleInstance) {
                registration.InSingletonScope();
            }
        }

        /// <summary>
        /// Simple binding without interface or service mumbojumbo
        /// </summary>
        /// <param name="target"></param>
        /// <param name="singleInstance"></param>
        public void AddBinding(Type target, bool singleInstance) {
            var registration = resolver.Bind(target).ToSelf();
            if (singleInstance) {
                registration.InSingletonScope();
            }
        }

        /// <summary>
        /// Bind a target type to a factory method.
        /// This exposes Ninject API.
        ///
        /// </summary>
        /// <param name="target"></param>
        /// <param name="factory"></param>
        /// <param name="singleInstance"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void AddBindingToMethod<T>(Type target, Func<IContext, T> factory, bool singleInstance) {
            if (!typeof(T).IsAssignableFrom(target)) {
                throw new Exception($"Cannot bind to method. type mismatch. cannot assign {target} to {typeof(T)}");
            }
            var registration = resolver.Bind(target).ToMethod(factory);
            if (singleInstance) {
                registration.InSingletonScope();
            }
        }

        /// <summary>
        /// Get the injection module for advanced bindings.
        /// You are exposing your code to ninject API here. Beware.
        ///
        /// </summary>
        /// <returns></returns>
        public BotInjectionModule GetImplementation() {
            return (BotInjectionModule) resolver.GetModules().FirstOrDefault(x => x is BotInjectionModule);
        }

        public void Dispose() {
            resolver?.Dispose();
        }
    }
}