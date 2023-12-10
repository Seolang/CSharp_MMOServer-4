using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        List<T> _heap = new();

        public int Count { get { return _heap.Count; } }

        // O(logN)
        public void Push(T data)
        {
            // 힙의 맨 끝에 새로운 데이터를 삽입한다
            _heap.Add(data);

            int now = _heap.Count - 1; // 방금 넣은 데이터
            // 위치 승격 시작
            while (now > 0)
            {
                // 부모 노드와 비교
                int next = (now - 1) / 2;
                // 현재 값이 부모 값보다 우선 순위가 낮거나 같으면 종료
                if (_heap[now].CompareTo(_heap[next]) < 0)
                    break;

                // 두 값을 교체
                T temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 현재 위치 갱신
                now = next;
            }
        }

        // O(logN)
        public T Pop()
        {
            // 반환할 데이터를 따로 저장
            T ret = _heap[0];

            // 마지막 데이터를 루트로 이동한다
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);
            lastIndex--;

            // 위치 강등 시작
            int now = 0; // 맨 마지막에 있던 데이터
            while (true)
            {
                int left = 2 * now + 1;
                int right = 2 * now + 2;

                int next = now;
                // 왼쪽 값이 현재 값보다 우선순위가 높으면
                if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                    next = left;

                // 오른쪽 값이 현재 값(왼쪽 이동 포함)보다 크면, 오른쪽으로 이동
                if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)  
                    next = right;

                // 왼쪽 오른쪽 모두 현재 값보다 우선순위가 작으면 종료
                if (next == now)
                    break;

                // 두 값을 교체한다
                T temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 검사 위치를 이동
                now = next;
            }

            return ret;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
                return default(T);

            return _heap[0];
        }
    }
}
