using System.Linq;
using System.Reflection;
using Traficante.TSQL.Plugins;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Schema.Managers
{
    public class MethodsManager : MethodsMetadatas
    {
        public void RegisterLibraries(LibraryBase library)
        {
            var type = library.GetType();
            var methods = type.GetMethods().Where(f => f.GetCustomAttribute<BindableMethodAttribute>() != null);

            foreach (var methodInfo in methods)
                RegisterMethod(methodInfo);
        }
    }
}