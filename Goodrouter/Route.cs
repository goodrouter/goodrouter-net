namespace Goodrouter;

/// <summary>
/// This is a route...
/// </summary>
public class Route : IEquatable<Route>
{
    /// <summary>
    /// Name of a route
    /// </summary>
    public string Name { get; private set; }

    private readonly Dictionary<string, string> parameters;
    /// <summary>
    /// parameters of a route
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters
    {
        get
        {
            return this.parameters;
        }
    }

    /// <summary>
    /// Create a new Route class providing a name only
    /// </summary>
    /// <param name="name">
    /// name of the route
    /// </param>
    public Route(
        string name
    ) : this(name, new Dictionary<string, string>())
    {
    }
    /// <summary>
    /// Create a new Route class providing a name and some parameters
    /// </summary>
    /// <param name="name">
    /// name of the route
    /// </param>
    /// <param name="parameters">
    /// parameters for this route
    /// </param>
    public Route(
        string name,
        Dictionary<string, string> parameters
    )
    {
        this.Name = name;
        this.parameters = parameters;
    }

    /// <summary>
    /// Checks if the provided other Route is equal to this one
    /// </summary>
    /// <param name="other">
    /// Route to compare to this
    /// </param>
    /// <returns>
    /// True is the provided other route is considered equal to this one
    /// </returns>
    public bool Equals(Route? other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Name.Equals(other.Name) &&
             this.parameters.
                OrderBy(kv => kv.Key).
                SequenceEqual(other.parameters.OrderBy(kv => kv.Key));
    }
}
