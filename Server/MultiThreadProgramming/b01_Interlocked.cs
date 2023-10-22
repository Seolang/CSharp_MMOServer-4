namespace MultiThreadProgramming
{
    class b01_Interlocked
    {
        /*
         * 멀티쓰레드에서 발생하는 경합 조건에 대해 알아본다
         * 
         * 쓰레드 경합 : 서로 다른 쓰레드들이 같은 업무를 중복해서 실행함
         * 
         */

        static int number = 0;

        static void Thread_1()
        {
            for(int i=0; i < 100000; i++) 
            {
                // number++; // 어셈블리는 이것을 temp = number -> temp += 1 -> number = temp 로 3단계로 해석함

                Interlocked.Increment(ref number); // 덧셈 과정을 atomic하게 실행 -> 원자성을 유지시켜 경합을 방지
                //Interlocked는 원자성은 보장해주지만 캐싱의 의미를 잃어버리게 됨
                 
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                Interlocked.Decrement(ref number);
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
