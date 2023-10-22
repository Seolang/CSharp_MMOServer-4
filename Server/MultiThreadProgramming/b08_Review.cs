namespace MultiThreadProgramming
{
    /*
     * Lock을 구현하는 3가지 방법 (어느 것이 좋다고 할 수 없음)
     * 1. 근성 : 계속 뺑뺑이 돌며 기다림
     * 2. 양보 : 조건부에 따라 실행가능한 쓰레드에게 실행 권한을 양도
     * 3. 갑질 : 이벤트를 발생시키도록 함
     */

    class b08_Review
    {
        // 상호 배제

        static object _lock = new object(); // Monitor 클래스 이용
        static SpinLock _lock2 = new SpinLock(); // 이미 C#에 SpinLock이 구현되어 있다!
        // static Mutex _lock3 = new Mutex(); 거의 안씀
        static ReaderWriterLock _lock4 = new ReaderWriterLock();

        void Main()
        {
            // Monitor Lock
            lock (_lock)
            {

            }

            // SpinLock 클래스 -> 근성 방식을 일정 시도 후 양보
            bool lockTaken = false;
            try // Exception 발생시 정상적으로 lock 처리를 위해 사용
            {
                _lock2.Enter(ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    _lock2.Exit();
                }
            }
        }

    }
}
