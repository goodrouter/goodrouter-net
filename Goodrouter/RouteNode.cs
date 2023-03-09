namespace Goodrouter;

internal class RouteNode : IComparable<RouteNode>, IEquatable<RouteNode>
{
    public string Anchor { get; private set; }
    public string? Parameter { get; private set; }
    public string? RouteName { get; private set; }

    public string[] RouteParameterNames { get; private set; }

    private SortedSet<RouteNode> children = new SortedSet<RouteNode>();
    public IReadOnlySet<RouteNode> Children
    {
        get
        {
            return this.children;
        }
    }


    private RouteNode? parent;
    public RouteNode? Parent
    {
        get
        {
            return this.parent;
        }
    }


    public RouteNode(
        string anchor = "",
        string? parameter = null,
        string? routeName = null
    ) : this(anchor, parameter, routeName, new string[] { })
    {
    }
    public RouteNode(
        string anchor,
        string? parameter,
        string? routeName,
        string[] routeParameterNames
    )
    {
        this.Anchor = anchor;
        this.Parameter = parameter;
        this.RouteName = routeName;
        this.RouteParameterNames = routeParameterNames;
    }

    public RouteNode Insert(
        string name,
        string template
    )
    {
        var chainNodes = NewChain(name, template);
        chainNodes = chainNodes.Reverse();

        var currentNode = this;
        foreach (var chainNode in chainNodes)
        {
            var (commonPrefixLength, similarNode) = currentNode.FindSimilar(chainNode);
            if (similarNode == null)
            {
                currentNode = currentNode.InsertNew(
                    chainNode
                );
            }
            else
            {
                var strategy = similarNode.GetInsertStrategy(chainNode, commonPrefixLength);
                switch (strategy)
                {
                    case RouteNodeInsertStrategy.Merge:
                        currentNode = currentNode.InsertMerge(
                            chainNode,
                            similarNode
                        );
                        break;

                    case RouteNodeInsertStrategy.AddToThis:
                        currentNode = currentNode.InsertAddTo(
                            chainNode,
                            similarNode,
                            commonPrefixLength
                        );
                        break;

                    case RouteNodeInsertStrategy.AddToOther:
                        currentNode = currentNode.InsertAddTo(
                            similarNode,
                            chainNode,
                            commonPrefixLength
                        );
                        break;

                    case RouteNodeInsertStrategy.Intermediate:
                        currentNode = currentNode.InsertIntermediate(
                            chainNode,
                            similarNode,
                            commonPrefixLength
                        );
                        break;

                }

            }

        }

        return currentNode;
    }

    public Route? Parse(
        string path
    )
    {
        return Parse(
            path,
            new Dictionary<string, string>()
        );
    }
    public Route? Parse(
        string path,
        Dictionary<string, string> parameters
    )
    {
        parameters = new Dictionary<string, string>(parameters);

        if (this.Parameter == null)
        {
            if (!path.StartsWith(this.Anchor))
            {
                return null;
            }

            path = path.Substring(this.Anchor.Length);
        }
        else
        {
            if (path.Length == 0)
            {
                return null;
            }

            var index = this.Anchor.Length == 0 ?
                path.Length :
                path.IndexOf(this.Anchor);
            if (index < 0)
            {
                return null;
            }

            var value = path.Substring(0, index);

            path = path.Substring(index + this.Anchor.Length);

            parameters.Add(this.Parameter, value);
        }

        foreach (var childNode in this.children)
        {
            // find a route in every child node
            var route = childNode.Parse(
                (string)path,
                parameters
            );

            // if a child node is matches, return that node instead of the current! So child nodes are matches first!
            if (route != null)
            {
                return route;
            }
        }

        // if the node had a route name and there is no path left to match against then we found a route
        if (this.RouteName != null && path.Length == 0)
        {
            return new Route(
                this.RouteName,
                parameters
            );
        }

        // we did not found a route :-(
        return null;
    }

    public string Stringify(
        IReadOnlyDictionary<string, string> parameters
    )
    {
        var path = "";
        var currentNode = this;
        while (currentNode != null)
        {
            path = currentNode.Anchor + path;
            if (currentNode.Parameter != null && parameters.ContainsKey(currentNode.Parameter))
            {
                var value = parameters[currentNode.Parameter];
                path = value + path;
            }
            currentNode = currentNode.Parent;
        }
        return path;
    }

    private RouteNode InsertNew(
        RouteNode chainNode
    )
    {
        var childNode = new RouteNode(
            chainNode.Anchor,
            chainNode.Parameter,
            chainNode.RouteName
        );
        this.AddChild(childNode);
        return childNode;
    }

    private RouteNode InsertMerge(
        RouteNode appendNode,
        RouteNode receivingNode
    )
    {
        foreach (var childNode in appendNode.children)
        {
            appendNode.RemoveChild(childNode);
            receivingNode.AddChild(childNode);
        }
        return receivingNode;
    }

