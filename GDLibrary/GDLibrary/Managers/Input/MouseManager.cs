﻿/*
Function: 		Provide mouse input functions (with no JibLibX functionality i.e. for I-CA project)
Author: 		NMCG
Version:		1.2
Date Updated:	5/12/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GDLibrary
{
    public class MouseManager : GameComponent
    {
        #region Fields
        private MouseState newState, oldState;
        #endregion

        #region Properties
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(this.newState.X, this.newState.Y, 1, 1);
            }
        }
        public Vector2 Position
        {
            get
            {
                return new Vector2(this.newState.X, this.newState.Y);
            }
        }
        public bool MouseVisible
        {
            get
            {
                return this.Game.IsMouseVisible;
            }
            set
            {
                this.Game.IsMouseVisible = value;
            }
        }
        #endregion

        //supports mouse picking
        public MouseManager(Game game, bool bMouseVisible)
            : base(game)
        {
            this.MouseVisible = bMouseVisible;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }
     
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //store the old state
            this.oldState = newState;

            //get the new state
            this.newState = Mouse.GetState();

            base.Update(gameTime);
        }

        public bool HasMoved(float mouseSensitivity)
        {
            float deltaPositionLength = new Vector2(newState.X - oldState.X, 
                newState.Y - oldState.Y).Length();

            return (deltaPositionLength > mouseSensitivity) ? true : false;
        }

        public float DirectionMovedX()
        {
            return newState.X - oldState.X;
        }

        public bool IsLeftButtonClickedOnce()
        {   
            return ((newState.LeftButton.Equals(ButtonState.Pressed)) && (!oldState.LeftButton.Equals(ButtonState.Pressed)));
        }

        public bool IsMiddleButtonClicked()
        {
            return (newState.MiddleButton.Equals(ButtonState.Pressed));
        }

        public bool IsMiddleButtonClickedOnce()
        {
            return ((newState.MiddleButton.Equals(ButtonState.Pressed)) && (!oldState.MiddleButton.Equals(ButtonState.Pressed)));
        }

        public bool IsLeftButtonClicked()
        {
            return (newState.LeftButton.Equals(ButtonState.Pressed));
        }

        public bool IsRightButtonClickedOnce()
        {
            return ((newState.RightButton.Equals(ButtonState.Pressed)) && (!oldState.RightButton.Equals(ButtonState.Pressed)));
        }

        public bool IsRightButtonClicked()
        {
            return (newState.RightButton.Equals(ButtonState.Pressed));
        }

        //Calculates the mouse pointer distance (in X and Y) from a user-defined position
        public Vector2 GetDeltaFromPosition(Vector2 position, Camera3D activeCamera)
        {
            Vector2 delta;
            if (this.Position != position) //e.g. not the centre
            {

                if (activeCamera.View.Up.Y == -1)
                {
                    delta.X = 0;
                    delta.Y = 0;
                }
                else
                {
                    delta.X = this.Position.X - position.X;
                    delta.Y = this.Position.Y - position.Y;
                }
                SetPosition(position);
                return delta;
            }
            return Vector2.Zero;
        }

        //Calculates the mouse pointer distance from the screen centre
        public Vector2 GetDeltaFromCentre(Vector2 screenCentre)
        {
            return new Vector2(this.newState.X - screenCentre.X, this.newState.Y - screenCentre.Y);
        }

        //has the mouse state changed since the last update?
        public bool IsStateChanged()
        {
            return (this.newState.Equals(oldState)) ? false : true;
        }

        //did the mouse move above the limits of precision from centre position
        public bool IsStateChangedOutsidePrecision(float mousePrecision)
        {
            return ((Math.Abs(newState.X - oldState.X) > mousePrecision) || (Math.Abs(newState.Y - oldState.Y) > mousePrecision));
        }

        public int GetScrollWheelValue()
        {
            return newState.ScrollWheelValue;
        }


        //how much has the scroll wheel been moved since the last update?
        public int GetDeltaFromScrollWheel()
        {
            if (IsStateChanged()) //if state changed then return difference
                return newState.ScrollWheelValue - oldState.ScrollWheelValue;

            return 0;
        }

        public void SetPosition(Vector2 position)
        {
            Mouse.SetPosition((int)position.X, (int)position.Y);
        }

        #region Ray Picking
        //get a ray positioned at the mouse's location on the screen - used for picking 
        public Ray GetMouseRay(Camera3D camera)
        {
            //get the positions of the mouse in screen space
            Vector3 near = new Vector3(this.newState.X, this.Position.Y, 0);

            //convert from screen space to world space
            near = camera.Viewport.Unproject(near, camera.ProjectionParameters.Projection, camera.View, Matrix.Identity);

            return GetMouseRayFromNearPosition(camera, near);
        }

        //get a ray from a user-defined near position in world space and the mouse pointer
        public Ray GetMouseRayFromNearPosition(Camera3D camera, Vector3 near)
        {
            //get the positions of the mouse in screen space
            Vector3 far = new Vector3(this.newState.X, this.Position.Y, 1);

            //convert from screen space to world space
            far = camera.Viewport.Unproject(far, camera.ProjectionParameters.Projection, camera.View, Matrix.Identity);

            //generate a ray to use for intersection tests
            return new Ray(near, Vector3.Normalize(far - near));
        }

        //get a ray positioned at the scnree position - used for picking when we have a centred reticule
        public Vector3 GetMouseRayDirection(Camera3D camera, Vector2 screenPosition)
        {
            //get the positions of the mouse in screen space
            Vector3 near = new Vector3(screenPosition.X, screenPosition.Y, 0);
            Vector3 far = new Vector3(this.Position, 1);

            //convert from screen space to world space
            near = camera.Viewport.Unproject(near, camera.ProjectionParameters.Projection, camera.View, Matrix.Identity);
            far = camera.Viewport.Unproject(far, camera.ProjectionParameters.Projection, camera.View, Matrix.Identity);


            //generate a ray to use for intersection tests
            return Vector3.Normalize(far - near);
        }

        public Vector3 GetMouseRayDirection(Camera3D camera)
        {
            return GetMouseRayDirection(camera, new Vector2(this.newState.X, this.newState.Y));
        }
        #endregion
    }
}