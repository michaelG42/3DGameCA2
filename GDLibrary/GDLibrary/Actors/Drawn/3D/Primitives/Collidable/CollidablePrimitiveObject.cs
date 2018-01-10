using GDLibrary.Enums;
using Microsoft.Xna.Framework;
using System;

namespace GDLibrary
{
    public class CollidablePrimitiveObject : PrimitiveObject
    {
        #region Variables
        //the skin used to wrap the object
        private ICollisionPrimitive collisionPrimitive;

        private GameState gameState;
        //the object that im colliding with
        private Actor collidee;
        private ObjectManager objectManager;
        private Vector3 velocity, collisionVector;

        private EventDispatcher eventDispatcher;
        #endregion

        #region Properties
        //returns a reference to whatever this object is colliding against
        public Actor Collidee
        {
            get
            {
                return collidee;
            }
            set
            {
                collidee = value;
            }
        }
        public ICollisionPrimitive CollisionPrimitive
        {
            get
            {
                return collisionPrimitive;
            }
            set
            {
                collisionPrimitive = value;
            }
        }
        public ObjectManager ObjectManager
        {
            get
            {
                return this.objectManager;
            }
        }

        public Vector3 Velocity { get => velocity; set => velocity = value; }
        public Vector3 CollisionVector { get => collisionVector; set => collisionVector = value; }
        public GameState GameState { get => GameState1; set => GameState1 = value; }
        public GameState GameState1 { get => gameState; set => gameState = value; }

        #endregion

        //used to draw collidable primitives that have a texture i.e. use VertexPositionColor vertex types only
        public CollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData,
             ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, EventDispatcher eventDispatcher)
            : base(id, actorType, transform, effectParameters, statusType, vertexData)
        {
            this.collisionPrimitive = collisionPrimitive;
            //unusual to pass this in but we use it to test for collisions - see Update();
            this.objectManager = objectManager;
            this.velocity = new Vector3(0, 0, 0);
            this.GameState = GameState.NotStarted;
            this.eventDispatcher = eventDispatcher;
            RegisterForEventHandling(eventDispatcher);
        }

        //used to make a collidable primitives from an existing PrimitiveObject (i.e. the type returned by the PrimitiveFactory
        public CollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, EventDispatcher eventDispatcher)
            : base(primitiveObject.ID, primitiveObject.ActorType, primitiveObject.Transform, primitiveObject.EffectParameters,
                  primitiveObject.StatusType, primitiveObject.VertexData)
        {
            this.collisionPrimitive = collisionPrimitive;
            //unusual to pass this in but we use it to test for collisions - see Update();
            this.objectManager = objectManager;
            this.velocity = new Vector3(0, 0, 0);
            this.GameState = GameState.NotStarted;
            this.eventDispatcher = eventDispatcher;
            RegisterForEventHandling(eventDispatcher);
        }

        protected void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.GameStateChanged += EventDispatcher_GameStateChanged;
        }
        protected void EventDispatcher_GameStateChanged(EventData eventData)
        {

            this.GameState = (GameState)Enum.Parse(typeof(GameState), eventData.AdditionalParameters[0].ToString());
        }


        public override void Update(GameTime gameTime)
        {
            //reset collidee to prevent colliding with the same object in the next update
            collidee = null;

            //reset any movements applied in the previous update from move keys
            this.Transform.TranslateIncrement = Vector3.Zero;
            this.Transform.RotateIncrement = 0;

            //update collision primitive with new object position
            if (collisionPrimitive != null)
                collisionPrimitive.Update(gameTime, this.Transform);

            base.Update(gameTime);
        }

        //read and store movement suggested by keyboard input
        protected virtual void HandleInput(GameTime gameTime)
        {

        }

        //define what happens when a collision occurs
        protected virtual void HandleCollisionResponse(Actor collidee)
        {

        }

        //test for collision against all opaque and transparent objects
        protected virtual Actor CheckCollisions(GameTime gameTime)
        {
           
            foreach (IActor actor in this.objectManager.OpaqueDrawList)
            {
                collidee = CheckCollisionWithActor(gameTime, actor as Actor3D);
                if (collidee != null)
                    return collidee;
            }

            foreach (IActor actor in this.objectManager.TransparentDrawList)
            {
                collidee = CheckCollisionWithActor(gameTime, actor as Actor3D);
                if (collidee != null)
                    return collidee;
            }

            return null;
        }

        //test for collision against a specific object
        private Actor CheckCollisionWithActor(GameTime gameTime, Actor3D actor3D)
        {

            //dont test for collision against yourself - remember the player is in the object manager list too!
            if (this != actor3D)
            {
                if(this.GameState == GameState.Level1)
                {
                    if (actor3D is CollidablePrimitiveObject && actor3D.ActorType != ActorType.CollidableGround)
                    {
                        CollidablePrimitiveObject collidableObject = actor3D as CollidablePrimitiveObject;
                        if (this.CollisionPrimitive.Intersects(collidableObject.CollisionPrimitive, this.Transform.TranslateIncrement))
                            return collidableObject;
                    }
                    else if (actor3D is SimpleZoneObject)
                    {
                        SimpleZoneObject zoneObject = actor3D as SimpleZoneObject;
                        if (this.CollisionPrimitive.Intersects(zoneObject.CollisionPrimitive, this.Transform.TranslateIncrement))
                            return zoneObject;
                    }
                }
                else
                {
                    if (actor3D is CollidablePrimitiveObject)
                    {
                        CollidablePrimitiveObject collidableObject = actor3D as CollidablePrimitiveObject;
                        if (this.CollisionPrimitive.Intersects(collidableObject.CollisionPrimitive, this.Transform.TranslateIncrement))
                            return collidableObject;
                    }
                    else if (actor3D is SimpleZoneObject)
                    {
                        SimpleZoneObject zoneObject = actor3D as SimpleZoneObject;
                        if (this.CollisionPrimitive.Intersects(zoneObject.CollisionPrimitive, this.Transform.TranslateIncrement))
                            return zoneObject;
                    }
                }

            }

            return null;
        }

        //apply suggested movement since no collision will occur if the player moves to that position
        protected virtual void ApplyInput(GameTime gameTime)
        {
            //was a move/rotate key pressed, if so then these values will be > 0 in dimension
            if (this.Transform.TranslateIncrement != Vector3.Zero)
                this.Transform.TranslateBy(this.Transform.TranslateIncrement);

            if (this.Transform.RotateIncrement != 0)
                this.Transform.RotateAroundYBy(this.Transform.RotateIncrement);
        }

        public override object GetDeepCopy()
        {
            CollidablePrimitiveObject actor = new CollidablePrimitiveObject("clone - " + ID, //deep
                      this.ActorType, //deep
                      (Transform3D)this.Transform.Clone(), //deep
                      (EffectParameters)this.EffectParameters.Clone(), //deep
                      this.StatusType, //deep
                      this.VertexData, //shallow - its ok if objects refer to the same vertices
                      (ICollisionPrimitive)this.CollisionPrimitive.Clone(), //deep
                      this.objectManager,
                      this.eventDispatcher); //shallow - reference


            if (this.ControllerList != null)
            {
                //clone each of the (behavioural) controllers
                foreach (IController controller in this.ControllerList)
                    actor.AttachController((IController)controller.Clone());
            }

            return actor;
        }

        public new object Clone()
        {
            return GetDeepCopy();
        }
    }
}
