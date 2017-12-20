﻿/*
Function: 		Applies a color change to a UI actor based on a sine wave and user-specified min and max colours.
Author: 		NMCG
Version:		1.0
Date Updated:	6/10/17
Bugs:			None
Fixes:			None
*/
using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class UIColorSineLerpController : UIController
    {
        #region Fields
        private TrigonometricParameters trigonometricParameters;
        private Color colorMin, colorMax;
        #endregion

        #region Properties
        public TrigonometricParameters TrigonometricParameters
        {
            get
            {
                return this.trigonometricParameters;
            }
            set
            {
                this.trigonometricParameters = value;
            }
        }
        public Color ColorMin
        {
            get
            {
                return this.colorMin;
            }
            set
            {
                this.colorMin = value;
            }
        }
        public Color ColorMax
        {
            get
            {
                return this.colorMax;
            }
            set
            {
                this.colorMax = value;
            }
        }
        #endregion

        public UIColorSineLerpController(string id, ControllerType controllerType, TrigonometricParameters trigonometricParameters,
            Color colorMin, Color colorMax) 
            : base(id, controllerType)
        {
            this.TrigonometricParameters = trigonometricParameters;
            this.colorMin = colorMin;
            this.colorMax = colorMax;
        }

        public override void SetActor(IActor actor)
        {
            UIObject uiObject = actor as UIObject;
            uiObject.Color = uiObject.OriginalColor;
        }

        protected override void ApplyController(GameTime gameTime, UIObject uiObject, float totalElapsedTime)
        {
            //sine wave in the range 0 -> max amplitude
            float lerpFactor = MathUtility.SineLerpByElapsedTime(this.TrigonometricParameters, totalElapsedTime);
            //apply color change
            uiObject.Color = MathUtility.Lerp(this.colorMin, this.colorMax, lerpFactor);
        }

        public override bool Equals(object obj)
        {
            UIColorSineLerpController other = obj as UIColorSineLerpController;

            if (other == null)
                return false;
            else if (this == other)
                return true;

            return this.colorMin.Equals(other.ColorMin)
                    && this.colorMax.Equals(other.ColorMax)
                        && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 31 + this.colorMin.GetHashCode();
            hash = hash * 17 + this.colorMax.GetHashCode();
            hash = hash * 11 + base.GetHashCode();
            return hash;
        }

        public override object GetDeepCopy()
        {
            IController clone = new UIColorSineLerpController("clone - " + this.ID, //deep
                this.ControllerType, //deep
                (TrigonometricParameters)this.trigonometricParameters.Clone(), //deep
                this.colorMin,  //deep
                this.colorMax); //deep

            clone.SetControllerPlayStatus(this.PlayStatusType);

            return clone;
        }

        public new object Clone()
        {
            return GetDeepCopy();
        }
    }
}
