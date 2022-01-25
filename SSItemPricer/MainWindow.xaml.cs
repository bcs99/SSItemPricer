using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using SSItemPricer.Annotations;
using SSItemPricer.Models;

namespace SSItemPricer
{
    public partial class MainWindow : Window
    {
        private const int LaborRateItemNumber = 10030791;

        private readonly ViewModel _viewModel;
        private readonly BackgroundWorker _worker;
        private int _remaining;
        private int _last;
        private int _pass = 1;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = (ViewModel)DataContext;
            _viewModel.Version = $" v{typeof(MainWindow).Assembly.GetName().Version}";

            foreach (var item in Mis.FindMany<Item>("SELECT *  FROM tblItem ORDER BY ItemNumber"))
            {
                item.BuyUnitPrice = item.UseBOM ? 0M : item.BuyUnitPrice;
                item.Status = item.ECOStatusID switch
                {
                    1 => "Draft",
                    2 => "Review",
                    3 => "Purchase Hold",
                    4 => "Released",
                    5 => "Discontinued",
                    6 => "Obsolete",
                    _ => "UNKNOWN"
                };
                _viewModel.Items.Add(item);
            }

            _last = _remaining = _viewModel.Items.Count(i => i.Calculated == false);
            StatusBarItem.Content =
                $"{_viewModel.Items.Count} items to be calculated";

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += WorkerOnDoWork;
            _worker.ProgressChanged += OnWorkerOnProgressChanged;
            _worker.RunWorkerCompleted += OnWorkerOnRunWorkerCompleted;
        }

        private void OnWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusBarItem.Content = $"Pass {_pass}: {e.ProgressPercentage}% of {_remaining} items.";
        }

        private void OnWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CancelBtn.Visibility = Visibility.Hidden;
            if (e.Cancelled == true)
                StatusBarItem.Content = "Scan Cancelled";

            else if (e.Error != null)
                StatusBarItem.Content = $"Scan Error: {e.Error.Message}";

            else
            {
                _last = _remaining;
                _remaining = _viewModel.Items.Count(i => i.Calculated == false);
                if (_last == _remaining)
                    StatusBarItem.Content = $"Calculations completed ({_remaining} items could not be calculated).";
                else
                {
                    _pass++;
                    _worker.RunWorkerAsync(_viewModel.Items.ToList());
                }
            }
        }

        private void Calculate_Clicked(object sender, RoutedEventArgs e)
        {
            CancelBtn.Visibility = Visibility.Visible;
            _worker.RunWorkerAsync(_viewModel.Items.ToList());
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e) => _worker.CancelAsync();

        private static void WorkerOnDoWork([CanBeNull] object sender, DoWorkEventArgs e)
        {
            if (sender is not BackgroundWorker worker || e.Argument is not List<Item> items) return;

            var itemsToCalculate = items.Where(i => i.Calculated == false).ToList();

            for (var i = 0; i < itemsToCalculate.Count; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                if (itemsToCalculate[i].Calculated == false)
                {
                    GetCost(items, itemsToCalculate[i]);
                    Thread.Sleep(0);
                }

                worker.ReportProgress((int)(100 * i / (float)itemsToCalculate.Count));
            }
        }

        private static void GetCost(List<Item> items, Item item)
        {
            item.BuyUnitPrice = 0M;

            var vendor = Mis.FindOne<ItemVendor>(
                $"SELECT * FROM tblItemVendor AS I JOIN tblVendor AS V ON (V.VendorID = I.VendorID) WHERE ItemID={item.ItemNumber} AND VendorPriority=1");

            if (vendor == null)
            {
                item.Notes = "Item has no Vendor";
                item.Calculated = true;
                return;
            }

            if (vendor.UseBOM == false)
            {
                item.BuyUnitPrice = vendor.BuyUnitPrice;
                item.Calculated = true;

                return;
            }

            item.UseBOM = true;

            var bomItems =
                Mis.FindMany<BOMItems>(
                    $@"SELECT * FROM tblBOMItems AS B JOIN tblItem AS I ON (B.ItemID = I.ItemNumber) JOIN tblItemVendor AS V ON (V.ItemID=I.ItemNumber AND V.VendorPriority=1) WHERE B.BOMID={item.ItemNumber}");

            foreach (var bomItem in bomItems)
            {
                if (bomItem.UseBOM)
                {
                    var lookup = items.FirstOrDefault(i => i.ItemNumber == bomItem.ItemNumber && i.Calculated);

                    if (lookup == null)
                        return;

                    if (item.Discontinued == false && bomItem.Discontinued)
                        item.Notes = "Item BOM contains discontinued item(s)";
                }

                if (bomItem.ItemNumber == LaborRateItemNumber)
                    item.LaborCost = bomItem.BuyUnitPrice * bomItem.ItemQuantity;

                item.BuyUnitPrice += bomItem.BuyUnitPrice * bomItem.ItemQuantity;
            }

            item.Calculated = true;
        }
    }
}