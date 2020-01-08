using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Lib.Attributes;
using Traficante.TSQL.Schema.Helpers;

namespace Traficante.TSQL.Schema.Managers
{
    public class MethodsManager
    {
        private static readonly Dictionary<Type, Type[]> TypeCompatibilityTable;
        private readonly List<MethodGroupInfo> _methods;

        static MethodsManager()
        {
            TypeCompatibilityTable = new Dictionary<Type, Type[]>
            {
                {typeof(bool), new[] {typeof(bool)}},
                {typeof(short), new[] {typeof(short), typeof(bool)}},
                {typeof(int), new[] {typeof(int), typeof(short), typeof(bool)}},
                {typeof(long), new[] {typeof(long), typeof(int), typeof(short), typeof(bool)}},
                {typeof(DateTimeOffset), new[] {typeof(DateTimeOffset)}},
                {typeof(DateTime), new[] {typeof(DateTime)}},
                {typeof(string), new[] {typeof(string)}},
                {typeof(decimal), new[] {typeof(decimal)}}
            };
        }

        public MethodsManager()
        {
            _methods = new List<MethodGroupInfo>();
        }

        public void RegisterLibraries(Library library)
        {
            var type = library.GetType();
            var methods = type.GetMethods().Where(f => f.GetCustomAttribute<BindableMethodAttribute>() != null);

            foreach (var methodInfo in methods)
                RegisterMethod(methodInfo);
        }

        public MethodInfo GetMethod(string name, Type[] args)
        {
            return GetMethod(name, new string[0], args);
        }

        public MethodInfo GetMethod(string name, string[] path, Type[] methodArgs)
        {
            var methods = MatchMethods(name, path);
            if (methods == null)
                return null;


            for (int i = 0, j = methods.Methods.Count; i < j; ++i)
            {
                var methodInfo = methods.Methods[i].FunctionMethod;
                var parameters = methodInfo.GetParameters();
                var optionalParametersCount = parameters.CountOptionalParameters();
                var allParameters = parameters.Length;
                var notAnnotatedParametersCount = parameters.Count();//parameters.CountWithoutParametersAnnotatedBy<InjectTypeAttribute>();
                var paramsParameter = parameters.GetParametersWithAttribute<ParamArrayAttribute>();
                var parametersToInject = allParameters - notAnnotatedParametersCount;

                //Wrong amount of argument's. That's not our function.
                if (!paramsParameter.HasParameters() &&
                    (HasMoreArgumentsThanMethodDefinitionContains(methodArgs, notAnnotatedParametersCount) ||
                    !CanUseSomeArgumentsAsDefaultParameters(methodArgs, notAnnotatedParametersCount, optionalParametersCount)))
                    continue;

                var parametersToSkip = parametersToInject;

                var hasMatchedArgTypes = true;
                for (int f = 0, g = paramsParameter.HasParameters() ? Math.Min(methodArgs.Length - (parameters.Length - 1), parameters.Length) : methodArgs.Length; f < g; ++f)
                {
                    //1. When constant value, it won't be nullable<type> but type.
                    //So it is possible to call function with such value. 
                    //That's why GetUnderlyingNullable exists here.
                    var param = parameters[f + parametersToSkip].ParameterType.GetUnderlyingNullable();
                    var arg = methodArgs[f].GetUnderlyingNullable();

                    if (IsTypePossibleToConvert(param, arg) || param.IsGenericParameter || param.IsArray && param.GetElementType().IsGenericParameter)
                        continue;

                    hasMatchedArgTypes = false;
                    break;
                }

                if (paramsParameter.HasParameters())
                {
                    var paramsParameters = methodArgs.Skip(parameters.Length - 1);
                    var arrayType = paramsParameters.ElementAt(0).MakeArrayType();
                    var paramType = parameters[parameters.Length - 1].ParameterType;
                    hasMatchedArgTypes = paramType == arrayType || CanBeAssignedFromGeneric(paramType, arrayType);
                }

                if (!hasMatchedArgTypes)
                    continue;

                return methods.Methods[i];
            }
            return null;
        }

