namespace FSM.POCO.Internal {
    using System;
    using System.Diagnostics;

    static class @MayBe {
        [DebuggerStepThrough, DebuggerHidden]
        public static TResult @Get<T, TResult>(this T @this, Func<T, TResult> @get, TResult defaultValue = default(TResult)) {
            return (@this != null) ? @get(@this) : defaultValue;
        }
        [DebuggerStepThrough, DebuggerHidden]
        public static void @Do<T>(this T @this, Action<T> @do) {
            if(@this != null) @do(@this);
        }
    }
}