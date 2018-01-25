namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BF = System.Reflection.BindingFlags;

    sealed class DispatcherAccessor<TState> {
        readonly static IDictionary<Type, Func<IPOCOMachine<TState>, IDispatcher<TState>>> cache = new Dictionary<Type, Func<IPOCOMachine<TState>, IDispatcher<TState>>>();
        public static Func<IPOCOMachine<TState>, IDispatcher<TState>> Get(Type machineType) {
            Func<IPOCOMachine<TState>, IDispatcher<TState>> accessor;
            if(!cache.TryGetValue(machineType, out accessor)) {
                accessor = Make(machineType);
                cache.Add(machineType, accessor);
            }
            return accessor;
        }
        static Func<IPOCOMachine<TState>, IDispatcher<TState>> Make(Type machineType) {
            var field = GetField(machineType.BaseType) ?? GetField(machineType);
            var pMachine = Expression.Parameter(typeof(IPOCOMachine<TState>), "machine");
            return Expression.Lambda<Func<IPOCOMachine<TState>, IDispatcher<TState>>>(
                            Expression.MakeMemberAccess(Expression.Convert(pMachine, field.DeclaringType), field)
                   , pMachine).Compile();
        }
        static FieldInfo GetField(Type machineType) {
            if(machineType == null)
                return null;
            var fields = machineType.GetFields(BF.Public | BF.NonPublic | BF.Instance);
            return fields
                    .Where(f => f.FieldType == typeof(IDispatcher<TState>))
                    .FirstOrDefault();
        }
    }
}