﻿using Gremlin.Net.CosmosDb.Structure;

namespace GremlinSample.Schema
{
    [Label("purchased")]
    public sealed class PersonPurchasedProductEdge : EdgeBase<PersonVertex, ProductVertex>
    {
    }
}