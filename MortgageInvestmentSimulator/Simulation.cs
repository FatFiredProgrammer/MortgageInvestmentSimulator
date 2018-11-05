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
        public Simulation(Scenario scenario, Strategy strategy, IOutput output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Scenario = new Scenario(scenario);
            Strategy = strategy;
        }

        public IOutput Output { get; }

        public Scenario Scenario { get; }

        public Strategy Strategy { get; }

        /// <summary>
        ///     Gets or sets the cash on hand.
        /// </summary>
        /// <value>The cash.</value>
        public decimal Cash { get; private set; }

        public decimal MonthlyIncome { get; private set; }

        public decimal MortgageDollarMonths { get; private set; }

        public decimal MortgageInterest { get; private set; }

        public decimal MortgageInterestDeductions { get; private set; }

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

        /// <summary>
        ///     Gets or sets the external capital.
        ///     This keeps tracked of how much money we either started with or earned.
        /// </summary>
        /// <value>The external capital.</value>
        public decimal ExternalCapital { get; set; }

        private void AdjustCash(decimal amount, MonthYear now)
        {
            amount = amount.ToDollarCents();
            if (amount < 0)
            {
                if (Math.Abs(amount) > Cash)
                {
                    Debug.WriteLine(GetStatus(now));
                    throw new SimulationException($"Withdrawal of {Math.Abs(amount):C2} would overdraw cash balance of {Cash:C2}");
                }
            }

            Cash += amount;
        }

        private void BuyBonds(decimal bondAmount, MonthYear now)
        {
            if (bondAmount <= Math.Max(0, Scenario.MinimumBond))
                return;

            AdjustCash(-bondAmount, now);

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

            AdjustCash(-stockAmount, now);

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
            var dividendPercentage = (dividend1.DividendPercentage + dividend2.DividendPercentage + dividend3.DividendPercentage) / 3;

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
            AdjustCash(amount, now);
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
            if (Scenario.ShouldPayOffHouseAtCompletion)
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
            var monthlyIncome = MonthlyIncome;
            if (monthlyIncome <= 0)
                return;

            // Continuously changes with this option.
            if (Scenario.MonthlyIncomeStrategy == MonthlyIncomeStrategy.FixedInflationAdjustedMonthly)
                monthlyIncome = Inflation.Adjust(Scenario.MonthlyIncome, MonthYear.BaseLine, now);

            Output.VerboseLine($"Monthly income of {monthlyIncome:C0}");
            AdjustCash(monthlyIncome, now);
            ExternalCapital += monthlyIncome;
        }

        private void FakePayMortgage(int years)
        {
            for (var months = years * 12; months >= 0; --months)
            {
                if (Mortgage == null || Mortgage.Balance <= 0)
                    break;

                var interest = Mortgage.Balance * Mortgage.InterestRate / 12;
                var principal = Mortgage.Payment - interest;
                Mortgage.Balance -= principal;
            }

            if (Mortgage == null || Mortgage.Balance <= 0)
                Mortgage = null;
            if (Mortgage != null)
                Output.VerboseLine($"Fake mortgage of {Mortgage}");
        }

        private decimal GetBondLiquidity(Treasury bond, MonthYear now)
        {
            var faceValue = bond.GetFaceValue(now);
            var net = faceValue - bond.Purchase;
            if (net <= 0)
                return 0;

            return (faceValue - net * Scenario.TreasuryInterestTaxRate).ToDollarCents();
        }

        private decimal GetBondLiquidity(MonthYear now)
        {
            return Bonds.Sum(c => GetBondLiquidity(c, now));
        }

        private decimal GetBondValues(MonthYear now)
        {
            return Bonds.Sum(c => c.GetFaceValue(now)).ToDollarCents();
        }

        private decimal GetHomeValue(MonthYear now)
            => Scenario.ShouldAdjustForInflation ? Inflation.Adjust(Scenario.HomeValue, MonthYear.BaseLine, now) : Scenario.HomeValue;

        private decimal GetNetWorth(MonthYear now)
            => (GetHomeValue(now) + Cash - (Mortgage?.Balance ?? 0m) + GetStockValues(now) + GetBondValues(now)).ToDollarCents();

        public string GetOverview(MonthYear now)
        {
            var text = new StringBuilder();
            text.AppendLine($"{GetNetWorth(now):C0} net worth and {GetNetWorth(now) - ExternalCapital:C0} gain/loss over contributions");
            text.AppendLine($"{Cash:C0} cash");
            text.AppendLine(IsFinanciallySecure(now) ? "Is financially secure" : "Is not financially secure");
            if (Mortgage != null)
                text.AppendLine($"{Mortgage}");
            if (Bonds.Count > 0)
                text.AppendLine($"{Bonds.Count:N0} bonds with value {GetBondValues(now):C0}");
            if (Stocks.Count > 0)
                text.AppendLine($"{Stocks.Count:N0} stocks with value {GetStockValues(now):C0}");
            return text.ToString().TrimEnd();
        }

        public string GetStatus(MonthYear now)
        {
            var text = new StringBuilder();
            text.AppendLine($"Net worth is {GetNetWorth(now):C0}");
            text.AppendLine($"External capital of {ExternalCapital:C0} added");
            text.AppendLine($"Home value is {GetHomeValue(now):C0}");
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

        private decimal GetStockLiquidity(Sp500 stock, MonthYear now)
        {
            // Get current stock value
            var current = stock.GetValue(now);

            // Calculate net gain by subtracting basis from current value
            var net = current - (stock.BasisPrice * stock.Shares);
            if (net <= 0)
                return 0;

            // Return the current value minus the net gain times capital gains tax rate.
            return (current - net * Scenario.CapitalGainsTaxRate);
        }

        private decimal GetStockLiquidity(MonthYear now)
        {
            return Stocks.Sum(c => GetStockLiquidity(c, now));
        }

        private decimal GetStockValues(MonthYear now)
        {
            return Stocks.Sum(c => c.GetValue(now)).ToDollarCents();
        }

        private void Initialize(MonthYear start)
        {
            Output.VerboseLine("Starting simulation");

            Cash = (Scenario.ShouldAdjustForInflation ? Inflation.Adjust(Scenario.StartingCash, MonthYear.BaseLine, start) : Scenario.StartingCash).ToDollarCents();
            ExternalCapital += Scenario.StartingCash;
            MonthsUntilRebalance = Scenario.RebalanceMonths ?? 0;
            InitializeMortgage(start);
            InitializeMonthlyIncome(start);
            if (Scenario.StartingCash <= 0)
            {
                if (Mortgage != null && MonthlyIncome < Mortgage.Payment)
                    throw new SimulationInvalidException($"Monthly income of {MonthlyIncome:C0} is not enough to cover mortgage payment of {Mortgage.Payment:C0}");
            }
        }

        private void InitializeMonthlyIncome(MonthYear start)
        {
            switch (Scenario.MonthlyIncomeStrategy)
            {
                case MonthlyIncomeStrategy.FixedInflationAdjusted:
                    MonthlyIncome = Inflation.Adjust(Scenario.MonthlyIncome, MonthYear.BaseLine, start);
                    Output.VerboseLine($"Monthly income is an inflation adjusted {MonthlyIncome:C0}");
                    break;

                case MonthlyIncomeStrategy.FixedInflationAdjustedMonthly:
                    MonthlyIncome = Inflation.Adjust(Scenario.MonthlyIncome, MonthYear.BaseLine, start);
                    Output.VerboseLine($"Monthly income is an inflation adjusted monthly starting at {MonthlyIncome:C0}");
                    break;

                case MonthlyIncomeStrategy.Fixed:
                    MonthlyIncome = Scenario.MonthlyIncome;
                    Output.VerboseLine($"Monthly income is fixed at {MonthlyIncome:C0}");
                    break;

                case MonthlyIncomeStrategy.Mortgage:

                    // Add a dollar so we don't have some rounding failure.
                    MonthlyIncome = (Mortgage?.Payment ?? Scenario.MonthlyIncome) + 1m;
                    Output.VerboseLine($"Monthly income is equals monthly payment {MonthlyIncome:C0}");
                    break;

                case MonthlyIncomeStrategy.MortgagePlus50Percent:

                    // Add a dollar so we don't have some rounding failure.
                    MonthlyIncome = (Mortgage?.Payment ?? Scenario.MonthlyIncome) * 1.5m + 1m;
                    Output.VerboseLine($"Monthly income is equals 150% monthly payment {MonthlyIncome:C0}");
                    break;

                case MonthlyIncomeStrategy.MortgagePlus25Percent:

                    // Add a dollar so we don't have some rounding failure.
                    MonthlyIncome = (Mortgage?.Payment ?? Scenario.MonthlyIncome) * 1.25m + 1m;
                    Output.VerboseLine($"Monthly income is equals 125% monthly payment {MonthlyIncome:C0}");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            MonthlyIncome = MonthlyIncome.ToDollarCents();
        }

        private void InitializeMortgage(MonthYear start)
        {
            var homeValue = GetHomeValue(start);

            if (Scenario.ExistingLoanYears.HasValue)
            {
                // Assume an existing mortgage that doesn't cost us anything. 
                // It simply exists.
                var years = Scenario.MortgageTerm.GetYears();
                var existingYears = Math.Max(1, Math.Min(Scenario.ExistingLoanYears.Value, years));
                Output.VerboseLine($"Assuming a prior mortgage of {homeValue:C0} starting {existingYears:N0} years ago.");
                Mortgage = TakeOutMortgage(homeValue, start.AddYears(-existingYears));
                FakePayMortgage(existingYears);
            }
            else
            {
                if (Strategy == Strategy.AvoidMortgage)
                {
                    // Buy house straight out
                    if (Cash >= homeValue)
                    {
                        /* Nothing here - pay off later */
                    }
                    else
                    {
                        // Pay what we can
                        Mortgage = TakeOutMortgage(homeValue - Cash, start);
                        AdjustCash(Mortgage?.Proceeds ?? 0m, start);
                    }
                }
                else
                {
                    // Borrow as much as we can.
                    Mortgage = TakeOutMortgage(homeValue, start);
                    AdjustCash(Mortgage?.Proceeds ?? 0m, start);
                }

                AdjustCash(-homeValue, start);
            }
        }

        private void Invest(MonthYear now)
        {
            if (Cash <= Math.Max(0, Scenario.MinimumCash))
                return;

            var cash = Cash;
            var stockPercentage = Math.Max(0, Math.Min(Scenario.StockPercentage, 100));
            var stockAmount = (stockPercentage * cash).ToDollarCents();
            BuyStocks(Math.Min(stockAmount, Cash), now);
            var bondAmount = ((1 - stockPercentage) * cash).ToDollarCents();
            BuyBonds(Math.Min(bondAmount, Cash), now);
        }

        /// <summary>
        ///     Determines whether we are financially secure because we either do
        ///     now or could own our home.
        /// </summary>
        /// <param name="now">The now.</param>
        /// <returns><c>true</c> if is financially secure; otherwise, <c>false</c>.</returns>
        private bool IsFinanciallySecure(MonthYear now)
        {
            if (Mortgage == null)
                return true;
            if (Mortgage.Balance <= 0)
                return true;

            var amount = Mortgage.Balance;
            amount -= Cash;
            amount -= GetBondLiquidity(now);
            amount -= GetStockLiquidity(now);
            return amount <= 0;
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

            AdjustCash(valueOfDeduction, now);
            MortgageInterestDeductions += valueOfDeduction;
            Output.VerboseLine($"Claimed a mortgage interest deduction worth {valueOfDeduction:C0}");
        }

        private void PayDownHouse(MonthYear now)
        {
            CheckMortgageIsPaid();
            if (Mortgage == null)
                return;
            if (Mortgage.Balance <= 0)
                return;
            if (Cash <= 0)
                return;

            var extraPrincipal = (Strategy == Strategy.AvoidMortgage ? Math.Min(Mortgage.Balance, Cash) : Math.Min(Cash, Scenario.ExtraPayment)).ToDollarCents();
            if (extraPrincipal <= 0)
                return;

            AdjustCash(-extraPrincipal, now);
            Mortgage.Balance -= extraPrincipal;
            Output.VerboseLine($"Additional mortgage principal of {extraPrincipal:C0}; remaining balance of {Mortgage.Balance:C0}");
        }

        private void PayMortgage(MonthYear now)
        {
            CheckMortgageIsPaid();
            if (Mortgage == null || Mortgage.Balance <= 0)
                return;

            if (!ScroungeMoney(Mortgage.Payment, now))
            {
                Debug.WriteLine(GetStatus(now));
                throw new SimulationFailedException($"Could not make mortgage payment of {Mortgage.Payment:C0} in {now}") { When = new MonthYear(now) };
            }

            AdjustCash(-Mortgage.Payment, now);

            var interest = Mortgage.Balance * Mortgage.InterestRate / 12;
            CurrentTaxes.MortgageInterest += interest;
            MortgageInterest += interest;
            MortgageDollarMonths += Mortgage.Balance;

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
            {
                Debug.WriteLine(GetStatus(now));
                throw new SimulationFailedException($"Could not find {Mortgage.Balance:C0} to pay off loan in {now}") { When = new MonthYear(now) };
            }

            AdjustCash(-Mortgage.Balance, now);
            Output.VerboseLine($"Paid off mortgage of {Mortgage.Balance:C0}");

            Mortgage = null;
            CheckMortgageIsPaid();
        }

        private void PayTaxes(decimal amount, MonthYear now)
        {
            if (amount < 0)
                return;

            if (!ScroungeMoney(amount, now))
            {
                Debug.WriteLine(GetStatus(now));
                throw new SimulationFailedException($"Could not pay taxes of {amount:C0} in {now}") { When = new MonthYear(now) };
            }

            AdjustCash(-amount, now);
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
            // Check if we are even re-balancing.
            if (!Scenario.RebalanceMonths.HasValue)
                return;

            MonthsUntilRebalance--;
            if (MonthsUntilRebalance > 0)
                return;

            MonthsUntilRebalance = Scenario.RebalanceMonths.Value;

            var bondAmount = GetBondValues(now);
            var stockAmount = GetStockValues(now);
            if (bondAmount + stockAmount <= 0)
                return;

            var totalAmount = bondAmount + stockAmount;
            var bondPercentage = bondAmount / totalAmount;
            var stockPercentage = stockAmount / totalAmount;
            var desiredStockPercentage = Math.Max(0, Math.Min(Scenario.StockPercentage, 100));
            var desiredBondPercentage = 1 - desiredStockPercentage;

            const decimal thresholdPercentage = .01m;

            if (Math.Abs(bondPercentage - desiredBondPercentage) <= thresholdPercentage && Math.Abs(stockPercentage - desiredStockPercentage) <= thresholdPercentage)
                return;

            var desiredBondAmount = (desiredBondPercentage * totalAmount).ToDollarCents();
            var desiredStockAmount = (desiredStockPercentage * totalAmount).ToDollarCents();

            const decimal thresholdAmount = 1000;
            if (Math.Abs(desiredBondAmount - bondAmount) < thresholdAmount || Math.Abs(desiredStockAmount - stockAmount) < thresholdAmount)
                return;

            Output.VerboseLine("Rebalancing");
            if (desiredBondAmount < bondAmount)
                SellBonds(bondAmount - desiredBondAmount, now);
            if (desiredStockAmount < stockAmount)
                SellStocks(stockAmount - desiredStockAmount, now);

            if (desiredBondAmount > bondAmount)
                BuyBonds(Math.Min(desiredBondAmount - bondAmount, Cash), now);
            if (desiredStockAmount > stockAmount)
                BuyStocks(Math.Min(desiredStockAmount - stockAmount, Cash), now);
        }

        private void RedeemBond(Treasury bond, MonthYear now)
        {
            AdjustCash(bond.Par, now);
            CurrentTaxes.TreasuryInterest += bond.Par - bond.Purchase;
            Bonds.Remove(bond);
            Output.VerboseLine($"Redeemed {bond.Par:C0} bond with {bond.Purchase:C0} purchase; {bond.Par - bond.Purchase:C0} gain/loss");
        }

        private void RedeemBonds(MonthYear now)
        {
            var matured = Bonds.Where(c => c.IsMatured(now)).ToList();
            foreach (var treasury in matured)
            {
                RedeemBond(treasury, now);
            }
        }

        private void Refinance(MonthYear now)
        {
            // Check if we should refinance.
            if (!ShouldRefinance(now))
                return;

            var mortgage = Mortgage;
            Mortgage = null;

            var amount = mortgage.Balance;
            if (Strategy == Strategy.Invest && Scenario.CashOutAtRefinance)
            {
                var homeValue = GetHomeValue(now);
                if (homeValue > amount)
                {
                    Output.VerboseLine($"Taking {homeValue - amount:C0} extra cash out at refinance");
                    amount = homeValue;
                }
            }

            Output.VerboseLine($"Refinancing {mortgage}");
            Mortgage = TakeOutMortgage(amount, now);
            AdjustCash(Mortgage?.Proceeds ?? 0m, now);

            // Must remember to pay off the old loan
            AdjustCash(-mortgage?.Balance ?? 0m, now);
        }

        public Result Run(MonthYear start)
        {
            Output.VerboseLine(null);
            Output.VerboseLine($"===== {start} Simulation ===== ");

            var now = MonthYear.Constrain(start);
            var end = MonthYear.Constrain(now.AddYears(Scenario.SimulationYears));
            Initialize(start);

            var result = new Result(start, Outcome.Success)
            {
                Months = MonthYear.MonthDifference(start, end),
            };

            while (now < end)
            {
                result.TotalMonths++;
                Simulate(now);
                if (IsFinanciallySecure(now))
                {
                    result.FinanciallySecureMonths++;
                    if (result.FinanciallySecureMonthYear == null)
                        result.FinanciallySecureMonthYear = new MonthYear(now);
                }

                Output.VerboseLine(GetOverview(now));

                now = now.AddMonths(1);
            }

            if (MortgageDollarMonths > 0)
            {
                result.AverageMortgageInterestRate = (MortgageInterest / (MortgageDollarMonths / 12)).ToPercent();
                if (MortgageInterestDeductions != 0)
                    result.AverageEffectiveMortgageInterestRate = ((MortgageInterest - MortgageInterestDeductions) / (MortgageDollarMonths / 12)).ToPercent();
            }

            Output.VerboseLine($"===== {now}: Simulation ended");

            CloseBooks(now);

            result.Status = GetStatus(now);
            Output.VerboseLine(result.Status);

            result.NetWorth = GetNetWorth(now);
            if (Scenario.ShouldAdjustForInflation)
                result.NetWorth = Inflation.Adjust(result.NetWorth, now, MonthYear.BaseLine);

            Output.WriteLine($"{start} simulation succeeded with net worth of {GetNetWorth(now):C0} including a gain of {GetNetWorth(now) - ExternalCapital:C0} on {ExternalCapital:C0} committed");

            return result;
        }

        private bool ScroungeMoney(decimal amount, MonthYear now)
        {
            if (Cash < amount)
                SellBonds(Math.Ceiling(amount - Cash).ToDollarCents(), now);
            if (Cash < amount)
                SellStocks(Math.Ceiling(amount - Cash).ToDollarCents(), now);

            return Cash >= amount;
        }

        private decimal SellBond(Treasury bond, decimal amount, MonthYear now)
        {
            var faceValue = bond.GetFaceValue(now);
            if (faceValue <= amount)
            {
                var profit = faceValue - bond.Purchase;
                CurrentTaxes.TreasuryInterest = profit;
                AdjustCash(faceValue, now);
                Bonds.Remove(bond);
                Output.VerboseLine($"Sold {bond.Par:C0} bond with {bond.Purchase:C0} purchase; {profit:C0} gain/loss");
                return faceValue;
            }

            var percentage = amount / faceValue;
            Debug.Assert(percentage >= 0 && percentage <= 1m);
            var fractionalProfit = (faceValue - bond.Purchase) * percentage;
            CurrentTaxes.TreasuryInterest = fractionalProfit;
            AdjustCash(faceValue * percentage, now);

            Output.VerboseLine($"Sold {percentage:P2} of {bond.Par:C0} bond with {bond.Purchase:C0} purchase; {fractionalProfit:C0} gain/loss");

            bond.Par *= (1 - percentage);
            bond.Purchase *= (1 - percentage);
            Debug.Assert(bond.Par > 0);
            Debug.Assert(bond.Par > bond.Purchase);
            Output.VerboseLine($"Adjusted {bond}");
            return amount;
        }

        private void SellBonds(decimal amount, MonthYear now)
        {
            while (amount > 0 && Bonds.Any())
            {
                // No tax calc here. Just take the first.
                var bond = Bonds.First();
                amount -= SellBond(bond, amount, now);
            }
        }

        private decimal SellStock(Sp500 stock, decimal amount, MonthYear now)
        {
            var price = Sp500Prices.GetPrice(now);
            if (stock.Shares * price.Price <= amount)
            {
                var sale = stock.Shares * price.Price;
                var basis = stock.Shares * stock.BasisPrice;
                var capitalGain = amount - basis;
                CurrentTaxes.CapitalGains = capitalGain;
                AdjustCash(sale, now);
                Stocks.Remove(stock);
                Output.VerboseLine($"Sold {stock.Shares:N2} shares at {price.Price:C0} for {sale:C0}; {capitalGain:C0} capital gain");
                return sale;
            }

            var shares = amount / price.Price;
            stock.Shares -= shares;
            var partialCapitalGains = shares * (price.Price - stock.BasisPrice);
            CurrentTaxes.CapitalGains = partialCapitalGains;
            AdjustCash(amount, now);
            Output.VerboseLine($"Sold {stock.Shares:N2} shares at {price.Price:C0} for {amount:C0}; {partialCapitalGains:C0} capital gain");
            return amount;
        }

        private void SellStocks(decimal amount, MonthYear now)
        {
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
            if (!Scenario.AllowRefinance)
                return false;

            if (Mortgage == null)
                return false;

            var rate = MortgageInterestRates.GetRate(now, Scenario.MortgageTerm);
            if (rate.InterestRate >= Mortgage.InterestRate)
                return false;

            var months = Math.Max(1, Scenario.RefinancePayBackMonths);

            var currentPayment = Mortgage.Payment;
            var costOfRefinance = Mortgage.Balance * Math.Max(0, Scenario.OriginationFee);

            var newPayment = PaymentCalculator.CalculatePayment(Mortgage.Balance + costOfRefinance, rate.InterestRate, Mortgage.Years);
            if (newPayment >= currentPayment)
                return false;

            return (currentPayment - newPayment) * months > costOfRefinance;
        }

        private void Simulate(MonthYear now)
        {
            Output.VerboseLine(null);
            Output.VerboseLine($"===== {now}");
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

        private Mortgage TakeOutMortgage(decimal amount, MonthYear start)
        {
            if (amount <= 0)
                return null;

            var rate = (start < MonthYear.Min ? Scenario.MortgageInterestRate : MortgageInterestRates.GetRate(start, Scenario.MortgageTerm).InterestRate).ToPercent();

            var origination = amount * Math.Max(0, Scenario.OriginationFee).ToDollarCents();
            var amountOrigination = (amount + origination).ToDollarCents();
            var years = Scenario.MortgageTerm.GetYears();
            var mortgage = new Mortgage
            {
                Amount = amountOrigination,
                Balance = amountOrigination,
                Years = years,
                InterestRate = rate,
                Payment = PaymentCalculator.CalculatePayment(amountOrigination, rate, years),
                Proceeds = amount
            };
            Output.VerboseLine($"Take out {amount:C0} mortgage with {origination:C0} origination");
            Output.VerboseLine($"{mortgage}");
            return mortgage;
        }
    }
}
