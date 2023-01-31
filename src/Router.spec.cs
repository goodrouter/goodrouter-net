namespace goodrouter;

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
}
