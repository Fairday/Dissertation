namespace Dissertation.Modeling.Helpers
{
    public static class MathEx
    {
        public static int NOD(int a, int b)
        {
            var nod = 0;

            var q = a / b;
            var ret = a - q * b;

            if (ret == 0)
                nod = b;

            while (ret != 0)
            {
                q = b / ret;
                var newRet = b - ret *q;
                b = ret;
                ret = newRet;
                if (b != 0 )
                    nod = b;
            }
            return nod;
        }
    }
}
