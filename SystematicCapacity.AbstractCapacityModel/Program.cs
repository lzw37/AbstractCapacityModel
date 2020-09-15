using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystematicCapacity.AbstractCapacityModel
{
    class Program
    {
        static void Main(string[] args)
        {
            DataRepository.ReadData("../../Data");

            CapacityEstimationModel model = new CapacityEstimationModel();
            model.Preprocess();
            model.Solve();


            Console.WriteLine("Output solution...");
            DataRepository.OutputSolution("../../Solution");

            Console.WriteLine("Program terminated!");
            Console.ReadKey();
        }       
    }
}
