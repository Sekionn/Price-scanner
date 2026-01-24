namespace price_bot.Timer;
public class Timer(int delayInHours)
{
    TimeSpan TargetTime = TimeSpan.FromHours(delayInHours);
    public async Task<bool> Countdown()
    {
        return await UpdateTimer();
    }

    private async Task<bool> UpdateTimer()
    {
        var target = DateTime.Now + TargetTime;

        bool targetNotReached = true;

        DateTime lastTick = DateTime.Now;
        while (targetNotReached)
        {
            DateTime now = DateTime.Now;
            var countdown = target - now;
            if (lastTick < now)
            {
                Console.Write($"\rTid til start: {countdown.Hours:D2}:{countdown.Minutes:D2}:{countdown.Seconds:D2}");
                lastTick = now.AddSeconds(1);
            }

            if (target <= now)
            {
                targetNotReached = false;
            }
        }

        return !targetNotReached;
    }
}