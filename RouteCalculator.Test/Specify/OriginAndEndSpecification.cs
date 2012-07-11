﻿namespace RouteCalculator.Test.Specify
{
    using NSubstitute;
    using NUnit.Framework;
    using RouteCalculator.Map;
    using RouteCalculator.Plan;
    using RouteCalculator.Specify;

    /// <summary>
    /// This class contains all of the unit tests for the OrignAndEndSpecification class
    /// </summary>
    [TestFixture]
    public class OriginAndEndSpecificationTest
    {
        /// <summary>
        /// It contains the test data used for configuring the routes for this PathSpecificationTest
        /// </summary>
        private static object[] testDataForSatisfiedBy =
        {
            new object[] 
            {
                "A", "B", "A", "B", true
            },
            new object[] 
            {
                "A", "C", "A", "B", false
            },
            new object[] 
            {
                "A", "A", "A", "A", true
            },
            new object[] 
            {
                "A", "A", "B", "A", false
            }
        };

        /// <summary>
        /// Tests if it can specify origin and destination correctly
        /// </summary>
        /// <param name="routeOrigin">The route origin.</param>
        /// <param name="routeDestination">The route destination.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="expectedResult">if set to <c>true</c> [expected result].</param>
        [Test]
        [TestCaseSource("testDataForSatisfiedBy")]
        public void TestIfItCanSpecifyOriginAndDestinationCorrectly(string routeOrigin, string routeDestination, string origin, string destination, bool expectedResult)
        {
            // Arrange
            var target = new OriginAndDestinationSpecification(origin, destination);
            IRoute route = Substitute.For<IRoute>();
            route.Origin.Returns(new City()
            {
                Name = routeOrigin
            });
            route.Destination.Returns(new City()
            {
                Name = routeDestination
            });

            // Act
            bool actual = target.IsSatisfiedBy(route);

            // Assert
            Assert.AreEqual(expectedResult, actual);
            Assert.Null(route.Received().Destination);
            if (destination == routeDestination)
            {
                Assert.Null(route.Received().Origin);
            }
        }
    }
}
