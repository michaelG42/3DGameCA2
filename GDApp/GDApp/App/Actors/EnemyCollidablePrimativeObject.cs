//using GDLibrary.Enums;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using System;

//namespace GDLibrary
//{
//    public class PlayerCollidablePrimitiveObject : CollidablePrimitiveObject
//    {
//        #region Fields
//        private float accelerationSpeed;
//        private float stoppingDistance;
//        private int currentDirection;
//        private int previousDirection;
//        private Keys[] moveKeys;
//        private bool bThirdPersonZoneEventSent;
//        private ManagerParameters managerParameters;

//        private Vector3 initialPosition, previousPosition, currentPosition, accelerationVector;

//        private bool positionsInitialized;
//        private bool initialPosSet;
//        private bool colliding;

//        //private Vector3 acceleration;

//        //public Vector3 Acceleration { get => acceleration; set => acceleration = value; }
//        #endregion

//        #region Properties

//        #endregion

//        public PlayerCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
//            StatusType statusType, IVertexData vertexData, ICollisionPrimitive collisionPrimitive,
//            ManagerParameters managerParameters,
//            Keys[] moveKeys, float accelerationSpeed)
//            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, managerParameters.ObjectManager)
//        {
//            this.moveKeys = moveKeys;
//            this.accelerationSpeed = accelerationSpeed;
//            //for input
//            this.managerParameters = managerParameters;
//            this.positionsInitialized = false;
//            this.initialPosSet = false;
//            this.stoppingDistance = 12;
//            this.currentDirection = 0;
//            //this.accelerationVector = Vector3.Zero;
//        }

//        //used to make a player collidable primitives from an existing PrimitiveObject (i.e. the type returned by the PrimitiveFactory
//        public PlayerCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive,
//                                ManagerParameters managerParameters, Keys[] moveKeys, float accelerationSpeed)
//            : base(primitiveObject, collisionPrimitive, managerParameters.ObjectManager)
//        {
//            this.moveKeys = moveKeys;
//            //this.accelerationSpeed = accelerationSpeed;
//            this.accelerationSpeed = accelerationSpeed;
//            //for input
//            this.managerParameters = managerParameters;
//            this.positionsInitialized = false;
//            this.initialPosSet = false;
//            this.stoppingDistance = 12;
//            this.currentDirection = 0;
//            //this.accelerationVector = Vector3.Zero;
//        }


//        public override void Update(GameTime gameTime)
//        {
//            if (!positionsInitialized)
//            {
//                InitalizePositions();
//                this.positionsInitialized = true;
//            }

//            this.currentPosition = this.Transform.Translation;
//            this.currentDirection = getCurrentDirection();
//            //read any input and store suggested increments
//            if (this.ActorType == ActorType.Player)
//            {
//                HandleInput(gameTime);
//            }
//            else
//            {
//                Move(gameTime);
//            }

//            HandleAcceleration(gameTime);

//            ApplyGravity(gameTime);

//            //have we collided with something?
//            this.Collidee = CheckCollisions(gameTime);

//            //how do we respond to this collidee e.g. pickup?
//            HandleCollisionResponse(this.Collidee);

//            //if no collision then move - see how we set this.Collidee to null in HandleCollisionResponse() 
//            //below when we hit against a zone
//            if (this.Collidee == null)
//            {
//                this.colliding = false;
//                ApplyInput(gameTime);
//            }
//            else
//            {
//                this.colliding = true;
//            }



//            //reset translate and rotate and update primitive
//            base.Update(gameTime);
//            this.previousPosition = this.currentPosition;
//            this.previousDirection = this.currentDirection;

//        }

//        protected void InitalizePositions()
//        {
//            this.initialPosition = this.currentPosition = this.previousPosition = this.Transform.Translation;
//        }
//        //this is where you write the application specific CDCR response for your game
//        protected override void HandleCollisionResponse(Actor collidee)
//        {
//            if (collidee is SimpleZoneObject)
//            {
//                if (collidee.ID.Equals(AppData.SwitchToThirdPersonZoneID))
//                {
//                    if (!bThirdPersonZoneEventSent) //add a boolean to stop the event being sent multiple times!
//                    {
//                        //publish some sort of event - maybe an event to switch the camera?
//                        object[] additionalParameters = { AppData.ThirdPersonCameraID };
//                        EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalParameters));
//                        bThirdPersonZoneEventSent = true;
//                    }

//                    //setting this to null means that the ApplyInput() method will get called and the player can move through the zone.
//                    this.Collidee = null;
//                }
//            }
//            else if (collidee is CollidablePrimitiveObject)
//            {
//                if (collidee.ActorType == ActorType.CollidableDecorator)
//                {
//                    //we dont HAVE to do anything here but lets change its color just to see something happen
//                    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Yellow;
//                }

//                //decide what to do with the thing you've collided with
//                else if (collidee.ActorType == ActorType.CollidableAmmo)
//                {
//                    //do stuff...maybe a remove
//                    EventDispatcher.Publish(new EventData(collidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
//                }

