/*
Function: 		Represents an area that can detect collisions by using only a simple BoundingSphere or AA BoundingBox. It does 
                NOT have an associated model. We can use this class to create activation zones e.g. for camera switching or event generation

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class SimpleZoneObject : Actor3D
    {

        #region Variables
        private ICollisionPrimitive collisionPrimitive;
        #endregion

        #region Properties
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
        #endregion

        public SimpleZoneObject(string id, ActorType actorType, Transform3D transform, StatusType statusType, ICollisionPrimitive collisionPrimitive)
            : base(id, actorType, transform, statusType)
        {
            this.collisionPrimitive = collisionPrimitive;
        }

        public override void Update(GameTime gameTime)
        {
            //update collision primitive with new object position
            if (collisionPrimitive != null)
                collisionPrimitive.Update(gameTime, this.Transform);
        }

        public override object GetDeepCopy()
        {
            SimpleZoneObject actor = new SimpleZoneObject("clone - " + ID, //deep
                 this.ActorType, //deep
                 (Transform3D)this.Transform.Clone(), //deep
                 this.StatusType, //deep
                 (ICollisionPrimitive)this.CollisionPrimitive.Clone()); //deep 

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
