using System;
using GDLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GDApp
{
    public class MyPrimitiveFactory : PrimitiveFactory
    {
        #region Fields
        //re-used local vars inside methods
        private PrimitiveType primitiveType;
        private int primitiveCount;
        private IVertexData vertexData;
        #endregion

        /*
         * For each new shape in your game do the following:
         * 
         * 1. In PrimitiveUtility add a unique enum entry to ShapeType. 
         * 2. In PrimitiveUtility add a Get...() method to return the vertices for your new primitive.
         * 3. In MyPrimitiveFactory::GetArchetypePrimitiveObject() add a switch case for your new PrimitiveUtility::Get...() method and ShapeType enum entry.
         * 4. In MyPrimitiveFactory add a Get...() method which calls the underlying PrimitiveUtility::Get...() method and creates a PrimitiveObject from the vertex data and effect parameters provided.
         * 5. In your game using your primitive factory to create objects - See code like Main::InitializeQuadPrimitives().
         * 
         * Q. Do you remember the different vertex types like VertexPositionColor, VertexPositionColorTexture?
         * Well, for each vertex type in your final game (e.g. textured cubes using VertexPositionColorTexture, and wireframe lines using VertexPositionColor)
         * you need a SEPARATE BasicEffect instance. You cant just use one effect for different vertex types because once the effect is given vertices
         * which use a specific vertex type (e.g. VertexPositionColor) it then EXPECTS all other vertices to be of that type (i.e. VertexPositionColor).
         * 
         * This is why in Main::InitializeEffects() I create a different BasicEffect (and hence BasicEffectParameters) for each of the vertex types found in the
         * primitives that I know I will create (e.g. textured primitives, colored primitives, wireframe primitives).
         * 
         * Lastly, when you create new vertices in PrimitiveUtility, always centre these primitives close to, or on the origin and give them unit scale (e.g. 1x1).
         * Why? Remember that we use a primitives WORLD matrix in the Transform3D to change its position, rotation, and scale. Therefore, we can stretch a primitive
         * and change its position in the world using Transform3D as opposed to having a 1x2x4 cube and a 2x8x10 cube etc in our factories dictionary of primitives.
         * 
         */
         //returns a primitive object (with Transform3D set to Zero) based on the shape type and effect parameters provided by the developer
        public PrimitiveObject GetArchetypePrimitiveObject(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            PrimitiveObject primitiveObject = null;

            //add a Get...() method for each of the UNIQUE shapes that you have in your game
            switch (shapeType)
            {
                case ShapeType.WireframeLine:
                        {
                            if (this.PrimitiveDictionary.ContainsKey(shapeType))
                                primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                            else
                            primitiveObject = GetWireframeLine(graphics, shapeType, effectParameters);
                        }
                        break;

                case ShapeType.WireframeOrigin:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetWireframeOrigin(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.WireframeSphere:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetWireframeSphere(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.WireframeSpiral:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetWireframeSpiral(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.ColoredQuad:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetColoredQuad(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.TexturedQuad:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetTexturedQuad(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.TexturedCube:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetTexturedCube(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.TexturedPyramidSquare:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetTexturedPyramidSquare(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.Billboard:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetTexturedBillboard(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.NormalCube:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetNormalTexturedCube(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.NormalSphere:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetNormalTexturedSphere(graphics, shapeType, effectParameters);
                    }
                    break;

                case ShapeType.ColoredSphere:
                    {
                        if (this.PrimitiveDictionary.ContainsKey(shapeType))
                            primitiveObject = this.PrimitiveDictionary[shapeType].Clone() as PrimitiveObject;
                        else
                            primitiveObject = GetColoredSphere(graphics, shapeType, effectParameters);
                    }
                    break;

                //add a case here for each of your custom shapes (e.g. lit diamonds)

                default:
                    break;

            }

            return primitiveObject;
        }

     
        private PrimitiveObject GetWireframeLine(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.GetWireframeLine(out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColor>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetWireframeOrigin(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.GetWireframeOrigin(out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColor>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetWireframeSphere(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.GetWireframeSphere(15, out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColor>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetWireframeSpiral(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.GetWireframeSpiral(15, 1, out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColor>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }


        private PrimitiveObject GetColoredQuad(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //vertex colors for the quad
            Color[] vertexColorArray = { Color.Red, Color.Green, Color.Blue, Color.Orange };

            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.GetColoredQuad(vertexColorArray, out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColor>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetTexturedQuad(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColorTexture[] vertices = PrimitiveUtility.GetTexturedQuad(out primitiveType, out primitiveCount);

            //create the buffered data
            vertexData = new BufferedVertexData<VertexPositionColorTexture>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetTexturedCube(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColorTexture[] vertices = PrimitiveUtility.GetTexturedCube(out primitiveType, out primitiveCount);

            /* 
             * Remember we saw in PrimitiveUtility.GetTexturedCube() that we were using too many vertices? 
             * We can use IndexedBufferedVertexData to now reduce the 36 vertices to 12.
             * 
             * Notice however that we are STILL using TriangleList as our PrimitiveType. The optimal solution is to use an IndexedBufferedVertexData object where you define the indices and vertices
             * and also choose TriangleStrip or LineStrip as your PrimitiveType.
             */

            vertexData = new IndexedBufferedVertexData<VertexPositionColorTexture>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }


        private PrimitiveObject GetTexturedPyramidSquare(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColorTexture[] vertices = PrimitiveUtility.GetTexturedPyramidSquare(out primitiveType, out primitiveCount);

            vertexData = new BufferedVertexData<VertexPositionColorTexture>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        

        private PrimitiveObject GetNormalTexturedCube(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionNormalTexture[] vertices = PrimitiveUtility.GetNormalTexturedCube(out primitiveType, out primitiveCount);

            //create the buffered data using indexed since it will reduce the number of vertices required from 36 - 12 - see GetTexturedCube() comment
            vertexData = new IndexedBufferedVertexData<VertexPositionNormalTexture>(graphics, vertices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetNormalTexturedSphere(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionNormalTexture[] vertices = PrimitiveUtility.GetNormalTexturedSphere(out primitiveType, out primitiveCount);

            short[] indices = PrimitiveUtility.GetSphereIndices();

            //create the buffered data using indexed since it will reduce the number of vertices required from 36 - 12 - see GetTexturedCube() comment
            vertexData = new IndexedBufferedVertexData<VertexPositionNormalTexture>(graphics, vertices, indices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

        private PrimitiveObject GetColoredSphere(GraphicsDevice graphics, ShapeType shapeType, EffectParameters effectParameters)
        {
            //get the vertices
            VertexPositionColor[] vertices = PrimitiveUtility.getColoredSphere(out primitiveType, out primitiveCount);

            short[] indices = PrimitiveUtility.GetColoredSphereIndices();

            //create the buffered data using indexed since it will reduce the number of vertices required from 36 - 12 - see GetTexturedCube() comment
            vertexData = new IndexedBufferedVertexData<VertexPositionColor>(graphics, vertices, indices, primitiveType, primitiveCount);

            //instanciate the object and return a reference
            return GetPrimitiveObjectFromVertexData(vertexData, shapeType, effectParameters);
        }

    }
}
