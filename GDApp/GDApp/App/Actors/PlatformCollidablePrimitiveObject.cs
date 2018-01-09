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
        }
            protected Vector3 CalculateVelocity()
        {
            return (this.currentPosition - this.previousPosition);
        }
    }
}
