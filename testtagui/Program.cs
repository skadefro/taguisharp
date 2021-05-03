using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testtagui
{
    class Program
    {
        static void Main(string[] args)
        {
            // ***** TEST WITHOUT BROWSER
            //using (var instance = tagui.Instance.Create(nobrowser: true, quiet: true))
            //{
            //    instance.EchoSendCommand = true;
            //    instance.onOutput += Instance_onOutput;
            //    if (instance.Mouse(out int x, out int y) != null)
            //    {
            //        Console.WriteLine(string.Format("{0},{1}", x, y));
            //    }
            //    if (instance.Present("startmenu.png"))
            //    {
            //        Console.WriteLine("Click Start menu");
            //        instance.Click("startmenu.png");
            //    }
            //    if (instance.Mouse(out int newx, out int newy) != null)
            //    {
            //        Console.WriteLine(string.Format("{0},{1}", newx, newy));
            //    }
            //    instance.Click(x, y);
            //    Console.WriteLine("completed");
            //}
            // ***** BROWSER TEST
            //using (var instance = tagui.Instance.Create(quiet: true))
            //{
            //    instance.EchoSendCommand = true;
            //    instance.onOutput += Instance_onOutput;
            //    Console.WriteLine("open google.com and search for q");
            //    if (instance.Web("https://www.google.com/imghp?hl=en&ogbl").Present("q"))
            //    {
            //        Console.WriteLine("type openrpa and new line");
            //        Console.WriteLine(instance.Type("q", "OpenIAP rocks").Wait(1).Read("//*[@name=\"q\"]"));
            //        instance.Type("q", "[clear]cute kitten[enter]");
            //    }
            //    //Console.WriteLine("Search for first picture");
            //    //if (instance.Present("//div[@data-ri=\"0\"]"))
            //    //{
            //    //    Console.WriteLine("Snapshot of picture");
            //    //    instance.Wait(1).Snap("//div[@data-ri=\"0\"]", "kitten.png");
            //    //}
            //}
            // ***** TABLE TEST 
            //using (var instance = tagui.Instance.Create(quiet: true))
            //{
            //    instance.EchoSendCommand = true;
            //    instance.onOutput += Instance_onOutput;
            //    Console.WriteLine(instance.Web("https://faculty.etsu.edu/tarnoff/ntes1710/tables/tables.htm").WaitFor("//table").Table("//table"));
            //}

            using (var instance = tagui.Instance.Create(nobrowser: true, quiet: true))
            {
                instance.EchoSendCommand = true;
                instance.onOutput += Instance_onOutput;

                if (instance.Present("startmenu.png"))
                {
                    Console.WriteLine("Click Start menu");
                    instance.Click("startmenu.png").Keyboard("calculator[enter]");

                }


                Console.WriteLine(instance.API("https://api.chucknorris.io/jokes/random"));
            }



            //Console.WriteLine("completed");
            //Console.WriteLine("Free console type directly to live");
            //using (var instance = tagui.Instance.Create())
            //{
            //    instance.onOutput += Instance_onOutput;
            //    instance.Web("https://www.google.com");
            //    string s = "dummy";
            //    while (s != "q" && s != "done")
            //    {
            //        s = Console.ReadLine();
            //        instance.Send(s);
            //    }
            //}


        }
        private static void Instance_onOutput(object sender, tagui.OutputEventArgs e)
        {
            Console.WriteLine(e.Output);
        }
    }
}
