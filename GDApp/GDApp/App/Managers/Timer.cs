using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Following Guide from here http://xboxforums.create.msdn.com/forums/t/56895.aspx
namespace GDApp.App.Managers
{
    public class Timer
    {
        private int startTime;
        private int endTime;
        private string display;
        private bool isActive;
        private bool isComplete;

        public string Display { get => display; set => display = value; }
        public int StartTime { get => startTime; set => startTime = value; }
        public int EndTime { get => endTime; set => endTime = value; }
        public bool IsActive { get => isActive; set => isActive = value; }
        public bool IsComplete { get => isComplete; set => isComplete = value; }

        public Timer()
        {
            this.IsActive = false;
            this.IsComplete = false;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
        }

        public void set(GameTime gameTime, int seconds)
        {
            this.StartTime = (int)gameTime.TotalGameTime.TotalSeconds;
            this.EndTime = this.StartTime - seconds;
            this.IsActive = true;
            this.Display = Math.Abs(this.EndTime).ToString();
        }

        public void finish()
        {
            this.isActive = false;
            this.IsComplete = true;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
        }

        public void reset()
        {
            this.IsActive = false;
            this.IsComplete = false;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
        }
    }
}
