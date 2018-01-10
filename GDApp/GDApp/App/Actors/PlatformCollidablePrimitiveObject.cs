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
        private bool collisionSoundPlayed;
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

        }

        protected override void HandleCollisionResponse(Actor collidee)
        {
            if ((collidee is SimpleZoneObject))
            {

                if (collidee.ID.Equals(AppData.LooseZoneID))
                {
                    //Removes Platform When Below Lava
                    this.ObjectManager.Remove(this);
                    this.Collidee = null;
                }

            }
            else if (collidee is CollidablePrimitiveObject)
            {

                if (collidee.ActorType == ActorType.Player)
                {

                    if ((this.Transform.Translation.Y + 4.5) > (collidee as CollidablePrimitiveObject).Transform.Translation.Y)
                    {
                        float YDiff = (float)(this.Transform.Translation.Y + 4.5) - (float)(collidee as CollidablePrimitiveObject).Transform.Translation.Y;

                        (collidee as CollidablePrimitiveObject).CollisionVector = CalculateCollision((collidee as CollidablePrimitiveObject).Velocity, YDiff);//-((collidee as CollidablePrimitiveObject).Velocity);

                    }

                }
            }
            else
            {
                this.collisionSoundPlayed = false;
            }

        }
        protected Vector3 CalculateVelocity()
        {
            return (this.currentPosition - this.previousPosition);
        }

        protected Vector3 CalculateCollision(Vector3 playerVelocity, float YDifferance)
        {
            //Values less than 0 mean Player is Above Platform
            if (YDifferance >= 2.8f)
            {
                if(!this.collisionSoundPlayed)
                {
                    object[] additionalParametersSound = { "Bang" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));
                    this.collisionSoundPlayed = true;
                }
                return -(playerVelocity * 0.75f);
            }
            else if (YDifferance >= 0.8f)
            {
                //If the player Hits the Platform at an angle it will send the player up slightly
                //Like hitting a curb
                float Yvel = (Math.Abs(playerVelocity.X) + Math.Abs(playerVelocity.Z) * 2);

                return new Vector3(playerVelocity.X, Yvel, playerVelocity.Z);
            }
            else
            {
                return Vector3.Zero;
            }

        }
    }
}
