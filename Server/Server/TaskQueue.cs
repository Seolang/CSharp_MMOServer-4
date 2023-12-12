using Server.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /*
     * 람다식이 아닌, 클래스를 지정해 JobQueue를 생성하는 방식 예시
     */

    interface ITask
    {
        void Execute();
    }

    class BroadcastTask : ITask
    {
        GameRoom _room;
        ClientSession _session;
        string _chat;

        BroadcastTask(GameRoom room, ClientSession session, string chat)
        {
            _room = room;
            _session = session;
            _chat = chat;
        }

        public void Execute() 
        { 
            // 할 작업
        }
    }

    class TaskQueue
    {
        Queue<ITask> _queue = new();
    }
}
