namespace MultiThreadProgramming
{
    /*
     * 스핀락에 대해 알아본다
     * Spinlock : 쓰레드가 권한을 얻을 때 루프를 돌면서 기다린다
     */

    class MySpinLock
    {
        /*
         * 1. while을 사용한 대기방식의 Spinlock : "문을 잠금 -> 잠금을 푼다" 의 동작이 원자적으로 이루어지지 않아 _num이 비정상적으로 동작함 
         * (e_interlock과 같은 문제)
         */

        //volatile bool _locked = false;

        //public void Acquire()
        //{
        //    while(_locked)
        //    {
        //        // 잠김이 풀리기를 기다린다.
        //    }

        //    // 잠금이 풀렸으므로 작업권을 가져간다
        //    _locked = true;
        //}

        //public void Release() 
        //{
        //    _locked = false;
        //}

        /*
         * 2. Interlock을 사용한 방식 : original은 다른 쓰레드와 공유하지 않는 값이다. 따라서 _locked를 1로 만들고 _locked의 이전값을 비교하는
         * 동안 다른 쓰레드가 값을 변경하지 못한다.
         */
        volatile int _locked = 0;

        public void Acquire()
        {
            while(true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;

                // CAS Compare-And_Snap

                // _locked 가 0이면 1로 바꾸어주고 원본값을 리턴하는 함수
                int expected = 0;
                int desired = 1;
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (original == 0)
                    break;
            }
        }

        public void Release()
        {
            _locked = 0;
        }

    }

    class b04_SpinLock
    {
        static int _num = 0;
        static MySpinLock _lock = new MySpinLock();

        static void Thread_1()
        {
            for(int i=0; i<100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for(int i=0; i<100000; i++)
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