//                //activate some/all of the controllers when we touch the object
//                else if (collidee.ActorType == ActorType.CollidableActivatable)
//                {
//                    //when we touch get a particular controller to start
//                    // collidee.SetAllControllers(PlayStatusType.Play, x => x.GetControllerType().Equals(ControllerType.SineColorLerp));

//                    //when we touch get a particular controller to start
//                    collidee.SetAllControllers(PlayStatusType.Play, x => x.GetControllerType().Equals(ControllerType.PickupDisappear));
//                }
//                else if (collidee.ActorType == ActorType.CollidableEnemy || collidee.ActorType == ActorType.Player)
//                {
//                    // Vector3 temp = (collidee as CollidablePrimitiveObject).Velocity;
//                    //this.Velocity = -this.Velocity;

//                    (collidee as CollidablePrimitiveObject).Velocity = this.Velocity * 0.8f;
//                    //this.Velocity = temp;

//                }
//                else if (collidee.ActorType == ActorType.CollidableGround)
//                {
//                    //this.Collidee = null;
//                }
//            }
//        }

//        protected void HandleAcceleration(GameTime gameTime)
//        {
//            //this.Transform.TranslateIncrement += this.accelerationVector;
//            // this.Velocity = (this.currentPosition - this.previousPosition) + this.accelerationVector;
//            //Console.WriteLine("Current position " + this.currentPosition);
//            //Console.WriteLine("Previous Position" + this.previousPosition);
//            //Console.WriteLine("C - P" + (this.currentPosition - this.previousPosition));
//            //Console.WriteLine("Acceleration is " + this.accelerationVector);

//            // this.Velocity = (this.currentPosition - this.previousPosition) + this.accelerationVector;

//            //Will Calculate Velocity By subtracting Previous position from Current position, Then add the acceleration Vector
//            this.Transform.TranslateBy(CalculateVelocity() + this.accelerationVector);
//            //this.accelerationVector = Vector3.Zero;
//        }

//        protected void ApplyGravity(GameTime gameTime)
//        {
//            //Gravity
//            //Always applies a downward force unless colliding with Ground
//            this.Transform.TranslateIncrement
//    = -this.Transform.Up * gameTime.ElapsedGameTime.Milliseconds
//            * 0.002f;
//        }

//        protected override void HandleInput(GameTime gameTime)
//        {

//            if (this.managerParameters.KeyboardManager.IsAnyKeyPressed())//CHANGE - To if MoveKeys Are Pressed
//            {
//                //Set initial acceleration Values to 0
//                float accelerationX = 0;
//                float accelerationZ = 0;

//                // if input is pressed then set the appropriate value
//                if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveForward])) //Forward
//                {
//                    accelerationZ = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
//                }
//                else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexMoveBackward])) //Backward
//                {
//                    accelerationZ = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
//                }

//                if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateLeft])) //Left
//                {
//                    accelerationX = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
//                }
//                else if (this.managerParameters.KeyboardManager.IsKeyDown(this.moveKeys[AppData.IndexRotateRight])) //Right
//                {
//                    accelerationX = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds;
//                }

//                //Fianlly set the acceleration Vector
//                //This will allow the player to travel in both X and Z direction at the same time
//                this.accelerationVector = new Vector3(accelerationX, 0, accelerationZ);
//            }
//            else
//            {
//                //Else No input is pressed It will graduly decelerate the player
//                Stop(gameTime);
//            }

//        }

//        protected Vector3 CalculateVelocity()
//        {

//            Vector3 TempVelocity = (this.currentPosition - this.previousPosition);

//            if (Math.Abs(TempVelocity.X) <= 0.0001f)
//            {
//                TempVelocity.X = 0;
//            }
//            if (Math.Abs(TempVelocity.Z) <= 0.0001f)
//            {
//                TempVelocity.Z = 0;
//            }


//            return TempVelocity;
//        }

//        protected void Stop(GameTime gameTime)
//        {
//            //Get Players Velocity in X and Y direction


//            float XVelocity = CalculateVelocity().X;
//            float ZVelocity = CalculateVelocity().Z;

//            //First Check Z Velocity + or - for the direction
//            //then gradualy slow down by adding the opposite direction, if it is nearly 0, set to 0 to stop
//            if (ZVelocity < -0.001f)
//            {
//                ZVelocity = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds * 0.7f;
//            }
//            else if (ZVelocity > 0.001f)
//            {
//                ZVelocity = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds * 0.7f;
//            }
//            else
//            {
//                ZVelocity = 0;
//            }

//            //Same as above for X direction
//            if (XVelocity < -0.001f)
//            {

//                XVelocity = this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds * 0.7f;
//            }
//            else if (XVelocity > 0.001f)
//            {
//                XVelocity = -this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds * 0.7f;
//            }
//            else
//            {
//                XVelocity = 0;
//            }

//            //Finaly set the new velocity

//            //Console.WriteLine("Acceleration in calculate velocity " + this.accelerationVector);
//            this.accelerationVector = (new Vector3(XVelocity, 0, ZVelocity));
//            Console.WriteLine("Acceleration in calculate velocity " + this.accelerationVector);
//        }

//        #region AI Movement

//        protected void Move(GameTime gameTime)
//        {