    private RouteNode InsertAddTo(
        RouteNode addNode,
        RouteNode receivingNode,
        int commonPrefixLength
    )
    {
        addNode.Anchor = addNode.Anchor.Substring(commonPrefixLength);
        addNode.Parameter = null;

        var childNode = receivingNode.Children.FirstOrDefault(
            childNode => childNode.Equals(addNode)
        );
        if (childNode == null)
        {
            if (addNode.Parent != null)
            {
                addNode.Parent.RemoveChild(addNode);
            }
            receivingNode.AddChild(addNode);
            return addNode;
        }
        else
        {
            return childNode;
        }
        throw new NotImplementedException();
    }

    private RouteNode InsertIntermediate(
        RouteNode newNode,
        RouteNode childNode,
        int commonPrefixLength
    )
    {
        var intermediateNode = new RouteNode(
            childNode.Anchor.Substring(0, commonPrefixLength),
            childNode.Parameter
        );
        this.AddChild(intermediateNode);
        this.RemoveChild(childNode);
        intermediateNode.AddChild(childNode);
        intermediateNode.AddChild(newNode);

        childNode.Anchor = childNode.Anchor.Substring(commonPrefixLength);
        newNode.Anchor = newNode.Anchor.Substring(commonPrefixLength);

        childNode.Parameter = null;
        newNode.Parameter = null;

        return newNode;
    }

    private (int, RouteNode?) FindSimilar(
        RouteNode otherNode
    )
    {
        if (this.Parameter != null) return (0, null);

        foreach (var childNode in this.Children)
        {
            if (childNode.Parameter != null) continue;

            var commonPrefixLength = StringUtility.FindCommonPrefixLength(otherNode.Anchor, childNode.Anchor);
            if (commonPrefixLength == 0) continue;

            return (commonPrefixLength, childNode);
        }

        return (0, null);
    }

    private RouteNodeInsertStrategy GetInsertStrategy(
        RouteNode otherNode,
        int commonPrefixLength
    )
    {
        var commonPrefix = this.Anchor.Substring(0, commonPrefixLength);

        if (this.Anchor == otherNode.Anchor)
        {
            if (
                this.RouteName != null &&
                otherNode.RouteName != null &&
                this.RouteName != otherNode.RouteName
            )
            {
                throw new ArgumentException("ambigous route");
            }
            else if (
                this.Parameter != null &&
                otherNode.Parameter != null &&
                this.Parameter != otherNode.Parameter
            )
            {
                return RouteNodeInsertStrategy.Intermediate;
            }
            else
            {
                return RouteNodeInsertStrategy.Merge;
            }
        }
        else if (this.Anchor == commonPrefix)
        {
            return RouteNodeInsertStrategy.AddToThis;
        }
        else if (otherNode.Anchor == commonPrefix)
        {
            return RouteNodeInsertStrategy.AddToOther;
        }
        else
        {
            return RouteNodeInsertStrategy.Intermediate;
        }
    }

    private static IEnumerable<RouteNode> NewChain(
        string name,
        string template
    )
    {
        var parts = StringUtility.ParsePlaceholders(template).Reverse().ToArray();
        string? currentName = name;

        for (var index = 0; index < parts.Length; index += 2)
        {
            var anchor = parts[index];
            var parameter = (index + 1) < parts.Length ?
                parts[index + 1] :
                null;

            if (anchor == null)
            {
                throw new ArgumentException();
            }

            yield return new RouteNode(
                anchor,
                parameter,
                currentName
            );

            currentName = null;
        }
    }

    private void AddChild(RouteNode node)
    {
        if (node.parent != null)
        {
            throw new ArgumentException("node already has a parent", "node");
        }

        node.parent = this;
        this.children.Add(node);
    }

    private void RemoveChild(RouteNode node)
    {
        node.parent = null;
        this.children.Remove(node);
    }

    public int CompareTo(RouteNode? other)
    {
        if (other == null)
        {
            return 1;
        }

        {
            var compared = this.Anchor.Length.CompareTo(other.Anchor.Length);
            if (compared != 0)
            {
                return 0 - compared;
            }
        }

        {
            var compared = (this.RouteName == null).CompareTo(other.RouteName == null);
            if (compared != 0)
            {
                return 0 - compared;
            }
        }

        {
            var compared = (this.Parameter == null).CompareTo(other.Parameter == null);
            if (compared != 0)
            {
                return compared;
            }
        }

        {
            var compared = this.Anchor.CompareTo(other.Anchor);
            if (compared != 0)
            {
                return compared;
            }
        }

        return 0;
    }

    public bool Equals(RouteNode? other)
    {
        if (other == null) return false;

        return this.Anchor == other.Anchor &&
            this.Parameter == other.Parameter &&
            this.RouteName == other.RouteName;
    }

}
