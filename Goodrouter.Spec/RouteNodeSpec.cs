namespace Goodrouter.Spec;

using Xunit;

public class RouteNodeSpec
{

    [Fact]
    public void RouteNodeSortTest()
    {
        var routeNodes = new RouteNode[]{
            new RouteNode("aa", true),
            new RouteNode("aa"),
            new RouteNode("xx"),
            new RouteNode("aa", false, "n"),
            new RouteNode("x")
        };

        var sortedRouteNodes = new SortedSet<RouteNode>(routeNodes).ToArray();

        Assert.Equal(routeNodes, sortedRouteNodes);
    }
}
