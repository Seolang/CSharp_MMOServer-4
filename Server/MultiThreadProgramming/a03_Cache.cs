namespace MultiThreadProgramming
{
    /*
     * CPU의 Spatial locality 캐싱이 어떤 퍼포먼스 차이를 보여주는지 알아봄
     */

    class a03_Cache
    {
        void Main(string[] args)
        {
            // 2차원 배열
            int[,] arr = new int[10000, 10000];

            {
                long now = DateTime.Now.Ticks;
                for(int y=0; y<10000; y++)
                {
                    for(int x=0; x<10000; x++)
                    {
                        arr[y, x] = 1; // Spatial locality near
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린 시간 {end - now}");
            }

            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                {
                    for (int x = 0; x < 10000; x++)
                    {
                        arr[x, y] = 1; // Spatial locality far
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(x, y) 순서 걸린 시간 {end - now}");
            }
        }
    }
}
