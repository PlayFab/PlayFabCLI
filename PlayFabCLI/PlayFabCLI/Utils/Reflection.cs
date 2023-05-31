using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayFabCLI.Utils
{

    /// <summary>
    /// Reflection methods specific for this application
    /// </summary>
    public static class Reflection
    {

        /// <summary>
        /// Extract all concrete implementations of a specific type/interface from this assembly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetConcreteImplementationsOf<T>(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            IOrderedEnumerable<Type> commandTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(T)))
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName);
            return commandTypes;
        }
    }
}
