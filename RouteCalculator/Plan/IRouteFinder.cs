﻿namespace RouteCalculator.Plan
{
    using System.Collections.Generic;
    using RouteCalculator.Map;
    using RouteCalculator.Specify;

    /// <summary>
    /// This class finds a route according to a specification
    /// </summary>
    public interface IRouteFinder
    {
        /// <summary>
        /// Finds the routes that satisfy a specification
        /// </summary>
        /// <param name="map">The map of all railroads and cities.</param>
        /// <param name="specification">The specification to satisfy.</param>
        /// <returns>
        /// The routes that satisfy the specified attributes
        /// </returns>
        IEnumerable<IRoute> FindRoutes(IRailroadMap map, IRouteSpecification specification);

        /// <summary>
        /// Finds the first satisfying route to the specification.
        /// </summary>
        /// <param name="map">The map of all railroads and cities.</param>
        /// <param name="specification">The specification to satisfy.</param>
        /// <returns>
        /// The first route to satisfy the previously specified attributes
        /// </returns>
        IRoute FindFirstSatisfyingRoute(IRailroadMap map, IRouteSpecification specification);
    }
}
