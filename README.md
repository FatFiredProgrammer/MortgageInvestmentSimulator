# MortgageInvestmentSimulator

**TL/DR** I believe that, in most cases, a _rational investor_ with a sufficiently long term view - 
say _greater than 10 years_ - is better off carrying a home mortgage
and investing the difference in a broad index fund. 

**Details**

Your goal is to convince me there are reasonable scenarios where it is better to pay down or avoid a mortgage.
I will use hard simulation data to support or disprove the hypothesis.
If you have a new scenario, I can run it or I can try to code the simulator to support it - if possible.
Obviously, risk is an important consideration. 
To reduce the impact of risk, I am limiting bonds to 1 year US treasuries
and limiting stocks to the S&amp;P 500.

The simulator compares paying off a mortgage/paying down a mortgage vs investing in the S&amp;P 500.
The C# code for this simulator is on [GitHub](https://github.com/johnweeder/MortgageInvestmentSimulator).
There are no secrets here. 
The data and the simulator itself are public knowledge.

The simulator uses actual **historical market data** for the S&P 500 (price and dividend), 
1 year US treasuries, and the average monthly mortgage interest rate. 
The simulation runs starting each month from April 1972 until September 2013. 
Buying/selling is done purely as needed (spot) with no attempt to guess the market.
A small amount of cash is maintained and the remainder of the money is either invested in
1 year treasuries or in the S&P 500. 
The stock/bond mix is a parameter of the simulation.
The simulation makes attempts to include taxation.
The simulation treats bond buying and selling in a very crude matter and does not include transaction fees or discounts.
Essentially, this means all simulation can invest any funds at the 1 year US treasury rate.
Stock buying and selling does not have transaction fees (yet).

_It is likely there are still bugs in the simulator._ No program is without bugs.
If you find problems, I will try to fix them and report data which is significantly mis-representative.

The simulator has a number of options and you can vary them within reason.
In all cases, the simulation makes 2 passes. 
One pass tries to maximize investment and
one pass tries to avoid or pay off a mortgage as soon as possible.

* You can control whether the simulation tries to avoid or pay off a mortgage as soon as possible.
* You can control how many years the simulation runs.
* You can control the value of the home you are buying. Downpayment, PMI, taxes and insurance are not part of the simulation because they are invariant.
* You can optionally supply a monthly income (during wealth accumulation phase).
* You can optionally supply a starting cash amout (for people who are retiring soon and may want to pay off a mortgage).
* You can control the percentage of bonds to stocks.
* You can have the simulation rebalance your portfolio at some interval of months.
* You can specify a 15 or 30 year mortgage.
* You can specify a mortgage origination fee (to justify refinancing).
* You can allow or disallow mortgage refinancing.
* You can define various tax rates.
* You can allow or disallow mortgage interest deductions.
* You can set threshhold amounts for stock and bond purchases to prevent frequent trading - given there are no transaction fees.
