using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    public sealed class Simulation
    {
        public Simulation(Scenario scenario, IOutput output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        }

        public IOutput Output { get; }

        public Scenario Scenario { get; }

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

        public decimal ExternalCapital { get; set; }

        private void AdjustCash(decimal amount)
        {
            if (amount < 0)
            {
                if (Math.Abs(amount) > Cash)
                    throw new SimulationException($"Withdrawal of {Math.Abs(amount):C0} would overdraw cash balance of {Cash:C0}");
            }

            Cash += amount;
        }

        private void BuyBonds(decimal bondAmount, MonthYear now)
        {
            if (bondAmount <= Math.Max(0, Scenario.MinimumBond))
                return;

            AdjustCash(-bondAmount);

            var rate = TreasuryInterestRates.GetRate(now);
            var bond = new Treasury
            {
                Maturity = now.AddYears(1),
                InitialInterestRate = rate.InterestRate,
                Par = Treasury.GetFutureValue(bondAmount, rate.InterestRate, 1.0m),
                Purchase = bondAmount,
            };
            Debug.Assert(bond.Par > 0);
            Debug.Assert(bond.Par > bond.Purchase);
            Bonds.Add(bond);
            Output.VerboseLine($"Purchased {bond}");
        }

        private void BuyStocks(decimal stockAmount, MonthYear now)
        {
            if (stockAmount <= Math.Max(0, Scenario.MinimumStock))
                return;

            AdjustCash(-stockAmount);

            var price = Sp500Prices.GetPrice(now);
            var stock = new Sp500
            {
                BasisPrice = price.Price,
                Shares = stockAmount / price.Price,
            };
            Stocks.Add(stock);
            Output.VerboseLine($"Purchased {stock}");
        }

        private void CalculateDividends(MonthYear now)
        {
            // We only calculate dividends quarterly.
            if (!(now.Month == 3 || now.Month == 6 || now.Month == 9 || now.Month == 12))
                return;

            // Average dividend this quarter
            var dividend1 = Sp500Dividends.GetDividend(now);
            var dividend2 = Sp500Dividends.GetDividend(now.AddMonths(-1));
            var dividend3 = Sp500Dividends.GetDividend(now.AddMonths(-2));
            var dividendPercentage = (dividend1.DividendPercentage+ dividend2.DividendPercentage+ dividend3.DividendPercentage) / 3;

            decimal dividends = 0;
            foreach (var sp500 in Stocks)
            {
                dividends += CalculateDividends(sp500, now, dividendPercentage * 3 / 12);
            }

            if (dividends > 0)
                Output.VerboseLine($"{dividends:C0} dividends on stocks valued at {GetStockValues(now):C0}; {dividendPercentage:P2} dividend percentage per anum");
        }

        private decimal CalculateDividends(Sp500 sp500, MonthYear now, decimal dividendPercentage)
        {
            var amount = dividendPercentage * sp500.GetValue(now);
            AdjustCash(amount);
            CurrentTaxes.Dividends += amount;
            return amount;
        }

        private void CheckMortgageIsPaid()
        {
            if (Mortgage == null || Mortgage.Balance > 0)
                return;

            Output.VerboseLine("Mortgage is paid off!");
            Mortgage = null;
        }

        private void CloseBooks(MonthYear now)
        {
            Output.VerboseLine("Closing books");

            CheckMortgageIsPaid();
            if (Scenario.ShouldPayOffHouse)
                PayOffHouse(now);

            RedeemBonds(now);
            if (PreviousTaxes != null)
            {
                Output.VerboseLine("Paying previous year taxes");
                PayTaxes(PreviousTaxes, now);
                PreviousTaxes = null;
            }

            if (CurrentTaxes != null)
            {
                Output.VerboseLine("Paying current year taxes");
                PayTaxes(CurrentTaxes, now);
                CurrentTaxes = new Taxes();
            }
        }

        private void EarnIncome(MonthYear now)
        {
            if (Scenario.MonthlyIncome <= 0)
                return;

            Output.VerboseLine($"Monthly income of {Scenario.MonthlyIncome:C0}");
            AdjustCash(Scenario.MonthlyIncome);
            ExternalCapital += Scenario.MonthlyIncome;
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
            text.AppendLine($"Net worth is {GetNetWorth(now):C0}");
            text.AppendLine($"External capital of {ExternalCapital:C0} added");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (Cash > 0)
                text.AppendLine($"Cash on hand is {Cash:C0}");
            if (Mortgage != null)
                text.AppendLine($"{Mortgage}");
            if (!string.IsNullOrWhiteSpace(CurrentTaxes?.ToString()))
            {
                text.AppendLine("Current Year Taxes");
                text.AppendLine($"{CurrentTaxes}");
            }

            if (!string.IsNullOrWhiteSpace(PreviousTaxes?.ToString()))
            {
                text.AppendLine("Previous Year Taxes");
                text.AppendLine($"{PreviousTaxes}");
            }

            if (Bonds.Count > 0)
            {
                text.AppendLine($"{Bonds.Count:N0} bonds with value {GetBondValues(now):C0}");
                foreach (var bond in Bonds)
                {
                    text.AppendLine($"\t{bond}; {bond.GetFaceValue(now):C0} face value; {bond.GetFaceValue(now) - bond.Purchase:C0} gain/loss");
                }
            }

            if (Stocks.Count > 0)
            {
                text.AppendLine($"{Stocks.Count:N0} stocks with value {GetStockValues(now):C0}");
                foreach (var stock in Stocks)
                {
                    text.AppendLine($"\t{stock}; {stock.GetValue(now):C0} value; {stock.GetValue(now) - stock.BasisPrice * stock.Shares:C0} gain/loss");
                }
            }

            return text.ToString().TrimEnd();
        }

        private decimal GetStockValues(MonthYear now)
        {
            return Stocks.Sum(c => c.GetValue(now));
        }

        private void Initialize(MonthYear start)
        {
            Output.VerboseLine("Starting simulation");

            HomeValue = Scenario.HomeValue;
            Cash = Scenario.StartingCash;
            ExternalCapital += Scenario.StartingCash;
            MonthsUntilRebalance = Scenario.RebalanceMonths ?? 0;
            if (Scenario.AvoidMortgage)
            {
                // Buy house straight out
                if (Cash >= HomeValue)
                    AdjustCash(-HomeValue);
                else
                {
                    // Pay what we can
                    TakeOutMortgage(Scenario.HomeValue - Cash, start);
                    AdjustCash(-Cash);
                }
            }
            else
            {
                // Borrow as much as we can.
                TakeOutMortgage(Scenario.HomeValue, start);
            }
        }

        private void Invest(MonthYear now)
        {
            if (Cash <= Math.Max(0, Scenario.MinimumCash))
                return;

            var stockPercentage = Math.Max(0, Math.Min(Scenario.StockPercentage, 100));
            var stockAmount = Math.Floor(stockPercentage * Cash);
            BuyStocks(stockAmount, now);
            var bondAmount = Math.Floor((1 - stockPercentage) * Cash);
            BuyBonds(bondAmount, now);
        }

        private void MortgageInterestDeduction(Taxes taxes, MonthYear now)
        {
            if (!Scenario.AllowMortgageInterestDeduction)
                return;

            var mortgageInterest = taxes.MortgageInterest;
            if (mortgageInterest <= 0)
                return;

            var valueOfDeduction = mortgageInterest * Scenario.MarginalTaxRate;
            if (valueOfDeduction <= 0)
                return;

            AdjustCash(valueOfDeduction);
            Output.VerboseLine($"Claimed a mortgage interest deduction worth {valueOfDeduction:C0}");
        }

        private void PayDownHouse(MonthYear now)
        {
            if (!Scenario.AvoidMortgage)
                return;

            CheckMortgageIsPaid();
            if (Mortgage == null)
                return;
            if (Mortgage.Balance <= 0)
                return;
            if (Cash <= 0)
                return;

            var principal = Math.Min(Mortgage.Balance, Cash);
            AdjustCash(-principal);
            Mortgage.Balance -= principal;
            Output.VerboseLine($"Additional mortgage principal of {principal:C0}; remaining balance of {Mortgage.Balance:C0}");
        }

        private void PayMortgage(MonthYear now)
        {
            CheckMortgageIsPaid();
            if (Mortgage == null || Mortgage.Balance <= 0)
                return;

            if (!ScroungeMoney(Mortgage.Payment, now))
                throw new SimulationFailedException($"Could not make mortgage payment of {Mortgage.Payment:C0} in {now}") { When = new MonthYear(now) };

            AdjustCash(-Mortgage.Payment);

            var interest = Mortgage.Balance * Mortgage.InterestRate / 12;
            CurrentTaxes.MortgageInterest += interest;

            if (interest > Mortgage.Payment)
            {
                var growth = interest - Mortgage.Payment;
                Mortgage.Balance += growth;
                Output.VerboseLine($"Mortgage payment {Mortgage.Payment:C0} with interest of {interest:C0}; balance grew by {growth:C0} to {Mortgage.Balance:C0}");
            }
            else
            {
                var principal = Mortgage.Payment - interest;
                Mortgage.Balance -= principal;
                Output.VerboseLine($"Mortgage payment {Mortgage.Payment:C0} with interest of {interest:C0}; principal payment of {principal:C0} with balance of {Mortgage.Balance:C0}");
            }
        }

        private void PayOffHouse(MonthYear now)
        {
            CheckMortgageIsPaid();
            if (Mortgage == null)
                return;

            if (!ScroungeMoney(Mortgage.Balance, now))
                throw new SimulationFailedException($"Could not find {Mortgage.Balance:C0} to pay off loan in {now}") { When = new MonthYear(now) };

            AdjustCash(-Mortgage.Balance);
            Output.VerboseLine($"Paid off mortgage of {Mortgage.Balance:C0}");

            Mortgage = null;
            CheckMortgageIsPaid();
        }

        private void PayTaxes(decimal amount, MonthYear now)
        {
            if (amount < 0)
                return;

            if (!ScroungeMoney(amount, now))
                throw new SimulationFailedException($"Could not pay taxes of {amount:C0} in {now}") { When = new MonthYear(now) };

            AdjustCash(-amount);
            Output.VerboseLine($"Paid taxes of {amount:C0}");
        }

        private void PayTaxes(Taxes taxes, MonthYear now)
        {
            // No taxes.
            if (taxes == null)
                return;

            MortgageInterestDeduction(taxes, now);

            // Calculate our actual taxes.
            var amount =
                Math.Max(taxes.Dividends, 0) * Scenario.DividendTaxRate +
                Math.Max(taxes.CapitalGains, 0) * Scenario.CapitalGainsTaxRate +
                Math.Max(taxes.TreasuryInterest, 0) * Scenario.TreasuryInterestTaxRate;

            Output.VerboseLine($"Taxes on dividends of {taxes.Dividends:C0}, capital gains of {taxes.CapitalGains:C0}, and treasury interest of {taxes.TreasuryInterest:C0}");
            PayTaxes(amount, now);
        }

        private void PayTaxes(MonthYear now)
        {
            switch (now.Month)
            {
                case 4:
                {
                    // April. Pay previous year's taxes.
                    if (PreviousTaxes != null)
                    {
                        // Actually pay taxes
                        PayTaxes(PreviousTaxes, now);

                        if (CurrentTaxes != null)
                        {
                            // Carry over losses, if any.
                            if (PreviousTaxes.Dividends < 0)
                            {
                                CurrentTaxes.Dividends += PreviousTaxes.Dividends;
                                PreviousTaxes.Dividends = 0;
                            }

                            if (PreviousTaxes.CapitalGains < 0)
                            {
                                CurrentTaxes.CapitalGains += PreviousTaxes.CapitalGains;
                                PreviousTaxes.CapitalGains = 0;
                            }

                            if (PreviousTaxes.TreasuryInterest < 0)
                            {
                                CurrentTaxes.TreasuryInterest += PreviousTaxes.TreasuryInterest;
                                PreviousTaxes.TreasuryInterest = 0;
                            }
                        }
                    }

                    PreviousTaxes = null;
                    break;
                }

                case 12 when PreviousTaxes != null:
                    throw new InvalidOperationException("PreviousTaxes != null");

                case 12:

                    // Set previous year taxes
                    PreviousTaxes = CurrentTaxes;
                    CurrentTaxes = new Taxes();
                    break;
            }
        }

        private void Rebalance(MonthYear now)
        {
            // TODO:
            // Check if we are even re-balancing.
            if (!Scenario.RebalanceMonths.HasValue)
                return;

            MonthsUntilRebalance--;
            if (MonthsUntilRebalance > 0)
                return;

            MonthsUntilRebalance = Scenario.RebalanceMonths.Value;
        }

        private void RedeemBond(Treasury treasury)
        {
            // TODO:
            AdjustCash(treasury.Par);
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

        private void Refinance(MonthYear now)
        {
            // TODO:
            // Check if we should refinance.
            if (!ShouldRefinance(now))
                return;

            var mortgage = Mortgage;
            Mortgage = null;

            TakeOutMortgage(mortgage.Balance, now);
        }

        public decimal Run(MonthYear start)
        {
            Output.VerboseLine(null);
            Output.VerboseLine($"***** {start} ***** ");

            Initialize(start);
            var now = MonthYear.Constrain(start);
            var end = MonthYear.Constrain(now.AddYears(Scenario.SimulationYears));
            while (now < end)
            {
                Simulate(now);
                now = now.AddMonths(1);
            }

            Output.VerboseLine($"***** {now}: Simulation ended");

            CloseBooks(now);

            Output.VerboseLine(GetStatus(now));

            var netWorth = GetNetWorth(now);
            Output.WriteLine($"{start} simulation succeeded with net worth of {GetNetWorth(now):C0} including a gain of {GetNetWorth(now) - ExternalCapital:C0} on {ExternalCapital:C0} committed");

            return netWorth;
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
                AdjustCash(faceValue);
                Bonds.Remove(bond);
                return faceValue;
            }

            var percentage = amount / faceValue;
            Debug.Assert(percentage >= 0 && percentage <= 1m);
            CurrentTaxes.TreasuryInterest = (faceValue - bond.Purchase) * percentage;
            AdjustCash(faceValue * percentage);

            bond.Par *= (1 - percentage);
            bond.Purchase *= (1 - percentage);
            Debug.Assert(bond.Par > 0);
            Debug.Assert(bond.Par > bond.Purchase);
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
                var basis = stock.Shares * stock.BasisPrice;
                CurrentTaxes.CapitalGains = amount - basis;
                AdjustCash(sale);
                Stocks.Remove(stock);
                return sale;
            }

            var shares = amount / price.Price;
            stock.Shares -= shares;
            CurrentTaxes.CapitalGains = shares * (price.Price - stock.BasisPrice);
            AdjustCash(amount);
            return amount;
        }

        private void SellStocks(decimal amount, MonthYear now)
        {
            // TODO:
            while (amount > 0 && Stocks.Any())
            {
                // Take highest price to reduce our taxes.
                var maxPurchasePrice = Stocks.Max(c => c.BasisPrice);
                var stock = Stocks.First(c => c.BasisPrice == maxPurchasePrice);
                amount -= SellStock(stock, amount, now);
            }
        }

        private bool ShouldRefinance(MonthYear now)
        {
            // TODO:
            if (!Scenario.AllowRefinance)
                return false;

            if (Mortgage == null)
                return false;

            var rate = MortgageInterestRates.GetRate(now, Scenario.MortgageTerm);
            if (rate.InterestRate >= Mortgage.InterestRate)
                return false;

            var months = Math.Max(1, Scenario.RefinancePayBackMonths);

            var costOfRefinance = Mortgage.Balance * Scenario.OriginationFee;
            var currentPayment = Mortgage.Payment;

            var newPayment = PaymentCalculator.CalculatePayment(Mortgage.Balance + costOfRefinance, rate.InterestRate, Mortgage.Years);
            if (newPayment >= currentPayment)
                return false;

            return (currentPayment - newPayment) * months > costOfRefinance;
        }

        private void Simulate(MonthYear now)
        {
            Output.VerboseLine(null);
            Output.VerboseLine($"***** {now}");
            EarnIncome(now);
            RedeemBonds(now);
            CalculateDividends(now);
            PayMortgage(now);
            PayDownHouse(now);
            CheckMortgageIsPaid();
            Refinance(now);
            PayTaxes(now);
            Invest(now);
            Rebalance(now);
        }

        private void TakeOutMortgage(decimal amount, MonthYear start)
        {
            if (Mortgage != null)
                throw new InvalidOperationException("Mortgage != null");

            if (amount <= 0)
                return;

            var rate = Scenario.MortgageInterestRate;
            if (!rate.HasValue || rate.Value <= 0)
                rate = MortgageInterestRates.GetRate(start, Scenario.MortgageTerm).InterestRate;

            var origination = amount * Math.Max(0, Scenario.OriginationFee);
            var amountOrigination = amount + origination;
            var years = Scenario.MortgageTerm.GetYears();
            Mortgage = new Mortgage
            {
                Amount = amountOrigination,
                Balance = amountOrigination,
                Years = years,
                InterestRate = rate.Value,
                Payment = PaymentCalculator.CalculatePayment(amountOrigination, rate.Value, years),
            };
            Output.VerboseLine($"Take out {amount:C0} mortgage with {origination:C0} origination");
            Output.VerboseLine($"{Mortgage}");
        }
    }
}
