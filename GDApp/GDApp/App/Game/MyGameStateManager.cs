/*
Function: 		Use this class to say exactly how your game listens for events and responds with changes to the game.
Author: 		NMCG
Version:		1.0
Date Updated:	17/11/17
Bugs:			
Fixes:			None
*/

using GDLibrary;
using Microsoft.Xna.Framework;

namespace GDApp
{
    public class MyGameStateManager : GameStateManager
    {
        public MyGameStateManager(Game game, EventDispatcher eventDispatcher, StatusType statusType) 
            : base(game, eventDispatcher, statusType)
        {

        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //to do...

            base.ApplyUpdate(gameTime);
        }
    }
}
