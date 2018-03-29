namespace FSM.POCO.Internal {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BF = System.Reflection.BindingFlags;
    using MA = System.Reflection.MethodAttributes;

    static class ExpressionHelper {
        public static MethodInfo Method(this LambdaExpression actionSelector) {
            if(actionSelector == null)
                throw new ArgumentNullException("actionSelector");
            if(!(actionSelector.Body is MethodCallExpression))
                throw new NotSupportedException(actionSelector.ToString()); // TODO
            return ((MethodCallExpression)actionSelector.Body).Method;
        }
        public static string MethodName(this LambdaExpression actionSelector) {
            return Method(actionSelector).Name;
        }
        public static TAttribute Attribute<TAttribute>(
            this LambdaExpression actionSelector, bool throwWhenEmpty = true)
            where TAttribute : Attribute {
            return Attribute<TAttribute>(actionSelector.@Method(), throwWhenEmpty);
        }
        public static TAttribute Attribute<TAttribute>(this MethodInfo method, bool throwWhenEmpty = true)
            where TAttribute : Attribute {
            var attributes = method.GetCustomAttributes(typeof(TAttribute), true);
            if(attributes == null || attributes.Length == 0) {
                if(throwWhenEmpty)
                    throw new ArgumentException("The " + method.Name + "method do not marked with the " + typeof(TAttribute).Name + "attribute");
                attributes = new Attribute[] { null };
            }
            return (TAttribute)attributes[0];
        }
        //
        public static Expression New(Type type) {
            var cInfo = type.GetConstructor(Type.EmptyTypes) ??
                FindConstructorWithAllOptionalParameters(type);
            return Expression.New(cInfo);
        }
        public static Expression Init(Type type, LambdaExpression ctorExpression) {
            NewExpression newExpression = ctorExpression.Body as NewExpression;
            if(newExpression != null)
                return GetNewExpression(type, newExpression);
            MemberInitExpression memberInitExpression = ctorExpression.Body as MemberInitExpression;
            if(memberInitExpression != null)
                return Expression.MemberInit(GetNewExpression(type, memberInitExpression.NewExpression), memberInitExpression.Bindings);
            throw new ArgumentException("constructorExpression");
        }
        static ConstructorInfo FindConstructorWithAllOptionalParameters(Type type) {
            return
                type.GetConstructors(BF.Public | BF.NonPublic | BF.Instance)
                .FirstOrDefault(x => x.IsPublicOrFamily() && x.AllOptionalParameters());
        }
        static bool IsPublicOrFamily(this ConstructorInfo cInfo) {
            return cInfo.Attributes.HasFlag(MA.Public) || cInfo.Attributes.HasFlag(MA.Family);
        }
        static bool AllOptionalParameters(this ConstructorInfo cInfo) {
            return cInfo.GetParameters().All(y => y.IsOptional);
        }
        static NewExpression GetNewExpression(Type type, NewExpression newExpression) {
            Type[] argsTypes = newExpression.Constructor.GetParameters()
                .Select(x => x.ParameterType).ToArray();
            var cInfo = type.GetConstructor(argsTypes ?? Type.EmptyTypes);
            return Expression.New(cInfo, newExpression.Arguments);
        }
    }
}