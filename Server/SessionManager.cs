using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class SessionManager
    {
        static SessionManager instance = new SessionManager();
        public static SessionManager Instance { get { return instance; } }

        int sessionId = 0;

        Dictionary<int ,ClientSession> playerSession = new Dictionary<int, ClientSession> ();

        object playerLock = new object ();

        public ClientSession Generate()
        {
            lock (playerLock)
            {
                int id = ++this.sessionId;

                ClientSession session = new ClientSession ();
                session.SessionId = sessionId;
                playerSession.Add (id, session);

                Console.WriteLine($"Connected : {id}");

                return session;
            }

        }

        public ClientSession Find(int id)
        {

            lock (playerLock)
            {
                ClientSession session = null;
                playerSession.TryGetValue (id, out session);
                return session;
            }

        }

        public void Remove(ClientSession session)
        {
            lock (playerLock) 
            {
                playerSession.Remove(session.SessionId);
            }
        }


    }
}
