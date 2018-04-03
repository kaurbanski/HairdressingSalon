using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Constants;

namespace WebApplication3.Tests.ConstantsTest
{
    [TestClass]
    public class TestDateUtils
    {
        [TestMethod]
        public void IsHoliday_ShouldReturn13HolidayDaysForEachYear()
        {
            // Arrange
            int expectedQuantityOfHolidaysInPoland = 13;
            int currentYear = DateTime.Now.Year;

            // Act
            var result1 = PolandHolidays.GetHolidays(currentYear);
            var result2 = PolandHolidays.GetHolidays(currentYear + 1);
            var result3 = PolandHolidays.GetHolidays(currentYear + 2);

            // Assert
            Assert.AreEqual(expectedQuantityOfHolidaysInPoland, result1.Count());
            Assert.AreEqual(expectedQuantityOfHolidaysInPoland, result2.Count());
            Assert.AreEqual(expectedQuantityOfHolidaysInPoland, result3.Count());
        }

        [TestMethod]
        public void IsHoliday_CheckIfListOfHolidaysIsSorted()
        {
            // Act
            var result = PolandHolidays.GetHolidays(DateTime.Now.Year).ToList();
            var sorted = result.OrderBy(x => x.Date).ToList();

            // Assert
            CollectionAssert.AreEqual(sorted, result);
        }

        [TestMethod]
        public void GetTodayDateWithNoMinutesAndHours_ShouldReturnTodayDateTimeWithNoHoursAndMinutes()
        {
            // Arrange
            DateTime today = DateTime.Now;

            // Act
            DateTime result = DateUtils.GetTodayDateWithNoMinutesAndHours();

            // Assert
            Assert.AreEqual(today.Year, result.Year);
            Assert.AreEqual(today.Month, result.Month);
            Assert.AreEqual(today.Day, result.Day);
            Assert.AreEqual(result.Minute, 0);
            Assert.AreEqual(result.Second, 0);
        }
    }
}
