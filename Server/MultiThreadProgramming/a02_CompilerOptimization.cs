namespace MultiThreadProgramming
{
    /*
     * 컴파일러가 최적화를 하면서 어떤 사이드 이펙트를 발생시키는지 알아봄
     */

    class a02_CompilerOptimization
    {
        // static bool _stop = false;
        volatile static bool _stop = false; // 컴파일러에게 최적화 하지 말라고 volatile 키워드로 알림 (다만 사용하지 않는 것을 권장함)

        /*
         * 프로그램을 빌드해 실행하면 컴파일러 최적화 과정에서 무한루프에서 빠져나오지 못하는 상황이 발생함
         * (컴파일러 입장에서 ThreadMain 함수만 보았을 때는 _stop 변수가 변하지 않기 때문에 while문 조건을 true로 바꿔버림)
         */
        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");

            while(!_stop)
            {
                // 누군가가 Stop 신호를 해주기를 기다린다
            }

            Console.WriteLine("쓰레드 종료");
        }


        void Main(string[] args)
        {
            Task t = new Task(ThreadMain); 
            t.Start();

            Thread.Sleep(1000); // Main 함수의 쓰레드를 1초 동안 슬립

            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");

            t.Wait(); // Thread.Join()과 같은 기능
            Console.WriteLine("종료 성공");
        }
    }
}
