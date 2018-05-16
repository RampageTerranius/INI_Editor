using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INI_Editor
{
    class Program
    {
        static void Main(string[] args)
        {
            INI i = new INI();
            i.AddTree("blah blah");
            i.AddValue("blah blah", "name", "tyler");
            i.AddValue("blah blah", "name2", "tylero");
            i.AddValue("blah blah", "name3", "tyleroOOOOs");
            i.AddTree("ect");
            i.EditValue("blah blah", "name3", "mcname", "mr browno");
            i.EditTree("ect", "ect ect");
            i.SaveTo("C:\\Users\\tylerbrown\\source\\repos\\INI Editor\\INI Editor\\", "yep.ini");        
            Console.WriteLine(i.ToString());

            INI n = new INI();

            n.Load("C:\\Users\\tylerbrown\\AppData\\Roaming\\Autodesk\\Revit\\Autodesk Revit Architecture 2014\\", "revit.ini");
            n.EditValue
            Console.WriteLine("\n");
            Console.WriteLine(n.ToString());

            Console.ReadKey();
        }
    }
}
