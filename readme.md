**Assemblies affected**
Microsoft.AspNetCore.OData Version="8.0.11"
Microsoft.OData.Core" Version="7.12.5"
Microsoft.OData.Edm" Version="7.12.5"

**Describe the bug**
I am trying to implement a deep update according to [Update Related Entities When Updating an Entity](http://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#sec_UpdateRelatedEntitiesWhenUpdatinganE) and I am receiving an error when patches on nested collections occur.

**Reproduce steps**
Make a patch request that is sending a delta update on a nested collection.

**Data Model**

```csharp
public class OrderDTO
    {
        public string? Name { get; set; }
        public long Id { get; set; }
        public DateTime? Date { get; set; }
        public long? CustomerId { get; set; }
        public decimal? NetPrice { get; set; }
        public OrderItemDTO[] Items { get; set; }
    }
```

Controller:

```csharp
 [ApiController]
    public class OrderController : ODataController
    {

        public OrderController(ILogger<OrderController> logger)
        {
            Logger = logger;
        }

        public ILogger<OrderController> Logger { get; }

        private static Task<IEnumerable<OrderDTO>> GetData()
        {
            return Task.FromResult(Enumerable.Range(1, 20).Select(id => new OrderDTO()
            {
                CustomerId = id,
                Id = id,
                Date = DateTime.UtcNow.AddDays(id),
                Name = "test",
                NetPrice = Convert.ToDecimal(Random.Shared.NextDouble()),
                Items = Enumerable.Range(1, 20).Select(itemId => new OrderItemDTO()
                {
                    Id = itemId,
                    OrderId = id,
                    Price = Convert.ToDecimal(Random.Shared.NextDouble())
                }).ToArray()
            }));
        }

        [HttpGet("v1/Orders")]
        [EnableQuery]
        public async Task<IEnumerable<OrderDTO>> GetAll()
        {
            return await GetData();
        }



        [HttpPatch("v1/Orders/{orderId}")]
        [EnableQuery]
        public async Task<IEnumerable<OrderDTO>> PatchOrder([FromRoute] long orderId, [FromBody] Delta<OrderDTO> patchedOrder)
        {
            Logger.LogInformation("Patched Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(patchedOrder.GetInstance()));
            return await Task.FromResult(new List<OrderDTO>() { patchedOrder.GetInstance() });
        }


        [HttpPatch("v1/Orders/{orderId}/items")]
        [EnableQuery]
        public async void PatchOrderItems([FromRoute] long orderId, [FromBody] DeltaSet<OrderItemDTO> patchedOrder)
        {
            foreach (var item in patchedOrder)
            {
                if (item.Kind == DeltaItemKind.DeletedResource && item is DeltaDeletedResource<OrderItemDTO> deletedItem)
                {
                    Logger.LogInformation("Deleted Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(deletedItem.GetInstance()));
                }
                else if (item.Kind == DeltaItemKind.Resource && item is Delta<OrderItemDTO> upsert)
                {
                    Logger.LogInformation("Patched Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(upsert.GetInstance()));
                }
            }

        }
    }
```

**EDM (CSDL) Model**

```xml
<?xml version="1.0" encoding="utf-8"?>
  <edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
    <edmx:DataServices>
      <Schema Namespace="Demo" xmlns="http://docs.oasis-open.org/odata/ns/edm">
        <EntityType Name="OrderDTO">
          <Key>
            <PropertyRef Name="id" />
          </Key>
          <Property Name="id" Type="Edm.Int64" Nullable="false" />
          <Property Name="name" Type="Edm.String" />
          <Property Name="date" Type="Edm.DateTimeOffset" />
          <Property Name="customerId" Type="Edm.Int64" />
          <Property Name="netPrice" Type="Edm.Decimal" Scale="Variable" />
          <NavigationProperty Name="items" Type="Collection(Demo.OrderItemDTO)" />
        </EntityType>
        <EntityType Name="OrderItemDTO">
          <Key>
            <PropertyRef Name="id" />
            <PropertyRef Name="orderId" />
          </Key>
          <Property Name="orderId" Type="Edm.Int64" Nullable="false" />
          <Property Name="id" Type="Edm.Int32" Nullable="false" />
          <Property Name="price" Type="Edm.Decimal" Scale="Variable" />
        </EntityType>
        <EntityContainer Name="ODataDemo">
          <EntitySet Name="Orders" EntityType="Demo.OrderDTO">
            <NavigationPropertyBinding Path="items" Target="OrderItems" />
          </EntitySet>
          <EntitySet Name="OrderItems" EntityType="Demo.OrderItemDTO" />
        </EntityContainer>
        <Annotations Target="Demo.OrderItemDTO">
          <Annotation Term="Org.OData.Capabilities.V1.UpdateRestrictions">
            <Record>
              <PropertyValue Property="Updatable" Bool="true" />
              <PropertyValue Property="DeltaUpdateSupported" Bool="true" />
            </Record>
          </Annotation>
        </Annotations>
        <Annotations Target="Demo.ODataDemo/Orders">
          <Annotation Term="Org.OData.Capabilities.V1.DeepUpdateSupport">
            <Record>
              <PropertyValue Property="Supported" Bool="true" />
            </Record>
          </Annotation>
          <Annotation Term="Org.OData.Capabilities.V1.UpdateRestrictions">
            <Record>
              <PropertyValue Property="DeltaUpdateSupported" Bool="true" />
            </Record>
          </Annotation>
        </Annotations>
      </Schema>
    </edmx:DataServices>
  </edmx:Edmx>
```

**Request/Response**
Request Headers:

```
Connection: keep-alive
Accept: application/json;odata.metadata=minimal
Content-Type: application/json
OData-Version: 4.01
OData-MaxVersion: 4.01
Content-Length: 207
Host: localhost:5086
User-Agent: Apache-HttpClient/4.5.13 (Java/13.0.2)
```

Request Body:

```
PATCH http://localhost:5086/v1/orders/2

PATCH data:
{
	"name":"anothertest",
	"@id": "Order(2)",
	"items@delta":[
	{
		"@id": "OrderItems(2, 1)",
		"price":0
	},
	{
	"@removed":{
			"reason":"deleted"
			},
		"@id": "OrderItems(2, 2)"
		}
		]
}

[no cookies]
```

Response Headers:

```
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8
Date: Wed, 16 Nov 2022 06:25:36 GMT
Server: Kestrel
Transfer-Encoding: chunked
```

Response Body:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-e5e58a4c6478a3ed25f2de9eeb25ad4a-435d9e53608229b5-00",
  "errors": {
    "": ["The input was not valid."],
    "patchedOrder": ["The patchedOrder field is required."]
  }
}
```

**Expected behavior**
The updates on the OrderItems should be populated in the Delta<OrderDTO> when using nested delta updates.

**Additional context**
When I am using the endpoint `v1/Orders/{orderId}/items` the DeltaSet is populated correctly. Only when nested patches occur the delta is not working.

Exception message from OData:
```
The value "Microsoft.AspNetCore.OData.Deltas.Delta`1[ODataDemo.Controllers.OrderItemDTO]" is not of type "ODataDemo.Controllers.OrderItemDTO" and cannot be used in this generic collection. (Parameter 'value')
```

Stacktrace:
```
   at System.Collections.Generic.List`1.System.Collections.IList.Add(Object item)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.CollectionDeserializationHelpers.AddToCollectionCore(IEnumerable items, IEnumerable collection, Type elementType, IList list, MethodInfo addMethod, ODataDeserializerContext context)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.DeserializationHelpers.SetCollectionProperty(Object resource, String propertyName, IEdmCollectionTypeReference edmPropertyType, Object value, Boolean clearCollection, ODataDeserializerContext context)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer.ApplyNestedDeltaResourceSet(IEdmProperty nestedProperty, Object resource, ODataDeltaResourceSetWrapper deltaResourceSetWrapper, ODataDeserializerContext readContext)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer.ApplyNestedProperty(Object resource, ODataNestedResourceInfoWrapper resourceInfoWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer.ApplyNestedProperties(Object resource, ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer.ReadResource(ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
   at Microsoft.AspNetCore.OData.Formatter.Deserialization.ODataResourceDeserializer.ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
   at Microsoft.AspNetCore.OData.Formatter.ODataInputFormatter.ReadFromStreamAsync(Type type, Object defaultValue, Uri baseAddress, ODataVersion version, HttpRequest request, IList`1 disposes)
```
