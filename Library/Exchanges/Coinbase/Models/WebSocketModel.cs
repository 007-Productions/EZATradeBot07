using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZATB07.Library.Exchanges.Coinbase.Models
{
    public class WebSocketModel
    {
        public string channel { get; set; }
        public string client_id { get; set; }
        public string timestamp { get; set; }
        public int sequence_num { get; set; }
        public Event[] events { get; set; }
        
        public class Event
        {
            public string type { get; set; }
            public Order[] orders { get; set; }
            public Positions positions { get; set; }
        }

        public class Positions
        {
            public Perpetual_Futures_Positions[] perpetual_futures_positions { get; set; }
            public Expiring_Futures_Positions[] expiring_futures_positions { get; set; }
        }

        public class Perpetual_Futures_Positions
        {
            public string product_id { get; set; }
            public string portfolio_uuid { get; set; }
            public string vwap { get; set; }
            public string entry_vwap { get; set; }
            public string position_side { get; set; }
            public string margin_type { get; set; }
            public string net_size { get; set; }
            public string buy_order_size { get; set; }
            public string sell_order_size { get; set; }
            public string leverage { get; set; }
            public string mark_price { get; set; }
            public string liquidation_price { get; set; }
            public string im_notional { get; set; }
            public string mm_notional { get; set; }
            public string position_notional { get; set; }
            public string unrealized_pnl { get; set; }
            public string aggregated_pnl { get; set; }
        }

        public class Expiring_Futures_Positions
        {
            public string product_id { get; set; }
            public string side { get; set; }
            public string number_of_contracts { get; set; }
            public string realized_pnl { get; set; }
            public string unrealized_pnl { get; set; }
            public string entry_price { get; set; }
        }

        public class Order
        {
            public string avg_price { get; set; }
            public string cancel_reason { get; set; }
            public string client_order_id { get; set; }
            public string completion_percentage { get; set; }
            public string contract_expiry_type { get; set; }
            public string cumulative_quantity { get; set; }
            public string filled_value { get; set; }
            public string leaves_quantity { get; set; }
            public string limit_price { get; set; }
            public string number_of_fills { get; set; }
            public string order_id { get; set; }
            public string order_side { get; set; }
            public string order_type { get; set; }
            public string outstanding_hold_amount { get; set; }
            public string post_only { get; set; }
            public string product_id { get; set; }
            public string product_type { get; set; }
            public string reject_reason { get; set; }
            public string retail_portfolio_id { get; set; }
            public string risk_managed_by { get; set; }
            public string status { get; set; }
            public string stop_price { get; set; }
            public string time_in_force { get; set; }
            public string total_fees { get; set; }
            public string total_value_after_fees { get; set; }
            public string trigger_status { get; set; }
            public DateTime creation_time { get; set; }
            public DateTime end_time { get; set; }
            public DateTime start_time { get; set; }
        }

    }
}
