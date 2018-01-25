namespace FSM.POCO {
    using System;
    using System.Linq.Expressions;

    public static class Source {
        public static TMachine Create<TMachine>()
            where TMachine : class, Internal.IPOCOMachine {
            return default(TMachine); // TODO
        }
        public static TMachine Create<TMachine>(Expression<Func<TMachine>> ctorExpression)
            where TMachine : class, Internal.IPOCOMachine {
            return default(TMachine); // TODO
        }
    }
}