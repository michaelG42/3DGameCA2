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
            ManagerParameters managerParameters)
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, managerParameters.ObjectManager)
        {
            this.currentPosition = this.previousPosition = this.Transform.Translation;
        }

        public PlatformCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive,
                        ManagerParameters managerParameters)
            : base(primitiveObject, collisionPrimitive, managerParameters.ObjectManager)
        {

        }

        public override void Update(GameTime gameTime)
        {
            this.currentPosition = this.Transform.Translation;

            this.Velocity = CalculateVelocity();

            base.Update(gameTime);

            this.previousPosition = this.currentPosition;

            Console.WriteLine("Velocity is " + this.Velocity);
        }

        protected Vector3 CalculateVelocity()
        {
            return (this.currentPosition - this.previousPosition);
        }
    }
}
