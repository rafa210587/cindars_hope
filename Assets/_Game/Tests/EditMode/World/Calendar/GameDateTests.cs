using CindarsHope.World.Calendar;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.World.Calendar
{
    public class GameDateTests
    {
        [Test]
        public void GameDate_Day1_ReturnsYearSeasonDayOfWeekCorrectly()
        {
            var date = GameDate.FromAbsoluteDay(1);
            Assert.AreEqual(1, date.AbsoluteDay);
            Assert.AreEqual(1, date.Year);
            Assert.AreEqual(Season.Primavera, date.CurrentSeason);
            Assert.AreEqual(1, date.DayInSeason);
            Assert.AreEqual(1, date.DayOfWeek);
        }

        [Test]
        public void GameDate_Day28_LastDayOfSeason()
        {
            var date = GameDate.FromAbsoluteDay(28);
            Assert.AreEqual(28, date.DayInSeason);
            Assert.AreEqual(Season.Primavera, date.CurrentSeason);
        }

        [Test]
        public void GameDate_Day29_FirstDayOfSecondSeason()
        {
            var date = GameDate.FromAbsoluteDay(29);
            Assert.AreEqual(1, date.DayInSeason);
            Assert.AreEqual(Season.Verao, date.CurrentSeason);
        }

        [Test]
        public void GameDate_Day112_LastDayOfYear()
        {
            var date = GameDate.FromAbsoluteDay(112);
            Assert.AreEqual(28, date.DayInSeason);
            Assert.AreEqual(Season.Inverno, date.CurrentSeason);
            Assert.AreEqual(1, date.Year);
        }

        [Test]
        public void GameDate_Day113_FirstDayOfNextYear()
        {
            var date = GameDate.FromAbsoluteDay(113);
            Assert.AreEqual(1, date.DayInSeason);
            Assert.AreEqual(Season.Primavera, date.CurrentSeason);
            Assert.AreEqual(2, date.Year);
        }

        [Test]
        public void GameDate_DayOfWeek_CyclesCorrectly()
        {
            for (int i = 1; i <= 14; i++)
            {
                var date = GameDate.FromAbsoluteDay(i);
                int expectedDayOfWeek = ((i - 1) % 7) + 1;
                Assert.AreEqual(expectedDayOfWeek, date.DayOfWeek, $"Day {i} should have DayOfWeek {expectedDayOfWeek}");
            }
        }

        [Test]
        public void GameDate_NegativeDay_ClampsTo1()
        {
            var date = GameDate.FromAbsoluteDay(-5);
            Assert.AreEqual(1, date.AbsoluteDay);
        }

        [Test]
        public void GameDate_Seasons_AreCorrectly_Indexed()
        {
            var primavera = GameDate.FromAbsoluteDay(1);
            Assert.AreEqual(Season.Primavera, primavera.CurrentSeason);

            var verao = GameDate.FromAbsoluteDay(29);
            Assert.AreEqual(Season.Verao, verao.CurrentSeason);

            var outono = GameDate.FromAbsoluteDay(57);
            Assert.AreEqual(Season.Outono, outono.CurrentSeason);

            var inverno = GameDate.FromAbsoluteDay(85);
            Assert.AreEqual(Season.Inverno, inverno.CurrentSeason);
        }
    }
}
