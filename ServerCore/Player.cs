using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public struct PlayerStruct
    {
        public PlayerStruct(int sessionId , bool isPlayer , bool isWhite)
        {
            this.sessionId = sessionId ;
            this.isPlayer = isPlayer ;
            this.isWhite = isWhite ;
        }
        public int sessionId;
        public bool isPlayer;
        public bool isWhite;
    }

}
