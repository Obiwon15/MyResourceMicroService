using DBIntializers.Models.New;
using DBIntializers.Models.Old;
using DBIntializers.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DBIntializers
{
    class Program
    {
        static void Main(string[] args)
        {           
            UserService services = new UserService();

            var result = services.AddNewUsers();
            if (result != null)
            {
                Console.WriteLine($"{result.Count} was added to New DB");
            }
            else
            {
                Console.WriteLine("Failed");
            }

            Console.ReadLine();
        }
    }
}
