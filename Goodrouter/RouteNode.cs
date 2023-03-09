using System.Text.RegularExpressions;

internal class RouteNode : IComparable<RouteNode>, IEquatable<RouteNode>
{
    public string Anchor { get; private set; }
    public bool HasParameter { get; private set; }
    public string? RouteKey { get; private set; }

    public string[] RouteParameterNames { get; private set; }

    private SortedSet<RouteNode> children = new SortedSet<RouteNode>();
    public IReadOnlySet<RouteNode> Children
    {
        get
        {
            return this.children;
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
        bool hasParameter = false,
        string? routeKey = null
    ) : this(
        anchor,
        hasParameter,
        routeKey,
        new string[] { }
    )
    {
    }
    public RouteNode(
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames
    )
    {
        this.Anchor = anchor;
        this.HasParameter = hasParameter;
        this.RouteKey = routeKey;
        this.RouteParameterNames = routeParameterNames;
    }

    public RouteNode Insert(
        string routeKey,
        string routeTemplate,
        Regex parameterPlaceholderRE
    )
    {
        var pairs =
            TemplateUtility.ParseTemplatePairs(routeTemplate, parameterPlaceholderRE).ToArray();

        var routeParameterNames = pairs.
            Select(pair =>
            {
                var (anchor, parameterName) = pair;
                return parameterName;
            }).
            Where(parameterName => parameterName != null).
            ToArray();

        var currentNode = this;
        for (var index = 0; index < pairs.Length; index++)
        {
            var (anchor, parameterName) = pairs[index];
            var hasParameter = parameterName != null;

            var (commonPrefixLength, childNode) =
                currentNode.FindSimilarChild(anchor, hasParameter);

            currentNode = currentNode.Merge(
                childNode,
                anchor,
                hasParameter,
                index == pairs.Length - 1 ? routeKey : null,
                routeParameterNames,
                commonPrefixLength
            );

        }

        return currentNode;
    }

    public (string?, string[], string[]) Parse(
        string path,
        int maximumParameterValueLength
    )
    {
        var parameterValues = new string[] { };

        if (this.HasParameter)
        {
            if (path.Length == 0)
            {
                return (null, new string[] { }, new string[] { });
            }

            var index = this.Anchor.Length == 0 ?
            path.Length :
            path.Substring(0, Math.Min(
                maximumParameterValueLength + this.Anchor.Length,
                path.Length
            )).
                IndexOf(this.Anchor);

            if (index < 0)
            {
                return (null, new string[] { }, new string[] { });
            }
        }
        else
        {
            if (!path.StartsWith(this.Anchor))
            {
                return (null, new string[] { }, new string[] { });
            }

            path = path.Substring(this.Anchor.Length);
        }

        foreach (var childNode in this.children)
        {
            var (childRouteKey, childRouteParameterNames, childParameterValues) = childNode.Parse(
                path,
                maximumParameterValueLength
            );

            if (childRouteKey != null)
            {
                return (
                    childRouteKey,
                    childRouteParameterNames,
                    parameterValues.Concat(childParameterValues).ToArray()
                );
            }
        }

        if (this.RouteKey != null && path.Length == 0)
        {
            return (
                this.RouteKey,
                this.RouteParameterNames,
                parameterValues
                );
        }

        return (null, new string[] { }, new string[] { });
    }

    public string Stringify(
        string[] parameterValues
    )
    {
        var parameterIndex = parameterValues.Length;
        var path = "";
        var currentNode = this;
        while (currentNode != null)
        {
            path = currentNode.Anchor + path;
            if (currentNode.HasParameter)
            {
                parameterIndex--;
                var value = parameterValues[parameterIndex];
                path = value + path;
            }
            currentNode = currentNode.Parent;
        }
        return path;
    }


    private RouteNode Merge(
        RouteNode? childNode,
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames,
        int commmonPrefixLength
    )
    {
        if (childNode == null)
        {
            return this.MergeNew(
                anchor,
                hasParameter,
                routeKey,
                routeParameterNames
            );
        }

        var commonPrefix = childNode.Anchor.Substring(0, commmonPrefixLength);

        if (childNode.Anchor == anchor)
        {
            return this.MergeJoin(
                childNode,
                routeKey,
                routeParameterNames
            );
        }
        else if (childNode.Anchor == commonPrefix)
        {
            return this.MergeAddToChild(
                childNode,
                anchor,
                hasParameter,
                routeKey,
                routeParameterNames,
                commmonPrefixLength
            );
        }
        else if (anchor == commonPrefix)
        {
            return this.MergeAddToNew(
                childNode,
                anchor,
                hasParameter,
                routeKey,
                routeParameterNames,
                commmonPrefixLength
            );
        }
        else
        {
            return this.MergeIntermediate(
                childNode,
                anchor,
                hasParameter,
                routeKey,
                routeParameterNames,
                commmonPrefixLength
            );
        }
    }

    private RouteNode MergeNew(
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames
    )
    {
        var newNode = new RouteNode(
            anchor,
            hasParameter,
            routeKey,
            routeParameterNames
        );
        this.AddChild(newNode);
        return newNode;
    }

    private RouteNode MergeJoin(
        RouteNode childNode,
        string? routeKey,
        string[] routeParameterNames
    )
    {
        if (
            childNode.RouteKey != null &&
            routeKey != null
        )
        {
            throw new Exception("ambiguous route");
        }

        if (childNode.RouteKey == null)
        {
            childNode.RouteKey = routeKey;
            childNode.RouteParameterNames = routeParameterNames;
        }

        return childNode;
    }

    private RouteNode MergeIntermediate(
        RouteNode childNode,
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames,
        int commmonPrefixLength
    )
    {
        this.RemoveChild(childNode);

        var newNode = new RouteNode(
            anchor.Substring(commmonPrefixLength),
            false,
            routeKey,
            routeParameterNames
        );

        childNode.Anchor = childNode.Anchor.Substring(commmonPrefixLength);
        childNode.HasParameter = false;

        var intermediateNode = new RouteNode(
            anchor.Substring(0, commmonPrefixLength),
            hasParameter
        );
        intermediateNode.AddChild(childNode);
        intermediateNode.AddChild(newNode);

        this.AddChild(intermediateNode);

        return newNode;
    }

    private RouteNode MergeAddToChild(
        RouteNode childNode,
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames,
        int commmonPrefixLength
    )
    {
        anchor = anchor.Substring(commmonPrefixLength);
        hasParameter = false;

        var (commonPrefixLength2, childNode2) =
            childNode.FindSimilarChild(anchor, hasParameter);

        return childNode.Merge(
            childNode2,
            anchor,
            hasParameter,
            routeKey,
            routeParameterNames,
            commonPrefixLength2
        );
    }

    private RouteNode MergeAddToNew(
        RouteNode childNode,
        string anchor,
        bool hasParameter,
        string? routeKey,
        string[] routeParameterNames,
        int commmonPrefixLength
    )
    {
        var newNode = new RouteNode(
            anchor,
            hasParameter,
            routeKey,
            routeParameterNames
        );
        this.AddChild(newNode);

        this.RemoveChild(childNode);

        childNode.Anchor = childNode.Anchor.Substring(commmonPrefixLength);
        childNode.HasParameter = false;

        newNode.AddChild(childNode);

        return newNode;
    }

    private (int, RouteNode?) FindSimilarChild(
        string anchor,
        bool hasParameter
    )
    {
        foreach (var childNode in this.children)
        {
            if (childNode.HasParameter != hasParameter)
            {
                continue;
            }

            var commonPrefixLength = StringUtility.FindCommonPrefixLength(anchor, childNode.Anchor);
            if (commonPrefixLength == 0)
            {
                continue;
            }

            return (commonPrefixLength, childNode);
        }

        return (0, null);
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
            var compared = this.HasParameter.CompareTo(other.HasParameter);
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
            this.HasParameter == other.HasParameter &&
            this.RouteKey == other.RouteKey;
    }

}
