using System.Text.RegularExpressions;

// http://phiras.wordpress.com/2007/04/08/password-strength-meter-a-jquery-plugin/
// not my code, but can't remember where I got it from

namespace CryptoInator
{
    internal static class PasswordQuality
    {
        public static int GetPasswordScore(string password)
        {
            int score = 0;

            if (password.Length < 1)
            {
                return 1;
            }

            score += password.Length*4;
            score += (CheckRepetition(1, password).Length - password.Length);
            score += (CheckRepetition(2, password).Length - password.Length);
            score += (CheckRepetition(3, password).Length - password.Length);
            score += (CheckRepetition(4, password).Length - password.Length);

            var pattern = new Regex(@"(.*[\d].*[\d].*)");
            if (pattern.IsMatch(password))
                score += 5;

            pattern = new Regex(@"(.*[\W].*[\W].*)");
            if (pattern.IsMatch(password))
                score += 5;

            pattern = new Regex("(.*[a-z].*[A-Z].*)|(.*[A-Z].*[a-z].*)");
            if (pattern.IsMatch(password))
                score += 10;

            bool conditionOne = false;
            bool conditionTwo = false;

            pattern = new Regex(".*[a-zA-Z].*");
            conditionOne = pattern.IsMatch(password);

            pattern = new Regex(".*[0-9].*");
            conditionTwo = pattern.IsMatch(password);

            if (conditionTwo && conditionOne)
                score += 10;

            pattern = new Regex(@".*[\W].*");
            conditionOne = pattern.IsMatch(password);

            if (conditionTwo && conditionOne)
                score += 10;

            pattern = new Regex("(.*[a-zA-Z].*)");
            conditionTwo = pattern.IsMatch(password);

            if (conditionTwo && conditionOne)
                score += 10;

            pattern = new Regex("[0-9]*");
            conditionOne = pattern.IsMatch(password);
            if (conditionOne || conditionTwo)
                score -= 10;

            if (score < 1) score = 1;
            if (score > 100) score = 100;

            return score;
        }


        private static string CheckRepetition(int pLen, string str)
        {
            string res = "";
            for (int i = 0; i < str.Length; i++)
            {
                int j = 0;
                bool repeated = true;
                for (j = 0; j < pLen && (j + i + pLen) < str.Length; j++)
                {
                    repeated = repeated && (str[j + i] == str[j + i + pLen]);
                }
                if (j < pLen) repeated = false;
                if (repeated)
                {
                    i += pLen - 1;
                    repeated = false;
                }
                else
                {
                    res += str[i];
                }
            }
            return res;
        }
    }
}
