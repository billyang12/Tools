namespace YXBPictureViewer.Services
{
    public enum EraseMethod
    {
        /// <summary>
        /// Single pass with zeros (fast, sufficient for most needs)
        /// </summary>
        Quick = 1,

        /// <summary>
        /// DoD 5220.22-M (3 passes): zeros, ones, random
        /// </summary>
        DoD3Pass = 3,

        /// <summary>
        /// DoD 5220.22-M (7 passes): zeros, ones, random, zeros, ones, random, random
        /// </summary>
        DoD7Pass = 7,

        /// <summary>
        /// Gutmann method (35 passes): maximum security, very slow
        /// </summary>
        Gutmann = 35
    }

    public static class EraseMethodExtensions
    {
        public static string GetDescription(this EraseMethod method)
        {
            return method switch
            {
                EraseMethod.Quick => "Quick (1 pass) - Fast, sufficient for most needs",
                EraseMethod.DoD3Pass => "DoD 3-Pass - Good security, balanced speed",
                EraseMethod.DoD7Pass => "DoD 7-Pass - High security, slower",
                EraseMethod.Gutmann => "Gutmann 35-Pass - Maximum security, very slow",
                _ => "Unknown"
            };
        }

        public static int GetPassCount(this EraseMethod method)
        {
            return (int)method;
        }
    }
}
