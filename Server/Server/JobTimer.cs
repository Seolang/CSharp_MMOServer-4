using ServerCore;

namespace Server
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int execTick; // 실행 시간
        public Action action;

        public int CompareTo(JobTimerElement other)
        {
            return other.execTick - execTick;
        }
    }

    // [20ms][20ms][20ms][20ms][20ms][][][][][][][][][][][][][][]
    // 
    class JobTimer
    {
        PriorityQueue<JobTimerElement> _pq = new ();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer ();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElement job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while(true)
            {
                // 현재 틱
                int now = System.Environment.TickCount;

                JobTimerElement job;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    // 작업을 시작할 때까지 아직 시간이 안됐으면 패스
                    if (job.execTick > now)
                        break;

                    // 작업할 시간이면 job은 peek에서 가져왔으니 그냥 빼기만 함
                    _pq.Pop();
                }

                // 작업 실행
                job.action.Invoke();
            }
        }
    }
}
