# GoodRouter, .NET edition

A good router should:

- [x] work in a server or a client (or any other) environment
- [x] be able to construct routes based on their name
- [x] should have a simple API!
- [x] not do the actual navigation!
- [x] be framework agnostic
- [x] be very minimal and simple!

Check out the [website](https://www.goodrouter.org), join our [Discord server](https://discord.gg/BJ8v7xTq8d)!

Example:

```csharp
var router = new Router();

router.InsertRoute("all-products", "/product/all");
router.InsertRoute("product-detail", "/product/{id}");

// And now we can parse routes!

{
    var route = router.ParseRoute("/not-found");
    Assert.Equal(null, route);
}

{
    var route = router.ParseRoute("/product/all");
    Assert.Equal(
        new Route(
            "all-products"
        ),
        route
    );
}

{
    var route = router.ParseRoute("/product/1");
    Assert.Equal(
        new Route(
        "product-detail",
            new Dictionary<string, string>() {
                {"id", "1"}
            }
        ),
        route
    );
}

// And we can stringify routes

{
    var path = router.StringifyRoute(new Route(
        "all-products"
    ));
    Assert.Equal("/product/all", path);
}

{
    var path = router.StringifyRoute(new Route(
        "product-detail",
        new Dictionary<string, string>() {
            {"id", "2"}
        }
    ));
    Assert.Equal("/product/2", path);
}
```
