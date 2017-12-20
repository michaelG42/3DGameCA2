namespace GDLibrary
{
    public class PickupCollidablePrimitiveObject : CollidablePrimitiveObject
    {
        #region Variables
        private PickupParameters pickupParameters;
        #endregion

        #region Properties
        public PickupParameters PickupParameters
        {
            get
            {
                return pickupParameters;
            }
            set
            {
                this.pickupParameters = value;
            }
        }
        #endregion

        //used to draw collidable primitives that a value associated with them e.g. health
        public PickupCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData,
             ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, PickupParameters pickupParameters)
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive, objectManager)
        {
            this.pickupParameters = pickupParameters;
        }

        //used to make a pick collidable primitives from an existing PrimitiveObject (i.e. the type returned by the PrimitiveFactory
        public PickupCollidablePrimitiveObject(PrimitiveObject primitiveObject, ICollisionPrimitive collisionPrimitive, 
                                ObjectManager objectManager, PickupParameters pickupParameters)
            : base(primitiveObject, collisionPrimitive, objectManager)
        {
            this.pickupParameters = pickupParameters;
        }



        public override object GetDeepCopy()
        {
            PickupCollidablePrimitiveObject actor = new PickupCollidablePrimitiveObject("clone - " + ID, //deep
                 this.ActorType, //deep
                 (Transform3D)this.Transform.Clone(), //deep
                 (EffectParameters)this.EffectParameters.Clone(), //deep
                 this.StatusType, //deep
                 this.VertexData, //shallow - its ok if objects refer to the same vertices
                 (ICollisionPrimitive)this.CollisionPrimitive.Clone(), //deep
                 this.ObjectManager, //shallow - reference
                 (PickupParameters)this.pickupParameters.Clone()); //deep 

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
