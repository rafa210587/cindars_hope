using NUnit.Framework;
using CindarsHope.UI.Calendar;
using CindarsHope.UI.Runtime;
using CindarsHope.UI.Runtime.Screens;
using CindarsHope.UI.Modal;
using CindarsHope.World.Calendar;
using CindarsHope.World.Lunar;
using CindarsHope.World.Weather;

namespace CindarsHope.Tests.EditMode.UI.Calendar
{
    /// <summary>
    /// fable_20 — testes de lógica determinística do widget de relógio/calendário e da tela de
    /// detalhe do dia: formatação de data/hora (nomes Vaalaran), spoiler gate dos picos lunares,
    /// binding do modelo de detalhe e ordem de foco navegável. Pure C#, sem UnityEngine.
    /// </summary>
    [TestFixture]
    public class CalendarUiTests
    {
        // ---------------- Nomes de exibição (CA-1 formatação) ----------------

        [Test]
        public void SeasonName_MapsEnumToCanonicalVaalaranNames_WithoutReordering()
        {
            Assert.AreEqual("Semeio", CalendarDisplayNames.SeasonName(Season.Primavera));
            Assert.AreEqual("Brasa", CalendarDisplayNames.SeasonName(Season.Verao));
            Assert.AreEqual("Véu", CalendarDisplayNames.SeasonName(Season.Outono));
            Assert.AreEqual("Gelo", CalendarDisplayNames.SeasonName(Season.Inverno));
        }

        [Test]
        public void WeekdayLabel_HasNoProperName_UsesDIndex_AndClamps()
        {
            Assert.AreEqual("D1", CalendarDisplayNames.WeekdayLabel(1));
            Assert.AreEqual("D7", CalendarDisplayNames.WeekdayLabel(7));
            Assert.AreEqual("D1", CalendarDisplayNames.WeekdayLabel(0));   // clamp baixo
            Assert.AreEqual("D7", CalendarDisplayNames.WeekdayLabel(99));  // clamp alto
        }

        [Test]
        public void LunarPhaseName_CoversAllEightRuntimePhases()
        {
            Assert.AreEqual("Lua Nova", CalendarDisplayNames.LunarPhaseName(LunarPhase.NewMoon));
            Assert.AreEqual("Lua Cheia", CalendarDisplayNames.LunarPhaseName(LunarPhase.FullMoon));
            Assert.AreEqual("Minguante Côncava", CalendarDisplayNames.LunarPhaseName(LunarPhase.WaningCrescent));
        }

        // ---------------- Widget model (CA-1 reflete data/clima/lua/nível) ----------------

        [Test]
        public void WidgetModel_SetDate_DerivesSeasonDayYearAndDateLabel()
        {
            var model = new ClockCalendarHudWidgetModel();
            // Dia 1 = Semeio, dia-da-estação 1, ano 1.
            model.SetDate(GameDate.FromAbsoluteDay(1));
            Assert.AreEqual("Semeio", model.SeasonName);
            Assert.AreEqual(1, model.DayInSeason);
            Assert.AreEqual(1, model.Year);
            StringAssert.Contains("Semeio", model.DateLabel);
            StringAssert.Contains("Ano 1", model.DateLabel);
        }

        [Test]
        public void WidgetModel_SetDate_SeasonRollAndYearRoll()
        {
            var model = new ClockCalendarHudWidgetModel();
            model.SetDate(GameDate.FromAbsoluteDay(29)); // primeiro dia da 2ª estação (Brasa)
            Assert.AreEqual("Brasa", model.SeasonName);
            Assert.AreEqual(1, model.DayInSeason);

            model.SetDate(GameDate.FromAbsoluteDay(113)); // ano 2, dia 1
            Assert.AreEqual(2, model.Year);
            Assert.AreEqual("Semeio", model.SeasonName);
        }

        [Test]
        public void WidgetModel_Phase_TogglesLabel()
        {
            var model = new ClockCalendarHudWidgetModel();
            Assert.AreEqual("Dia", model.PhaseLabel);
            model.SetPhase(isNight: true);
            Assert.IsTrue(model.IsNight);
            Assert.AreEqual("Noite", model.PhaseLabel);
        }

