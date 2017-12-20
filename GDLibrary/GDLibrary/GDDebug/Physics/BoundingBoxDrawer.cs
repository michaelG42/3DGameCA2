/*
Function: 		Creates the vertices to represent a bounding box which is NOT axis-aligned. Remember XNA, by default, can only create axis-aligned bounding boxes. Used by the PhysicsDebugDrawer.
Author: 		NMCG
Version:		1.0
Date Updated:	27/10/17
Bugs:			
Fixes:			None
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace GDLibrary
{
    //See http://timjones.tw/blog/archive/2010/12/10/drawing-an-xna-model-bounding-box
    public class BoundingBoxBuffers
    {
        public VertexBuffer Vertices;
        public int VertexCount;
        public IndexBuffer Indices;
        public int PrimitiveCount;
    }

    //See http://timjones.tw/blog/archive/2010/12/10/drawing-an-xna-model-bounding-box
    public class BoundingBoxDrawer
    {
        public static BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox, GraphicsDevice graphicsDevice)
        {
            return CreateBoundingBoxBuffers(boundingBox, graphicsDevice, Color.White);
        }

        public static BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox, GraphicsDevice graphicsDevice, Color color)
        {
            BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();

            boundingBoxBuffers.PrimitiveCount = 24;
            boundingBoxBuffers.VertexCount = 48;

            VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionColor), boundingBoxBuffers.VertexCount,
                BufferUsage.WriteOnly);
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            const float ratio = 5.0f;

            Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
            Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
            Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
            Vector3[] corners = boundingBox.GetCorners();

            // Corner 1.
            AddVertex(vertices, corners[0], color);
            AddVertex(vertices, corners[0] + xOffset, color);
            AddVertex(vertices, corners[0], color);
            AddVertex(vertices, corners[0] - yOffset, color);
            AddVertex(vertices, corners[0], color);
            AddVertex(vertices, corners[0] - zOffset, color);

            // Corner 2.
            AddVertex(vertices, corners[1], color);
            AddVertex(vertices, corners[1] - xOffset, color);
            AddVertex(vertices, corners[1], color);
            AddVertex(vertices, corners[1] - yOffset, color);
            AddVertex(vertices, corners[1], color);
            AddVertex(vertices, corners[1] - zOffset, color);

            // Corner 3.
            AddVertex(vertices, corners[2], color);
            AddVertex(vertices, corners[2] - xOffset, color);
            AddVertex(vertices, corners[2], color);
            AddVertex(vertices, corners[2] + yOffset, color);
            AddVertex(vertices, corners[2], color);
            AddVertex(vertices, corners[2] - zOffset, color);

            // Corner 4.
            AddVertex(vertices, corners[3], color);
            AddVertex(vertices, corners[3] + xOffset, color);
            AddVertex(vertices, corners[3], color);
            AddVertex(vertices, corners[3] + yOffset, color);
            AddVertex(vertices, corners[3], color);
            AddVertex(vertices, corners[3] - zOffset, color);

            // Corner 5.
            AddVertex(vertices, corners[4], color);
            AddVertex(vertices, corners[4] + xOffset, color);
            AddVertex(vertices, corners[4], color);
            AddVertex(vertices, corners[4] - yOffset, color);
            AddVertex(vertices, corners[4], color);
            AddVertex(vertices, corners[4] + zOffset, color);

            // Corner 6.
            AddVertex(vertices, corners[5], color);
            AddVertex(vertices, corners[5] - xOffset, color);
            AddVertex(vertices, corners[5], color);
            AddVertex(vertices, corners[5] - yOffset, color);
            AddVertex(vertices, corners[5], color);
            AddVertex(vertices, corners[5] + zOffset, color);

            // Corner 7.
            AddVertex(vertices, corners[6], color);
            AddVertex(vertices, corners[6] - xOffset, color);
            AddVertex(vertices, corners[6], color);
            AddVertex(vertices, corners[6] + yOffset, color);
            AddVertex(vertices, corners[6], color);
            AddVertex(vertices, corners[6] + zOffset, color);

            // Corner 8.
            AddVertex(vertices, corners[7], color);
            AddVertex(vertices, corners[7] + xOffset, color);
            AddVertex(vertices, corners[7], color);
            AddVertex(vertices, corners[7] + yOffset, color);
            AddVertex(vertices, corners[7], color);
            AddVertex(vertices, corners[7] + zOffset, color);

            vertexBuffer.SetData(vertices.ToArray());
            boundingBoxBuffers.Vertices = vertexBuffer;

            IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
            boundingBoxBuffers.Indices = indexBuffer;

            return boundingBoxBuffers;
        }

        private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position, Color color)
        {
            vertices.Add(new VertexPositionColor(position, color));
        }

        public static void DrawBoundingBox(BoundingBoxBuffers buffers, BasicEffect effect, GraphicsDevice graphicsDevice, Camera3D camera)
        {
            graphicsDevice.SetVertexBuffer(buffers.Vertices);
            graphicsDevice.Indices = buffers.Indices;
            effect.World = Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.ProjectionParameters.Projection;
            effect.CurrentTechnique.Passes[0].Apply();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
                    buffers.VertexCount, 0, buffers.PrimitiveCount);
            }
        }

    }
}
