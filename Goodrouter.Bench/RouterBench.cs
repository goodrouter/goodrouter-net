namespace Goodrouter.Bench;

using BenchmarkDotNet.Attributes;

public class RouterBench
{
    private Router router = new Router();

    public RouterBench()
    {
        router.InsertRoute("all-products", "/product/all");
        router.InsertRoute("product-detail", "/product/{id}");
    }


    [Benchmark]
    public void NotFound()
    {
        router.ParseRoute("/not-found");
    }

    [Benchmark]
    public void NoParameters()
    {
        router.ParseRoute("/product/all");
    }

    [Benchmark]
    public void OneParameter()
    {
        router.ParseRoute("/product/1");
    }

}

