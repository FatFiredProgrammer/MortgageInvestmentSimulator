using System;
using System.Text;

namespace MortgageInvestmentSimulator
{
    public sealed class Simulation
    {
        public Simulation(IOutput output)
            => Output = output ?? throw new ArgumentNullException(nameof(output));

        public IOutput Output { get; }

        private Financials _financials;

        public decimal NetWorth => _financials?.NetWorth ?? 0m;

        private void Initialize(Scenario scenario, MonthYear start, Strategy strategy)
        {
            _financials = new Financials
            {
                HomeValue = scenario.HomeValue,
                Cash = scenario.StartingCash,
            };
            switch (strategy)
            {
                case Strategy.PayOffHouse:
                    InitializePayOffHouse(scenario, start);
                    break;

                case Strategy.Invest:
                    InitializeInvest(scenario, start);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            Output.VerboseLine(_financials?.ToString());
        }

        private void InitializeInvest(Scenario scenario, MonthYear start)
        {
            TakeOutLoan(scenario.HomeValue, scenario.MortgageTerm, scenario.MortgageInterestRate, start);
        }

        private void InitializePayOffHouse(Scenario scenario, MonthYear start)
        {
            if (_financials.Cash >= _financials.HomeValue)
            {
                _financials.Cash -= _financials.HomeValue;
            }
            else
            {
                TakeOutLoan(scenario.HomeValue - _financials.Cash, scenario.MortgageTerm, scenario.MortgageInterestRate, start);
                _financials.Cash = 0;
            }
        }

        public void Run(Scenario scenario, MonthYear start, Strategy strategy)
        {
            try
            {
                Output.WriteLine($"{start} with strategy {strategy}");
                Initialize(scenario, start, strategy);
                var now = new MonthYear(start);
                var end = new MonthYear(now).AddYears(scenario.SimulationYears);
                while (now <= end)
                {
                    Simulate(scenario, now, strategy);
                    now = now.AddMonths(1);
                }

                Output.VerboseLine(_financials?.ToString());
                Output.WriteLine(ToString());
            }
            catch (Exception exception)
            {
                Output.WriteLine("*** Simulation Failed ***");
                Output.WriteLine(exception.Message);
            }
        }

        private void Simulate(Scenario scenario, MonthYear now, Strategy strategy)
        {
            Output.VerboseLine($"Simulating {now}");
            SimulateIncome(scenario, now);

            SimulatePayMortgage(now);
            switch (strategy)
            {
                case Strategy.PayOffHouse:
                    SimulatePayOffHouse(scenario, now);
                    break;

                case Strategy.Invest:
                    SimulateInvest(scenario, now);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }
        }

        private void SimulatePayMortgage(MonthYear now)
        {
            if (_financials.MortgageBalance <= 0)
                return;

            if (_financials.Cash < _financials.MonthlyPayment)
                throw new SimulationFailedException($"Could not make mortgage payment in {now}");

            _financials.Cash -= _financials.MonthlyPayment;

            var interest = _financials.MortgageBalance * _financials.MortgageInterestRate / 12;
            if (interest > _financials.MonthlyPayment)
            {
                var growth = interest - _financials.MonthlyPayment;
                _financials.MortgageBalance += growth;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; balance grew by {growth:C0} to {_financials.MortgageBalance:C0}");
            }
            else
            {
                var principal = _financials.MonthlyPayment - interest;
                _financials.MortgageBalance -= principal;
                Output.VerboseLine($"Mortgage interest of {interest:C0}; principal payment of {principal:C0} with balance of {_financials.MortgageBalance:C0}");
            }
        }

        private void SimulateIncome(Scenario scenario, MonthYear now)
        {
            if (scenario.MonthlyIncome <= 0)
                return;

            Output.VerboseLine($"Monthly income of {scenario.MonthlyIncome:C0} for {now}");
            _financials.Cash += scenario.MonthlyIncome;

        }

        private void SimulateInvest(Scenario scenario, MonthYear now)
        {
        }

        private void SimulatePayOffHouse(Scenario scenario, MonthYear now)
        {
            if (_financials.Cash <= 0)
                return;
            if (_financials.MortgageBalance <= 0)
                return;

            var principal = Math.Min(_financials.MortgageBalance, _financials.Cash);
            _financials.Cash -= principal;
            _financials.MortgageBalance -= principal;
            Output.VerboseLine($"Addition mortgage principal of {principal:C0}; remaining balance of {_financials.MortgageBalance:C0}");
        }

        private void TakeOutLoan(decimal amount, MortgageTerm term, decimal? interestRate, MonthYear start)
        {
            if (amount <= 0)
                return;

            _financials.MortgageAmount = _financials.MortgageBalance = amount;
            switch (term)
            {
                case MortgageTerm.FifteenYear:
                    _financials.MortgageYears = 15;
                    break;

                case MortgageTerm.ThirtyYear:
                    _financials.MortgageYears = 30;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(term), term, null);
            }

            if (!interestRate.HasValue)
                interestRate = MortgageInterestRate.GetRate(start, term)?.InterestRate;
            if (!interestRate.HasValue)
                throw new SimulationException($"No mortgage interest data for {start}");

            _financials.MortgageInterestRate = interestRate.Value;

            var calculator = new PaymentCalculator
            {
                Years = _financials.MortgageYears,
                InterestRate = _financials.MortgageInterestRate,
                LoanAmount = _financials.MortgageAmount,
            };
            _financials.MonthlyPayment = calculator.CalculatePayment();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            return $"Net worth is {NetWorth:C0}";
        }
    }
}
