/*
Function: 		A utility used to return the vertices for common primitives used by your game.

Author: 		NMCG
Version:		1.0
Date Updated:	7/12/17
Bugs:			
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    #region Ignore
    //billboards are drawn with a custom shader and need a special vertex type declaration
    public struct VertexBillboard : IVertexType
    {
        #region Variables
        public Vector3 position;
        public Vector4 texCoordAndOffset;
        #endregion

        public VertexBillboard(Vector3 position, Vector4 texCoordAndOffset)
        {
            this.position = position;
            this.texCoordAndOffset = texCoordAndOffset;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    //used to create a simple 3D sphere from 3 circles on 3 planes
    public enum OrientationType : sbyte
    {
        XYAxis,
        XZAxis,
        YZAxis
    }
    #endregion

    public enum ShapeType : sbyte
    {
        //add an enum for each of the UNIQUE shapes that you have in your game...
        WireframeLine, //a little redundant qualifying a line as wireframe! :)
        WireframeOrigin,
        WireframeCircle,
        WireframeSphere,
        WireframeSpiral,
        WireframeTriangle,
        WireframePyramid,
        WireframeQuad,
        WireframeCube,

        ColoredTriangle,
        ColoredQuad,
        ColoredCube,
        ColoredSphere,

        TexturedQuad,
        TexturedCube,
        TexturedPyramidSquare, //square based pyramid

        //creates primitives that we can apply lighting to using the directional lights of BasicEffect
        NormalQuad,
        NormalCube,
        NormalSphere,

        //creates normal, spherical, or cylindrical billboards
        Billboard
    }

    public class PrimitiveUtility
    {
        #region Ignore
        /*************************************************************** Billboard (unlit) ***************************************************************/

        //returns the vertices for a billboard which has a custom vertex declaration
        public static VertexBillboard[] GetBillboard(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            VertexBillboard[] vertices = new VertexBillboard[4];
            float halfSideLength = 0.5f;

            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexBillboard(Vector3.Zero, new Vector4(uvTopLeft, -halfSideLength, halfSideLength));
            vertices[1] = new VertexBillboard(Vector3.Zero, new Vector4(uvTopRight, halfSideLength, halfSideLength));
            vertices[2] = new VertexBillboard(Vector3.Zero, new Vector4(uvBottomLeft, -halfSideLength, -halfSideLength));
            vertices[3] = new VertexBillboard(Vector3.Zero, new Vector4(uvBottomRight, halfSideLength, -halfSideLength));

            return vertices;
        }
        #endregion

        /*************************************************************** Wireframe ***************************************************************/
        
        //returns the vertices for a 1 unit length line segment centred around the origin
        public static VertexPositionColor[] GetWireframeLine(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 1;

            VertexPositionColor[] vertices = new VertexPositionColor[2];

            float halfSideLength = 0.5f;

            Vector3 left = new Vector3(-halfSideLength, 0, 0);
            Vector3 right = new Vector3(halfSideLength, 0, 0);

            vertices[0] = new VertexPositionColor(left, Color.White);
            vertices[1] = new VertexPositionColor(right, Color.White);

            return vertices;
        }

        public static VertexPositionColor[] GetWireframeOrigin(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 10;

            VertexPositionColor[] vertices = new VertexPositionColor[20];

            //x-axis
            vertices[0] = new VertexPositionColor(-Vector3.UnitX, Color.DarkRed);
            vertices[1] = new VertexPositionColor(Vector3.UnitX, Color.DarkRed);

            //y-axis
            vertices[2] = new VertexPositionColor(-Vector3.UnitY, Color.DarkGreen);
            vertices[3] = new VertexPositionColor(Vector3.UnitY, Color.DarkGreen);

            //z-axis
            vertices[4] = new VertexPositionColor(-Vector3.UnitZ, Color.DarkBlue);
            vertices[5] = new VertexPositionColor(Vector3.UnitZ, Color.DarkBlue);

            //to do - x-text , y-text, z-text
            //x label
            vertices[6] = new VertexPositionColor(new Vector3(1.1f, 0.1f, 0), Color.DarkRed);
            vertices[7] = new VertexPositionColor(new Vector3(1.3f, -0.1f, 0), Color.DarkRed);
            vertices[8] = new VertexPositionColor(new Vector3(1.3f, 0.1f, 0), Color.DarkRed);
            vertices[9] = new VertexPositionColor(new Vector3(1.1f, -0.1f, 0), Color.DarkRed);


            //y label
            vertices[10] = new VertexPositionColor(new Vector3(-0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[11] = new VertexPositionColor(new Vector3(0, 1.2f, 0), Color.DarkGreen);
            vertices[12] = new VertexPositionColor(new Vector3(0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[13] = new VertexPositionColor(new Vector3(-0.1f, 1.1f, 0), Color.DarkGreen);

            //z label
            vertices[14] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[15] = new VertexPositionColor(new Vector3(0, 0.1f, 1.3f), Color.DarkBlue);
            vertices[16] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[17] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[18] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[19] = new VertexPositionColor(new Vector3(0, -0.1f, 1.1f), Color.DarkBlue);


            return vertices;
        }

        //returns the vertices for a spiral with a user-defined segment angle, vertical increment and orientation centred around the origin
        public static VertexPositionColor[] GetWireframeSpiral(int segmentAngleInDegrees, float verticalIncrement, out PrimitiveType primitiveType, out int primitiveCount)
        {
            VertexPositionColor[] vertices = GetWireframeCircle(segmentAngleInDegrees, out primitiveType, out primitiveCount, OrientationType.XZAxis);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position.Y = verticalIncrement * i;
            }

            return vertices;
        }

        //returns the vertices for a circle with a user-defined segment angle and orientation centred around the origin
        public static VertexPositionColor[] GetWireframeCircle(int segmentAngleInDegrees, out PrimitiveType primitiveType, out int primitiveCount,
            OrientationType orientationType)
        {
            primitiveType = PrimitiveType.LineStrip;
            primitiveCount = 360 / segmentAngleInDegrees;

            VertexPositionColor[] vertices = new VertexPositionColor[primitiveCount + 1];

            Vector3 position = Vector3.Zero;
            float angleInRadians = MathHelper.ToRadians(segmentAngleInDegrees);

            for (int i = 0; i <= primitiveCount; i++)
            {
                if (orientationType == OrientationType.XYAxis)
                {
                    position.X = (float)(Math.Cos(i * angleInRadians));
                    position.Y = (float)(Math.Sin(i * angleInRadians));
                }
                else if (orientationType == OrientationType.XZAxis)
                {
                    position.X = (float)(Math.Cos(i * angleInRadians));
                    position.Z = (float)(Math.Sin(i * angleInRadians));
                }
                else
                {
                    position.Y = (float)(Math.Cos(i * angleInRadians));
                    position.Z = (float)(Math.Sin(i * angleInRadians));
                }

                vertices[i] = new VertexPositionColor(position, Color.White);
            }
            return vertices;
        }

        //returns the vertices for a simple sphere (i.e. 3 circles) with a user-defined sweep angle centred around the origin
        public static VertexPositionColor[] GetWireframeSphere(int segmentAngleInDegrees, out PrimitiveType primitiveType, out int primitiveCount)
        {
            List<VertexPositionColor> vertList = new List<VertexPositionColor>();

            vertList.AddRange(GetWireframeCircle(segmentAngleInDegrees, out primitiveType, out primitiveCount, OrientationType.XYAxis));
            vertList.AddRange(GetWireframeCircle(segmentAngleInDegrees, out primitiveType, out primitiveCount, OrientationType.YZAxis));
            vertList.AddRange(GetWireframeCircle(segmentAngleInDegrees, out primitiveType, out primitiveCount, OrientationType.XZAxis));
            primitiveCount = vertList.Count - 1;
            return vertList.ToArray();
        }

        /*************************************************************** Colored ***************************************************************/
        
        //returns the vertices for an equilateral triangle, with colors defined by the array, and centred around the origin
        public static VertexPositionColor[] GetColoredTriangle(Color[] vertexColorArray, out PrimitiveType primitiveType, out int primitiveCount)
        {
            if (vertexColorArray.Length != 3)
                throw new Exception("Your color array doesnt have enough colors to create this primitive");

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 1;

            float halfSideLength = 0.5f;
            float triangleHeight = (float)(halfSideLength * Math.Tan(MathHelper.ToRadians(60)));
            float triangleHalfHeight = triangleHeight / 2.0f;

            VertexPositionColor[] vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(new Vector3(-halfSideLength, -triangleHalfHeight, 0), vertexColorArray[0]); //Left
            vertices[1] = new VertexPositionColor(new Vector3(0, triangleHalfHeight, 0), vertexColorArray[1]); //Top
            vertices[2] = new VertexPositionColor(new Vector3(halfSideLength, -triangleHalfHeight, 0), vertexColorArray[2]); //Right

            return vertices;
        }

        //returns the vertices for an 1x1 quad, with colors defined by the array, and centred around the origin
        public static VertexPositionColor[] GetColoredQuad(Color[] vertexColorArray, out PrimitiveType primitiveType, out int primitiveCount)
        {
            if (vertexColorArray.Length != 4)
                throw new Exception("Your color array doesnt have enough colors to create this primitive");

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            VertexPositionColor[] vertices = new VertexPositionColor[4];

            float halfSideLength = 0.5f;

            Vector3 topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            Vector3 topRight = new Vector3(halfSideLength, halfSideLength, 0);
            Vector3 bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            Vector3 bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColor(topLeft, vertexColorArray[1]);
            vertices[1] = new VertexPositionColor(topRight, vertexColorArray[2]);
            vertices[2] = new VertexPositionColor(bottomLeft, vertexColorArray[0]);
            vertices[3] = new VertexPositionColor(bottomRight, vertexColorArray[3]);

            return vertices;
        }

        //returns the vertices for an 1x1x1 white cube centred around the origin
        //notice that the number of vertices (i.e. 36) is larger than necessary (i.e. 24) - we can use IndexedBufferedVertexData to reduce
        public static VertexPositionColor[] GetColoredCube()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[36];

            float halfSideLength = 0.5f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[1] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[2] = new VertexPositionColor(topRightBack, Color.White);

            vertices[3] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[4] = new VertexPositionColor(topRightBack, Color.White);
            vertices[5] = new VertexPositionColor(topRightFront, Color.White);

            //front
            vertices[6] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[7] = new VertexPositionColor(topRightFront, Color.White);
            vertices[8] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[9] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[10] = new VertexPositionColor(topRightFront, Color.White);
            vertices[11] = new VertexPositionColor(bottomRightFront, Color.White);

            //back
            vertices[12] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[13] = new VertexPositionColor(topRightBack, Color.White);
            vertices[14] = new VertexPositionColor(topLeftBack, Color.White);

            vertices[15] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[16] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[17] = new VertexPositionColor(bottomLeftBack, Color.White);

            //left 
            vertices[18] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[19] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[20] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[21] = new VertexPositionColor(bottomLeftBack, Color.White);
            vertices[22] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[23] = new VertexPositionColor(bottomLeftFront, Color.White);

            //right
            vertices[24] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[25] = new VertexPositionColor(topRightFront, Color.White);
            vertices[26] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[27] = new VertexPositionColor(topRightFront, Color.White);
            vertices[28] = new VertexPositionColor(topRightBack, Color.White);
            vertices[29] = new VertexPositionColor(bottomRightBack, Color.White);

            //bottom
            vertices[30] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[31] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[32] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[33] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[34] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[35] = new VertexPositionColor(bottomLeftBack, Color.White);

            return vertices;
        }

        /*************************************************************** Textured (unlit) ***************************************************************/

        //returns the vertices for an 1x1 textured quad centred around the origin
        public static VertexPositionColorTexture[] GetTexturedQuad(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
            float halfSideLength = 0.5f;

            Vector3 topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            Vector3 topRight = new Vector3(halfSideLength, halfSideLength, 0);
            Vector3 bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            Vector3 bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColorTexture(topLeft, Color.White, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(topRight, Color.White, Vector2.UnitX);
            vertices[2] = new VertexPositionColorTexture(bottomLeft, Color.White, Vector2.UnitY);
            vertices[3] = new VertexPositionColorTexture(bottomRight, Color.White, Vector2.One);

            return vertices;
        }

        //returns the vertices for an 1x1x1 textured cube centred around the origin
        public static VertexPositionColorTexture[] GetTexturedCube(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[36];

            float halfSideLength = 0.5f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);


            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[1] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[2] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);

            vertices[3] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[4] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[5] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);

            //front
            vertices[6] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[7] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);

            vertices[9] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[10] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);

            //back
            vertices[12] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[13] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);

            vertices[15] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[16] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[17] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            //left 
            vertices[18] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[19] = new VertexPositionColorTexture(topLeftFront, Color.White, uvTopRight);
            vertices[20] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            vertices[21] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);
            vertices[22] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[23] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            //right
            vertices[24] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvBottomLeft);
            vertices[25] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[26] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[27] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[28] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[29] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[31] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);
            vertices[32] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[33] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[34] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[35] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            return vertices;
        }

        //returns the vertices for a 4-sided base textured pyramid sitting on the XZ plane
        public static VertexPositionColorTexture[] GetTexturedPyramidSquare(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 6;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[18];
            float halfSideLength = 0.5f;

            Vector3 topCentre = new Vector3(0, 0.71f * halfSideLength * 2, 0); //multiplier gives a pyramid where the length of the rising edges == length of the base edges
            Vector3 frontLeft = new Vector3(-halfSideLength, 0, halfSideLength);
            Vector3 frontRight = new Vector3(halfSideLength, 0, halfSideLength);
            Vector3 backLeft = new Vector3(-halfSideLength, 0, -halfSideLength);
            Vector3 backRight = new Vector3(halfSideLength, 0, -halfSideLength);

            Vector2 uvTopCentre = new Vector2(0.5f, 0);
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);

            //front 
            vertices[0] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[1] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomRight);
            vertices[2] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomLeft);

            //left 
            vertices[3] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[4] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomRight);
            vertices[5] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            //right 
            vertices[6] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[7] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomLeft);

            //back 
            vertices[9] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[10] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(backRight, Color.White, uvBottomLeft);

            //bottom 
            vertices[12] = new VertexPositionColorTexture(frontLeft, Color.White, uvTopLeft);
            vertices[13] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            vertices[15] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[16] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[17] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            return vertices;
        }


        /*************************************************************** Normal Textured (lit) ***************************************************************/

        //try to create a quad that can be lit with the directional lights of the BasicEffect...

        //adding normals - step 1 - add the vertices for the object shape
        public static VertexPositionNormalTexture[] GetNormalTexturedCube(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[36];

            float halfSideLength = 0.5f;

            Vector3 topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            Vector3 topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            Vector3 topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            Vector3 topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            Vector3 bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            Vector3 bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            Vector3 bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);


            //top - 1 polygon for the top
            vertices[0] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[1] = new VertexPositionNormalTexture(topLeftBack, Vector3.UnitY, uvTopLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);

            vertices[3] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitY, uvBottomRight);

            //front
            vertices[6] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitZ, uvBottomLeft);
            vertices[7] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[8] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);

            vertices[9] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);
            vertices[10] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[11] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitZ, uvTopRight);

            //back
            vertices[12] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, -Vector3.UnitZ, uvTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);

            vertices[15] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[16] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);
            vertices[17] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitZ, uvBottomLeft);

            //left 
            vertices[18] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[19] = new VertexPositionNormalTexture(topLeftFront, -Vector3.UnitX, uvTopRight);
            vertices[20] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            vertices[21] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitX, uvBottomLeft);
            vertices[22] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[23] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            //right
            vertices[24] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitX, uvBottomLeft);
            vertices[25] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[26] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            vertices[27] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[28] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitX, uvTopRight);
            vertices[29] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[31] = new VertexPositionNormalTexture(bottomRightFront, -Vector3.UnitY, uvTopRight);
            vertices[32] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);

            vertices[33] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[34] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);
            vertices[35] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitY, uvBottomLeft);

            return vertices;
        }

        public static VertexPositionNormalTexture[] GetNormalTexturedSphere(out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            Vector2 uvTopLeft = new Vector2(0, 0);
            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomLeft = new Vector2(0, 1);
            Vector2 uvBottomRight = new Vector2(1, 1);


            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[12];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(-0.25f, 0f, 0.5f), Vector3.Normalize(new Vector3(-0.25f, 0f, 0.5f)), uvTopLeft);
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0.25f, 0f, 0.5f), Vector3.Normalize(new Vector3(0.25f, 0f, 0.5f)), uvBottomRight);
            vertices[2] = new VertexPositionNormalTexture(new Vector3(-0.25f, 0f, -0.5f), Vector3.Normalize(new Vector3(-0.25f, 0f, -0.5f)), uvBottomLeft);

            vertices[3] = new VertexPositionNormalTexture(new Vector3(0.25f, 0f, -0.5f), Vector3.Normalize(new Vector3(0.25f, 0f, -0.5f)), uvTopLeft);
            vertices[4] = new VertexPositionNormalTexture(new Vector3(0f, 0.5f, 0.25f), Vector3.Normalize(new Vector3(0f, 0.5f, 0.25f)), uvBottomRight);
            vertices[5] = new VertexPositionNormalTexture(new Vector3(0f, 0.5f, -0.25f), Vector3.Normalize(new Vector3(0f, 0.5f, -0.25f)), uvBottomLeft);

            vertices[6] = new VertexPositionNormalTexture(new Vector3(0f, -0.5f, 0.25f), Vector3.Normalize(new Vector3(0f, -0.5f, 0.25f)), uvTopLeft);
            vertices[7] = new VertexPositionNormalTexture(new Vector3(0f, -0.5f, -0.25f), Vector3.Normalize(new Vector3(0f, -0.5f, -0.25f)), uvBottomRight);
            vertices[8] = new VertexPositionNormalTexture(new Vector3(0.5f, 0.25f, 0f), Vector3.Normalize(new Vector3(0.5f, 0.25f, 0f)), uvBottomLeft);

            vertices[9] = new VertexPositionNormalTexture(new Vector3(-0.5f, 0.25f, 0f), Vector3.Normalize(new Vector3(-0.5f, 0.25f, 0f)), uvTopLeft);
            vertices[10] = new VertexPositionNormalTexture(new Vector3(0.5f, -0.25f, 0f), Vector3.Normalize(new Vector3(0.5f, -0.25f, 0f)), uvBottomRight);
            vertices[11] = new VertexPositionNormalTexture(new Vector3(-0.5f, -0.25f, 0f), Vector3.Normalize(new Vector3(-0.5f, -0.25f, 0f)), uvBottomLeft);

            return vertices;
        }

        public static short[] GetSphereIndices()
        {
            short[] indices = new short[60];
            indices[0] = 0; indices[1] = 6; indices[2] = 1;
            indices[3] = 0; indices[4] = 11; indices[5] = 6;
            indices[6] = 1; indices[7] = 4; indices[8] = 0;
            indices[9] = 1; indices[10] = 8; indices[11] = 4;
            indices[12] = 1; indices[13] = 10; indices[14] = 8;
            indices[15] = 2; indices[16] = 5; indices[17] = 3;
            indices[18] = 2; indices[19] = 9; indices[20] = 5;
            indices[21] = 2; indices[22] = 11; indices[23] = 9;
            indices[24] = 3; indices[25] = 7; indices[26] = 2;
            indices[27] = 3; indices[28] = 10; indices[29] = 7;
            indices[30] = 4; indices[31] = 8; indices[32] = 5;
            indices[33] = 4; indices[34] = 9; indices[35] = 0;
            indices[36] = 5; indices[37] = 8; indices[38] = 3;
            indices[39] = 5; indices[40] = 9; indices[41] = 4;
            indices[42] = 6; indices[43] = 10; indices[44] = 1;
            indices[45] = 6; indices[46] = 11; indices[47] = 7;
            indices[48] = 7; indices[49] = 10; indices[50] = 6;
            indices[51] = 7; indices[52] = 11; indices[53] = 2;
            indices[54] = 8; indices[55] = 10; indices[56] = 3;
            indices[57] = 9; indices[58] = 11; indices[59] = 0;

            return indices;
        }

        public static VertexPositionColor[] getColoredSphere(out PrimitiveType primitiveType, out int primitiveCount)
        {
            Random random = new Random();
            float radius = 0.5f;
            int nvertices = 90 * 90; // 90 vertices in a circle, 90 circles in a sphere
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 90 * 90 * 3;
            Color[] vertexColorArray = { Color.Red, Color.Green, Color.Blue, Color.Orange };
            VertexPositionColor[]  vertices = new VertexPositionColor[nvertices];
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 rad = new Vector3((float)Math.Abs(radius), 0, 0);
            Color c = vertexColorArray[random.Next(0, 3)];
            for (int x = 0; x < 90; x++) //90 circles, difference between each is 4 degrees
            {
                float difx = 360.0f / 90.0f;
                for (int y = 0; y < 90; y++) //90 veritces, difference between each is 4 degrees 
                {
                    float dify = 360.0f / 90.0f;
                    Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify));// rotate vertex around z
                    Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx));// rotate circle around y
                    Vector3 point = Vector3.Transform(Vector3.Transform(rad, zrot), yrot);//transformation
                   
                    vertices[x + y * 90] = new VertexPositionColor(point, Color.White);
                }
            }

            return vertices;
        }

        public static short[] GetColoredSphereIndices()
        {

            int nindices = 90 * 90 * 6;
            short[] indices = new short[nindices];
            int i = 0;
            for (int x = 0; x < 90; x++)
            {
                for (int y = 0; y < 90; y++)
                {
                    int s1 = x == 89 ? 0 : x + 1;
                    int s2 = y == 89 ? 0 : y + 1;
                    short upperLeft = (short)(x * 90 + y);
                    short upperRight = (short)(s1 * 90 + y);
                    short lowerLeft = (short)(x * 90 + s2);
                    short lowerRight = (short)(s1 * 90 + s2);
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
            }

            return indices;
        }
    }
}
