using System.Text.RegularExpressions;

/// <summary>
/// This is the router!
/// </summary>
public class Router
{
    private RouteNode rootNode = new RouteNode();
    private readonly Dictionary<string, RouteNode> leafNodes = new Dictionary<string, RouteNode>();

    private Regex parameterPlaceholderRE = new Regex("\\{(.*?)\\}");
    private int MaximumParameterValueLength = 20;

    /// <summary>
    /// Adds a new route
    /// </summary>
    /// <param name="routeKey">
    /// name of the route
    /// </param>
    /// <param name="routeTemplate">
    /// template for the route, als defines parameters
    /// </param>
    public Router InsertRoute(
        string routeKey,
        string routeTemplate
    )
    {
        var leafNode = this.rootNode.Insert(
            routeKey,
            routeTemplate,
            this.parameterPlaceholderRE
        );
        this.leafNodes.Add(routeKey, leafNode);
        return this;
    }

    /// <summary>
    /// Match the path against one of the provided routes and parse the parameters in it
    /// </summary>
    /// <param name="path">
    /// path to match
    /// </param>
    /// <returns>
    /// route that is matches to the path or null if no match is found
    /// </returns>
    public (string?, IReadOnlyDictionary<string, string>) ParseRoute(
        string path
    )
    {
        var parameters = new Dictionary<string, string>();

        var (routeKey, parameterNames, parameterValues) = this.rootNode.Parse(
            path,
            this.MaximumParameterValueLength
        );

        for (var index = 0; index < parameterNames.Length; index++)
        {
            var parameterName = parameterNames[index];
            var parameterValue = parameterValues[index];
            parameters[parameterName] = parameterValue;
        }

        return (
            routeKey,
            parameters
        );
    }

    /// <summary>
    /// Convert a route to a path string.
    /// </summary>
    /// <param name="routeKey">
    /// route to stringify
    /// </param>
    /// <returns>
    /// string representing the route or null if the route is not found by name
    /// </returns>
    public string? StringifyRoute(
        string routeKey
    )
    {
        return StringifyRoute(routeKey, new Dictionary<string, string>());
    }
    /// <summary>
    /// Convert a route to a path string.
    /// </summary>
    /// <param name="routeKey">
    /// route to stringify
    /// </param>
    /// <param name="routeParameters">
    /// parameters for the route
    /// </param>
    /// <returns>
    /// string representing the route or null if the route is not found by name
    /// </returns>
    public string? StringifyRoute(
        string routeKey,
        IReadOnlyDictionary<string, string> routeParameters
    )
    {
        var node = this.leafNodes[routeKey];
        if (node == null) return null;

        var parameterValues = new List<string>();
        foreach (var parameterName in node.RouteParameterNames)
        {
            var parameterValue = routeParameters[parameterName];
            parameterValues.Add(parameterValue);
        }

        return node.Stringify(parameterValues.ToArray());
    }

}

