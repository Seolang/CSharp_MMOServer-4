namespace MultiThreadProgramming
{
    /*
     * 하드웨어 최적화와 그에 따른 Side Effect에 대한 영향을 알아봄
     * 
     * 하드웨어는 최적화를 위해 임의로 코드의 실행 순서를 변경할 수도 있음
     * 
     * 메모리 베리어
     * A) 코드 재배치 억제 -> 하드웨어 최적화중 임의의 코드 재배치를 막음
     * B) 가시성 -> 캐시와 메모리간 내용을 동기화 (Commit, Pull)
     * 
     * 메모리 베리어의 종류
     * 1) Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘다 막음
     * 2) Store Memory Barrier (ASM SFENCE) : Store만 막음
     * 3) Load Memory Barrier (ASM LFENCE) : Load만 막음
     * 
     */

    class a04_MemoryBarrier
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread_1()
        {
            y = 1; // Store y

            Thread.MemoryBarrier(); // 이 구문 위아래로 실행 순서를 하드웨어가 임의로 변경할 수 없게 억제

            r1 = x; // Load x
        }

        static void Thread_2()
        {
            x = 1; // Store y

            Thread.MemoryBarrier();

            r2 = y; // Load x
        }

        int _answer;
        bool _complete;

        void A()
        {
            _answer = 123;
            Thread.MemoryBarrier(); // Barrier 1
            _complete = true;
            Thread.MemoryBarrier(); // Barrier 2
        }

        void B()
        {
            Thread.MemoryBarrier(); // Barrier 3
            if ( _complete )
            {
                Thread.MemoryBarrier(); // Barrier 4
                Console.WriteLine(_answer);
            }
        }

        void Main(string[] args)
        {
            int count = 0;
            while(true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                if (r1 == 0 && r2 == 0) 
                    break;

                /* 
                 * 어째서 r1과 r2가 0이 될 수가 있는거지?
                 * => 하드웨어의 멀티쓰레드 최적화로 인해 발생한 문제 
                 * (y -> r1, x -> r2 순서로 동작해야할 것이 r1 -> y, r2 -> x 순서로 코드를 재배치 해버림)
                 */
            }

            Console.WriteLine($"{count}번 만에 빠져나옴");

        }
    }
}
