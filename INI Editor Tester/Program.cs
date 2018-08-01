using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//created by rampage_terranius
//tester program used for testing all the functions of INI_Editor.dll
//upto date as of VER: x

namespace INI_Editor_Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Test())
				Console.WriteLine("All tests were successful!");
			else
				Console.WriteLine("WARNING: Last test was unsuccessful!");

			Console.ReadKey();
		}

		static bool Test()
		{
			INI_Editor.INI a;

			Console.WriteLine("Testing new INI_Editor.INI()...");
			a = new INI_Editor.INI();
			if (a == null)
				return false;

			Console.WriteLine("Testing load(string)...");
			a.Load("test.ini");
			if (!a.FileLoaded || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing Clear()/Close()");
			a.Close();
			if (a.FileLoaded || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing load(string, string)...");
			a.Load(Directory.GetCurrentDirectory(), "test.ini");
			if (!a.FileLoaded || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing new INI_Editor.INI(string)...");
			a = new INI_Editor.INI("test.ini");
			if (a == null)
				return false;
			if (!a.FileLoaded || a.GetLastError() != "")
				return false;

			a.Close();

			Console.WriteLine("Testing new INI_Editor.INI(string, string)...");
			a = new INI_Editor.INI(Directory.GetCurrentDirectory(), "test.ini");
			if (a == null)
				return false;
			if (!a.FileLoaded || a.GetLastError() != "")
				return false;

			a.Close();

			Console.WriteLine("Testing Save()...");
			a.Load("test.ini");
			a.Save();
			if (a.GetLastError() != "")
				return false;

			a.Close();

			Console.WriteLine("Testing SaveTo(string)...");
			a.Load("test.ini");
			a.SaveTo("test.ini");
			if (a.GetLastError() != "")
				return false;

			a.Close();

			Console.WriteLine("Testing SaveTo(string, string)...");
			a.Load("test.ini");
			a.SaveTo(Directory.GetCurrentDirectory(), "test.ini");
			if (a.GetLastError() != "")
				return false;

			a.Close();

			Console.WriteLine("Testing SaveAndClose()...");
			a.Load("test.ini");
			a.SaveAndClose();
			if (a.GetLastError() != "" || a.FileLoaded)
				return false;

			Console.WriteLine("Testing TreeExists(string)...");
			a.Load("test.ini");
			if (!a.TreeExists("Test1") || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing ValueExists(string, string)...");
			if (!a.ValueExists("Test1", "test1data1") || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing GetValue(string, string)...");
			if (a.GetValue("Test1", "test1data1") != "a" || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing GetTree(string, string)...");
			INI_Editor.Tree t = new INI_Editor.Tree();
			t = a.GetTree("Test1");
			if (t.treeName != "Test1" || t.tree == null || a.GetLastError() != "")
				return false;


			/*Console.WriteLine("Testing GetAll(string, string)...");
			List<INI_Editor.Tree> tr = new List<INI_Editor.Tree>();
			if (a.GetAll())
				return false;*/

			Console.WriteLine("Testing AddValue(string, string, string)...");
			a.AddValue("Test2", "newdata", "new");
			if (a.GetValue("Test2", "newdata") != "new" || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing AddTree(string)...");
			a.AddTree("NewTest");
			if (!a.TreeExists("NewTest") || a.GetLastError() != "")
				return false;

			Console.WriteLine("Testing AddTree(Tree)...");
			INI_Editor.Tree newTree = new INI_Editor.Tree();
			newTree.treeName = "ManualTree";

			a.AddTree(newTree);

			a.Close();

			return true;
		}

	}
}
