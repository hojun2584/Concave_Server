using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Network.Packet
{
    public enum PacketID
    {
        InitPacket = 1,
        PlayerInfoOk = 2,
        playerPos = 3,
        RpcPacket = 4,
        MakeStone = 5,
        NextTurn = 10,
    }

    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract void Read(ArraySegment<byte> buffer);
        public abstract ArraySegment<byte> Write();

        public void WriteString(ref ArraySegment<byte> buffer , ref ushort size ,string value)
        {

            ushort stringLength = (ushort)Encoding.Unicode.GetByteCount(value);
            BitConverter.TryWriteBytes(new Span<byte>(buffer.Array, buffer.Offset + size, buffer.Count - size), (ushort)stringLength);
            size += sizeof(ushort);
            
            byte[] sendString = Encoding.Unicode.GetBytes(value);
            for (int i = buffer.Offset; i < stringLength; i++)
            {
                buffer[i + size] = sendString[i];
            }
            size += stringLength;
        }

        public void WriteVector3(ref ArraySegment<byte> buffer , ref ushort size , Vector3 pos)
        {
            BitConverter.TryWriteBytes(new Span<byte>(buffer.Array, buffer.Offset, buffer.Count), pos.X);
            size += sizeof(float);
            BitConverter.TryWriteBytes(new Span<byte>(buffer.Array, buffer.Offset, buffer.Count), pos.Y);
            size += sizeof(float);
            BitConverter.TryWriteBytes(new Span<byte>(buffer.Array, buffer.Offset, buffer.Count), pos.Z);
            size += sizeof(float);
        }

        public Vector3 ReadVector3(ref ArraySegment<byte> buffer , ref ushort size)
        {
            Vector3 compleVector3 = new Vector3();

            compleVector3.X = BitConverter.ToSingle(buffer.Array, buffer.Offset + size);
            size += sizeof(float);
            compleVector3.Y = BitConverter.ToSingle(buffer.Array, buffer.Offset + size);
            size += sizeof(float);
            compleVector3.Z = BitConverter.ToSingle(buffer.Array, buffer.Offset + size);
            size += sizeof(float);

            return compleVector3;
        }

    }

    public class NextTurnPacket : Packet
    {
        public int sessionId;

        public void PacketInit(int sessionId)
        {
            this.sessionId = sessionId;
        }

        public override void Read(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            count += sizeof(ushort);
            count += sizeof(ushort);

            sessionId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(int);

        }

        public override ArraySegment<byte> Write()
        {
            ushort count = 0; 
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);

            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array , sendBuffer.Offset + count , sendBuffer.Count - count) , (ushort)PacketID.NextTurn);
            count += sizeof(ushort);

            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), sessionId);
            count += sizeof(int);

            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array , sendBuffer.Offset, sendBuffer.Count),count);
            return SendBufferHelper.Close(count);
        }
    }

    public class GameStart : Packet
    {
        public PlayerStruct playerData;

        public void PacketInit(PlayerStruct playerData)
        {
            this.playerData = playerData;
        }

        public void PacketInit(int sessionId, bool isPlayer, bool isWhite)
        {
            playerData = new PlayerStruct();
            playerData.sessionId = sessionId;
            playerData.isWhite = isWhite;
            playerData.isPlayer = isPlayer;
        }

        public override void Read(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            count += sizeof(ushort);
            count += sizeof(ushort);

            playerData.sessionId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(int);

            playerData.isPlayer = BitConverter.ToBoolean(buffer.Array, buffer.Offset + count);
            count += sizeof(bool);

            playerData.isWhite = BitConverter.ToBoolean(buffer.Array, buffer.Offset + count);
            count += sizeof(bool);

        }

        public override ArraySegment<byte> Write()
        {
            ushort count = 0;

            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            // count pos
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), (ushort)PacketID.InitPacket);
            count += sizeof(ushort);

            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.sessionId);
            count += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.isPlayer);
            count += sizeof(bool);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.isWhite);
            count += sizeof(bool);

            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count), count);
            ArraySegment<byte> doneBuffer = SendBufferHelper.Close(count);
            return doneBuffer;
        }
    }


    public class InitPacket : Packet
    {

        public bool isMaster = false;

        public PlayerStruct playerData;

        public void PacketInit(PlayerStruct playerData)
        {
            this.playerData = playerData;
        }

        public void PacketInit(int sessionId, bool isPlayer, bool isWhite)
        {
            playerData = new PlayerStruct();
            playerData.sessionId = sessionId;
            playerData.isWhite = isWhite;
            playerData.isPlayer = isPlayer;
        }

        public override void Read(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            count += sizeof(ushort); // packet Size
            count += sizeof(ushort); // packet Id
            playerData.sessionId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(int);
            playerData.isPlayer = BitConverter.ToBoolean(buffer.Array, buffer.Offset + count);
            count += sizeof(bool);
            playerData.isWhite = BitConverter.ToBoolean(buffer.Array, buffer.Offset + count);
            count += sizeof(bool);

        }


        public override ArraySegment<byte> Write()
        {
            ushort count = 0;
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), (ushort)PacketID.InitPacket);
            count += sizeof(ushort);

            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.sessionId);
            count += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.isPlayer);
            count += sizeof(bool);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), playerData.isWhite);
            count += sizeof(bool);

            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count), count);

            return SendBufferHelper.Close(count); ;
        }
    }

    public class MakeStone : Packet
    {
        public ushort x, y;
        public bool isWhite;

        public override void Read(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            count += sizeof(ushort); // packet Size
            count += sizeof(ushort); // packet Id
            isWhite = BitConverter.ToBoolean(buffer.Array, buffer.Offset + count);
            count += sizeof(bool);
            x = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);
            y = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);
        }

        public override ArraySegment<byte> Write()
        {
            ushort count = 0;
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            //todo 에러 메시지라도 보낼 것


            ArraySegment<byte> doneBuffer = SendBufferHelper.Close(count);

            return doneBuffer;
        }

        public ArraySegment<byte> Write(ushort x, ushort y)
        {
            ushort count = 0;

            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);


            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), x);
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), y);
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), isWhite);
            count += sizeof(bool);

            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), count);

            ArraySegment<byte> doneBuffer = SendBufferHelper.Close(count);
            return doneBuffer;
        }

        public ArraySegment<byte> BroadMakeStone()
        {
            ushort count = 0;
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);//packetid

            

            count += sizeof(ushort);//size
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), count);

            ArraySegment<byte> doneBuffer = SendBufferHelper.Close(count);
            return doneBuffer;
        }

    }


    public class RPCPacket : Packet
    {

        string funcName = null;
        ushort stringLength = 0;


        public override void Read(ArraySegment<byte> buffer)
        {
            
            size += sizeof(ushort);
            size += sizeof(ushort);


            stringLength = BitConverter.ToUInt16(buffer.Array, buffer.Offset + size);
            Console.WriteLine($"stringSize : {stringLength}");
            size += sizeof(ushort);

            funcName = Encoding.Unicode.GetString(buffer.Array, size, stringLength);
            Console.WriteLine($"receivString : {funcName}");


        }

        public override ArraySegment<byte> Write()
        {

            ushort count = 0;
            bool success = true;


            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), (ushort)4);
            count += sizeof(ushort);


            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count), count);
            ArraySegment<byte> endBuffer = SendBufferHelper.Close(count);

            return endBuffer;
        }
        public ArraySegment<byte> BroadCastWrite()
        {

            ushort count = 0;
            bool success = true;

            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);
            count += sizeof(ushort);
            BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset + count, sendBuffer.Count - count), (ushort)4);
            count += sizeof(ushort);


            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(new Span<byte>(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count), count);
            ArraySegment<byte> endBuffer = SendBufferHelper.Close(count);

            return endBuffer;
        }

    }


}
