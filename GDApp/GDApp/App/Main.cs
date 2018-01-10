#define DEMO

using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using System;
using GDApp.App.Managers;
using GDLibrary.Enums;
using GDApp.App.Actors;
//using GDApp.App.Actors;
/*
 * Diffuse color not pickup up when drawing primitive objects
 * TranslationLerpController
 * Is frustum culling working on primitive objects?
 * 
 * 
 * No statustype on controllers
 * mouse object text interleaving with progress controller
 * Z-fighting on ground plane in 3rd person mode
 * Elevation angle on 3rd person view
 * PiP
 * menu transparency
*/

namespace GDApp
{
    public class Main : Game
    {

        #region Fields
#if DEBUG
        //used to visualize debug info (e.g. FPS) and also to draw collision skins
        private DebugDrawer debugDrawer;
#endif

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //added property setters in short-hand form for speed
        public ObjectManager objectManager { get; private set; }
        public CameraManager cameraManager { get; private set; }
        public MouseManager mouseManager { get; private set; }
        public KeyboardManager keyboardManager { get; private set; }
        public ScreenManager screenManager { get; private set; }
        public MyAppMenuManager menuManager { get; private set; }
        public UIManager uiManager { get; private set; }
        public UIManager MenuUIManager { get; private set; }
        public GamePadManager gamePadManager { get; private set; }
        public SoundManager soundManager { get; private set; }
        public MySimplePickingManager pickingManager { get; private set; }

        //receives, handles and routes events
        public EventDispatcher eventDispatcher { get; private set; }

        //stores loaded game resources
        // private ContentDictionary<Model> modelDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        private ContentDictionary<SpriteFont> fontDictionary;

        //stores curves and rails used by cameras, viewport, effect parameters
        private Dictionary<string, Transform3DCurve> curveDictionary;
        private Dictionary<string, RailParameters> railDictionary;
        private Dictionary<string, Viewport> viewPortDictionary;
        private Dictionary<string, EffectParameters> effectDictionary;
        private ContentDictionary<Video> videoDictionary;
        private Dictionary<string, IVertexData> vertexDataDictionary;

        private ManagerParameters managerParameters;
        private MyPrimitiveFactory primitiveFactory;
        private PlayerCollidablePrimitiveObject playerCollidablePrimitiveObject;
        private PrimitiveObject lava;
        private PlayerCollidablePrimitiveObject[] enemys;
        private PrimitiveDebugDrawer collisionSkinDebugDrawer;

        private PlatformCollidablePrimitiveObject[] platforms;
        private Timer timer;
        private UITextObject GameStateText;
        private UITextObject textClone;
        private UITextObject MenutextClone;
        private GameState gameState;
        private SimpleZoneObject looseZoneObject;
        private IController IntroCurvecontroller;

        private float lavaSpeed;
        private int InitialTimerTime;
        private int lavaTimer;
        private int GameTextSize;
        private int verticalTextOffset;
        private int restartTime;


        private bool level2Initalized;
        private bool opacitySet;
        private bool thirdPersonEventSent;
        private bool introCameraSkipped;
        private bool lavaTimerSet;
        private bool restartButtonAdded;
        private bool restarting;
        private bool restartTimeSet;
        #endregion

        #region Properties
        #endregion

        #region Constructor
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Initialization
        protected override void Initialize()
        {
            //moved instanciation here to allow menu and ui managers to be moved to InitializeManagers()
            spriteBatch = new SpriteBatch(GraphicsDevice);

            bool isMouseVisible = false;
            Integer2 screenResolution = ScreenUtility.HD720;
            ScreenUtility.ScreenType screenType = ScreenUtility.ScreenType.SingleScreen;
            int numberOfGamePadPlayers = 1;

            //set the title
            Window.Title = "VOLCANO ESCAPE!";

            //EventDispatcher
            InitializeEventDispatcher();

            //Dictionaries, Media Assets and Non-media Assets
            LoadDictionaries();
            LoadAssets();
            LoadCurvesAndRails();
            LoadViewports(screenResolution);

            //factory to produce primitives
            LoadFactories();

            //Effects
            InitializeEffects();

            //Managers
            InitializeManagers(screenResolution, screenType, isMouseVisible, numberOfGamePadPlayers);

            //Add Menu and UI elements
            AddMenuElements();
            AddUIElements();


            LoadGame();

            //Add Camera(s)
            InitializeCameras(screenResolution);

            //Publish Start Event(s)
            StartGame();

#if DEBUG
            //InitializeDebugTextInfo();
           // InitializeDebugCollisionSkinInfo();
#endif
            RegisterForEventHandling(this.eventDispatcher);
            base.Initialize();
        }

        private void Restart()
        {
            //Reset Bools
            this.opacitySet = false;
            this.introCameraSkipped = false;
            this.thirdPersonEventSent = false;
            this.level2Initalized = false;
            this.lavaTimerSet = false;

            DisplayMessage("Restarting");
            this.GameStateText.Text = "";

            //Clear the object manager and Camera Manager
            this.cameraManager.Clear();
            this.objectManager.Clear();

            //Reload Game
            LoadGame();

            //Re Initialize Cameras
            Integer2 screenResolution = ScreenUtility.HD720;
            InitializeCameras(screenResolution);

            //Restart Timer
            object[] additionalEventParamsTime = { 25 };
            EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.Timer, additionalEventParamsTime));

