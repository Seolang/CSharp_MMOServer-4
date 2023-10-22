namespace MultiThreadProgramming
{
    /*
     * Context Switching의 개념
     * 쓰레드 양보 이후 원래 쓰레드의 실행이 돌아왔을 때, 쓰레드는 자신이 이전에 한 일을 기억하지 못한다
     * => 쓰레드가 전환될 때, 이전 작업 기록을 같이 캐시에 복원하는 작업이 필요하다.
     * 위의 작업은 운영체제가 시행하지만 그 과정에서 많은 자원이 소모될 수 있다
     * 즉, Context Switch가 Spin Lock보다 반드시 좋다고 보장할 수 없다는 것
     */

    class ContextSwitch
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            while (true)
            {
                int expected = 0;
                int desired = 1;
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (original == 0)
                    break;

                // Context Switching (쉬다 올게~)
                // Thread.Sleep(1); // 무조건 휴식 => 무조건 1ms 정도 쉬고 싶어요
                // Thread.Sleep(0); // 조건부 양보 => 나보다 우선순위가 낮은 애들한테는 양보 불가 => 우선순위가 나보다 같거나 높은 쓰레드가 없으면 다시 본인한테
                Thread.Yield(); // 관대한 양보 => 관대하게 양보할테니 지금 실행이 가능한 쓰레드가 있으면 실행하세요 => 실행 가능한 애가 없으면 남은 시간 소진
            }
        }

        public void Release()
        {
            _locked = 0;
        }

    }

    class b05_ContextSwtich
    {
        static int _num = 0;
        static ContextSwitch _lock = new ContextSwitch();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
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
