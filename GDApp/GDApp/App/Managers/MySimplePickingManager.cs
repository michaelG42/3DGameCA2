using GDLibrary;
using System;
using Microsoft.Xna.Framework;

namespace GDApp
{
    public class MySimplePickingManager : SimplePickingManager
    {
        #region Statics
        protected static readonly int DefaultDistanceToTargetPrecision = 2;
        #endregion

        #region Fields
        private int totalElapsedTimeInMs;
        private int minTimeBetweenSoundEventInMs = 250;
        #endregion

        public MySimplePickingManager(Game game, EventDispatcher eventDispatcher, StatusType statusType, ManagerParameters managerParameters) : base(game, eventDispatcher, statusType, managerParameters)
        {
        }

        protected override void HandleResponse(GameTime gameTime, Actor3D collidee)
        {
            //what objects are we interested in?
            if (collidee.ActorType == ActorType.CollidablePickup || collidee.ActorType == ActorType.Player)
            {
                //sends an event every minTimeBetweenSoundEventInMs seconds
                if (totalElapsedTimeInMs > minTimeBetweenSoundEventInMs)
                {
                    object[] additionalParametersA = { "boing" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersA));
                    totalElapsedTimeInMs = 0;

                    //or tell a UI mouse something
                    float distanceToObject = (float)Math.Round(Vector3.Distance(this.ManagerParameters.CameraManager.ActiveCamera.Transform.Translation,
                                                                                            collidee.Transform.Translation), DefaultDistanceToTargetPrecision);
                    object[] additionalParametersB = { collidee, distanceToObject };
                    EventDispatcher.Publish(new EventData(EventActionType.OnObjectPicked, EventCategoryType.ObjectPicking, additionalParametersB));

                    //or do something directly like make the object rotate
                    //collidee.Transform.RotateAroundYBy(15f);

                }
                else
                    totalElapsedTimeInMs += gameTime.ElapsedGameTime.Milliseconds;

                //or we could remove the if..else code above and just remove the object - it all depends on how we want to handle mouse over events
                //or generate event to tell object manager and physics manager to remove the object
                //EventDispatcher.Publish(new EventData(collidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));


            }
        }
    }
}
