using Assets.Network.Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class GameRoom
    {
        public const int NONEMASTER = -1;
        public const int MAX_GAME_PLAYER = 2;
        public bool isStart = false;
        public int masterSessionId = -1;
        public List<ClientSession> playerList = new List<ClientSession>();
        public List<ClientSession> crowdList = new List<ClientSession>();

        object listLock = new object();
        InitPacket initPac;
        private int currentPlayer = 0;
        public Session NextPlayer
        {
            get
            {
                currentPlayer = (currentPlayer + 1) % MAX_GAME_PLAYER;

                Console.WriteLine(currentPlayer);

                return playerList[currentPlayer];
            }
        }
        public void NextTurn()
        {
            Session next = NextPlayer;
            NextTurnPacket nextPacket = new NextTurnPacket();
            nextPacket.PacketInit(next.SessionId);

            foreach ( var player in playerList)
            {
                player.Send(nextPacket.Write());
            }

        }
        public void Enter(ClientSession session)
        {
            lock(listLock)
            {
                bool playerFlag = false;

                if (playerList.Count == 0)
                    masterSessionId = session.SessionId;

                if(isStart == false)
                {
                    playerList.Add(session);
                    playerFlag = true;
                }
                else
                {
                    crowdList.Add(session);
                }

                if (playerList.Count == MAX_GAME_PLAYER)
                    isStart = true;

                session.Room = this;
                initPac = new InitPacket();
                PlayerStruct playerData = new PlayerStruct(session.SessionId, playerFlag, masterSessionId != session.SessionId );

                initPac.PacketInit(playerData);
                session.Send(initPac.Write());
            }
        }
        public void Leave(ClientSession session)
        {
            lock (listLock)
            {
                if (crowdList.Contains(session))
                {
                    crowdList.Remove(session);
                    return;
                }


                if (playerList.Contains(session))
                {
                    playerList.Remove(session);

                    if (session.SessionId == this.masterSessionId)
                    {
                        if (playerList.Count != 0)
                            this.masterSessionId = playerList[0].SessionId;
                        else
                            this.masterSessionId = NONEMASTER;
                    }
                }
            }

        }
    }
}
