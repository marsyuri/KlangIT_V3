namespace KlangIT_V3.Helpers
{
    public static class Utility
    {
        public static string GetCurrentUserName()
        {
            // Implement logic to retrieve the current user's ID
            // This is a placeholder implementation and should be replaced with actual logic
            string[] userNames = new string[] { "Sunako", "DomeJRY", "TontIT3", "Kim" };
            var random = Random.Shared;
            int index = random.Next(userNames.Length);
            string currentUserName = userNames[index];
            return currentUserName;
        }
    }
}
