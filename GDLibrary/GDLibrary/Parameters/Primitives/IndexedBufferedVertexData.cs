/*
Function: 		This child class for drawing primitives where the vertex AND index buffer data are buffered on the GFX card in VRAM.
                Note: 
                - The class is generic and can be used to draw VertexPositionColor, VertexPositionColorTexture, VertexPositionColorNormal types etc.
                - For each draw the GFX card refers to vertexa and index data that has already been buffered to VRAM 
                - This is a more efficient approach than either using the VertexData or DynamicBufferedVertexData classes if
                  you wish to draw a large number of primitives on scree since it reduces the actual number of vertices required to draw a primitive
                  where the primitive contains multiple surfaces with a single shared point (e.g. consider a cube and the number of faces that share any given vertex).

                See http://rbwhitaker.wikidot.com/index-and-vertex-buffers

Author: 		NMCG
Version:		1.0
Date Updated:	6/12/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GDLibrary
{
    public class IndexedBufferedVertexData<T> : BufferedVertexData<T> where T : struct, IVertexType
    {
        #region Variables
        private IndexBuffer indexBuffer;
        #endregion

        #region Properties
        private IndexBuffer IndexBuffer
        {
            get
            {
                return this.indexBuffer;
            }
            set
            {
                this.indexBuffer = value;
            }
        }
        #endregion


        //allows developer to specify the indices and vertices to be used directly - gives us complete control
        public IndexedBufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, short[] indices, PrimitiveType primitiveType, int primitiveCount)
            : base(graphicsDevice, vertices, primitiveType, primitiveCount)
        {
            this.indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
            this.indexBuffer.SetData<short>(indices);
        }

        //allows developer to pass in vertices AND buffer - more efficient since buffer is defined ONCE outside of the object instead of a new VertexBuffer for EACH instance of the class
        public IndexedBufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, PrimitiveType primitiveType, int primitiveCount)
            : base(graphicsDevice, null, primitiveType, primitiveCount)
        {
            ReduceVertexCount(graphicsDevice, vertices);
        }

        //internal - only called by Clone()
        protected IndexedBufferedVertexData(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveType primitiveType, int primitiveCount)
            : base(vertexBuffer, primitiveType, primitiveCount)
        {
            this.indexBuffer = indexBuffer;
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            //use the vertices in this buffer in VRAM to draw the primitive
            effect.GraphicsDevice.SetVertexBuffer(this.VertexBuffer);

            //use the indices in this index buffer in VRAM to select the vertices from the vertex buffer and draw the primitive
            effect.GraphicsDevice.Indices = this.indexBuffer;

            //draw!
            effect.GraphicsDevice.DrawIndexedPrimitives(this.PrimitiveType, 0, 0, this.Vertices.Length, 0, this.PrimitiveCount);

        }

        public new object Clone()
        {
            return new IndexedBufferedVertexData<T>(this.VertexBuffer,  //shallow - reference
                this.indexBuffer,  //shallow - reference
                this.PrimitiveType, //struct - deep
                this.PrimitiveCount);  //deep - primitive
        }

        //reduces the number of vertices necessary to render the primitive by identifying duplication and utilising an index buffer
        private void ReduceVertexCount(GraphicsDevice graphicsDevice, T[] vertices)
        {  
            Dictionary<T, short> dictionary = new Dictionary<T, short>();
            List<T> vertexList = new List<T>();
            List<short> indexList = new List<short>();
            short index = 0;

            foreach(T vertex in vertices)
            {
                if(!dictionary.ContainsKey(vertex))
                {
                    dictionary.Add(vertex, index);
                    vertexList.Add(vertex);
                    index++;
                }
            }

            foreach (T vertex in vertices)
            {
                indexList.Add(dictionary[vertex]);
            }

            //garbage collect old vertices
            this.Vertices = null;
            //assign new vertices
            this.Vertices = vertexList.ToArray();

            //garbage collect old vertex buffer
            this.VertexBuffer = null;

            //create the vertex buffer based on the reduced set of necessary vertices and assign to vertexBuffer
            this.VertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), this.Vertices.Length, BufferUsage.WriteOnly);

            //set the data
            this.VertexBuffer.SetData<T>(this.Vertices);

            //convert from list to array
            short[] indices = indexList.ToArray();

            //BufferUsage set to WriteOnly will instruct the GFX card to choose the most efficient VRAM location for retrieving the index data (i.e. closest to the GPU(s))
            this.indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);

            //set the data
            this.indexBuffer.SetData<short>(indices);
        }
    }
}