        [Test]
        public void WidgetModel_Weather_UsesGeneratorDescription()
        {
            var model = new ClockCalendarHudWidgetModel();
            model.SetWeather(WeatherType.Rainy);
            Assert.AreEqual(WeatherGenerator.GetWeatherDescription(WeatherType.Rainy), model.WeatherName);
        }

        [Test]
        public void WidgetModel_Lunar_ReflectsPhaseAndDayInPhase()
        {
            var model = new ClockCalendarHudWidgetModel();
            model.SetLunar(LunarCycle.FromAbsoluteDay(1));
            Assert.AreEqual(LunarPhase.NewMoon, model.LunarPhase);
            Assert.AreEqual(1, model.LunarDayInPhase);
        }

        [Test]
        public void WidgetModel_Level_XpPercentClampsAndLabel()
        {
            var model = new ClockCalendarHudWidgetModel();
            Assert.IsFalse(model.HasLevel);
            Assert.AreEqual(string.Empty, model.LevelLabel);

            model.SetLevel(level: 5, currentXp: 50, xpForNext: 100);
            Assert.IsTrue(model.HasLevel);
            Assert.AreEqual("Nv 5", model.LevelLabel);
            Assert.AreEqual(0.5f, model.XpPercent, 0.0001f);

            model.SetLevel(level: 5, currentXp: 999, xpForNext: 100); // satura em 1
            Assert.AreEqual(1f, model.XpPercent, 0.0001f);

            model.SetLevel(level: 5, currentXp: 10, xpForNext: 0); // próximo desconhecido
            Assert.AreEqual(0f, model.XpPercent, 0.0001f);
        }

        // ---------------- Spoiler gate dos picos lunares (CA-2) ----------------

        [Test]
        public void LunarPeaks_PeakDays_MapToThreeNamedMoons()
        {
            Assert.IsTrue(LunarVisibilityProjection.TryGetMoonForPeakDay(7, out var m7));
            Assert.AreEqual(VaalaraMoon.Alihana, m7);
            Assert.IsTrue(LunarVisibilityProjection.TryGetMoonForPeakDay(14, out var m14));
            Assert.AreEqual(VaalaraMoon.Senya, m14);
            Assert.IsTrue(LunarVisibilityProjection.TryGetMoonForPeakDay(21, out var m21));
            Assert.AreEqual(VaalaraMoon.Nyx, m21);
            Assert.IsFalse(LunarVisibilityProjection.TryGetMoonForPeakDay(5, out _));
        }

        [Test]
        public void LunarPeaks_KnownPeak_IsShown_UnknownPeak_IsHidden()
        {
            var dayWithAlihanaPeak = GameDate.FromAbsoluteDay(7); // dia-da-estação 7 = pico Alihana

            // Não descoberto → oculto (spoiler gate).
            var hidden = LunarVisibilityProjection.BuildKnownPeaksForDay(
                dayWithAlihanaPeak, (moon, day) => false);
            Assert.AreEqual(0, hidden.Count, "Pico não descoberto não pode aparecer");

            // Descoberto → visível.
            var shown = LunarVisibilityProjection.BuildKnownPeaksForDay(
                dayWithAlihanaPeak, (moon, day) => moon == VaalaraMoon.Alihana && day == 7);
            Assert.AreEqual(1, shown.Count);
            Assert.AreEqual("Alihana", shown[0].MoonName);
        }

        [Test]
        public void LunarPeaks_NonPeakDay_HasNoPeaks_EvenIfKnownPredicateTrue()
        {
            var nonPeakDay = GameDate.FromAbsoluteDay(3); // dia-da-estação 3, sem pico
            var peaks = LunarVisibilityProjection.BuildKnownPeaksForDay(nonPeakDay, (moon, day) => true);
            Assert.AreEqual(0, peaks.Count);
        }

        // ---------------- Binding do modelo de detalhe (CA-2) ----------------

        [Test]
        public void DayDetail_Build_BindsDateWeatherAndForecast()
        {
            var date = GameDate.FromAbsoluteDay(1);
            var model = CalendarDayDetailProjectionBuilder.Build(
                date, WeatherType.Clear, WeatherType.Rainy, hasForecast: true, isLunarPeakKnown: null);

            Assert.AreEqual(1, model.AbsoluteDay);
            Assert.AreEqual("Semeio", model.SeasonName);
            Assert.AreEqual(WeatherGenerator.GetWeatherDescription(WeatherType.Clear), model.CurrentWeather);
            Assert.IsTrue(model.HasForecast);
            Assert.AreEqual(WeatherGenerator.GetWeatherDescription(WeatherType.Rainy), model.ForecastWeather);
        }

