using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataDemo.Leaders;

namespace ODataDemo.Controllers
{
    public static class ODataModel
    {
        public static ODataOptions AddDemoApiV1Model(this ODataOptions? options)
        {
            options ??= new ODataOptions();
            options.AddRouteComponents(
                "v1",
                CreateV1Model(),
                 services =>
                 {
                     services.AddSingleton(
                    new ODataMessageReaderSettings()
                    {
                        MaxProtocolVersion = ODataVersion.V401,
                        Version = ODataVersion.V401,

                    });
                     services.AddSingleton<ODataBatchHandler>(sp => new DefaultODataBatchHandler { PrefixName = "v1" });
                 });
            return options;
        }

        public static IEdmModel CreateV1Model()
        {
            var modelBuilder = new ODataConventionModelBuilder
            {
                ContainerName = "ODataDemo",
                Namespace = "Demo",
            };
            modelBuilder.AddLeadersModel();
            modelBuilder.AddOrderModel();
            modelBuilder.EnableLowerCamelCase();

            return modelBuilder.GetEdmModel();
        }

        private static ODataModelBuilder AddOrderModel(this ODataModelBuilder modelBuilder)
        {
            var orders = modelBuilder.EntitySet<OrderDTO>("Orders");
            orders.HasDeepUpdateSupport().IsSupported(true);
            orders.HasUpdateRestrictions().IsDeltaUpdateSupported(true);
            orders.HasManyBinding(customer => customer.Items, "OrderItems");

            var orderBuilder = orders.EntityType;
            orderBuilder.HasKey(order => order.Id);
            orderBuilder.Select().Filter().Page(30, 30).Expand();
            var orderItems = orderBuilder.HasMany(order => order.Items);
            orderItems.Page(30, 30);

            var orderItemBuilder = modelBuilder.EntityType<OrderItemDTO>();
            orderItemBuilder.HasKey(item => new { item.OrderId, item.Id });
            orderItemBuilder.Select().Filter().Page(30, 30).OrderBy().Expand();
            orderItemBuilder.HasUpdateRestrictions().IsDeltaUpdateSupported(true).IsUpdatable(false);

            return modelBuilder;
        }

        private static ODataModelBuilder AddLeadersModel(this ODataModelBuilder modelBuilder)
        {
            var leaders = modelBuilder.EntitySet<LeaderDTO>("Leaders");
            leaders.HasTopSupported().IsTopSupported(false);
            leaders.HasComputeSupported().IsComputeSupported(false);
            var leaderModel = leaders.EntityType;
            leaderModel.HasKey(leader => leader.Id);
            leaderModel.Property(leader => leader.City).IsNonFilterable().IsNotCountable().IsNotSortable();
            leaderModel.Property(leader => leader.DateOfBirth).IsRequired();

            leaderModel.Select().Filter().Page(10, 2).Count().OrderBy(true);

            var countries = modelBuilder.EntitySet<CountryDTO>("Countries");
            var countryModel = countries.EntityType;
            countryModel.HasKey(item => item.Id);
            countryModel.Select().Filter().Page(10, 2).OrderBy().Expand().Count();

            return modelBuilder;
        }

    }
}
