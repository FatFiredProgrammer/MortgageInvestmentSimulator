using System;
using System.Linq;

namespace MortgageInvestmentSimulator
{
    public sealed class Simulation
    {
        public Simulation(IOutput output)
            => Output = output ?? throw new ArgumentNullException(nameof(output));

        public IOutput Output { get; }

        private Financials _financials;

        private void BuyBonds(decimal bondAmount, MonthYear now)
        {
            if (bondAmount <= 0)
                return;

            if (bondAmount > _financials.Cash)
                throw new SimulationException($"Only {_financials.Cash:C0} to buy {bondAmount:C0} bonds");

            var rate = TreasuryInterestRates.GetRate(now);
            var treasury = new Treasury
            {
                Maturity = now.AddYears(1),
                Par = Treasury.GetFutureValue(bondAmount, rate.InterestRate, 1.0m),
                Purchase = bondAmount,
            };
            _financials.Bonds.Add(treasury);

            _financials.Cash -= bondAmount;
        }

        private void BuyStocks(decimal stockAmount, MonthYear now)
        {
            if (stockAmount <= 0)
                return;

            if (stockAmount > _financials.Cash)
                throw new SimulationException($"Only {_financials.Cash:C0} to buy {stockAmount:C0} stocks");

            var price = Sp500Prices.GetPrice(now);
            var sp500 = new Sp500
            {
                PurchasePrice = price.Price,
                Shares = stockAmount / price.Price,
            };
            _financials.Stocks.Add(sp500);
            _financials.Cash -= stockAmount;
        }

        private void CalculateDividends(MonthYear now)
        {
            foreach (var sp500 in _financials.Stocks)
            {
                CalculateDividends(sp500, now);
            }
        }

        private void CalculateDividends(Sp500 sp500, MonthYear now)
        {
            var dividend = Sp500Dividends.GetDividend(now);
            var price = Sp500Prices.GetPrice(now);

            var amount = (dividend.DividendPercentage / 12) * sp500.Shares * price.Price;
            _financials.Cash += amount;
            _financials.CurrentTaxes.Dividends += amount;
        }

        private void CloseBooks(Scenario scenario, MonthYear now)
        {
            if (scenario.ShouldPayOffHouse)
                PayOffHouse(now);

            if (_financials.PreviousTaxes != null)
            {
                PayTaxes(_financials.PreviousTaxes, scenario, now);
                _financials.PreviousTaxes = null;
            }

            if (_financials.CurrentTaxes != null)
            {
                PayTaxes(_financials.CurrentTaxes, scenario, now);
                _financials.CurrentTaxes = new Taxes();
            }
        }

        private void EarnIncome(Scenario scenario, MonthYear now)
        {
            if (scenario.MonthlyIncome <= 0)
                return;

            Output.VerboseLine($"Monthly income of {scenario.MonthlyIncome:C0} for {now}");
            _financials.Cash += scenario.MonthlyIncome;
        }

        private void Initialize(Scenario scenario, MonthYear start)
        {
            _financials = new Financials
            {
                HomeValue = scenario.HomeValue,
                Cash = scenario.StartingCash,
                MonthsUntilRebalance = scenario.RebalanceMonths ?? 0,
            };
            if (scenario.AvoidMortgage)
            {
                if (_financials.Cash >= _financials.HomeValue)
                    _financials.Cash -= _financials.HomeValue;
                else
                {
                    TakeOutMortgage(scenario.HomeValue - _financials.Cash, scenario, start);
                    _financials.Cash = 0;
                }
            }
            else
                TakeOutMortgage(scenario.HomeValue, scenario, start);

            Output.VerboseLine(_financials?.ToString());
        }

        private void Invest(Scenario scenario, MonthYear now)
        {
            if (_financials.Cash <= 0)
                return;

            var stockAmount = Math.Floor(scenario.StockPercentage * _financials.Cash);
            BuyStocks(stockAmount, now);
            var bondAmount = Math.Floor((1 - scenario.StockPercentage) * _financials.Cash);
            BuyBonds(bondAmount, now);
        }

        private void MortgageInterestDeduction(Taxes taxes, Scenario scenario, MonthYear now)
        {
            if (!scenario.AllowMortgageInterestDeduction)
                return;

            var mortgageInterest = taxes.MortgageInterest;
            if(mortgageInterest <= 0)
                return;

            var valueOfDeduction = mortgageInterest * scenario.MarginalTaxRate;
            _financials.Cash += valueOfDeduction;
        }

        private void PayDownHouse(Scenario scenario, MonthYear now)
        {
            if (!scenario.AvoidMortgage)
                return;

            if (_financials.Cash <= 0)
                return;
            if (_financials.Mortgage.Balance <= 0)
                return;

            var principal = Math.Min(_financials.Mortgage.Balance, _financials.Cash);
            _financials.Cash -= principal;
            _financials.Mortgage.Balance -= principal;
            Output.VerboseLine($"Addition mortgage principal of {principal:C0}; remaining balance of {_financials.Mortgage.Balance:C0}");
        }

