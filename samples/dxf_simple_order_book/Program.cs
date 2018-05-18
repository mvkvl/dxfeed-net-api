using com.dxfeed.api;
using com.dxfeed.api.data;
using com.dxfeed.api.events;
using com.dxfeed.native;
// using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_simple_order_book_sample
{
    public class OrderListener : IDxOrderSnapshotListener
    {
        class Offer
        {
            public com.dxfeed.api.data.Side Side { get; set; }
            public double Price { get; set; }
            public DateTime Timestamp { get; set; }
            public long Size { get; set; }
            public long Sequence { get; set; }
            public string Source { get; set; }
            public string MarketMaker { get; set; }
        }

        public void OnOrderSnapshot<TB, TE>(TB buf)
             where TB : IDxEventBuf<TE>
             where TE : IDxOrder
        {

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{3}: Snapshot {0} {{Symbol: '{1}', RecordsCount: {2}}}", buf.EventType, buf.Symbol, buf.Size, DateTime.Now.ToString("o")));

            var book = new List<Offer>();
            foreach (var o in buf)
            {
                book.Add(new Offer { Side = o.Side, Price = o.Price, Size = o.Size, Timestamp = o.Time, Sequence = o.Sequence, Source = o.Source?.Name, MarketMaker = o.MarketMaker.ToString() });
            }

            Console.Write("Bids:\n");
            var bids = book.Where(o => o.Side == com.dxfeed.api.data.Side.Buy).OrderByDescending(o => o.Price).Take(10);
            foreach (var o in bids)
                Console.WriteLine(o.Price + " " + o.Size);

            Console.WriteLine();

            Console.Write("Asks:\n");
            var asks = book.Where(o => o.Side == com.dxfeed.api.data.Side.Sell).OrderBy(o => o.Price).Take(10);
            foreach (var o in asks)
                Console.WriteLine(o.Price + " " + o.Size);
        }
    }

    class Program
    {
        public void Run()
        {
            var address = "demo.dxfeed.com:7300";
            using (var con = new NativeConnection(address, OnDisconnect))
            {
                IDxSubscription s = null;
                try
                {
                    s = con.CreateSnapshotSubscription(EventType.Order, 0, new OrderListener());
                    s.SetSource(OrderSource.NTV);
                    s.AddSymbol("IBM");
                    Console.WriteLine("Press enter to stop");
                    Console.ReadLine();
                }
                catch (DxException dxException)
                {
                    Console.WriteLine("Native exception occured: " + dxException.Message);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Exception occured: " + exc.Message);
                }
                finally
                {
                    if (s != null)
                        s.Dispose();
                }
            }
        }

        private void OnDisconnect(IDxConnection con)
        {
            Console.WriteLine("Disconnected");
        }

        static void Main(string[] args)
        {
            Console.ReadKey();
            new Program().Run();
        }
    }
}