namespace FSM.POCO.Tests {
    using System;
    using FSM.POCO.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ResolverTests {
        #region Test Classes
        public class Lamp : IPOCOMachine<Lamp.State> {
            public enum State { Off, On }
            public readonly int OnTurnOnID;
            public readonly int OnTurnOffID;
            public Lamp() {
                OnTurnOnID = new Action(OnTurnOn).Method.MetadataToken;
                OnTurnOffID = new Action(OnTurnOff).Method.MetadataToken;
            }
            public int Counter = 0;
            [State(State.Off)]
            protected void OnTurnOn() {
                Counter++;
            }
            [State(State.On)]
            protected void OnTurnOff() {
                Counter--;
            }
        }
        public class Employee { }
        public enum Status { Default }
        public class Bug : IPOCOMachine<Bug.State> {
            public readonly int OnAssignID;
            public readonly int OnCloseID;
            public readonly int OnEscalateID;
            public readonly int OnReopenID;
            public Bug() {
                OnAssignID = new Action<Employee>(OnAssign).Method.MetadataToken;
                OnCloseID = new Action(OnClose).Method.MetadataToken;
                OnEscalateID = new Action(OnEscalate).Method.MetadataToken;
                OnReopenID = new Action<int>(OnReopen).Method.MetadataToken;
            }
            public enum State { Open, Assigned, Closed }
            [State(State.Open)]
            protected void OnAssign(Employee assignee) { }
            [State(State.Assigned)]
            protected void OnClose() { }
            [State(State.Assigned)]
            protected void OnEscalate() { }
            [State(State.Closed), Trigger("REOPEN")]
            protected void OnReopen(int count = 10) { }
        }
        #endregion Test Classes
        Lamp lamp = new Lamp();
        [Test]
        public void Test_00_Triggers() {
            IResolver<Lamp.State> resolver = new Resolver<Lamp.State>();
            var triggers = resolver.ResolveTriggers(typeof(Lamp));
            Assert.AreEqual(2, triggers.Count);
            Assert.AreEqual("TurnOff", triggers[(Resolver<Lamp.State>.Trigger)lamp.OnTurnOffID]);
            Assert.AreEqual("TurnOn", triggers[(Resolver<Lamp.State>.Trigger)lamp.OnTurnOnID]);
        }
        [Test]
        public void Test_01_Transitions() {
            IResolver<Lamp.State> resolver = new Resolver<Lamp.State>();
            var transitions = resolver.ResolveTransitions(typeof(Lamp));
            Assert.AreEqual(2, transitions.Count);
            Assert.IsTrue(transitions.ContainsKey(Lamp.State.On));
            Assert.IsTrue(transitions.ContainsKey(Lamp.State.Off));
            //
            var onTurnOff = transitions[Lamp.State.On][(Resolver<Lamp.State>.Trigger)lamp.OnTurnOffID];
            var onTurnOn = transitions[Lamp.State.Off][(Resolver<Lamp.State>.Trigger)lamp.OnTurnOnID];
            Assert.IsNotNull(onTurnOff);
            Assert.IsNotNull(onTurnOn);
            onTurnOn(new object[] { lamp });
            Assert.AreEqual(1, lamp.Counter);
            onTurnOff(new object[] { lamp });
            Assert.AreEqual(0, lamp.Counter);
        }
        Bug bug = new Bug();
        [Test]
        public void Test_02_Triggers() {
            IResolver<Bug.State> resolver = new Resolver<Bug.State>();
            var triggers = resolver.ResolveTriggers(typeof(Bug));
            Assert.AreEqual(4, triggers.Count);
            Assert.AreEqual("Assign", triggers[(Resolver<Bug.State>.Trigger)bug.OnAssignID]);
            Assert.AreEqual("Close", triggers[(Resolver<Bug.State>.Trigger)bug.OnCloseID]);
            Assert.AreEqual("Escalate", triggers[(Resolver<Bug.State>.Trigger)bug.OnEscalateID]);
            Assert.AreEqual("REOPEN", triggers[(Resolver<Bug.State>.Trigger)bug.OnReopenID]);
        }
        [Test]
        public void Test_03_Transitions() {
            IResolver<Bug.State> resolver = new Resolver<Bug.State>();
            var transitions = resolver.ResolveTransitions(typeof(Bug));
            Assert.AreEqual(3, transitions.Count);
            //
            var onAssign = transitions[Bug.State.Open][(Resolver<Bug.State>.Trigger)bug.OnAssignID];
            Assert.IsNotNull(onAssign);
            onAssign(new object[] { bug, new Employee() });

            var onReopen = transitions[Bug.State.Closed][(Resolver<Bug.State>.Trigger)bug.OnReopenID];
            Assert.IsNotNull(onReopen);
            onReopen(new object[] { bug });
        }
    }
}