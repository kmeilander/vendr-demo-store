using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Vendr.Common.Validation;
using Vendr.Core;
using Vendr.Core.Api;
using Vendr.DemoStore.Web.Dtos;
using Vendr.Extensions;
using System.Collections.Generic;
using System;

namespace Vendr.DemoStore.Web.Controllers
{
    public class CartSurfaceController : SurfaceController
    {
        private readonly IVendrApi _vendrApi;

        public CartSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, 
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IVendrApi vendrApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _vendrApi = vendrApi;
        }

        [HttpPost]
        public IActionResult AddToCart(AddToCartDto postModel)
        {
            for (var i = 0; i < 1; i++) //Add 50 orders
            {
                //try
                //{
                //    _vendrApi.Uow.Execute(uow =>
                //    {
                //        var store = CurrentPage.GetStore();
                //        _vendrApi.ClearCurrentOrder(store.Id);

                //        uow.Complete();
                //    });
                //}
                //catch (ValidationException ex)
                //{
                //    ModelState.AddModelError("productReference", "Failed to add product to cart");

                //    return CurrentUmbracoPage();
                //}

                try
                {
                    _vendrApi.Uow.Execute(uow =>
                    {
                        var store = CurrentPage.GetStore();
                        var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                            .AsWritable(uow)
                            .AddProduct(postModel.ProductReference, postModel.ProductVariantReference, 1);

                        _vendrApi.SaveOrder(order);

                        uow.Complete();
                    });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("productReference", "Failed to add product to cart");

                    return CurrentUmbracoPage();
                }

                //PlaceTestOrder();
            }

            TempData["addedProductReference"] = postModel.ProductReference;

            return RedirectToCurrentUmbracoPage();
        }

        public void PlaceTestOrder()
        {

            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetProperties(new Dictionary<string, string>
                        {
                            { Constants.Properties.Customer.EmailPropertyAlias, "kevinrm@emergentsoftware.net" },
                            { "marketingOptIn", "0" },

                            { Constants.Properties.Customer.FirstNamePropertyAlias, "Kevin" },
                            { Constants.Properties.Customer.LastNamePropertyAlias, "Test" },
                            { "billingAddressLine1", "123 Fake Street" },
                            { "billingAddressLine2", "" },
                            { "billingCity", "Minneapolis" },
                            { "billingZipCode", "55419" },
                            { "billingTelephone", "3213213211" },

                            { "shippingSameAsBilling", "1" },
                            { "shippingFirstName", "Kevin" },
                            { "shippingLastName", "Test" },
                            { "shippingAddressLine1", "123 Fake Street" },
                            { "shippingAddressLine2", "" },
                            { "shippingCity", "Minneapolis" },
                            { "shippingZipCode", "55419" },
                            { "shippingTelephone", "3213213211" },

                            { "comments", "This order was automated" }
                        })
                        .SetPaymentCountryRegion(new Guid("af697207-d370-4aee-824c-15711d43a9f2"), null)
                        .SetShippingCountryRegion(new Guid("af697207-d370-4aee-824c-15711d43a9f2"), null);

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to update information");
           }

            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetShippingMethod(new Guid("b12dc9ab-fa47-49be-9ee5-bc0d069d6ca6"));

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to set order shipping method");
            }


            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetPaymentMethod(new Guid("e35677ac-a544-45a0-ba4a-a78dd43dbaf2"));

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to set order payment method");
            }


            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow);
                    var orderTOtal = order.TotalPrice.WithPreviousAdjustments.WithTax;

                    order.InitializeTransaction();
                    order.Finalize(orderTOtal, Guid.NewGuid().ToString(), Core.Models.PaymentStatus.Authorized);

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to set order payment method");
            }


        }



        [HttpPost]
        public IActionResult UpdateCart(UpdateCartDto postModel)
        {
            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow);

                    foreach (var orderLine in postModel.OrderLines)
                    {
                        order.WithOrderLine(orderLine.Id)
                            .SetQuantity(orderLine.Quantity);
                    }

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to update cart");

                return CurrentUmbracoPage();
            }

            TempData["cartUpdated"] = "true";

            return RedirectToCurrentUmbracoPage();
        }

        [HttpGet]
        public IActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {
            try
            {
                _vendrApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .RemoveOrderLine(postModel.OrderLineId);

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("productReference", "Failed to remove cart item");

                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }
    }
}
