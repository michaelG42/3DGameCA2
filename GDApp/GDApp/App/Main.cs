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
//test commit
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
        #region Statics
        private readonly Color GoogleGreenColor = new Color(152, 234, 224, 225);
        #endregion

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
        private int InitialTimerTime;
        private int lavaTimer;
        private UITextObject GameStateText;
        private GameState gameState;
        private SimpleZoneObject looseZoneObject;
        private float lavaSpeed;

        private bool level2Initalized;
        private bool opacitySet;
        private bool thirdPersonEventSent;
        private bool introCameraSkipped;
        private bool lavaTimerSet;
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

            bool isMouseVisible = true;
            Integer2 screenResolution = ScreenUtility.HD720;
            ScreenUtility.ScreenType screenType = ScreenUtility.ScreenType.SingleScreen;
            int numberOfGamePadPlayers = 1;

            //set the title
            Window.Title = "SUMO";

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
            InitializeDebugTextInfo();
            InitializeDebugCollisionSkinInfo();
#endif
            RegisterForEventHandling(this.eventDispatcher);
            base.Initialize();
        }

       
        private void InitializeManagers(Integer2 screenResolution, 
            ScreenUtility.ScreenType screenType, bool isMouseVisible, int numberOfGamePadPlayers) //1 - 4
        {
            //add sound manager
            this.soundManager = new SoundManager(this, this.eventDispatcher, StatusType.Update, "Content/Assets/Audio/", "Demo2DSound.xgs", "WaveBank1.xwb", "SoundBank1.xsb");
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
        
            //this object packages together all managers to give the mouse object the ability to listen for all forms of input from the user, as well as know where camera is etc.
            this.managerParameters = new ManagerParameters(this.objectManager,
                this.cameraManager, this.mouseManager, this.keyboardManager, this.gamePadManager, this.screenManager, this.soundManager);


            //used for simple picking (i.e. non-JigLibX)
            this.pickingManager = new MySimplePickingManager(this, this.eventDispatcher, StatusType.Update, this.managerParameters);
            Components.Add(this.pickingManager);

            //CountDown timer
            this.timer = new Timer();
       
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
            //this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1"); //demo use of the shorter form of Load() that generates key from asset name
            //this.textureDictionary.Load("Assets/Textures/Props/Crates/crate2");
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
            //this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            this.textureDictionary.Load("Assets/Textures/Skybox/back");
            this.textureDictionary.Load("Assets/Textures/Skybox/left");
            this.textureDictionary.Load("Assets/Textures/Skybox/right");
            this.textureDictionary.Load("Assets/Textures/Skybox/sky");
            this.textureDictionary.Load("Assets/Textures/Skybox/front");
            //this.textureDictionary.Load("Assets/Textures/Foliage/Trees/tree2");
            this.textureDictionary.Load("Assets/Textures/Enviornment/Lava");
            this.textureDictionary.Load("Assets/Textures/Enviornment/VolcanoWall");
            //this.textureDictionary.Load("Assets/Textures/Enviornment/Space");


            //this.textureDictionary.Load("Assets/Textures/Semitransparent/transparentstripes");


            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");

            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mainmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/audiomenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/controlsmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/exitmenuwithtrans");

            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");


            //dual texture demo - see Main::InitializeCollidableGround()
            //this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard_greywhite");

            //levels
           // this.textureDictionary.Load("Assets/Textures/Level/level1");


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

            #region Rails
            //create the track to be applied to the non-collidable track camera 1
            //this.railDictionary.Add("rail1 - parallel to x-axis", new RailParameters("rail1 - parallel to x-axis", new Vector3(-80, 10, 40), new Vector3(80, 10, 40)));
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
                verticalOffset,  viewPortDimensions.X, viewPortDimensions.Y));
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

            this.collisionSkinDebugDrawer = new PrimitiveDebugDrawer(this, this.eventDispatcher, StatusType.Update | StatusType.Drawn,
                this.managerParameters, true, true, true, sphereVertexData);
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

            //Initial timer is Countdown of 5 seconds + the introCamera time
            this.InitialTimerTime = 24;
            InitializeNonCollidableGround(worldScale);
            InitializeNonCollidableSkyBox(worldScale);
           // InitializeCollidableDecorators();
            //collidable and drivable player
            InitializeCollidablePlayer(arenaScale);

            //collidable objects that we can turn on when we hit them
            InitializeCollidableAISpheres(arenaScale);

            InitializeArena(arenaScale);

            //InitializePlatforms();
            InitializeCollidableZones();
        }
        private void InitializeLevel2()
        {

            if (!this.level2Initalized)
            {
                this.lavaSpeed = 0.001f;
                this.playerCollidablePrimitiveObject.Transform.TranslateTo(new Vector3(0,5,0));
                InitializePlatforms();
                this.level2Initalized = true;
            }
            if(!opacitySet)
            {
                OpacifyPlatforms();
            }
            
        }


        private void InitializePlatforms()
        {
            this.platforms = new PlatformCollidablePrimitiveObject[13];

            this.opacitySet = false;

            InitializePlatform(0, new Transform3D(new Vector3(0, 0.5f, -70), Vector3.Zero, new Vector3(10, 2, 80), Vector3.UnitX, Vector3.UnitY)
                , new TranslationSineLerpController("transControl1", ControllerType.LerpTranslation, new Vector3(0, 1, 0), new TrigonometricParameters(10, 0.1f, 180 * (5)))
                , ShapeType.NormalCube);

            InitializePlatform(1, new Transform3D(new Vector3(95, 10f, -80), Vector3.Zero, new Vector3(20, 2, 20), Vector3.UnitX, Vector3.UnitY)
                , new TranslationSineLerpController("transControl2", ControllerType.LerpTranslation, new Vector3(-1, 0, 0), new TrigonometricParameters(80, 0.02f, 180 * (5)))
                , ShapeType.NormalCube);

            InitializePlatform(2, new Transform3D(new Vector3(95, 8f, -10), Vector3.Zero, new Vector3(20, 2, 120), Vector3.UnitX, Vector3.UnitY)
                ,null
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
            int count = 0;

                for (int i = 0; i < platforms.Length; i++)
                {
                    platforms[i].Alpha += 0.005f;
                if(platforms[i].Alpha == 1)
                {
                    count++;
                }
                }

            if(count == platforms.Length)
            {
                this.opacitySet = true;
            }

            //Console.WriteLine("ALPHA ids");


        }

        private void InitializeCollidableZones()
        {
            #region Win Zone
            Transform3D winTransform = null;
            SimpleZoneObject winZoneObject = null;
            ICollisionPrimitive winCollisionPrimitive = null;

            //place the zone and scale it based on how big you want the zone to be
            winTransform = new Transform3D(new Vector3(0, 150, 10), new Vector3(80, 20, 80));

            //we can have a sphere or a box - its entirely up to the developer
            winCollisionPrimitive = new BoxCollisionPrimitive(winTransform);
            //collisionPrimitive = new BoxCollisionPrimitive(transform);

            winZoneObject = new SimpleZoneObject(AppData.WinZoneID, ActorType.Zone, winTransform,
                StatusType.Drawn | StatusType.Update, winCollisionPrimitive);//, mParams);

            this.objectManager.Add(winZoneObject);
            #endregion

            #region Loose Zone
            Transform3D looseTransform = null;
            this.looseZoneObject = null;
            ICollisionPrimitive looseCollisionPrimitive = null;

            //place the zone and scale it based on how big you want the zone to be
            looseTransform = new Transform3D(new Vector3(0, -8, 0), new Vector3(300, 10, 300));

            //we can have a sphere or a box - its entirely up to the developer
            looseCollisionPrimitive = new BoxCollisionPrimitive(looseTransform);
            //collisionPrimitive = new BoxCollisionPrimitive(transform);

            this.looseZoneObject = new SimpleZoneObject(AppData.LooseZoneID, ActorType.Zone, looseTransform,
                StatusType.Drawn | StatusType.Update, looseCollisionPrimitive);//, mParams);

            this.objectManager.Add(this.looseZoneObject);
            #endregion


        }
        private void InitializePlatform(int index, Transform3D transform, IController controller, ShapeType shapeType)
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            PrimitiveObject primativeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, shapeType, effectParameters);

            //set the texture that all clones will have
            primativeObject.EffectParameters.Texture = this.textureDictionary["ml"];

  

            BoxCollisionPrimitive collisionPrimitive = new BoxCollisionPrimitive(transform);

            this.platforms[index] = new PlatformCollidablePrimitiveObject(primativeObject, collisionPrimitive,
                this.managerParameters, this.eventDispatcher);

            this.platforms[index].ActorType = ActorType.CollidablePlatform;

            this.platforms[index].Transform = transform;

            this.platforms[index].EffectParameters.Alpha = 0;
            #region Translation Lerp
            //if we want to make the boxes move (or do something else) then just attach a controller
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
            InitializeCollidableEnemy(arenaScale, 0, transform, Color.Red);
            #endregion

            #region Blue Enemy
            transform = new Transform3D(new Vector3(-position, Scale / 2 + 2, 0), Vector3.Zero, new Vector3(Scale, Scale, Scale), Vector3.UnitX, Vector3.UnitY);
            InitializeCollidableEnemy(arenaScale, 1, transform, Color.Blue);
            #endregion

            #region Yellow Enemy
            transform = new Transform3D(new Vector3(0, Scale / 2 + 2, -position), Vector3.Zero, new Vector3(Scale, Scale, Scale), Vector3.UnitX, Vector3.UnitY);
            InitializeCollidableEnemy(arenaScale, 2, transform, Color.Yellow);
            #endregion

            updateTargets();

        }

        private void InitializeCollidableDecorators()
        {
            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            //get the archetypal primitive object from the factory
            PrimitiveObject archetypeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCube, effectParameters);

            //set the texture that all clones will have
            archetypeObject.EffectParameters.Texture = this.textureDictionary["checkerboard"];

            Transform3D transform;
            CollidablePrimitiveObject collidablePrimitiveObject;

            for (int i = 0; i < 10; i++)
            {
                //remember the primitive is at Transform3D.Zero so we need to say where we want OUR player to start
                transform = new Transform3D(new Vector3(5 * i + 10, 2, 0), Vector3.Zero, new Vector3(2, 2, 2), Vector3.UnitX, Vector3.UnitY);

                //make the collidable primitive
                collidablePrimitiveObject = new CollidablePrimitiveObject(archetypeObject.Clone() as PrimitiveObject,
                    new BoxCollisionPrimitive(transform), this.objectManager, this.eventDispatcher);

                //do we want an actor type for CDCR?
                collidablePrimitiveObject.ActorType = ActorType.CollidableDecorator;

                //set the position otherwise the boxes will all have archetypeObject.Transform positional properties
                collidablePrimitiveObject.Transform = transform;

                this.objectManager.Add(collidablePrimitiveObject);
            }
        }

        private void InitializeArena(int arenaScale)
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitTexturedPrimitivesEffectID] as BasicEffectParameters;

            PrimitiveObject archetypeObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.NormalCylinder, effectParameters);

            //set the texture that all clones will have
            archetypeObject.EffectParameters.Texture = this.textureDictionary["ml"];

            Transform3D transform;
            CollidablePrimitiveObject collidablePrimitiveObject;
            // IController controller;

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

        private void InitializeCollidablePlayer(int arenaScale)
        {

            float position = arenaScale - (arenaScale / 4);
            float Scale = arenaScale / 5;


            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnLitColoredPrimitivesEffectID] as BasicEffectParameters;
          
            //get the archetypal primitive object from the factory
            PrimitiveObject primitiveObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.ColoredSphere, effectParameters);

            //remember the primitive is at Transform3D.Zero so we need to say where we want OUR player to start
            Transform3D transform = new Transform3D(new Vector3(0, Scale / 2 + 2, position), Vector3.Zero, new Vector3(Scale, Scale, Scale), -Vector3.UnitZ, Vector3.UnitY);
            //Transform3D transform = new Transform3D(new Vector3(0, 45, 61), Vector3.Zero, new Vector3(Scale, Scale, Scale), -Vector3.UnitZ, Vector3.UnitY);

            //instanciate a box primitive at player position
            SphereCollisionPrimitive collisionPrimitive = new SphereCollisionPrimitive(transform, Scale / 2);

            //make the player object and store as field for use by the 3rd person camera - see camera initialization
            this.playerCollidablePrimitiveObject = new PlayerCollidablePrimitiveObject(primitiveObject, collisionPrimitive,
                this.managerParameters, AppData.PlayerOneMoveKeys, AppData.PlayerMoveSpeed, this.eventDispatcher);
            this.playerCollidablePrimitiveObject.ActorType = ActorType.Player;
            this.playerCollidablePrimitiveObject.Transform = transform;
            this.playerCollidablePrimitiveObject.EffectParameters.DiffuseColor = Color.ForestGreen;

            //do we want a texture?
            // playerCollidablePrimitiveObject.EffectParameters.Texture = this.textureDictionary["ml"];

            //set an ID if we want to access this later
            playerCollidablePrimitiveObject.ID = "collidable player";

            //add to the object manager
            this.objectManager.Add(playerCollidablePrimitiveObject);
        }

        private void InitializeCollidableEnemy(int arenaScale, int index, Transform3D transform, Color color)
        {

           // float position = arenaScale - (arenaScale / 4);
            float Scale = arenaScale / 5;

            //get the effect relevant to this primitive type (i.e. colored, textured, wireframe, lit, unlit)
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnLitColoredPrimitivesEffectID] as BasicEffectParameters;

            //get the archetypal primitive object from the factory
            PrimitiveObject primitiveObject = this.primitiveFactory.GetArchetypePrimitiveObject(graphics.GraphicsDevice, ShapeType.ColoredSphere, effectParameters);

            //remember the primitive is at Transform3D.Zero so we need to say where we want OUR player to start

            //instanciate a box primitive at player position
            SphereCollisionPrimitive collisionPrimitive = new SphereCollisionPrimitive(transform, Scale / 2);

            //make the player object and store as field for use by the 3rd person camera - see camera initialization
            this.enemys[index] = new PlayerCollidablePrimitiveObject(primitiveObject, collisionPrimitive,
                this.managerParameters, AppData.PlayerOneMoveKeys, AppData.PlayerMoveSpeed, this.eventDispatcher);
            this.enemys[index].ActorType = ActorType.CollidableEnemy;
            this.enemys[index].Transform = transform;
            this.enemys[index].EffectParameters.DiffuseColor = color;

            //do we want a texture?
            // playerCollidablePrimitiveObject.EffectParameters.Texture = this.textureDictionary["ml"];

            //add to the object manager
            this.objectManager.Add(this.enemys[index]);
        }


        private void InitializeNonCollidableVolcanoBox(int worldScale)
        {
           // worldScale /= 2;
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

            ////add the top skybox plane
            //clonePlane = Skybox.Clone() as PrimitiveObject;

            //clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            //clonePlane.Transform.Translation = new Vector3(0, ((worldScale) / 2.0f) - 5, 0);
            //clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            //this.objectManager.Add(clonePlane);

            //add the front skybox plane
            clonePlane = Skybox.Clone() as PrimitiveObject;

            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3(0, -5, (worldScale) / 2.0f);
            clonePlane.Transform.ScaleBy(new Vector3(worldScale, 1, worldScale));
            this.objectManager.Add(clonePlane);
            #endregion
        }

        private void InitializeNonCollidableSkyBox(int worldScale)
        {
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
            IController controller = null;
            string viewportDictionaryKey = "full viewport";

            //track camera 1

            transform = new Transform3D(new Vector3(0, 0, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new CurveController(AppData.IntroCurveCameraID + " controller", ControllerType.Track, this.curveDictionary["introCurveCamera"], PlayStatusType.Play);
            InitializeCamera(screenResolution, AppData.IntroCurveCameraID, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0, StatusType.Off);

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
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));

            //publish an event to set the camera

            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));
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
            int verticalBtnSeparation = 50;

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
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add start button
            buttonID = "startbtn";
            buttonText = "Start";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200);
            texture = this.textureDictionary["genericbtn"];
            transform = new Transform2D(position,
                0, new Vector2(1.8f, 0.6f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.LightPink, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkGray, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            this.menuManager.Add(sceneID, uiButtonObject);


            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "audiobtn";
            clone.Text = "Audio";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add controls button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Text = "Controls";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Text = "Exit";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightYellow;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.LightSeaGreen, Color.LightGreen));
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
            clone.Color = Color.LightPink;
            this.menuManager.Add(sceneID, clone);

            //add volume down button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            clone.ID = "volumeDownbtn";
            clone.Text = "Volume Down";
            //change the texture blend color
            clone.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            clone.ID = "volumeMutebtn";
            clone.Text = "Volume Mute";
            //change the texture blend color
            clone.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            clone.ID = "volumeUnMutebtn";
            clone.Text = "Volume Un-mute";
            //change the texture blend color
            clone.Color = Color.LightSalmon;
            this.menuManager.Add(sceneID, clone);

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 4 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.LightYellow;
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
            clone.Transform.Translation += new Vector2(0, 9 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.LightYellow;
            this.menuManager.Add(sceneID, clone);
            #endregion
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
            this.uiManager.Add(myUIMouseObject);
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
            string initalText = "GET READY!";
            Transform2D transform = null;

            this.GameStateText = null;
            Vector2 position = Vector2.Zero;

            Vector2 scale = Vector2.Zero;
            float verticalOffset = 60;

            scale = Vector2.One;

            #region Player 1 Progress Bar

            //to center align horizontaly, it is half window width - lenght of text multiplyed by Scale and fontSize divided by 2
            position = new Vector2((GraphicsDevice.Viewport.Width - (initalText.Length * 60)) /2, verticalOffset);

            transform = new Transform2D(position, 0, scale,
                Vector2.Zero, /*new Vector2(texture.Width/2.0f, texture.Height/2.0f),*/
                new Integer2(1, 1));


            this.GameStateText = new UITextObject("GameText",
                    ActorType.UIDynamicText,
                    StatusType.Drawn | StatusType.Update,
                    transform, Color.Green,
                    SpriteEffects.None,
                    1,
                    initalText,
                    this.fontDictionary["GameText"]);


            //textObject.AttachController(new UIProgressController(AppData.PlayerOneProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));
            
            this.uiManager.Add(this.GameStateText);

            #endregion


        }
        private void CheckGameState(GameTime gameTime)
        {
            
            UpdateGameText(gameTime);
            if(this.gameState == GameState.Level1)
            {
                checkLevel1Win(gameTime);
            }
            if(this.gameState == GameState.Level2Intro)
            {
                InitializeLevel2();
            }
            if(this.gameState == GameState.Level2)
            {
                RaiseLava(gameTime);

            }
        }
        private void UpdateGameText(GameTime gameTime)
        {
            switch(this.gameState)
            {
                case GameState.NotStarted:
                    this.GameStateText.Text = "GET READY!";
                    break;
                case GameState.CountDown:
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - 100) / 2, 60);
                    CountDownLevel1(gameTime);
                    break;
                case GameState.Level1:
                    this.GameStateText.Text = "";
                    break;
                case GameState.Level2Intro:
                    CountDownLevel2(gameTime);
                    break;
                case GameState.Level2:
                    this.GameStateText.Text = "";
                    break;
                case GameState.Won:
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (7 * 62)) / 2, 60);
                    this.GameStateText.Text = "YOU WIN!";
                    break;
                case GameState.Lost:
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (7 * 62)) / 2, 60);
                    this.GameStateText.Text = "Game Over!";
                    break;
            }

        }

        private void CountDownLevel1(GameTime gameTime)
        {

            
            if(!this.timer.IsComplete)
            {
                
                this.timer.set(gameTime, InitialTimerTime);

                if (this.timer.EndTime == 0)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - 160) / 2, 60);
                    this.GameStateText.Text = "GO!";
                }
                else if (this.timer.EndTime > 0)
                {
                    this.timer.finish();
                    this.timer.reset();
                    object[] additionalParameters = { GameState.Level1 };
                    EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));
                }
                else if(this.timer.EndTime >= -5)
                {
                    this.GameStateText.Text = this.timer.Display;
                }
            }

        }

        private void CountDownLevel2(GameTime gameTime)
        {

            if (!this.timer.IsComplete)
            {
                this.timer.set(gameTime, InitialTimerTime + 15);

                if (this.timer.EndTime == 0)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - 160) / 2, 60);
                    this.GameStateText.Text = "GO!";
                }
                else if (this.timer.EndTime > 0)
                {
                    this.timer.finish();
                    this.timer.reset();
                    if(!this.lavaTimerSet)
                    {
                        this.lavaTimer = gameTime.TotalGameTime.Seconds + 180;
                        this.lavaTimerSet = true;
                    }

                    object[] additionalParameters = { GameState.Level2 };
                    EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));
                }
                else if (this.timer.EndTime >= -5)
                {
                    this.GameStateText.Text = this.timer.Display;
                    if(!this.thirdPersonEventSent)
                    {
                        object[] additionalEventParamsB = { AppData.ThirdPersonCameraID };
                        EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
                        this.thirdPersonEventSent = true;
                    }

                }
                else if (this.timer.EndTime >= -10)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (15 * 64)) / 2, 60);
                    this.GameStateText.Text = "ESCAPE THE VOLCANO!";
                }
                else if (this.timer.EndTime >= -15)
                {
                    this.GameStateText.Transform.Translation = new Vector2((GraphicsDevice.Viewport.Width - (15 * 64)) / 2, 60);
                    this.GameStateText.Text = "Level 2";
                }
            }

        }

        private void RaiseLava(GameTime gameTime)
        {
            this.timer.set(gameTime, lavaTimer);
            if (this.timer.EndTime >= -150)
            {
                this.lavaSpeed = 0.002f;
            }
            else if(this.timer.EndTime >= -130)
            {
                this.lavaSpeed = 0.004f;
            }
            else if (this.timer.EndTime >= -100)
            {
                this.lavaSpeed = 0.01f;
            }
            else if (this.timer.EndTime >= -60)
            {
                this.lavaSpeed = 0.015f;
            }
            else if (this.timer.EndTime >= -30)
            {
                this.lavaSpeed = 0.02f;
            }
            else if (this.timer.EndTime >= 0)
            {
                this.lavaSpeed = 0.04f;
            }

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
                if(!enemys[i].InGame)
                {
                    count++;
                }
            }
            if(count == 3)
            {
                this.timer.reset();
                this.InitialTimerTime = gameTime.TotalGameTime.Seconds;

                object[] additionalEventParamsB = { AppData.Level2FixedCameraID };
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));

                object[] additionalParameters = { GameState.Level2Intro };
                EventDispatcher.Publish(new EventData(EventActionType.GameStateChanged, EventCategoryType.GameState, additionalParameters));
            }
        }

        protected void updateTargets()
        {
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
        protected override void LoadContent()
        {
            //moved to Initialize
            //spriteBatch = new SpriteBatch(GraphicsDevice);

//            #region Add Menu & UI
//            InitializeMenu();
//            AddMenuElements();
//            InitializeUI();
//            AddUIElements();
//            #endregion

//#if DEBUG
//            InitializeDebugTextInfo();
//#endif

        }
        protected override void UnloadContent()
        {
            //formally call garbage collection on all ContentDictionary objects to de-allocate resources from RAM
            this.textureDictionary.Dispose();
            this.fontDictionary.Dispose();
            this.videoDictionary.Dispose();

        }

        protected void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.GameStateChanged += EventDispatcher_GameStateChanged;
            eventDispatcher.LavaSpeedChanged += EventDispatcher_LavaSpeedChanged;
        }
        protected void EventDispatcher_GameStateChanged(EventData eventData)
        {
            
            this.gameState = (GameState)Enum.Parse(typeof(GameState), eventData.AdditionalParameters[0].ToString());
        }

        protected void EventDispatcher_LavaSpeedChanged(EventData eventData)
        {
            this.lavaSpeed = (float)eventData.AdditionalParameters[0];
        }

        protected override void Update(GameTime gameTime)
        {
            //exit using new gamepad manager
            if(this.gamePadManager != null && this.gamePadManager.IsPlayerConnected(PlayerIndex.One) && this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                this.Exit();

            CheckGameState(gameTime);
            


#if DEMO
            DoDebugToggleDemo();
            DoCameraCycle();
#endif

            
            base.Update(gameTime);
        }

        private void DoCameraCycle()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraCycle, EventCategoryType.Camera));
            }
        }

        private void DoDebugToggleDemo()
        {
            if(this.keyboardManager.IsFirstKeyPress(Keys.F5))
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

            if (this.keyboardManager.IsFirstKeyPress(Keys.S))
            {
                if(!this.introCameraSkipped)
                {
                    this.InitialTimerTime = this.timer.StartTime + 5;
                    object[] additionalEventParamsB = { AppData.Level1FixedCameraID };
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
                    this.introCameraSkipped = true;
                }

            }

        }

        protected override void Draw(GameTime gameTime)
        {

            //    RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = rasterizerState;


            GraphicsDevice.Clear(GoogleGreenColor);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        #endregion
    }
}
