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
    public class TestCostUtils
    {
        [TestMethod]
        public void GetCostOfTheVisitAfterDiscount_ShouldReturnCostAfterDiscountTheSameAsCostIfNoDiscount()
        {
            // Arrange
            decimal cost = 150;
            decimal discount = 0;

            // Act
            decimal result = CostUtils.GetCostOfTheVisitAfterDiscount(cost, discount);

            // Assert
            Assert.AreEqual(cost, result);
        }

        [TestMethod]
        public void GetCostOfTheVisitAfterDiscount_ShouldReturnCorrectCostAfterDiscount()
        {
            // Arrange
            decimal cost1 = 200;
            decimal cost2 = 175;
            decimal discount1 = 7;
            decimal discount2 = 30;

            // Act
            var cost1discount1 = Constants.CostUtils.GetCostOfTheVisitAfterDiscount(cost1, discount1);
            var cost1discount2 = Constants.CostUtils.GetCostOfTheVisitAfterDiscount(cost1, discount2);
            var cost2discount1 = Constants.CostUtils.GetCostOfTheVisitAfterDiscount(cost2, discount1);
            var cost2discount2 = Constants.CostUtils.GetCostOfTheVisitAfterDiscount(cost2, discount2);

            // Assert
            Assert.AreEqual(186, cost1discount1);
            Assert.AreEqual(140, cost1discount2);
            Assert.AreEqual(163, cost2discount1);
            Assert.AreEqual(122, cost2discount2);
        }
    }
}
