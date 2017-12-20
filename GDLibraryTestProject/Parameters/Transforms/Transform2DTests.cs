using GDLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

//See https://docs.microsoft.com/en-us/visualstudio/test/getting-started-with-unit-testing
namespace GDLibraryTestProject
{
    [TestClass]
    public class Transform2DTests
    {
        [TestMethod]
        public void CloneTest()
        {
            Transform2D original = new Transform2D(new Vector2(10, 10), 45, Vector2.One, Vector2.Zero, new Integer2(10, 20));
            Transform2D clone = (Transform2D)original.Clone();
            Assert.AreEqual(original, clone);

            //change clone and should be distince from original because its a deep copy
            clone.Translation = new Vector2(100, 300);
            Assert.AreNotEqual(original, clone);
        }

        [TestMethod]
        public void ResetTest()
        {
            //original and a copy for comparison after reset
            Transform2D original = new Transform2D(new Vector2(10, 10), 45, Vector2.One, Vector2.Zero, new Integer2(10, 20));
            Transform2D clone = (Transform2D)original.Clone();

            //change somethings in the original
            
            original.Translation = Vector2.Zero;
            original.RotationInDegrees = 45;
            original.Scale = new Vector2(45, 55);
            original.Origin = new Vector2(-10, 10);

            //reset the original
            original.Reset();

            //ensure its been reset
            Assert.AreEqual(original, clone);
        }

    }
}
