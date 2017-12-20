using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class SimplePickingManager : PausableGameComponent
    {
        #region Statics
        protected static readonly string NoObjectSelectedText = "no object selected";
        #endregion

        #region Fields
        private ManagerParameters managerParameters;

        //local vars
        private Actor3D collidee;
        private ICollisionPrimitive collisionPrimitive;
        #endregion

        #region Properties
        public ManagerParameters ManagerParameters
        {
            get
            {
                return this.managerParameters;
            }
        }
        #endregion

        public SimplePickingManager(Game game, EventDispatcher eventDispatcher, StatusType statusType, ManagerParameters managerParameters) 
            : base(game, eventDispatcher, statusType)
        {
            this.managerParameters = managerParameters;
        }


        protected override void ApplyUpdate(GameTime gameTime)
        {
            //is the mouse over something right now?
            CheckCollisions(gameTime);
        }

        private void CheckCollisions(GameTime gameTime)
        {
            Ray mouseRay = this.managerParameters.MouseManager.GetMouseRay(this.managerParameters.CameraManager.ActiveCamera);

            foreach (Actor3D actor in this.managerParameters.ObjectManager.OpaqueDrawList)
            {
                collidee = CheckCollision(gameTime, actor, mouseRay);

                if (collidee != null)
                {
                    HandleResponse(gameTime, collidee);
                    break; //if we collide then break and handle collision
                }
            }

            foreach (Actor3D actor in this.managerParameters.ObjectManager.TransparentDrawList)
            {
                collidee = CheckCollision(gameTime, actor, mouseRay);

                if (collidee != null)
                {
                    HandleResponse(gameTime, collidee);
                    break; //if we collide then break and handle collision
                }
            }

            //we're not over anything
            if(collidee == null)
            {
                //notify listeners that we're no longer picking
                object[] additionalParameters = { NoObjectSelectedText };
                EventDispatcher.Publish(new EventData(EventActionType.OnNonePicked, EventCategoryType.ObjectPicking, additionalParameters));

            }
        }

        private Actor3D CheckCollision(GameTime gameTime, Actor3D actor, Ray mouseRay)
        {
            if (actor is CollidablePrimitiveObject)
            {
                collisionPrimitive = (actor as CollidablePrimitiveObject).CollisionPrimitive;
                if (collisionPrimitive.Intersects(mouseRay))
                    return actor;
            }
            else if (actor is SimpleZoneObject)
            {
                collisionPrimitive = (actor as SimpleZoneObject).CollisionPrimitive;
                if (collisionPrimitive.Intersects(mouseRay))
                    return actor;
            }
            else //PrimitiveObject - uses basic BoundingSphere created for camera frustum culling
            {
                float? result = mouseRay.Intersects(actor.BoundingSphere);

                if (result != null)
                    return actor;
            }

            return null;
        }

        protected virtual void HandleResponse(GameTime gameTime, Actor3D collidee)
        {
            //child class defines how the collision is responded to - see MySimplePickingManager
        }
    }
}
