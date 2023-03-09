using Xunit;

public class RouterSpec
{

    [Fact]
    public void RouterTest()
    {
        var router = new Router();

        router.InsertRoute("a", "/a");
        router.InsertRoute("b", "/b/{x}");
        router.InsertRoute("c", "/b/{x}/c");
        router.InsertRoute("d", "/b/{x}/d");

        {
            var (routeKey, routeParameters) = router.ParseRoute("/a");
            Assert.Equal("a", routeKey);
            Assert.Equal(new Dictionary<string, string>() { }, routeParameters);
        }

        {
            var (routeKey, routeParameters) = router.ParseRoute("/b/x");
            Assert.Equal("b", routeKey);
            Assert.Equal(new Dictionary<string, string>() { { "x", "x" } }, routeParameters);
        }

        {
            var (routeKey, routeParameters) = router.ParseRoute("/b/y/c");
            Assert.Equal("c", routeKey);
            Assert.Equal(new Dictionary<string, string>() { { "x", "y" } }, routeParameters);
        }

        {
            var (routeKey, routeParameters) = router.ParseRoute("/b/z/d");
            Assert.Equal("d", routeKey);
            Assert.Equal(new Dictionary<string, string>() { { "x", "x" } }, routeParameters);
        }

    }

    // [Fact]
    // public void RouterTestReadme()
    // {
    //     var router = new Router();

    //     router.InsertRoute("all-products", "/product/all");
    //     router.InsertRoute("product-detail", "/product/{id}");

    //     // And now we can parse routes!

    //     {
    //         var route = router.ParseRoute("/not-found");
    //         Assert.Null(route);
    //     }

    //     {
    //         var route = router.ParseRoute("/product/all");
    //         Assert.Equal(
    //             new Route(
    //                 "all-products"
    //             ),
    //             route
    //         );
    //     }

    //     {
    //         var route = router.ParseRoute("/product/1");
    //         Assert.Equal(
    //             new Route(
    //             "product-detail",
    //                 new Dictionary<string, string>() {
    //                     {"id", "1"}
    //                 }
    //             ),
    //             route
    //         );
    //     }

    //     // And we can stringify routes

    //     {
    //         var path = router.StringifyRoute(new Route(
    //            "all-products"
    //         ));
    //         Assert.Equal("/product/all", path);
    //     }

    //     {
    //         var path = router.StringifyRoute(new Route(
    //            "product-detail",
    //             new Dictionary<string, string>() {
    //                 {"id", "2"}
    //             }
    //         ));
    //         Assert.Equal("/product/2", path);
    //     }

    // }

}
