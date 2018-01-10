using GDLibrary;
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
        private int pauseTime;


        private string display;
        private bool isActive;
        private bool isComplete;
        private bool isPaused;

        public string Display { get => display; set => display = value; }
        public int StartTime { get => startTime; set => startTime = value; }
        public int EndTime { get => endTime; set => endTime = value; }
        public bool IsActive { get => isActive; set => isActive = value; }
        public bool IsComplete { get => isComplete; set => isComplete = value; }
        public int PauseTime { get => pauseTime; set => pauseTime = value; }

        public Timer(EventDispatcher eventDispatcher)
        {
            this.IsActive = true;
            this.IsComplete = false;
            this.isPaused = false;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
            this.PauseTime = 0;
            RegisterForEventHandling(eventDispatcher);
        }

        public void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.TimerChanged += EventDispatcher_TimerChanged;
        }

        protected void EventDispatcher_TimerChanged(EventData eventData)
        {
            switch (eventData.EventType)
            {
                case EventActionType.OnPause:
                    this.pause();
                    break;

                case EventActionType.OnResume:
                    this.resume();
                    break;

                case EventActionType.OnRestart:
                    this.reset();
                    break;

                case EventActionType.OnStop:
                    this.finish();
                    break;

                case EventActionType.OnStart:
                    this.setTime((int)eventData.AdditionalParameters[0]);
                    break;
            }
        }

        public void set(GameTime gameTime, int seconds)
        {
            //If the timer has been paused, PauseTime will not be 0
            if(this.PauseTime != 0)
            {
                seconds = PauseTime;
            }
            
            if (this.isActive && !this.IsComplete)
            {
                this.StartTime = (int)gameTime.TotalGameTime.TotalSeconds;

                this.EndTime = this.StartTime - seconds;
                
                this.Display = Math.Abs(this.EndTime).ToString();
            }

            //Finish and Stop Counting
            //if(this.EndTime == 2)
            //{
            //    this.reset();
            //}

            if (this.isPaused)
            {
                //Counts the time it is paused for, this will replace seconds when unpaused
                this.PauseTime = Math.Abs(this.EndTime) + (int)gameTime.TotalGameTime.TotalSeconds;
            }

        }

        public void pause()
        {
            this.isActive = false;
            this.isPaused = true;
        }

        public void resume()
        {
            this.isActive = true;
            this.isPaused = false;
        }

        public void finish()
        {
            this.isActive = false;
            this.IsComplete = true;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
            this.PauseTime = 0;
        }

        public void reset()
        {
            this.IsActive = true;
            this.IsComplete = false;
            this.isPaused = false;
            this.Display = "";
            this.StartTime = 0;
            this.EndTime = 0;
            this.PauseTime = 0;
        }

        public void setTime(int time)
        {
            this.IsActive = true;
            this.PauseTime = this.StartTime + time;
        }
    }
}