            //Set gamestate to Level 1 Countdown
            object[] additionalParameters = { GameState.CountDown };
            EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));

            //Set Camera to Intro Curve Camera
            object[] additionalEventParamsB = { AppData.IntroCurveCameraID };
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraResume, EventCategoryType.Camera));

        }
        private void InitializeManagers(Integer2 screenResolution,
            ScreenUtility.ScreenType screenType, bool isMouseVisible, int numberOfGamePadPlayers) //1 - 4
        {
            //add sound manager
            this.soundManager = new SoundManager(this, this.eventDispatcher, StatusType.Update, "Content/Assets/Audio/", "Sounds.xgs", "WaveBank1.xwb", "SoundBank1.xsb");
            Components.Add(this.soundManager);

            this.cameraManager = new CameraManager(this, 1, this.eventDispatcher);
            Components.Add(this.cameraManager);

            //create the object manager - notice that its not a drawablegamecomponent. See ScreeManager::Draw()
            this.objectManager = new ObjectManager(this, this.cameraManager, this.eventDispatcher, 10);

            //add keyboard manager
            this.keyboardManager = new KeyboardManager(this);
            Components.Add(this.keyboardManager);

            //create the manager which supports multiple camera viewports
            this.screenManager = new ScreenManager(this, graphics, screenResolution, screenType,
                this.objectManager, this.cameraManager, this.keyboardManager,
                AppData.KeyPauseShowMenu, this.eventDispatcher, StatusType.Off);
            this.screenManager.DrawOrder = 0;
            Components.Add(this.screenManager);

            //add mouse manager
            this.mouseManager = new MouseManager(this, isMouseVisible);
            Components.Add(this.mouseManager);

            //add gamepad manager
            if (numberOfGamePadPlayers > 0)
            {
                this.gamePadManager = new GamePadManager(this, numberOfGamePadPlayers);
                Components.Add(this.gamePadManager);
            }

            //menu manager
            this.menuManager = new MyAppMenuManager(this, this.mouseManager, this.keyboardManager, this.cameraManager, spriteBatch, this.eventDispatcher, StatusType.Off);
            //set the main menu to be the active menu scene
            this.menuManager.SetActiveList("mainmenu");
            this.menuManager.DrawOrder = 3;
            Components.Add(this.menuManager);

            //ui (e.g. reticule, inventory, progress)
            this.uiManager = new UIManager(this, this.spriteBatch, this.eventDispatcher, 10, StatusType.Off);
            this.uiManager.DrawOrder = 4;
            Components.Add(this.uiManager);

            this.MenuUIManager = new UIManager(this, this.spriteBatch, this.eventDispatcher, 10, StatusType.Update | StatusType.Drawn);
            this.MenuUIManager.DrawOrder = 4;
            Components.Add(this.MenuUIManager);

            //this object packages together all managers to give the mouse object the ability to listen for all forms of input from the user, as well as know where camera is etc.
            this.managerParameters = new ManagerParameters(this.objectManager,
                this.cameraManager, this.mouseManager, this.keyboardManager, this.gamePadManager, this.screenManager, this.soundManager);


            //used for simple picking (i.e. non-JigLibX)
            this.pickingManager = new MySimplePickingManager(this, this.eventDispatcher, StatusType.Update, this.managerParameters);
            Components.Add(this.pickingManager);

            //CountDown timer
            this.timer = new Timer(this.eventDispatcher);

        }

        private void LoadDictionaries()
        {
            //models
            // this.modelDictionary = new ContentDictionary<Model>("model dictionary", this.Content);

            //textures
            this.textureDictionary = new ContentDictionary<Texture2D>("texture dictionary", this.Content);

            //fonts
            this.fontDictionary = new ContentDictionary<SpriteFont>("font dictionary", this.Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            this.curveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails
            this.railDictionary = new Dictionary<string, RailParameters>();

            //viewports - used to store different viewports to be applied to multi-screen layouts
            this.viewPortDictionary = new Dictionary<string, Viewport>();

            //stores default effect parameters
            this.effectDictionary = new Dictionary<string, EffectParameters>();

            //notice we go back to using a content dictionary type since we want to pass strings and have dictionary load content
            this.videoDictionary = new ContentDictionary<Video>("video dictionary", this.Content);

            //used to store IVertexData (i.e. when we want to draw primitive objects, as in I-CA)
            this.vertexDataDictionary = new Dictionary<string, IVertexData>();

        }
        private void LoadAssets()
        {
            #region Textures
            //environment
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");

            this.textureDictionary.Load("Assets/Textures/Skybox/back");
            this.textureDictionary.Load("Assets/Textures/Skybox/left");
            this.textureDictionary.Load("Assets/Textures/Skybox/right");
            this.textureDictionary.Load("Assets/Textures/Skybox/sky");
            this.textureDictionary.Load("Assets/Textures/Skybox/front");

            this.textureDictionary.Load("Assets/Textures/Enviornment/Lava");
            this.textureDictionary.Load("Assets/Textures/Enviornment/VolcanoWall");
            this.textureDictionary.Load("Assets/Textures/Enviornment/Platform");

            this.textureDictionary.Load("Assets/Textures/Players/Red");
            this.textureDictionary.Load("Assets/Textures/Players/Green");
            this.textureDictionary.Load("Assets/Textures/Players/Blue");
            this.textureDictionary.Load("Assets/Textures/Players/Yellow");

            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");

            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mainmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/audiomenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/controlsmenu");


            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");

#if DEBUG
            //demo
            this.textureDictionary.Load("Assets/GDDebug/Textures/ml");
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
#endif
            #endregion

            #region Fonts
#if DEBUG
            this.fontDictionary.Load("Assets/GDDebug/Fonts/debug");
#endif
            this.fontDictionary.Load("Assets/Fonts/menu");
            this.fontDictionary.Load("Assets/Fonts/GameText");

            this.fontDictionary.Load("Assets/Fonts/mouse");
            #endregion

            #region Video
            // this.videoDictionary.Load("Assets/Video/sample");
            #endregion
        }
        private void LoadCurvesAndRails()
        {

            #region Curves
            //create the camera curve to be applied to the track controller

            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Linear); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(-40, 80, 0), new Vector3(1, -2f, 0f), Vector3.UnitY, 0); //start position
            curveA.Add(new Vector3(-100, 15, 0), new Vector3(1, -0.2f, 0f), Vector3.UnitY, 5);
            curveA.Add(new Vector3(0, 100, -120), new Vector3(0, -0.6f, 0.8f), Vector3.UnitY, 9);
            curveA.Add(new Vector3(60, 25, 0), new Vector3(-1, -0.4f, 0), Vector3.UnitY, 13);
            curveA.Add(new Vector3(5, 40, 60), new Vector3(-0.1f, -0.6f, -0.7f), Vector3.UnitY, 17);
            curveA.Add(new Vector3(0, 45, 65), new Vector3(0, -0.6f, -0.8f), Vector3.UnitY, 19); //end position
            //add to the dictionary
            this.curveDictionary.Add("introCurveCamera", curveA);
            #endregion


        }
        private void LoadViewports(Integer2 screenResolution)
        {

            //the full screen viewport with optional padding
            int leftPadding = 0, topPadding = 0, rightPadding = 0, bottomPadding = 0;
            Viewport paddedFullViewPort = ScreenUtility.Pad(new Viewport(0, 0, screenResolution.X, (int)(screenResolution.Y)), leftPadding, topPadding, rightPadding, bottomPadding);
            this.viewPortDictionary.Add("full viewport", paddedFullViewPort);

            //work out the dimensions of the small camera views along the left hand side of the screen
            int smallViewPortHeight = 144; //6 small cameras along the left hand side of the main camera view i.e. total height / 5 = 720 / 5 = 144
            int smallViewPortWidth = 5 * smallViewPortHeight / 3; //we should try to maintain same ProjectionParameters aspect ratio for small cameras as the large     
            //the five side viewports in multi-screen mode
            this.viewPortDictionary.Add("column0 row0", new Viewport(0, 0, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row1", new Viewport(0, 1 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row2", new Viewport(0, 2 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row3", new Viewport(0, 3 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row4", new Viewport(0, 4 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            //the larger view to the right in column 1
            this.viewPortDictionary.Add("column1 row0", new Viewport(smallViewPortWidth, 0, screenResolution.X - smallViewPortWidth, screenResolution.Y));

            //picture-in-picture viewport
            Integer2 viewPortDimensions = new Integer2(240, 150); //set to 16:10 ratio as with screen dimensions
            int verticalOffset = 20;
            int rightHorizontalOffset = 20;
            this.viewPortDictionary.Add("PIP viewport", new Viewport((screenResolution.X - viewPortDimensions.X - rightHorizontalOffset),
                verticalOffset, viewPortDimensions.X, viewPortDimensions.Y));
        }
        private void LoadFactories()
        {
            this.primitiveFactory = new MyPrimitiveFactory();
        }

#if DEBUG
        private void InitializeDebugTextInfo()
        {
            //add debug info in top left hand corner of the screen
            this.debugDrawer = new DebugDrawer(this, this.managerParameters, spriteBatch,
                this.fontDictionary["debug"], Color.White, new Vector2(5, 5), this.eventDispatcher, StatusType.Off);
            this.debugDrawer.DrawOrder = 1;
            Components.Add(this.debugDrawer);

        }

        //draws the frustum culling spheres, the collision primitive surfaces, and the zone object collision primitive surfaces based on booleans passed
        private void InitializeDebugCollisionSkinInfo()
        {
            int primitiveCount = 0;
            PrimitiveType primitiveType;

            //used to draw spherical collision surfaces
            IVertexData sphereVertexData = new BufferedVertexData<VertexPositionColor>(
                graphics.GraphicsDevice, PrimitiveUtility.GetWireframeSphere(5, out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            this.collisionSkinDebugDrawer = new PrimitiveDebugDrawer(this, this.eventDispatcher, StatusType.Off,
                this.managerParameters, false, false, false, sphereVertexData);
            collisionSkinDebugDrawer.DrawOrder = 2;
            Components.Add(collisionSkinDebugDrawer);

        }
#endif
        #endregion

        #region Load Game
        private void LoadGame()
        {
            //non-collidable ground
            int worldScale = 300;
            int arenaScale = 30;

            this.level2Initalized = false;
            this.GameTextSize = 54;
            this.verticalTextOffset = 60;
            //Initial timer is Countdown of 5 seconds + the introCamera time

            InitializeNonCollidableGround(worldScale);
            InitializeNonCollidableSkyBox(worldScale);

            //collidable and drivable player
            InitializeCollidableTexturedPlayer(arenaScale);

            //collidable enemys
            InitializeCollidableAISpheres(arenaScale);

            InitializeArena(arenaScale);

            // Win and loose Zones
            InitializeCollidableZones();
        }
        private void InitializeLevel2()
        {

            if (!this.level2Initalized)
            {
                //Lava Speed to Raise the lava nad Loose Zone
                this.lavaSpeed = 0.001f;
                //Move Player to middle of arena
                this.playerCollidablePrimitiveObject.Transform.TranslateTo(new Vector3(0, 5, 0));
                //Load the level 2 Platforms
                InitializePlatforms();
                this.level2Initalized = true;
            }
            if (!this.opacitySet)
            {
                //Fade in Platforms from Transparent
                OpacifyPlatforms();
            }

        }


        private void InitializePlatforms()
        {
            //Places Moveing Platforms for Level 2
            this.platforms = new PlatformCollidablePrimitiveObject[13];

            this.opacitySet = false;

            InitializePlatform(0, new Transform3D(new Vector3(0, 0.5f, -70), Vector3.Zero, new Vector3(10, 2, 80), Vector3.UnitX, Vector3.UnitY)
                , new TranslationSineLerpController("transControl1", ControllerType.LerpTranslation, new Vector3(0, 1, 0), new TrigonometricParameters(10, 0.1f, 180 * (5)))
                , ShapeType.NormalCube);

            InitializePlatform(1, new Transform3D(new Vector3(95, 10f, -80), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
                , new TranslationSineLerpController("transControl2", ControllerType.LerpTranslation, new Vector3(-1, 0, 0), new TrigonometricParameters(80, 0.02f, 180 * (5)))
                , ShapeType.NormalCube);

            InitializePlatform(2, new Transform3D(new Vector3(95, 8f, -10), Vector3.Zero, new Vector3(20, 2, 120), Vector3.UnitX, Vector3.UnitY)
                , null
                , ShapeType.NormalCube);

            InitializePlatform(3, new Transform3D(new Vector3(95, 8f, 61), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , new TranslationSineLerpController("transControl3", ControllerType.LerpTranslation, new Vector3(0, 1, 0), new TrigonometricParameters(40, 0.02f, 180 * (5)))
            , ShapeType.NormalCube);

            InitializePlatform(4, new Transform3D(new Vector3(0, 40, 61), Vector3.Zero, new Vector3(160, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , null
            , ShapeType.NormalCube);

            InitializePlatform(5, new Transform3D(new Vector3(-70, 40, -40), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , new TranslationSineLerpController("transControl4", ControllerType.LerpTranslation, new Vector3(0, 0, 1), new TrigonometricParameters(80, 0.02f, 180 * (5)))
            , ShapeType.NormalCube);

            InitializePlatform(6, new Transform3D(new Vector3(-20, 40, -40), Vector3.Zero, new Vector3(80, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , null
            , ShapeType.NormalCube);

            InitializePlatform(7, new Transform3D(new Vector3(30, 40, -40), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , new TranslationSineLerpController("transControl5", ControllerType.LerpTranslation, new Vector3(0, 1, 0), new TrigonometricParameters(40, 0.02f, 180 * (5)))
            , ShapeType.NormalCube);

            InitializePlatform(8, new Transform3D(new Vector3(30, 80, 10), Vector3.Zero, new Vector3(20, 2, 80), Vector3.UnitX, Vector3.UnitY)
            , null
            , ShapeType.NormalCube);

            InitializePlatform(9, new Transform3D(new Vector3(-30, 80, 60), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , new TranslationSineLerpController("transControl6", ControllerType.LerpTranslation, new Vector3(1, 0, 0), new TrigonometricParameters(60, 0.02f, 180 * (5)))
            , ShapeType.NormalCube);

            InitializePlatform(10, new Transform3D(new Vector3(-50, 80, 30), Vector3.Zero, new Vector3(20, 2, 80), Vector3.UnitX, Vector3.UnitY)
            , null
            , ShapeType.NormalCube);

            InitializePlatform(11, new Transform3D(new Vector3(-50, 80, -20), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
            , new TranslationSineLerpController("transControl7", ControllerType.LerpTranslation, new Vector3(0, 1, 0), new TrigonometricParameters(60, 0.02f, 180 * (5)))
            , ShapeType.NormalCube);

            InitializePlatform(12, new Transform3D(new Vector3(0, 140, 10), Vector3.Zero, new Vector3(80, 2, 80), Vector3.UnitX, Vector3.UnitY)
            , null
            , ShapeType.NormalCube);


        }

        private void OpacifyPlatforms()
        {
            //Loops through the Platforms Arry and Increases there Opacity until they are fully Opaque
            int count = 0;

            for (int i = 0; i < platforms.Length; i++)
            {
                platforms[i].Alpha += 0.005f;
                if (platforms[i].Alpha == 1)
                {
                    count++;
                }
            }

            if (count == platforms.Length)
            {
                this.opacitySet = true;
            }

        }

        private void InitializeCollidableZones()
        {
            #region Win Zone
            Transform3D winTransform = null;
            SimpleZoneObject winZoneObject = null;
            ICollisionPrimitive winCollisionPrimitive = null;

            //place the zone and scale it based on how big you want the zone to be
            winTransform = new Transform3D(new Vector3(0, 150, 10), new Vector3(40, 20, 40));

            winCollisionPrimitive = new BoxCollisionPrimitive(winTransform);

            winZoneObject = new SimpleZoneObject(AppData.WinZoneID, ActorType.Zone, winTransform,
                StatusType.Drawn | StatusType.Update, winCollisionPrimitive);//, mParams);

            this.objectManager.Add(winZoneObject);
            #endregion

            #region Loose Zone
            Transform3D looseTransform = null;
            this.looseZoneObject = null;
            ICollisionPrimitive looseCollisionPrimitive = null;

            //place the zone and scale it based on how big you want the zone to be
            looseTransform = new Transform3D(new Vector3(0, -9, 0), new Vector3(300, 10, 300));

            looseCollisionPrimitive = new BoxCollisionPrimitive(looseTransform);

            this.looseZoneObject = new SimpleZoneObject(AppData.LooseZoneID, ActorType.Zone, looseTransform,
                StatusType.Drawn | StatusType.Update, looseCollisionPrimitive);//, mParams);

            this.objectManager.Add(this.looseZoneObject);
            #endregion

        }

        private void InitializePlatform(int index, Transform3D transform, IController controller, ShapeType shapeType)
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            PrimitiveObject primativeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, shapeType, effectParameters);

            primativeObject.EffectParameters.Texture = this.textureDictionary["Platform"];

            BoxCollisionPrimitive collisionPrimitive = new BoxCollisionPrimitive(transform);

            this.platforms[index] = new PlatformCollidablePrimitiveObject(primativeObject, collisionPrimitive,
                this.managerParameters, this.eventDispatcher);

            this.platforms[index].ActorType = ActorType.CollidablePlatform;

            this.platforms[index].Transform = transform;

            this.platforms[index].EffectParameters.Alpha = 0;
            #region Translation Lerp

            //Attach Move Controller to Some Platforms
            if (controller != null)
            {
                this.platforms[index].AttachController(controller);
            }

            #endregion

            this.objectManager.Add(this.platforms[index]);
        }

        private void InitializeCollidableAISpheres(int arenaScale)
        {
            //Initialize Enemy Array
            this.enemys = new PlayerCollidablePrimitiveObject[3];

            Transform3D transform;

            // to get position on edge of Arena
            float position = arenaScale - (arenaScale / 4);
            // Scale players with Arena
            float Scale = arenaScale / 5;

            #region Red Enemy
            transform = new Transform3D(new Vector3(position, Scale / 2 + 2, 0), Vector3.Zero, new Vector3(Scale, Scale, Scale), Vector3.UnitX, Vector3.UnitY);
            InitializeCollidableEnemy(arenaScale, 0, transform, this.textureDictionary["Red"], Color.Red);
            #endregion

            #region Blue Enemy
            transform = new Transform3D(new Vector3(-position, Scale / 2 + 2, 0), Vector3.Zero, new Vector3(Scale, Scale, Scale), Vector3.UnitX, Vector3.UnitY);
            InitializeCollidableEnemy(arenaScale, 1, transform, this.textureDictionary["Blue"], Color.Blue);
            #endregion

            #region Yellow Enemy
            transform = new Transform3D(new Vector3(0, Scale / 2 + 2, -position), Vector3.Zero, new Vector3(Scale, Scale, Scale), Vector3.UnitX, Vector3.UnitY);
            InitializeCollidableEnemy(arenaScale, 2, transform, this.textureDictionary["Yellow"], Color.Yellow);
            #endregion

            updateTargets();

        }


        private void InitializeArena(int arenaScale)
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            PrimitiveObject archetypeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCylinder, effectParameters);

            //set the texture that all clones will have
            archetypeObject.EffectParameters.Texture = this.textureDictionary["Platform"];

            Transform3D transform;
            CollidablePrimitiveObject collidablePrimitiveObject;

            transform = new Transform3D(new Vector3(0, 0.5f, 0), Vector3.Zero, new Vector3(arenaScale, 1, arenaScale), Vector3.UnitX, Vector3.UnitY);

            collidablePrimitiveObject = new CollidablePrimitiveObject(archetypeObject.Clone() as PrimitiveObject,
            new SphereCollisionPrimitive(transform, arenaScale), this.objectManager, this.eventDispatcher);

            collidablePrimitiveObject.ActorType = ActorType.CollidableGround;

            collidablePrimitiveObject.Transform = transform;

            this.objectManager.Add(collidablePrimitiveObject);


        }

        private void InitializeNonCollidableGround(int worldScale)
        {
            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            //get the primitive object from the factory (remember the factory returns a clone)
            this.lava = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCube, effectParameters);

            //set the texture
            this.lava.EffectParameters.Texture = this.textureDictionary["Lava"];

            //set the transform
            //since the object is 1 unit in height, we move it down to Y-axis == -0.5f so that the top of the surface is at Y == 0
            this.lava.Transform = new Transform3D(new Vector3(0, -0.5f, 0), new Vector3(worldScale, 1, worldScale));

            //set an ID if we want to access this later
            this.lava.ID = "non-collidable lava";

            //add 
            this.objectManager.Add(this.lava);

        }

        private void InitializeCollidableTexturedPlayer(int arenaScale)
        {

            float position = arenaScale - (arenaScale / 4);
            float Scale = arenaScale / 5;

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            PrimitiveObject primitiveObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalSphere, effectParameters);

            Transform3D transform = new Transform3D(new Vector3(0, Scale / 2 + 2, position), Vector3.Zero, new Vector3(Scale, Scale, Scale), -Vector3.UnitZ, Vector3.UnitY);

            SphereCollisionPrimitive collisionPrimitive = new SphereCollisionPrimitive(transform, Scale / 2);

            this.playerCollidablePrimitiveObject = new PlayerCollidablePrimitiveObject(primitiveObject, collisionPrimitive,
                this.managerParameters, AppData.PlayerOneMoveKeys, AppData.PlayerMoveSpeed, this.eventDispatcher);
            this.playerCollidablePrimitiveObject.ActorType = ActorType.Player;
            this.playerCollidablePrimitiveObject.Transform = transform;
            this.playerCollidablePrimitiveObject.EffectParameters.DiffuseColor = Color.ForestGreen;
            this.playerCollidablePrimitiveObject.EffectParameters.Texture = this.textureDictionary["Green"]; ;

            //set an ID if we want to access this later
            playerCollidablePrimitiveObject.ID = "collidable player";

            //add to the object manager
            this.objectManager.Add(playerCollidablePrimitiveObject);
        }

        private void InitializeCollidableEnemy(int arenaScale, int index, Transform3D transform, Texture2D texture, Color color)
        {

            float Scale = arenaScale / 5;

            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            //get the archetypal primitive object from the factory
            PrimitiveObject primitiveObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalSphere, effectParameters);

            //remember the primitive is at Transform3D.Zero so we need to say where we want OUR player to start

            //instanciate a box primitive at player position
            SphereCollisionPrimitive collisionPrimitive = new SphereCollisionPrimitive(transform, Scale / 2);

            //make the player object and store as field for use by the 3rd person camera - see camera initialization
            this.enemys[index] = new PlayerCollidablePrimitiveObject(primitiveObject, collisionPrimitive,
                this.managerParameters, AppData.PlayerOneMoveKeys, AppData.PlayerMoveSpeed, this.eventDispatcher);
            this.enemys[index].ActorType = ActorType.CollidableEnemy;
            this.enemys[index].Transform = transform;
            this.enemys[index].EffectParameters.DiffuseColor = color;
            this.enemys[index].EffectParameters.Texture = texture;

            //add to the object manager
            this.objectManager.Add(this.enemys[index]);
        }


        private void InitializeNonCollidableVolcanoBox(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, -5, 0), new Vector3(worldScale, 1, worldScale));

            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            //get the archetype from the factory
            PrimitiveObject Skybox = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCube, effectParameters);
            PrimitiveObject clonePlane = null;
            //set texture once so all clones have the same
            Skybox.EffectParameters.Texture = this.textureDictionary["VolcanoWall"];

            #region Skybox

            //
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);

            clonePlane.Transform.Translation = new Vector3(0, -5, (-1.0f * worldScale) / 2.0f);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);

            //As an exercise the student should add the remaining 4 skybox planes here by repeating the clone, texture assignment, rotation, and translation steps above...
            //add the left skybox plane
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-1.0f * worldScale) / 2.0f, -5, 0);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));

            this.objectManager.Add(clonePlane);

            //add the right skybox plane
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((worldScale) / 2.0f, -5, 0);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);

            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3(0, -5, (worldScale) / 2.0f);
            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);
            #endregion
        }

        private void InitializeNonCollidableSkyBox(int worldScale)
        {
            // I added A skybox Over the Volcano Box to make It seem As there Is an Outside Above the Volcano
            InitializeNonCollidableVolcanoBox(worldScale);

            worldScale *= 2;
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, -5, 0), new Vector3(worldScale, 1, worldScale));

            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            //get the archetype from the factory
            PrimitiveObject Skybox = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCube, effectParameters);
            PrimitiveObject clonePlane = null;
            //set texture once so all clones have the same
            Skybox.EffectParameters.Texture = this.textureDictionary["back"];
            //Skybox.EffectParameters.DiffuseColor = Color.White;
            #region Skybox

            //
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);

            clonePlane.Transform.Translation = new Vector3(0, 50, (-1.0f * worldScale) / 2.0f);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);

            //As an exercise the student should add the remaining 4 skybox planes here by repeating the clone, texture assignment, rotation, and translation steps above...
            //add the left skybox plane
            Skybox.EffectParameters.Texture = this.textureDictionary["left"];
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-1.0f * worldScale) / 2.0f, 50, 0);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));

            this.objectManager.Add(clonePlane);

            //add the right skybox plane
            Skybox.EffectParameters.Texture = this.textureDictionary["right"];
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((worldScale) / 2.0f, 50, 0);

            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);

            //add the top skybox plane
            Skybox.EffectParameters.Texture = this.textureDictionary["sky"];
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            clonePlane.Transform.Translation = new Vector3(0, ((worldScale) / 2.0f) - 50, 0);
            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);

            //add the front skybox plane
            Skybox.EffectParameters.Texture = this.textureDictionary["front"];
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3(0, 50, (worldScale) / 2.0f);
            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);
            #endregion
        }
        #endregion


        #region Layout Helpers
        private void InitializeHelperPrimitives()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnLitColoredPrimitivesEffectID] as BasicEffectParameters;
            //since its wireframe we dont set color, texture, alpha etc.

            //get the archetype from the factory
            PrimitiveObject archetypeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.WireframeOrigin, effectParameters);

            //clone to set the unique properties of the origin helper
            PrimitiveObject originObject = archetypeObject.Clone() as PrimitiveObject;

            //make it a little more visible!
            originObject.Transform.Translation = new Vector3(0, 10, 0);
            originObject.Transform.Scale *= 4;

            //set an ID if we want to access this later
            originObject.ID = "origin helper";

            //add to the object manager
            this.objectManager.Add(originObject);

        }
        #endregion

        #region Initialize Cameras
        private void InitializeCameras(Integer2 screenResolution)
        {
            InitializeThirdPersonCamera(screenResolution);
            InitializeIntroCamera(screenResolution);
            InitializeLevel1Camera(screenResolution);
            InitializeLevel2IntroCamera(screenResolution);
        }
        private void InitializeCamera(Integer2 screenResolution, string id, Viewport viewPort, Transform3D transform, IController controller, float drawDepth, StatusType statusType)
        {
            Camera3D camera = new Camera3D(id, ActorType.Camera, transform, ProjectionParameters.StandardShallowSixteenNine, viewPort, drawDepth, statusType);

            if (controller != null)
                camera.AttachController(controller);

            this.cameraManager.Add(camera);
        }

        //adds three camera from 3 different perspectives that we can cycle through
        private void InitializeThirdPersonCamera(Integer2 screenResolution)
        {
            #region Flight Camera
            Transform3D transform = new Transform3D(new Vector3(0, 45, 65), new Vector3(0, -0.6f, -0.8f), Vector3.UnitY);

            IController controller = new FlightCameraController("fcc", ControllerType.FirstPerson, AppData.CameraMoveKeys,
               AppData.CameraMoveSpeed, AppData.CameraStrafeSpeed, AppData.CameraRotationSpeed, this.managerParameters);

            //InitializeCamera(screenResolution, AppData.FlightCameraID, this.viewPortDictionary["full viewport"], transform, null, 0, StatusType.Update);
            #endregion

            #region Third Person Camera
            //if (this.playerCollidablePrimitiveObject != null) //if demo 4 then we have player to track
            //{
            //position is irrelevant since its based on tracking a player object
            transform = Transform3D.Zero;

            controller = new ThirdPersonController("tpc", ControllerType.ThirdPerson, this.playerCollidablePrimitiveObject,
                AppData.CameraThirdPersonDistance, AppData.CameraThirdPersonScrollSpeedDistanceMultiplier,
                AppData.CameraThirdPersonElevationAngleInDegrees, AppData.CameraThirdPersonScrollSpeedElevationMultiplier,
                LerpSpeed.Medium, LerpSpeed.Fast, this.mouseManager);

            InitializeCamera(screenResolution, AppData.ThirdPersonCameraID, this.viewPortDictionary["full viewport"], transform, controller, 0, StatusType.Update);
            //}
            #endregion

        }


        private void InitializeIntroCamera(Integer2 screenResolution)
        {

            Transform3D transform = null;

            string viewportDictionaryKey = "full viewport";

            //track camera 1

            transform = new Transform3D(new Vector3(0, 0, 20), -Vector3.UnitZ, Vector3.UnitY);
            this.IntroCurvecontroller = new CurveController(AppData.IntroCurveCameraID + " controller", ControllerType.Track, this.curveDictionary["introCurveCamera"], PlayStatusType.Play);
            InitializeCamera(screenResolution, AppData.IntroCurveCameraID, this.viewPortDictionary[viewportDictionaryKey], transform, this.IntroCurvecontroller, 0, StatusType.Off);

        }

        private void InitializeLevel1Camera(Integer2 screenResolution)
        {

            Transform3D transform = new Transform3D(new Vector3(0, 45, 65), new Vector3(0, -0.6f, -0.8f), Vector3.UnitY);

            string viewportDictionaryKey = "full viewport";

            InitializeCamera(screenResolution, AppData.Level1FixedCameraID, this.viewPortDictionary[viewportDictionaryKey], transform, null, 0, StatusType.Off);

        }

        private void InitializeLevel2IntroCamera(Integer2 screenResolution)
        {

            Transform3D transform = new Transform3D(new Vector3(-145, 100, 75), new Vector3(0.8f, -0.4f, -0.4f), Vector3.UnitY);

            string viewportDictionaryKey = "full viewport";

            InitializeCamera(screenResolution, AppData.Level2FixedCameraID, this.viewPortDictionary[viewportDictionaryKey], transform, null, 0, StatusType.Off);

        }


        #endregion

        #region Events
        private void InitializeEventDispatcher()
        {
            //initialize with an arbitrary size based on the expected number of events per update cycle, increase/reduce where appropriate
            this.eventDispatcher = new EventDispatcher(this, 20);

            //dont forget to add to the Component list otherwise EventDispatcher::Update won't get called and no event processing will occur!
            Components.Add(this.eventDispatcher);
        }

        private void StartGame()
        {

            EventDispatcher.Publish(new EventData(EventActionType.OnObjectPicked, EventCategoryType.ObjectPicking));
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));
            object[] additionalParametersSound = { "Music" };
            EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));
            //publish an event to set the camera

            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));
        }

        protected void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.GameStateChanged += EventDispatcher_GameStateChanged;
            eventDispatcher.LavaSpeedChanged += EventDispatcher_LavaSpeedChanged;
            eventDispatcher.MenuTextChanged += EventDispatcher_MenuTextChanged;
            eventDispatcher.MenuChanged += EventDispatcher_MenuChanged;
        }
        protected void EventDispatcher_GameStateChanged(EventData eventData)
        {

            this.gameState = (GameState)Enum.Parse(typeof(GameState), eventData.AdditionalParameters[0].ToString());
        }

        protected void EventDispatcher_LavaSpeedChanged(EventData eventData)
        {
            this.lavaSpeed = (float)eventData.AdditionalParameters[0];
        }

        protected void EventDispatcher_MenuTextChanged(EventData eventData)
        {
            this.MenutextClone.Text = eventData.AdditionalParameters[0].ToString();
        }

        protected void EventDispatcher_MenuChanged(EventData eventData)
        {
            if (eventData.AdditionalParameters != null)
            {
                this.restarting = true;
                Restart();
            }

        }
        #endregion

        #region Menu & UI
        private void AddMenuElements()
        {
            Transform2D transform = null;
            Texture2D texture = null;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            string sceneID = "", buttonID = "", buttonText = "";
            int verticalBtnSeparation = 100;

            #region Main Menu
            sceneID = "main menu";

            //retrieve the background texture
            texture = this.textureDictionary["mainmenu"];
            //scale the texture to fit the entire screen
            Vector2 scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);

            this.menuManager.Add(sceneID, new UITextureObject("mainmenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.GhostWhite, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add start button
            buttonID = "startbtn";
            buttonText = "Start";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200);
            texture = this.textureDictionary["genericbtn"];
            transform = new Transform2D(position,
                0, new Vector2(0.5f, 0.5f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.GhostWhite, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.Black, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            this.menuManager.Add(sceneID, uiButtonObject);

            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
        new TrigonometricParameters(1, 0.4f, 0), Color.LightSeaGreen, Color.LightGreen));

            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "audiobtn";
            clone.Text = "Audio";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add controls button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Text = "Controls";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Text = "Exit";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), new Color(255, 156, 0), new Color(200, 50, 50)));//Colors are orange and red
            this.menuManager.Add(sceneID, clone);


            #endregion

            #region Audio Menu
            sceneID = "audio menu";

            //retrieve the audio menu background texture
            texture = this.textureDictionary["audiomenu"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("audiomenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));


            //add volume up button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "volumeUpbtn";
            clone.Text = "Volume Up";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add volume down button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            clone.ID = "volumeDownbtn";
            clone.Text = "Volume Down";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            clone.ID = "volumeMutebtn";
            clone.Text = "Volume Mute";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            clone.ID = "volumeUnMutebtn";
            clone.Text = "Volume Un-mute";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 4 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);
            #endregion

            #region Controls Menu
            sceneID = "controls menu";

            //retrieve the controls menu background texture
            texture = this.textureDictionary["controlsmenu"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("controlsmenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 9 * (verticalBtnSeparation / 2));
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.GhostWhite;
            this.menuManager.Add(sceneID, clone);
            #endregion

        }

        private void AddRestartBtn()
        {
            //Adds restart Button to the menu Once the game has Begun
            if (!this.restartButtonAdded)
            {
                //this.menuManager
                string buttonID = "restartbtn";
                string buttonText = "Restart";
                Vector2 position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 600);
                Texture2D texture = this.textureDictionary["genericbtn"];
                Transform2D transform = new Transform2D(position,
                    0, new Vector2(0.5f, 0.5f),
                    new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

                UIButtonObject uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                    transform, Color.GhostWhite, SpriteEffects.None, 0.1f, texture, buttonText,
                    this.fontDictionary["menu"],
                    Color.Black, new Vector2(0, 2));

                uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
                  new TrigonometricParameters(0.1f, 0.2f, 1)));


                uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
            new TrigonometricParameters(1, 0.4f, 0), Color.LightSeaGreen, Color.LightGreen));


                this.menuManager.Add("main menu", uiButtonObject);

                this.restartButtonAdded = true;
            }


        }

        private void AddUIElements()
        {
            InitializeUIMousePointer();
            //InitializeUIProgress();
            InitializeGameStateText();
        }

        private void InitializeUIMousePointer()
        {
            Texture2D texture = this.textureDictionary["reticuleDefault"];
            //show complete texture
            Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height);

            //listens for object picking events from the object picking manager
            UIPickingMouseObject myUIMouseObject = new UIPickingMouseObject("picking mouseObject",
                ActorType.UITexture,
                new Transform2D(Vector2.One),
                this.fontDictionary["mouse"],
                "",
                new Vector2(0, 40),
                texture,
                this.mouseManager,
                this.eventDispatcher);
            this.MenuUIManager.Add(myUIMouseObject);
        }

        private void InitializeUIProgress()
        {
            float separation = 20; //spacing between progress bars

            Transform2D transform = null;
            Texture2D texture = null;
            UITextureObject textureObject = null;
            Vector2 position = Vector2.Zero;
            Vector2 scale = Vector2.Zero;
            float verticalOffset = 20;
            int startValue;

            texture = this.textureDictionary["progress_gradient"];
            scale = new Vector2(1, 0.75f);

            #region Player 1 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f - texture.Width * scale.X - separation, verticalOffset);
            transform = new Transform2D(position, 0, scale,
                Vector2.Zero, /*new Vector2(texture.Width/2.0f, texture.Height/2.0f),*/
                new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerOneProgressID,
                    ActorType.UITexture,
                    StatusType.Drawn | StatusType.Update,
                    transform, Color.Green,
                    SpriteEffects.None,
                    1,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            startValue = 3; //just a random number between 0 and max to demonstrate we can set initial progress value
            textureObject.AttachController(new UIProgressController(AppData.PlayerOneProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));
            this.uiManager.Add(textureObject);
            #endregion


            #region Player 2 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f + separation, verticalOffset);
            transform = new Transform2D(position, 0, scale, Vector2.Zero, new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerTwoProgressID,
                    ActorType.UITexture,
                    StatusType.Drawn | StatusType.Update,
                    transform,
                    Color.Red,
                    SpriteEffects.None,
                    1,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            startValue = 7; //just a random number between 0 and max to demonstrate we can set initial progress value
            textureObject.AttachController(new UIProgressController(AppData.PlayerTwoProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));
            this.uiManager.Add(textureObject);
            #endregion
        }

        private void InitializeGameStateText()
        {
            string initalText = "";
            Transform2D transform = null;

            this.GameStateText = null;
            Vector2 position = Vector2.Zero;

            Vector2 scale = Vector2.Zero;


            scale = Vector2.One;

            //to center align horizontaly, it is half window width - lenght of text multiplyed by Scale and fontSize divided by 2
            position = new Vector2((GraphicsDevice.Viewport.Width - (initalText.Length * 60)) / 2, this.verticalTextOffset);

            transform = new Transform2D(position, 0, scale,
                Vector2.Zero, /*new Vector2(texture.Width/2.0f, texture.Height/2.0f),*/
                new Integer2(1, 1));


            this.GameStateText = new UITextObject("GameText",
                    ActorType.UIDynamicText,
                    StatusType.Drawn | StatusType.Update,
                    transform, new Color(200, 200, 200),//Light Grey
                    SpriteEffects.None,
                    1,
                    initalText,
                    this.fontDictionary["GameText"]);


            //textObject.AttachController(new UIProgressController(AppData.PlayerOneProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));

            this.uiManager.Add(this.GameStateText);


            this.textClone = (this.GameStateText.Clone() as UITextObject);


            this.textClone.Transform.Scale = new Vector2(0.5f, 0.5f);

            this.textClone.Color = new Color(40, 40, 40);//Dark Grey

            this.uiManager.Add(textClone);

            this.MenutextClone = (this.GameStateText.Clone() as UITextObject);

            this.MenutextClone.Transform.Scale = new Vector2(0.4f, 0.4f);

            this.MenutextClone.Color = Color.WhiteSmoke;

            this.MenutextClone.Text = "";

            this.MenutextClone.Transform.Translation = new Vector2(GraphicsDevice.Viewport.Width / 12, 160);
            this.MenuUIManager.Add(this.MenutextClone);


        }
        private void CheckGameState(GameTime gameTime)
        {
            //Updates the game text
            UpdateGameText(gameTime);
            if (this.gameState == GameState.Level1)
            {
                //Vhecks for win on level 1
                checkLevel1Win(gameTime);
            }
            //Initialize level 2 intro
            if (this.gameState == GameState.Level2Intro)
            {
                if (this.timer.EndTime >= -12)
                {
                    InitializeLevel2();
                }

            }
            if (this.gameState == GameState.Level2)
            {
                RaiseLava(gameTime);
            }

        }
        private void UpdateGameText(GameTime gameTime)
        {
            //Game text is Centered By Multiplying the length of the text By the size, Taking that away from the width then dividing by 2
            //Checks gamestate and sets appropriate text
            if (!this.restarting)
            {
                switch (this.gameState)
                {
                    case GameState.NotStarted:
                        this.GameStateText.Text = "";
                        this.InitialTimerTime = 24 + (int)gameTime.TotalGameTime.TotalSeconds;
                        break;
                    case GameState.CountDown:
                        AddRestartBtn();
                        DoCountDown(GameState.Level1, gameTime);
                        break;
                    case GameState.Level1:
                        this.GameStateText.Text = "";
                        break;
                    case GameState.Level2Intro:
                        DoCountDown(GameState.Level2, gameTime);
                        break;
                    case GameState.Level2:
                        this.GameStateText.Text = "";
                        break;
                    case GameState.Won:
                        this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (8 * this.GameTextSize)) / 2, this.verticalTextOffset);
                        this.GameStateText.Text = "YOU WIN!";
                        DisplayMessage("Play Again From Pause Menu");
                        break;
                    case GameState.Lost:
                        this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (10 * this.GameTextSize)) / 2, this.verticalTextOffset);
                        this.GameStateText.Text = "Game Over!";
                        DisplayMessage("Restart From Pause Menu");
                        break;
                }
            }
            else
            {
                CheckRestart(gameTime);
            }

        }

        private void CheckRestart(GameTime gameTime)
        {
            //Times 2 seconds for a restart event
            if (!this.restartTimeSet)
            {
                this.restartTime = (int)gameTime.TotalGameTime.TotalSeconds;
                this.restartTimeSet = true;
            }

            if ((this.restartTime + 1) < (int)gameTime.TotalGameTime.TotalSeconds)
            {
                this.restarting = false;
                this.restartTimeSet = false;
                DisplayMessage("");
            }

        }


        private void DoCountDown(GameState gamestate, GameTime gameTime)
        {
            //If the current timer is not finished
            if (!this.timer.IsComplete)
            {
                //timer time from last update
                int previousTimerTime = this.timer.EndTime;
                this.timer.set(gameTime, InitialTimerTime);
                int timerTime = this.timer.EndTime;

                //If timer has reached 0
                //Note Timer counts forward from a negative number and just displays the absolute value
                if (timerTime > 0)
                {
                    //this.timer.reset();
                    //Publish event to change GameState
                    object[] additionalParameters = { gamestate };
                    EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));

                    if (gamestate == GameState.Level2)
                    {
                        if (!this.lavaTimerSet)
                        {
                            this.lavaTimer = (int)gameTime.TotalGameTime.TotalSeconds + 180;
                            this.lavaTimerSet = true;
                        }
                    }
                    DisplayMessage("");
                }
                else if (timerTime == 0)
                {
                    if (previousTimerTime != this.timer.EndTime)//Will only play sound Once
                    {
                        object[] additionalParametersSound = { "FinalBeep" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));
                    }

                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (3 * GameTextSize)) / 2, this.verticalTextOffset);
                    this.GameStateText.Text = "GO!";

                }
                else if (timerTime >= -5)
                {
                    //Stops the camera being reset
                    this.introCameraSkipped = true;

                    //Centers the gametext 
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (1 * GameTextSize)) / 2, this.verticalTextOffset);
                    //Sets the gametext to the tiomers absolute value
                    this.GameStateText.Text = this.timer.Display;

                    if (previousTimerTime != this.timer.EndTime)
                    {
                        //Play sound Once a second
                        object[] additionalParametersSound = { "CountDownBeep" };
                        EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParametersSound));
                    }

                    if (gamestate == GameState.Level2)
                    {
                        if (!this.thirdPersonEventSent)
                        {
                            //Switch to third person
                            object[] additionalEventParamsB = { AppData.ThirdPersonCameraID };
                            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));

                            //Sets the active Camera to update
                            EventDispatcher.Publish(new EventData(EventActionType.OnCameraResume, EventCategoryType.Camera));

                            this.thirdPersonEventSent = true;
                        }

                        //Display message to player
                        this.textClone.Color = this.GameStateText.Color;
                        DisplayMessage("The Lava is Rising!");
                    }
                    else
                    {
                        //Set display message to nothing
                        DisplayMessage("");
                    }

                }
                else if (this.timer.EndTime >= -10 && gamestate == GameState.Level2)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (19 * GameTextSize)) / 2, this.verticalTextOffset);
                    this.GameStateText.Text = "ESCAPE THE VOLCANO!";
                }
                else if (this.timer.EndTime >= -15 && gamestate == GameState.Level2)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (7 * GameTextSize)) / 2, this.verticalTextOffset);
                    this.GameStateText.Text = "Level 2";
                }
                else
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (10 * GameTextSize)) / 2, this.verticalTextOffset);
                    this.GameStateText.Text = "GET READY!";

                    DisplayMessage("Press Space To Skip");
                }
            }
        }

        private void DisplayMessage(string text)
        {
            this.textClone.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (text.Length * (GameTextSize / 2))) / 2, this.verticalTextOffset * 10);
            this.textClone.Text = text;
        }

        private void RaiseLava(GameTime gameTime)
        {
            //Sets Lava timer, Lava gets faster over time
            this.timer.set(gameTime, lavaTimer);
            if (this.timer.EndTime >= -45)
            {
                this.lavaSpeed = 0.015f;
            }
            else if (this.timer.EndTime >= -100)
            {
                this.lavaSpeed = 0.012f;
            }
            else if (this.timer.EndTime >= -130)
            {
                this.lavaSpeed = 0.008f;
            }
            else if (this.timer.EndTime >= -150)
            {
                this.lavaSpeed = 0.003f;
            }

            //Moves the lava ground and the kill zone up
            this.lava.Transform.TranslateBy(new Vector3(0, this.lavaSpeed, 0));
            this.looseZoneObject.Transform.TranslateBy(new Vector3(0, this.lavaSpeed, 0));
        }
        #endregion

        #region Effects
        private void InitializeEffects()
        {
            BasicEffect basicEffect = null;

            #region For unlit colored primitive objects incl. simple lines and wireframe primitives
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;
            this.effectDictionary.Add(AppData.UnLitColoredPrimitivesEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For unlit textured primitive objects
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;
            this.effectDictionary.Add(AppData.UnLitTexturedPrimitivesEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For lit (i.e. normals defined) textured primitive objects
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;
            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;
            this.effectDictionary.Add(AppData.LitTexturedPrimitivesEffectID, new BasicEffectParameters(basicEffect));
            #endregion

        }
        #endregion

        private void checkLevel1Win(GameTime gameTime)
        {
            int count = 0;
            for (int i = 0; i < enemys.Length; i++)
            {
                if (!enemys[i].InGame)
                {
                    count++;
                }
            }
            //If the enmeys are all out of the game and the player is still in the game
            if (count == 3 && this.playerCollidablePrimitiveObject.InGame)
            {
                //this.timer.reset();
                this.timer.PauseTime = 0;

                this.InitialTimerTime = (int)(gameTime.TotalGameTime.TotalSeconds + 15);

                object[] additionalEventParamsB = { AppData.Level2FixedCameraID };
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));

                object[] additionalParameters = { GameState.Level2Intro };
                EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));


            }
        }

        protected void updateTargets()
        {
            // give each ai A list of targets Not Including themselves
            PlayerCollidablePrimitiveObject[] enemey0targets = new PlayerCollidablePrimitiveObject[3];
            PlayerCollidablePrimitiveObject[] enemey1targets = new PlayerCollidablePrimitiveObject[3];
            PlayerCollidablePrimitiveObject[] enemey2targets = new PlayerCollidablePrimitiveObject[3];


            enemey0targets[0] = playerCollidablePrimitiveObject;
            enemey1targets[0] = playerCollidablePrimitiveObject;
            enemey2targets[0] = playerCollidablePrimitiveObject;

            enemey0targets[1] = enemys[1];
            enemey0targets[2] = enemys[2];

            enemey1targets[1] = enemys[0];
            enemey1targets[2] = enemys[2];

            enemey2targets[1] = enemys[0];
            enemey2targets[2] = enemys[1];

            enemys[0].Targets = enemey0targets;

            enemys[1].Targets = enemey1targets;

            enemys[2].Targets = enemey2targets;

        }
        #region Content, Update, Draw        

        protected override void UnloadContent()
        {
            //formally call garbage collection on all ContentDictionary objects to de-allocate resources from RAM
            this.textureDictionary.Dispose();
            this.fontDictionary.Dispose();
            this.videoDictionary.Dispose();

        }


        protected override void Update(GameTime gameTime)
        {

            //exit using new gamepad manager
            if (this.gamePadManager != null && this.gamePadManager.IsPlayerConnected(PlayerIndex.One) && this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                this.Exit();

            //Checks Game State and updates accordingly e.g if level 2 raise the lava
            CheckGameState(gameTime);

            DoToggleFullScreen();
            DoSkipCamera();
#if DEMO
            //DoDebugToggleDemo();
            // DoCameraCycle();
#endif


            base.Update(gameTime);

        }

        private void DoCameraCycle()
        {
            //if (this.keyboardManager.IsFirstKeyPress(Keys.F1))
            //{
            //    EventDispatcher.Publish(new EventData(EventActionType.OnCameraCycle, EventCategoryType.Camera));
            //}
        }

        private void DoDebugToggleDemo()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                //toggle the boolean variables directly in the debug drawe to show CDCR surfaces
                this.collisionSkinDebugDrawer.ShowCollisionSkins = !this.collisionSkinDebugDrawer.ShowCollisionSkins;
                this.collisionSkinDebugDrawer.ShowZones = !this.collisionSkinDebugDrawer.ShowZones;
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                //toggle the boolean variables directly in the debug drawer to show the frustum culling bounding spheres
                this.collisionSkinDebugDrawer.ShowFrustumCullingSphere = !this.collisionSkinDebugDrawer.ShowFrustumCullingSphere;
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.F7))
            {
                //we can turn all debug off 
                EventDispatcher.Publish(new EventData(EventActionType.OnToggleDebug, EventCategoryType.Debug));
            }



        }

        private void DoToggleFullScreen()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                graphics.ToggleFullScreen();
            }
        }

        private void DoSkipCamera()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.Space))
            {

                if (!this.introCameraSkipped)
                {
                    //Set Timer to 5 seconds
                    object[] additionalEventParamsTime = { 5 };
                    EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.Timer, additionalEventParamsTime));
                    //Set camera
                    object[] additionalEventParamsB = { AppData.Level1FixedCameraID };
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));

                    //set bool to true, to stop events being sent multiple times
                    this.introCameraSkipped = true;
                }

            }
        }
        protected override void Draw(GameTime gameTime)
        {
            //Obviously has to be Cornflower Blue
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
        #endregion
    }
}
