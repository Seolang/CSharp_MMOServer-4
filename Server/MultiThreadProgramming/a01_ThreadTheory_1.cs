namespace MultiThreadProgramming
{
    /*
     * Thread와 Task에 대한 개념을 배워봄
     */


    class a01_ThreadTheory_1
    {
        static void MainThread(Object state)
        {
            for(int i=0; i<5; i++)
            {
                Console.WriteLine("Hello Thread");
            }
        }

        // 실행 시 static void로 변경 (main함수 중복 예방)
        void Main(string[] args) 
        {
            /*
             * C#의 쓰레드는 기본적으로 foreground에서 실행 (메인함수와 별개로 실행)
             * 따라서 IsBackground를 true로 지정해주면 background로 실행되어 호출 함수에 종속적으로 실행됨
             */
            //Thread t = new Thread(MainThread);
            //t.Name = "TestThread";
            //t.IsBackground = true; 
            //t.Start();

            //Console.WriteLine("Waiting for thread");

            //t.Join(); // thread가 종료될때 까지 기다린다
            //Console.WriteLine("Hello World");


            // 쓰레드풀 최소, 최대 쓰레드 설정
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);


            /* 
             * i<5일 경우 최대 쓰레드까지 전부 점유하게 되어 이후의 쓰레드가 실행 안됨
             * 결국 쓰레드를 오래쓰는 프로그램이 최대치만큼 쓰면 전체 시스템이 먹통이 될 수 있음
             * 이를 해결하기 위해 Task 사용
             */
            //for (int i = 0; i < 4; i++)
            //{
            //    ThreadPool.QueueUserWorkItem((obj) => { while (true) { } });
            //}

            /*
             * Task는 쓰레드풀에서 쓸수도 있고, 새로 쓰레드를 정의해서 쓸수도 있음
             * TaskCreationOptions를 사용해 오래 걸리는 작업임을 명시해주면 쓰레드풀이 부족할 때 새 쓰레드를 만들어 쓸 수도 있음
             * 따라서 C#에서는 직접적으로 쓰레드를 관리할 필요가 없음
             */
            for (int i=0; i<5 ; i++)
            {
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                t.Start();
            }

            ThreadPool.QueueUserWorkItem(MainThread); // C#의 쓰레드풀에서 쓰레드를 빌려와 사용, 함수 내용을 실행 후 자동으로 쓰레드를 반환함

            while (true)
            {

            }
        }
    }
}