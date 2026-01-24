namespace price_bot.Style
{
    public class ProgressBarService
    {
        private const int progressBarAccuracyNumber = 2;

        public static int UpdateProgressBar(int listLength, int NumberCount)
        {
            var productPercentage = ((float)NumberCount / listLength * 100) / progressBarAccuracyNumber;
            var numberNotFilled = (100 / progressBarAccuracyNumber) - (int)productPercentage;
            int cleanPercentage = (int)((float)NumberCount / listLength * 100);

            string percentageDisplay = $"{cleanPercentage}%";

            string percentageFilled = "".PadRight((int)productPercentage);

            string amountNotFilled = "".PadRight(numberNotFilled);

            string progressBarFull = percentageFilled + amountNotFilled;

            string completeProgressBar = progressBarFull.Insert((progressBarFull.Length / 2) - (percentageDisplay.Length / 2), percentageDisplay).Remove(progressBarFull.Length);

            Console.Write("\r[");

            Console.BackgroundColor = ConsoleColor.Blue;

            Console.Write(completeProgressBar.Substring(0, percentageFilled.Length));

            Console.ResetColor();

            Console.Write(completeProgressBar.Substring(percentageFilled.Length, completeProgressBar.Length - percentageFilled.Length) + "]");
            
            return cleanPercentage;
        }
    }
}