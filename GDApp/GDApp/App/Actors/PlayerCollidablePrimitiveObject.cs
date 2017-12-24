﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    public class PlayerCollidablePrimitiveObject : CollidablePrimitiveObject
    {
        #region Fields
        private float moveSpeed, rotationSpeed;
        private Keys[] moveKeys;
        private bool bThirdPersonZoneEventSent;
        private ManagerParameters managerParameters;

        private Vector3 acceleration;
        #endregion

        #region Properties
        #endregion

        public PlayerCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
            StatusType statusType, IVertexData vertexData, ICollisionPrimitive collisionPrimitive, 
            ManagerParameters managerParameters,
            Keys[] moveKeys, float moveSpeed, float rotationSpeed) 
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, managerParameters.ObjectManager)
        {
            this.moveKeys = moveKeys;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
            this.acceleration = new Vector3(0,0,0);
            //for input
            this.managerParameters = managerParameters;
        }

        //used to make a player collidable primitives from an existing PrimitiveObject (i.e. the type returned by the PrimitiveFactory
        public PlayerCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive,
                                ManagerParameters managerParameters, Keys[] moveKeys, float moveSpeed, float rotationSpeed)
            : base(primitiveObject, collisionPrimitive, managerParameters.ObjectManager)
        {
            this.moveKeys = moveKeys;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;

            //for input
            this.managerParameters = managerParameters;
        }


        public override void Update(GameTime gameTime)
        {
            //read any input and store suggested increments
            HandleInput(gameTime);
            HandleAcceleration(gameTime);

          // ApplyGravity(gameTime);

            //have we collided with something?
            this.Collidee = CheckCollisions(gameTime);

            //how do we respond to this collidee e.g. pickup?
            HandleCollisionResponse(this.Collidee);

            //if no collision then move - see how we set this.Collidee to null in HandleCollisionResponse() 
            //below when we hit against a zone
            if (this.Collidee == null)
                ApplyInput(gameTime);

            //reset translate and rotate and update primitive
            base.Update(gameTime);
        }

        //this is where you write the application specific CDCR response for your game
        protected override void HandleCollisionResponse(Actor collidee)
        {
            if(collidee is SimpleZoneObject)
            {
                if (collidee.ID.Equals(AppData.SwitchToThirdPersonZoneID))
                {
                    if (!bThirdPersonZoneEventSent) //add a boolean to stop the event being sent multiple times!
                    {
                        //publish some sort of event - maybe an event to switch the camera?
                        object[] additionalParameters = { AppData.ThirdPersonCameraID };
                        EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalParameters));
                        bThirdPersonZoneEventSent = true;
                    }

                    //setting this to null means that the ApplyInput() method will get called and the player can move through the zone.
                    this.Collidee = null;
                }
            }
            else if(collidee is CollidablePrimitiveObject)
            {
                if (collidee.ActorType == ActorType.CollidableDecorator)
                {
                    //we dont HAVE to do anything here but lets change its color just to see something happen
                    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Yellow;
                }

                //decide what to do with the thing you've collided with
                else if (collidee.ActorType == ActorType.CollidableAmmo)
                {
                    //do stuff...maybe a remove
                    EventDispatcher.Publish(new EventData(collidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                }

                //activate some/all of the controllers when we touch the object
                else if (collidee.ActorType == ActorType.CollidableActivatable)
                {
                    //when we touch get a particular controller to start
                   // collidee.SetAllControllers(PlayStatusType.Play, x => x.GetControllerType().Equals(ControllerType.SineColorLerp));

                    //when we touch get a particular controller to start
                    collidee.SetAllControllers(PlayStatusType.Play, x => x.GetControllerType().Equals(ControllerType.PickupDisappear));
                }
                else if (collidee.ActorType == ActorType.CollidableEnemy)
                {
                    this.acceleration = -this.acceleration;
                }
                else if (collidee.ActorType == ActorType.CollidableGround)
                {
                    //this.Collidee = null;
                }
            }
        }

        protected void HandleAcceleration(GameTime gameTime)
        {
           this.Transform.TranslateIncrement = this.acceleration;
           // this.Transform.TranslateBy(this.acceleration);
        }

        protected void ApplyGravity(GameTime gameTime)
        {
            //Gravity
                this.Transform.TranslateIncrement
        = -this.Transform.Up * gameTime.ElapsedGameTime.Milliseconds
                * 0.02f;
        }

        protected override void HandleInput(GameTime gameTime)
        {

            if(this.managerParameters.KeyboardManager.IsAnyKeyPressed())
            {
           
            if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveForward])) //Forward
                {
                    //this.Transform.TranslateIncrement
                    //    = this.Transform.Look * gameTime.ElapsedGameTime.Milliseconds
                    //            * this.moveSpeed;

                    //this.Transform.TranslateBy(new Vector3(0, 0, -this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds));

                   this.acceleration += new Vector3(0, 0, -this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds);
            }
            else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveBackward])) //Backward
            {
                    //this.Transform.TranslateIncrement
                    //   = -this.Transform.Look * gameTime.ElapsedGameTime.Milliseconds
                    //           * this.moveSpeed;

                    this.acceleration += (new Vector3(0, 0, this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds));
                }

                if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateLeft])) //Left
            {
                //this.Transform.RotateIncrement = gameTime.ElapsedGameTime.Milliseconds * this.rotationSpeed;
                //             this.Transform.TranslateIncrement
                //= -this.Transform.Right * gameTime.ElapsedGameTime.Milliseconds
                //        * this.moveSpeed;

                this.acceleration += (new Vector3(-this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds, 0, 0));
            }
            else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateRight])) //Right
            {
               // this.Transform.RotateIncrement = -gameTime.ElapsedGameTime.Milliseconds * this.rotationSpeed;

                //         this.Transform.TranslateIncrement
                //= this.Transform.Right * gameTime.ElapsedGameTime.Milliseconds
                //        * this.moveSpeed;

                this.acceleration += (new Vector3(this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds, 0, 0));
            }
            }
            else
            {
                //Else No input is pressed It will graduly decelerate the player
                CalculateVelocity(gameTime);
            }


        }
        
        protected void CalculateVelocity(GameTime gameTime)
        {
            //Get Players Velocity in X and Y direction
            float XVelocity = this.acceleration.X;
            float ZVelocity = this.acceleration.Z;

            //First Check Z Velocity + or - for the direction
            //then gradualy slow down by adding the opposite direction, if it is nearly 0, set to 0 to stop
            if (ZVelocity < -0.05)
            {
                ZVelocity += this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (ZVelocity > 0.05)
            {
                ZVelocity -= this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                ZVelocity = 0;
            }

            //Same as above for X direction
            if (XVelocity < -0.05)
            {

                XVelocity += this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (XVelocity > 0.05)
            {
                XVelocity -= this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                XVelocity = 0;
            }

            //Finaly set the new velocity
            this.acceleration = (new Vector3(XVelocity, 0, ZVelocity));
        }
    }
}
