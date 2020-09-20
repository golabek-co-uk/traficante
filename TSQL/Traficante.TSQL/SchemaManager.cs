using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Schema.Managers
{
    public class SchemaManager
    {


        private List<MethodGroupInfo> _methods = new List<MethodGroupInfo>();
        private List<TableInfo> _tables = new List<TableInfo>();
        private List<Func<string, string[], Type[], Delegate>> _methodsResolver = new List<Func<string, string[], Type[], Delegate>>();
        private List<Func<string, string[], Delegate>> _tablesResolver = new List<Func<string, string[], Delegate>>();

        public SchemaManager()
        {
        }

        public void RegisterLibraries(Library library)
        {
            var type = library.GetType();
            var methods = type.GetMethods().Where(f => f.GetCustomAttribute<BindableMethodAttribute>() != null);

            foreach (var methodInfo in methods)
                RegisterMethod(methodInfo);
        }

        public void RegisterTable(TableInfo tableInfo)
        {
            _tables.Add(tableInfo);
        }

        public TableInfo ResolveTable(string name, string[] path)
        {
            var table = _tables
                .Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase))
                .Where(x =>
                {
                    var pathOfX = x.Path.Reverse().ToList();
                    var pathToFind = path.Reverse().ToList();
                    for (int i = 0; i < pathToFind.Count; i++)
                    {
                        if (pathOfX.ElementAtOrDefault(i) != pathToFind.ElementAtOrDefault(i))
                            return false;
                    }
                    return true;
                }).FirstOrDefault();
            if (table != null)
                return table;

            foreach (var resolver in _tablesResolver)
            {
                var @delegate = resolver(name, path);
                if (@delegate != null)
                    return new TableInfo(name, path, new MethodInfo(@delegate));
            }

            return null;
        }

        public MethodInfo ResolveMethod(string name, Type[] args)
        {
            return ResolveMethod(name, new string[0], args);
        }

        public MethodInfo ResolveMethod(string name, string[] path, Type[] methodArgs)
        {
            var methods = MatchMethods(name, path);
            if (methods != null)
            {
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

                        if (TypeHelper.IsSafeTypePossibleToConvert(param, arg) || TypeHelper.IsUnsafeTypePossibleToConvert(param, arg) || param.IsGenericParameter || param.IsArray && param.GetElementType().IsGenericParameter)
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
            }

            foreach (var resolver in _methodsResolver)
            {
                var @delegate = resolver(name, path, methodArgs);
                if (@delegate != null)
                    return new MethodInfo(@delegate);
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

        public void RegisterMethodResolver(Func<string, string[], Type[], Delegate> resolver)
        {
            _methodsResolver.Add(resolver);
        }

        public void RegisterTableResolver(Func<string, string[], Delegate> resolver)
        {
            _tablesResolver.Add(resolver);
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
    }

    public class MethodGroupInfo
    {
        public string Name { get; set; }
        public string[] Path { get; set; }
        public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    }

    public class MethodInfo
    {
        public MethodInfo()
        {
        }

        public MethodInfo(Delegate @delegate)
        {
            FunctionDelegate = @delegate;
            FunctionMethod = @delegate.Method;
        }

        public string Name => FunctionMethod.Name;
        public System.Reflection.MethodInfo FunctionMethod { get; set; }
        public Delegate FunctionDelegate { get; set; }
    }

    public class TableInfo
    {
        public TableInfo(string name, string[] path, object result)
        {
            this.Name = name;
            this.Path = path;
            this.Result = result;
        }
        public TableInfo(string name, string[] path, MethodInfo methodInfo)
        {
            this.Name = name;
            this.Path = path;
            this.MethodInfo = methodInfo;
        }
        
        public string Name { get; set; }
        public string[] Path { get; set; }
        public object Result { get; set; }
        public MethodInfo MethodInfo { get; set; }

        public string FullName => $"{string.Join(".", this.Path)}.{this.Name}".ToLower();
    }
}