        [Test]
        public void DayDetail_Build_NoForecast_HidesTomorrowWeather()
        {
            var date = GameDate.FromAbsoluteDay(1);
            var model = CalendarDayDetailProjectionBuilder.Build(
                date, WeatherType.Clear, WeatherType.Rainy, hasForecast: false, isLunarPeakKnown: null);

            Assert.IsFalse(model.HasForecast);
            Assert.IsNull(model.ForecastWeather);
        }

        [Test]
        public void DayDetail_Build_UnknownLunarPeak_StaysHidden()
        {
            var date = GameDate.FromAbsoluteDay(21); // pico Nyx, mas não descoberto
            var model = CalendarDayDetailProjectionBuilder.Build(
                date, WeatherType.Clear, WeatherType.Clear, hasForecast: true,
                isLunarPeakKnown: (moon, day) => false);

            Assert.IsFalse(model.HasLunarEvent, "Pico oculto permanece oculto");
            Assert.IsNull(model.KnownLunarEvent);
            Assert.AreEqual(0, model.KnownEventsText.Count);
        }

        [Test]
        public void DayDetail_Build_KnownLunarPeak_AppearsInModel()
        {
            var date = GameDate.FromAbsoluteDay(21); // pico Nyx, descoberto
            var model = CalendarDayDetailProjectionBuilder.Build(
                date, WeatherType.Clear, WeatherType.Clear, hasForecast: true,
                isLunarPeakKnown: (moon, day) => moon == VaalaraMoon.Nyx);

            Assert.IsTrue(model.HasLunarEvent);
            StringAssert.Contains("Nyx", model.KnownLunarEvent);
            Assert.AreEqual(1, model.KnownEventsText.Count);
        }

        // ---------------- Foco navegável da tela (CA-3) ----------------

        [Test]
        public void Screen_FocusOrder_OmitsSectionsWithoutContent_AndAlwaysEndsWithClose()
        {
            var model = new CalendarDayDetailModel { HasForecast = false, HasLunarEvent = false };
            var order = CalendarDayDetailScreenView.BuildFocusOrder(model);

            Assert.AreEqual(CalendarDayDetailScreenView.FocusToday, order[0]);
            Assert.AreEqual(CalendarDayDetailScreenView.FocusClose, order[order.Count - 1]);
            CollectionAssert.DoesNotContain(order, CalendarDayDetailScreenView.FocusForecast);
            CollectionAssert.DoesNotContain(order, CalendarDayDetailScreenView.FocusLunar);
        }

        [Test]
        public void Screen_FocusOrder_IncludesForecastAndLunarWhenPresent()
        {
            var model = new CalendarDayDetailModel { HasForecast = true, HasLunarEvent = true };
            var order = CalendarDayDetailScreenView.BuildFocusOrder(model);

            CollectionAssert.Contains(order, CalendarDayDetailScreenView.FocusForecast);
            CollectionAssert.Contains(order, CalendarDayDetailScreenView.FocusLunar);
        }

        [Test]
        public void Screen_FocusController_WrapsLinearly()
        {
            var model = new CalendarDayDetailModel { HasForecast = true, HasLunarEvent = false };
            var focus = new UiFocusController();
            focus.SetElements(CalendarDayDetailScreenView.BuildFocusOrder(model));

            // 3 elementos: today, forecast, close. Começa em today.
            Assert.AreEqual(CalendarDayDetailScreenView.FocusToday, focus.FocusedElementId);
            focus.Apply(UiFocusController.FocusInput.Next);
            Assert.AreEqual(CalendarDayDetailScreenView.FocusForecast, focus.FocusedElementId);
            focus.Apply(UiFocusController.FocusInput.Previous);
            focus.Apply(UiFocusController.FocusInput.Previous); // wrap para o último
            Assert.AreEqual(CalendarDayDetailScreenView.FocusClose, focus.FocusedElementId);
        }
    }
}
