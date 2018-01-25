namespace FSM.POCO.Internal {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    static class ExpressionHelper {
        public static MethodInfo Method(this Expression<Action<object[]>> actionSelector) {
            if(actionSelector == null)
                throw new ArgumentNullException("actionSelector");
            if(!(actionSelector.Body is MethodCallExpression))
                throw new NotSupportedException(actionSelector.ToString()); // TODO
            return ((MethodCallExpression)actionSelector.Body).Method;
        }
        public static string MethodName(this Expression<Action<object[]>> actionSelector) {
            return Method(actionSelector).Name;
        }
        public static TAttribute Attribute<TAttribute>(
            this Expression<Action<object[]>> actionSelector, bool throwWhenEmpty = true)
            where TAttribute : Attribute {
            var method = Method(actionSelector);
            var attributes = method.GetCustomAttributes(typeof(TAttribute), true);
            if(attributes == null || attributes.Length == 0) {
                if(throwWhenEmpty)
                    throw new ArgumentException("The " + method.Name + "method do not marked with the " + typeof(TAttribute).Name + "attribute");
                attributes = new Attribute[] { null };
            }
            return (TAttribute)attributes[0];
        }
    }
}