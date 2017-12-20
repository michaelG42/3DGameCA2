/*
Function: 		Renders the collision skins of any ICollisionPrimitives used in the I-CA project.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			
Fixes:			None
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    //Draws the bounding volume for your primitive objects
    public class PrimitiveDebugDrawer : PausableDrawableGameComponent
    {
        #region Statics
        //set your desired CDCR surface colors
        private static Color sphereColor = Color.Yellow;
        private static Color boxColor = sphereColor;
        private static Color frustumSphereColor = Color.Gray;
        #endregion

        #region Fields
        private ManagerParameters managerParameters;
        private BasicEffect wireframeEffect;
        private bool bShowCollisionSkins, bShowFrustumCullingSphere, bShowZones;

        //temp vars
        private Matrix world;
        private IVertexData sphereVertexData;
        #endregion


        #region Properties
        public bool ShowCollisionSkins
        {
            get
            {
                return this.bShowCollisionSkins;
            }
            set
            {
                this.bShowCollisionSkins = value;
            }
        }

        public bool ShowZones
        {
            get
            {
                return this.bShowZones;
            }
            set
            {
                this.bShowZones = value;
            }

        }

        public bool ShowFrustumCullingSphere
        {
            get
            {
                return this.bShowFrustumCullingSphere;
            }
            set
            {
                this.bShowFrustumCullingSphere = value;
            }
        }
        #endregion

        public PrimitiveDebugDrawer(Game game, EventDispatcher eventDispatcher, StatusType statusType, 
            ManagerParameters managerParameters, bool bShowCollisionSkins, bool bShowFrustumCullingSphere, bool bShowZones, 
            IVertexData sphereVertexData)
            : base(game, eventDispatcher, statusType)
        {
            this.managerParameters = managerParameters;
            this.bShowCollisionSkins = bShowCollisionSkins;
            this.bShowFrustumCullingSphere = bShowFrustumCullingSphere;
            this.bShowZones = bShowZones;

            //used to draw the default BoundingSphere for any primitive and the collision skin for any primitive with a sphere collision primitive
            this.sphereVertexData = sphereVertexData;
        }

        #region Event Handling
        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.DebugChanged += EventDispatcher_DebugChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        //See MenuManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
                //turn on update and draw i.e. hide the menu
                this.StatusType = StatusType.Update | StatusType.Drawn;
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //turn off update and draw i.e. show the menu since the game is paused
                this.StatusType = StatusType.Off;
            }
        }

        //enable dynamic show/hide of debug info
        private void EventDispatcher_DebugChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnToggleDebug)
            {
                if (this.StatusType == StatusType.Off)
                    this.StatusType = StatusType.Drawn | StatusType.Update;
                else
                    this.StatusType = StatusType.Off;
            }
        }
        #endregion

        public override void Initialize()
        {
            //used to draw bounding volumes
            this.wireframeEffect = new BasicEffect(this.Game.GraphicsDevice);
            this.wireframeEffect.VertexColorEnabled = true;
            base.Initialize();
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //set so we dont see the bounding volume through the object is encloses - disable to see result
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (IActor actor in this.managerParameters.ObjectManager.OpaqueDrawList)
            {
                DrawSurfaceOrZonePrimitive(gameTime, actor);
                DrawFrustumCullingSphere(gameTime, actor);
            }
            foreach (IActor actor in this.managerParameters.ObjectManager.TransparentDrawList)
            {
                DrawSurfaceOrZonePrimitive(gameTime, actor);
                DrawFrustumCullingSphere(gameTime, actor);
            }

        }

        private void DrawFrustumCullingSphere(GameTime gameTime, IActor actor)
        {
            if(this.bShowFrustumCullingSphere)
            {
                Actor3D actor3D = actor as Actor3D;
                world = Matrix.Identity 
                    * Matrix.CreateScale(actor3D.BoundingSphere.Radius) 
                    * Matrix.CreateTranslation(actor3D.BoundingSphere.Center);
                this.wireframeEffect.World = world;
                this.wireframeEffect.View = this.managerParameters.CameraManager.ActiveCamera.View;
                this.wireframeEffect.Projection = this.managerParameters.CameraManager.ActiveCamera.ProjectionParameters.Projection;
                this.wireframeEffect.DiffuseColor = frustumSphereColor.ToVector3();
                this.wireframeEffect.CurrentTechnique.Passes[0].Apply();
                sphereVertexData.Draw(gameTime, this.wireframeEffect);
            }
        }

        private void DrawSurfaceOrZonePrimitive(GameTime gameTime, IActor actor)
        {      
            if (actor is SimpleZoneObject)
            {
                if(this.bShowZones)
                    DrawCollisionPrimitive(gameTime, (actor as SimpleZoneObject).CollisionPrimitive);
            }
            else if(actor is CollidablePrimitiveObject)
            {
                if (this.bShowCollisionSkins)
                    DrawCollisionPrimitive(gameTime, (actor as CollidablePrimitiveObject).CollisionPrimitive);
            }
        }

        private void DrawCollisionPrimitive(GameTime gameTime, ICollisionPrimitive collisionPrimitive)
        {      
            if (collisionPrimitive is SphereCollisionPrimitive)
            {
                SphereCollisionPrimitive coll = collisionPrimitive as SphereCollisionPrimitive;
                this.wireframeEffect.World = Matrix.Identity * Matrix.CreateScale(coll.BoundingSphere.Radius) * Matrix.CreateTranslation(coll.BoundingSphere.Center); 
                this.wireframeEffect.View = this.managerParameters.CameraManager.ActiveCamera.View;
                this.wireframeEffect.Projection = this.managerParameters.CameraManager.ActiveCamera.ProjectionParameters.Projection;
                this.wireframeEffect.DiffuseColor = sphereColor.ToVector3();
                this.wireframeEffect.CurrentTechnique.Passes[0].Apply();
                sphereVertexData.Draw(gameTime, this.wireframeEffect);
            }
            else
            {
                BoxCollisionPrimitive coll = collisionPrimitive as BoxCollisionPrimitive;
                BoundingBoxBuffers buffers = BoundingBoxDrawer.CreateBoundingBoxBuffers(coll.BoundingBox, this.GraphicsDevice, boxColor);
                BoundingBoxDrawer.DrawBoundingBox(buffers, this.wireframeEffect, this.GraphicsDevice, this.managerParameters.CameraManager.ActiveCamera);
            }
        }

        
    }
}
