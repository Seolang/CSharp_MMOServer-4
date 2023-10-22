namespace MultiThreadProgramming
{
    /*
     * 쓰레드 락을 통한 원자성 유지 방법에 대해 알아본다
     * 
     * Monitor : 쓰레드 잠금을 수동으로 조작 가능 -> DeadLock의 가능성
     * 
     */

    class b02_Lock
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                //// 상호 배제 (Mutual Exclusive)
                // Monitor.Enter(_obj); // 문을 잠그는 행위
                // number++;
                // //return           // 만약 return되어 Exit을 해주지 않으면 Thread_2가 영원히 기다리게됨 => Deadlock 상태
                // Monitor.Exit(_obj); // Enter -> Exit 사이의 행위는 다른 Enter -> Exit 안의 행위와 동시에 실행되지 않음

                lock (_obj) // 위 동작을 구현하면서 Deadlock을 예방
                {
                    number++;
                }
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                //Monitor.Enter(_obj);

                //number--;

                //Monitor.Exit(_obj);

                lock (_obj)
                {
                    number--;
                }
            }
        }

        void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number); // 0 이 나와야 한다
        }
    }
}
