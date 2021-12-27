using System;
using BugReportData;

namespace BugReportUI {
    class UI {
        static void Main(string[] args) {
            using (var c=new BugReportContext()) {
                c.Database.EnsureDeleted();
                c.Database.EnsureCreated();
            }
            Console.WriteLine("Hello World!");
        }
    }
}