        private void PayMortgage(MonthYear now)
        {
            if (_financials.Mortgage.Balance <= 0)
                return;

            if (!ScroungeMoney(_financials.Mortgage.Payment, now))
                throw new SimulationFailedException($"Could not make mortgage payment of {_financials.Mortgage.Payment:C0} in {now}");

            _financials.Cash -= _financials.Mortgage.Payment;

            var interest = _financials.Mortgage.Balance * _financials.Mortgage.InterestRate / 12;
            _financials.CurrentTaxes.MortgageInterest += interest;

            if (interest > _financials.Mortgage.Payment)
            {
                var growth = interest - _financials.Mortgage.Payment;
                _financials.Mortgage.Balance += growth;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; balance grew by {growth:C0} to {_financials.Mortgage.Balance:C0}");
            }
            else
            {
                var principal = _financials.Mortgage.Payment - interest;
                _financials.Mortgage.Balance -= principal;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; principal payment of {principal:C0} with balance of {_financials.Mortgage.Balance:C0}");
            }
        }

        private void PayOffHouse(MonthYear now)
        {
            // TODO: Code needs work
#if false
                 if(_financials.MortgageBalance == null)
            if(_financials.MortgageBalance <= 0)
            if (!ScroungeMoney(amount))
                throw new SimulationFailedException($"Could not find {_financials.MortgageBalance:C0} to pay off loan in {now}"); 
#endif
        }

        private void PayTaxes(decimal amount, MonthYear now)
        {
            if (!ScroungeMoney(amount, now))
                throw new SimulationFailedException($"Could not pay taxes of {amount:C0} in {now}");
        }

        private void PayTaxes(Taxes taxes, Scenario scenario, MonthYear now)
        {
            MortgageInterestDeduction(taxes, scenario, now);

            // TODO: Code needs work
#if false

/// <summary>
/// Gets or sets the marginal tax rate.
/// This is used to calculate the value of the mortgage interest deduction
/// </summary>
/// <value>The marginal tax rate.</value>
        public decimal MarginalTaxRate { get; set; } = .38m;

        /// <summary>
        /// Gets or sets a value indicating whether mortgage interest deduction is used.
        /// </summary>
        /// <value><c>true</c> if mortgage interest deduction; otherwise, <c>false</c>.</value>
        public bool MortgageInterestDeduction { get; set; }



        /// <summary>
        /// Gets or sets the dividend tax rate.
        /// </summary>
        /// <value>The dividend tax rate.</value>
        public decimal DividendTaxRate { get; set; } = .15m;

        /// <summary>
        /// Gets or sets the capital gains tax rate.
        /// </summary>
        /// <value>The capital gains tax rate.</value>
        public decimal CapitalGainsTaxRate { get; set; } = .15m;

        /// <summary>
        /// Gets or sets the treasury interest tax rate.
        /// Normally, this is only the fed rate. Not the state rate.
        /// </summary>
        /// <value>The treasury interest tax rate.</value>
        public decimal TreasuryInterestTaxRate { get; set; } = .32m; 
#endif
        }

        private void PayTaxes(Scenario scenario, MonthYear now)
        {
            if (now.Month == 4)
            {
                // April. Pay previous year's taxes.
                var previousTaxes = _financials.PreviousTaxes;
                PayTaxes(previousTaxes, scenario, now);
                _financials.PreviousTaxes = null;
            }

            if (now.Month == 12)
            {
            }
        }

        private void Rebalance(Scenario scenario, MonthYear now)
        {
            // Check if we are even re-balancing.
            if (!scenario.RebalanceMonths.HasValue)
                return;

            _financials.MonthsUntilRebalance--;
            if (_financials.MonthsUntilRebalance > 0)
                return;

            _financials.MonthsUntilRebalance = scenario.RebalanceMonths.Value;
        }

        private void RedeemBond(Treasury treasury)
        {
            _financials.Cash += treasury.Par;
            _financials.CurrentTaxes.TreasuryInterest += treasury.Par - treasury.Purchase;
            _financials.Bonds.Remove(treasury);
        }

        private void RedeemBonds(MonthYear now)
        {
            var matured = _financials.Bonds.Where(c => c.IsMature(now)).ToList();
            foreach (var treasury in matured)
            {
                RedeemBond(treasury);
            }
        }

        private void Refinance(Scenario scenario, MonthYear now)
        {
            // Check if we should refinance.
            if (!ShouldRefinance(scenario, now))
                return;

            var mortgage = _financials.Mortgage;
            _financials.Mortgage = null;

            TakeOutMortgage(mortgage.Balance * (1+scenario.OriginationFee), scenario, now);
        }

