
namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    /*
     * JobQueue의 실행 정책 2가지 방법
     * 1. 메인 쓰레드가 순차적으로 돌면서 queue를 비우기
     * 2. Push 시 맨 처음 queue에 넣는 경우라면 전체 실행하기, 아니면 넣기만 하고 끝내기 방식
     */

    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new();
        object _lock = new object();
        bool _flush = false; // Queue에 쌓인 작업을 처리하는 중인지 아닌지 상태

        public void Push(Action job)
        {
            bool flush = false;

            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                    flush = _flush = true;
            }

            if (flush)
                Flush();
        }

        /*
         * Queue에 쌓인 작업을 처리하는 메소드
         * 
         * 추가) lock은 Pop에 걸려 있으므로, while문이 돌아가는 동안 Pop 이전, 이후 상태에서 Push가 가능하다
         *       => Flush 작업 동안 작업을 추가 할 수 있다 !!!
         */
        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }

                return _jobQueue.Dequeue();
            }
        }
    }
}
