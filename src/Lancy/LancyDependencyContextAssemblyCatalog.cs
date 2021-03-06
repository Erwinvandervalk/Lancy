namespace Lancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyModel;
    using Nancy;

    /// <summary>
    /// Default implementation of the <see cref="IAssemblyCatalog"/> interface, based on
    /// retrieving <see cref="Assembly"/> information from <see cref="DependencyContext"/>.
    /// </summary>
    public class LancyDependencyContextAssemblyCatalog : IAssemblyCatalog
    {
        private static readonly Assembly NancyAssembly = typeof(INancyEngine).GetTypeInfo().Assembly;
        private readonly DependencyContext dependencyContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LancyDependencyContextAssemblyCatalog"/> class,
        /// using <see cref="Assembly.GetEntryAssembly()"/>.
        /// </summary>
        public LancyDependencyContextAssemblyCatalog()
            : this(Assembly.GetEntryAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LancyDependencyContextAssemblyCatalog"/> class,
        /// using <paramref name="entryAssembly"/>.
        /// </summary>
        public LancyDependencyContextAssemblyCatalog(Assembly entryAssembly)
        {
            this.dependencyContext = DependencyContext.Load(entryAssembly);
        }

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            var results = new HashSet<Assembly>
            {
                typeof (DependencyContextAssemblyCatalog).GetTypeInfo().Assembly
            };

            foreach (var library in this.dependencyContext.RuntimeLibraries)
            {
                if (IsReferencingNancy(library))
                {
                    foreach (var assemblyName in library.GetDefaultAssemblyNames(this.dependencyContext))
                    {
                        results.Add(SafeLoadAssembly(assemblyName));
                    }
                }
            }

            return results.ToArray();
        }

        private static Assembly SafeLoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsReferencingNancy(Library library)
        {
            return library.Dependencies.Any(dependency => dependency.Name.Equals(NancyAssembly.GetName().Name));
        }
    }
}