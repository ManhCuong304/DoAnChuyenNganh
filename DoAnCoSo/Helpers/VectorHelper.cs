namespace DoAnCoSo.Helpers
{
    public static class VectorMath
    {
        public static double CosineSimilarity(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return 0;

            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            magA = Math.Sqrt(magA);
            magB = Math.Sqrt(magB);

            return magA == 0 || magB == 0 ? 0 : dot / (magA * magB);
        }
    }
}
