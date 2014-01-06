using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Windows;

namespace Ombro.ViewModels
{
    public class BuyMeABeerViewModel : Screen
    {
        // PurchaseItems
        private ObservableCollection<ProductListing> _purchases = new ObservableCollection<ProductListing>();

        public BuyMeABeerViewModel()
        {
            SetupMockIAP();
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            if (_purchases.Count == 0)
            {
                try
                {
                    ListingInformation li = await CurrentApp.LoadListingInformationAsync();

                    foreach (var p in li.ProductListings.Where(l => l.Value.Keywords.Contains("Thanks")).OrderBy(l => l.Value.Tag))
                    {
                        _purchases.Add(p.Value);
                    }
                }
                catch(Exception)
                {
                    MessageBox.Show("We tried our best, but we must be out of drinks.  Try checking back another time.  Thanks!", "Dang!", MessageBoxButton.OK);
                }
            }
        }

        public ObservableCollection<ProductListing> PurchaseItems
        {
            get { return _purchases; }
            set
            {
                _purchases = value;
                NotifyOfPropertyChange(() => PurchaseItems);
            }
        }

        public async void BuyMeClickAction(ProductListing product)
        {
            if (product == null)
            {
                return;
            }
            
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(product.ProductId, false);

                ProductLicense productLicense;
                if (CurrentApp.LicenseInformation.ProductLicenses.TryGetValue(product.ProductId, out productLicense))
                {
                    if (productLicense.IsActive)
                    {
                        CurrentApp.ReportProductFulfillment(product.ProductId);
                        MessageBox.Show("I shall raise a glass in your honor!", "Thanks!", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                // the transaction failed - let's just move on.
            }
        }


        private void SetupMockIAP()
        {
#if DEBUG
            MockIAP.Init();

            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "A description", "1", "TestApp");

            // Add some more items manually.
            ProductListing p = new ProductListing
            {
                Name = "Thanks99",
                ImageUri = new Uri("/Assets/Drinks/Thanks99_64x64.png", UriKind.Relative),
                ProductId = "Thanks99",
                ProductType = Windows.ApplicationModel.Store.ProductType.Consumable,
                Keywords = new string[] { "Thanks" },
                Description = "Are you enjoying Ombro Rain Gauge? Let me know by buying me a soda.",
                FormattedPrice = "0.99",
                Tag = "Buy me a soft drink!"
            };
            MockIAP.AddProductListing("Thanks99", p);

            p = new ProductListing
            {
                Name = "Thanks349",
                ImageUri = new Uri("/Assets/Drinks/Thanks349_64x64.png", UriKind.Relative),
                ProductId = "Thanks349",
                ProductType = Windows.ApplicationModel.Store.ProductType.Consumable,
                Keywords = new string[] { "Thanks" },
                Description = "Do you enjoy Ombro Rain Gauge? Let me know by buying me a beer.",
                FormattedPrice = "3.49",
                Tag = "Buy me a beer!"
            };
            MockIAP.AddProductListing("Thanks349", p);

            p = new ProductListing
            {
                Name = "Thanks599",
                ImageUri = new Uri("/Assets/Drinks/Thanks599_64x64.png", UriKind.Relative),
                ProductId = "Thanks599",
                ProductType = Windows.ApplicationModel.Store.ProductType.Consumable,
                Keywords = new string[] { "Thanks" },
                Description = "Do you enjoy using Ombro Rain Gauge? Say thanks by buying me a good beer!",
                FormattedPrice = "5.99",
                Tag = "Buy me a good beer!"
            };
            MockIAP.AddProductListing("Thanks599", p);
#endif
        }
    }
}