        public void Run(Scenario scenario, MonthYear start)
        {
            try
            {
                Initialize(scenario, start);
                var now = new MonthYear(start);
                var end = new MonthYear(now).AddYears(scenario.SimulationYears);
                while (now <= end)
                {
                    Simulate(scenario, now);
                    now = now.AddMonths(1);
                }

                CloseBooks(scenario, now);

                Output.VerboseLine(_financials?.ToString());
                Output.WriteLine(ToString());
            }
            catch (Exception exception)
            {
                Output.WriteLine("*** Simulation Failed ***");
                Output.WriteLine(exception.Message);
            }
        }

        private bool ScroungeMoney(decimal amount, MonthYear now)
        {
            if (_financials.Cash < amount)
                SellBonds(Math.Ceiling(amount - _financials.Cash), now);
            if (_financials.Cash < amount)
                SellStocks(Math.Ceiling(amount - _financials.Cash), now);

            return _financials.Cash >= amount;
        }

        private decimal SellBond(Treasury bond, decimal amount, MonthYear now)
        {
            var rate = TreasuryInterestRates.GetRate(now);
            var faceValue = bond.GetFaceValue(now, rate.InterestRate);
            if (faceValue <= amount)
            {
                _financials.CurrentTaxes.TreasuryInterest = faceValue - bond.Purchase;
                _financials.Cash += faceValue;
                _financials.Bonds.Remove(bond);
                return faceValue;
            }

            var percentage = faceValue / amount;
            _financials.CurrentTaxes.TreasuryInterest = (faceValue - bond.Purchase) * percentage;
            _financials.Cash += faceValue * percentage;

            bond.Par *= (1 - percentage);
            bond.Purchase *= (1 - percentage);
            return amount;
        }

        private void SellBonds(decimal amount, MonthYear now)
        {
            while (amount > 0 && _financials.Bonds.Any())
            {
                // No tax calc here. Just take the first.
                var bond = _financials.Bonds.First();
                amount -= SellBond(bond, amount, now);
            }
        }

        private decimal SellStock(Sp500 stock, decimal amount, MonthYear now)
        {
            var price = Sp500Prices.GetPrice(now);
            if (stock.Shares * price.Price <= amount)
            {
                var sale = stock.Shares * price.Price;
                var basis = stock.Shares * stock.PurchasePrice;
                _financials.CurrentTaxes.CapitalGains = amount - basis;
                _financials.Cash += sale;
                _financials.Stocks.Remove(stock);
                return sale;
            }

            var shares = amount / price.Price;
            stock.Shares -= shares;
            _financials.CurrentTaxes.CapitalGains = shares * (price.Price - stock.PurchasePrice);
            _financials.Cash += amount;
            return amount;
        }

        private void SellStocks(decimal amount, MonthYear now)
        {
            while (amount > 0 && _financials.Stocks.Any())
            {
                // Take highest price to reduce our taxes.
                var maxPurchasePrice = _financials.Stocks.Max(c => c.PurchasePrice);
                var stock = _financials.Stocks.First(c => c.PurchasePrice == maxPurchasePrice);
                amount -= SellStock(stock, amount, now);
            }
        }

        private bool ShouldRefinance(Scenario scenario, MonthYear now)
        {
            if (!scenario.AllowRefinance)
                return false;

            if (_financials.Mortgage == null)
                return false;

            var rate = MortgageInterestRates.GetRate(now, scenario.MortgageTerm);
            if (rate.InterestRate >= _financials.Mortgage.InterestRate)
                return false;

            var months = Math.Max(1, scenario.RefinancePayBackMonths);

            var costOfRefinance = _financials.Mortgage.Balance * scenario.OriginationFee;
            var currentPayment = _financials.Mortgage.Payment;

            var calculator = new PaymentCalculator
            {
                Years = _financials.Mortgage.Years,
                InterestRate = rate.InterestRate,
                LoanAmount = _financials.Mortgage.Balance + costOfRefinance,
            };

            var newPayment = calculator.CalculatePayment();
            if (newPayment >= currentPayment)
                return false;

            return (currentPayment - newPayment) * months > costOfRefinance;
        }

        private void Simulate(Scenario scenario, MonthYear now)
        {
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
            if (amount <= 0)
                return;

            // origination fee

            _financials.Mortgage.Amount = _financials.Mortgage.Balance = amount;
            switch (scenario.MortgageTerm)
            {
                case MortgageTerm.FifteenYear:
                    _financials.Mortgage.Years = 15;
                    break;

                case MortgageTerm.ThirtyYear:
                    _financials.Mortgage.Years = 30;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario.MortgageTerm), scenario.MortgageTerm, null);
            }

            var interestRate = scenario.MortgageInterestRate;
            if (!interestRate.HasValue)
                interestRate = MortgageInterestRates.GetRate(start, scenario.MortgageTerm).InterestRate;

            _financials.Mortgage.InterestRate = interestRate.Value;

            var calculator = new PaymentCalculator
            {
                Years = _financials.Mortgage.Years,
                InterestRate = _financials.Mortgage.InterestRate,
                LoanAmount = _financials.Mortgage.Amount,
            };
            _financials.Mortgage.Payment = calculator.CalculatePayment();
        }
    }
}