        private bool CanBeAssignedFromGeneric(Type paramType, Type arrayType)
        {
            return paramType.IsArray && paramType.GetElementType().IsGenericParameter && arrayType.IsArray;
        }

        private static bool CanUseSomeArgumentsAsDefaultParameters(IReadOnlyCollection<Type> methodArgs,
            int parametersCount, int optionalParametersCount)
        {
            return methodArgs.Count >= parametersCount - optionalParametersCount && methodArgs.Count <= parametersCount;
        }

        private static bool HasMoreArgumentsThanMethodDefinitionContains(IReadOnlyList<Type> methodArgs,
            int parametersCount)
        {
            return methodArgs.Count > parametersCount;
        }

        public void RegisterMethod(System.Reflection.MethodInfo methodInfo)
        {
            RegisterMethod(methodInfo.Name, methodInfo);
        }

        public void RegisterMethod(string name, System.Reflection.MethodInfo methodInfo)
        {
            MethodInfo method = new MethodInfo
            {
                FunctionMethod = methodInfo
            };
            RegisterMethod(name, new string[0], method);
        }

        public void RegisterMethod(string name, Delegate @delegate)
        {
            MethodInfo method = new MethodInfo
            {
                FunctionDelegate = @delegate,
                FunctionMethod = @delegate?.Method,
            };
            RegisterMethod(name, new string[0], method);
        }

        public void RegisterMethod(string name, string[] path, Delegate @delegate)
        {
            MethodInfo method = new MethodInfo
            {
                FunctionDelegate = @delegate,
                FunctionMethod = @delegate?.Method,
            };
            RegisterMethod(name, path, method);
        }

        public void RegisterMethod(string name, string[] path, MethodInfo methodInfo)
        {
            var existingMethod = MatchMethods(name, path, true);
            if (existingMethod != null)
            {
                existingMethod.Methods.Add(methodInfo);
            }
            else
            {
                MethodGroupInfo methodDef = new MethodGroupInfo();
                methodDef.Name = name;
                methodDef.Path = path ?? new string[0];
                methodDef.Methods.Add(methodInfo);
                _methods.Add(methodDef);
            }
        }

        public MethodGroupInfo MatchMethods(string name, string[] path, bool exact = false)
        {
            return _methods
                .Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase))
                .Where(x =>
                {
                    var pathOfX = x.Path.Reverse().ToList();
                    var pathToFind = path.Reverse().ToList();
                    if (exact && pathOfX.Count != pathToFind.Count)
                        return false;
                    for (int i = 0; i < pathToFind.Count; i++)
                    {
                        if (pathOfX.ElementAtOrDefault(i) != pathToFind.ElementAtOrDefault(i))
                            return false;
                    }
                    return true;
                }).FirstOrDefault();
        }


        public static bool IsTypePossibleToConvert(Type to, Type from)
        {
            if (TypeCompatibilityTable.ContainsKey(to))
                return TypeCompatibilityTable[to].Any(f => f == from);
            return to == from || to.IsAssignableFrom(from);
        }
    }

    public class MethodGroupInfo
    {
        public string Name { get; set; }
        public string[] Path { get; set; }
        public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    }

    public class MethodInfo
    {
        public string Name => FunctionMethod.Name;
        public System.Reflection.MethodInfo FunctionMethod { get; set; }
        public Delegate FunctionDelegate { get; set; }
    }

    public class TableInfo
    {
        public TableInfo(string name, string[] path, IEnumerable items, Type itemsType)
        {
            this.Name = name;
            this.Path = path;
            this.Items = items;
            this.ItemsType = itemsType;
        }
        public string Name { get; set; }
        public string[] Path { get; set; }
        public IEnumerable Items { get; set; }
        public Type ItemsType { get; set; }
    }
}