using GDLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

//See https://docs.microsoft.com/en-us/visualstudio/test/getting-started-with-unit-testing
namespace GDLibraryTestProject
{
    [TestClass]
    public class Transform3DTests
    {

        [TestMethod]
        public void CloneTest()
        {
            Transform3D original = new Transform3D(new Vector3(0, 0, 20), Vector3.Zero, Vector3.One, -Vector3.UnitZ, Vector3.UnitY);
            Transform3D clone = (Transform3D)original.Clone();
            Assert.AreEqual(original, clone);

            //change clone and should be distince from original because its a deep copy
            clone.Translation = new Vector3(100, 300, 10);
            Assert.AreNotEqual(original, clone);
        }

        [TestMethod]
        public void ResetTest()
        {
            //original and a copy for comparison after reset
            Transform3D original = new Transform3D(new Vector3(0, 0, 20), Vector3.Zero, Vector3.One, -Vector3.UnitZ, Vector3.UnitY);
            Transform3D clone = (Transform3D)original.Clone();

            //change somethings in the original

            original.Translation = Vector3.Zero;
            original.Rotation = new Vector3(15, 45, 90);
            original.Scale = new Vector3(1, 2, 3);
            original.Look = Vector3.UnitY;
            original.Up = Vector3.UnitX;

            //reset the original
            original.Reset();

            //ensure its been reset
            Assert.AreEqual(original, clone);
        }

    }
}
