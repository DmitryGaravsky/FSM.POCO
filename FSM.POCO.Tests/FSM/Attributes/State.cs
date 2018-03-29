namespace FSM.POCO.Tests {
    using System;
    using System.Linq.Expressions;
    using FSM.POCO.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class AttributesTests {
        #region Test Classes
        enum State { Idle = 5 };
        class EnumMachine : IPOCOMachine<State> {
            [State(State.Idle)]
            internal void OnIdle_0(params object[] parameters) { }
            [State("Idle")]
            internal void OnIdle_1(params object[] parameters) { }
            [State("idle")]
            internal void OnIdle_2(params object[] parameters) { }
            [State(5)]
            internal void OnIdle_3(params object[] parameters) { }
        }
        class IntMachine : IPOCOMachine<int> {
            [State("5")]
            internal void OnIdle_0(params object[] parameters) { }
        }
        #endregion Test Classes
        EnumMachine em = null;
        IntMachine im = null;
        [Test]
        public void Test00_GetState_Enum() {
            Expression<Action<object[]>> onIdle = x => em.OnIdle_0(x);
            Assert.AreEqual(State.Idle, onIdle.GetState<State>());
        }
        [Test]
        public void Test00_GetState_String() {
            Expression<Action<object[]>> onIdle = x => em.OnIdle_1(x);
            Assert.AreEqual(State.Idle, onIdle.GetState<State>());
        }
        [Test]
        public void Test00_GetState_StringIgnoreCase() {
            Expression<Action<object[]>> onIdle = x => em.OnIdle_2(x);
            Assert.AreEqual(State.Idle, onIdle.GetState<State>());
        }
        [Test]
        public void Test00_GetState_UnderlyingType() {
            Expression<Action<object[]>> onIdle = x => em.OnIdle_2(x);
            Assert.AreEqual(State.Idle, onIdle.GetState<State>());
        }
        [Test]
        public void Test00_GetState_Convertible() {
            Expression<Action<object[]>> onIdle = x => im.OnIdle_0(x);
            Assert.AreEqual(5, onIdle.GetState<int>());
        }
        [Test]
        public void Test01_GetStateType() {
            Assert.AreEqual(typeof(int), StateExtension.GetStateType(typeof(IntMachine)));
            Assert.AreEqual(typeof(State), StateExtension.GetStateType(typeof(EnumMachine)));
        }
    }
}