using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{

    public class ProductLotSize
    {
        public string PartName { set; get; }

        public int LotSize { set; get; }
    }

    public class ProductLotSizeRecommendation
    {
        public ProductLotSizeRecommendation()
        {
            LotSizeReport = new List<ProductLotSizeReport>();
        }
        public int TryLotSize { set; get; }

        public int FinalRecommendation { set; get; }

        public List<ProductLotSizeReport> LotSizeReport { set; get; }
    }



    public class ProductLotSizeReport
    {
        public int Iteration { get; set; }

        public string PartName { set; get; }

        public int LotSize { set; get; }

        public int Tardiness { get; set; }
    }
}
