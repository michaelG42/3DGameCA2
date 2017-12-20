using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDLibrary
{
    public class GDBody
    {
        #region Fields
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 momentum;

        public float mass;
        public float inverseMass;
        private bool isImmovable;
        #endregion

        public GDBody(Vector3 position, Vector3 initialVelocity)
        {
            this.position = position;
            this.velocity = initialVelocity;
        }

        protected virtual void Enable(bool isImmovable, float mass)
        {
            this.isImmovable = isImmovable;
            this.mass = mass;

            this.inverseMass = 1.0f / mass;
        }

        protected virtual void Update()
        {
            this.velocity = this.momentum * inverseMass;
        }

    }
}
