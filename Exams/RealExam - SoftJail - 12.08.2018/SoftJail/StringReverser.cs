namespace SoftJail
{
    using System.Text;

    public static class StringReverser
    {
        public static string ReverseString(this string s)
        {
            var sb = new StringBuilder();

            for (int i = s.Length - 1; i >= 0; i--)
            {
                sb.Append(s[i]);
            }

            return sb.ToString();
        }
    }
}
