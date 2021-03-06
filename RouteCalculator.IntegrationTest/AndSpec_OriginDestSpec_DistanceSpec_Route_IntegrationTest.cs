﻿namespace RouteCalculator.IntegrationTest
{
    using System.Collections.Generic;
    using NSubstitute;
    using NSubstitute.Exceptions;
    using NUnit.Framework;
    using RouteCalculator.Map;
    using RouteCalculator.Plan;
    using RouteCalculator.Specify;
    using RouteCalculator.Testing;

    /// <summary>
    /// This class contains the integration tests for the ISpecification implementations along with the Route class
    /// </summary>
    [TestFixture]
    public class AndSpec_OriginDestSpec_DistanceSpec_Route_IntegrationTest
    {
        /// <summary>
        /// Tests if AndSpec, DistanceSpec and OriginAndDestinationSpec can specify a Route
        /// </summary>
        /// <param name="actualRoutePath">The actual route path.</param>
        /// <param name="specifiedMinDistance">The specified min distance.</param>
        /// <param name="specifiedMaxDistance">The specified max distance.</param>
        /// <param name="specifiedOrigin">The specified origin.</param>
        /// <param name="specifiedDestination">The specified destination.</param>
        /// <param name="callSatisfiedBy">if set to <c>true</c> [SatisfiedBy] will be used.</param>
        /// <param name="expectedResult">if set to <c>true</c> [expected result].</param>
        [Test]
        [TestCase(new string[] { "AB1" }, 1, 1, "A", "B", true, true)] // IsSatisfied: Valid
        [TestCase(new string[] { "AB2" }, 1, 2, "A", "B", true, true)] // IsSatisfied: Valid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 2, "A", "C", true, true)] // IsSatisfied: Valid
        [TestCase(new string[] { "AB2", "BC1" }, 1, 3, "A", "C", true, true)] // IsSatisfied: Valid
        [TestCase(new string[] { "AB1", "BC1", "CD1" }, 1, 3, "A", "D", true, true)] // IsSatisfied: Valid
        [TestCase(new string[] { "AB1" }, 1, 1, "A", "C", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1" }, 1, 1, "B", "B", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB2" }, 1, 1, "A", "B", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 1, "A", "B", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 2, "A", "B", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1", "BC2" }, 1, 2, "A", "C", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1", "BC2" }, 4, 7, "A", "C", true, false)] // IsSatisfied: Invalid
        [TestCase(new string[] { "AB1" }, 1, 1, "A", "C", false, true)] // MightBeSatisfied: Valid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 3, "A", "C", false, true)] // MightBeSatisfied: Valid
        [TestCase(new string[] { "AB1", "BD1" }, 1, 2, "A", "C", false, true)] // MightBeSatisfied: Valid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 3, "A", "D", false, true)] // MightBeSatisfied: Valid
        [TestCase(new string[] { "AB2" }, 1, 1, "A", "B", false, false)] // MIghtBeSatisfied: Invalid
        [TestCase(new string[] { "CB1" }, 1, 2, "A", "B", false, false)] // MIghtBeSatisfied: Invalid
        [TestCase(new string[] { "AB1", "BC1" }, 1, 1, "A", "B", false, false)] // MIghtBeSatisfied: Invalid
        public void TestIfAndSpecDistanceSpecAndOriginSpecCanSpecifyARoute(
            string[] actualRoutePath,
            int specifiedMinDistance,
            int specifiedMaxDistance,
            string specifiedOrigin,
            string specifiedDestination,
            bool callSatisfiedBy,
            bool expectedResult)
        {
            // Arrange
            IRouteSpecification originAndDestinationSpec =
                Substitute.For<OriginAndDestinationSpecification>(specifiedOrigin, specifiedDestination);
            IRouteSpecification distanceSpec =
                Substitute.For<DistanceSpecification>(specifiedMinDistance, specifiedMaxDistance);
            IRouteSpecification andSpec =
                Substitute.For<AndSpecification>(originAndDestinationSpec, distanceSpec);
            IRoute route = Substitute.For<Route>();
            IList<IRailroad> legs = TestHelper.GenerateLegs(actualRoutePath);
            foreach (IRailroad leg in legs)
            {
                route.AddLeg(leg);
            }

            bool actualResult = true;

            // Act
            if (callSatisfiedBy)
            {
                actualResult = GetSatisfyResult(originAndDestinationSpec, distanceSpec, andSpec, route);
            }
            else
            {
                actualResult = actualResult && GetMightSatisfyResult(originAndDestinationSpec, distanceSpec, andSpec, route);
            }

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        /// <summary>
        /// Tests if test helper gets correct results
        /// </summary>
        /// <param name="andSpecResult">if set to <c>true</c> [and spec result].</param>
        /// <param name="originAndDestinationResult">if set to <c>true</c> [origin and destination result].</param>
        /// <param name="badOrigin">if set to <c>true</c> [bad origin].</param>
        /// <param name="badDistance">if set to <c>true</c> [bad distance].</param>
        /// <param name="expectedResult">if set to <c>true</c> [expected result].</param>
        [TestCase(true, true, false, false, true)] // Valid behavior
        [TestCase(true, false, false, false, true)] // Valid behavior
        [TestCase(false, true, false, false, false)] // Valid behavior
        [TestCase(true, true, true, false, true, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [TestCase(true, true, false, true, true, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [TestCase(false, true, true, false, false, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [Test]
        public void TestIfTestHelperGetsCorrectResults(
            bool andSpecResult,
            bool originAndDestinationResult,
            bool badOrigin,
            bool badDistance,
            bool expectedResult)
        {
            // Arrange
            IRouteSpecification originAndDestinationSpec = Substitute.For<IRouteSpecification>();
            IRouteSpecification distanceSpec = Substitute.For<IRouteSpecification>();
            IRouteSpecification andSpec = Substitute.For<IRouteSpecification>();
            IRoute route = Substitute.For<IRoute>();

            // Specify distanceSpec behavior
            if (!badDistance)
            {
                distanceSpec.WhenForAnyArgs(s => s.IsSatisfiedBy(null))
                            .Do(c => c.Equals(route.Distance));
            }

            // Specify originAndDesintationSpec behavior
            if (!badOrigin)
            {
                originAndDestinationSpec.IsSatisfiedBy(null)
                                        .ReturnsForAnyArgs(originAndDestinationResult);
                originAndDestinationSpec.WhenForAnyArgs(s => s.IsSatisfiedBy(null))
                            .Do(c =>
                            {
                                Assert.NotNull(route.Origin);
                                if (originAndDestinationResult)
                                {
                                    Assert.NotNull(route.Destination);
                                    distanceSpec.IsSatisfiedBy(null);
                                }
                            });
            }

            // Specify andSpec behavior
            andSpec.IsSatisfiedBy(null)
                   .ReturnsForAnyArgs(andSpecResult);
            andSpec.WhenForAnyArgs(s => s.IsSatisfiedBy(null))
                   .Do(call =>
                       {
                           if (!badOrigin)
                           {
                               originAndDestinationSpec.IsSatisfiedBy(null);
                           }
                       });
            andSpec.WhenForAnyArgs(s => s.IsSatisfiedBy(null))
                   .Do(call =>
                       {
                           if (!badDistance && andSpecResult)
                           {
                               originAndDestinationSpec.IsSatisfiedBy(null);
                           }
                       });

            // Act
            bool actualResult = GetSatisfyResult(originAndDestinationSpec, distanceSpec, andSpec, route);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        /// <summary>
        /// Tests if Might test helper gets correct results
        /// </summary>
        /// <param name="andSpecResult">if set to <c>true</c> [and spec result].</param>
        /// <param name="originAndDestinationResult">if set to <c>true</c> [origin and destination result].</param>
        /// <param name="badOrigin">if set to <c>true</c> [bad origin].</param>
        /// <param name="badDistance">if set to <c>true</c> [bad distance].</param>
        /// <param name="expectedResult">if set to <c>true</c> [expected result].</param>
        [TestCase(true, true, false, false, true)] // Valid behavior
        [TestCase(true, false, false, false, true)] // Valid behavior
        [TestCase(false, true, false, false, false)] // Valid behavior
        [TestCase(true, true, true, false, true, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [TestCase(true, true, false, true, true, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [TestCase(false, true, true, false, false, ExpectedException = typeof(CallNotReceivedException))] // Invalid behavior
        [Test]
        public void TestIfMightTestHelperGetsCorrectResults(
            bool andSpecResult,
            bool originAndDestinationResult,
            bool badOrigin,
            bool badDistance,
            bool expectedResult)
        {
            // Arrange
            IRouteSpecification originAndDestinationSpec = Substitute.For<IRouteSpecification>();
            IRouteSpecification distanceSpec = Substitute.For<IRouteSpecification>();
            IRouteSpecification andSpec = Substitute.For<IRouteSpecification>();
            IRoute route = Substitute.For<IRoute>();

            // Specify distanceSpec behavior
            if (!badDistance)
            {
                distanceSpec.WhenForAnyArgs(s => s.MightBeSatisfiedBy(null))
                            .Do(c => c.Equals(route.Distance));
            }

            // Specify originAndDesintationSpec behavior
            if (!badOrigin)
            {
                originAndDestinationSpec.MightBeSatisfiedBy(null)
                                        .ReturnsForAnyArgs(originAndDestinationResult);
                originAndDestinationSpec.WhenForAnyArgs(s => s.MightBeSatisfiedBy(null))
                            .Do(c =>
                            {
                                Assert.NotNull(route.Origin);
                                if (originAndDestinationResult)
                                {
                                    distanceSpec.MightBeSatisfiedBy(null);
                                }
                            });
            }

            // Specify andSpec behavior
            andSpec.MightBeSatisfiedBy(null)
                   .ReturnsForAnyArgs(andSpecResult);
            andSpec.WhenForAnyArgs(s => s.MightBeSatisfiedBy(null))
                   .Do(call =>
                   {
                       if (!badOrigin)
                       {
                           originAndDestinationSpec.MightBeSatisfiedBy(null);
                       }
                   });
            andSpec.WhenForAnyArgs(s => s.MightBeSatisfiedBy(null))
                   .Do(call =>
                   {
                       if (!badDistance && andSpecResult)
                       {
                           originAndDestinationSpec.MightBeSatisfiedBy(null);
                       }
                   });

            // Act
            bool actualResult = GetMightSatisfyResult(originAndDestinationSpec, distanceSpec, andSpec, route);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        /// <summary>
        /// Gets the might satisfy result.
        /// </summary>
        /// <param name="originAndDestinationSpec">The origin and destination specification to use.</param>
        /// <param name="distanceSpec">The distance specification to use.</param>
        /// <param name="andSpec">The AndSpecification to use.</param>
        /// <param name="route">The route to test.</param>
        /// <returns>The result of the AndSpecification</returns>
        private static bool GetMightSatisfyResult(IRouteSpecification originAndDestinationSpec, IRouteSpecification distanceSpec, IRouteSpecification andSpec, IRoute route)
        {
            bool mightSatisfyResult = andSpec.MightBeSatisfiedBy(route);
            originAndDestinationSpec.ReceivedWithAnyArgs().MightBeSatisfiedBy(route);
            Assert.AreNotEqual(new object(), route.ReceivedWithAnyArgs().Origin);
            if (originAndDestinationSpec.MightBeSatisfiedBy(route))
            {
                distanceSpec.ReceivedWithAnyArgs().MightBeSatisfiedBy(route);
                Assert.AreNotEqual(new object(), route.ReceivedWithAnyArgs().Distance);
            }
            else
            {
                distanceSpec.DidNotReceiveWithAnyArgs().MightBeSatisfiedBy(route);
                Assert.AreNotEqual(new object(), route.DidNotReceiveWithAnyArgs().Distance);
            }

            return mightSatisfyResult;
        }

        /// <summary>
        /// Gets the actual result of calling IsSatisfiedBy on the <paramref name="andSpec"/>
        /// Also verifies the call is made correctly.
        /// </summary>
        /// <param name="originAndDestinationSpec">The origin and destination specification to use.</param>
        /// <param name="distanceSpec">The distance specification to use.</param>
        /// <param name="andSpec">The AndSpecification to use.</param>
        /// <param name="route">The route to test.</param>
        /// <returns>The result of the AndSpecification</returns>
        private static bool GetSatisfyResult(IRouteSpecification originAndDestinationSpec, IRouteSpecification distanceSpec, IRouteSpecification andSpec, IRoute route)
        {
            bool actualResult = andSpec.IsSatisfiedBy(route);
            originAndDestinationSpec.ReceivedWithAnyArgs().IsSatisfiedBy(route);
            Assert.AreNotEqual(new object(), route.ReceivedWithAnyArgs().Origin);
            if (originAndDestinationSpec.IsSatisfiedBy(route))
            {
                Assert.AreNotEqual(new object(), route.ReceivedWithAnyArgs().Destination);
                distanceSpec.ReceivedWithAnyArgs().IsSatisfiedBy(route);
                Assert.AreNotEqual(new object(), route.ReceivedWithAnyArgs().Distance);
            }
            else
            {
                distanceSpec.DidNotReceiveWithAnyArgs().IsSatisfiedBy(route);
                Assert.AreNotEqual(new object(), route.DidNotReceiveWithAnyArgs().Distance);
            }

            return actualResult;
        }
    }
}
