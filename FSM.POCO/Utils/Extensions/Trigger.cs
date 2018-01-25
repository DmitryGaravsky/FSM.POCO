namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Selector = System.Linq.Expressions.Expression<System.Action<object[]>>;

    static class TriggerExtension {
        readonly static MethodInfo getTriggerMethodInfo = typeof(TriggerExtension).GetMethod("GetTrigger", new Type[] { typeof(Selector) });
        readonly static IDictionary<Type, Func<Selector, Enum>> getTriggerCache = new Dictionary<Type, Func<Selector, Enum>>();
        public static Enum GetTrigger(this Selector actionSelector, Type enumType) {
            Func<Selector, Enum> getTrigger;
            if(!getTriggerCache.TryGetValue(enumType, out getTrigger)) {
                MethodInfo mInfoGetTrigger = getTriggerMethodInfo.MakeGenericMethod(enumType);
                var pActionSelector = Expression.Parameter(typeof(Selector), "actionSelector");
                getTrigger = Expression.Lambda<Func<Selector, Enum>>(
                                Expression.Convert(Expression.Call(mInfoGetTrigger, pActionSelector), typeof(Enum))
                            , pActionSelector).Compile();
                getTriggerCache.Add(enumType, getTrigger);
            }
            return getTrigger(actionSelector);
        }
        readonly static IDictionary<MethodInfo, Enum> triggersCache = new Dictionary<MethodInfo, Enum>();
        public static TEnum GetTrigger<TEnum>(this Selector actionSelector)
            where TEnum : struct {
            Enum result;
            if(!triggersCache.TryGetValue(actionSelector.Method(), out result)) {
                var trigger = actionSelector.Attribute<TriggerAttribute>(false);
                string triggerName = (trigger != null) ? trigger.name : GetTriggerName(actionSelector.MethodName());
                if(string.IsNullOrEmpty(triggerName))
                    throw new NotSupportedException("Unable to resolve the trigger for the method " + actionSelector.MethodName());
                TEnum typedResult;
                if(!Enum.TryParse<TEnum>(triggerName, out typedResult))
                    throw new NotSupportedException("Unable to associate the method " + actionSelector.MethodName() + " with a specific trigger.");
                result = (Enum)(object)typedResult;
                triggersCache.Add(actionSelector.Method(), result);
            }
            return (TEnum)(object)result;
        }
        //
        const string Trigger_Prefix = "on";
        static string GetTriggerName(string methodName) {
            string name = methodName.Trim();
            var li = name.ToLowerInvariant();
            if(li != Trigger_Prefix && li.StartsWith(Trigger_Prefix))
                return name.Substring(2);
            return name;
        }
    }
}