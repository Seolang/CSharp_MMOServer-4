namespace MultiThreadProgramming
{
    /*
     * Mutex는 커널 단위에서 거는 스핀락으로 생각할 수 있다
     * 
     * Mutex는 AutoResetEvent와 달리 bool, changed_count, threadId등 여러 값을 가질 수 있다 => 무거워서 잘 사용하지 않음
     */

    class b07_Mutex
    {
        static int _num = 0;
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for (int i = 0; i < 1000; i++)
            {
                // AutoResetEvent의 Wait, Release와 비슷함
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
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
