namespace MultiThreadProgramming
{
    /*
     * 멀티쓰레드에서 쓰레드가 몰린 어떤 작업에 락을 건다는 것? => 모든 쓰레드들이 한줄로 서서 일을 하는 상황 => 단일 쓰레드나 마찬가지 (오히려 더 안좋을 수 있음)
     * 
     * 쓰레드는 로컬 스토리지로 스택을, 공용 스토리지로 힙(new, static)을 사용 했었음
     * TLS(ThreadLocalStorage)는 힙에서 필요한 만큼 덩어리를 로컬로 가져와 사용한다고 생각할 수 있다 (쓰레드 개개인 만의 영역으로 지정)
     * 
     * 예를들어 할 일이 100개인 JobQueue가 있고 이를 5개의 쓰레드가 매번 락을 통해 접근한다면 접근 횟수는 총 100번 이지만,
     * 한번 접근할때 일 20개를 TLS에 넣어서 가져오면 5번만 접근 후 각 쓰레드가 할 일을 각 20번씩 병렬 처리할 수 있다.
     * 
     */

    class b11_ThreadLocalStorage
    {
        // 쓰레드마다 고유한 string을 할당
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My Name Is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated; // 기존에 만들어진 쓰레드는 true

            if (repeat)
            {
                Console.WriteLine(ThreadName.Value + " (repeat)");
            }
            else
            {
                Console.WriteLine(ThreadName.Value);
            }
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI); // Task 여러개를 동시에 생성해준다

            ThreadName.Dispose();
        }
    }
}
