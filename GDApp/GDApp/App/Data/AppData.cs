/*
Function: 		Stores common hard-coded variable values used within the game e.g. key mappings, mouse sensitivity
Author: 		NMCG
Version:		1.0
Date Updated:	5/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace GDLibrary
{
    public sealed class LerpSpeed
    {
        private static readonly float SpeedMultiplier = 2;
        public static readonly float VerySlow = 0.05f;
        public static readonly float Slow = SpeedMultiplier * VerySlow;
        public static readonly float Medium = SpeedMultiplier * Slow;
        public static readonly float Fast = SpeedMultiplier * Medium;
        public static readonly float VeryFast = SpeedMultiplier * Fast;
    }
    public sealed class AppData
    {
        #region Common
        public static int IndexMoveForward = 0;
        public static int IndexMoveBackward = 1;
        public static int IndexRotateLeft = 2;
        public static int IndexRotateRight = 3;
        public static int IndexMoveJump = 4;
        public static int IndexMoveCrouch = 5;
        public static int IndexStrafeLeft = 6;
        public static int IndexStrafeRight = 7;
        #endregion

        #region Camera
        public static readonly int CurveEvaluationPrecision = 4;

        public static readonly float CameraRotationSpeed = 0.0125f;
        public static readonly float CameraMoveSpeed = 0.025f;
        public static readonly float CameraStrafeSpeed = 0.6f * CameraMoveSpeed;
    
        //JigLib related collidable camera properties
        public static readonly float CollidableCameraJumpHeight = 12;
        public static readonly float CollidableCameraMoveSpeed = 0.6f;
        public static readonly float CollidableCameraStrafeSpeed = 0.6f * CollidableCameraMoveSpeed;
        public static readonly float CollidableCameraCapsuleRadius = 2;
        public static readonly float CollidableCameraViewHeight = 8; //how tall is the first person player?
        public static readonly float CollidableCameraMass = 10;

        public static readonly Keys[] CameraMoveKeys = { Keys.W, Keys.S, Keys.A, Keys.D, 
                                         Keys.Space, Keys.C, Keys.LeftShift, Keys.RightShift};
        public static readonly Keys[] CameraMoveKeys_Alt1 = { Keys.T, Keys.G, Keys.F, Keys.H };

        public static readonly float CameraThirdPersonScrollSpeedDistanceMultiplier = 0.00125f;
        public static readonly float CameraThirdPersonScrollSpeedElevationMultiplier = 0.01f;
        public static readonly float CameraThirdPersonDistance = 12;
        public static readonly float CameraThirdPersonElevationAngleInDegrees = 150;

        public static readonly float SecurityCameraRotationSpeedSlow = 0.5f;
        public static readonly float SecurityCameraRotationSpeedMedium = 2 * SecurityCameraRotationSpeedSlow;
        public static readonly float SecurityCameraRotationSpeedFast = 2 * SecurityCameraRotationSpeedMedium;

        //yaw means to rotate around the Y-axis - this will confuse you at first since we're using UnitX but you need to look at Transform3D::RotateBy()
        public static readonly Vector3 SecurityCameraRotationAxisYaw = Vector3.UnitX;
        public static readonly Vector3 SecurityCameraRotationAxisPitch = Vector3.UnitY;
        public static readonly Vector3 SecurityCameraRotationAxisRoll = Vector3.UnitZ;

        #endregion

        #region Player
        public static readonly string PlayerOneID = "player1";
        public static readonly string PlayerTwoID = "player2";

        public static readonly Keys[] PlayerOneMoveKeys = { Keys.U, Keys.J, Keys.H, Keys.K, Keys.Y, Keys.I, Keys.N, Keys.M};
        public static readonly Keys[] PlayerTwoMoveKeys = { Keys.NumPad8, Keys.NumPad5, Keys.NumPad4, Keys.NumPad6, Keys.NumPad7, Keys.NumPad9, Keys.NumPad2, Keys.NumPad3 };
        public static readonly float PlayerMoveSpeed = 0.02f;
        public static readonly float PlayerStrafeSpeed = 0.7f * PlayerMoveSpeed;
        public static readonly float PlayerRotationSpeed = 0.08f;
        public static readonly float PlayerRadius = 1.5f;
        public static readonly float PlayerHeight = 4.5f;
        public static readonly float PlayerMass = 25;
        public static readonly float PlayerJumpHeight = 25;

        public static readonly float SquirrelPlayerMoveSpeed = 0.4f;
        public static readonly float SquirrelPlayerRotationSpeed = 0.2f;
        public static readonly Keys[] SquirrelPlayerMoveKeys = { Keys.NumPad8, Keys.NumPad5, Keys.NumPad4, Keys.NumPad6, Keys.NumPad7, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3 };

        public static readonly float DudeMoveSpeed = 0.15f;
        public static readonly float DudeRotationSpeed = 0.1f;
        public static readonly float DudeJumpHeight = 25;

        #endregion

        #region Menu
        public static readonly Keys KeyPauseShowMenu = Keys.Escape;
        public static readonly Keys KeyToggleCameraLayout = Keys.F1;
        #endregion

        #region Mouse
        //defines how much the mouse has to move in pixels before a movement is registered - see MouseManager::HasMoved()
        public static readonly float MouseSensitivity = 1;

        //always ensure that we start picking OUTSIDE the collidable first person camera radius - otherwise we will always pick ourself!
        public static readonly float PickStartDistance = CollidableCameraCapsuleRadius * 2f;
        public static readonly float PickEndDistance = 1000; //can be related to camera far clip plane radius but should be limited to typical level max diameter
        public static readonly bool EnablePickAndPlace = true;

        #endregion

        #region UI
        public static readonly string PlayerOneProgressID = PlayerOneID + " progress";
        public static readonly string PlayerTwoProgressID = PlayerTwoID + " progress";
        public static readonly string PlayerOneProgressControllerID = PlayerOneProgressID + " ctrllr";
        public static readonly string PlayerTwoProgressControllerID = PlayerTwoProgressID + " ctrllr";
        #endregion

        #region JigLibX
        public static readonly Vector3 Gravity = -10 * Vector3.UnitY;
        public static readonly Vector3 BigGravity = 5 * Gravity;

        #endregion


        #region Video
        public static readonly string VideoIDMainHall;
        public static readonly string ControllerIDSuffix = " controller";
        #endregion

        #region Primitive IDs
        public static readonly string TexturedQuadID = "textured quad";
        public static readonly string TexturedBillboardQuadID = "textured billboard quad";
        public static readonly string OriginHelperID = "XYZ origin helper";
        public static readonly string TexturedCubeID = "textured cube";
        #endregion

        #region Effect IDs
        //used by primitives based on VertexPositionColor vertex type
        public static readonly string UnLitColoredPrimitivesEffectID = "unlit colored";

        //used by primitives based on VertexPositionColorTexture vertex type
        public static readonly string UnLitTexturedPrimitivesEffectID = "unlit textured";

        //used by primitives based on VertexBillboard vertex type
        public static readonly string UnlitBillboardsEffectID = "unlit textured billboards";

        //used by primitives based on VertexPositionNormalTexture vertex type
        public static readonly string LitTexturedPrimitivesEffectID = "lit textured ";
        #endregion

        #region Camera IDs
        public static readonly string FlightCameraID = "flight camera 1";
        public static readonly string ThirdPersonCameraID = "third person camera 1";
        #endregion

        #region Zone IDs
        public static readonly string SwitchToThirdPersonZoneID = "third person camera zone";
        #endregion

    }
}
