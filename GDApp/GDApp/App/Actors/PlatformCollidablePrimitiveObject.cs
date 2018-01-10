using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDApp.App.Actors
{
    public class PlatformCollidablePrimitiveObject : CollidablePrimitiveObject
    {
        private Vector3 previousPosition, currentPosition;

        public PlatformCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform, EffectParameters effectParameters,
            StatusType statusType, IVertexData vertexData, ICollisionPrimitive collisionPrimitive,
            ManagerParameters managerParameters, EventDispatcher eventDispatcher)
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, managerParameters.ObjectManager, eventDispatcher)
        {
            this.currentPosition = this.previousPosition = this.Transform.Translation;
        }

        public PlatformCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive,
                        ManagerParameters managerParameters, EventDispatcher eventDispatcher)
            : base(primitiveObject, collisionPrimitive, managerParameters.ObjectManager, eventDispatcher)
        {

        }

        public override void Update(GameTime gameTime)
        {
            this.currentPosition = this.Transform.Translation;

            this.Velocity = CalculateVelocity();

            this.Collidee = CheckCollisions(gameTime);
            HandleCollisionResponse(this.Collidee);

            base.Update(gameTime);

            this.previousPosition = this.currentPosition;

            //Console.WriteLine("Velocity is " + this.Velocity);
        }

        protected override void HandleCollisionResponse(Actor collidee)
        {
            if ((collidee is SimpleZoneObject))
            {

                if (collidee.ID.Equals(AppData.LooseZoneID))
                {

                    this.ObjectManager.Remove(this);
                    //setting this to null means that the ApplyInput() method will get called and the player can move through the zone.
                    this.Collidee = null;
                }

            }
            else if (collidee is CollidablePrimitiveObject)
            {

                if (collidee.ActorType == ActorType.Player)
                {

                    //object[] additionalParametersSound = { "Bang" };
                    //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));

                    //there was a Problem where if a player wasnt moving it wouldnt detect collsion
                    //Solved by setting the other players collisionVelocity, rather than this.collisionVelocity
                    if ((this.Transform.Translation.Y + 4.5) > (collidee as CollidablePrimitiveObject).Transform.Translation.Y)
                    {
                        float YDiff = (float)(this.Transform.Translation.Y + 4.5) - (float)(collidee as CollidablePrimitiveObject).Transform.Translation.Y;

                        Console.WriteLine("YDiff IS " + YDiff);
                        (collidee as CollidablePrimitiveObject).CollisionVector = CalculateCollision((collidee as CollidablePrimitiveObject).Velocity, YDiff);//-((collidee as CollidablePrimitiveObject).Velocity);

                    }
                    Console.WriteLine(" Platform Y is " + this.Transform.Translation.Y);
                    Console.WriteLine("Player Y is " + (collidee as CollidablePrimitiveObject).Transform.Translation.Y);


                    //setting the enemys acceleration vector to -this.acceleration vector means the AI will alway accelerate towards the collision
                    //this is what most human players would do to avoid being sent flying off really far
                   // (collidee as PlayerCollidablePrimitiveObject).accelerationVector = -((collidee as CollidablePrimitiveObject).Velocity);

                }
            }

            }
        protected Vector3 CalculateVelocity()
        {
            return (this.currentPosition - this.previousPosition);
        }

        protected Vector3 CalculateCollision(Vector3 playerVelocity, float YDifferance)
        {
            //Values less than 0 mean Player is Above Platform
            if(YDifferance >= 2.8f)
            {
                return -(playerVelocity * 0.75f);
            }
            else if(YDifferance >= 0.8f)
            {
                Console.WriteLine("Returning Vectpor");
                float Yvel = (Math.Abs(playerVelocity.X) + Math.Abs(playerVelocity.Z) * 2);

                Console.WriteLine("Y velocity is " + Yvel);
                return new Vector3(playerVelocity.X, Yvel, playerVelocity.Z);
            }
            else
            {
                return Vector3.Zero;
            }

        }
    }
}
