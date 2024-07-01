using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using Assets.Network.Packet;
using ServerCore;

namespace Server
{

    class Packet
	{
		public ushort size;
		public ushort packetId;
	}

	class PlayerInfoReq : Packet
	{
		public long playerId;
	}

	class PlayerInfoOk : Packet
	{
		public int hp;
		public int attack;
	}

	class PlayerPos : Packet
	{
		public int x;
	}

	public enum PacketID
	{
		PlayerInfoReq = 1,
		PlayerInfoOk = 2,
		playerPos = 3,
		RpcPacket = 4,
		MakeStone = 5

	}

	class ClientSession : PacketSession
	{

		public GameRoom Room { get; set; }
		public bool isMaster = false;

        string readTest;
		

        public override void OnConnected(EndPoint endPoint)
		{
			Program.room.Enter(this);
			Console.WriteLine($"OnConnected : {endPoint}");
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			int pos = 0;

			ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset); 
			pos += sizeof(ushort);
			ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
			pos += sizeof(ushort);

            // TODO
            switch ((PacketID)id)
			{
                case PacketID.RpcPacket:
					{

						RPCPacket myPacket = new RPCPacket();

						myPacket.Read(buffer);

						SessionManager.Instance.BroadCast(myPacket.BroadCastWrite());

                    }
                    break;
					case PacketID.MakeStone:
					{
						MakeStone stone = new MakeStone();

						stone.Read(buffer);

                        foreach (var player in Room.playerList)
                        {
							if (player != this)
								player.Send(buffer);
                        }
                        foreach (var crowd in Room.crowdList)
                        {
							crowd.Send(buffer);
                        }
                        Send(buffer);

						Room.NextTurn();
                    }
                    break;
				default:
					break;
			}

			Console.WriteLine($"RecvPacketId: {id}, Size {size}");
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnDisconnected : {endPoint}");
			Room.Leave(this);

			SessionManager.Instance.Remove(this);
		}

		public override void OnSend(int numOfBytes)
		{
			Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
