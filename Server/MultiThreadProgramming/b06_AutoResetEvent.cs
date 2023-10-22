namespace MultiThreadProgramming
{
    /*
     * AutoResetEvent : 고속도로 톨게이트의 차단선처럼 자동으로 락을 관리
     * (ManualResetEvent : 수동으로 락을 조절하는 이벤트 처리 방식)
     * 
     * AutoResetEvent의 단점: 느리다!, 커널 단에서 스핀 락을 하기 때문에 부담이 큼
     * 
     */

    class AutoLock
    {
        AutoResetEvent _available = new AutoResetEvent(true);
        // ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne(); // 입장 시도
            // _available.Reset(); // bool = false, AutoResetEvent에서는 WaitOne에 포함되어있음, MenualResetEvent에서는 수동으로 문을 닫는 역할
            // ManualResetEvent는 Wait과 Lock이 두 단계로 이루어지기 때문에 의도치않게 쓰레드가 동작할 수 있다.
        }

        public void Release()
        {
            _available.Set(); // bool = true
        }

    }

    class b06_AutoResetEvent
    {
        static int _num = 0;
        static AutoLock _lock = new AutoLock();

        static void Thread_1()
        {
            for (int i = 0; i < 1000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

        void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}
