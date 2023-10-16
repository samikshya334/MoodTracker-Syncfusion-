using Mental_Health_Traker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mental_Health_Traker.Controllers
{
    public class DashboardController : Controller
    {
		private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task <ActionResult> Index()
        {
            //last 7 days 
            DateTime StartDate=DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;
            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();
            //total positive 
            int TotalPositive = (int)SelectedTransactions
                .Where(i => i.Category.Type == "Positive")
                .Sum(j => j.Percentage);
            double TotalPositiveAsPercentage = (double)TotalPositive / 100; // Convert to a value between 0 and 1
            ViewBag.TotalPositive = TotalPositiveAsPercentage.ToString("P");
          

			//total negative
			int TotalNegative = (int)SelectedTransactions
				.Where(i => i.Category.Type == "Negative")
				.Sum(j => j.Percentage);
            double TotalNegativeAsPercentage = (double)TotalNegative / 100; // Convert to a value between 0 and 1
            ViewBag.TotalNegative = TotalNegativeAsPercentage.ToString("P");

            //total Balance
            int Balance = TotalPositive - TotalNegative;
            double BalanceAsPercentage = (double)Balance / 100;
            ViewBag.Balance = BalanceAsPercentage.ToString("P");
            //Doughnut chart
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type=="Positive")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon= k.First().Category.Icon+" "+ k.First().Category.Title,
                    percentage=k.Sum(j=> j.Percentage),
                    formattedPercentage = k.Sum(j=>j.Percentage).ToString("P"),

                })

                .ToList();
            //positive
            List<SplineChartData> PositiveSummary = SelectedTransactions

              .Where(i =>i.Category.Type =="Positive")
              .GroupBy(j => j.Date)
              .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    positive = (int)k.Sum(l => l.Percentage)
                })
                .ToList();

            //negative

            List<SplineChartData>NegativeSummary= SelectedTransactions
                .Where(i => i.Category.Type =="Negative")
                .GroupBy (j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    negative = (int)k.Sum(l => l.Percentage)
                })
                .ToList();

            //combine positive nad negative
            string[]last7Days =Enumerable.Range(0,7)
                .Select(i=>StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();
            ViewBag.SplineChartData=from day in last7Days
                                    join positive in PositiveSummary on day equals positive.day into dayPositiveJoined
                                    from positive in dayPositiveJoined.DefaultIfEmpty()
                                    join negative in NegativeSummary on day equals negative.day into dayNegativeJoined
                                    from negative in dayNegativeJoined.DefaultIfEmpty()
                                    select new
                                    {
                                        day = day,
                                        positive = positive==null ? 0 : positive.positive,
                                        negative = negative==null ? 0 : negative.negative,
                                    };
                                    
            //recent transaction
            ViewBag.RecentTransactions=await _context.Transactions
                .Include(i =>i.Category)
                .OrderByDescending(i => i.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }

   
     
    }
    public class SplineChartData
    {
        public string day;
        public int positive;
        public int negative;
       
    }
}
