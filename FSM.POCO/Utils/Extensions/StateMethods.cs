namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using BF = System.Reflection.BindingFlags;

    class StateMethods {
        public static MethodInfo[] GetStateMethods(Type machineType) {
            return machineType.GetMethods(BF.Public | BF.NonPublic | BF.Instance)
                .Where(m => IsStateMethod(m)).ToArray();
        }
        static bool IsStateMethod(MethodInfo mInfo) {
            if(IsSpecialOrGeneric(mInfo))
                return false;
            if(!CanAccessFromDescendant(mInfo) && !CanHandeReturnType(mInfo))
                return false;
            ParameterInfo[] parameters = mInfo.GetParameters();
            for(int i = 0; i < parameters.Length; i++) {
                if(!CanHandleParameter(parameters[i]))
                    return false;
            }
            Attribute stateAttribute = GetMemberAttributes(mInfo)
                .FirstOrDefault(x => stateAttributeType.IsAssignableFrom(x.GetType()));
            return stateAttribute != null;
        }
        static bool IsSpecialOrGeneric(MethodInfo mInfo) {
            return
                mInfo.IsSpecialName || mInfo.DeclaringType == typeof(object) ||
                mInfo.IsGenericMethodDefinition;
        }
        static bool CanAccessFromDescendant(MethodBase method) {
            return
                method.IsPublic ||
                method.IsFamily || method.IsFamilyOrAssembly;
        }
        static bool CanHandeReturnType(MethodInfo mInfo) {
            return
                mInfo.ReturnType == typeof(void) ||
                mInfo.ReturnType == typeof(Task); // TODO: Support async methods
        }
        static bool CanHandleParameter(ParameterInfo parameter) {
            return
                !parameter.IsOut &&
                !parameter.ParameterType.IsByRef && !parameter.ParameterType.IsGenericParameter;
        }
        readonly static Attribute[] EmptyAttributes = new Attribute[0];
        readonly static Type stateAttributeType = typeof(StateAttribute);
        static IEnumerable<Attribute> GetMemberAttributes(MemberInfo mInfo) {
            return mInfo.GetCustomAttributes(stateAttributeType, false)
                .OfType<Attribute>() ?? EmptyAttributes;
        }
    }
}