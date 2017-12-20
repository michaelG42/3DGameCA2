using Microsoft.VisualStudio.TestTools.UnitTesting;

//See https://docs.microsoft.com/en-us/visualstudio/test/getting-started-with-unit-testing
namespace GDLibrary.Tests
{
    [TestClass()]
    public class ActorTests
    {

        [TestMethod()]
        public void EqualsTest()
        {
            Actor actorA = new Actor("testid", ActorType.Camera, StatusType.Drawn | StatusType.Update);
            actorA.GroupParameters = new GroupParameters("group0", 0, 1);
            Actor actorB = new Actor("testid", ActorType.Camera, StatusType.Drawn | StatusType.Update);
            actorB.GroupParameters = new GroupParameters("group0", 0, 1);

            Actor actorC = null;

            Assert.IsNotNull(actorA);
            Assert.AreEqual(actorA, actorB);
            Assert.AreNotEqual(actorA, actorC);
        }

        [TestMethod()]
        public void CloneTest()
        {
            Actor actor = new Actor("testid", ActorType.Camera, StatusType.Drawn | StatusType.Update);
            actor.GroupParameters = new GroupParameters("group0", 0, 1);

            Actor clone1 = actor.Clone() as Actor; //another way to call the clone vs. clone = (Actor)actor.Clone();
            Actor clone2 = actor.Clone() as Actor; 
            Actor clone3 = actor.Clone() as Actor; 
            Assert.AreEqual(actor, clone1);

            clone1.ID = "different";
            Assert.AreNotEqual(actor, clone1);

            clone2.ActorType = ActorType.Player;
            Assert.AreNotEqual(actor, clone2);

            clone3.StatusType = StatusType.Drawn;
            Assert.AreNotEqual(actor, clone3);

            clone3.GroupParameters.Name = "group1"; //random value
            Assert.AreNotEqual(actor, clone3);
        }
    }
}