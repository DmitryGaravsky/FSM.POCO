namespace FSM.POCO.Tests {
    using FSM.POCO.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class StateMethodsTests {
        #region Test Classes
        enum State { Idle };
        class Machine : IPOCOMachine<State> {
            [State(State.Idle)]
            public void OnIdle() { }
        }
        #endregion Test Classes
        [Test]
        public void Test00_GetStateMethods() {
            var methods = StateMethods.GetStateMethods(typeof(Machine));
            Assert.AreEqual(1, methods.Length);
        }
    }
}