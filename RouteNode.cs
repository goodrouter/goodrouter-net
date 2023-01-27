namespace goodrouter;

internal class RouteNode
{
    public string? name;
    public string anchor = "";
    public string? parameter;
    public List<RouteNode> children = new List<RouteNode>();
    public RouteNode? parent;
}
