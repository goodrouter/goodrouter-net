namespace Goodrouter.Spec;

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
            var route = router.ParseRoute("/a");
            Assert.Equal(
                new Route("a"),
                route
            );
        }

        {
            var route = router.ParseRoute("/b/x");
            Assert.Equal(
                new Route("b", new Dictionary<string, string>() {
                    {"x","x"}
                }),
                route
            );
        }

        {
            var route = router.ParseRoute("/b/y/c");
            Assert.Equal(
                new Route("c", new Dictionary<string, string>() {
                    {"x","y"}
                }),
                route
            );
        }

        {
            var route = router.ParseRoute("/b/z/d");
            Assert.Equal(
                new Route("d", new Dictionary<string, string>() {
                    {"x","z"}
                }),
                route
            );
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
