using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    public sealed class Simulation
    {
        public Simulation(IOutput output)
            => Output = output ?? throw new ArgumentNullException(nameof(output));

        public IOutput Output { get; }

        /// <summary>
        ///     Gets or sets the cost of the home we have.
        ///     Down payment/etc are not included since it doesn't really matter.
        /// </summary>
        /// <value>The home cost.</value>
        public decimal HomeValue { get; private set; }

        /// <summary>
        ///     Gets or sets the cash on hand.
        /// </summary>
        /// <value>The cash.</value>
        public decimal Cash { get; private set; }

        /// <summary>
        ///     Gets or sets the mortgage if we have one..
        /// </summary>
        /// <value>The mortgage.</value>
        public Mortgage Mortgage { get; private set; }

        /// <summary>
        ///     Gets or sets the current year's taxes.
        /// </summary>
        /// <value>The current taxes.</value>
        public Taxes CurrentTaxes { get; private set; } = new Taxes();

        /// <summary>
        ///     Gets or sets the current previous year's taxes.
        /// </summary>
        /// <value>The current taxes.</value>
        public Taxes PreviousTaxes { get; private set; }

        /// <summary>
        ///     Gets the bonds we own.
        /// </summary>
        /// <value>The bonds.</value>
        public List<Treasury> Bonds { get; } = new List<Treasury>();

        /// <summary>
        ///     Gets the stocks we own.
        /// </summary>
        /// <value>The stocks.</value>
        public List<Sp500> Stocks { get; } = new List<Sp500>();

        /// <summary>
        ///     Gets or sets the months until rebalance.
        /// </summary>
        /// <value>The months until rebalance.</value>
        public int MonthsUntilRebalance { set; private get; }

        private void BuyBonds(decimal bondAmount, MonthYear now)
        {
            // TODO:
            if (bondAmount <= 0)
                return;

            if (bondAmount > Cash)
                throw new SimulationException($"Only {Cash:C0} to buy {bondAmount:C0} bonds");

            var rate = TreasuryInterestRates.GetRate(now);
            var treasury = new Treasury
            {
                Maturity = now.AddYears(1),
                Par = Treasury.GetFutureValue(bondAmount, rate.InterestRate, 1.0m),
                Purchase = bondAmount,
            };
            Bonds.Add(treasury);

            Cash -= bondAmount;
        }

        private void BuyStocks(decimal stockAmount, MonthYear now)
        {
            // TODO:
            if (stockAmount <= 0)
                return;

            if (stockAmount > Cash)
                throw new SimulationException($"Only {Cash:C0} to buy {stockAmount:C0} stocks");

            var price = Sp500Prices.GetPrice(now);
            var sp500 = new Sp500
            {
                PurchasePrice = price.Price,
                Shares = stockAmount / price.Price,
            };
            Stocks.Add(sp500);
            Cash -= stockAmount;
        }

        private void CalculateDividends(MonthYear now)
        {
            // TODO:
            foreach (var sp500 in Stocks)
            {
                CalculateDividends(sp500, now);
            }
        }

        private void CalculateDividends(Sp500 sp500, MonthYear now)
        {
            // TODO:
            var dividend = Sp500Dividends.GetDividend(now);
            var price = Sp500Prices.GetPrice(now);

            var amount = (dividend.DividendPercentage / 12) * sp500.Shares * price.Price;
            Cash += amount;
            CurrentTaxes.Dividends += amount;
        }

        private void CloseBooks(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (scenario.ShouldPayOffHouse)
                PayOffHouse(now);

            if (PreviousTaxes != null)
            {
                PayTaxes(PreviousTaxes, scenario, now);
                PreviousTaxes = null;
            }

            if (CurrentTaxes != null)
            {
                PayTaxes(CurrentTaxes, scenario, now);
                CurrentTaxes = new Taxes();
            }
        }

        private void EarnIncome(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (scenario.MonthlyIncome <= 0)
                return;

            Output.VerboseLine($"Monthly income of {scenario.MonthlyIncome:C0} for {now}");
            Cash += scenario.MonthlyIncome;
        }

        private decimal GetBondValues(MonthYear now)
        {
            return Bonds.Sum(c => c.GetFaceValue(now));
        }

        public decimal GetNetWorth(MonthYear now)
            => HomeValue + Cash - (Mortgage?.Balance ?? 0m) + GetStockValues(now) + GetBondValues(now);

        public string GetStatus(MonthYear now)
        {
            var text = new StringBuilder();
            text.AppendLine($"Net worth is {GetNetWorth(now):C0} as of {now}");

            // TODO: Code needs work
#if false
        public override string ToString()
        {
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (Cash > 0)
                text.AppendLine($"Cash on hand is {Cash:C0}");

            // TODO: Code needs work
#if false
                 if (MortgageAmount > 0)
                text.AppendLine($"{MortgageYears} year loan for {MortgageAmount:C0} @ {MortgageInterestRate:P2}");
            if (MonthlyPayment > 0)
                text.AppendLine($"Monthly mortgage payment is {MonthlyPayment:C0}");
            if (MortgageBalance > 0)
                text.AppendLine($"Mortgage balance is {MortgageBalance:C0}"); 
#endif

            if (CurrentTaxes != null)
            {
                text.AppendLine("Current Year Taxes");
                text.AppendLine(CurrentTaxes.ToString());
            }
            if (PreviousTaxes != null)
            {
                text.AppendLine("Previous Year Taxes");
                text.AppendLine(PreviousTaxes.ToString());
            }
        }

#endif
            return text.ToString().TrimEnd();
        }

        private decimal GetStockValues(MonthYear now)
        {
            return Stocks.Sum(c => c.GetValue(now));
        }

        private void Initialize(Scenario scenario, MonthYear start)
        {
            Output.VerboseLine($"Starting simulation in {start}");

            HomeValue = scenario.HomeValue;
            Cash = scenario.StartingCash;
            MonthsUntilRebalance = scenario.RebalanceMonths ?? 0;
            if (scenario.AvoidMortgage)
            {
                // Buy house straight out
                if (Cash >= HomeValue)
                    Cash -= HomeValue;
                else
                {
                    // Pay what we can
                    TakeOutMortgage(scenario.HomeValue - Cash, scenario, start);
                    Cash = 0;
                }
            }
            else
            {
                // Borrow as much as we can.
                TakeOutMortgage(scenario.HomeValue, scenario, start);
            }

            Output.VerboseLine(GetStatus(start));
        }

        private void Invest(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (Cash <= 0)
                return;

            var stockAmount = Math.Floor(scenario.StockPercentage * Cash);
            BuyStocks(stockAmount, now);
            var bondAmount = Math.Floor((1 - scenario.StockPercentage) * Cash);
            BuyBonds(bondAmount, now);
        }

        private void MortgageInterestDeduction(Taxes taxes, Scenario scenario, MonthYear now)
        {
            if (!scenario.AllowMortgageInterestDeduction)
                return;

            var mortgageInterest = taxes.MortgageInterest;
            if (mortgageInterest <= 0)
                return;

            var valueOfDeduction = mortgageInterest * scenario.MarginalTaxRate;
            if (valueOfDeduction <= 0)
                return;

            Cash += valueOfDeduction;
            Output.WriteLine($"Claimed a mortgage interest deduction worth {valueOfDeduction:C0}");
        }

        private void PayDownHouse(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (!scenario.AvoidMortgage)
                return;

            if (Mortgage == null)
                return;
            if (Mortgage.Balance <= 0)
                return;
            if (Cash <= 0)
                return;

            var principal = Math.Min(Mortgage.Balance, Cash);
            Cash -= principal;
            Mortgage.Balance -= principal;
            Output.VerboseLine($"Addition mortgage principal of {principal:C0}; remaining balance of {Mortgage.Balance:C0}");
        }

        private void PayMortgage(MonthYear now)
        {
            // TODO:
            if (Mortgage.Balance <= 0)
                return;

            if (!ScroungeMoney(Mortgage.Payment, now))
                throw new SimulationFailedException($"Could not make mortgage payment of {Mortgage.Payment:C0} in {now}");

            Cash -= Mortgage.Payment;

            var interest = Mortgage.Balance * Mortgage.InterestRate / 12;
            CurrentTaxes.MortgageInterest += interest;

            if (interest > Mortgage.Payment)
            {
                var growth = interest - Mortgage.Payment;
                Mortgage.Balance += growth;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; balance grew by {growth:C0} to {Mortgage.Balance:C0}");
            }
            else
            {
                var principal = Mortgage.Payment - interest;
                Mortgage.Balance -= principal;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; principal payment of {principal:C0} with balance of {Mortgage.Balance:C0}");
            }
        }

        private void PayOffHouse(MonthYear now)
        {
            // TODO: Code needs work
#if false
                 if(MortgageBalance == null)
            if(MortgageBalance <= 0)
            if (!ScroungeMoney(amount))
                throw new SimulationFailedException($"Could not find {MortgageBalance:C0} to pay off loan in {now}");
#endif
        }

        private void PayTaxes(decimal amount, MonthYear now)
        {
            // TODO:
            if (!ScroungeMoney(amount, now))
                throw new SimulationFailedException($"Could not pay taxes of {amount:C0} in {now}");

            Cash -= amount;
        }

        private void PayTaxes(Taxes taxes, Scenario scenario, MonthYear now)
        {
            // TODO:
            if (taxes == null)
                throw new ArgumentNullException(nameof(taxes));
            if (scenario == null)
                throw new ArgumentNullException(nameof(scenario));

            MortgageInterestDeduction(taxes, scenario, now);

            var amount =
                taxes.Dividends * scenario.DividendTaxRate +
                taxes.CapitalGains * scenario.CapitalGainsTaxRate +
                taxes.TreasuryInterest * scenario.TreasuryInterestTaxRate;

            PayTaxes(amount, now);

            // TODO: Code needs work
        }

        private void PayTaxes(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (now.Month == 4)
            {
                // April. Pay previous year's taxes.
                var previousTaxes = PreviousTaxes;
                PayTaxes(previousTaxes, scenario, now);
                PreviousTaxes = null;
            }

            if (now.Month == 12)
            {
            }
        }

        private void Rebalance(Scenario scenario, MonthYear now)
        {
            // TODO:
            // Check if we are even re-balancing.
            if (!scenario.RebalanceMonths.HasValue)
                return;

            MonthsUntilRebalance--;
            if (MonthsUntilRebalance > 0)
                return;

            MonthsUntilRebalance = scenario.RebalanceMonths.Value;
        }

        private void RedeemBond(Treasury treasury)
        {
            // TODO:
            Cash += treasury.Par;
            CurrentTaxes.TreasuryInterest += treasury.Par - treasury.Purchase;
            Bonds.Remove(treasury);
        }

        private void RedeemBonds(MonthYear now)
        {
            // TODO:
            var matured = Bonds.Where(c => c.IsMatured(now)).ToList();
            foreach (var treasury in matured)
            {
                RedeemBond(treasury);
            }
        }

        private void Refinance(Scenario scenario, MonthYear now)
        {
            // TODO:
            // Check if we should refinance.
            if (!ShouldRefinance(scenario, now))
                return;

            var mortgage = Mortgage;
            Mortgage = null;

            TakeOutMortgage(mortgage.Balance, scenario, now);
        }

        public void Run(Scenario scenario, MonthYear start)
        {
            // TODO:
            Initialize(scenario, start);
            var now = new MonthYear(start);
            var end = new MonthYear(now).AddYears(scenario.SimulationYears);
            while (now <= end)
            {
                Simulate(scenario, now);
                now = now.AddMonths(1);
            }

            CloseBooks(scenario, now);

            Output.VerboseLine(GetStatus(now));
            Output.WriteLine($"{start} simulation succeeded with net worth of {GetNetWorth(now)}");
        }

        private bool ScroungeMoney(decimal amount, MonthYear now)
        {
            // TODO:
            if (Cash < amount)
                SellBonds(Math.Ceiling(amount - Cash), now);
            if (Cash < amount)
                SellStocks(Math.Ceiling(amount - Cash), now);

            return Cash >= amount;
        }

        private decimal SellBond(Treasury bond, decimal amount, MonthYear now)
        {
            // TODO:
            var faceValue = bond.GetFaceValue(now);
            if (faceValue <= amount)
            {
                CurrentTaxes.TreasuryInterest = faceValue - bond.Purchase;
                Cash += faceValue;
                Bonds.Remove(bond);
                return faceValue;
            }

            var percentage = faceValue / amount;
            CurrentTaxes.TreasuryInterest = (faceValue - bond.Purchase) * percentage;
            Cash += faceValue * percentage;

            bond.Par *= (1 - percentage);
            bond.Purchase *= (1 - percentage);
            return amount;
        }

        private void SellBonds(decimal amount, MonthYear now)
        {
            // TODO:
            while (amount > 0 && Bonds.Any())
            {
                // No tax calc here. Just take the first.
                var bond = Bonds.First();
                amount -= SellBond(bond, amount, now);
            }
        }

        private decimal SellStock(Sp500 stock, decimal amount, MonthYear now)
        {
            // TODO:
            var price = Sp500Prices.GetPrice(now);
            if (stock.Shares * price.Price <= amount)
            {
                var sale = stock.Shares * price.Price;
                var basis = stock.Shares * stock.PurchasePrice;
                CurrentTaxes.CapitalGains = amount - basis;
                Cash += sale;
                Stocks.Remove(stock);
                return sale;
            }

            var shares = amount / price.Price;
            stock.Shares -= shares;
            CurrentTaxes.CapitalGains = shares * (price.Price - stock.PurchasePrice);
            Cash += amount;
            return amount;
        }

        private void SellStocks(decimal amount, MonthYear now)
        {
            // TODO:
            while (amount > 0 && Stocks.Any())
            {
                // Take highest price to reduce our taxes.
                var maxPurchasePrice = Stocks.Max(c => c.PurchasePrice);
                var stock = Stocks.First(c => c.PurchasePrice == maxPurchasePrice);
                amount -= SellStock(stock, amount, now);
            }
        }

        private bool ShouldRefinance(Scenario scenario, MonthYear now)
        {
            // TODO:
            if (!scenario.AllowRefinance)
                return false;

            if (Mortgage == null)
                return false;

            var rate = MortgageInterestRates.GetRate(now, scenario.MortgageTerm);
            if (rate.InterestRate >= Mortgage.InterestRate)
                return false;

            var months = Math.Max(1, scenario.RefinancePayBackMonths);

            var costOfRefinance = Mortgage.Balance * scenario.OriginationFee;
            var currentPayment = Mortgage.Payment;

            var newPayment = PaymentCalculator.CalculatePayment(Mortgage.Balance + costOfRefinance, rate.InterestRate, Mortgage.Years);
            if (newPayment >= currentPayment)
                return false;

            return (currentPayment - newPayment) * months > costOfRefinance;
        }

        private void Simulate(Scenario scenario, MonthYear now)
        {
            // TODO:
            Output.VerboseLine($"Simulating {now}");
            EarnIncome(scenario, now);
            RedeemBonds(now);
            CalculateDividends(now);
            PayMortgage(now);
            Refinance(scenario, now);
            PayDownHouse(scenario, now);
            PayTaxes(scenario, now);
            Invest(scenario, now);
            Rebalance(scenario, now);
        }

        private void TakeOutMortgage(decimal amount, Scenario scenario, MonthYear start)
        {
            if (Mortgage != null)
                throw new InvalidOperationException("Mortgage != null");

            if (amount <= 0)
                return;

            var rate = scenario.MortgageInterestRate;
            if (!rate.HasValue || rate.Value <= 0)
                rate = MortgageInterestRates.GetRate(start, scenario.MortgageTerm).InterestRate;

            amount *= 1 + Math.Max(0, scenario.OriginationFee);
            var years = scenario.MortgageTerm.GetYears();
            Mortgage = new Mortgage
            {
                Amount = amount,
                Balance = amount,
                Years = years,
                InterestRate = rate.Value,
                Payment = PaymentCalculator.CalculatePayment(amount, rate.Value, years),
            };
            Output.VerboseLine($"Take out mortgage on {start}");
            Output.VerboseLine($"{Mortgage}");
        }
    }
}
