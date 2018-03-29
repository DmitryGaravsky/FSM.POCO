namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    sealed class TypesActivator {
        readonly IDictionary<Type, Delegate> createCache = new Dictionary<Type, Delegate>();
        public T Create<T>(Func<Type, Type> createType) {
            Delegate create = null;
            if(!createCache.TryGetValue(typeof(T), out create)) {
                Type type = CreateType(typeof(T), createType);
                create = Expression.Lambda(ExpressionHelper.New(type)).Compile();
                createCache.Add(typeof(T), create);
            }
            return ((Func<T>)create)();
        }
        readonly IDictionary<Type, Delegate> initCache = new Dictionary<Type, Delegate>();
        public T Create<T>(Func<Type, Type> createType, LambdaExpression initExpression) {
            Delegate init = null;
            if(!initCache.TryGetValue(typeof(T), out init)) {
                Type type = CreateType(typeof(T), createType);
                init = Expression.Lambda(ExpressionHelper.Init(type, initExpression)).Compile();
                initCache.Add(typeof(T), init);
            }
            return ((Func<T>)init)();
        }
        readonly IDictionary<Type, Type> typesCache = new Dictionary<Type, Type>();
        Type CreateType(Type sourceType, Func<Type, Type> createType) {
            Type result;
            if(!typesCache.TryGetValue(sourceType, out result)) {
                result = createType(sourceType);
                typesCache.Add(sourceType, result);
            }
            return result;
        }
    }
}