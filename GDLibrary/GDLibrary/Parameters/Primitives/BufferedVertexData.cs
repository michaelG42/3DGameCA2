/*
Function: 		This child class for drawing primitives where the vertex data is buffered on the GFX card in VRAM.
                Note: 
                - The class is generic and can be used to draw VertexPositionColor, VertexPositionColorTexture, VertexPositionColorNormal types etc.
                - For each draw the GFX card refers to vertex data that has already been buffered to VRAM 
                - This is a more efficient approach than either using the VertexData or DynamicBufferedVertexData classes if
                  you wish to draw a large number of primitives on screen.

                See http://rbwhitaker.wikidot.com/index-and-vertex-buffers

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BufferedVertexData<T> : VertexData<T> where T : struct, IVertexType
    {
        #region Variables
        private VertexBuffer vertexBuffer;
        #endregion

        #region Properties
        public VertexBuffer VertexBuffer
        {
            get
            {
                return vertexBuffer;
            }
            set
            {
                vertexBuffer = value;

            }
        }
        #endregion

        //allows developer to pass in buffer only - more efficient since buffer is defined ONCE outside of the object instead of a new VertexBuffer for EACH instance of the class
        public BufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, PrimitiveType primitiveType, int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            if (vertices != null)
            {
                //BufferUsage set to WriteOnly will instruct the GFX card to choose the most efficient VRAM location for retrieving this data (i.e. closest to the GPU(s))
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.WriteOnly);
                //serialize the data to the reserved buffer on VRAM
                vertexBuffer.SetData<T>(vertices);

                //reference to the buffer
                this.VertexBuffer = vertexBuffer;
            }
        }

        //internal - only called by Clone()
        protected BufferedVertexData(VertexBuffer vertexBuffer, PrimitiveType primitiveType, int primitiveCount)
            : base(null, primitiveType, primitiveCount)
        {
            //reference to the buffer
            this.VertexBuffer = vertexBuffer;

            //set underlying vertices that were temporarily passed as null in call to base constructor above
            this.VertexBuffer.GetData<T>(this.Vertices);
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            //use the vertices in this buffer in VRAM to draw the primitive
            effect.GraphicsDevice.SetVertexBuffer(this.vertexBuffer);

            //draw!
            effect.GraphicsDevice.DrawPrimitives(this.PrimitiveType, 0, this.PrimitiveCount);           
        }

        public new object Clone()
        {
            return new BufferedVertexData<T>(this.VertexBuffer, //shallow - reference
                this.PrimitiveType, //struct - deep
                this.PrimitiveCount); //deep - primitive
        }
    }
}
