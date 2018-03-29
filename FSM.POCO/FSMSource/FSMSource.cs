namespace FSM.POCO {
    using System;
    using System.Linq.Expressions;

    public static partial class Source {
        public static TMachine Create<TMachine>() 
            where TMachine : class, Internal.IPOCOMachine {
            Type stateType = Internal.StateExtension.GetStateType(typeof(TMachine));
            return typesActivator.Create<TMachine>(type => CreateType(type, stateType));
        }
        public static TMachine Create<TMachine>(Expression<Func<TMachine>> ctorExpression)
            where TMachine : class, Internal.IPOCOMachine {
            Type stateType = Internal.StateExtension.GetStateType(typeof(TMachine));
            return typesActivator.Create<TMachine>(type => CreateType(type, stateType), ctorExpression);
        }
    }
}