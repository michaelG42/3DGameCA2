using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDLibrary
{

    public class PrimitiveFactory
    {

        #region Fields
        private Dictionary<ShapeType, PrimitiveObject> primitiveDictionary;
        #endregion

        #region Properties
        public Dictionary<ShapeType, PrimitiveObject> PrimitiveDictionary
        {
            get
            {
                return this.primitiveDictionary;
            }
        }
        #endregion


        public PrimitiveFactory()
        {
            this.primitiveDictionary = new Dictionary<ShapeType, PrimitiveObject>();
        }

        protected PrimitiveObject GetPrimitiveObjectFromVertexData(IVertexData vertexData, ShapeType shapeType, EffectParameters effectParameters)
        {
            //instanicate the object
            PrimitiveObject primitiveObject = new PrimitiveObject("Archetype - " + shapeType.ToString(),
                ActorType.NotYetAssigned,
                Transform3D.Zero,
                effectParameters.Clone() as EffectParameters, 
                StatusType.Update | StatusType.Drawn, 
                vertexData);

            //add to the dictionary for re-use in any subsequent call to this method
            primitiveDictionary.Add(shapeType, primitiveObject);

            //return a reference to a CLONE of our original object - remember we always clone the dictionary object, rather than modify the archetype in the dictionary
            return primitiveObject.Clone() as PrimitiveObject;
        }


        protected PrimitiveObject GetTexturedBillboard(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            PrimitiveType primitiveType;
            int primitiveCount;
            IVertexData vertexData;

            //get the vertices
            VertexBillboard[] vertices = PrimitiveUtility.GetBillboard(out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexBillboard>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }
    }
}
