using Assets.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        public List<ClientSession> playerList = new List<ClientSession>();
        object listLock = new object();
        InitPacket initPac = new InitPacket();

        public void Enter(ClientSession session)
        {
            

            lock(listLock)
            {
                if (playerList.Count == 0)
                    session.isMaster = true;

                Console.WriteLine(session.isMaster);
                playerList.Add(session);
                session.Room = this;
                session.Send(initPac.Write(session.isMaster));
            }

        }

        public void Leave(ClientSession session)
        {
            lock (listLock)
            {

                playerList.Remove(session);
                if (session.isMaster && playerList.Count != 0)
                {
                    playerList[0].isMaster = true;
                    playerList[0].Send(initPac.Write(playerList[0].isMaster));
                }

            }
        }


    }
}
