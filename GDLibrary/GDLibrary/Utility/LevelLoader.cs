using Microsoft.Xna.Framework;
/*
Function: 		Allows me to load the level by reading pixel values from a PNG file and instanciating Actor3D objects (primitive objects, collidable primitive objects, zone objects) based on the 
                mappings provided in colorToPrimitiveObjectDictionary.

                Notice that I treat colors coming from the PNG file as RGB and NOT RGBA this is to prevent any issues where you accidentally set the alpha channel to a value != 255.

Author: 		NMCG
Version:		1.1
Date Updated:	9/12/17
Bugs:			None
Fixes:			None
Mods:           None
*/

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDLibrary
{
    public class LevelLoader
    {
        #region Fields
        private Texture2D levelTexture;
        private Dictionary<Vector3, Actor3D> colorToActorDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        #endregion

        #region Properties
        public Texture2D LevelTexture
        {
            get
            {
                return this.levelTexture;
            }
        }
        public Dictionary<Vector3, Actor3D> ColorToActorDictionary
        {
            get
            {
                return this.colorToActorDictionary;
            }
        }
        public ContentDictionary<Texture2D> TextureDictionary
        {
            get
            {
                return this.textureDictionary;
            }
        }
        #endregion

        public LevelLoader(ContentDictionary<Texture2D> textureDictionary, string levelTextureName)
        {
            //contains the pixel data that generate the level primitives
            this.levelTexture = textureDictionary[levelTextureName];

            //create dictionary filled with color, primitive mappings to be used when reading color value from the level texture
            this.colorToActorDictionary = new Dictionary<Vector3, Actor3D>();

            //reference to game textures in case we want to draw the same primitive object but with a different texture
            this.textureDictionary = textureDictionary;
        }

        //when setting up the level loader we create these mappings to say what to do when we find a particular pixel color
        public void AddColorActor3DPair(Color color, Actor3D actor)
        {
            if(!this.colorToActorDictionary.ContainsKey(color.ToVector3()))
            {
                this.colorToActorDictionary.Add(color.ToVector3(), actor);
            }
        }

        //reads through each pixel in a PNG texture and generates an object based on the users GetActor3DFromColor() method and colorToActorDictionary mappings
        public List<Actor3D> Process(Vector2 scaleOnXZPlane, float yAxisHeight, Vector3 worldOffset)
        {
            if (this.colorToActorDictionary.Count == 0)
                throw new System.Exception("You haven't set up your <color, actor> mappings in the colorToActorDictionary yet by calling AddColorActor3DPair()");

            List<Actor3D> list = new List<Actor3D>();
            Color[] colorData = new Color[this.levelTexture.Height * this.levelTexture.Width];
            this.levelTexture.GetData(colorData);

            Vector3 colorAsVector3; 
            Vector3 position;
            Actor3D actor;

            for (int y = 0; y < this.levelTexture.Height; y++)
            {
                for (int x = 0; x < this.levelTexture.Width; x++)
                {
                    colorAsVector3 = colorData[x + y * this.levelTexture.Width].ToVector3();

                    if (colorToActorDictionary.ContainsKey(colorAsVector3))
                    {
                        //scale allows us to increase the separation between objects in the XZ plane
                        position = new Vector3(x * scaleOnXZPlane.X, yAxisHeight, y * scaleOnXZPlane.Y);

                        //offset allows us to shift the whole set of objects in X, Y, and Z      
                        position += worldOffset;

                        actor = GetActor3DFromColor(colorAsVector3, position);

                        if (actor != null)
                            list.Add(actor);
                    }
                } //end for x
            } //end for y
            return list;
        }

        //returns a reference to a new actor based on the current pixel color and the if...else logic of this method - see MyLevelLoader::GetActorFromPixelColor()
        private Actor3D GetActor3DFromColor(Vector3 colorAsVector3, Vector3 position)
        {
            Actor3D clonedActor = this.colorToActorDictionary[colorAsVector3].Clone() as Actor3D;
            clonedActor.Transform.Translation = position;
            return clonedActor;
        }

    }
}
