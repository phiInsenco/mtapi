using MtApi5;
using Newtonsoft.Json;
using Xunit;

namespace MtApi5.Tests
{
    // Payloads mirror the JSON produced by mq5/MtApi5.mq5:
    //  - GetDealHistories (406): MqlDealToJson
    //  - HistoryPosition  (401): MtHistoryPosition::CreateJson (numbers cast to double serialize as "2.0")
    public class TradeHistoryDeserializationTests
    {
        // Raw MT5 ENUM_DEAL_TYPE values that external consumers compare against
        private const int DealTypeBuy = 0;
        private const int DealTypeSell = 1;
        private const int DealTypeBalance = 2;
        private const int DealTypeCredit = 3;
        private const int DealTypeCommission = 7;

        private const string DealHistoriesJson = @"[
  {""Ticket"":1001,""Order"":2001,""Time"":""2026.06.01 10:00:00"",""TimeMsc"":1780308000000,""DealType"":0,""Entry"":0,""Magic"":0,""Reason"":0,""PositionId"":3001,
   ""Volume"":0.1,""Price"":1.1,""Commission"":-0.5,""Swap"":0,""Profit"":0,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":""EURUSD"",""Comment"":"""",""ExternalId"":"""",""TypeDescription"":""Buy type"",""EntryDescription"":""In entry""},
  {""Ticket"":1002,""Order"":2002,""Time"":""2026.06.01 11:00:00"",""TimeMsc"":1780311600000,""DealType"":1,""Entry"":1,""Magic"":0,""Reason"":0,""PositionId"":3001,
   ""Volume"":0.1,""Price"":1.2,""Commission"":-0.5,""Swap"":0,""Profit"":100,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":""EURUSD"",""Comment"":"""",""ExternalId"":"""",""TypeDescription"":""Sell type"",""EntryDescription"":""Out entry""},
  {""Ticket"":1003,""Order"":0,""Time"":""2026.06.01 08:00:00"",""TimeMsc"":1780300800000,""DealType"":2,""Entry"":0,""Magic"":0,""Reason"":0,""PositionId"":0,
   ""Volume"":0,""Price"":0,""Commission"":0,""Swap"":0,""Profit"":1000,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":"""",""Comment"":""deposit"",""ExternalId"":"""",""TypeDescription"":""Balance type"",""EntryDescription"":""In entry""},
  {""Ticket"":1004,""Order"":0,""Time"":""2026.06.02 08:00:00"",""TimeMsc"":1780387200000,""DealType"":2,""Entry"":0,""Magic"":0,""Reason"":0,""PositionId"":0,
   ""Volume"":0,""Price"":0,""Commission"":0,""Swap"":0,""Profit"":-500,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":"""",""Comment"":""withdrawal"",""ExternalId"":"""",""TypeDescription"":""Balance type"",""EntryDescription"":""In entry""},
  {""Ticket"":1005,""Order"":0,""Time"":""2026.06.02 09:00:00"",""TimeMsc"":1780390800000,""DealType"":3,""Entry"":0,""Magic"":0,""Reason"":0,""PositionId"":0,
   ""Volume"":0,""Price"":0,""Commission"":0,""Swap"":0,""Profit"":200,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":"""",""Comment"":""credit"",""ExternalId"":"""",""TypeDescription"":""Credit type"",""EntryDescription"":""In entry""},
  {""Ticket"":1006,""Order"":0,""Time"":""2026.06.02 10:00:00"",""TimeMsc"":1780394400000,""DealType"":7,""Entry"":0,""Magic"":0,""Reason"":0,""PositionId"":0,
   ""Volume"":0,""Price"":0,""Commission"":0,""Swap"":0,""Profit"":-3,""Fee"":0,""Sl"":0,""Tp"":0,
   ""Symbol"":"""",""Comment"":""commission"",""ExternalId"":"""",""TypeDescription"":""Commission type"",""EntryDescription"":""In entry""}
]";

        private const string HistoryPositionJson = @"[
  {""Ticket"":""3001"",""Symbol"":""EURUSD"",""TypeDescription"":""Buy"",""CloseComment"":"""",""OpenComment"":"""",
   ""TimeOpen"":""2026.06.01 10:00:00"",""TimeClose"":""2026.06.01 11:00:00"",""TimeOpenMsc"":1780308000000.0,""TimeCloseMsc"":1780311600000.0,
   ""PositionType"":0,""RecordKind"":""Position"",""DealType"":0,""Magic"":0,""Identifier"":3001.0,""OpenReason"":0,""CloseReason"":0,
   ""Volume"":0.1,""PriceOpen"":1.1,""StopLoss"":0,""TakeProfit"":0,""PriceClose"":1.2,""Commission"":-1.0,""Swap"":0,""Profit"":100.0,""DealTickets"":""1001,1002""},
  {""Ticket"":""3002"",""Symbol"":""GBPUSD"",""TypeDescription"":""Sell"",""CloseComment"":"""",""OpenComment"":"""",
   ""TimeOpen"":""2026.06.01 12:00:00"",""TimeClose"":""2026.06.01 13:00:00"",""TimeOpenMsc"":1780315200000.0,""TimeCloseMsc"":1780318800000.0,
   ""PositionType"":1.0,""RecordKind"":""Position"",""DealType"":1.0,""Magic"":0,""Identifier"":3002.0,""OpenReason"":0,""CloseReason"":0,
   ""Volume"":0.2,""PriceOpen"":1.3,""StopLoss"":0,""TakeProfit"":0,""PriceClose"":1.29,""Commission"":-1.0,""Swap"":0,""Profit"":20.0,""DealTickets"":""1007,1008""},
  {""Ticket"":""1003"",""Symbol"":"""",""TypeDescription"":""Balance type"",""CloseComment"":""deposit"",""OpenComment"":""deposit"",
   ""TimeOpen"":""2026.06.01 08:00:00"",""TimeClose"":""2026.06.01 08:00:00"",""TimeOpenMsc"":0,""TimeCloseMsc"":0,
   ""PositionType"":0,""RecordKind"":""Balance"",""DealType"":2.0,""Magic"":0,""Identifier"":0,""OpenReason"":0,""CloseReason"":0,
   ""Volume"":0,""PriceOpen"":0,""StopLoss"":0,""TakeProfit"":0,""PriceClose"":0,""Commission"":0,""Swap"":0,""Profit"":1000.0,""DealTickets"":""1003""},
  {""Ticket"":""1004"",""Symbol"":"""",""TypeDescription"":""Balance type"",""CloseComment"":""withdrawal"",""OpenComment"":""withdrawal"",
   ""TimeOpen"":""2026.06.02 08:00:00"",""TimeClose"":""2026.06.02 08:00:00"",""TimeOpenMsc"":0,""TimeCloseMsc"":0,
   ""PositionType"":0,""RecordKind"":""Balance"",""DealType"":2.0,""Magic"":0,""Identifier"":0,""OpenReason"":0,""CloseReason"":0,
   ""Volume"":0,""PriceOpen"":0,""StopLoss"":0,""TakeProfit"":0,""PriceClose"":0,""Commission"":0,""Swap"":0,""Profit"":-500.0,""DealTickets"":""1004""},
  {""Ticket"":""1005"",""Symbol"":"""",""TypeDescription"":""Credit type"",""CloseComment"":""credit"",""OpenComment"":""credit"",
   ""TimeOpen"":""2026.06.02 09:00:00"",""TimeClose"":""2026.06.02 09:00:00"",""TimeOpenMsc"":0,""TimeCloseMsc"":0,
   ""PositionType"":0,""RecordKind"":""Balance"",""DealType"":3.0,""Magic"":0,""Identifier"":0,""OpenReason"":0,""CloseReason"":0,
   ""Volume"":0,""PriceOpen"":0,""StopLoss"":0,""TakeProfit"":0,""PriceClose"":0,""Commission"":0,""Swap"":0,""Profit"":200.0,""DealTickets"":""1005""}
]";

        [Fact]
        public void Deals_CarryRawDealTypeAndDescriptions()
        {
            var deals = JsonConvert.DeserializeObject<List<MT5Deal>>(DealHistoriesJson)!;

            Assert.Equal(
                new[] { DealTypeBuy, DealTypeSell, DealTypeBalance, DealTypeBalance, DealTypeCredit, DealTypeCommission },
                deals.Select(d => d.DealType));
            Assert.Equal(
                new[] { "Buy type", "Sell type", "Balance type", "Balance type", "Credit type", "Commission type" },
                deals.Select(d => d.TypeDescription));
            Assert.All(deals, d => Assert.False(string.IsNullOrEmpty(d.EntryDescription)));
            Assert.Equal("Out entry", deals[1].EntryDescription);
        }

        [Fact]
        public void Deals_TimeIsFormattedString()
        {
            var deals = JsonConvert.DeserializeObject<List<MT5Deal>>(DealHistoriesJson)!;

            Assert.Equal("2026.06.01 10:00:00", deals[0].Time);
            Assert.Equal(1780308000000L, deals[0].TimeMsc);
        }

        [Fact]
        public void Deals_WithdrawalDistinguishableFromDepositBySign()
        {
            var deals = JsonConvert.DeserializeObject<List<MT5Deal>>(DealHistoriesJson)!;
            var balance = deals.Where(d => d.DealType == DealTypeBalance).ToList();

            Assert.Equal(2, balance.Count);
            Assert.Single(balance, d => d.Profit > 0);
            Assert.Single(balance, d => d.Profit < 0);
        }

        [Fact]
        public void HistoryPosition_PositionRowsHaveBuyOrSellPositionType()
        {
            var rows = JsonConvert.DeserializeObject<List<MT5Position>>(HistoryPositionJson)!;
            var positions = rows.Where(r => r.RecordKind == MT5Position.RecordKindPosition).ToList();

            Assert.Equal(2, positions.Count);
            Assert.All(positions, p => Assert.Contains(p.PositionType, new[] { 0, 1 }));
            Assert.All(positions, p => Assert.False(p.IsBalanceRecord));
            Assert.Equal(1, positions[1].PositionType);
        }

        [Fact]
        public void HistoryPosition_BalanceRowsUseRecordKindAndDefaultPositionType()
        {
            var rows = JsonConvert.DeserializeObject<List<MT5Position>>(HistoryPositionJson)!;
            var balance = rows.Where(r => r.IsBalanceRecord).ToList();

            Assert.Equal(3, balance.Count);
            Assert.All(balance, b => Assert.Equal(MT5Position.RecordKindBalance, b.RecordKind));
            Assert.All(balance, b => Assert.Equal(0, b.PositionType));
        }

        [Fact]
        public void HistoryPosition_BalanceRowsCarryRawDealType()
        {
            var rows = JsonConvert.DeserializeObject<List<MT5Position>>(HistoryPositionJson)!;

            Assert.Equal(DealTypeBalance, rows.Single(r => r.Ticket == "1003").DealType);
            Assert.Equal(DealTypeBalance, rows.Single(r => r.Ticket == "1004").DealType);
            Assert.Equal(DealTypeCredit, rows.Single(r => r.Ticket == "1005").DealType);
        }

        [Fact]
        public void HistoryPosition_WithdrawalIsIdentifiable()
        {
            var rows = JsonConvert.DeserializeObject<List<MT5Position>>(HistoryPositionJson)!;

            Assert.Equal(new[] { "1004" }, rows.Where(r => r.IsWithdrawal).Select(r => r.Ticket));
            Assert.False(rows.Single(r => r.Ticket == "1003").IsWithdrawal);
            Assert.False(rows.Single(r => r.Ticket == "1005").IsWithdrawal);
        }
    }
}
