﻿using System;
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
            using (var instance = tagui.Instance.Create(nobrowser: true, quiet: true))
            {
                instance.onOutput += Instance_onOutput;
                Console.WriteLine("Search for start menu");
                if (instance.Present("startmenu.png"))
                {
                    Console.WriteLine("Click Start menu");
                    instance.Click("startmenu.png");
                }
                Console.WriteLine("completed");
            }
            // ***** BROWSER TEST
            using (var instance = tagui.Instance.Create(quiet: true))
            {
                instance.onOutput += Instance_onOutput;
                Console.WriteLine("init started");
                instance.WaitIsReady();
                Console.WriteLine("init completed");

                Console.WriteLine("open google.com and search for q");
                if (instance.OpenURL("https://www.google.dk/imghp?hl=en&ogbl").Present("q"))
                {
                    Console.WriteLine("type openrpa and new line");
                    instance.Type("q", "cute kitten[enter]");
                }
                Console.WriteLine("Search for first picture");
                if (instance.Present("//div[@data-ri=\"0\"]"))
                {
                    Console.WriteLine("Snapshot of picture");
                    instance.Wait(1).Snap("//div[@data-ri=\"0\"]", "kitten.png");
                }
            }
            Console.WriteLine("completed");
            Console.ReadLine();
        }

        private static void Instance_onOutput(object sender, tagui.OutputEventArgs e)
        {
            Console.WriteLine(e.Output);
        }
    }
}