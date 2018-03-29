namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    static class TriggerExtension {
        readonly static MethodInfo getTriggerMethodInfo = typeof(TriggerExtension)
                    .GetMethod("GetTrigger", new Type[] { typeof(LambdaExpression) });
        readonly static IDictionary<Type, Func<LambdaExpression, Enum>> getTriggerCache = new Dictionary<Type, Func<LambdaExpression, Enum>>();
        public static Enum GetTrigger(this LambdaExpression selectorExpression, Type enumType) {
            Func<LambdaExpression, Enum> getTrigger;
            if(!getTriggerCache.TryGetValue(enumType, out getTrigger)) {
                MethodInfo mInfoGetTrigger = getTriggerMethodInfo.MakeGenericMethod(enumType);
                var pActionSelector = Expression.Parameter(typeof(LambdaExpression), "actionSelector");
                getTrigger = Expression.Lambda<Func<LambdaExpression, Enum>>(
                                Expression.Convert(Expression.Call(mInfoGetTrigger, pActionSelector), typeof(Enum))
                            , pActionSelector).Compile();
                getTriggerCache.Add(enumType, getTrigger);
            }
            return getTrigger(selectorExpression);
        }
        readonly static IDictionary<MethodInfo, Enum> triggersCache = new Dictionary<MethodInfo, Enum>();
        public static TEnum GetTrigger<TEnum>(this LambdaExpression selectorExpression) 
            where TEnum : struct {
            Enum result;
            if(!triggersCache.TryGetValue(selectorExpression.@Method(), out result)) {
                var triggerAttribute = selectorExpression.@Attribute<TriggerAttribute>(false);
                string triggerName = (triggerAttribute != null) ?
                    triggerAttribute.name : GetTriggerName(selectorExpression.@MethodName());
                if(string.IsNullOrEmpty(triggerName))
                    throw new NotSupportedException("Unable to resolve the trigger for the method " + selectorExpression.@MethodName());
                TEnum typedResult;
                if(!Enum.TryParse<TEnum>(triggerName, out typedResult))
                    throw new NotSupportedException("Unable to associate the method " + selectorExpression.@MethodName() + " with a specific trigger.");
                result = (Enum)(object)typedResult;
                triggersCache.Add(selectorExpression.@Method(), result);
            }
            return (TEnum)(object)result;
        }
        public static string TriggerName(this MethodInfo method) {
            var triggerAttribute = method.@Attribute<TriggerAttribute>(false);
            return (triggerAttribute != null) ?
                triggerAttribute.name : GetTriggerName(method.Name);
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
        const string TriggerBody_Prefix = "<.ctor>b__";
        internal static string GetTriggerName(MethodInfo mInfo) {
            return TriggerBody_Prefix + mInfo.MetadataToken.ToString();
        }
    }
}