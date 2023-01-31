namespace goodrouter;

/// <summary>
/// This is the router!
/// </summary>
public class Router
{
    private RouteNode rootNode = new RouteNode();
    private readonly Dictionary<string, RouteNode> leafNodes = new Dictionary<string, RouteNode>();

    /// <summary>
    /// Adds a new route
    /// </summary>
    /// <param name="name">
    /// name of the route
    /// </param>
    /// <param name="template">
    /// template for the route, als defines parameters
    /// </param>
    public void InsertRoute(
        string name,
        string template
    )
    {
        var leafNode = this.rootNode.Insert(name, template);
        this.leafNodes.Add(name, leafNode);
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
    public Route? ParseRoute(
        string path
    )
    {
        var route = this.rootNode.Parse(path);
        return route;
    }

    /// <summary>
    /// Convert a route to a path string.
    /// </summary>
    /// <param name="route">
    /// route to stringify
    /// </param>
    /// <returns>
    /// string representing the route or null if the route is not found by name
    /// </returns>
    public string? StringifyRoute(
        Route route
    )
    {
        var node = this.leafNodes[route.Name];
        if (node == null) return null;
        return node.Stringify(route.Parameters);
    }

}

