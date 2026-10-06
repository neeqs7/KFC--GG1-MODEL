# KFC G/G/1 Model — Current Uploaded Data

This project was rebuilt from the **latest uploaded file: `kfc data.xlsx`**.

The workbook contains one worksheet (`Sheet1`). This model uses that uploaded dataset only.
It extracts the **80 complete customer records** and ignores the spreadsheet's calculation/summary rows at the bottom.

## Results from this dataset

- Customers: 80
- Mean inter-arrival time: 2.2152 min
- Mean service time (Queue 1): 2.0250 min
- Arrival rate lambda: 0.4514 cust/min
- Service rate mu: 0.4938 cust/min
- Utilization rho: 0.9141
- Arrival CV: 1.0265
- Service CV: 0.4005
- Arrival Ca²: 1.0538
- Service Cs²: 0.1604
- Observed mean Wq: 4.5500 min
- Trace-DES mean Wq: 4.5500 min
- Maximum trace error: 0.000000 min
- Kingman Wq: 13.0891 min

## Model definition

The single server is Queue 1:

Arrival -> Service Start -> Order End

Service time = `Order End Time - Service Start Time`.

Queue 2 is retained in the CSV as auxiliary observed information but is not combined with Queue 1 service time.

## Output sections

1. Empirical variability analysis
2. Kingman's G/G/1 approximation
3. Trace-driven discrete-event simulation
4. Monte Carlo with Gamma-fitted general distributions
5. Grand comparison

## Run

Double-click `run.bat`.

It requires the .NET 8 SDK.
