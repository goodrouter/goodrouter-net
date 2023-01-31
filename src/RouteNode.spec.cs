namespace goodrouter;

using Xunit;

public class RouteNodeSpec
{

    [Fact]
    public void RouteNodeSortTest()
    {
        var routeNodes = new RouteNode[]{
            new RouteNode("aa", "p"),
            new RouteNode("aa"),
            new RouteNode("xx"),
            new RouteNode("aa", null, "n"),
            new RouteNode("x")
        };

        var sortedRouteNodes = new SortedSet<RouteNode>(routeNodes).ToArray();

        Assert.Equal(routeNodes, sortedRouteNodes);
    }
}