//            //Console.WriteLine("Previous Position is " + this.previousPosition);
//            //Console.WriteLine("Current Position is " + this.currentPosition);
//            //Console.WriteLine("Stopping distance is " + this.stoppingDistance);
//            // Console.WriteLine("Current Velocity is " + this.Velocity);
//            //Console.WriteLine("is in middle " + IsInMiddle());

//            //GetDirectionToMove just returns 1 or -1 to accelerate the appropriate direction

//            SetStoppingDistance();
//            int direction = GetDirectionToMove(this.Transform.Translation.X);

//            //Console.WriteLine("Stopping distance is " + this.stoppingDistance);
//            if (!IsInMiddle())
//            {
//                if (Math.Abs(this.Transform.Translation.X) <= this.stoppingDistance)
//                {

//                    // Console.WriteLine("Accelerating Away from middle");
//                    //Pass in -direction to decelerate(Accelerate opposite direction)

//                    //Accelerate away from middle
//                    Accelerate(gameTime, AxisDirectionType.X, -direction);

//                }
//                else
//                {
//                    //Console.WriteLine("Accelerating to middle");
//                    //Accelerate to middle
//                    Accelerate(gameTime, AxisDirectionType.X, direction);
//                }
//            }
//            else
//            {
//                //Console.WriteLine("STOPPING");
//                Stop(gameTime, -direction);
//            }

//            if ((!IsMovingToMiddle()) && this.initialPosSet)
//            {
//                this.initialPosSet = false;
//            }
//        }

//        protected int GetDirectionToMove(float position)
//        {

//            Console.WriteLine("Position is " + position);
//            if (position > 0.01f)
//            {
//                return -1;
//            }
//            else if (position < -0.01f)
//            {
//                return 1;
//            }
//            else
//            {
//                return 0;
//            }
//        }

//        protected int getCurrentDirection()
//        {
//            //1 = moving forward, -1 = moving backward, 0 = not moving
//            if (this.previousPosition.X - this.currentPosition.X > 0.1f)
//            {
//                return 1;
//            }
//            else if (this.previousPosition.X - this.currentPosition.X < -0.1f)
//            {
//                return -1;
//            }
//            return 0;

//        }

//        protected bool IsInMiddle()
//        {
//            float Xpos = this.Transform.Translation.X;
//            float Zpos = this.Transform.Translation.Z;

//            //Since middle is 0,0 just get the absolute value of position
//            if (Math.Abs(Xpos) < 5 && Math.Abs(Zpos) < 5)
//            {
//                return true;
//            }

//            return false;
//        }

//        protected bool IsMovingToMiddle()
//        {
//            if ((Math.Abs(this.previousPosition.X) - Math.Abs(this.currentPosition.X)) > 0)
//            {
//                return true;
//            }
//            return false;
//        }

//        protected void SetInitialPosition()
//        {
//            this.initialPosition = this.currentPosition;
//        }

//        protected float GetStoppingDistance(AxisDirectionType axis)
//        {
//            float stoppingDistance = 0;

//            stoppingDistance = (Math.Abs(this.initialPosition.X) / 2) + 1;

//            return stoppingDistance;
//        }

//        protected bool HasChangedDirection()
//        {
//            if (currentDirection != previousDirection)
//            {
//                return true;
//            }
//            return false;
//        }

//        protected void SetStoppingDistance()
//        {
//            if (HasChangedDirection() && IsMovingToMiddle() && (!this.initialPosSet))
//            {
//                SetInitialPosition();
//                this.stoppingDistance = GetStoppingDistance(AxisDirectionType.X);
//                this.initialPosSet = true;
//            }
//        }

//        protected void Accelerate(GameTime gameTime, AxisDirectionType axis, int direction)
//        {
//            //Get Players Velocity in X and Y direction
//            float VelocityX;
//            float Position;
//            switch (axis)
//            {
//                case AxisDirectionType.X:
//                    VelocityX = this.Velocity.X;
//                    Position = this.Transform.Translation.X;
//                    break;
//                case AxisDirectionType.Z:
//                    VelocityX = this.Velocity.Z;
//                    Position = this.Transform.Translation.Z;
//                    break;
//                default:
//                    VelocityX = 0;
//                    Position = 0;
//                    break;
//            }

//            VelocityX += (this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds) * direction;

//            this.Velocity = (new Vector3(VelocityX, 0, 0));

//        }

//        protected void Stop(GameTime gameTime, int direction)
//        {


//            float VelocityX = this.Velocity.X;

//            if (Math.Abs(VelocityX) <= 0.001f)
//            {

//                VelocityX = 0;
//            }
//            else
//            {
//                VelocityX += (this.accelerationSpeed * gameTime.ElapsedGameTime.Milliseconds) * direction;
//            }

//            this.Velocity = (new Vector3(VelocityX, 0, 0));
//        }

//        protected void ApplyDisplacmentVector(Vector3 ColideeVector)
//        {

//            Console.WriteLine("Vectoor is" + ColideeVector);

//            Vector3 DisplacmentVector = (-ColideeVector - this.Velocity);

//            this.Velocity = -this.Velocity;


//        }

//        #endregion
//    }
//}
