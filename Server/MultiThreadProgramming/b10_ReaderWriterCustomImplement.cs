using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadProgramming
{
    // 재귀적 락을 허용할지 : 락 권한을 얻은 쓰레드가 이어서 다른 락 획득 권한을 가질 것인가
    // WriteLock -> WriteLock OK, WriteLock -> ReadLock OK, ReadLock -> WriteLock X
    class b10_Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000; // 맨 앞의 비트는 사용하지 않음 (음수때문)
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
            // 재귀적 락 사용시 현재 소유하고 있는 쓰레드 추적을 위해 ThreadId를 flag 요소로 사용
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while(true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 시도해서 성공하면 return
                    //if (_flag == EMPTY_FLAG)
                    //{
                    //    _flag = desired;    // 멀티 쓰레드 환경에서는 다음과 같이 비교와 할당 단계가 분리되어 있으면 안된다
                    //    return;
                    //}

                    // CompareExchange : _flag 값이 EMPTY_FLAG이면 desired로 할당하며 성공 여부에 상관없이 기존값을 리턴함
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
            {
                // _flag를 초기상태로 설정 (할당 해제)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            }
        }

        // ReadLock은 상호배타적 수행이 아니다!
        public void ReadLock()
        {
            // 동일 쓰레드가 ReadLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 올린다
            while (true)
            {
                for (int i = 0; i <= MAX_SPIN_COUNT; i++)
                {
                    //if ((_flag & WRITE_MASK) == 0)
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}

                    // WriteLock이 걸려있다면 WRITE_MASK에 값이 있으므로 실패조건이 됨
                    int expected = (_flag & READ_MASK); // A와 B가 ReadLock을 동시 경합 시, 하나만 Read에 접근 가능
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }

    class b10_ReaderWriterCustomImplement
    {
        static volatile int count = 0;
        static b10_Lock _lock = new b10_Lock();

        void Main(string[] args)
        {
            Task t1 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                    _lock.WriteUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(count);
        }
    }
}
