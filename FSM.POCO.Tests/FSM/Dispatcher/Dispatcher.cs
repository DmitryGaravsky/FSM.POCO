namespace FSM.POCO.Tests {
    using System;
    using System.Collections.Generic;
    using FSM.POCO.Internal;
    using NUnit.Framework;

    [TestFixture]
    public partial class DispatcherTests {
        #region Test Classes
        enum State { First, Second };
        enum Trigger { ToSecond, ToFirst };
        readonly static IDispatchersSettings<State> Settings = new DispatchersSettings<State, Trigger>();
        sealed class TestSettings : IDispatchersSettings<State> {
            public TestSettings(Type triggerType) {
                TriggerType = triggerType;
            }
            public State InitialState { get; set; }
            public Type TriggerType { get; private set; }
        }
        #endregion Test Classes
        [Test]
        public void Test_00_Smoke() {
            Assert.Throws(typeof(ArgumentNullException), () => new Dispatcher<State>(null, null));
            Assert.Throws(typeof(ArgumentNullException), () => new Dispatcher<State>(null, new TestSettings(null)));
            Assert.Throws(typeof(ArgumentException), () => new Dispatcher<State>(null, new TestSettings(typeof(string))));
        }
        [Test]
        public void Test_01_Initial() {
            IDispatcher<State> dispatcher = new Dispatcher<State>(null, Settings);
            Assert.AreEqual(typeof(Trigger), dispatcher.Settings.TriggerType);
            Assert.AreEqual(State.First, dispatcher.Current);
        }
        [Test]
        public void Test_02_SetState() {
            IDispatcher<State> dispatcher = new Dispatcher<State>(null, Settings);
            dispatcher.SetState(State.Second);
            Assert.AreEqual(State.Second, dispatcher.Current);
            dispatcher.SetState(State.First);
            Assert.AreEqual(State.First, dispatcher.Current);
        }
        [Test]
        public void Test_03_Dispatch_Smoke() {
            IDispatcher<State> dispatcher = new Dispatcher<State>(null, Settings);
            Assert.Throws(typeof(ArgumentException), () => dispatcher.Dispatch(State.First));
        }
        [Test]
        public void Test_03_Dispatch() {
            int toSecond = 0;
            var firstActions = new Dictionary<Enum, Action<object[]>> {
                    { Trigger.ToSecond, _=> toSecond++ }};
            int toFirst = 0;
            var secondActions = new Dictionary<Enum, Action<object[]>> {
                    { Trigger.ToFirst, _=> toFirst++ }};
            IDispatcher<State> dispatcher = new Dispatcher<State>(new Dictionary<State, IDictionary<Enum, Action<object[]>>> { 
                { State.First, firstActions },
                { State.Second, secondActions },
            }, Settings);
            // First state
            dispatcher.Dispatch(Trigger.ToSecond);
            Assert.AreEqual(1, toSecond);
            dispatcher.Dispatch(Trigger.ToSecond);
            Assert.AreEqual(2, toSecond);
            dispatcher.Dispatch(Trigger.ToFirst);
            Assert.AreEqual(0, toFirst);
            // Second state
            dispatcher.SetState(State.Second);
            dispatcher.Dispatch(Trigger.ToFirst);
            Assert.AreEqual(1, toFirst);
            dispatcher.Dispatch(Trigger.ToFirst);
            Assert.AreEqual(2, toFirst);
            dispatcher.Dispatch(Trigger.ToSecond);
            Assert.AreEqual(2, toSecond);
        }
    }
}