namespace MultiThreadProgramming
{
    /*
     * DeadLock이 발생할 수 있는 상황에 대해 알아본다
     */

    class SessionManager
    {
        static object _lock = new object();

        public static void TestSession()
        {
            lock (_lock)
            {

            }
        }

        public static void Test()
        {
            lock (_lock)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        public static void TestUser()
        {
            lock (_lock)
            {

            }
        }

        public static void Test()
        {
            lock (_lock)
            {
                SessionManager.TestSession();
            }
        }
    }

    class b03_DeadLock
    {

        static int number = 0;
        static object _lock = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 100; i++)
            {
                SessionManager.Test();  
                // Thread_1의 lock과 Thread_2의 lock이 맞물리면서 더이상 진행되지 않는 DeadLock이 발생해버림
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100; i++)
            {
                UserManager.Test();
            }
        }

        void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();

            Thread.Sleep(1000); // 쓰레드간 실행 시간이 어긋나기만 해도 데드락이 해결될"수도" 있음

            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
