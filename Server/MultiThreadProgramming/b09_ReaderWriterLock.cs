namespace MultiThreadProgramming
{
    /*
     * ReaderWriterLock : 어떤 객체를 쓰거나 읽을 때 상호배타적으로 접근하게 하는 기법
     */

    class Reward
    {

    }

    class b09_ReaderWriterLock
    {

        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        static Reward GetRewardById(int id)
        {
            _lock.EnterReadLock();

            _lock.ExitReadLock();

            return null;
        }

        static void AddReward(Reward reward)
        {
            _lock.EnterWriteLock();

            _lock.ExitWriteLock();

        }

        void Main(string[] args)
        {

        }

    }
}
