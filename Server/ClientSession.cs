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

		public int SessionId { get; set; }
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
			pos += 2;
			ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
			pos += 2;

			// TODO
			switch ((PacketID)id)
			{
				case PacketID.PlayerInfoReq:
					{
						long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + pos);
						pos += 8;
					}
					break;
				case PacketID.PlayerInfoOk:
					{
						int hp = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
						pos += 4;
						int attack = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
						pos += 4;
                        Console.WriteLine($"hp {hp} , attack {attack}");
                    }
					//Handle_PlayerInfoOk();
					break;
				case PacketID.playerPos:
					{
                        
                        //int x = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
                        //pos += 4;
                        //Console.WriteLine($"Pos x : {x}");

						ushort stringSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
                        Console.WriteLine($"stringSize : {stringSize}");
                        pos += 2;
                        readTest = Encoding.Unicode.GetString(buffer.Array, pos, stringSize);
                        Console.WriteLine($"pos : {pos}, stringsize : {stringSize}");

                        Console.WriteLine($"receivString : {readTest}");
                        Console.WriteLine($"readTestLength : {readTest.Length}");
                        pos += stringSize;

                        int maxhp = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
						Console.WriteLine($"maxhp = {maxhp}");
                    }
					break;
                case PacketID.RpcPacket:
					{

						
						RPCPacket myPacket = new RPCPacket();

						myPacket.Read(buffer);


                        //ushort stringSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
                        //Console.WriteLine($"stringSize : {stringSize}");
                        //pos += 2;

                        //readTest = Encoding.Unicode.GetString(buffer.Array, pos, stringSize);
                        //Console.WriteLine($"receivString : {readTest}");
                        //pos += stringSize;

                    }
                    break;
					case PacketID.MakeStone:
					{


						MakeSton myPacket = new MakeSton();
						myPacket.Read(buffer);

                        foreach (var player in Room.playerList)
                        {
							if (player != this)
								player.Send(buffer);
                        }
						Send(buffer);
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
