using GDLibrary.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GDLibrary
{
    public class PlayerCollidablePrimitiveObject : CollidablePrimitiveObject
    {
        #region Fields
        private float accelerationSpeed;
        private float gravity;
        private int currentDirection;
        private int previousDirection;
        private Keys[] moveKeys;
        private bool bThirdPersonZoneEventSent;
        private ManagerParameters managerParameters;

        private Vector3 initialPosition, previousPosition, currentPosition, accelerationVector;//, collisionVector;

        private bool positionsInitialized;
        private bool initialPosSet;
        private bool inGame;


        //private Vector3 acceleration;

        //public Vector3 Acceleration { get => acceleration; set => acceleration = value; }
        #endregion

        #region Properties

        #endregion

        public PlayerCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
            StatusType statusType, IVertexData vertexData, ICollisionPrimitive collisionPrimitive,
            ManagerParameters managerParameters,
            Keys[] moveKeys, float accelerationSpeed)
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, managerParameters.ObjectManager)
        {
            this.moveKeys = moveKeys;
            this.accelerationSpeed = accelerationSpeed;
            //for input
            this.managerParameters = managerParameters;
            this.positionsInitialized = false;
            this.initialPosSet = false;
            this.currentDirection = 0;
            this.CollisionVector = Vector3.Zero;
            this.gravity = 0;
            this.inGame = true;
            //this.accelerationVector = Vector3.Zero;
        }

        //used to make a player collidable primitives from an existing PrimitiveObject (i.e. the type returned by the PrimitiveFactory
        public PlayerCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive,
                                ManagerParameters managerParameters, Keys[] moveKeys, float accelerationSpeed)
            : base(primitiveObject, collisionPrimitive, managerParameters.ObjectManager)
        {
            this.moveKeys = moveKeys;
            //this.accelerationSpeed = accelerationSpeed;
            this.accelerationSpeed = accelerationSpeed;
            //for input
            this.managerParameters = managerParameters;
            this.positionsInitialized = false;
            this.initialPosSet = false;
            this.currentDirection = 0;
            this.CollisionVector = Vector3.Zero;
            this.gravity = 0;
            this.inGame = true;
            //this.accelerationVector = Vector3.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            if (!positionsInitialized)
            {
                InitalizePositions();
                this.positionsInitialized = true;
            }

            this.currentPosition = this.Transform.Translation;
            this.currentDirection = getCurrentDirection();
            //read any input and store suggested increments
            if(inGame)
            {
                if (this.ActorType == ActorType.Player)
                {
                    //If It is A Human Player get Input from Keyboard
                    HandleInput(gameTime);
                }
                else
                {
                    //Else its AI Call Move to Calculate Velocity etc
                    Move(gameTime);
                }
            }
            else
            {
                Stop(gameTime);
            }


            //Adds Acceleration to the current velocity
            //ApplyGravity();
            HandleAcceleration(gameTime);

            

            //have we collided with something?
            this.Collidee = CheckCollisions(gameTime);

            //how do we respond to this collidee e.g. pickup?
            HandleCollisionResponse(this.Collidee);

            //if no collision then move - see how we set this.Collidee to null in HandleCollisionResponse() 
            if (this.Collidee == null)
            {
                //Collision Vector is applied to Velosity when we collide With an Enemy
                this.CollisionVector = Vector3.Zero;
                ApplyInput(gameTime);
            }

            //reset translate and rotate and update primitive
            base.Update(gameTime);
            //Keeps Track of the position and Direction from the last frame
            //Used for calculating velocity and if directoin has changed
            this.previousPosition = this.currentPosition;
            this.previousDirection = this.currentDirection;

            Console.WriteLine("Current Pos is " + this.currentPosition);
        }

        protected void InitalizePositions()
        {
            // need to keep track of various positions
            // current position and previous position is used for calculating current velocity
            // Initial position is set whenever AI changes direction
            // It is used to calculate when to decelerate so we dont overshoot a target.
            this.initialPosition = this.currentPosition = this.previousPosition = this.Transform.Translation;
        }

        protected override void HandleCollisionResponse(Actor collidee)
        {
            if (collidee is SimpleZoneObject)
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
            else if (collidee is CollidablePrimitiveObject)
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
                else if (collidee.ActorType == ActorType.CollidableEnemy || collidee.ActorType == ActorType.Player)
                {
                    //there was a Problem where if a player wasnt moving it wouldnt detect collsion
                    //Solved by setting the other players collisionVelocity, rather than this.collisionVelocity
                    (collidee as CollidablePrimitiveObject).CollisionVector = CalculateCollisionVector((collidee as CollidablePrimitiveObject).Velocity);

                    //setting the enemys acceleration vector to -this.acceleration vector means the AI will alway accelerate towards the collision
                    //this is what most human players would do to avoid being sent flying off really far
                    (collidee as PlayerCollidablePrimitiveObject).accelerationVector = -CalculateCollisionVector((collidee as CollidablePrimitiveObject).Velocity);
                }
                else if (collidee.ActorType == ActorType.CollidableGround)
                {
                    //this.Collidee = null;
                }
            }
        }

        protected void HandleAcceleration(GameTime gameTime)
        {
            //Velocity is currentposition - previousposition, then we add the acceleration vector
            //This means a player gradually accelerates and decelerates from the current velocity
            this.Velocity = CalculateVelocity() + this.accelerationVector;
            this.Transform.TranslateIncrement = (this.Velocity);
        }

        protected float ApplyGravity()
        {
            //Gravity
            //Always applies a downward force unless colliding with Ground
            
            if(GetDistanceToTarget(new Vector3(0, 0.5f, 0)) > 32f)
            {
                this.inGame = false;
                if (this.currentPosition.Y > 2.5f)
                {
                    this.gravity += -0.0025f;
                }
                else if(this.currentPosition.Y > 0.75f)
                {
                    this.gravity += 0.00075f;
                }
                else if(this.currentPosition.Y > -2f)
                {
                    if (this.gravity < -0.01f)
                    {
                        this.gravity += 0.0020f;
                    }
                    else
                    {
                        this.gravity = -0.01f;
                    }
                }
                //else
                //{

                //}
                //else
                //{
                //    if(this.gravity < -0.001f)
                //    {


                //        if (this.currentPosition.Y > 1.8f)
                //        {
                //            
                //        }
                //        else
                //        {
                //            this.gravity += 0.0010f;
                //        }
                //    }
                //    else
                //    {
                //        this.gravity = -0.01f;
                //    }

                //}


            }
            Console.WriteLine("Gravity is " + this.gravity);
            return gravity;
        }

        protected override void HandleInput(GameTime gameTime)
        {

            if (this.managerParameters.KeyboardManager.IsAnyKeyPressed())//CHANGE - To if MoveKeys Are Pressed
            {
                //Set initial acceleration Values to 0
                //So if nothing is pressed the acceleration vector will be 0
                float accelerationX = 0;
                float accelerationZ = 0;

                // if input is pressed then set the appropriate value
                if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveForward])) //Forward
                {
                    accelerationZ = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveBackward])) //Backward
                {
                    accelerationZ = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }

                if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateLeft])) //Left
                {
                    accelerationX = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateRight])) //Right
                {
                    accelerationX = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
                }

                //Fianlly set the acceleration Vector
                //This will allow the player to travel in both X and Z direction at the same time
                this.accelerationVector = new Vector3(accelerationX, 0, accelerationZ);
            }
            else
            {
                //Else No input is pressed It will graduly decelerate the player
                //Stop just Accelerates in the opposite direction of the velocity 
                //until the value is very minute then it is set to 0
                //this means the player will come to a stop when no input is pressed
                Stop(gameTime);
            }

        }

        protected Vector3 CalculateVelocity()
        {
            //Create temporary Vector for velocity
            Vector3 TempVelocity;

            //if collision Vecor is Zero that means No collision Occured
            // and we set current velocity to current position - previous position
            if (this.CollisionVector == Vector3.Zero)
            {
                TempVelocity = (this.currentPosition - this.previousPosition);
            }
            else
            {
                //collision vector is set, so we set the velocity to this.
                TempVelocity = this.CollisionVector;
            }
            if (Math.Abs(TempVelocity.X) <= 0.001f)
            {
                //If the velocity is really small just set it to 0 to stop it
                TempVelocity.X = 0;
            }
            if (Math.Abs(TempVelocity.Z) <= 0.001f)
            {
                TempVelocity.Z = 0;
            }

            TempVelocity.Y = ApplyGravity();

            return TempVelocity;
        }

        protected Vector3 CalculateCollisionVector(Vector3 ColideeVelocity)
        {
            //Collision vector is calculated by adding the two velocitys
            // then adding this.velocity divided by 4

            //In real life two objects with the same mass would simply swap velocitys
            //But I didnt like the results of this 
            //this is much closer to what I want to happen when two spheres collide
            return (ColideeVelocity + this.Velocity) + this.Velocity/4;
        }

        #region AI Movement

        protected void Move(GameTime gameTime)
        { 
            //accelerate to target
            Vector3 target = Vector3.Zero;

            //gets the min distance to begin stopping before taget so it doesnt overshoot
            SetInitialPosition(target);
            float stoppingDistance = getStoppingDistance(target);

            //if moving to target and distance to target is less than stopping distance then begin stopping
            if (IsMovingToTarget(target) && (Math.Abs(GetDistanceToTarget(target)) <= stoppingDistance))
            {
                Stop(gameTime);
            }
            else
            {
                //Otherwise we alwways want to be acceleratin to the target
                if(!IsOnTarget(target, 1))
                {
                    AccelerateToTarget(gameTime, Vector3.Zero);
                }
                else
                {
                    Stop(gameTime);
                }
                
            }

        }

        protected void AccelerateToTarget(GameTime gameTime, Vector3 target)
        {
            //gets the direction of the target, will be 1 0r -1
            //this is then multiplied in the acceleration to achive acceration in the appropriate direction
            int directionX = GetDirectionToTarget(this.currentPosition.X, target.X);
            int directionZ = GetDirectionToTarget(this.currentPosition.Z, target.Z);

            //Returns a float value approprate for the acceleration
            float accelerationX = Accelerate(gameTime, directionX);
            float accelerationZ = Accelerate(gameTime, directionZ);

            //then sets the acceleration vector
            this.accelerationVector = (new Vector3(accelerationX, 0, accelerationZ));
        }

        protected int GetDirectionToTarget(float position, float target)
        {
            //returns - 1 if taget is in negative direction, +1 if taget is in posative direction
            if (position > target)
            {
                return -1;
            }
            else if (position < target)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        protected int getCurrentDirection()
        {
            //1 = moving forward, -1 = moving backward, 0 = not moving
            if (this.previousPosition.X - this.currentPosition.X > 0.1f)
            {
                return 1;
            }
            else if (this.previousPosition.X - this.currentPosition.X < -0.1f)
            {
                return -1;
            }
            return 0;

        }

        protected bool IsInMiddle()
        {
            float Xpos = this.currentPosition.X;
            float Zpos = this.currentPosition.Z;

            //Since middle is 0,0 just get the absolute value of position
            //assuming the middle has a radious of 5
            if (Math.Abs(Xpos) < 5 && Math.Abs(Zpos) < 5)
            {
                return true;
            }

            return false;
        }

        protected bool IsMovingToTarget(Vector3 target)
        {
            //if the distance between the previous position is greater than
            //the distance between the current positions
            //object is moving towards target
            if (Vector3.Distance(this.previousPosition, target)
                > Vector3.Distance(this.currentPosition, target))
            {
                //is moving to target
                return true;
            }
            //is not moving to target
            return false;
        }

        protected float GetDistanceToTarget(Vector3 target)
        {
            return Vector3.Distance(this.currentPosition, target);
        }

        //protected bool IsAcceleratingToTarget(Vector3 target)
        //{
        //    //bool AcceleratingToTargetX = false;
        //    //bool AcceleratingToTargetZ = false;

        //    //if (target.X > 0 && this.accelerationVector.X > 0)
        //    //{

        //    //}
        //    //else if(target.X < 0 && this.accelerationVector.X < 0)
        //    //{

        //    //}

        //    //return AcceleratingToTarget;
        //}

        protected bool IsOnTarget(Vector3 target, float distance)
        {
            //if the distance between the current position is less than
            //the distance proivided
            //object is Within provided distance of target
            //this is used to tell the AI Sphere to stop when on target
            if (Math.Abs(Vector3.Distance(this.currentPosition, target)) <= distance)
            {
                return true;
            }
            return false;
        }

        protected void SetInitialPosition(Vector3 target)
        {
            //if moving away from target and the initial position is not set
            if ((!IsMovingToTarget(target)) && this.initialPosSet)
            {
                //resets initialpos bool so initial position can be set again
                this.initialPosSet = false;
            }

            if (HasChangedDirection() && IsMovingToTarget(target) && (!this.initialPosSet))
            {
                //When AI changes direction ad is traveling twards the target
                //this will set the initial position to its current position
                //This is usfull so we dont overshoot a target
                this.initialPosition = this.currentPosition;
                this.initialPosSet = true;
            }
            
        }

        protected float getStoppingDistance(Vector3 target)//, AxisDirectionType axis)
        {
            //Since we accelerate and decelerate at the same rate, 
            //stopping distance will always be half the distance from when we started accelerating towards a vector
            //Adding one means we will stop a very small amount before the target 
            float stoppingDistance = (Vector3.Distance(target, this.initialPosition)/2) + 1;

            return stoppingDistance;
        }

        protected bool HasChangedDirection()
        {
            if (currentDirection != previousDirection)
            {
                return true;
            }
            return false;
        }

        protected float Accelerate(GameTime gameTime, int direction)
        {
            return (this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds) * direction;
        }

        protected void Stop(GameTime gameTime)
        {
            //gets current velocity on the x and z axis
            float VelocityX = CalculateVelocity().X;
            float VelocityZ = CalculateVelocity().Z;
            //if velocity is very small
            //set it to 0 and it will stop
            if (Math.Abs(VelocityX) <= 0.001f)
            {
                VelocityX = 0;
            }
            else
            {
                //Otherwise Decelerate until small enough to stop
                if(VelocityX >0)
                {
                    VelocityX = (-this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds);
                }
                else
                {
                    VelocityX = (this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds);
                }
               
            }

            //same as above for z axis
            if (Math.Abs(VelocityZ) <= 0.001f)
            {
                VelocityZ = 0;
            }
            else
            {
                if (VelocityZ > 0)
                {
                    VelocityZ = (-this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds);
                }
                else
                {
                    VelocityZ = (this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds);
                }
            }

            //set the new acceleratiopn vector
            this.accelerationVector = (new Vector3(VelocityX, 0, VelocityZ));
        }


        #endregion
    }
}
