using GDLibrary;
using GDLibrary.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDApp
{
    public class MyAppMenuManager : MenuManager
    {
        private bool firstStart = true;
        public MyAppMenuManager(Game game, MouseManager mouseManager, KeyboardManager keyboardManager, CameraManager cameraManager,
            SpriteBatch spriteBatch, EventDispatcher eventDispatcher,
            StatusType statusType) : base(game, mouseManager, keyboardManager, cameraManager, spriteBatch, eventDispatcher, statusType)
        {

        }

        #region Event Handling
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //call base method to show/hide the menu
            base.EventDispatcher_MenuChanged(eventData);

            //then generate sound events particular to your game e.g. play background music in a menu
            if (eventData.EventType == EventActionType.OnStart)
            {
                ////add event to stop background menu music here...
                //object[] additionalParameters = { "menu elevator music", 1 };
                //EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, additionalParameters));
            }
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //add event to play background menu music here...
                //object[] additionalParameters = { "menu elevator music" };
                //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }
        }
        #endregion

        protected override void HandleMouseOver(UIObject currentUIObject, GameTime gameTime)
        {
            //accumulate time over menu item
            //if greater than X milliseconds then play a boing and reset accumulated time
            //object[] additionalParameters = { "boing" };
            //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));

        }

        protected override void HandleMouseEntered(UIObject clickedUIObject)
        {

            object[] additionalParameters = { "ButtonHover" };
            EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
        }

        //add the code here to say how click events are handled by your code
        protected override void HandleMouseClick(UIObject clickedUIObject, GameTime gameTime)
        {
            object[] additionalParameters = { "ButtonClick" };
            EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));

            //notice that the IDs are the same as the button IDs specified when we created the menu in Main::AddMenuElements()
            switch (clickedUIObject.ID)
            {
                case "startbtn":
                    DoStart();
                    break;

                case "exitbtn":
                    DoExit();
                    break;

                case "audiobtn":
                    SetActiveList("audio menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                    EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.VolumeText));
                    break;

                case "volumeUpbtn":
                        object[] MusicUp = { 0.1f, "Music" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.GlobalSound, MusicUp));

                        object[] SoundsUp = { 0.1f, "Default" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.GlobalSound, SoundsUp));

                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.VolumeText));
                    break;

                case "volumeDownbtn":

                        object[] MusicDown = { -0.1f, "Music" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.GlobalSound, MusicDown));


                        object[] SoundDown = { -0.1f, "Music" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.GlobalSound, SoundDown));

                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.VolumeText));
                    break;

                case "volumeMutebtn":
                        object[] soundsMute = { 0.0f, "Default" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnMute, EventCategoryType.GlobalSound, soundsMute));

                        object[] MusicMute = { 0.0f, "Music" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnMute, EventCategoryType.GlobalSound, MusicMute));

                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.VolumeText));
                    break;

                case "volumeUnMutebtn":
                        object[] musicUnMute = { 0.5f, "Music" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnUnMute, EventCategoryType.GlobalSound, musicUnMute));

                        object[] soundsUnMute = { 0.5f, "Default" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnUnMute, EventCategoryType.GlobalSound, soundsUnMute));

                        EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.VolumeText));
                    break;

                case "backbtn":
                    object[] VolumeText = { "" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.MenuText, VolumeText));
                    SetActiveList("main menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                    break;

                case "controlsbtn":
                    SetActiveList("controls menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                    break;

                default:
                    break;
            }

            //add event to play mouse click sound here...

        }

        private void DoStart()
        {
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.MainMenu));
            if (this.firstStart)
            {
                object[] additionalParametersSound = { "Lava" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));

                object[] additionalEventParamsB = { AppData.IntroCurveCameraID };
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraResume, EventCategoryType.Camera));
                object[] additionalParameters = { GameState.CountDown };
                EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));
                this.firstStart = false;
            }
        }

        private void DoExit()
        {

            this.Game.Exit();
        }

    }
}
